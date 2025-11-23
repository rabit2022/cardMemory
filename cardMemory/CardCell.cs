using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


public class CardCell : UserControl
{
    private Color backColorSelected = Color.White;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color BackColorSelected
    {
        get => backColorSelected;
        set
        {
            backColorSelected = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Points { get; set; } = "";

    private TextBox _editBox = null;
    private Button _delBtn;
    private Button _copyBtn; // 改为字段
    private Button _pasteBtn; // 改为字段

    private readonly StringFormat _sf = new StringFormat
    {
        Alignment = StringAlignment.Center, // 水平居中
        LineAlignment = StringAlignment.Center, // 垂直居中
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoClip // 允许换行
    };

// 放在 CardCell 类内部，所有实例共享
    private static string _clipPoints = "";
    private static Color _clipColor = Color.LightSkyBlue;
    private static ToolTip _toolTip; // 静态：所有 CardCell 共用
    private Button _presetBtn;
    public CardCell()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        // Size = new Size(80, 80);

        BackColorSelected = Palette[0];
        BorderStyle = BorderStyle.FixedSingle;
        MouseDown += CardCell_Click;

        if (_toolTip == null)
        {
            _toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 200,
                ReshowDelay = 200
            };
        }

        // === 删除按钮 ===
        _delBtn = new Button
        {
            Text = "×",
            Size = new Size(18, 18),
            Font = new Font("Segoe UI", 9, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.DarkRed,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _delBtn.FlatAppearance.BorderSize = 0;
        _delBtn.Click += (s, e) => Reset(); // 重置

        // === 复制按钮 ===
        _copyBtn = new Button
        {
            Text = "⎘",
            Size = new Size(18, 18),
            Font = new Font("Segoe UI", 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _copyBtn.FlatAppearance.BorderSize = 0;
        _copyBtn.Click += (_, _) =>
        {
            _clipPoints = Points;
            _clipColor = BackColorSelected;
        };

// === 粘贴按钮 ===
        _pasteBtn = new Button
        {
            Text = "⎙",
            Size = new Size(18, 18),
            Font = new Font("Segoe UI", 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _pasteBtn.FlatAppearance.BorderSize = 0;
        _pasteBtn.Click += (_, _) =>
        {
            Points = _clipPoints;
            BackColorSelected = _clipColor;
        };

        _toolTip.SetToolTip(_copyBtn, "复制文本和颜色");
        _toolTip.SetToolTip(_pasteBtn, "粘贴文本和颜色");
        _toolTip.SetToolTip(_delBtn, "重置当前卡片");

        /* 一次性加到控件集合 */
        Controls.Add(_pasteBtn); // 最底层
        Controls.Add(_copyBtn); // 中间层
        Controls.Add(_delBtn); // 最上层（Z-Order 最高）
        
        
        _presetBtn = new Button
        {
            Text = "≡",
            Size = new Size(18, 18),
            Font = new Font("Segoe UI", 8),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            // Anchor = AnchorStyles.Top | AnchorStyles.Right
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right   // ← 贴底
        };
        _presetBtn.FlatAppearance.BorderSize = 0;
        _presetBtn.Click += (_, __) =>
        {
            var ans = PresetDialog.PickPreset(this.FindForm());
            if (ans.HasValue)
            {
                Points = ans.Value.Text;
                BackColorSelected = ans.Value.Color;
                Invalidate();
            }
            
        };
        _toolTip.SetToolTip(_presetBtn, "快速预设");
/* 加到 Controls，顺序决定 Z-Order */
        Controls.Add(_presetBtn);   // 最底层
    }

    public void Reset()
    {
        BackColorSelected = Palette[0]; // 颜色回到调色板首色
        Points = ""; // 点数清空
        if (_editBox != null) // 如果正在编辑，一起关掉
            EndInlineEdit();
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (_delBtn == null || _copyBtn == null || _pasteBtn == null) return;

        const int btnSize = 18, gap = 2;
        int x = Width - btnSize - gap; // 从右边缘开始

        _delBtn.Location = new Point(x, gap);
        x -= btnSize + gap; // 最右
        _pasteBtn.Location = new Point(x, gap);
        x -= btnSize + gap; // 中间
        _copyBtn.Location = new Point(x, gap); // 最左
        
        
        /* ---------- 右下角预设按钮 ---------- */
        _presetBtn.Location = new Point(Width - btnSize - gap,   // 贴右
            Height - btnSize - gap); // 贴底
    }

// 1. 把调色板做成只读配置，甚至可以放到配置文件
    private static readonly Color[] Palette =
    {
        Color.LightSkyBlue, Color.Red, Color.Violet, Color.Yellow, Color.Green
    };


    private void CardCell_Click(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) // 左键就地编辑点数
        {
            // 如果文本框已经存在，第二次左键 = 提交
            if (_editBox != null)
            {
                Parent.Focus(); // 让文本框失焦，触发 Leave → EndInlineEdit
                return;
            }

            // 否则第一次左键，打开编辑
            StartInlineEdit();
        }
        else if (e.Button == MouseButtons.Right)
        {
            // // 如果正在编辑，先结束，避免弹窗被文本框压住
            // if (_editBox != null) Parent.Focus();
            //
            // var owner = this.FindForm();      // 1. 确定父窗体
            // var picked = MiniPaletteDialog.PickColor(owner, Palette);
            // if (picked.HasValue)
            // {
            //     BackColorSelected = picked.Value;
            //     Invalidate();
            // }
            // 直接轮换颜色
            int index = Array.IndexOf(Palette, BackColorSelected);
            index = (index + 1) % Palette.Length;
            BackColorSelected = Palette[index];
            Invalidate();
        }
    }

    private void StartInlineEdit()
    {
        if (_editBox != null) return;

        // 偏上，否则 右键选颜色 会被编辑框遮挡
        int top = (Height - 20) / 3; // 垂直居中
        _editBox = new TextBox
        {
            Text = Points,
            Location = new Point(5, top),
            Size = new Size(Width - 10, 20),
            Font = Font,
            BorderStyle = BorderStyle.FixedSingle
        };

        _editBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                Parent.Focus(); // 失焦即提交
        };
        _editBox.Leave += (_, _) => EndInlineEdit();

        Controls.Add(_editBox);
        _editBox.Focus();
        _editBox.SelectAll();
    }

    private void EndInlineEdit()
    {
        if (_editBox == null) return;
        Points = _editBox.Text;
        Controls.Remove(_editBox);
        _editBox.Dispose();
        _editBox = null;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(BackColorSelected);
        // 文字区域 = 卡片客户区，上下左右各留 3 像素
        var textRect = new RectangleF(3, 3, Width - 6, Height - 6);

        using (var brush = new SolidBrush(Color.Black))
        {
            e.Graphics.DrawString(Points, Font, brush, textRect, _sf);
        }
    }
}