using System;
using System.Drawing;
using System.Windows.Forms;

/// <summary>
/// 弹出一个 4×N 的迷你调色板，让用户点选颜色。
/// 选完后通过 <see cref="ColorSelected"/> 事件把结果抛出去，
/// 并自动关闭窗体。
/// </summary>
public sealed class MiniPaletteDialog : Form
{
    /// <summary>用户最终选中的颜色（如取消则为 null）。</summary>
    public Color? SelectedColor { get; private set; }

    public static Color? PickColor(IWin32Window owner, params Color[] palette)
    {
        if (palette == null || palette.Length == 0) return null;

        var ownerForm = owner as Form;
        using var dlg = new MiniPaletteDialog(palette)
        {
            Owner = ownerForm,
            // 关键：主窗是TopMost，子窗就必须也是TopMost，否则被压到下面
            TopMost = ownerForm?.TopMost ?? false
        };
        dlg.ShowDialog(owner);
        return dlg.SelectedColor;
    }

    /* -------------------------------------------------- */
    /* 下面是内部实现，调用方完全不用关心                 */
    /* -------------------------------------------------- */
    private readonly Color[] _palette;
    private const int CellSize = 40;
    private const int Gap = 4;

    private MiniPaletteDialog(Color[] palette)
    {
        _palette = palette;
        int colorCount = palette.Length;
        int colCount = Math.Min(colorCount, 4);
        int rowCount = (int)Math.Ceiling(colorCount / (double)colCount);

        Text = "选颜色";
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = colCount,
            RowCount = rowCount,
            Padding = new Padding(Gap)
        };

        // ① 列宽=行高=40，绝对正方形
        for (int c = 0; c < colCount; c++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, CellSize));
        for (int r = 0; r < rowCount; r++)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, CellSize));

        for (int i = 0; i < colorCount; i++)
        {
            var btn = new Button
            {
                BackColor = palette[i],
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(2),
                Tag = palette[i],
                Dock = DockStyle.Fill
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.Click += OnColorClick;
            tbl.Controls.Add(btn, i % colCount, i / colCount);
        }

        Controls.Add(tbl);

        // ② 客户区=色块+双边距，系统自己加边框
        this.ClientSize = new Size(colCount * CellSize + Gap * 2,
            rowCount * CellSize + Gap * 2);
    }
    private void OnColorClick(object sender, EventArgs e)
    {
        SelectedColor = (Color)((Button)sender).Tag;
        DialogResult = DialogResult.OK;   // 标记成功
        Close();
    }
}