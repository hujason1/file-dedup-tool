using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FileDedup
{
    public class ProgressPanel : Panel
    {
        private int _value;
        private bool _marquee;
        private int _offset;
        private readonly Timer _timer;

        private static Color TrackColor { get { return Theme.Line; } }
        private static Color FillColor { get { return Theme.Accent; } }

        public ProgressPanel()
        {
            this.DoubleBuffered = true;
            this.Height = 14;
            _timer = new Timer();
            _timer.Interval = 30;
            _timer.Tick += delegate
            {
                _offset += 10;
                if (_offset > (this.Width + 400)) _offset = 0;
                this.Invalidate();
            };
        }

        public void SetValue(int v)
        {
            if (v < 0) v = 0;
            if (v > 100) v = 100;
            _value = v;
            _marquee = false;
            if (_timer.Enabled) _timer.Stop();
            this.Invalidate();
        }

        public void SetMarquee()
        {
            _marquee = true;
            if (!_timer.Enabled) _timer.Start();
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.Width < 3 || this.Height < 3) return;   // 布局初期尺寸为 0 时直接跳过，避免无效路径
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int radius = this.Height / 2;
            if (radius < 2) radius = 2;
            using (GraphicsPath path = Round(r, radius))
            {
                using (SolidBrush bg = new SolidBrush(TrackColor))
                {
                    g.FillPath(bg, path);
                }
                g.SetClip(path);
                if (_marquee)
                {
                    int w = Math.Max(70, this.Width / 4);
                    int x = (_offset % (this.Width + w)) - w;
                    Color acc = FillColor;
                    using (LinearGradientBrush lg = new LinearGradientBrush(
                        new Rectangle(x, 0, w, this.Height), Color.FromArgb(150, acc.R, acc.G, acc.B), acc, 0f))
                    {
                        g.FillRectangle(lg, new Rectangle(x, 0, w, this.Height));
                    }
                }
                else if (_value > 0)
                {
                    int w = (int)Math.Round((this.Width - 1) * _value / 100.0);
                    using (SolidBrush fg = new SolidBrush(FillColor))
                    {
                        g.FillRectangle(fg, 0, 0, w, this.Height - 1);
                    }
                }
                g.ResetClip();
            }
        }

        public static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int w = r.Width, h = r.Height;
            if (w < 0) w = 0;
            if (h < 0) h = 0;
            int d = radius * 2;
            if (d > w) d = w;
            if (d > h) d = h;
            if (w <= 1 || h <= 1 || d < 2)
            {
                if (w > 0 && h > 0) p.AddRectangle(new Rectangle(r.X, r.Y, w, h));
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.X + w - d, r.Y, d, d, 270, 90);
            p.AddArc(r.X + w - d, r.Y + h - d, d, d, 0, 90);
            p.AddArc(r.X, r.Y + h - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }

    // 自绘复选框：白色（青灰）主题下按参考图做成“深青方块 + 白勾”，未勾选是白底细边
    public class FlatCheck : CheckBox
    {
        public FlatCheck()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            CheckedChanged += delegate { Invalidate(); };
            EnabledChanged += delegate { Invalidate(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.Width < 2 || this.Height < 2) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color under = Theme.Card;
            if (Parent != null) under = Parent.BackColor;
            if (BackColor.A == 255) under = BackColor;
            using (SolidBrush bg = new SolidBrush(under)) g.FillRectangle(bg, -1, -1, this.Width + 2, this.Height + 2);

            int box = (int)Math.Round(Font.Height * 0.95);
            if (box < 14) box = 14;
            if (box > 24) box = 24;
            int by = (this.Height - box) / 2;
            Rectangle r = new Rectangle(0, by, box, box);

            Color fill = Enabled ? (Checked ? Theme.Accent : Theme.Card) : Theme.Neutral;
            Color border = Enabled ? (Checked ? Theme.Accent : Theme.FieldLine) : Theme.Line;
            using (GraphicsPath path = ProgressPanel.Round(r, 3))
            {
                using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
                using (Pen pen = new Pen(border, 1)) g.DrawPath(pen, path);
            }
            if (Checked)
            {
                using (Pen pen = new Pen(Theme.OnAccent, Math.Max(1.5f, box / 11f)))
                {
                    g.DrawLine(pen, box * 0.27f, by + box * 0.52f, box * 0.45f, by + box * 0.71f);
                    g.DrawLine(pen, box * 0.45f, by + box * 0.71f, box * 0.75f, by + box * 0.31f);
                }
            }

            int tx = box + 7;
            if (this.Width > tx + 2)
            {
                Rectangle tr = new Rectangle(tx, 0, this.Width - tx, this.Height);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                    | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(g, Text, Font, tr, Enabled ? Theme.TextC : Theme.SubText, flags);
            }
        }
    }

    // 自绘按钮：V5 起改为圆角填充 + 可选描边；禁用态用自定义颜色（系统 GrayText 在深色主题下几乎看不见）
    public class FlatButton : Button
    {
        public Color OffFore = Color.Gray;
        public Color OffBorder = Color.Transparent;
        public Color Stroke = Color.Transparent;
        public int Radius = 8;

        public FlatButton()
        {
            FlatStyle = FlatStyle.Flat;
            UseVisualStyleBackColor = false;
            TabStop = false;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.Width < 2 || this.Height < 2) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 圆角外侧要用“卡片底色”垫掉，否则会留下方角痕迹
            Color under = Theme.Card;
            if (Parent != null && Parent.BackColor != Theme.Page) under = Parent.BackColor;
            g.Clear(under);

            Rectangle r = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            Color fill = Enabled ? BackColor : Theme.Neutral;
            int rad = Radius;
            if (rad > 2 && this.Width > rad * 2 && this.Height > rad * 2)
            {
                using (GraphicsPath path = ProgressPanel.Round(r, rad))
                {
                    using (SolidBrush br = new SolidBrush(fill)) g.FillPath(br, path);
                    Color stroke = Enabled ? Stroke : OffBorder;
                    if (stroke.A > 0)
                    {
                        using (Pen pen = new Pen(stroke, 1)) g.DrawPath(pen, path);
                    }
                }
            }
            else
            {
                using (SolidBrush br = new SolidBrush(fill)) g.FillRectangle(br, r);
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
                | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(g, Text, Font, r, Enabled ? ForeColor : OffFore, flags);
        }
    }
}


