using System;
using System.Drawing;
using System.Windows.Forms;

public class MainForm : Form
{
    private TableLayoutPanel grid;
    private ToolStrip toolStrip;

    /* 当前卡片区域大小（不含按钮行列） */
    private int cardRows = 4;
    private int cardCols = 4;

    public MainForm()
    {
        Text = "双人成行 记牌器";
        Size = new Size(400, 450);
        StartPosition = FormStartPosition.CenterScreen;
        TopMost = true;


        /* ========== 顶部工具栏 ========== */
        toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System,
            AutoSize = false,
            Height = 30,
            Dock = DockStyle.Top
        };
        var resetAllBtn = new ToolStripButton("重置全部")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Alignment = ToolStripItemAlignment.Right
        };
        resetAllBtn.Click += (_, _) => ResetAllCards();

        var addRowBtn = new ToolStripButton("+1行")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Alignment = ToolStripItemAlignment.Left
        };
        addRowBtn.Click += (_, _) => AddOneRow();

        var addColBtn = new ToolStripButton("+1列")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Alignment = ToolStripItemAlignment.Left
        };
        addColBtn.Click += (_, _) => AddOneCol();

// 按顺序先加增行列，再加重置
        toolStrip.Items.Add(addRowBtn);
        toolStrip.Items.Add(addColBtn);
        toolStrip.Items.Add(resetAllBtn);

        /* ========== 网格（预留按钮行/列） ========== */
        BuildGrid();


        Controls.Add(grid); // 网格后加，填满剩余
        Controls.Add(toolStrip); // 工具栏先加
    }

    /* 初次建表，4×4 卡片 + 1 按钮行 + 1 按钮列 */
    private void BuildGrid()
    {
        grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = cardRows + 1,
            ColumnCount = cardCols + 1
        };
        grid.MinimumSize = new Size(320, 320); // 4×80


        // 卡片区 25% 高度/宽度，按钮区 35 像素固定
        for (int r = 0; r < cardRows; r++)
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F)); // 底部按钮行

        for (int c = 0; c < cardCols; c++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35F)); // 右侧按钮列

        FillGrid();
    }

    /* 填充卡片与行列按钮 */
    private void FillGrid()
    {
        grid.Controls.Clear();


        /* 4×4 卡片 */
        for (int r = 0; r < cardRows; r++)
        for (int c = 0; c < cardCols; c++)
        {
            var cell = new CardCell
            {
                Dock = DockStyle.Fill, // ← 关键
                Margin = new Padding(2) // ← 上下左右都留 4 像素
            };
            grid.Controls.Add(cell, c, r);
        }

        /* 每行最右 - 重置行 */
        for (int r = 0; r < cardRows; r++)
        {
            int row = r;
            var btn = new Button
            {
                Text = "▣",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            btn.Click += (_, _) => ResetRow(row);
            grid.Controls.Add(btn, cardCols, r);
        }

        /* 每列最下 - 重置列 */
        for (int c = 0; c < cardCols; c++)
        {
            int col = c;
            var btn = new Button
            {
                Text = "▣",
                Font = new Font("Segoe UI", 9),
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
            btn.Click += (_, _) => ResetCol(col);
            grid.Controls.Add(btn, c, cardRows);
        }
    }

    /* ========== 重置逻辑 ========== */
    private void ResetAllCards()
    {
        // 优化：没有添加行列，直接重置
        if (cardRows == 4 && cardCols == 4)
        {
            for (int r = 0; r < cardRows; r++)
            for (int c = 0; c < cardCols; c++)
            {
                if (grid.GetControlFromPosition(c, r) is CardCell cell)
                    cell.Reset();
            }
            return;
        }
        
        
        // 1. 彻底拆掉旧表
        Controls.Remove(grid);
        Controls.Remove(toolStrip);
        grid?.Dispose();

        // 2. 回到初始尺寸
        cardRows = 4;
        cardCols = 4;

        // 3. 重新建 4×4 表格
        BuildGrid();


        Controls.Add(grid); // 重新挂到窗体
        Controls.Add(toolStrip); // 工具栏先加
    }

    private void ResetRow(int row)
    {
        for (int c = 0; c < cardCols; c++)
            if (grid.GetControlFromPosition(c, row) is CardCell card)
                card.Reset();
    }

    private void ResetCol(int col)
    {
        for (int r = 0; r < cardRows; r++)
            if (grid.GetControlFromPosition(col, r) is CardCell card)
                card.Reset();
    }

    /* ========== 动态增行/增列 ========== */
    private void AddOneRow()
    {
        cardRows++;
        grid.RowCount = cardRows + 1;
        grid.RowStyles.Insert(cardRows - 1, new RowStyle(SizeType.Percent, 25F));
        FillGrid(); // 重新摆一遍
    }

    private void AddOneCol()
    {
        cardCols++;
        grid.ColumnCount = cardCols + 1;
        grid.ColumnStyles.Insert(cardCols - 1, new ColumnStyle(SizeType.Percent, 25F));
        FillGrid(); // 重新摆一遍
    }
}