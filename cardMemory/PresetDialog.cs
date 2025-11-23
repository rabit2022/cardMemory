using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public sealed class PresetDialog : Form
{
    public (string Text, Color Color)? Selected { get; private set; }

    public static (string Text, Color Color)? PickPreset(IWin32Window owner)
    {
        using var dlg = new PresetDialog
        {
            Owner = owner as Form,
            TopMost = owner is Form f && f.TopMost
        };
        dlg.ShowDialog(owner);
        return dlg.Selected;
    }

    /* ========== 数据区：交错数组，一行一个 PresetItem[] ========== */
    private static readonly PresetItem[][] Presets =
    {
        // // 行 1
        // new[]
        // {
        //     new PresetItem { Text = "75", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "150", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "225", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "300", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "375", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "450", Color = Color.LightSkyBlue },
        //     new PresetItem { Text = "525", Color = Color.LightSkyBlue }
        // },
        // 行 0
        new[]
        {
            new PresetItem { Text = "10", Color = Color.LightSkyBlue },
            new PresetItem { Text = "20", Color = Color.LightSkyBlue },
            new PresetItem { Text = "25", Color = Color.LightSkyBlue },
            new PresetItem { Text = "30", Color = Color.LightSkyBlue },
            new PresetItem { Text = "50", Color = Color.LightSkyBlue },
            new PresetItem { Text = "100", Color = Color.LightSkyBlue }
        },
        
        // 行 2
        new[]
        {
            new PresetItem { Text = "10", Color = Color.Red },
            new PresetItem { Text = "20", Color = Color.Red },
            new PresetItem { Text = "25", Color = Color.Red },
            new PresetItem { Text = "30", Color = Color.Red },
            new PresetItem { Text = "50", Color = Color.Red },
            new PresetItem { Text = "100", Color = Color.Red }
        },
        // 行 4
        new[]
        {
            // new PresetItem{ Text = "x1.5",    Color = Color.Red },
            new PresetItem { Text = "x2", Color = Color.Red },
            // new PresetItem{ Text = "xRANDOM", Color = Color.Red }
        },
        // 行 5
        new[]
        {
            new PresetItem { Text = "x1.5", Color = Color.Violet },
            new PresetItem { Text = "x2", Color = Color.Violet },
            new PresetItem { Text = "xRANDOM", Color = Color.Violet }
        },
        // Color.Yellow
        new[]
        {
            new PresetItem { Text = "4", Color = Color.Yellow },
            new PresetItem { Text = "5", Color = Color.Yellow },
            new PresetItem { Text = "6", Color = Color.Yellow },
            new PresetItem { Text = "150", Color = Color.Yellow },
        }
    };

    /* 单个预设项 */
    private class PresetItem
    {
        public Color Color = Color.White;
        public string Text = "";
    }

    /* ========== UI 构造 ========== */
    private PresetDialog()
    {
        const int cell = 40, gap = 4;

        Text = "快速预设";
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;

        int rowCount = Presets.Length;
        int colCount = Presets.Max(r => r.Length); // 最长一行=列数

        var tbl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = colCount,
            RowCount = rowCount,
            Padding = new Padding(gap)
        };

        for (int c = 0; c < colCount; c++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, cell));
        for (int r = 0; r < rowCount; r++)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, cell));

        for (int r = 0; r < rowCount; r++)
        {
            var row = Presets[r];
            for (int c = 0; c < row.Length; c++)
            {
                var item = row[c];
                var btn = new Button
                {
                    Text = item.Text,
                    BackColor = item.Color,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(1),
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI",
                        item.Text.Length > 4 ? 7 :
                        item.Text.Length > 3 ? 8 : 9),
                    TextAlign = ContentAlignment.MiddleCenter,
                    AutoSize = false,
                    Tag = (item.Text, item.Color)
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, _) =>
                {
                    Selected = ((string, Color))((Button)s).Tag;
                    DialogResult = DialogResult.OK;
                    Close();
                };
                tbl.Controls.Add(btn, c, r);
            }
        }

        Controls.Add(tbl);
        ClientSize = new Size(colCount * cell + gap * 2,
            rowCount * cell + gap * 2);
    }
}