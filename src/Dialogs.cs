using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FileDedup
{
    // 对话框：字体设置 / 自定义主题 / 关于（版本信息 + 作者）
    internal static class Dialogs
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        // 让对话框按真实 DPI 等比放大（与主窗口相同的做法）
        public static void Dpi(Form f)
        {
            uint dpi = 96;
            try { if (f.Handle != IntPtr.Zero) dpi = GetDpiForWindow(f.Handle); }
            catch { }
            if (dpi == 0) dpi = 96;
            float s = dpi / 96f;
            if (s > 1.02f || s < 0.98f)
            {
                f.SuspendLayout();
                f.Scale(new SizeF(s, s));
                f.ResumeLayout(true);
            }
        }

        public static Button Btn(string text, Color bg, Color fg, int w, int h)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(w, h);
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = bg;
            b.ForeColor = fg;
            b.Font = new Font(Settings.UiFont(), 9.5F);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            return b;
        }

        public static Label Lbl(string text, float size, bool bold, Color color)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Font = new Font(Settings.UiFont(), size, bold ? FontStyle.Bold : FontStyle.Regular);
            l.ForeColor = color;
            return l;
        }

        // ============================================================
        //  字体设置
        // ============================================================
        public static bool ShowFont(IWin32Window owner, string currentName, double currentPt,
                                    out string name, out double pt)
        {
            name = currentName;
            pt = currentPt;
            string selName = currentName;
            double selPt = currentPt;

            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("字体设置");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(452, 384);
            f.Load += delegate { Dpi(f); };

            Label lbFont = Lbl(Lang.T("字体"), 9.5F, true, Theme.TextC);
            lbFont.Location = new Point(24, 20);

            ListBox lst = new ListBox();
            lst.Location = new Point(24, 44);
            lst.Size = new Size(404, 150);
            lst.BorderStyle = BorderStyle.FixedSingle;
            lst.BackColor = Theme.InputBg;
            lst.ForeColor = Theme.TextC;
            lst.Font = new Font(Settings.UiFont(), 10F);
            lst.IntegralHeight = false;

            string[] fonts;
            try
            {
                System.Drawing.Text.InstalledFontCollection col = new System.Drawing.Text.InstalledFontCollection();
                List<string> names = new List<string>();
                foreach (System.Drawing.FontFamily ff in col.Families) names.Add(ff.Name);
                names.Sort(StringComparer.OrdinalIgnoreCase);
                fonts = names.ToArray();
            }
            catch
            {
                fonts = new string[] { "Microsoft YaHei UI", "SimSun", "SimHei", "Arial", "Segoe UI" };
            }
            lst.Items.AddRange(fonts);
            int sel = -1;
            for (int i = 0; i < fonts.Length; i++)
                if (string.Equals(fonts[i], currentName, StringComparison.OrdinalIgnoreCase)) { sel = i; break; }
            if (sel < 0) sel = 0;
            lst.SelectedIndex = sel;

            Label lbSize = Lbl(Lang.T("字号"), 9.5F, true, Theme.TextC);
            lbSize.Location = new Point(24, 206);

            NumericUpDown num = new NumericUpDown();
            num.Location = new Point(lbSize.Right + 8, 202);
            num.Size = new Size(74, 26);
            num.Font = new Font(Settings.UiFont(), 9.5F);
            num.DecimalPlaces = 1;
            num.Increment = 0.5M;
            num.Minimum = 6M;
            num.Maximum = 24M;
            decimal ptValue = (decimal)currentPt;
            if (ptValue < num.Minimum) ptValue = num.Minimum;
            if (ptValue > num.Maximum) ptValue = num.Maximum;
            num.Value = ptValue;

            Label lbPreview = Lbl(Lang.T("预览"), 9.5F, true, Theme.TextC);
            lbPreview.Location = new Point(num.Right + 22, 206);

            Label lbSample = new Label();
            lbSample.Location = new Point(24, 236);
            lbSample.Size = new Size(404, 48);
            lbSample.TextAlign = ContentAlignment.MiddleCenter;
            lbSample.BackColor = Theme.InputBg;
            lbSample.ForeColor = Theme.TextC;
            lbSample.Text = Lang.T("预览文字：文件查重工具 Aa 123");

            Label lbHint = Lbl(Lang.T("字号越大，界面整体越大（适合高分屏或视力不便时使用）。"), 8.5F, false, Theme.SubText);
            lbHint.Location = new Point(24, 292);
            lbHint.MaximumSize = new Size(404, 0);

            EventHandler upd = delegate
            {
                string nm = (lst.SelectedItem == null) ? Settings.FontName : lst.SelectedItem.ToString();
                try { lbSample.Font = new Font(nm, (float)num.Value); }
                catch { lbSample.Font = new Font(Settings.UiFont(), (float)num.Value); }
            };
            lst.SelectedIndexChanged += upd;
            num.ValueChanged += upd;
            upd(null, EventArgs.Empty);

            Button reset = Btn(Lang.T("还原默认"), Theme.Neutral, Theme.TextC, 100, 34);
            reset.Location = new Point(24, 334);
            reset.Click += delegate
            {
                for (int i = 0; i < fonts.Length; i++)
                    if (string.Equals(fonts[i], "Microsoft YaHei UI", StringComparison.OrdinalIgnoreCase)) { lst.SelectedIndex = i; break; }
                num.Value = 9M;
                upd(null, EventArgs.Empty);
            };

            Button cancel = Btn(Lang.T("取消"), Theme.Neutral, Theme.TextC, 96, 34);
            cancel.Location = new Point(232, 334);
            cancel.DialogResult = DialogResult.Cancel;

            Button ok = Btn(Lang.T("确定"), Theme.Accent, Theme.OnAccent, 96, 34);
            ok.Location = new Point(340, 334);
            ok.Click += delegate
            {
                selName = (lst.SelectedItem == null) ? currentName : lst.SelectedItem.ToString();
                selPt = (double)num.Value;
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            f.Controls.Add(lbFont);
            f.Controls.Add(lst);
            f.Controls.Add(lbSize);
            f.Controls.Add(num);
            f.Controls.Add(lbPreview);
            f.Controls.Add(lbSample);
            f.Controls.Add(lbHint);
            f.Controls.Add(reset);
            f.Controls.Add(cancel);
            f.Controls.Add(ok);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            f.ShowDialog(owner);
            bool done = (f.DialogResult == DialogResult.OK);
            if (done)
            {
                name = selName;
                pt = selPt;
            }
            f.Dispose();
            return done;
        }

        // ============================================================
        //  自定义主题（背景色 / 背景图片）
        // ============================================================
        public static bool ShowCustomTheme(IWin32Window owner, ref Color bg, ref string imgPath, ref bool glass, ref int glassOpacity)
        {
            Color cur = bg;
            string curImg = imgPath;
            Color selBg = bg;
            string selImg = imgPath;
            bool changed = false;
            bool curGlass = glass;
            int curOpacity = glassOpacity;

            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("自定义主题");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(456, 432);
            f.Load += delegate { Dpi(f); };

            Label lbBg = Lbl(Lang.T("背景颜色"), 9.5F, true, Theme.TextC);
            lbBg.Location = new Point(24, 22);

            Panel swatch = new Panel();
            swatch.Location = new Point(24, 50);
            swatch.Size = new Size(120, 56);
            swatch.BackColor = cur;
            swatch.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen pen = new Pen(Theme.Line)) e.Graphics.DrawRectangle(pen, 0, 0, swatch.Width - 1, swatch.Height - 1);
            };

            Button pick = Btn(Lang.T("选择背景色"), Theme.Neutral, Theme.TextC, 124, 34);
            pick.Location = new Point(160, 60);
            pick.Click += delegate
            {
                using (ColorDialog cd = new ColorDialog())
                {
                    cd.Color = swatch.BackColor;
                    cd.FullOpen = true;
                    if (cd.ShowDialog(f) == DialogResult.OK)
                    {
                        swatch.BackColor = cd.Color;
                        changed = true;
                    }
                }
            };

            Label lbImg = Lbl(Lang.T("背景图片"), 9.5F, true, Theme.TextC);
            lbImg.Location = new Point(24, 126);

            Label lbImgPath = Lbl((curImg.Length == 0) ? Lang.T("未选择") : curImg, 8.5F, false, Theme.SubText);
            lbImgPath.Location = new Point(24, 152);
            lbImgPath.MaximumSize = new Size(400, 0);

            Button chooseImg = Btn(Lang.T("选择图片…"), Theme.Neutral, Theme.TextC, 124, 34);
            chooseImg.Location = new Point(24, 196);
            Button clearImg = Btn(Lang.T("清除图片"), Theme.Neutral, Theme.TextC, 100, 34);
            clearImg.Location = new Point(160, 196);

            Label lbNote = Lbl(Lang.T("卡片会自动保持可读的底色；图片会铺满窗口背景。"), 8.5F, false, Theme.SubText);
            lbNote.Location = new Point(24, 238);
            lbNote.MaximumSize = new Size(400, 0);

            CheckBox chkGlass = new CheckBox();
            chkGlass.Text = Lang.T("半透明玻璃效果（卡片透出模糊的背景图）");
            chkGlass.Location = new Point(24, 286);
            chkGlass.AutoSize = true;
            chkGlass.Checked = curGlass;
            chkGlass.ForeColor = Theme.TextC;
            chkGlass.BackColor = Theme.Card;
            chkGlass.Font = new Font(Settings.UiFont(), 9F);

            Label lbOpacity = Lbl(Lang.T("透明度"), 9F, false, Theme.TextC);
            lbOpacity.Location = new Point(24, 318);
            lbOpacity.AutoSize = true;

            Label lbOpacityVal = Lbl(curOpacity.ToString() + "%", 9F, true, Theme.TextC);
            lbOpacityVal.Location = new Point(120, 318);
            lbOpacityVal.AutoSize = true;

            TrackBar barOpacity = new TrackBar();
            barOpacity.Minimum = 0;
            barOpacity.Maximum = 100;
            barOpacity.TickFrequency = 10;
            barOpacity.SmallChange = 1;
            barOpacity.LargeChange = 10;
            barOpacity.Value = curOpacity;
            barOpacity.Location = new Point(150, 310);
            barOpacity.Size = new Size(276, 45);
            barOpacity.BackColor = Theme.Card;
            barOpacity.ForeColor = Theme.TextC;

            Label lbOpacityHint = Lbl(Lang.T("0% = 完全不透明，100% = 完全透明"), 8F, false, Theme.SubText);
            lbOpacityHint.Location = new Point(24, 350);
            lbOpacityHint.AutoSize = true;

            barOpacity.ValueChanged += delegate
            {
                curOpacity = barOpacity.Value;
                lbOpacityVal.Text = curOpacity.ToString() + "%";
                changed = true;
            };

            chkGlass.CheckedChanged += delegate
            {
                curGlass = chkGlass.Checked;
                changed = true;
                barOpacity.Enabled = curGlass;
                lbOpacity.ForeColor = (curGlass ? Theme.TextC : Theme.SubText);
                lbOpacityVal.ForeColor = (curGlass ? Theme.TextC : Theme.SubText);
                lbOpacityHint.ForeColor = (curGlass ? Theme.SubText : Theme.Line);
            };
            barOpacity.Enabled = curGlass;
            if (!curGlass)
            {
                lbOpacity.ForeColor = Theme.SubText;
                lbOpacityVal.ForeColor = Theme.SubText;
                lbOpacityHint.ForeColor = Theme.Line;
            }

            chooseImg.Click += delegate
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Title = Lang.T("背景图片");
                    ofd.Filter = Lang.T("图片文件 (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif");
                    if (ofd.ShowDialog(f) == DialogResult.OK)
                    {
                        curImg = ofd.FileName;
                        lbImgPath.Text = curImg;
                        changed = true;
                    }
                }
            };
            clearImg.Click += delegate
            {
                curImg = "";
                lbImgPath.Text = Lang.T("未选择");
                changed = true;
            };

            Button ok = Btn(Lang.T("确定"), Theme.Accent, Theme.OnAccent, 96, 34);
            ok.Location = new Point(328, 382);
            Button cancel = Btn(Lang.T("取消"), Theme.Neutral, Theme.TextC, 96, 34);
            cancel.Location = new Point(220, 382);
            cancel.DialogResult = DialogResult.Cancel;

            ok.Click += delegate
            {
                selBg = swatch.BackColor;
                selImg = curImg;
                changed = true;
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            f.Controls.Add(lbBg);
            f.Controls.Add(swatch);
            f.Controls.Add(pick);
            f.Controls.Add(lbImg);
            f.Controls.Add(lbImgPath);
            f.Controls.Add(chooseImg);
            f.Controls.Add(clearImg);
            f.Controls.Add(lbNote);
            f.Controls.Add(chkGlass);
            f.Controls.Add(lbOpacity);
            f.Controls.Add(lbOpacityVal);
            f.Controls.Add(barOpacity);
            f.Controls.Add(lbOpacityHint);
            f.Controls.Add(cancel);
            f.Controls.Add(ok);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            f.ShowDialog(owner);
            bool isOk = (f.DialogResult == DialogResult.OK) && changed;
            if (isOk)
            {
                bg = selBg;
                imgPath = selImg;
                glass = curGlass;
                glassOpacity = curOpacity;
            }
            f.Dispose();
            return isOk;
        }

        // ============================================================
        //  关于软件（简介 / 使用说明 / 支持的格式 / 开发者主页）
        // ============================================================
        private static readonly string[] AuthorLinksLabel = new string[] { "B站", "油管", "新片场" };
        private static readonly string[] AuthorLinksUrl = new string[] {
            "https://space.bilibili.com/430925916",
            "https://www.youtube.com/@hujason1",
            "https://www.xinpianchang.com/u11140498"
        };
        private static readonly string[] AuthorLinksText = new string[] {
            "space.bilibili.com/430925916",
            "youtube.com/@hujason1",
            "xinpianchang.com/u11140498"
        };
        private static readonly string[] PhotoLinksUrl = new string[] {
            "https://www.pexels.com/@hujason",
            "https://unsplash.com/@hujason",
            "https://500px.com.cn/hujason"
        };
        private static readonly string[] PhotoLinksText = new string[] {
            "www.pexels.com/@hujason",
            "www.unsplash.com/@hujason",
            "www.500px.com.cn/hujason"
        };

        private static void OpenUrl(string url)
        {
            try { Process.Start(url); } catch { }
        }

        private static int TextHeight(Label l, int w)
        {
            l.MaximumSize = new Size(w, 0);
            Size sz = TextRenderer.MeasureText(l.Text, l.Font, new Size(w, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);
            return sz.Height + 2;
        }

        private static Label AddHead(Panel p, string text, int x, ref int y, int w)
        {
            Label l = Lbl(text, 10.5F, true, Theme.TextC);
            l.BackColor = Theme.Card;
            l.Location = new Point(x, y);
            int h = TextHeight(l, w);
            p.Controls.Add(l);
            y += h + 8;
            return l;
        }

        private static Label AddBody(Panel p, string text, int x, ref int y, int w, Color color, float size)
        {
            Label l = Lbl(text, size, false, color);
            l.BackColor = Theme.Card;
            l.Location = new Point(x, y);
            int h = TextHeight(l, w);
            p.Controls.Add(l);
            y += h + 6;
            return l;
        }

        private static Label AddBody(Panel p, string text, int x, ref int y, int w, Color color)
        {
            return AddBody(p, text, x, ref y, w, color, 9F);
        }

        private static LinkLabel AddLink(Panel p, string prefix, string shown, string url, int x, ref int y, int w)
        {
            LinkLabel l = new LinkLabel();
            l.Text = (prefix.Length == 0) ? shown : (prefix + "    " + shown);
            l.AutoSize = true;
            l.Font = new Font(Settings.UiFont(), 9F);
            l.ForeColor = Theme.SubText;
            l.BackColor = Theme.Card;
            l.LinkColor = Theme.Accent;
            l.ActiveLinkColor = Theme.Accent;
            l.VisitedLinkColor = Theme.Accent;
            l.LinkBehavior = LinkBehavior.HoverUnderline;
            int st = l.Text.IndexOf(shown);
            if (st >= 0) l.LinkArea = new LinkArea(st, shown.Length);
            l.Location = new Point(x, y);
            int h = TextHeight(l, w);
            l.LinkClicked += delegate { OpenUrl(url); };
            p.Controls.Add(l);
            y += h + 4;
            return l;
        }

        // 开发者 + 各平台主页（供“关于软件”“关于作者”共用）
        private static void AddAuthorBlock(Panel p, int x, ref int y, int w)
        {
            AddBody(p, Lang.F("开发者：{0}", Lang.T("摄影师hujason")), x, ref y, w, Theme.TextC, 9.5F);

            for (int i = 0; i < AuthorLinksLabel.Length; i++)
            {
                AddLink(p, Lang.T(AuthorLinksLabel[i]), AuthorLinksText[i], AuthorLinksUrl[i], x, ref y, w);
            }

            AddBody(p, Lang.T("照片网站"), x, ref y, w, Theme.SubText, 9F);
            for (int i = 0; i < PhotoLinksText.Length; i++)
            {
                AddLink(p, "", PhotoLinksText[i], PhotoLinksUrl[i], x + 12, ref y, w - 12);
            }

            AddBody(p, Lang.F("{0}：{1}", Lang.T("微博"), "@摄影师hujason"), x, ref y, w, Theme.SubText, 9F);
            AddBody(p, Lang.F("{0}：{1}", Lang.T("抖音"), "@摄影师hujason"), x, ref y, w, Theme.SubText, 9F);
            AddBody(p, Lang.F("{0}：{1}", Lang.T("小红书"), "@摄影师hujason"), x, ref y, w, Theme.SubText, 9F);
        }

        public static void ShowAuthor(IWin32Window owner)
        {
            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("关于作者");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(470, 478);
            f.Load += delegate { Dpi(f); };

            Panel body = new Panel();
            body.Location = new Point(0, 0);
            body.Size = new Size(470, 402);
            body.BackColor = Theme.Card;

            int x = 26, y = 22, w = 410;
            AddHead(body, Lang.T("关于作者"), x, ref y, w);
            y += 6;
            AddAuthorBlock(body, x, ref y, w);

            Button close = Btn(Lang.T("关闭"), Theme.Accent, Theme.OnAccent, 96, 34);
            close.Location = new Point(348, 416);
            close.DialogResult = DialogResult.Cancel;

            f.Controls.Add(body);
            f.Controls.Add(close);
            f.AcceptButton = close;
            f.CancelButton = close;
            f.ShowDialog(owner);
            f.Dispose();
        }

        // 各分类的扩展名清单（与程序中实际认领的集合保持一致，按可读顺序排列）
        private static readonly string FormatImg =
            "jpg/jpeg/jpe/jfif、png/apng、gif、bmp/dib、tif/tiff、webp、heic/heif、avif、jxl、jp2/j2k/jpx、ico/cur、svg/svgz、wmf/emf、psd/psb、ai/eps、tga/icb、dds/exr/hdr/pfm、fits、dpx/cin、RAW：cr2/cr3/crw/nef/nrw/arw/sr2/srf/orf/rw2/rwl/raf/pef/x3f/erf/mrw/dcr/k25/3fr/fff/iiq";
        private static readonly string FormatVid =
            "mp4/m4v/m4s、mov/qt、avi/divx/xvid、mkv、webm、wmv/asf/wm、flv/f4v、mpg/mpeg/m1v/m2v/mpv、vob/vro、ts/m2ts/mts/m2t、tp/trp/tod、mxf、dv/hdv、mod/mv、3gp/3g2、ogv/ogm、rm/rmvb、wtv/dvr-ms、r3d/braw/ari、insv/lrv/360、h264/h265/hevc/av1/vp9/vp8、bik/smk";
        private static readonly string FormatDoc =
            "txt/md/rst/rtf/log/tex、doc/docx/docm/dot、wps/wpt、odt/ott、xls/xlsx/xlsm/xlsb/xlt、et/ett、ods/ots、ppt/pptx/pptm/pps/pot、dps/dpt、odp/otp、vsd/vsdx/vss/vst、dwg/dxf/dwf、xmind、pdf/xps/oxps、djvu/caj/ofd/uof、epub/mobi/azw/azw3/fb2/chm、pages/key/numbers、mpp/mpt、pub、mdb/accdb、one/indd、html/mht、json/xml/yaml/ini、srt/ass/vtt";
        private static readonly string FormatAud =
            "mp3/mp2/mpga、wav/wave、flac、aac/m4a/m4b/m4p、ogg/oga/opus/spx、wma、ape、wv、tta、aiff/aif、au/snd、mid/midi/rmi、amr/awb、ac3/dts/dtshd/mlp/mka、caf/w64/voc/mpc、dsf/dff";
        private static readonly string FormatArc =
            "zip/zipx、rar/r00、7z、tar/tgz/tbz2/txz、gz/bz2/xz/lzma/zst、cab、arj/lzh/lha/ace、cpio、rpm/deb、iso/img/dmg、vhd/vhdx/wim/esd、msi/msix/appx、jar/war/ear、apk/aab/xapk/ipa、xpi/crx/nupkg/whl/gem、sit/sitx、001/002/003";

        // 把控件按真实高度串成流式布局（DPI 缩放下也准确，不依赖 PreferredSize）
        private static void FlowAdd(Panel p, List<Control> flow, List<int> gaps, Label l, int x, int w, int gapAfter)
        {
            l.BackColor = Theme.Card;
            l.MaximumSize = new Size(w, 0);
            l.Left = x;
            p.Controls.Add(l);
            flow.Add(l);
            gaps.Add(gapAfter);
        }

        private static void FlowText(Panel p, List<Control> flow, List<int> gaps, string text, float size, bool bold, Color color, int x, int w, int gapAfter)
        {
            FlowAdd(p, flow, gaps, Lbl(text, size, bold, color), x, w, gapAfter);
        }

        private static void FlowLink(Panel p, List<Control> flow, List<int> gaps, string prefix, string shown, string url, int x, int w, int gapAfter)
        {
            LinkLabel l = new LinkLabel();
            l.Text = (prefix.Length == 0) ? shown : (prefix + "    " + shown);
            l.AutoSize = true;
            l.Font = new Font(Settings.UiFont(), 9F);
            l.ForeColor = Theme.SubText;
            l.BackColor = Theme.Card;
            l.LinkColor = Theme.Accent;
            l.ActiveLinkColor = Theme.Accent;
            l.VisitedLinkColor = Theme.Accent;
            l.LinkBehavior = LinkBehavior.HoverUnderline;
            int st = l.Text.IndexOf(shown);
            if (st >= 0) l.LinkArea = new LinkArea(st, shown.Length);
            l.LinkClicked += delegate { OpenUrl(url); };
            FlowAdd(p, flow, gaps, l, x, w, gapAfter);
        }

        private static void FlowAuthor(Panel p, List<Control> flow, List<int> gaps, int x, int w)
        {
            FlowText(p, flow, gaps, Lang.F("开发者：{0}", Lang.T("摄影师hujason")), 9.5F, true, Theme.TextC, x, w, 6);
            for (int i = 0; i < AuthorLinksLabel.Length; i++)
                FlowLink(p, flow, gaps, Lang.T(AuthorLinksLabel[i]), AuthorLinksText[i], AuthorLinksUrl[i], x, w, 4);
            FlowText(p, flow, gaps, Lang.T("照片网站"), 9F, false, Theme.SubText, x, w, 3);
            for (int i = 0; i < PhotoLinksText.Length; i++)
                FlowLink(p, flow, gaps, "", PhotoLinksText[i], PhotoLinksUrl[i], x + 12, w - 12, 4);
            FlowText(p, flow, gaps, Lang.F("{0}：{1}", Lang.T("微博"), "@摄影师hujason"), 9F, false, Theme.SubText, x, w, 3);
            FlowText(p, flow, gaps, Lang.F("{0}：{1}", Lang.T("抖音"), "@摄影师hujason"), 9F, false, Theme.SubText, x, w, 3);
            FlowText(p, flow, gaps, Lang.F("{0}：{1}", Lang.T("小红书"), "@摄影师hujason"), 9F, false, Theme.SubText, x, w, 4);
        }

        private static void FlowPack(Panel content, List<Control> flow, List<int> gaps, float s)
        {
            content.SuspendLayout();
            int yy = (int)(22 * s);
            for (int i = 0; i < flow.Count; i++)
            {
                flow[i].Top = yy;
                yy += flow[i].Height + (int)(gaps[i] * s);
            }
            content.AutoScrollMinSize = new Size(0, yy + (int)(20 * s));
            content.ResumeLayout(true);
        }

        public static void ShowAboutSoftware(IWin32Window owner, string version)
        {
            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("关于软件");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(760, 844);

            Panel band = new Panel();
            band.Location = new Point(0, 0);
            band.Size = new Size(760, 88);
            band.Paint += delegate(object s, PaintEventArgs e)
            {
                Panel bp = (Panel)s;
                using (LinearGradientBrush br = new LinearGradientBrush(new Rectangle(0, 0, bp.Width, bp.Height),
                           Theme.HeaderFrom, Theme.HeaderTo, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(br, 0, 0, bp.Width, bp.Height);
                }
            };
            Label lbName = Lbl(Lang.T("文件查重工具"), 15F, true, Theme.OnAccent);
            lbName.Location = new Point(24, 18);
            lbName.BackColor = Color.Transparent;
            Label lbVer = Lbl(Lang.F("{0} · 版本 1.4", Lang.T("关于软件")), 9F, false, Theme.SubOnHeader);
            lbVer.Location = new Point(26, 54);
            lbVer.BackColor = Color.Transparent;
            band.Controls.Add(lbName);
            band.Controls.Add(lbVer);

            Panel content = new Panel();
            content.Location = new Point(0, 88);
            content.Size = new Size(760, 844 - 88 - 78);
            content.BackColor = Theme.Card;
            content.AutoScroll = true;

            List<Control> flow = new List<Control>();
            List<int> gaps = new List<int>();
            int x = 28, w = 664;

            FlowText(content, flow, gaps, Lang.T("版本与定位"), 10.5F, true, Theme.TextC, x, w, 8);
            FlowText(content, flow, gaps, Lang.T("文件查重工具 1.4 是一个免安装的 Windows 单文件查重工具：按文件内容（不是文件名、不是后缀）判断重复，显示每个副本的完整路径，可按大小/时间/路径排序，并可删除到回收站或永久删除；勾选“相似图片”还能按画面比对照片，并并排预览缩略图与拍摄参数。"),
                9F, false, Theme.SubText, x, w, 18);

            FlowText(content, flow, gaps, Lang.T("使用说明（要点）"), 10.5F, true, Theme.TextC, x, w, 8);
            FlowText(content, flow, gaps, Lang.T("1. 把要检查的文件夹拖进左侧“扫描范围”（可添加多个），或点“添加文件夹”。\n2. 选择文件类型；想连陌生后缀、改过名的文件一起查，勾选“全文件扫描”。\n3. 在“扫描设置”里选择扫描层级（仅当前目录 / 含 1-3 层 / 所有子目录）与最小文件大小。\n4. 点“开始扫描”，扫描过程中可“暂停/继续”，也可以“取消扫描”。\n5. 结果按组显示：勾选要删的副本 → “删除到回收站”或“永久删除”；也可“导出清单”存成 CSV。\n6. 勾选“相似图片（感知哈希）”可选三档阈值（严格/标准/宽松），按画面找相似照片；选中一组，右侧“对比预览”会并排显示缩略图，并对照分辨率、位深色域、拍摄时间、相机与镜头（EXIF）。\n7. 顶部菜单：文件（字体/导出/编辑）、视图（白色/深色/自定义背景）、语言（简中/繁中/English/日本語）、关于。"),
                9F, false, Theme.SubText, x, w, 18);

            FlowText(content, flow, gaps, Lang.T("支持的文件格式"), 10.5F, true, Theme.TextC, x, w, 8);
            FlowText(content, flow, gaps, Lang.T("格式按分类认领（共约 380 种），没列到的可以在“补充扩展名”里自己加；勾选“全文件扫描”则完全不看扩展名。"),
                9F, false, Theme.SubText, x, w, 10);

            FlowText(content, flow, gaps, Lang.T("图片（98 种）"), 9F, true, Theme.TextC, x, w, 4);
            FlowText(content, flow, gaps, FormatImg, 8.5F, false, Theme.SubText, x + 12, w - 12, 10);
            FlowText(content, flow, gaps, Lang.T("视频（71 种）"), 9F, true, Theme.TextC, x, w, 4);
            FlowText(content, flow, gaps, FormatVid, 8.5F, false, Theme.SubText, x + 12, w - 12, 10);
            FlowText(content, flow, gaps, Lang.T("文档（110 种）"), 9F, true, Theme.TextC, x, w, 4);
            FlowText(content, flow, gaps, FormatDoc, 8.5F, false, Theme.SubText, x + 12, w - 12, 10);
            FlowText(content, flow, gaps, Lang.T("音频（47 种）"), 9F, true, Theme.TextC, x, w, 4);
            FlowText(content, flow, gaps, FormatAud, 8.5F, false, Theme.SubText, x + 12, w - 12, 10);
            FlowText(content, flow, gaps, Lang.T("压缩包（55 种）"), 9F, true, Theme.TextC, x, w, 4);
            FlowText(content, flow, gaps, FormatArc, 8.5F, false, Theme.SubText, x + 12, w - 12, 18);

            FlowText(content, flow, gaps, Lang.T("开发者与主页"), 10.5F, true, Theme.TextC, x, w, 8);
            FlowAuthor(content, flow, gaps, x, w);

            Button close = Btn(Lang.T("关闭"), Theme.Accent, Theme.OnAccent, 96, 34);
            close.Location = new Point(628, 844 - 56);
            close.DialogResult = DialogResult.Cancel;

            f.Controls.Add(band);
            f.Controls.Add(close);
            f.Controls.Add(content);
            f.AcceptButton = close;
            f.CancelButton = close;

            f.Load += delegate
            {
                Dpi(f);
                float s = f.ClientSize.Width / 760f;
                content.SuspendLayout();
                int yy = (int)(22 * s);
                for (int i = 0; i < flow.Count; i++)
                {
                    flow[i].Top = yy;
                    yy += flow[i].Height + (int)(gaps[i] * s);
                }
                content.AutoScrollMinSize = new Size(0, yy + (int)(18 * s));
                content.ResumeLayout(true);
            };

            f.ShowDialog(owner);
            f.Dispose();
        }


        // ============================================================
        //  关于（版本说明 / 升级 / 作者）
        // ============================================================
        public static void ShowAbout(IWin32Window owner, string version)
        {
            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("关于");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(500, 424);
            f.Load += delegate { Dpi(f); };

            Panel band = new Panel();
            band.Location = new Point(0, 0);
            band.Size = new Size(500, 88);
            band.Paint += delegate(object s, PaintEventArgs e)
            {
                using (LinearGradientBrush br = new LinearGradientBrush(new Rectangle(0, 0, band.Width, band.Height),
                           Theme.HeaderFrom, Theme.HeaderTo, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(br, 0, 0, band.Width, band.Height);
                }
            };

            Label lbName = Lbl(Lang.T("文件查重工具"), 15F, true, Theme.OnAccent);
            lbName.Location = new Point(24, 18);
            lbName.BackColor = Color.Transparent;
            Label lbVer = Lbl(Lang.T("版本 1.4"), 9F, false, Theme.SubOnHeader);
            lbVer.Location = new Point(26, 54);
            lbVer.BackColor = Color.Transparent;
            band.Controls.Add(lbName);
            band.Controls.Add(lbVer);

            Label lbNow = Lbl(Lang.T("当前版本") + "：" + version, 9.5F, true, Theme.TextC);
            lbNow.Location = new Point(24, 104);

            Label lbDesc = Lbl(Lang.T("本版说明"), 9.5F, true, Theme.TextC);
            lbDesc.Location = new Point(24, 132);

            Label lbDescBody = Lbl(Lang.T("1.4 版：新增「选择助手」，按规则自动挑选保留的副本，可设每组留几份，并预览要删多少、能省多少空间；新增扫描完成统计报告；表格新增「创建时间」列，列可自由开关，列宽与窗口大小自动记忆。1.3 的透明度滑杆与深色提亮照旧。"), 9F, false, Theme.SubText);
            lbDescBody.Location = new Point(24, 156);
            lbDescBody.MaximumSize = new Size(452, 0);

            Label lbAuthor = Lbl(Lang.F("开发者：{0}", Lang.T("摄影师hujason")), 9.5F, false, Theme.TextC);
            lbAuthor.Location = new Point(24, 252);

            Label lbTip = Lbl(Lang.T("本工具为本地单机版，没有在线升级服务器；当前已是最新版本 1.4。"), 8.5F, false, Theme.SubText);
            lbTip.Location = new Point(24, 296);
            lbTip.MaximumSize = new Size(452, 0);

            Button check = Btn(Lang.T("检查更新"), Theme.AccentSoft, Theme.Accent, 108, 34);
            check.Location = new Point(24, 352);
            check.Click += delegate
            {
                MessageBox.Show(f, Lang.T("本工具为本地单机版，没有在线升级服务器；当前已是最新版本 1.4。"),
                    Lang.T("升级提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Button close = Btn(Lang.T("关闭"), Theme.Accent, Theme.OnAccent, 96, 34);
            close.Location = new Point(380, 352);
            close.DialogResult = DialogResult.Cancel;

            Button more = Btn(Lang.T("关于软件…"), Theme.Neutral, Theme.TextC, 108, 34);
            more.Location = new Point(140, 352);
            more.Click += delegate { ShowAboutSoftware(f, version); };

            f.Controls.Add(band);
            f.Controls.Add(lbNow);
            f.Controls.Add(lbDesc);
            f.Controls.Add(lbDescBody);
            f.Controls.Add(lbAuthor);
            f.Controls.Add(lbTip);
            f.Controls.Add(check);
            f.Controls.Add(more);
            f.Controls.Add(close);
            f.AcceptButton = close;
            f.CancelButton = close;

            f.ShowDialog(owner);
            Dpi(f);
            f.Dispose();
        }
    }
}






