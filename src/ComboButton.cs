using System;
using System.Drawing;
using System.Windows.Forms;

namespace FileDedup
{
    // 主题化的下拉选择按钮：系统 ComboBox 在深色主题下底色始终是白色，
    // 这里用「按钮 + 右键菜单」实现，颜色完全跟随主题。
    public class ComboButton : Button
    {
        private string[] _items = new string[0];
        private int _index;
        private bool _silent;

        public ComboButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 1;
            this.FlatAppearance.BorderColor = Theme.Line;
            this.BackColor = Theme.Neutral;
            this.ForeColor = Theme.TextC;
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.Font = new Font(Settings.FontName, 9F);
            this.Cursor = Cursors.Hand;
            this.UseVisualStyleBackColor = false;
            this.AutoSize = false;
            this.Padding = new Padding(10, 0, 6, 0);
        }

        public void SetItems(params string[] items)
        {
            _items = items;
            _index = 0;
            Refresh0();
        }

        public int Index
        {
            get { return _index; }
            set
            {
                if (value < 0 || value >= _items.Length) return;
                _index = value;
                Refresh0();
            }
        }

        public int Count
        {
            get { return _items.Length; }
        }

        // 语言切换后重新取译文
        public void Translate()
        {
            Refresh0();
        }

        private string _display = "";

        private void Refresh0()
        {
            string cur = (_index >= 0 && _index < _items.Length) ? _items[_index] : "";
            _display = Lang.T(cur);
            this.Text = "";      // 文本由 OnPaint 自绘，避免基类再画一遍
            Invalidate();
        }

        // 文字左对齐铺满整个宽度（超出用省略号），倒三角固定贴在右边缘
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int padL = 12;
            int arrowArea = 28;                       // 右侧给三角留的固定宽度
            int w = ClientRectangle.Width;
            int h = ClientRectangle.Height;

            Color fg = this.Enabled ? this.ForeColor : Theme.SubText;
            Rectangle textRect = new Rectangle(padL, 0, Math.Max(0, w - padL - arrowArea), h);
            TextRenderer.DrawText(e.Graphics, _display, this.Font, textRect, fg,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine);

            float scale = e.Graphics.DpiX / 96f;
            int triW = (int)Math.Round(10 * scale);
            int triH = (int)Math.Round(6 * scale);
            int cx = w - (int)Math.Round(16 * scale);
            int cy = h / 2;
            Point[] pts = new Point[]
            {
                new Point(cx - triW / 2, cy - triH / 2),
                new Point(cx + triW / 2, cy - triH / 2),
                new Point(cx, cy + triH / 2)
            };
            System.Drawing.Drawing2D.SmoothingMode old = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (SolidBrush br = new SolidBrush(fg)) e.Graphics.FillPolygon(br, pts);
            e.Graphics.SmoothingMode = old;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            ShowList();
        }

        private void ShowList()
        {
            ContextMenuStrip cs = new ContextMenuStrip();
            cs.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            cs.BackColor = Theme.MenuBg;
            cs.ForeColor = Theme.TextC;
            cs.Font = new Font(Settings.FontName, 9.5F);
            cs.ShowImageMargin = false;
            cs.ShowCheckMargin = false;
            for (int i = 0; i < _items.Length; i++)
            {
                int idx = i;
                ToolStripMenuItem it = new ToolStripMenuItem(Lang.T(_items[i]));
                it.Checked = (i == _index);
                it.ForeColor = Theme.TextC;
                it.BackColor = Theme.MenuBg;
                it.Click += delegate
                {
                    bool changed = (_index != idx);
                    _index = idx;
                    Refresh0();
                    if (!_silent)
                    {
                        if (changed && _changed != null) _changed(this, EventArgs.Empty);
                        // 同一个选项再点一次也要通知（例如“全不选”后重挑保留规则，需要重新勾选）
                        if (_picked != null) _picked(this, EventArgs.Empty);
                    }
                };
                cs.Items.Add(it);
            }
            cs.Show(this, new Point(0, this.Height));
        }

        private EventHandler _changed;

        public event EventHandler IndexChanged
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        private EventHandler _picked;

        // 任何时候点中一个选项都会触发（哪怕选的就是当前项）
        public event EventHandler Picked
        {
            add { _picked += value; }
            remove { _picked -= value; }
        }

        // 静默设置（不触发事件）
        public void SetIndexSilent(int value)
        {
            _silent = true;
            Index = value;
            _silent = false;
        }
    }
}


