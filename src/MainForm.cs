using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FileDedup
{
    public class MainForm : Form
    {
        // ---------- 配色（随主题切换，由 ApplyThemeVars 赋値） ----------
        private static Color CPage;
        private static Color CCard;
        private static Color CAccent;
        private static Color CAccentSoft;
        private static Color CNeutral;
        private static Color CDanger;
        private static Color CDangerSoft;
        private static Color CText;
        private static Color CSubText;
        private static Color CLine;

        private static Color CBand;
        private static Color CGood;
        private static Color CInputBg;
        private static Color CStatusBg;
        private static Color CMenuBg;
        private static Color CMenuSel;
        private static Color CHdrFrom;
        private static Color CHdrTo;
        private static Color CSubOnHeader;
        private static Color CGridLine;
        private static Color CGridHeader;
        private static Color CGridSel;
        private static Color COnAccent;
        private static Color CFieldLine;
        private static string UiFont = "Microsoft YaHei UI";

        // ---------- 扩展名分类（尽可能覆盖常见与专业格式） ----------
        private static readonly HashSet<string> ImgExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            // 常见位图
            ".jpg", ".jpeg", ".jpe", ".jfif", ".jfi", ".pjpeg", ".pjp", ".png", ".apng", ".gif", ".bmp", ".dib", ".rle",
            ".tif", ".tiff", ".tga", ".icb", ".vda", ".webp", ".heic", ".heif", ".avif", ".jxl", ".jp2", ".j2k", ".jpf",
            ".jpx", ".jpm", ".wdp", ".hdp", ".jxr", ".dds", ".exr", ".hdr", ".pfm", ".pcx", ".ppm", ".pgm", ".pbm", ".pnm",
            ".xpm", ".xbm", ".sgi", ".rgb", ".rgba", ".bw", ".fits", ".fit", ".fts", ".dpx", ".cin", ".mng", ".jng", ".qoi",
            ".ras", ".pct", ".wbmp", ".jbig", ".jbg", ".hif",
            // 图标与矢量
            ".ico", ".cur", ".svg", ".svgz", ".wmf", ".emf", ".ai", ".eps", ".psd", ".psb",
            // 相机原始格式（RAW）
            ".raw", ".cr2", ".cr3", ".crw", ".nef", ".nrw", ".arw", ".srf", ".sr2", ".orf", ".rw2", ".rwl", ".raf",
            ".pef", ".ptx", ".srw", ".x3f", ".erf", ".mef", ".mos", ".mrw", ".dcr", ".kdc", ".k25", ".3fr", ".fff",
            ".iiq", ".cap"
        };
        private static readonly HashSet<string> VidExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            // 常见封装
            ".mp4", ".m4v", ".mov", ".qt", ".avi", ".divx", ".xvid", ".mkv", ".webm", ".wmv", ".asf", ".wm", ".flv",
            ".f4v", ".f4p", ".mpg", ".mpeg", ".mpe", ".m1v", ".m2v", ".mpv", ".m2p", ".mp2v", ".vob", ".vro", ".ifo",
            // 高清/蓝光/电视录制
            ".ts", ".m2ts", ".mts", ".tp", ".trp", ".tod", ".mxf", ".dv", ".hdv", ".mod", ".mv",
            ".wtv", ".dvr-ms", ".m2t", ".ismv", ".m4s", ".bik", ".smk",
            // 手机与网络
            ".3gp", ".3g2", ".3gpp", ".3gpp2", ".ogv", ".ogm", ".rm", ".rmvb", ".amv", ".y4m", ".yuv", ".ivf",
            // 影视专业/无损
            ".r3d", ".braw", ".ari", ".cine", ".insv", ".lrv", ".360",
            // 裸流
            ".h264", ".h265", ".hevc", ".264", ".265", ".av1", ".vp9", ".vp8"
        };
        private static readonly HashSet<string> DocExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            // 纯文本与标记
            ".txt", ".text", ".md", ".markdown", ".rst", ".rtf", ".log", ".tex", ".latex", ".bib", ".sty", ".csv",
            ".tsv", ".json", ".xml", ".yaml", ".yml", ".ini", ".conf", ".cfg", ".properties", ".toml", ".ipynb", ".rmd",
            // 网页
            ".htm", ".html", ".xhtml", ".shtml", ".mht", ".mhtml",
            // Word 系
            ".doc", ".docx", ".docm", ".dot", ".dotx", ".dotm", ".wps", ".wpt", ".odt", ".ott", ".rtf",
            // 表格
            ".xls", ".xlsx", ".xlsm", ".xlsb", ".xlt", ".xltx", ".xltm", ".et", ".ett", ".ods", ".ots",
            // 演示
            ".ppt", ".pptx", ".pptm", ".pps", ".ppsx", ".ppsm", ".pot", ".potx", ".potm", ".dps", ".dpt", ".odp", ".otp",
            // 绘图与流程图
            ".vsd", ".vsdx", ".vss", ".vst", ".vssx", ".vstx", ".vsdm", ".odg", ".otg", ".dwg", ".dxf", ".dwf", ".xmind",
            // 项目/出版/数据库
            ".mpp", ".mpt", ".pub", ".mdb", ".accdb", ".one", ".indd", ".ps",
            // 电子书与文档归档
            ".pdf", ".xps", ".oxps", ".djvu", ".djv", ".caj", ".epub", ".mobi", ".azw", ".azw3", ".fb2", ".lit", ".prc",
            ".chm", ".pages", ".key", ".numbers", ".ofd", ".uof",
            // 字幕
            ".srt", ".ass", ".ssa", ".vtt", ".lrc", ".sub"
        };
        private static readonly HashSet<string> AudExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            ".mp3", ".mp2", ".wav", ".wave", ".flac", ".aac", ".m4a", ".m4b", ".ogg", ".oga", ".opus", ".spx", ".wma",
            ".ape", ".wv", ".tta", ".aiff", ".aif", ".aifc", ".au", ".snd", ".mid", ".midi", ".rmi", ".kar", ".amr", ".awb",
            ".ac3", ".dts", ".dtshd", ".thd", ".mlp", ".mka", ".ra", ".caf", ".w64", ".voc", ".mpc", ".dsf", ".dff", ".3ga",
            ".aa", ".aa3", ".at3", ".oma", ".m4p", ".mpga",
        };
        private static readonly HashSet<string> ArcExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            ".zip", ".zipx", ".rar", ".r00", ".7z", ".tar", ".tgz", ".tbz", ".tbz2", ".txz", ".tlz", ".gz", ".bz2",
            ".xz", ".lzma", ".lz", ".z", ".zst", ".cab", ".arj", ".lzh", ".lha", ".ace", ".ar", ".cpio", ".rpm", ".deb",
            ".iso", ".img", ".dmg", ".vhd", ".vhdx", ".wim", ".esd", ".swm", ".msi", ".msix", ".appx", ".jar", ".war",
            ".ear", ".apk", ".aab", ".xapk", ".ipa", ".xpi", ".crx", ".nupkg", ".whl", ".gem", ".sit", ".sitx", ".001", ".002", ".003"
        };

        // ---------- 控件 ----------
        private ListBox lstFolders;
        private TextBox txtCustomExt;
        private CheckBox chkImages, chkVideos, chkDocs, chkAudio, chkArchives, chkOther, chkAllFiles, chkSimilar;
        private NumericUpDown numMin;
        private CheckBox chkSkipHidden, chkSkipZero;
        private ComboButton cboDepth, cboView;
        private Button btnT1, btnT2, btnT3;
        private Button btnScan, btnPause, btnStop, btnAdd, btnRemove, btnClear;
        private DataGridView dgv, dgvExif;
        private ComboButton cboKeepRule, cboSort;
        private Button btnSelectAll, btnSelectNone, btnRecycle, btnPermanent, btnExport, btnOpenFolder;
        private ProgressPanel pb;
        private Label lblStatus, lblPercent, lblSummary;
        private Label lblPathDetail;
        private Panel pnlPreview, pnlThumbs;
        private HScrollBar sbPreview;              // 对比预览整行的横向滚动条（与主表格同一根系统滚动条）
        private DupGroup _previewGroup;            // 当前预览的分组
        private int _previewScrollX;               // 预览行已横向滚动多少像素
        private int _previewStep;                  // 一张缩略图占的宽度（含间距）
        private int _previewTileW;                 // 缩略图宽度（自适应）
        private int _previewGap;                   // 缩略图之间的间距
        private int _previewCols;                  // 一屏能完整显示几张
        private int _previewShownCols;             // 参数表当前显示了几列（变了就重建表）
        private int _previewFirstIdx;              // 参数表当前从第几个文件开始
        private int _previewPerRow;                 // 每行摆几张缩略图
        private int _previewRows;                   // 缩略图一共几行
        private int _previewTileH;                  // 缩略图格子高度
        private bool _previewCompact;               // 格子太窄时只显示名字+状态
        private System.Windows.Forms.Timer _previewTimer;
        private bool _syncingPreview;              // 防止滚动条被程序改写时触发递归
        private Label lblPreviewTitle, lblPreviewHint, lblPreviewEmpty;
        private Button btnPreviewToggle;
        private TableLayoutPanel _mainGrid;
        private MenuStrip menu;
        private TableLayoutPanel _rootGrid;
        private float _uiScale = 1f;
        // 正在按配置还原列顺序时，不要让 ColumnDisplayIndexChanged 又把它写回一遍
        private bool _applyingColOrder;

        // ---------- 毛玻璃（自定义背景图时的半透明卡片） ----------
        private Bitmap _glassBase;
        private int _glassW, _glassH;
        private System.Windows.Forms.Timer _glassTimer;
        private bool _glassHooked;

        // ---------- 状态 ----------
        private static readonly string[] ColDefs = new string[] { "删除", "组", "文件名", "完整路径（双击行可打开文件）", "大小", "修改时间", "创建时间", "状态" };
        private const int CColDel = 0;
        private const int CColGrp = 1;
        private const int CColName = 2;
        private const int CColPath = 3;
        private const int CColSize = 4;
        private const int CColTime = 5;
        private const int CColCreated = 6;
        private const int CColNote = 7;

        private ScanSummary _lastSummary;
        private string _pendingOpen = "";
        // 重新按规则勾选时，不要把表格里旧的勾选状态写回模型（否则刚算出的勾选会被冲掉）
        private bool _skipPersist;
        private ToolStripMenuItem[] _miColToggles;

        private List<DupGroup> _groups = new List<DupGroup>();
        private CancellationTokenSource _cts;
        private PauseGate _gate;
        private bool _busy;
        private bool _ready;
        private bool _syncing;
        private int _sortMode;   // 0 大小↓ 1 大小↑ 2 时间↓ 3 时间↑ 4 路径
        private int _keepRule;   // 0 最早 1 最新 2 路径最短 3 分辨率最高 4 文件名最短
        private int _viewFilter; // 0 全部 1 精确重复 2 相似图片
        private int _similarThreshold = 6;
        private Dictionary<string, ImageMeta> _metaCache = new Dictionary<string, ImageMeta>();
        private Dictionary<string, Bitmap> _thumbCache = new Dictionary<string, Bitmap>();

        public MainForm()
        {
            _sortMode = Settings.Sort;
            _keepRule = Settings.KeepRule;
            BuildUi();
            Application.AddMessageFilter(new PreviewWheelFilter(this));
        }

        // 支持命令行传入目录（可配合“发送到”或批处理使用）：文件查重工具.exe <目录1> <目录2> --scan
        private string[] _startupPaths;
        private bool _autoScan;

        public MainForm(string[] args) : this()
        {
            if (args == null) return;
            List<string> paths = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a == null) continue;
                string t = a.Trim().Trim('"');
                if (t.Length == 0) continue;
                if (t == "--scan" || t == "-scan" || t == "/scan" || t == "-s" || t == "/s")
                {
                    _autoScan = true;
                    continue;
                }
                if (t.StartsWith("--sort="))
                {
                    int sortValue = -1;
                    if (int.TryParse(t.Substring(7), out sortValue) && sortValue >= 0 && sortValue <= 4)
                    {
                        _sortMode = sortValue;
                        if (cboSort != null)
                        {
                            _syncing = true;
                            cboSort.SetIndexSilent(sortValue);
                            _syncing = false;
                        }
                    }
                    continue;
                }
                if (t.StartsWith("-")) continue;
                paths.Add(t);
            }
            if (paths.Count > 0) _startupPaths = paths.ToArray();
        }

        private bool _dpiScaled;
        private TableLayoutPanel _contentGrid;
        private Panel _cardLeft;
        private Panel _pnlScan;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!_dpiScaled)
            {
                _dpiScaled = true;
                ApplyUiScaling();
            }
            ApplyLanguage(this);
            ApplyWindowPrefs();
            SyncColumnMenu();
            WriteDiag();

            // 仅自测用：FILEDEDUP_OPEN=assistant / report 可在扫描完成后直接打开对应对话框
            try
            {
                string openWhat = Environment.GetEnvironmentVariable("FILEDEDUP_OPEN");
                if (!string.IsNullOrEmpty(openWhat)) _pendingOpen = openWhat;
            }
            catch { }

            // 仅自测用：FILEDEDUP_OPEN=about / aboutsoft 启动后直接打开对应对话框
            if (_pendingOpen == "about" || _pendingOpen == "aboutsoft")
            {
                string what = _pendingOpen;
                _pendingOpen = "";
                BeginInvoke(new MethodInvoker(delegate
                {
                    if (what == "about") OnAbout(); else OnAboutSoftware();
                }));
            }

            // 仅自测用：FILEDEDUP_SWITCH=light/dark/simple 启动后自动切一次主题并回写诊断信息
            try
            {
                string sw = Environment.GetEnvironmentVariable("FILEDEDUP_SWITCH");
                if (!string.IsNullOrEmpty(sw))
                {
                    int target = Theme.Light;
                    if (sw == "dark") target = Theme.Dark;
                    else if (sw == "simple") target = Theme.Simple;
                    System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                    t.Interval = 900;
                    t.Tick += delegate
                    {
                        t.Stop();
                        t.Dispose();
                        SetTheme(target);
                        System.Windows.Forms.Timer t2 = new System.Windows.Forms.Timer();
                        t2.Interval = 900;
                        t2.Tick += delegate
                        {
                            t2.Stop();
                            t2.Dispose();
                            WriteDiag();
                        };
                        t2.Start();
                    };
                    t.Start();
                }
            }
            catch { }

            // 仅自测用：FILEDEDUP_RESETORDER=1 启动后自动执行一次“恢复默认列序”
            try
            {
                if (Environment.GetEnvironmentVariable("FILEDEDUP_RESETORDER") == "1")
                {
                    BeginInvoke(new MethodInvoker(delegate { ResetColumnOrder(); }));
                }
            }
            catch { }

            // 仅自测用：FILEDEDUP_PREVSCROLL=max 启动后把对比预览横向滚到最右端
            try
            {
                if (Environment.GetEnvironmentVariable("FILEDEDUP_PREVSCROLL") == "max")
                {
                    System.Windows.Forms.Timer t3 = new System.Windows.Forms.Timer();
                    t3.Interval = 4000;
                    t3.Tick += delegate
                    {
                        t3.Stop();
                        t3.Dispose();
                        NudgePreviewScroll(1000);
                        WriteDiag();
                    };
                    t3.Start();
                }
            }
            catch { }

            if (_startupPaths != null)
            {
                try
                {
                    AddPaths(_startupPaths);
                    if (_autoScan)
                    {
                        BeginInvoke(new MethodInvoker(delegate { OnScan(); }));
                    }
                }
                catch
                {
                }
            }
        }

        // 按真实 DPI + 字号偏好的整体缩放（重建界面后也要再走一次）
        private void ApplyUiScaling()
        {
            uint dpi = 96;
            try
            {
                if (this.Handle != IntPtr.Zero) dpi = GetDpiForWindow(this.Handle);
            }
            catch { }
            if (dpi == 0) dpi = 96;
            float dpiF = dpi / 96f;
            float fontF = (float)(Settings.FontPt / 9.0);
            if (fontF < 0.75f) fontF = 0.75f;
            if (fontF > 1.71f) fontF = 1.71f;
            float f = dpiF * fontF;

            // 仅调试/自测用：可用环境变量 FILEDEDUP_SCALE 强制指定整体缩放比例
            try
            {
                string force = Environment.GetEnvironmentVariable("FILEDEDUP_SCALE");
                if (!string.IsNullOrEmpty(force))
                {
                    float v = float.Parse(force, System.Globalization.CultureInfo.InvariantCulture);
                    if (v > 0.5f && v < 4f) f = v;
                }
            }
            catch { }

            // 字号调大后如果整窗超出屏幕可用区域，就按可用区域封顶，保证窗口始终能完整显示
            // （换成小分辨率屏幕时同样生效：宁可整体小一点，也不要有控件被挤到屏幕外）
            try
            {
                Rectangle wa0 = Screen.PrimaryScreen.WorkingArea;
                float fMax = Math.Min((wa0.Height - 70) / 940f, (wa0.Width - 24) / 1340f);
                if (fMax < 0.75f) fMax = 0.75f;
                if (f > fMax) f = fMax;
                if (f < 0.75f) f = 0.75f;
            }
            catch { }
            _diagDpi = string.Format("{0} / f={1:F2} fontF={2:F2}", dpi, f, fontF);

            this.MinimumSize = Size.Empty;
            if (f > 1.02f || f < 0.98f)
            {
                this.SuspendLayout();
                this.Scale(new SizeF(f, f));
                FixDesignStyles(f);
                this.ResumeLayout(true);
            }
            // 部分系统上 Scale() 会把窗体自身尺寸也带偏，这里按设计尺寸重新钉住客户区
            this.ClientSize = new Size((int)Math.Round(1340 * f), (int)Math.Round(940 * f));
            this.MinimumSize = new Size((int)(1160 * f), (int)(760 * f));
            _uiScale = f;

            try
            {
                Rectangle wa = Screen.FromControl(this).WorkingArea;
                if (this.Width > wa.Width) this.Width = wa.Width;
                if (this.Height > wa.Height) this.Height = wa.Height;
                this.Location = new Point(wa.Left + (wa.Width - this.Width) / 2, wa.Top + (wa.Height - this.Height) / 2);
            }
            catch
            {
            }
        }

        private static string _diagDpi = "n/a";

        private void WriteDiag()
        {
            try
            {
                string diag = string.Format("dpi={0}\r\nclient={1}\r\nbounds={2}\r\nstate={3} autosize={4}\r\nscreen={5}\r\nwork={6}\r\ncol0={7} contentW={8}\r\nmenu={9}\r\ncardLeft={10}\r\npnlScan={11}\r\nbtnScan={12}\r\nbtnStop={13}\r\nmainGrid={14} row2={15}\r\npreview={16}\r\nthumbs={17}\r\nexif={18}\r\ntile={19}\r\nneed={20}\r\n",
                    _diagDpi, this.ClientSize, this.Bounds,
                    this.WindowState, this.AutoSize,
                    Screen.FromControl(this).Bounds, Screen.FromControl(this).WorkingArea,
                    _contentGrid == null ? -1f : _contentGrid.ColumnStyles[0].Width,
                    _contentGrid == null ? 0 : _contentGrid.Width,                    menu == null ? "-" : menu.Bounds.ToString(),
                    _cardLeft == null ? "-" : _cardLeft.Bounds.ToString(),
                    _pnlScan == null ? "-" : _pnlScan.Bounds.ToString(),
                    btnScan == null ? "-" : btnScan.Bounds.ToString(),
                    btnStop == null ? "-" : btnStop.Bounds.ToString(),
                    _mainGrid == null ? "-" : _mainGrid.ClientSize.ToString(),
                    _mainGrid == null ? "-" : _mainGrid.RowStyles[2].Height.ToString(),
                    pnlPreview == null ? "-" : pnlPreview.ClientSize.ToString(),
                    pnlThumbs == null ? "-" : pnlThumbs.Bounds.ToString(),
                    dgvExif == null ? "-" : dgvExif.Bounds.ToString(),
                    _previewTileW.ToString() + "x" + _previewTileH.ToString() + " r" + _previewPerRow.ToString() + " rows" + _previewRows.ToString() + ( _previewCompact ? " compact" : ""),
                    PreviewNeedHeight(_previewGroup == null ? 0 : _previewGroup.Files.Count).ToString());
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "filededup_diag.txt"), diag, System.Text.Encoding.UTF8);
            }
            catch { }
        }

        // ============================================================
        //  界面
        // ============================================================
        private void BuildUi()
        {
            ApplyThemeVars();

            this.Text = "文件查重工具";
            this.AutoScaleMode = AutoScaleMode.None;
            this.BackColor = CPage;
            this.Font = new Font(UiFont, 9F);
            this.ClientSize = new Size(1340, 830);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AllowDrop = true;
            this.DoubleBuffered = true;

            // ---------- 菜单栏 ----------
            menu = new MenuStrip();
            menu.Dock = DockStyle.Top;
            menu.BackColor = CMenuBg;
            menu.ForeColor = CText;
            menu.Font = new Font(UiFont, 9.5F);
            menu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            menu.ShowItemToolTips = false;
            menu.AutoSize = false;
            menu.Height = 38;
            menu.Padding = new Padding(14, 5, 0, 5);
            menu.Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen pen = new Pen(CLine)) e.Graphics.DrawLine(pen, 0, menu.Height - 1, menu.Width, menu.Height - 1);
            };

            ToolStripMenuItem miFile = new ToolStripMenuItem("文件");
            ToolStripMenuItem miView = new ToolStripMenuItem("视图");
            ToolStripMenuItem miLang = new ToolStripMenuItem("语言");
            ToolStripMenuItem miAbout = new ToolStripMenuItem("关于");

            // 文件 → 编辑
            ToolStripMenuItem miEdit = new ToolStripMenuItem("编辑");
            miEdit.DropDownItems.Add(new ToolStripMenuItem("复制选中路径", null, delegate { CopySelectedPaths(); }));
            miEdit.DropDownItems.Add(new ToolStripMenuItem("全选勾选", null, delegate { SetAllChecked(true); }));
            miEdit.DropDownItems.Add(new ToolStripMenuItem("全不选", null, delegate { SetAllChecked(false); }));
            miEdit.DropDownItems.Add(new ToolStripMenuItem("选择助手…", null, delegate { ShowSelectionAssistant(); }));
            miEdit.DropDownItems.Add(new ToolStripMenuItem("清空扫描结果", null, delegate { ClearResults(); }));

            // 文件 → 字体大小
            ToolStripMenuItem miFontSize = new ToolStripMenuItem("字体大小");
            string[] sizeNames = new string[] { "较小", "标准" };
            double[] sizePts = new double[] { 8.0, 9.0 };
            for (int i = 0; i < sizeNames.Length; i++)
            {
                double target = sizePts[i];
                ToolStripMenuItem it = new ToolStripMenuItem(sizeNames[i], null, delegate { SetFontPt(target); });
                it.Checked = (Math.Abs(Settings.FontPt - target) < 0.05);
                miFontSize.DropDownItems.Add(it);
            }

            miFile.DropDownItems.Add(miEdit);
            miFile.DropDownItems.Add(miFontSize);
            miFile.DropDownItems.Add(new ToolStripMenuItem("字体设置…", null, delegate { OnFontSettings(); }));
            miFile.DropDownItems.Add(new ToolStripSeparator());
            miFile.DropDownItems.Add(new ToolStripMenuItem("扫描报告…", null, delegate { ShowScanReport(); }));
            miFile.DropDownItems.Add(new ToolStripMenuItem("导出清单 (CSV)…", null, delegate { ExportCsv(); }));
            miFile.DropDownItems.Add(new ToolStripSeparator());
            miFile.DropDownItems.Add(new ToolStripMenuItem("退出", null, delegate { this.Close(); }));

            // 视图 → 主题（简约模式 = 1.4 原界面，作为第三套皮肤保留）
            ToolStripMenuItem miSimple = new ToolStripMenuItem("简约模式", null, delegate { SetTheme(Theme.Simple); });
            miSimple.Checked = (Theme.Mode == Theme.Simple);
            ToolStripMenuItem miLight = new ToolStripMenuItem("白色主题", null, delegate { SetTheme(Theme.Light); });
            miLight.Checked = (Theme.Mode == Theme.Light);
            ToolStripMenuItem miDark = new ToolStripMenuItem("深色主题", null, delegate { SetTheme(Theme.Dark); });
            miDark.Checked = (Theme.Mode == Theme.Dark);
            ToolStripMenuItem miCustom = new ToolStripMenuItem("自定义主题…", null, delegate { OnCustomTheme(); });
            miCustom.Checked = (Theme.Mode == Theme.Custom);
            miView.DropDownItems.Add(miSimple);
            miView.DropDownItems.Add(miLight);
            miView.DropDownItems.Add(miDark);
            miView.DropDownItems.Add(miCustom);
            miView.DropDownItems.Add(new ToolStripSeparator());
            miView.DropDownItems.Add(new ToolStripMenuItem("恢复默认背景", null, delegate { ResetBackground(); }));
            miView.DropDownItems.Add(new ToolStripSeparator());

            // 视图 → 列显示（1.4 新增：自己决定表格里显示哪几列，选择会记住）
            ToolStripMenuItem miCols = new ToolStripMenuItem("列显示");
            string[] colNames = new string[] { "组", "文件名", "路径", "大小", "修改时间", "创建时间", "状态" };
            _miColToggles = new ToolStripMenuItem[colNames.Length];
            for (int i = 0; i < colNames.Length; i++)
            {
                int ci = i + 1;
                ToolStripMenuItem it = new ToolStripMenuItem(colNames[i]);
                it.CheckOnClick = true;
                it.Checked = true;
                ToolStripMenuItem cur = it;
                int curCi = ci;
                it.Click += delegate { ToggleColumn(curCi, cur.Checked); };
                _miColToggles[i] = it;
                miCols.DropDownItems.Add(it);
            }
            miView.DropDownItems.Add(miCols);
            miView.DropDownItems.Add(new ToolStripMenuItem("恢复默认列宽", null, delegate { ResetColumnWidths(); }));
            miView.DropDownItems.Add(new ToolStripMenuItem("恢复默认列序", null, delegate { ResetColumnOrder(); }));

            // 语言
            ToolStripMenuItem setCn = new ToolStripMenuItem("简体中文", null, delegate { SetLang(Lang.ZhCn); });
            setCn.Checked = (Lang.Code == Lang.ZhCn);
            ToolStripMenuItem setTw = new ToolStripMenuItem("繁體中文", null, delegate { SetLang(Lang.ZhTw); });
            setTw.Checked = (Lang.Code == Lang.ZhTw);
            ToolStripMenuItem setEn = new ToolStripMenuItem("English", null, delegate { SetLang(Lang.En); });
            setEn.Checked = (Lang.Code == Lang.En);
            ToolStripMenuItem setJa = new ToolStripMenuItem("日本語", null, delegate { SetLang(Lang.Ja); });
            setJa.Checked = (Lang.Code == Lang.Ja);
            ToolStripMenuItem setRu = new ToolStripMenuItem("Русский", null, delegate { SetLang(Lang.Ru); });
            setRu.Checked = (Lang.Code == Lang.Ru);
            ToolStripMenuItem setKo = new ToolStripMenuItem("한국어", null, delegate { SetLang(Lang.Ko); });
            setKo.Checked = (Lang.Code == Lang.Ko);
            miLang.DropDownItems.Add(setCn);
            miLang.DropDownItems.Add(setTw);
            miLang.DropDownItems.Add(setEn);
            miLang.DropDownItems.Add(setJa);
            miLang.DropDownItems.Add(new ToolStripSeparator());
            miLang.DropDownItems.Add(setRu);
            miLang.DropDownItems.Add(setKo);

            // 关于
            miAbout.DropDownItems.Add(new ToolStripMenuItem("版本说明", null, delegate { OnAbout(); }));
            miAbout.DropDownItems.Add(new ToolStripMenuItem("关于软件", null, delegate { OnAboutSoftware(); }));
            miAbout.DropDownItems.Add(new ToolStripMenuItem("检查更新", null, delegate { OnCheckUpdate(); }));
            miAbout.DropDownItems.Add(new ToolStripSeparator());
            miAbout.DropDownItems.Add(new ToolStripMenuItem("关于作者", null, delegate { OnAboutAuthor(); }));

            menu.Items.Add(miFile);
            menu.Items.Add(miView);
            menu.Items.Add(miLang);
            menu.Items.Add(miAbout);

            // ---------- 顶部标题 ----------
            Panel header;
            if (Theme.Banner)
            {
                // 简约模式：1.4 原来的蓝色渐变大横幅；白色主题：青灰渐变大横幅（左深右浅）
                header = new Panel();
                header.Dock = DockStyle.Fill;
                header.BackColor = CHdrTo;
                header.Paint += delegate(object s, PaintEventArgs e)
                {
                    Panel p = (Panel)s;
                    if (p.Width <= 0 || p.Height <= 0) return;
                    using (LinearGradientBrush br = new LinearGradientBrush(new Rectangle(0, 0, p.Width, p.Height),
                               CHdrFrom, CHdrTo, LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(br, 0, 0, p.Width, p.Height);
                    }
                };

                Label t1 = new Label();
                t1.Text = "文件查重工具";
                t1.AutoSize = true;
                t1.Font = new Font(UiFont, 18F, FontStyle.Bold);
                t1.ForeColor = COnAccent;
                t1.BackColor = Color.Transparent;
                t1.Location = new Point(28, 16);

                Label t2 = new Label();
                t2.Text = "按文件内容精确比对照片、视频、文档 —— 看清每条路径，按大小和时间排序，安全清理重复副本";
                t2.AutoSize = true;
                t2.Font = new Font(UiFont, 9.5F);
                t2.ForeColor = CSubOnHeader;
                t2.BackColor = Color.Transparent;
                t2.Location = new Point(30, 56);

                header.Controls.Add(t1);
                header.Controls.Add(t2);
            }
            else
            {
                // 白色/深色主题：细顶栏 + 标志 + 名称
                header = new Panel();
                header.Dock = DockStyle.Fill;
                header.BackColor = CCard;
                header.Paint += delegate(object s, PaintEventArgs e)
                {
                    Panel p = (Panel)s;
                    if (p.Width <= 0 || p.Height <= 0) return;
                    using (Pen pen = new Pen(CLine, 1))
                    {
                        e.Graphics.DrawLine(pen, 0, p.Height - 1, p.Width, p.Height - 1);
                    }
                };

                // 标志：圆角方块 + 两个叠放的白色小方块，示意“重复副本”
                Panel logo = new Panel();
                logo.BackColor = Color.Transparent;
                logo.Size = new Size(26, 26);
                logo.Location = new Point(24, 19);
                logo.Paint += delegate(object s, PaintEventArgs e)
                {
                    Panel p = (Panel)s;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                    using (GraphicsPath path = ProgressPanel.Round(r, 7))
                    {
                        using (SolidBrush br = new SolidBrush(CAccent)) e.Graphics.FillPath(br, path);
                    }
                    using (Pen pen = new Pen(COnAccent, 1.2f))
                    {
                        e.Graphics.DrawRectangle(pen, 6, 8, 9, 9);
                        e.Graphics.DrawRectangle(pen, 10, 5, 9, 9);
                    }
                };

                Label title = new Label();
                title.Text = "文件查重工具";
                title.AutoSize = true;
                title.Font = new Font(UiFont, 15F, FontStyle.Bold);
                title.ForeColor = CText;
                title.BackColor = Color.Transparent;
                title.Location = new Point(60, 20);

                header.Controls.Add(logo);
                header.Controls.Add(title);
            }

            // ---------- 底部状态 ----------
            Panel statusBar = new Panel();
            statusBar.Dock = DockStyle.Fill;
            statusBar.BackColor = CStatusBg;
            statusBar.Paint += delegate(object s, PaintEventArgs e)
            {
                Panel p = (Panel)s;
                using (Pen pen = new Pen(CLine)) e.Graphics.DrawLine(pen, 0, 0, p.Width, 0);
            };

            TableLayoutPanel statusGrid = new TableLayoutPanel();
            statusGrid.Dock = DockStyle.Fill;
            statusGrid.ColumnCount = 3;
            statusGrid.RowCount = 1;
            statusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            statusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250));
            statusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62));
            statusGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            statusGrid.BackColor = CStatusBg;

            lblStatus = new Label();
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.AutoEllipsis = true;
            lblStatus.ForeColor = CSubText;
            lblStatus.Font = new Font(UiFont, 9F);
            lblStatus.Margin = new Padding(24, 0, 10, 0);
            lblStatus.Text = "就绪：添加要扫描的文件夹，然后点“开始扫描”。";

            pb = new ProgressPanel();
            pb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            pb.Height = 14;
            pb.Margin = Theme.Banner ? new Padding(2, 18, 2, 18) : new Padding(2, 13, 2, 13);

            lblPercent = new Label();
            lblPercent.Dock = DockStyle.Fill;
            lblPercent.TextAlign = ContentAlignment.MiddleLeft;
            lblPercent.ForeColor = CSubText;
            lblPercent.Font = new Font(UiFont, 9F);
            lblPercent.Margin = new Padding(8, 0, 10, 0);

            statusGrid.Controls.Add(lblStatus, 0, 0);
            statusGrid.Controls.Add(pb, 1, 0);
            statusGrid.Controls.Add(lblPercent, 2, 0);
            statusBar.Controls.Add(statusGrid);

            // ---------- 左侧卡片 ----------
            Panel cardLeft = MakeCard();
            cardLeft.Dock = DockStyle.Fill;
            cardLeft.Margin = Theme.Banner ? new Padding(18, 16, 0, 16) : new Padding(22, 16, 0, 16);

            TableLayoutPanel leftGrid = new TableLayoutPanel();
            leftGrid.Dock = DockStyle.Fill;
            leftGrid.ColumnCount = 1;
            leftGrid.RowCount = 4;
            leftGrid.BackColor = CCard;
            leftGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // 范围
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 266));  // 类型
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 164));  // 设置
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));   // 按钮

            // 1) 扫描范围
            TableLayoutPanel pnlRange = new TableLayoutPanel();
            pnlRange.Dock = DockStyle.Fill;
            pnlRange.ColumnCount = 1;
            pnlRange.RowCount = 3;
            pnlRange.BackColor = CCard;
            pnlRange.Margin = new Padding(0);
            pnlRange.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlRange.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            pnlRange.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlRange.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));

            Label lblRange = new Label();
            lblRange.Dock = DockStyle.Fill;
            lblRange.TextAlign = ContentAlignment.MiddleLeft;
            lblRange.Text = "扫描范围（可把文件夹拖进来）";
            lblRange.Font = new Font(UiFont, 9.5F, FontStyle.Bold);
            lblRange.ForeColor = CText;
            lblRange.BackColor = CCard;
            lblRange.Margin = new Padding(0);

            lstFolders = new ListBox();
            lstFolders.Dock = DockStyle.Fill;
            lstFolders.BorderStyle = BorderStyle.FixedSingle;
            lstFolders.IntegralHeight = false;
            lstFolders.Font = new Font(UiFont, 9F);
            lstFolders.ForeColor = CText;
            lstFolders.BackColor = CInputBg;
            lstFolders.HorizontalScrollbar = true;
            lstFolders.Margin = new Padding(0, 4, 0, 6);

            Panel pnlRangeBtns = new Panel();
            pnlRangeBtns.Dock = DockStyle.Fill;
            pnlRangeBtns.BackColor = CCard;
            pnlRangeBtns.Margin = new Padding(0);

            btnAdd = MakeButton("添加文件夹", CAccentSoft, CAccent, 104, 32);
            btnAdd.Location = new Point(0, 6);
            btnAdd.Click += delegate { OnAddFolder(); };
            btnRemove = MakeButton("移除选中", CNeutral, CText, 96, 32);
            btnRemove.Location = new Point(112, 6);
            btnRemove.Click += delegate { OnRemoveFolder(); };
            btnClear = MakeButton("清空", CNeutral, CText, 72, 32);
            btnClear.Location = new Point(216, 6);
            btnClear.Click += delegate { OnClearFolderList(); };

            pnlRangeBtns.Controls.Add(btnAdd);
            pnlRangeBtns.Controls.Add(btnRemove);
            pnlRangeBtns.Controls.Add(btnClear);

            pnlRange.Controls.Add(lblRange, 0, 0);
            pnlRange.Controls.Add(lstFolders, 0, 1);
            pnlRange.Controls.Add(pnlRangeBtns, 0, 2);

            // 2) 文件类型
            Panel pnlTypes = new Panel();
            pnlTypes.Dock = DockStyle.Fill;
            pnlTypes.BackColor = CCard;
            pnlTypes.Margin = new Padding(0);

            Label lblType = new Label();
            lblType.Text = "文件类型";
            lblType.AutoSize = true;
            lblType.Font = new Font(UiFont, 9.5F, FontStyle.Bold);
            lblType.ForeColor = CText;
            lblType.BackColor = CCard;
            lblType.Location = new Point(0, 8);

            FlowLayoutPanel typeFlow = new FlowLayoutPanel();
            typeFlow.Location = new Point(0, 34);
            typeFlow.Size = new Size(340, 64);
            typeFlow.BackColor = CCard;
            typeFlow.WrapContents = true;
            typeFlow.Margin = new Padding(0);

            chkImages = MakeCheck("图片", 106, 28);
            chkImages.Checked = true;
            chkVideos = MakeCheck("视频", 106, 28);
            chkVideos.Checked = true;
            chkDocs = MakeCheck("文档", 106, 28);
            chkDocs.Checked = true;
            chkAudio = MakeCheck("音频", 106, 28);
            chkAudio.Checked = true;
            chkArchives = MakeCheck("压缩包", 106, 28);
            chkArchives.Checked = false;
            chkOther = MakeCheck("其他文件", 106, 28);
            chkOther.Checked = false;

            typeFlow.Controls.Add(chkImages);
            typeFlow.Controls.Add(chkVideos);
            typeFlow.Controls.Add(chkDocs);
            typeFlow.Controls.Add(chkAudio);
            typeFlow.Controls.Add(chkArchives);
            typeFlow.Controls.Add(chkOther);

            // 全文件扫描：不看扩展名，只用内容哈希比对（改名/改后缀也能认出来）
            chkAllFiles = MakeCheck("全文件扫描", 340, 26);
            chkAllFiles.Checked = Settings.ScanAll;
            chkAllFiles.Location = new Point(0, 98);
            chkAllFiles.CheckedChanged += delegate
            {
                Settings.ScanAll = chkAllFiles.Checked;
                UpdateTypeEnable();
            };

            // 相似图片（感知哈希）
            chkSimilar = MakeCheck("相似图片（感知哈希，找“画面像”的照片）", 340, 24);
            chkSimilar.Checked = Settings.ScanSimilar;
            chkSimilar.Location = new Point(0, 128);
            chkSimilar.CheckedChanged += delegate
            {
                Settings.ScanSimilar = chkSimilar.Checked;
                UpdateTypeEnable();
            };

            Label lblTh = new Label();
            lblTh.Text = "阈值";
            lblTh.AutoSize = false;
            lblTh.Location = new Point(0, 158);
            lblTh.Size = new Size(48, 26);
            lblTh.TextAlign = ContentAlignment.MiddleLeft;
            lblTh.ForeColor = CSubText;
            lblTh.BackColor = CCard;

            btnT1 = MakeButton("严格 ≤3", CNeutral, CText, 84, 26);
            btnT1.Location = new Point(50, 158);
            btnT1.Margin = new Padding(0);
            btnT2 = MakeButton("标准 ≤6", CNeutral, CText, 84, 26);
            btnT2.Location = new Point(140, 158);
            btnT2.Margin = new Padding(0);
            btnT3 = MakeButton("宽松 ≤12", CNeutral, CText, 84, 26);
            btnT3.Location = new Point(230, 158);
            btnT3.Margin = new Padding(0);
            btnT1.Click += delegate { SetSimilarThreshold(3); };
            btnT2.Click += delegate { SetSimilarThreshold(6); };
            btnT3.Click += delegate { SetSimilarThreshold(12); };

            // 阈值按钮按当前语言文字实际宽度排版（俄语等长文案不至于被截断）
            {
                Button[] thBtns = new Button[] { btnT1, btnT2, btnT3 };
                int[] w = new int[3];
                int sum = 0;
                for (int i = 0; i < 3; i++)
                {
                    Size sz = TextRenderer.MeasureText(thBtns[i].Text, thBtns[i].Font);
                    w[i] = Math.Max(84, sz.Width + 14);
                    sum += w[i];
                }
                int startX = 50, gap = 6, avail = 340;
                if (sum + gap * 2 > avail - startX)
                {
                    double k = (double)(avail - startX - gap * 2) / sum;
                    for (int i = 0; i < 3; i++) w[i] = Math.Max(56, (int)(w[i] * k));
                }
                int x = startX;
                for (int i = 0; i < 3; i++)
                {
                    thBtns[i].Width = w[i];
                    thBtns[i].Left = x;
                    x += w[i] + gap;
                }
            }

            Label lblSimHint = new Label();
            lblSimHint.Text = "缩放、旋转、重压缩、转格式后仍能认出";
            lblSimHint.AutoSize = false;
            lblSimHint.Location = new Point(0, 188);
            lblSimHint.Size = new Size(340, 18);
            lblSimHint.ForeColor = CSubText;
            lblSimHint.BackColor = CCard;
            lblSimHint.Font = new Font(UiFont, 8.5F);

            Label lblExt = new Label();
            lblExt.Text = "补充扩展名（可选，逗号分隔）";
            lblExt.AutoSize = true;
            lblExt.ForeColor = CSubText;
            lblExt.BackColor = CCard;
            lblExt.Location = new Point(0, 210);

            txtCustomExt = new TextBox();
            txtCustomExt.Location = new Point(0, 232);
            txtCustomExt.Width = 340;
            txtCustomExt.Height = 24;
            txtCustomExt.Font = new Font(UiFont, 9F);
            txtCustomExt.BorderStyle = BorderStyle.FixedSingle;
            txtCustomExt.ForeColor = CText;
            txtCustomExt.BackColor = CInputBg;

            pnlTypes.Controls.Add(lblType);
            pnlTypes.Controls.Add(typeFlow);
            pnlTypes.Controls.Add(chkAllFiles);
            pnlTypes.Controls.Add(chkSimilar);
            pnlTypes.Controls.Add(lblTh);
            pnlTypes.Controls.Add(btnT1);
            pnlTypes.Controls.Add(btnT2);
            pnlTypes.Controls.Add(btnT3);
            pnlTypes.Controls.Add(lblSimHint);
            pnlTypes.Controls.Add(lblExt);
            pnlTypes.Controls.Add(txtCustomExt);
            UpdateSimilarButtons();
            UpdateTypeEnable();

            // 3) 扫描设置
            Panel pnlOptions = new Panel();
            pnlOptions.Dock = DockStyle.Fill;
            pnlOptions.BackColor = CCard;
            pnlOptions.Margin = new Padding(0);

            Label lblOpt = new Label();
            lblOpt.Text = "扫描设置";
            lblOpt.AutoSize = true;
            lblOpt.Font = new Font(UiFont, 9.5F, FontStyle.Bold);
            lblOpt.ForeColor = CText;
            lblOpt.BackColor = CCard;
            lblOpt.Location = new Point(0, 4);

            chkSkipHidden = MakeCheck("跳过隐藏文件与系统目录", 300, 28);
            chkSkipHidden.Checked = true;
            chkSkipHidden.Location = new Point(0, 68);

            chkSkipZero = MakeCheck("忽略 0 字节空文件", 300, 28);
            chkSkipZero.Checked = true;
            chkSkipZero.Location = new Point(0, 96);

            Label lblMin = new Label();
            lblMin.Text = "最小文件大小";
            lblMin.AutoSize = true;
            lblMin.ForeColor = CSubText;
            lblMin.BackColor = CCard;
            lblMin.Location = new Point(0, 130);

            numMin = new NumericUpDown();
            numMin.DecimalPlaces = 3;      // 单位改为 MB，保留 3 位小数（0.001 MB ≈ 原 1 KB 的精度）
            numMin.Minimum = 0;
            numMin.Maximum = 1024;         // 1024 MB = 1 GB，与旧版 KB 上限（1,048,576 KB）等价
            numMin.Value = 0.001m;         // 默认 0.001 MB ≈ 1 KB，行为与旧版默认一致
            numMin.Increment = 0.1m;
            numMin.Width = 76;
            numMin.Location = new Point(94, 126);
            numMin.Font = new Font(UiFont, 9F);
            numMin.ForeColor = CText;
            numMin.BackColor = CInputBg;

            Label lblKb = new Label();
            lblKb.Text = "MB（0 = 不限）";
            lblKb.AutoSize = true;
            lblKb.ForeColor = CSubText;
            lblKb.BackColor = CCard;
            lblKb.Location = new Point(178, 130);

            // 扫描层级（只扫当前目录 / 含 N 层子目录 / 全部子目录）
            Label lblDepth = new Label();
            lblDepth.Text = "扫描层级";
            lblDepth.AutoSize = true;
            lblDepth.ForeColor = CText;
            lblDepth.BackColor = CCard;
            lblDepth.Location = new Point(0, 41);

            cboDepth = new ComboButton();
            cboDepth.SetItems("仅当前目录（不含子目录）", "含 1 层子目录", "含 2 层子目录", "含 3 层子目录", "所有子目录（不限层级）");
            cboDepth.Location = new Point(100, 34);
            cboDepth.Size = new Size(236, 30);
            cboDepth.SetIndexSilent(Settings.ScanDepth);
            cboDepth.IndexChanged += delegate { Settings.ScanDepth = cboDepth.Index; };

            pnlOptions.Controls.Add(lblOpt);
            pnlOptions.Controls.Add(lblDepth);
            pnlOptions.Controls.Add(cboDepth);
            pnlOptions.Controls.Add(chkSkipHidden);
            pnlOptions.Controls.Add(chkSkipZero);
            pnlOptions.Controls.Add(lblMin);
            pnlOptions.Controls.Add(numMin);
            pnlOptions.Controls.Add(lblKb);

            // 4) 开始扫描 / 暂停 / 取消扫描
            TableLayoutPanel pnlScan = new TableLayoutPanel();
            pnlScan.Dock = DockStyle.Fill;
            pnlScan.ColumnCount = 2;
            pnlScan.RowCount = 2;
            pnlScan.BackColor = CCard;
            pnlScan.Margin = new Padding(0, 6, 0, 0);
            pnlScan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlScan.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            pnlScan.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            pnlScan.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            btnScan = MakeButton("开始扫描", CAccent, COnAccent, 200, 44);
            btnScan.Font = new Font(UiFont, 10.5F, FontStyle.Bold);
            btnScan.Dock = DockStyle.Fill;
            btnScan.Margin = new Padding(0, 0, 0, 8);
            btnScan.Click += delegate { OnScan(); };

            btnPause = MakeButton("暂停", CNeutral, CText, 96, 38);
            btnPause.Dock = DockStyle.Fill;
            btnPause.Margin = new Padding(0, 0, 6, 0);
            btnPause.Enabled = false;
            btnPause.Click += delegate { OnPause(); };

            btnStop = MakeButton("取消扫描", CDangerSoft, CDanger, 96, 38);
            btnStop.Dock = DockStyle.Fill;
            btnStop.Margin = new Padding(6, 0, 0, 0);
            btnStop.Enabled = false;
            btnStop.Click += delegate { OnStop(); };

            pnlScan.Controls.Add(btnScan, 0, 0);
            pnlScan.SetColumnSpan(btnScan, 2);
            pnlScan.Controls.Add(btnPause, 0, 1);
            pnlScan.Controls.Add(btnStop, 1, 1);

            leftGrid.Controls.Add(pnlRange, 0, 0);
            leftGrid.Controls.Add(pnlTypes, 0, 1);
            leftGrid.Controls.Add(pnlOptions, 0, 2);
            leftGrid.Controls.Add(pnlScan, 0, 3);
            cardLeft.Controls.Add(leftGrid);

            // ---------- 右侧卡片 ----------
            Panel cardMain = MakeCard();
            cardMain.Dock = DockStyle.Fill;
            cardMain.Margin = Theme.Banner ? new Padding(18, 16, 18, 16) : new Padding(18, 16, 22, 16);

            TableLayoutPanel mainGrid = new TableLayoutPanel();
            mainGrid.Dock = DockStyle.Fill;
            mainGrid.ColumnCount = 1;
            mainGrid.RowCount = 5;
            mainGrid.BackColor = CCard;
            mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 272));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            mainGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            _mainGrid = mainGrid;

            // 顶部信息 + 排序
            TableLayoutPanel pnlTop = new TableLayoutPanel();
            pnlTop.Dock = DockStyle.Fill;
            pnlTop.ColumnCount = 5;
            pnlTop.RowCount = 1;
            pnlTop.BackColor = CCard;
            pnlTop.Margin = new Padding(0);
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196));
            pnlTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            lblSummary = new Label();
            lblSummary.Dock = DockStyle.Top;
            lblSummary.Height = 34;
            lblSummary.TextAlign = ContentAlignment.MiddleLeft;
            lblSummary.ForeColor = CText;
            lblSummary.BackColor = CCard;
            lblSummary.Font = new Font(UiFont, 9.5F, FontStyle.Bold);
            lblSummary.AutoEllipsis = true;
            lblSummary.Margin = new Padding(0, 12, 10, 0);
            lblSummary.Text = "暂无扫描结果";

            btnSelectAll = MakeButton("全选", CNeutral, CText, 62, 32);
            GrowButton(btnSelectAll, 62, 32);
            btnSelectAll.Margin = new Padding(0, 12, 6, 12);
            btnSelectAll.Click += delegate { SetAllChecked(true); };

            btnSelectNone = MakeButton("全不选", CNeutral, CText, 74, 32);
            GrowButton(btnSelectNone, 74, 32);
            btnSelectNone.Margin = new Padding(0, 12, 14, 12);
            btnSelectNone.Click += delegate { SetAllChecked(false); };

            Label lblSort = new Label();
            lblSort.Text = "";
            lblSort.AutoSize = false;
            lblSort.Dock = DockStyle.Fill;
            lblSort.Width = 0;
            lblSort.TextAlign = ContentAlignment.MiddleLeft;
            lblSort.ForeColor = CSubText;
            lblSort.BackColor = CCard;
            lblSort.Margin = new Padding(0);
            lblSort.Visible = false;

            cboSort = new ComboButton();
            cboSort.Font = new Font(UiFont, 9F);
            cboSort.ForeColor = CText;
            cboSort.BackColor = CNeutral;
            cboSort.Dock = DockStyle.Fill;
            cboSort.Margin = new Padding(0, 13, 0, 13);
            cboSort.SetItems("按大小 ↓（大→小）", "按大小 ↑（小→大）", "按时间 ↓（新→旧）", "按时间 ↑（旧→新）", "按路径 A→Z");
            cboSort.Index = _sortMode;
            cboSort.IndexChanged += delegate
            {
                if (!_ready || _syncing) return;
                _sortMode = cboSort.Index;
                Settings.Sort = _sortMode;
                Settings.Save();
                if (_busy) return;
                RebuildGrid();
            };

            cboView = new ComboButton();
            cboView.Font = new Font(UiFont, 9F);
            cboView.ForeColor = CText;
            cboView.BackColor = CNeutral;
            cboView.Width = 132;
            cboView.Height = 32;
            cboView.Margin = new Padding(0, 12, 12, 14);
            cboView.SetItems("全部结果", "仅精确重复", "仅相似图片");
            cboView.Index = _viewFilter;
            cboView.IndexChanged += delegate
            {
                if (!_ready || _syncing) return;
                _viewFilter = cboView.Index;
                if (_busy) return;
                RebuildGrid();
            };

            pnlTop.Controls.Add(lblSummary, 0, 0);
            pnlTop.Controls.Add(cboView, 1, 0);
            pnlTop.Controls.Add(btnSelectAll, 2, 0);
            pnlTop.Controls.Add(btnSelectNone, 3, 0);
            pnlTop.Controls.Add(cboSort, 4, 0);

            // 表格
            dgv = new DataGridView();
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = CCard;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = CGridLine;
            dgv.RowHeadersVisible = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToOrderColumns = true;   // 左键按住列头左右拖动即可换列的位置（顺序会记住）
            dgv.MultiSelect = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.EditMode = DataGridViewEditMode.EditOnEnter;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 40;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 32;
            dgv.Font = new Font(UiFont, 9F);
            dgv.DefaultCellStyle.Font = new Font(UiFont, 9F);
            dgv.DefaultCellStyle.BackColor = CCard;
            dgv.DefaultCellStyle.ForeColor = CText;
            dgv.DefaultCellStyle.SelectionBackColor = CGridSel;
            dgv.DefaultCellStyle.SelectionForeColor = CText;
            dgv.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = CGridHeader;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = CText;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font(UiFont, 9F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = CGridHeader;
            dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = CText;
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.StandardTab = true;
            dgv.Margin = new Padding(0);

            DataGridViewCheckBoxColumn colDel = new DataGridViewCheckBoxColumn();
            colDel.HeaderText = "删除";
            colDel.Width = 66;
            colDel.ReadOnly = false;
            colDel.SortMode = DataGridViewColumnSortMode.NotSortable;
            colDel.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colDel.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn colGrp = new DataGridViewTextBoxColumn();
            colGrp.HeaderText = "组";
            colGrp.Width = 64;
            colGrp.ReadOnly = true;
            colGrp.SortMode = DataGridViewColumnSortMode.NotSortable;
            colGrp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewTextBoxColumn colName = new DataGridViewTextBoxColumn();
            colName.HeaderText = "文件名";
            colName.Width = 160;
            colName.ReadOnly = true;
            colName.SortMode = DataGridViewColumnSortMode.NotSortable;

            DataGridViewTextBoxColumn colPath = new DataGridViewTextBoxColumn();
            colPath.HeaderText = "完整路径（双击行可打开文件）";
            colPath.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPath.FillWeight = 100;
            colPath.MinimumWidth = 100;
            colPath.ReadOnly = true;
            colPath.SortMode = DataGridViewColumnSortMode.NotSortable;

            DataGridViewTextBoxColumn colSize = new DataGridViewTextBoxColumn();
            colSize.HeaderText = "大小";
            colSize.Width = 88;
            colSize.ReadOnly = true;
            colSize.SortMode = DataGridViewColumnSortMode.NotSortable;
            colSize.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.HeaderText = "修改时间";
            colTime.Width = 140;
            colTime.ReadOnly = true;
            colTime.SortMode = DataGridViewColumnSortMode.NotSortable;

            // 1.4：创建时间列（可与修改时间对照，判断是“拷贝副本”还是“另一个时间点的原件”）
            DataGridViewTextBoxColumn colCreated = new DataGridViewTextBoxColumn();
            colCreated.HeaderText = "创建时间";
            colCreated.Width = 140;
            colCreated.ReadOnly = true;
            colCreated.SortMode = DataGridViewColumnSortMode.NotSortable;

            DataGridViewTextBoxColumn colNote = new DataGridViewTextBoxColumn();
            colNote.HeaderText = "状态";
            colNote.Width = 118;
            colNote.ReadOnly = true;
            colNote.SortMode = DataGridViewColumnSortMode.NotSortable;

            dgv.Columns.Add(colDel);
            dgv.Columns.Add(colGrp);
            dgv.Columns.Add(colName);
            dgv.Columns.Add(colPath);
            dgv.Columns.Add(colSize);
            dgv.Columns.Add(colTime);
            dgv.Columns.Add(colCreated);
            dgv.Columns.Add(colNote);

            // 默认列序：删除 / 组 / 文件名 / 大小 / 修改时间 / 状态 / 创建时间 / 完整路径
            // 一眼能看全“哪份要删、多大、什么时候改的”，路径与创建时间放右边，需要时往右拖一下就看得到
            ApplyDefaultColumnOrder();

            // 恢复上次的列宽与“创建时间”显隐
            ApplyColumnPrefs();

            // 拖列头换过位置就记住（换主题/换语言重建界面后会按记下的顺序还原）
            dgv.ColumnDisplayIndexChanged += delegate
            {
                if (_applyingColOrder) return;
                SaveColumnOrder();
                Settings.Save();
            };

            // 白色（青灰）主题：表格里的“删除”复选框也画成深青方块 + 白勾
            if (Theme.Mode == Theme.Light)
            {
                dgv.CellPainting += delegate(object s, DataGridViewCellPaintingEventArgs ev)
                {
                    if (ev.RowIndex < 0 || ev.ColumnIndex != 0 || ev.Graphics == null) return;
                    ev.PaintBackground(ev.CellBounds, true);
                    bool val = false;
                    object o = dgv.Rows[ev.RowIndex].Cells[0].Value;
                    if (o is bool) val = (bool)o;
                    int box = (int)(ev.CellBounds.Height * 0.5f);
                    if (box < 13) box = 13;
                    if (box > 20) box = 20;
                    int x = ev.CellBounds.X + (ev.CellBounds.Width - box) / 2;
                    int y = ev.CellBounds.Y + (ev.CellBounds.Height - box) / 2;
                    ev.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle r = new Rectangle(x, y, box, box);
                    Color fill = val ? CAccent : CCard;
                    Color border = val ? CAccent : CFieldLine;
                    using (GraphicsPath path = ProgressPanel.Round(r, 3))
                    {
                        using (SolidBrush br = new SolidBrush(fill)) ev.Graphics.FillPath(br, path);
                        using (Pen pen = new Pen(border, 1)) ev.Graphics.DrawPath(pen, path);
                    }
                    if (val)
                    {
                        using (Pen pen = new Pen(COnAccent, 1.6f))
                        {
                            ev.Graphics.DrawLine(pen, x + box * 0.27f, y + box * 0.52f, x + box * 0.45f, y + box * 0.71f);
                            ev.Graphics.DrawLine(pen, x + box * 0.45f, y + box * 0.71f, x + box * 0.75f, y + box * 0.31f);
                        }
                    }
                    using (Pen line = new Pen(CGridLine, 1))
                    {
                        ev.Graphics.DrawLine(line, ev.CellBounds.X, ev.CellBounds.Bottom - 1,
                            ev.CellBounds.Right, ev.CellBounds.Bottom - 1);
                    }
                    ev.Handled = true;
                };
            }

            dgv.CurrentCellDirtyStateChanged += delegate
            {
                if (dgv.IsCurrentCellDirty && dgv.CurrentCell is DataGridViewCheckBoxCell)
                {
                    dgv.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            dgv.CellValueChanged += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                FileItem f = row.Tag as FileItem;
                if (f == null) return;
                object v = row.Cells[0].Value;
                bool chk = (v is bool) && (bool)v;
                if (chk)
                {
                    row.Cells[CColNote].Value = Lang.T("待删除");
                    row.Cells[CColNote].Style.ForeColor = CDanger;
                }
                else
                {
                    row.Cells[CColNote].Value = Lang.T(f.Note);
                    row.Cells[CColNote].Style.ForeColor = f.Note == "保留这份" ? CGood : CSubText;
                }
                UpdateSummary();
            };
            dgv.ColumnHeaderMouseClick += delegate(object sender, DataGridViewCellMouseEventArgs e)
            {
                if (e.ColumnIndex == 4)
                {
                    _sortMode = (_sortMode == 0) ? 1 : 0;
                }
                else if (e.ColumnIndex == 5)
                {
                    _sortMode = (_sortMode == 2) ? 3 : 2;
                }
                else if (e.ColumnIndex == 3)
                {
                    _sortMode = 4;
                }
                else
                {
                    return;
                }
                SyncSortCombo();
                RebuildGrid();
            };
            dgv.CellDoubleClick += delegate(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0 || e.ColumnIndex == 0) return;
                FileItem f = dgv.Rows[e.RowIndex].Tag as FileItem;
                OpenPath(f);
            };

            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Font = new Font(UiFont, 9.5F);
            cms.BackColor = CCard;
            cms.ForeColor = CText;
            cms.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
            cms.Items.Add(new ToolStripMenuItem("打开文件", null, delegate { OpenSelectedFiles(); }));
            cms.Items.Add(new ToolStripMenuItem("打开所在文件夹", null, delegate { OpenSelectedFolder(); }));
            cms.Items.Add(new ToolStripMenuItem("复制完整路径", null, delegate { CopySelectedPaths(); }));
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(new ToolStripMenuItem("只保留这一份（同组其余勾选删除）", null, delegate { KeepOnlyCurrent(); }));
            cms.Items.Add(new ToolStripMenuItem("取消本组全部勾选", null, delegate { UncheckGroupOfCurrent(); }));
            cms.Items.Add(new ToolStripSeparator());
            cms.Items.Add(new ToolStripMenuItem("选择助手…", null, delegate { ShowSelectionAssistant(); }));
            // 深色主题下菜单项的默认前景色是黑的，必须显式跟随主题
            for (int i = 0; i < cms.Items.Count; i++)
            {
                ToolStripItem it = cms.Items[i];
                it.ForeColor = CText;
                it.BackColor = CCard;
            }
            dgv.ContextMenuStrip = cms;

            // 选中行的完整路径提示条
            lblPathDetail = new Label();
            lblPathDetail.Dock = DockStyle.Fill;
            lblPathDetail.TextAlign = ContentAlignment.MiddleLeft;
            lblPathDetail.ForeColor = CSubText;
            lblPathDetail.BackColor = CCard;
            lblPathDetail.Font = new Font(UiFont, 8.5F);
            lblPathDetail.Padding = new Padding(8, 0, 8, 0);
            lblPathDetail.Margin = new Padding(0, 4, 0, 8);
            lblPathDetail.AutoEllipsis = true;
            lblPathDetail.Cursor = Cursors.Hand;
            lblPathDetail.Text = "选中某一行，这里会显示该文件的完整路径（点击可复制）";
            lblPathDetail.Click += delegate { CopySelectedPaths(); };
            dgv.SelectionChanged += delegate
            {
                UpdatePathDetail();
                LoadPreview();
            };

            // 预览与参数对照（选中一组后显示缩略图 + 分辨率 / EXIF 对照）
            pnlPreview = new Panel();
            pnlPreview.Dock = DockStyle.Fill;
            pnlPreview.BackColor = CCard;
            pnlPreview.Margin = new Padding(0, 2, 0, 0);

            lblPreviewTitle = new Label();
            lblPreviewTitle.Text = "对比预览";
            lblPreviewTitle.AutoSize = true;
            lblPreviewTitle.Font = new Font(UiFont, 9.5F, FontStyle.Bold);
            lblPreviewTitle.ForeColor = CText;
            lblPreviewTitle.BackColor = CCard;
            lblPreviewTitle.Location = new Point(2, 8);

            lblPreviewHint = new Label();
            lblPreviewHint.Text = "选中一组，这里并排显示缩略图和拍摄参数";
            lblPreviewHint.AutoSize = true;
            lblPreviewHint.Font = new Font(UiFont, 8.5F);
            lblPreviewHint.ForeColor = CSubText;
            lblPreviewHint.BackColor = CCard;
            lblPreviewHint.Location = new Point(86, 12);

            btnPreviewToggle = MakeButton("收起", CNeutral, CText, 64, 22, true);
            btnPreviewToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPreviewToggle.Location = new Point(0, 6);
            btnPreviewToggle.Margin = new Padding(0);

            pnlThumbs = new Panel();
            pnlThumbs.Location = new Point(2, 36);
            pnlThumbs.Size = new Size(420, 168);
            pnlThumbs.BackColor = CCard;
            pnlThumbs.AutoScroll = true;

            lblPreviewEmpty = new Label();
            lblPreviewEmpty.Text = "在表格里选中一行，即可在这里预览并对照参数";
            lblPreviewEmpty.AutoSize = false;
            lblPreviewEmpty.Size = new Size(400, 40);
            lblPreviewEmpty.Location = new Point(8, 60);
            lblPreviewEmpty.ForeColor = CSubText;
            lblPreviewEmpty.BackColor = CCard;
            lblPreviewEmpty.Font = new Font(UiFont, 9F);

            pnlThumbs.Controls.Add(lblPreviewEmpty);

            dgvExif = new DataGridView();
            dgvExif.Location = new Point(436, 36);
            dgvExif.Size = new Size(420, 168);
            dgvExif.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            dgvExif.BackgroundColor = CCard;
            dgvExif.BorderStyle = BorderStyle.None;
            dgvExif.ScrollBars = ScrollBars.Vertical;
            dgvExif.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvExif.GridColor = CGridLine;
            dgvExif.RowHeadersVisible = false;
            dgvExif.AllowUserToAddRows = false;
            dgvExif.AllowUserToDeleteRows = false;
            dgvExif.AllowUserToResizeRows = false;
            dgvExif.AllowUserToResizeColumns = false;
            dgvExif.ReadOnly = true;
            dgvExif.MultiSelect = false;
            dgvExif.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvExif.EnableHeadersVisualStyles = false;
            dgvExif.ColumnHeadersHeight = 28;
            dgvExif.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvExif.RowTemplate.Height = 24;
            dgvExif.Font = new Font(UiFont, 8.5F);
            dgvExif.DefaultCellStyle.Font = new Font(UiFont, 8.5F);
            dgvExif.DefaultCellStyle.BackColor = CCard;
            dgvExif.DefaultCellStyle.ForeColor = CText;
            dgvExif.DefaultCellStyle.SelectionBackColor = CGridSel;
            dgvExif.DefaultCellStyle.SelectionForeColor = CText;
            dgvExif.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            dgvExif.ColumnHeadersDefaultCellStyle.BackColor = CGridHeader;
            dgvExif.ColumnHeadersDefaultCellStyle.ForeColor = CText;
            dgvExif.ColumnHeadersDefaultCellStyle.Font = new Font(UiFont, 8.5F, FontStyle.Bold);
            dgvExif.ColumnHeadersDefaultCellStyle.SelectionBackColor = CGridHeader;
            dgvExif.ColumnHeadersDefaultCellStyle.SelectionForeColor = CText;
            dgvExif.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            btnPreviewToggle.Click += delegate { TogglePreview(); };

            // 预览整行的横向滚动条：系统原生 HScrollBar，样式与主表格那根完全一致
            sbPreview = new HScrollBar();
            sbPreview.Minimum = 0;
            sbPreview.Maximum = 0;
            sbPreview.LargeChange = 1;
            sbPreview.SmallChange = 1;
            sbPreview.TabStop = false;
            sbPreview.Visible = false;
            sbPreview.ValueChanged += delegate
            {
                if (_syncingPreview) return;
                ApplyPreviewScroll();
            };

            pnlPreview.Controls.Add(lblPreviewTitle);
            pnlPreview.Controls.Add(lblPreviewHint);
            pnlPreview.Controls.Add(btnPreviewToggle);
            pnlPreview.Controls.Add(pnlThumbs);
            pnlPreview.Controls.Add(dgvExif);
            pnlPreview.Controls.Add(sbPreview);
            pnlPreview.Resize += delegate
            {
                if (_layingPreview) return;
                LayoutPreview();
                if (_previewTimer == null)
                {
                    _previewTimer = new System.Windows.Forms.Timer();
                    _previewTimer.Interval = 220;
                    _previewTimer.Tick += delegate
                    {
                        _previewTimer.Stop();
                        if (NeedRetile()) LoadPreview();
                        else { LayoutPreview(); ApplyPreviewScroll(); }
                    };
                }
                _previewTimer.Stop();
                _previewTimer.Start();
            };

            // 底部操作条
            FlowLayoutPanel pnlActions = new FlowLayoutPanel();
            pnlActions.Dock = DockStyle.Fill;
            pnlActions.WrapContents = false;
            pnlActions.FlowDirection = FlowDirection.LeftToRight;
            pnlActions.BackColor = CCard;
            pnlActions.Padding = new Padding(0, 0, 0, 0);
            pnlActions.Margin = new Padding(0);

            Label lblKeep = new Label();
            lblKeep.Text = "每组保留";
            lblKeep.AutoSize = true;
            lblKeep.Height = 38;
            lblKeep.TextAlign = ContentAlignment.MiddleLeft;
            lblKeep.ForeColor = CSubText;
            lblKeep.BackColor = CCard;
            lblKeep.Margin = new Padding(0, 14, 10, 0);

            cboKeepRule = new ComboButton();
            cboKeepRule.Font = new Font(UiFont, 9F);
            cboKeepRule.ForeColor = CText;
            cboKeepRule.BackColor = CNeutral;
            cboKeepRule.Width = 250;
            cboKeepRule.Height = 36;
            cboKeepRule.Margin = new Padding(0, 15, 14, 0);
            cboKeepRule.SetItems(KeepRuleNames);
            cboKeepRule.Index = _keepRule;
            cboKeepRule.IndexChanged += delegate
            {
                if (!_ready || _syncing) return;
                _keepRule = cboKeepRule.Index;
                Settings.KeepRule = _keepRule;
                Settings.Save();
                if (_busy) return;
                if (_groups.Count > 0)
                {
                    ApplyRuleAndRebuild();
                }
            };

            // 同一个规则再点一次也重新勾选（“全不选”之后靠这个回来）
            cboKeepRule.Picked += delegate
            {
                if (!_ready || _syncing || _busy) return;
                if (_groups.Count == 0) return;
                ApplyRuleAndRebuild();

                int cnt = 0;
                long bytes = 0;
                for (int i = 0; i < _groups.Count; i++)
                {
                    DupGroup g = _groups[i];
                    for (int j = 0; j < g.Files.Count; j++)
                    {
                        if (g.Files[j].SelectedForDelete) { cnt++; bytes += g.Files[j].Length; }
                    }
                }
                lblStatus.Text = Lang.F("已按“{0}”重新勾选：勾选 {1} 项，可释放 {2}",
                    Lang.T(cboKeepRule.Index >= 0 ? KeepRuleNames[cboKeepRule.Index] : ""),
                    Fmt.Number(cnt), Fmt.Size(bytes));
            };

            btnOpenFolder = MakeButton("打开位置", CNeutral, CText, 96, 36);
            GrowButton(btnOpenFolder, 96, 36);
            btnOpenFolder.Margin = new Padding(0, 13, 7, 0);
            btnOpenFolder.Click += delegate { OpenSelectedFolder(); };

            btnExport = MakeButton("导出清单", CNeutral, CText, 92, 36);
            GrowButton(btnExport, 92, 36);
            btnExport.Margin = new Padding(0, 13, 7, 0);
            btnExport.Click += delegate { ExportCsv(); };

            btnRecycle = MakeButton("删除到回收站", CDanger, COnAccent, 116, 38);
            GrowButton(btnRecycle, 116, 38);
            btnRecycle.Margin = new Padding(0, 12, 7, 0);
            btnRecycle.Click += delegate { DeleteSelected(false); };

            btnPermanent = MakeButton("永久删除", CDangerSoft, CDanger, 96, 38);
            GrowButton(btnPermanent, 96, 38);
            btnPermanent.Margin = new Padding(0, 12, 0, 0);
            btnPermanent.MinimumSize = new Size(86, 38);
            btnPermanent.Click += delegate { DeleteSelected(true); };

            pnlActions.Controls.Add(lblKeep);
            pnlActions.Controls.Add(cboKeepRule);
            pnlActions.Controls.Add(btnOpenFolder);
            pnlActions.Controls.Add(btnExport);
            pnlActions.Controls.Add(btnRecycle);
            pnlActions.Controls.Add(btnPermanent);

            mainGrid.Controls.Add(pnlTop, 0, 0);
            if (Theme.Mode == Theme.Light)
            {
                // 白色（青灰）主题：表格外面套一个圆角描边容器（参考图里表格是圆角细边）
                Panel tableBox = new Panel();
                tableBox.Dock = DockStyle.Fill;
                tableBox.Margin = new Padding(0);
                tableBox.BackColor = CCard;
                tableBox.Padding = new Padding(1);
                tableBox.Paint += delegate(object s, PaintEventArgs e)
                {
                    Panel p = (Panel)s;
                    if (p.Width < 3 || p.Height < 3) return;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (GraphicsPath path = ProgressPanel.Round(new Rectangle(0, 0, p.Width - 1, p.Height - 1), 8))
                    using (Pen pen = new Pen(CLine, 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                };
                Panel box = tableBox;
                EventHandler clip = delegate
                {
                    if (box.Width < 4 || box.Height < 4) return;
                    using (GraphicsPath path = ProgressPanel.Round(new Rectangle(0, 0, box.Width, box.Height), 8))
                    {
                        box.Region = new Region(path);
                    }
                };
                tableBox.Resize += clip;
                dgv.Dock = DockStyle.Fill;
                dgv.Margin = new Padding(0);
                tableBox.Controls.Add(dgv);
                mainGrid.Controls.Add(tableBox, 0, 1);
            }
            else
            {
                mainGrid.Controls.Add(dgv, 0, 1);
            }
            mainGrid.Controls.Add(pnlPreview, 0, 2);
            mainGrid.Controls.Add(lblPathDetail, 0, 3);
            mainGrid.Controls.Add(pnlActions, 0, 4);
            cardMain.Controls.Add(mainGrid);

            // ---------- 组装 ----------
            TableLayoutPanel content = new TableLayoutPanel();
            content.Dock = DockStyle.Fill;
            content.ColumnCount = 2;
            content.RowCount = 1;
            content.BackColor = CPage;
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Theme.Banner ? 382 : 398));
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.Controls.Add(cardLeft, 0, 0);
            content.Controls.Add(cardMain, 1, 0);
            _contentGrid = content;
            _cardLeft = cardLeft;
            _pnlScan = pnlScan;

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.ColumnCount = 1;
            root.RowCount = 3;
            root.BackColor = CPage;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Banner ? 96 : 64));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, Theme.Banner ? 50 : 40));
            root.Controls.Add(header, 0, 0);
            root.Controls.Add(content, 0, 1);
            root.Controls.Add(statusBar, 0, 2);
            _rootGrid = root;

            this.Controls.Add(root);
            this.Controls.Add(menu);

            // 自定义背景：图片铺满 + 页面透明
            if (Theme.Mode == Theme.Custom)
            {
                if (Theme.CustomImage.Length > 0 && File.Exists(Theme.CustomImage))
                {
                    try
                    {
                        using (Image img = Image.FromFile(Theme.CustomImage))
                        {
                            this.BackgroundImage = new Bitmap(img);
                        }
                        this.BackgroundImageLayout = ImageLayout.Stretch;
                        root.BackColor = Color.Transparent;
                        content.BackColor = Color.Transparent;
                        content.Padding = new Padding(22, 16, 22, 12);
                    }
                    catch
                    {
                    }
                }
            }

            this.DragEnter += delegate(object sender, DragEventArgs e)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
                else e.Effect = DragDropEffects.None;
            };
            this.DragDrop += delegate(object sender, DragEventArgs e)
            {
                string[] items = e.Data.GetData(DataFormats.FileDrop) as string[];
                AddPaths(items);
            };

            _ready = true;
            CaptureDesignStyles(this);
            ApplyTitleBarTheme();
            ApplyGlassIfNeeded();
            SetUiBusy(false);
        }

        // 主题色写入 C* 字段（每次重建界面都会重新取值）
        private void ApplyThemeVars()
        {
            Theme.Refresh();
            CPage = Theme.Page;
            CCard = Theme.Card;
            CAccent = Theme.Accent;
            CAccentSoft = Theme.AccentSoft;
            CNeutral = Theme.Neutral;
            CDanger = Theme.Danger;
            CDangerSoft = Theme.DangerSoft;
            CText = Theme.TextC;
            CSubText = Theme.SubText;
            CLine = Theme.Line;
            CBand = Theme.Band;
            CGood = Theme.Good;
            CInputBg = Theme.InputBg;
            CStatusBg = Theme.StatusBg;
            CMenuBg = Theme.MenuBg;
            CMenuSel = Theme.MenuSel;
            CHdrFrom = Theme.HeaderFrom;
            CHdrTo = Theme.HeaderTo;
            CSubOnHeader = Theme.SubOnHeader;
            CGridLine = Theme.GridLine;
            CGridHeader = Theme.GridHeaderBg;
            CGridSel = Theme.GridSelBg;
            COnAccent = Theme.OnAccent;
            CFieldLine = Theme.FieldLine;
            UiFont = Settings.UiFont();
        }

        // ============================================================
        //  主题 / 语言 / 字体 / 菜单命令
        // ============================================================
        private const string AppVersion = "1.4";

        private void SetTheme(int mode)
        {
            Theme.Mode = mode;
            Theme.Refresh();
            Settings.Save();
            RebuildUi();
        }

        private void OnCustomTheme()
        {
            Color bg = Theme.CustomBg;
            string img = Theme.CustomImage;
            bool glass = Theme.Glass;
            int glassOpacity = Theme.GlassOpacity;
            if (Dialogs.ShowCustomTheme(this, ref bg, ref img, ref glass, ref glassOpacity))
            {
                Theme.Mode = Theme.Custom;
                Theme.CustomBg = bg;
                Theme.CustomImage = img;
                Theme.Glass = glass;
                Theme.GlassOpacity = glassOpacity;
                Theme.Refresh();
                Settings.Save();
                RebuildUi();
            }
        }

        private void ResetBackground()
        {
            Theme.Mode = Theme.Light;
            Theme.CustomImage = "";
            Theme.Refresh();
            Settings.Save();
            RebuildUi();
        }

        private void SetLang(string code)
        {
            if (Lang.Code == code) return;
            Lang.Code = code;
            Settings.Save();
            RebuildUi();
        }

        private void SetFontPt(double pt)
        {
            if (Math.Abs(Settings.FontPt - pt) < 0.05) return;
            Settings.FontPt = pt;
            Settings.Save();
            RebuildUi();
        }

        private void OnFontSettings()
        {
            string name = Settings.FontName;
            double pt = Settings.FontPt;
            if (Dialogs.ShowFont(this, Settings.FontName, Settings.FontPt, out name, out pt))
            {
                Settings.FontName = name;
                Settings.FontPt = pt;
                Settings.Save();
                RebuildUi();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_cts != null) _cts.Cancel();
            }
            catch { }
            SaveWindowPrefs();
            SaveColumnPrefs();
            Settings.Save();
            base.OnFormClosing(e);
        }

        // 1.4：记住窗口大小（V7 起：不再自动最大化——默认就按普通窗口打开，尺寸不超出当前屏幕）
        private void AdjustSizeToScreen(int w, int h, out int fitW, out int fitH)
        {
            Rectangle wa = Screen.FromControl(this).WorkingArea;
            fitW = w; fitH = h;
            // 屏幕装不下就按工作区收一点（留出标题栏与边框的余量），先保证整窗可见
            int maxW = wa.Width - 16, maxH = wa.Height - 60;
            if (fitW > maxW) fitW = maxW;
            if (fitH > maxH) fitH = maxH;
            if (this.MinimumSize.Width > 0 && fitW < this.MinimumSize.Width) fitW = this.MinimumSize.Width;
            if (this.MinimumSize.Height > 0 && fitH < this.MinimumSize.Height) fitH = this.MinimumSize.Height;
            if (fitW > maxW) fitW = maxW;
            if (fitH > maxH) fitH = maxH;
        }

        private void ApplyWindowPrefs()
        {
            if (Settings.WindowW <= 0 || Settings.WindowH <= 0) return;
            try
            {
                int w, h;
                AdjustSizeToScreen(Settings.WindowW, Settings.WindowH, out w, out h);
                this.ClientSize = new Size(w, h);
                Rectangle wa = Screen.FromControl(this).WorkingArea;
                this.Location = new Point(wa.Left + (wa.Width - this.Width) / 2, wa.Top + (wa.Height - this.Height) / 2);
            }
            catch { }
            // 一律以普通窗口打开：上次留下的是最大化状态时，只沿用它的窗口尺寸，不再恢复最大化
            Settings.WindowMax = false;
        }

        private void SaveWindowPrefs()
        {
            try
            {
                int w, h;
                if (this.WindowState == FormWindowState.Maximized)
                {
                    w = this.RestoreBounds.Width;
                    h = this.RestoreBounds.Height;
                    Settings.WindowMax = true;
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    return;   // 最小化时尺寸没有意义，跳过
                }
                else
                {
                    w = this.ClientSize.Width;
                    h = this.ClientSize.Height;
                    Settings.WindowMax = false;
                }
                if (w > 800 && h > 600 && w < 8000 && h < 8000)
                {
                    Settings.WindowW = w;
                    Settings.WindowH = h;
                }
            }
            catch { }
        }

        // 用新主题/语言/字体重建整套界面，保留扫描范围、选项与已有结果
        private void RebuildUi()
        {
            if (_busy) return;
            ClearGlass();

            string[] folders = GetFolders().ToArray();
            string ext = txtCustomExt.Text;
            decimal minKb = numMin.Value;
            bool[] types = new bool[] { chkImages.Checked, chkVideos.Checked, chkDocs.Checked, chkAudio.Checked, chkArchives.Checked, chkOther.Checked };
            bool hid = chkSkipHidden.Checked, zero = chkSkipZero.Checked;
            int depthIdx = cboDepth.Index;
            bool hadFolders = folders.Length > 0;

            // 切换主题/语言/字体前先记下当前窗口的位置、大小与状态：重建后原样还原，
            // 不能因为换皮肤就把窗口重新拉成最大化（或退回默认尺寸）
            FormWindowState wasState = this.WindowState;
            Size wasSize = (this.WindowState == FormWindowState.Normal) ? this.ClientSize : this.RestoreBounds.Size;
            Point wasLoc = this.Location;

            _ready = false;
            this.SuspendLayout();
            try
            {
                if (this.BackgroundImage != null)
                {
                    Image old = this.BackgroundImage;
                    this.BackgroundImage = null;
                    old.Dispose();
                }
                for (int i = this.Controls.Count - 1; i >= 0; i--)
                {
                    Control c = this.Controls[i];
                    this.Controls.RemoveAt(i);
                    c.Dispose();
                }
                _rowDesign.Clear();
                _colDesign.Clear();
                _dpiScaled = false;
                // 重建界面时先回到设计基准，避免上一次缩放系数被重复叠加（列宽会变成 1.5×1.5 倍）
                _uiScale = 1f;

                BuildUi();
                ApplyUiScaling();

                for (int i = 0; i < folders.Length; i++) lstFolders.Items.Add(folders[i]);
                txtCustomExt.Text = ext;
                decimal v = minKb;
                if (v < numMin.Minimum) v = numMin.Minimum;
                if (v > numMin.Maximum) v = numMin.Maximum;
                numMin.Value = v;
                chkImages.Checked = types[0];
                chkVideos.Checked = types[1];
                chkDocs.Checked = types[2];
                chkAudio.Checked = types[3];
                chkArchives.Checked = types[4];
                chkOther.Checked = types[5];
                cboDepth.SetIndexSilent(depthIdx);
                chkSkipHidden.Checked = hid;
                chkSkipZero.Checked = zero;

                ApplyLanguage(this);
                SyncColumnMenu();
                EnsureContrast(this);
                RebuildGrid();
                UpdateSummary();
                UpdatePathDetail();

                if (_groups.Count == 0)
                {
                    lblStatus.Text = hadFolders
                        ? Lang.F("已添加 {0} 个扫描范围，点“开始扫描”。", folders.Length)
                        : Lang.T("就绪：添加要扫描的文件夹，然后点“开始扫描”。");
                }
            }
            finally
            {
                _ready = true;
                this.ResumeLayout(true);
                if (wasState == FormWindowState.Maximized)
                {
                    // 原来就是最大化：重建后重新走一次尺寸计算，否则新控件仍按默认尺寸排版、右半边留白
                    this.WindowState = FormWindowState.Normal;
                    this.WindowState = FormWindowState.Maximized;
                }
                else
                {
                    // 普通窗口（或最小化前是普通窗口）：把原来的位置和大小还回去
                    Rectangle wa = Screen.FromControl(this).WorkingArea;
                    int w = wasSize.Width, h = wasSize.Height;
                    if (this.MinimumSize.Width > 0 && w < this.MinimumSize.Width) w = this.MinimumSize.Width;
                    if (this.MinimumSize.Height > 0 && h < this.MinimumSize.Height) h = this.MinimumSize.Height;
                    if (w > wa.Width) w = wa.Width;
                    if (h > wa.Height) h = wa.Height;
                    this.WindowState = FormWindowState.Normal;
                    this.ClientSize = new Size(w, h);
                    int x = wasLoc.X, y = wasLoc.Y;
                    if (x < wa.Left) x = wa.Left;
                    if (y < wa.Top) y = wa.Top;
                    if (x + this.Width > wa.Right) x = wa.Right - this.Width;
                    if (y + this.Height > wa.Bottom) y = wa.Bottom - this.Height;
                    this.Location = new Point(x, y);
                }
                this.PerformLayout();
                this.Refresh();
            }
        }

        // ============================================================
        //  毛玻璃：自定义背景图时，卡片透出模糊背景（半透明观感）
        // ============================================================
        private readonly List<Panel> _glassApplied = new List<Panel>();

        private void ApplyGlassIfNeeded()
        {
            bool on = (Theme.Mode == Theme.Custom) && Theme.Glass
                && Theme.CustomImage.Length > 0 && this.BackgroundImage != null;
            if (!on) { ClearGlass(); return; }
            if (!_glassHooked)
            {
                _glassHooked = true;
                this.Resize += delegate { ScheduleGlass(); };
            }
            RefreshGlass();
        }

        private void ScheduleGlass()
        {
            if (Theme.Mode != Theme.Custom || !Theme.Glass || this.BackgroundImage == null) return;
            if (_glassTimer == null)
            {
                _glassTimer = new System.Windows.Forms.Timer();
                _glassTimer.Interval = 220;
                _glassTimer.Tick += delegate
                {
                    _glassTimer.Stop();
                    RefreshGlass();
                };
            }
            _glassTimer.Stop();
            _glassTimer.Start();
        }

        private void ClearGlass()
        {
            for (int i = 0; i < _glassApplied.Count; i++)
            {
                try
                {
                    Image img = _glassApplied[i].BackgroundImage;
                    _glassApplied[i].BackgroundImage = null;
                    if (img != null) img.Dispose();
                }
                catch { }
            }
            _glassApplied.Clear();
            if (_glassBase != null) { _glassBase.Dispose(); _glassBase = null; }
            _glassW = _glassH = 0;
        }

        private void RefreshGlass()
        {
            if (Theme.Mode != Theme.Custom || !Theme.Glass || this.BackgroundImage == null) return;
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w < 400 || h < 300) return;
            if (_glassBase == null || _glassW != w || _glassH != h) BuildGlassBase(w, h);

            List<Panel> panels = new List<Panel>();
            CollectGlassPanels(this, panels);

            Color tint = CCard;
            // 透明度由用户滑块决定：0% = 完全不透明（alpha 255），100% = 完全透明（alpha 0）
            int pct = Theme.GlassOpacity;
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            int alpha = 255 - (int)Math.Round(255.0 * pct / 100.0);
            for (int i = 0; i < _glassApplied.Count; i++)
            {
                Image old = _glassApplied[i].BackgroundImage;
                _glassApplied[i].BackgroundImage = null;
                if (old != null) old.Dispose();
            }
            _glassApplied.Clear();

            for (int i = 0; i < panels.Count; i++)
            {
                Panel p = panels[i];
                if (p.Width < 40 || p.Height < 30) continue;
                Point pt = this.PointToClient(p.PointToScreen(Point.Empty));
                if (pt.X > w || pt.Y > h) continue;
                try
                {
                    Bitmap bmp = new Bitmap(p.Width, p.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(_glassBase,
                            new Rectangle(0, 0, p.Width, p.Height),
                            new Rectangle(pt.X, pt.Y, p.Width, p.Height),
                            GraphicsUnit.Pixel);
                        using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, tint)))
                        {
                            g.FillRectangle(b, 0, 0, p.Width, p.Height);
                        }
                    }
                    p.BackgroundImage = bmp;
                    p.BackgroundImageLayout = ImageLayout.Stretch;
                    _glassApplied.Add(p);
                }
                catch { }
            }

            // 卡片里的文字/勾选框改成透明底，玻璃才透得出来
            for (int i = 0; i < panels.Count; i++) GlassTextTransparent(panels[i]);
        }

        private void BuildGlassBase(int w, int h)
        {
            if (_glassBase != null) { _glassBase.Dispose(); _glassBase = null; }
            _glassW = w;
            _glassH = h;
            int sw = w / 10; if (sw < 32) sw = 32;
            int sh = h / 10; if (sh < 32) sh = 32;
            try
            {
                using (Bitmap small = new Bitmap(sw, sh))
                {
                    using (Graphics g = Graphics.FromImage(small))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g.DrawImage(this.BackgroundImage, new Rectangle(0, 0, sw, sh));
                    }
                    _glassBase = new Bitmap(w, h);
                    using (Graphics g2 = Graphics.FromImage(_glassBase))
                    {
                        g2.InterpolationMode = InterpolationMode.HighQualityBilinear;
                        g2.DrawImage(small, new Rectangle(0, 0, w, h));
                    }
                }
            }
            catch
            {
                _glassBase = null;
            }
        }

        private void CollectGlassPanels(Control parent, List<Panel> list)
        {
            for (int i = 0; i < parent.Controls.Count; i++)
            {
                Control c = parent.Controls[i];
                Panel p = c as Panel;
                if (p != null)
                {
                    if (p.BackColor == CCard && p.BackColor.A > 200) list.Add(p);
                    CollectGlassPanels(p, list);
                }
                else if (c is TableLayoutPanel)
                {
                    CollectGlassPanels(c, list);
                }
            }
        }

        private void GlassTextTransparent(Control parent)
        {
            for (int i = 0; i < parent.Controls.Count; i++)
            {
                Control c = parent.Controls[i];
                if ((c is Label || c is CheckBox || c is RadioButton || c is LinkLabel)
                    && (c.BackColor == CCard || c.BackColor == CPage || c.BackColor == CStatusBg))
                {
                    c.BackColor = Color.Transparent;
                }
                GlassTextTransparent(c);
            }
        }

        // 文字与底色太接近时（例如深色底上的黑字）自动换成当前主题的正文色
        private static double Lum(Color c)
        {
            return 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
        }

        private static Color EffectiveBg(Control c)
        {
            Control p = c;
            while (p != null)
            {
                Color b = p.BackColor;
                if (b.A > 200) return b;
                p = p.Parent;
            }
            return Color.Transparent;
        }

        private void EnsureContrast(Control parent)
        {
            for (int i = 0; i < parent.Controls.Count; i++)
            {
                Control c = parent.Controls[i];
                bool texty = (c is Label || c is LinkLabel || c is CheckBox || c is RadioButton || c is FlatButton || c is ComboButton);
                if (texty && c.Text.Length > 0 && c.Visible)
                {
                    Color bg = EffectiveBg(c);
                    if (bg != Color.Transparent && Math.Abs(Lum(bg) - Lum(c.ForeColor)) < 55)
                    {
                        c.ForeColor = (Lum(bg) > 140) ? Color.FromArgb(28, 36, 48) : Theme.TextC;
                    }
                }
                EnsureContrast(c);
            }
        }

        // 深色主题下把系统标题栏也变成深色（Windows 10 1809+）
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        private void ApplyTitleBarTheme()
        {
            try
            {
                if (this.Handle == IntPtr.Zero) return;
                int dark = (Theme.Mode == Theme.Dark || (Theme.Mode == Theme.Custom && Theme.UseDarkCards)) ? 1 : 0;
                int v = dark;
                int hr = DwmSetWindowAttribute(this.Handle, 20, ref v, 4);
                if (hr != 0) DwmSetWindowAttribute(this.Handle, 19, ref v, 4);
            }
            catch { }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyTitleBarTheme();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyTitleBarTheme();
        }

        // 把界面上的中文原文替换成当前语言（重建后调用，所以原文总是干净的）
        private void ApplyLanguage(Control parent)
        {
            if (parent == null) return;
            for (int i = 0; i < parent.Controls.Count; i++)
            {
                Control c = parent.Controls[i];
                try
                {
                    ComboButton combo = c as ComboButton;
                    if (combo != null)
                    {
                        combo.Translate();
                    }
                    else
                    {
                        string t = Lang.T(c.Text);
                        if (t != c.Text && c.Text.Length > 0) c.Text = t;
                    }
                }
                catch { }

                ComboBox cb = c as ComboBox;
                if (cb != null)
                {
                    int ix = cb.SelectedIndex;
                    for (int k = 0; k < cb.Items.Count; k++)
                    {
                        string s = cb.Items[k] as string;
                        if (s != null) cb.Items[k] = Lang.T(s);
                    }
                    if (ix >= 0 && ix < cb.Items.Count) cb.SelectedIndex = ix;
                }

                DataGridView g = c as DataGridView;
                if (g != null)
                {
                    for (int k = 0; k < g.Columns.Count; k++)
                    {
                        g.Columns[k].HeaderText = Lang.T(g.Columns[k].HeaderText);
                    }
                }

                if (c.ContextMenuStrip != null) ApplyLangToItems(c.ContextMenuStrip.Items);
                if (c is ToolStrip) ApplyLangToItems(((ToolStrip)c).Items);

                ApplyLanguage(c);
            }
            if (parent == this) this.Text = Lang.T("文件查重工具");
        }

        private void ApplyLangToItems(ToolStripItemCollection items)
        {
            if (items == null) return;
            for (int i = 0; i < items.Count; i++)
            {
                ToolStripItem it = items[i];
                string t = Lang.T(it.Text);
                if (t != it.Text) it.Text = t;
                ToolStripMenuItem mi = it as ToolStripMenuItem;
                if (mi != null && mi.DropDownItems.Count > 0) ApplyLangToItems(mi.DropDownItems);
            }
        }

        private void ClearResults()
        {
            if (_busy) return;
            _groups.Clear();
            _lastSummary = null;
            if (dgv != null) dgv.Rows.Clear();
            UpdateSummary();
            UpdatePathDetail();
            lblStatus.Text = Lang.T("就绪：添加要扫描的文件夹，然后点“开始扫描”。");
            if (pb != null) pb.SetValue(0);
            if (lblPercent != null) lblPercent.Text = "";
        }

        private void OnClearFolderList()
        {
            lstFolders.Items.Clear();
            lblStatus.Text = Lang.T("就绪：添加要扫描的文件夹，然后点“开始扫描”。");
        }

        private void OnAbout()
        {
            Dialogs.ShowAbout(this, AppVersion);
        }

        private void OnAboutSoftware()
        {
            Dialogs.ShowAboutSoftware(this, AppVersion);
        }

        private void OnCheckUpdate()
        {
            MessageBox.Show(this, Lang.T("本工具为本地单机版，没有在线升级服务器；当前已是最新版本 1.4。"),
                Lang.T("升级提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAboutAuthor()
        {
            Dialogs.ShowAuthor(this);
        }

        // 表格布局的绝对行列尺寸是否随 DPI 自动放大在不同环境下表现不一致，
        // 这里先记录设计值，缩放后只在“没有变化”时才手动补齐一次，避免重复放大。
        private Dictionary<TableLayoutPanel, float[]> _rowDesign = new Dictionary<TableLayoutPanel, float[]>();
        private Dictionary<TableLayoutPanel, float[]> _colDesign = new Dictionary<TableLayoutPanel, float[]>();

        private void CaptureDesignStyles(Control parent)
        {
            for (int i = 0; i < parent.Controls.Count; i++)
            {
                Control c = parent.Controls[i];
                TableLayoutPanel t = c as TableLayoutPanel;
                if (t != null)
                {
                    float[] rows = new float[t.RowStyles.Count];
                    for (int r = 0; r < t.RowStyles.Count; r++)
                    {
                        rows[r] = (t.RowStyles[r].SizeType == SizeType.Absolute) ? t.RowStyles[r].Height : -1f;
                    }
                    float[] cols = new float[t.ColumnStyles.Count];
                    for (int k = 0; k < t.ColumnStyles.Count; k++)
                    {
                        cols[k] = (t.ColumnStyles[k].SizeType == SizeType.Absolute) ? t.ColumnStyles[k].Width : -1f;
                    }
                    _rowDesign[t] = rows;
                    _colDesign[t] = cols;
                }
                CaptureDesignStyles(c);
            }
        }

        private void FixDesignStyles(float f)
        {
            foreach (KeyValuePair<TableLayoutPanel, float[]> kv in _rowDesign)
            {
                TableLayoutPanel t = kv.Key;
                float[] design = kv.Value;
                for (int r = 0; r < design.Length; r++)
                {
                    if (design[r] < 0) continue;
                    if (Math.Abs(t.RowStyles[r].Height - design[r]) < 1f)
                    {
                        t.RowStyles[r].Height = design[r] * f;
                    }
                }
            }
            foreach (KeyValuePair<TableLayoutPanel, float[]> kv in _colDesign)
            {
                TableLayoutPanel t = kv.Key;
                float[] design = kv.Value;
                for (int k = 0; k < design.Length; k++)
                {
                    if (design[k] < 0) continue;
                    if (Math.Abs(t.ColumnStyles[k].Width - design[k]) < 1f)
                    {
                        t.ColumnStyles[k].Width = design[k] * f;
                    }
                }
            }

            // DataGridView 的列宽/行高不会被 Control.Scale 处理，需单独按 DPI 放大
            if (dgv != null)
            {
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    DataGridViewColumn c = dgv.Columns[i];
                    if (c.AutoSizeMode == DataGridViewAutoSizeColumnMode.Fill)
                    {
                        c.MinimumWidth = (int)(c.MinimumWidth * f);
                    }
                    else
                    {
                        c.Width = (int)(c.Width * f);
                    }
                }
                if (dgv.RowTemplate.Height > 0) dgv.RowTemplate.Height = (int)(dgv.RowTemplate.Height * f);
                dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
                if (dgv.ColumnHeadersHeight > 0) dgv.ColumnHeadersHeight = (int)(dgv.ColumnHeadersHeight * f);
            }
        }

        private Panel MakeCard()
        {
            Panel card = new Panel();
            if (Theme.IsSimple())
            {
                // 简约模式：沿用 1.4 的方角卡片（卡片底色铺满，只描一条圆角细边）
                card.BackColor = CCard;
                card.Padding = new Padding(16, 14, 16, 14);
                card.Paint += delegate(object s, PaintEventArgs e)
                {
                    Panel p = (Panel)s;
                    if (p.Width < 3 || p.Height < 3) return;
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                    using (GraphicsPath path = ProgressPanel.Round(r, 8))
                    using (Pen pen = new Pen(CLine, 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                };
                return card;
            }
            card.BackColor = CPage;
            card.Padding = new Padding(18, 16, 18, 16);
            card.Paint += delegate(object s, PaintEventArgs e)
            {
                Panel p = (Panel)s;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle r = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
                using (GraphicsPath path = ProgressPanel.Round(r, 10))
                using (Pen pen = new Pen(CLine, 1))
                {
                    using (SolidBrush br = new SolidBrush(CCard)) e.Graphics.FillPath(br, path);
                    e.Graphics.DrawPath(pen, path);
                }
            };
            return card;
        }

        private static Button MakeButton(string text, Color back, Color fore, int w, int h)
        {
            return MakeButton(text, back, fore, w, h, false);
        }

        // ghost = 参考图里那种“白底 + 细边 + 深字”的小按钮（目前只有“收起”用它）
        private static Button MakeButton(string text, Color back, Color fore, int w, int h, bool ghost)
        {
            FlatButton b = new FlatButton();
            Color fill = back;
            Color ink = fore;
            Color stroke = Color.Transparent;
            int radius = 9;

            if (Theme.IsSimple())
            {
                // 简约模式：恢复 1.4 的方角按钮（1.4 的描边颜色就是按钮文字色）
                radius = 0;
                stroke = fore;
            }
            else if (Theme.Mode == Theme.Light)
            {
                // 白色（青灰）主题：按参考图，所有小控件统一深青填充 + 白字
                radius = 6;
                if (ghost)
                {
                    fill = CCard;
                    ink = CText;
                    stroke = CFieldLine;
                }
                else
                {
                    fill = CAccent;
                    ink = COnAccent;
                    stroke = Color.Transparent;
                }
            }
            else
            {
                // 深色主题：圆角按钮的描边——主按钮无描边，危险按钮用红色淡描边，其余用分隔线色
                if (fore == COnAccent) stroke = Color.Transparent;
                else if (fore == CDanger) stroke = Color.FromArgb(170, CDanger);
                else stroke = CLine;
                radius = 9;
            }

            b.Text = text;
            b.Width = w;
            b.Height = h;
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = fill;
            b.ForeColor = ink;
            b.Font = new Font(UiFont, 9F);
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
            b.AutoSize = false;
            b.TabStop = false;
            b.OffFore = CSubText;
            b.OffBorder = CLine;
            b.Stroke = stroke;
            b.Radius = radius;

            Color normal = fill;
            Color hover = Color.FromArgb(Math.Max(0, fill.R - 14), Math.Max(0, fill.G - 14), Math.Max(0, fill.B - 14));
            Color disabled = CNeutral;
            b.MouseEnter += delegate { if (b.Enabled) b.BackColor = hover; };
            b.MouseLeave += delegate { if (b.Enabled) b.BackColor = normal; };
            b.EnabledChanged += delegate { b.BackColor = b.Enabled ? normal : disabled; };
            return b;
        }

        private static Button GrowButton(Button b, int minW, int minH)
        {
            b.Dock = DockStyle.None;
            b.AutoSize = true;
            b.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            b.MinimumSize = new Size(minW, minH);
            b.Padding = new Padding(7, 0, 7, 0);
            return b;
        }

        private static CheckBox MakeCheck(string text, int w, int h)
        {
            CheckBox c;
            if (Theme.Mode == Theme.Light)
            {
                // 白色（青灰）主题：深青方块 + 白勾的自绘复选框
                c = new FlatCheck();
                c.FlatStyle = FlatStyle.Flat;
            }
            else
            {
                c = new CheckBox();
                c.FlatStyle = FlatStyle.System;
            }
            c.Text = text;
            c.Width = w;
            c.Height = h;
            c.Font = new Font(UiFont, 9.5F);
            c.ForeColor = CText;
            c.BackColor = CCard;
            return c;
        }

        // 全文件扫描时，文件类型勾选与补充扩展名自动置灰（不做格式过滤）
        private void UpdateTypeEnable()
        {
            bool all = (chkAllFiles != null && chkAllFiles.Checked);
            CheckBox[] boxes = new CheckBox[] { chkImages, chkVideos, chkDocs, chkAudio, chkArchives, chkOther };
            for (int i = 0; i < boxes.Length; i++)
            {
                if (boxes[i] != null) boxes[i].Enabled = !all;
            }
            if (txtCustomExt != null) txtCustomExt.Enabled = !all;

            // 相似图片依赖图片类型：没勾“图片”时也允许（自动按图片集合识别），只做视觉置灰提示
            bool sim = (chkSimilar != null && chkSimilar.Checked);
            Button[] th = new Button[] { btnT1, btnT2, btnT3 };
            for (int i = 0; i < th.Length; i++)
            {
                if (th[i] != null) th[i].Enabled = sim;
            }
        }

        private void SetSimilarThreshold(int t)
        {
            _similarThreshold = t;
            Settings.SimilarThreshold = t;
            UpdateSimilarButtons();
        }

        private void UpdateSimilarButtons()
        {
            Button[] b = new Button[] { btnT1, btnT2, btnT3 };
            int[] v = new int[] { 3, 6, 12 };
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] == null) continue;
                bool on = (v[i] == _similarThreshold);
                b[i].BackColor = on ? CAccentSoft : CNeutral;
                b[i].ForeColor = on ? CAccent : CText;
                b[i].FlatAppearance.BorderColor = on ? CAccent : CLine;
            }
        }

        // ============================================================
        //  扫描
        // ============================================================
        private List<string> GetFolders()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < lstFolders.Items.Count; i++)
            {
                string s = lstFolders.Items[i] as string;
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
            return list;
        }

        private void OnAddFolder()
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = Lang.T("选择要扫描的文件夹（可重复添加多个，也可以直接把文件夹拖进窗口）");
                dlg.ShowNewFolderButton = false;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    AddFolder(dlg.SelectedPath);
                }
            }
        }

        private void AddFolder(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                path = Path.GetFullPath(path).TrimEnd('\\');
            }
            catch
            {
                return;
            }
            for (int i = 0; i < lstFolders.Items.Count; i++)
            {
                string old = lstFolders.Items[i] as string;
                if (string.Equals(old, path, StringComparison.OrdinalIgnoreCase)) return;
            }
            lstFolders.Items.Add(path);
            lstFolders.TopIndex = lstFolders.Items.Count - 1;
        }

        private void AddPaths(string[] items)
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                string p = items[i];
                if (string.IsNullOrEmpty(p)) continue;
                try
                {
                    if (Directory.Exists(p)) AddFolder(p);
                    else if (File.Exists(p))
                    {
                        string dir = Path.GetDirectoryName(p);
                        if (!string.IsNullOrEmpty(dir)) AddFolder(dir);
                    }
                }
                catch
                {
                }
            }
            if (lstFolders.Items.Count > 0) lblStatus.Text = Lang.F("已添加 {0} 个扫描范围，点“开始扫描”。", lstFolders.Items.Count);
        }

        private void OnRemoveFolder()
        {
            if (lstFolders.SelectedIndices.Count == 0) return;
            List<int> idx = new List<int>();
            for (int i = 0; i < lstFolders.SelectedIndices.Count; i++) idx.Add(lstFolders.SelectedIndices[i]);
            idx.Sort();
            for (int i = idx.Count - 1; i >= 0; i--)
            {
                lstFolders.Items.RemoveAt(idx[i]);
            }
        }

        private Func<string, bool> BuildExtensionFilter()
        {
            // 全文件扫描：任何扩展名（含无扩展名）都收，只靠内容哈希判重
            if (chkAllFiles != null && chkAllFiles.Checked)
            {
                return delegate(string ext) { return true; };
            }

            HashSet<string> custom = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string raw = txtCustomExt.Text;
            if (!string.IsNullOrEmpty(raw))
            {
                string[] parts = raw.Split(new char[] { ',', '，', ';', '；', ' ', '|', '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    string p = parts[i].Trim();
                    if (p.Length == 0) continue;
                    if (p[0] != '.') p = "." + p;
                    custom.Add(p);
                }
            }
            bool img = chkImages.Checked;
            bool vid = chkVideos.Checked;
            bool doc = chkDocs.Checked;
            bool aud = chkAudio.Checked;
            bool arc = chkArchives.Checked;
            bool oth = chkOther.Checked;

            return delegate(string ext)
            {
                if (string.IsNullOrEmpty(ext)) return oth;
                if (custom.Contains(ext)) return true;
                if (chkSimilar != null && chkSimilar.Checked && (ImgExt.Contains(ext) || ImageHash.IsImage("x" + ext))) return true;
                if (ImgExt.Contains(ext)) return img;
                if (VidExt.Contains(ext)) return vid;
                if (DocExt.Contains(ext)) return doc;
                if (AudExt.Contains(ext)) return aud;
                if (ArcExt.Contains(ext)) return arc;
                return oth;
            };
        }

        // 下拉项 → 子目录层级：0 只扫当前目录、1/2/3 含 N 层、4 = 全部
        private static int DepthFromIndex(int idx)
        {
            if (idx <= 0) return 0;
            if (idx >= 4) return -1;
            return idx;
        }

        private void OnScan()
        {
            if (_busy) return;
            List<string> folders = GetFolders();
            if (folders.Count == 0)
            {
                MessageBox.Show(this, Lang.T("请先添加要扫描的文件夹。\n点“添加文件夹”，或直接把文件夹拖到窗口里。"), Lang.T("还没有扫描范围"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ScanOptions opt = new ScanOptions();
            opt.Folders = folders;
            // 仅自测用：FILEDEDUP_MIN_MB=1 先把“最小文件大小”设成该 MB 值；FILEDEDUP_MIN_LOG=路径 记录换算结果
            try
            {
                string mbEnv = Environment.GetEnvironmentVariable("FILEDEDUP_MIN_MB");
                if (!string.IsNullOrEmpty(mbEnv)) numMin.Value = decimal.Parse(mbEnv, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch { }
            // 界面上单位是 MB，核心要的是字节：MB × 1024 × 1024
            opt.MinSize = (long)Math.Round((double)numMin.Value * 1048576.0);
            try
            {
                string minLog = Environment.GetEnvironmentVariable("FILEDEDUP_MIN_LOG");
                if (!string.IsNullOrEmpty(minLog))
                    System.IO.File.AppendAllText(minLog, "界面值=" + numMin.Value.ToString() + " MB -> 字节=" + opt.MinSize + "\r\n", System.Text.Encoding.UTF8);
            }
            catch { }
            opt.Recursive = true;
            opt.MaxDepth = DepthFromIndex(cboDepth.Index);
            opt.SkipHiddenSystem = chkSkipHidden.Checked;
            opt.SkipZeroByte = chkSkipZero.Checked;
            opt.AcceptExtension = BuildExtensionFilter();
            opt.SimilarImages = (chkSimilar != null && chkSimilar.Checked);
            opt.SimilarThreshold = _similarThreshold;

            _groups.Clear();
            dgv.Rows.Clear();
            UpdateSummary();
            lblStatus.Text = Lang.T("准备扫描…");
            _busy = true;
            SetUiBusy(true);

            if (_cts != null)
            {
                try { _cts.Dispose(); } catch { }
            }
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            _gate = new PauseGate();

            Task.Run(delegate
            {
                ScanSummary res = null;
                Exception err = null;
                Stopwatch sw = Stopwatch.StartNew();
                try
                {
                    Scanner sc = new Scanner();
                    res = sc.Scan(opt, token, _gate, delegate(string text, int cur, int total) { OnProgress(text, cur, total); });
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { err = ex; }
                sw.Stop();

                try
                {
                    this.BeginInvoke(new Action(delegate
                    {
                        _busy = false;
                        SetUiBusy(false);
                        pb.SetValue(0);
                        lblPercent.Text = "";

                        if (token.IsCancellationRequested)
                        {
                            lblStatus.Text = Lang.T("已取消扫描。");
                            return;
                        }
                        if (err != null)
                        {
                            lblStatus.Text = Lang.F("扫描出错：{0}", err.Message);
                            MessageBox.Show(this, Lang.F("扫描过程中出错：\n{0}", err.Message), Lang.T("扫描失败"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (res == null) return;

                        _groups = new List<DupGroup>(res.Groups);
                        _groups.AddRange(res.SimilarGroups);
                        _lastSummary = res;
                        _metaCache.Clear();
                        ClearThumbCache();
                        ApplyRuleAndRebuild();
                        btnExport.Enabled = _groups.Count > 0;

                        string extra = res.SimilarGroups.Count > 0
                            ? Lang.F(" · 相似组 {0}（已算 {1} 张图片指纹）", Fmt.Number(res.SimilarGroups.Count), Fmt.Number(res.HashedImages))
                            : "";
                        lblStatus.Text = Lang.F("扫描完成 · 检查 {0} 个文件 · 用时 {1:F1} 秒{2}{3}",
                            Fmt.Number(res.FilesScanned), sw.Elapsed.TotalSeconds,
                            res.Unreadable > 0 ? Lang.F(" · {0} 个文件读取失败已跳过", res.Unreadable) : "", extra);

                        // 仅自测用：FILEDEDUP_SELFTEST=1 验证「全不选 → 重新按规则勾选」的勾选状态
                        try
                        {
                            if (Environment.GetEnvironmentVariable("FILEDEDUP_SELFTEST") == "1")
                            {
                                SetAllChecked(false);
                                int afterNone = CountChecked();
                                ApplyRuleAndRebuild();
                                int afterRule = CountChecked();
                                System.Text.StringBuilder log = new System.Text.StringBuilder();
                                log.AppendLine("afterNone=" + afterNone);
                                log.AppendLine("afterRule=" + afterRule);
                                for (int i = 0; i < dgv.Rows.Count; i++)
                                {
                                    FileItem f = dgv.Rows[i].Tag as FileItem;
                                    if (f == null) continue;
                                    log.AppendLine((dgv.Rows[i].Cells[0].Value is bool && (bool)dgv.Rows[i].Cells[0].Value) + " | " + f.FileName + " | " + f.Note);
                                }
                                System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "filededup_selftest.txt"), log.ToString());
                            }
                        }
                        catch { }

                        // 仅自测用：FILEDEDUP_CSV=路径 扫描完成后直接写出 CSV
                        try
                        {
                            string csvOut = Environment.GetEnvironmentVariable("FILEDEDUP_CSV");
                            if (!string.IsNullOrEmpty(csvOut) && _groups.Count > 0) WriteCsvTo(csvOut);
                        }
                        catch { }

                        // 扫描完成后一律给出统计报告：有没有重复都弹，0 结果也写明“重复组 0 / 可释放 0 B”
                        // （报告里仍有“以后每次扫描完成都自动显示”的勾选，想关随时关得掉）
                        if (Settings.AutoReport) ShowScanReport();

                        // 仅自测用：扫描完成后打开指定对话框
                        if (_pendingOpen == "assistant" && _groups.Count > 0)
                        {
                            _pendingOpen = "";
                            ShowSelectionAssistant();
                        }
                    }));
                }
                catch
                {
                }
            });
        }

        private void OnStop()
        {
            if (_cts != null)
            {
                try { _cts.Cancel(); } catch { }
            }
            if (_gate != null) _gate.Resume();
            if (btnPause != null) btnPause.Enabled = false;
            lblStatus.Text = Lang.T("正在取消扫描…");
        }

        private void OnPause()
        {
            if (!_busy || _gate == null) return;
            this.ActiveControl = null;
            if (_gate.Paused)
            {
                _gate.Resume();
                btnPause.Text = Lang.T("暂停");
                lblStatus.Text = Lang.T("已继续扫描…");
            }
            else
            {
                _gate.Pause();
                btnPause.Text = Lang.T("继续");
                lblStatus.Text = Lang.T("已暂停扫描（点“继续”接着扫）。");
            }
        }

        private void OnProgress(string text, int cur, int total)
        {
            if (this.IsDisposed) return;
            try
            {
                this.BeginInvoke(new Action(delegate
                {
                    if (this.IsDisposed) return;
                    lblStatus.Text = text;
                    if (total <= 0)
                    {
                        pb.SetMarquee();
                        lblPercent.Text = "";
                    }
                    else
                    {
                        int p = (int)Math.Round(cur * 100.0 / total);
                        pb.SetValue(p);
                        lblPercent.Text = p + "%";
                    }
                }));
            }
            catch
            {
            }
        }

        private void SetUiBusy(bool busy)
        {
            btnScan.Enabled = !busy;
            btnPause.Enabled = busy;
            if (!busy) btnPause.Text = Lang.T("暂停");
            btnStop.Enabled = busy;
            btnAdd.Enabled = !busy;
            btnRemove.Enabled = !busy;
            btnClear.Enabled = !busy;
            btnSelectAll.Enabled = !busy;
            btnSelectNone.Enabled = !busy;
            btnRecycle.Enabled = !busy;
            btnPermanent.Enabled = !busy;
            btnOpenFolder.Enabled = !busy;
            btnExport.Enabled = !busy && _groups.Count > 0;
            cboKeepRule.Enabled = !busy;
            cboSort.Enabled = !busy;
            btnExport.Enabled = !busy && _groups.Count > 0;
            if (busy)
            {
                pb.SetMarquee();
                lblPercent.Text = "";
            }
        }

        // ============================================================
        //  结果展示
        // ============================================================
        private FileItem ChooseKeeper(DupGroup g)
        {
            if (g.Files.Count == 0) return null;
            FileItem best = g.Files[0];
            for (int i = 1; i < g.Files.Count; i++)
            {
                FileItem f = g.Files[i];
                if (_keepRule == 0)
                {
                    if (f.Modified < best.Modified) best = f;
                }
                else if (_keepRule == 1)
                {
                    if (f.Modified > best.Modified) best = f;
                }
                else if (_keepRule == 2)
                {
                    if (f.FullPath.Length < best.FullPath.Length) best = f;
                }
                else if (_keepRule == 3)
                {
                    // 分辨率最高：像素总面积大的优先；面积相同取文件更大的
                    ImageMeta a = MetaOf(f), b = MetaOf(best);
                    long pa = (a != null ? (long)a.Width * a.Height : 0);
                    long pb = (b != null ? (long)b.Width * b.Height : 0);
                    if (pa > pb) best = f;
                    else if (pa == pb && f.Length > best.Length) best = f;
                }
                else
                {
                    // 文件名最短：只比文件名本身（不含目录），等长取字典序靠前的
                    int c = f.FileName.Length.CompareTo(best.FileName.Length);
                    if (c < 0) best = f;
                    else if (c == 0 && string.Compare(f.FileName, best.FileName, StringComparison.OrdinalIgnoreCase) < 0) best = f;
                }
            }
            return best;
        }

        private void ApplySmartSelection()
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                FileItem keep = ChooseKeeper(g);
                for (int j = 0; j < g.Files.Count; j++)
                {
                    FileItem f = g.Files[j];
                    f.SelectedForDelete = (f != keep);
                    f.Note = (f == keep) ? "保留这份" : "重复";
                }
            }
        }

        // 按规则重算勾选并刷新表格（不把表格旧的勾选回写模型）
        private void ApplyRuleAndRebuild()
        {
            _skipPersist = true;
            try
            {
                ApplySmartSelection();
                RebuildGrid();
            }
            finally
            {
                _skipPersist = false;
            }
        }

        private List<DupGroup> SortedGroups()
        {
            List<DupGroup> ordered = new List<DupGroup>();
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                if (_viewFilter == 1 && g.Similar) continue;
                if (_viewFilter == 2 && !g.Similar) continue;
                ordered.Add(g);
            }
            if (_sortMode == 0)
            {
                ordered.Sort(delegate(DupGroup a, DupGroup b)
                {
                    int c = b.Size.CompareTo(a.Size);
                    if (c != 0) return c;
                    return string.Compare(a.FirstPath(), b.FirstPath(), StringComparison.OrdinalIgnoreCase);
                });
            }
            else if (_sortMode == 1)
            {
                ordered.Sort(delegate(DupGroup a, DupGroup b)
                {
                    int c = a.Size.CompareTo(b.Size);
                    if (c != 0) return c;
                    return string.Compare(a.FirstPath(), b.FirstPath(), StringComparison.OrdinalIgnoreCase);
                });
            }
            else if (_sortMode == 2)
            {
                ordered.Sort(delegate(DupGroup a, DupGroup b) { return b.NewestTime().CompareTo(a.NewestTime()); });
            }
            else if (_sortMode == 3)
            {
                ordered.Sort(delegate(DupGroup a, DupGroup b) { return a.NewestTime().CompareTo(b.NewestTime()); });
            }
            else
            {
                ordered.Sort(delegate(DupGroup a, DupGroup b) { return string.Compare(a.FirstPath(), b.FirstPath(), StringComparison.OrdinalIgnoreCase); });
            }
            return ordered;
        }

        private void SortFilesInGroup(DupGroup g)
        {
            if (_keepRule == 0)
            {
                g.Files.Sort(delegate(FileItem a, FileItem b) { return a.Modified.CompareTo(b.Modified); });
            }
            else if (_keepRule == 1)
            {
                g.Files.Sort(delegate(FileItem a, FileItem b) { return b.Modified.CompareTo(a.Modified); });
            }
            else if (_keepRule == 2)
            {
                g.Files.Sort(delegate(FileItem a, FileItem b) { return a.FullPath.Length.CompareTo(b.FullPath.Length); });
            }
            else if (_keepRule == 3)
            {
                g.Files.Sort(delegate(FileItem a, FileItem b)
                {
                    ImageMeta ma = MetaOf(a), mb = MetaOf(b);
                    long pa = ma != null ? (long)ma.Width * ma.Height : 0;
                    long pb = mb != null ? (long)mb.Width * mb.Height : 0;
                    if (pa != pb) return pb.CompareTo(pa);
                    return b.Length.CompareTo(a.Length);
                });
            }
            else
            {
                g.Files.Sort(delegate(FileItem a, FileItem b)
                {
                    int c = a.FileName.Length.CompareTo(b.FileName.Length);
                    if (c != 0) return c;
                    return string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                });
            }
        }

        // 统计当前已勾选（按模型）
        private int CountChecked()
        {
            int n = 0;
            for (int i = 0; i < _groups.Count; i++)
            {
                for (int j = 0; j < _groups[i].Files.Count; j++)
                    if (_groups[i].Files[j].SelectedForDelete) n++;
            }
            return n;
        }

        private void PersistCheckStates()
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow row = dgv.Rows[i];
                FileItem f = row.Tag as FileItem;
                if (f == null) continue;
                object v = row.Cells[0].Value;
                f.SelectedForDelete = (v is bool) && (bool)v;
            }
        }

        // ---------- 1.4：列宽 / 列显隐 / 窗口尺寸 记忆 ----------
        // 默认列宽（按列序号）：删除 / 组 / 文件名 / 路径(Fill) / 大小 / 修改时间 / 创建时间 / 状态
        private static readonly int[] DefaultColW = new int[] { 88, 58, 290, 0, 100, 140, 140, 70 };
        // 默认列顺序（按显示位置记录每一格放的是第几列）：删除0 / 组1 / 文件名2 / 大小4 / 修改时间5 / 状态7 / 创建时间6 / 完整路径3
        private static readonly int[] DefaultColOrderIdx = new int[] { 0, 1, 2, 4, 5, 7, 6, 3 };


        // “每组保留”下拉的选项文案（索引即 _keepRule）
        private static readonly string[] KeepRuleNames = new string[]
        {
            "最早修改的那份", "最新修改的那份", "路径最短的那份", "分辨率最高的那份", "文件名最短的那份"
        };

        private void ApplyColumnPrefs()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            try
            {
                int[] w = DefaultColW;
                string saved = Settings.ColWidths;
                if (!string.IsNullOrEmpty(saved))
                {
                    string[] parts = saved.Split(',');
                    if (parts.Length == 7)
                    {
                        int[] p = new int[8];
                        p[0] = IntOr(parts[0], DefaultColW[0]);
                        p[1] = IntOr(parts[1], DefaultColW[1]);
                        p[2] = IntOr(parts[2], DefaultColW[2]);
                        p[3] = 0;
                        p[4] = IntOr(parts[3], DefaultColW[4]);
                        p[5] = IntOr(parts[4], DefaultColW[5]);
                        p[6] = IntOr(parts[5], DefaultColW[6]);
                        p[7] = IntOr(parts[6], DefaultColW[7]);
                        w = p;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i == CColPath) continue;   // 路径列是 Fill，宽度由窗口决定
                    int width = w[i];
                    if (width < 40) width = DefaultColW[i];
                    if (width > 1200) width = DefaultColW[i];
                    dgv.Columns[i].Width = (int)(width * _uiScale);
                }

                string vis = Settings.ColVisible;
                if (!string.IsNullOrEmpty(vis))
                {
                    string[] parts = vis.Split(',');
                    if (parts.Length == 8)
                    {
                        for (int i = 0; i < 8; i++) dgv.Columns[i].Visible = (parts[i] == "1");
                    }
                }
                else
                {
                    dgv.Columns[CColCreated].Visible = Settings.ShowCreated;
                }
                // 勾选列、路径列与“状态”列是操作核心，不允许隐藏
                dgv.Columns[CColDel].Visible = true;
                dgv.Columns[CColNote].Visible = true;
                dgv.Columns[CColPath].Visible = true;

                // 最后还原“拖列头”排出来的列顺序（显隐先定好，再排位置才稳）
                ApplyColumnOrder();
            }
            catch { }
        }

        private static int IntOr(string s, int def)
        {
            int v;
            if (int.TryParse(s, out v) && v > 0) return v;
            return def;
        }

        private void SaveColumnPrefs()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            try
            {
                string[] w = new string[7];
                w[0] = ((int)(dgv.Columns[CColDel].Width / _uiScale)).ToString();
                w[1] = ((int)(dgv.Columns[CColGrp].Width / _uiScale)).ToString();
                w[2] = ((int)(dgv.Columns[CColName].Width / _uiScale)).ToString();
                // 路径列是 Fill，不存
                w[3] = ((int)(dgv.Columns[CColSize].Width / _uiScale)).ToString();
                w[4] = ((int)(dgv.Columns[CColTime].Width / _uiScale)).ToString();
                w[5] = ((int)(dgv.Columns[CColCreated].Width / _uiScale)).ToString();
                w[6] = ((int)(dgv.Columns[CColNote].Width / _uiScale)).ToString();
                Settings.ColWidths = string.Join(",", w);

                string[] v = new string[8];
                for (int i = 0; i < 8; i++) v[i] = dgv.Columns[i].Visible ? "1" : "0";
                Settings.ColVisible = string.Join(",", v);
                Settings.ShowCreated = dgv.Columns[CColCreated].Visible;
            }
            catch { }
        }

        // ---------- 列顺序（拖列头换位置） ----------
        // 存法：按“显示位置 0,1,2…”记录每一格放的是原始第几列，例如 2,1,0,3,4,5,6,7
        private void SaveColumnOrder()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            try
            {
                string[] order = new string[dgv.Columns.Count];
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    DataGridViewColumn c = dgv.Columns[i];
                    int pos = c.DisplayIndex;
                    if (pos < 0 || pos >= order.Length) return;
                    order[pos] = i.ToString();
                }
                Settings.ColOrder = string.Join(",", order);
            }
            catch { }
        }

        private void ApplyColumnOrder()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            string saved = Settings.ColOrder;
            if (string.IsNullOrEmpty(saved)) return;

            string[] parts = saved.Split(',');
            if (parts.Length != dgv.Columns.Count) return;

            // 必须是一个合法排列（0..n-1 各出现一次），否则宁可不还原
            bool[] seen = new bool[dgv.Columns.Count];
            int[] order = new int[dgv.Columns.Count];
            for (int i = 0; i < parts.Length; i++)
            {
                int v;
                if (!int.TryParse(parts[i], out v)) return;
                if (v < 0 || v >= dgv.Columns.Count) return;
                if (seen[v]) return;
                seen[v] = true;
                order[i] = v;
            }

            _applyingColOrder = true;
            try
            {
                for (int pos = 0; pos < order.Length; pos++)
                {
                    if (dgv.Columns[order[pos]].DisplayIndex != pos)
                        dgv.Columns[order[pos]].DisplayIndex = pos;
                }
            }
            catch { }
            finally
            {
                _applyingColOrder = false;
            }
        }

        // 视图 → 恢复默认列序：删除 / 组 / 文件名 / 大小 / 修改时间 / 状态 / 创建时间 / 完整路径
        private void ResetColumnOrder()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            ApplyDefaultColumnOrder();
            Settings.ColOrder = "";
            Settings.Save();
            lblStatus.Text = Lang.T("已恢复默认列序。");
        }

        // 把列摆回默认顺序（删除 / 组 / 文件名 / 大小 / 修改时间 / 状态 / 创建时间 / 完整路径）
        private void ApplyDefaultColumnOrder()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            _applyingColOrder = true;
            try
            {
                for (int pos = 0; pos < DefaultColOrderIdx.Length; pos++)
                {
                    int ci = DefaultColOrderIdx[pos];
                    if (dgv.Columns[ci].DisplayIndex != pos) dgv.Columns[ci].DisplayIndex = pos;
                }
            }
            catch { }
            finally
            {
                _applyingColOrder = false;
            }
        }

        private void ResetColumnWidths()
        {
            if (dgv == null || dgv.Columns.Count < 8) return;
            Settings.ColWidths = "";
            Settings.ColVisible = "";
            for (int i = 0; i < 8; i++)
            {
                if (i == CColPath) continue;
                dgv.Columns[i].Width = (int)(DefaultColW[i] * _uiScale);
                dgv.Columns[i].Visible = true;
            }
            dgv.Columns[CColCreated].Visible = Settings.ShowCreated;
            SaveColumnPrefs();
            Settings.Save();
            SyncColumnMenu();
            lblStatus.Text = Lang.T("已恢复默认列宽。");
        }

        // 视图 → 列显示 的勾选状态与实际列显隐同步
        private void SyncColumnMenu()
        {
            if (_miColToggles == null || dgv == null) return;
            for (int i = 0; i < _miColToggles.Length; i++)
            {
                if (_miColToggles[i] == null) continue;
                int ci = i + 1;   // 跳过勾选列
                if (ci >= dgv.Columns.Count) continue;
                _miColToggles[i].Checked = dgv.Columns[ci].Visible;
            }
        }

        private void ToggleColumn(int columnIndex, bool visible)
        {
            if (dgv == null || columnIndex < 0 || columnIndex >= dgv.Columns.Count) return;
            // 勾选列、路径列、状态列是操作核心，禁止隐藏
            if (!visible && (columnIndex == CColDel || columnIndex == CColPath || columnIndex == CColNote))
            {
                lblStatus.Text = Lang.T("这一列是操作必需的，不能隐藏。");
                SyncColumnMenu();
                return;
            }
            dgv.Columns[columnIndex].Visible = visible;
            SaveColumnPrefs();
            Settings.Save();
            SyncColumnMenu();
        }

        private void RebuildGrid()
        {
            if (!_skipPersist) PersistCheckStates();
            dgv.SuspendLayout();
            try
            {
                dgv.Rows.Clear();
                if (_groups.Count == 0)
                {
                    dgv.ResumeLayout();
                    UpdateSummary();
                    return;
                }

                List<DupGroup> ordered = SortedGroups();
                for (int gi = 0; gi < ordered.Count; gi++)
                {
                    DupGroup g = ordered[gi];
                    g.Id = gi + 1;
                    SortFilesInGroup(g);
                    for (int fi = 0; fi < g.Files.Count; fi++)
                    {
                        FileItem f = g.Files[fi];
                        int ri = dgv.Rows.Add();
                        DataGridViewRow row = dgv.Rows[ri];
                        row.Cells[CColDel].Value = f.SelectedForDelete;
                        row.Cells[CColGrp].Value = g.Id;
                        row.Cells[CColName].Value = f.FileName;
                        row.Cells[CColPath].Value = f.FullPath;
                        row.Cells[CColPath].ToolTipText = f.FullPath;
                        row.Cells[CColName].ToolTipText = f.FullPath;
                        row.Cells[CColSize].Value = f.SizeText;
                        row.Cells[CColTime].Value = f.ModifiedText;
                        row.Cells[CColCreated].Value = f.CreatedText;
                        string note;
                        if (f.SelectedForDelete) note = Lang.T("待删除");
                        else if (f.Note == "保留这份")
                            note = g.Similar ? Lang.F("保留 · 相似 {0}%", f.Similarity) : Lang.T("保留这份");
                        else if (g.Similar) note = Lang.F("相似 {0}%", f.Similarity);
                        else note = Lang.T(f.Note);
                        row.Cells[CColNote].Value = note;
                        row.Tag = f;
                        if ((gi % 2) == 1) row.DefaultCellStyle.BackColor = CBand;
                        row.Cells[CColNote].Style.ForeColor = f.SelectedForDelete ? CDanger : (f.Note == "保留这份" ? CGood : CSubText);
                    }
                }
            }
            finally
            {
                dgv.ResumeLayout();
            }
            UpdateSummary();
            UpdatePathDetail();
            LoadPreview();
        }

        private void UpdatePathDetail()
        {
            if (lblPathDetail == null || dgv == null) return;
            try
            {
                DataGridViewRow row = dgv.CurrentRow;
                FileItem f = (row != null) ? (row.Tag as FileItem) : null;
                if (f == null)
                {
                    lblPathDetail.Text = Lang.T("选中某一行，这里会显示该文件的完整路径（点击可复制）");
                }
                else
                {
                    lblPathDetail.Text = Lang.F("完整路径：{0}    （点击复制）", f.FullPath);
                }
            }
            catch
            {
            }
        }

        private void UpdateSummary()
        {            int groupCount = _groups.Count;
            int fileCount = 0;
            long wasted = 0;
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                fileCount += g.Files.Count;
                if (g.Files.Count > 1) wasted += g.Size * (long)(g.Files.Count - 1);
            }

            int selCount = 0;
            long selBytes = 0;
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow row = dgv.Rows[i];
                FileItem f = row.Tag as FileItem;
                if (f == null) continue;
                object v = row.Cells[0].Value;
                if ((v is bool) && (bool)v)
                {
                    selCount++;
                    selBytes += f.Length;
                }
            }

            if (groupCount == 0 || dgv.Rows.Count == 0)
            {
                // 扫完了但一个重复都没有：也要给一句明确结论，不能只留“暂无扫描结果”
                if (groupCount == 0 && _lastSummary != null)
                {
                    lblSummary.Text = Lang.F("未发现重复文件 · 共检查 {0} 个文件 · 用时 {1:F1} 秒",
                        Fmt.Number(_lastSummary.FilesScanned), _lastSummary.Seconds);
                }
                else
                {
                    lblSummary.Text = groupCount == 0
                        ? Lang.T("暂无扫描结果")
                        : Lang.T("当前筛选下没有结果，可切换“表格”查看其他结果");
                }
                return;
            }

            string line = Lang.F("重复组 {0} · 文件 {1} · 可释放 {2} · 已勾 {3}",
                Fmt.Number(groupCount), Fmt.Number(fileCount), Fmt.Size(wasted), Fmt.Number(selCount));
            lblSummary.Text = line;
        }

        // ============================================================
        //  1.4 选择助手：多规则 + 预览
        // ============================================================
        private List<FileItem> PickKeepers(DupGroup g, int rule, int keepCount)
        {
            List<FileItem> list = new List<FileItem>(g.Files);
            if (rule == 0)
                list.Sort(delegate(FileItem a, FileItem b) { return b.Modified.CompareTo(a.Modified); });
            else if (rule == 1)
                list.Sort(delegate(FileItem a, FileItem b) { return a.Modified.CompareTo(b.Modified); });
            else if (rule == 2)
            {
                list.Sort(delegate(FileItem a, FileItem b)
                {
                    int c = a.FullPath.Length.CompareTo(b.FullPath.Length);
                    if (c != 0) return c;
                    return string.Compare(a.FullPath, b.FullPath, StringComparison.OrdinalIgnoreCase);
                });
            }
            else if (rule == 3)
            {
                list.Sort(delegate(FileItem a, FileItem b)
                {
                    int c = a.FileName.Length.CompareTo(b.FileName.Length);
                    if (c != 0) return c;
                    return string.Compare(a.FileName, b.FileName, StringComparison.OrdinalIgnoreCase);
                });
            }
            else
            {
                Dictionary<string, long> areas = new Dictionary<string, long>();
                for (int i = 0; i < list.Count; i++)
                {
                    ImageMeta m = MetaOf(list[i]);
                    long area = (m != null) ? (long)m.Width * m.Height : 0;
                    areas[list[i].FullPath] = area;
                }
                list.Sort(delegate(FileItem a, FileItem b)
                {
                    long pa, pb;
                    areas.TryGetValue(a.FullPath, out pa);
                    areas.TryGetValue(b.FullPath, out pb);
                    if (pa != pb) return pb.CompareTo(pa);
                    return b.Length.CompareTo(a.Length);
                });
            }
            int n = keepCount;
            if (n < 1) n = 1;
            if (n > list.Count) n = list.Count;
            return list.GetRange(0, n);
        }

        private bool InAssistantScope(DupGroup g, int scope)
        {
            if (scope == 1) return !g.Similar;
            if (scope == 2) return g.Similar;
            return true;
        }

        private void CountAssistant(int rule, int keep, int scope, out int groupCount, out int keepCount, out int delCount, out long delBytes)
        {
            groupCount = 0;
            keepCount = 0;
            delCount = 0;
            delBytes = 0;
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                if (!InAssistantScope(g, scope)) continue;
                groupCount++;
                List<FileItem> keepers = PickKeepers(g, rule, keep);
                keepCount += keepers.Count;
                for (int j = 0; j < g.Files.Count; j++)
                {
                    if (keepers.Contains(g.Files[j])) continue;
                    delCount++;
                    delBytes += g.Files[j].Length;
                }
            }
        }

        private void ApplyAssistant(int rule, int keep, int scope)
        {
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                if (!InAssistantScope(g, scope)) continue;
                List<FileItem> keepers = PickKeepers(g, rule, keep);
                for (int j = 0; j < g.Files.Count; j++)
                {
                    FileItem f = g.Files[j];
                    bool isKeep = keepers.Contains(f);
                    f.SelectedForDelete = !isKeep;
                    f.Note = isKeep ? "保留这份" : "重复";
                }
            }
        }

        private void ShowSelectionAssistant()
        {
            if (_groups.Count == 0)
            {
                MessageBox.Show(this, Lang.T("还没有扫描结果，选择助手要在扫描之后用。"), Lang.T("选择助手"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("选择助手");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(492, 508);
            f.Load += delegate { Dialogs.Dpi(f); };

            Label lbRule = Dialogs.Lbl(Lang.T("保留规则"), 9.5F, true, Theme.TextC);
            lbRule.Location = new Point(20, 16);

            string[] ruleNames = new string[] { "最新修改的那份", "最早修改的那份", "路径最短的那份", "文件名最短的那份", "分辨率最高的那份" };
            RadioButton[] radios = new RadioButton[ruleNames.Length];
            for (int i = 0; i < ruleNames.Length; i++)
            {
                RadioButton rb = new RadioButton();
                rb.Text = Lang.T(ruleNames[i]);
                rb.AutoSize = true;
                rb.Location = new Point(24 + (i % 2) * 236, 42 + (i / 2) * 26);
                rb.ForeColor = Theme.TextC;
                rb.BackColor = Theme.Card;
                rb.Font = new Font(Settings.UiFont(), 9F);
                rb.Tag = i;
                if (i == 1) rb.Checked = true;
                radios[i] = rb;
                f.Controls.Add(rb);
            }

            Label lbKeep = Dialogs.Lbl(Lang.T("每组保留份数"), 9.5F, true, Theme.TextC);
            lbKeep.Location = new Point(20, 124);

            NumericUpDown num = new NumericUpDown();
            num.Location = new Point(24, 150);
            num.Size = new Size(72, 26);
            num.Font = new Font(Settings.UiFont(), 9.5F);
            num.Minimum = 1M;
            num.Maximum = 50M;
            num.Value = 1M;

            Label lbScope = Dialogs.Lbl(Lang.T("应用范围"), 9.5F, true, Theme.TextC);
            lbScope.Location = new Point(20, 190);

            RadioButton rbAll = new RadioButton();
            rbAll.Text = Lang.T("全部组");
            rbAll.AutoSize = true;
            rbAll.Location = new Point(24, 216);
            rbAll.Checked = true;
            rbAll.ForeColor = Theme.TextC;
            rbAll.BackColor = Theme.Card;
            rbAll.Font = new Font(Settings.UiFont(), 9F);

            RadioButton rbExact = new RadioButton();
            rbExact.Text = Lang.T("仅精确重复");
            rbExact.AutoSize = true;
            rbExact.Location = new Point(24, 240);
            rbExact.ForeColor = Theme.TextC;
            rbExact.BackColor = Theme.Card;
            rbExact.Font = new Font(Settings.UiFont(), 9F);

            RadioButton rbSimilar = new RadioButton();
            rbSimilar.Text = Lang.T("仅相似图片");
            rbSimilar.AutoSize = true;
            rbSimilar.Location = new Point(24, 264);
            rbSimilar.ForeColor = Theme.TextC;
            rbSimilar.BackColor = Theme.Card;
            rbSimilar.Font = new Font(Settings.UiFont(), 9F);

            Panel preview = new Panel();
            preview.Location = new Point(20, 292);
            preview.Size = new Size(444, 130);
            preview.BackColor = Theme.InputBg;
            preview.BorderStyle = BorderStyle.FixedSingle;

            Label lbPreview = new Label();
            lbPreview.AutoSize = false;
            lbPreview.Location = new Point(12, 10);
            lbPreview.Size = new Size(416, 110);
            lbPreview.ForeColor = Theme.TextC;
            lbPreview.BackColor = Theme.InputBg;
            lbPreview.Font = new Font(Settings.UiFont(), 9F);
            preview.Controls.Add(lbPreview);

            Button cancel = Dialogs.Btn(Lang.T("取消"), Theme.Neutral, Theme.TextC, 100, 34);
            cancel.Location = new Point(178, 444);
            cancel.DialogResult = DialogResult.Cancel;

            Button ok = Dialogs.Btn(Lang.T("应用勾选"), Theme.Accent, Theme.OnAccent, 150, 34);
            ok.Location = new Point(288, 444);

            EventHandler upd = delegate
            {
                int rule = 0;
                for (int i = 0; i < radios.Length; i++) if (radios[i].Checked) rule = (int)radios[i].Tag;
                int keep = (int)num.Value;
                int scope = rbSimilar.Checked ? 2 : (rbExact.Checked ? 1 : 0);
                int gc, kc, dc;
                long db;
                CountAssistant(rule, keep, scope, out gc, out kc, out dc, out db);
                lbPreview.Text = Lang.F("参与分组：{0} 组", gc.ToString())
                    + "\r\n"
                    + Lang.F("保留 {0} 个文件，勾选删除 {1} 个", kc.ToString(), dc.ToString())
                    + "\r\n"
                    + Lang.F("预计释放约 {0}", Fmt.Size(db))
                    + "\r\n\r\n"
                    + Lang.T("勾选后不会立刻删除，确认无误再点“删除到回收站”。");
            };
            for (int i = 0; i < radios.Length; i++) radios[i].CheckedChanged += upd;
            rbAll.CheckedChanged += upd;
            rbExact.CheckedChanged += upd;
            rbSimilar.CheckedChanged += upd;
            num.ValueChanged += upd;

            ok.Click += delegate
            {
                int rule = 0;
                for (int i = 0; i < radios.Length; i++) if (radios[i].Checked) rule = (int)radios[i].Tag;
                int keep = (int)num.Value;
                int scope = rbSimilar.Checked ? 2 : (rbExact.Checked ? 1 : 0);
                ApplyAssistant(rule, keep, scope);
                Settings.KeepRule = rule;
                Settings.Save();
                f.DialogResult = DialogResult.OK;
                f.Close();
            };

            f.Controls.Add(lbRule);
            for (int i = 0; i < radios.Length; i++) f.Controls.Add(radios[i]);
            f.Controls.Add(lbKeep);
            f.Controls.Add(num);
            f.Controls.Add(lbScope);
            f.Controls.Add(rbAll);
            f.Controls.Add(rbExact);
            f.Controls.Add(rbSimilar);
            f.Controls.Add(preview);
            f.Controls.Add(cancel);
            f.Controls.Add(ok);
            f.AcceptButton = ok;
            f.CancelButton = cancel;

            upd(null, EventArgs.Empty);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                _skipPersist = true;
                try
                {
                    RebuildGrid();
                }
                finally
                {
                    _skipPersist = false;
                }
                lblStatus.Text = Lang.T("选择助手已应用，结果已按规则勾选。");
            }
            f.Dispose();
        }

        // ============================================================
        //  1.4 扫描报告
        // ============================================================
        private string BuildReportText()
        {
            if (_lastSummary == null) return Lang.T("还没有扫描结果。");
            ScanSummary s = _lastSummary;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Lang.T("扫描报告"));
            sb.AppendLine("----------------------------------------");
            sb.AppendLine(Lang.F("扫描模式：{0}", Lang.T(s.ModeText)));
            sb.AppendLine(Lang.F("共扫描 {0} 个文件（{1}）", Fmt.Number(s.FilesScanned), Fmt.Size(s.TotalBytes)));
            sb.AppendLine(Lang.F("重复组：{0} 组（其中相似图片 {1} 组）", Fmt.Number(s.Groups.Count), Fmt.Number(s.SimilarGroups.Count)));
            sb.AppendLine(Lang.F("重复文件：{0} 项，可释放 {1}", Fmt.Number((int)s.DupFiles), Fmt.Size(s.WastedBytes)));
            sb.AppendLine(Lang.F("已计算图片指纹：{0} 张", Fmt.Number(s.HashedImages)));
            sb.AppendLine(Lang.F("读取失败跳过：{0} 项", Fmt.Number(s.Unreadable)));
            sb.AppendLine(Lang.F("总耗时：{0:F1} 秒", s.Seconds));
            return sb.ToString();
        }

        private void ShowScanReport()
        {
            if (_lastSummary == null)
            {
                MessageBox.Show(this, Lang.T("还没有扫描结果，先扫描一次吧。"), Lang.T("扫描报告"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Form f = new Form();
            Theme.ApplyTitleBar(f);
            f.Text = Lang.T("扫描报告");
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.StartPosition = FormStartPosition.CenterParent;
            f.AutoScaleMode = AutoScaleMode.None;
            f.BackColor = Theme.Card;
            f.ForeColor = Theme.TextC;
            f.Font = new Font(Settings.UiFont(), 9F);
            f.ClientSize = new Size(468, 404);
            f.Load += delegate { Dialogs.Dpi(f); };

            Label lbTitle = Dialogs.Lbl(Lang.T("扫描完成"), 14F, true, Theme.TextC);
            lbTitle.Location = new Point(20, 16);

            Label lbHint = Dialogs.Lbl(Lang.T("下面是本次扫描的统计，点“复制”可以贴到聊天或文档里。"), 8.5F, false, Theme.SubText);
            lbHint.Location = new Point(20, 50);
            lbHint.MaximumSize = new Size(428, 0);

            TextBox tb = new TextBox();
            tb.Multiline = true;
            tb.ReadOnly = true;
            tb.ScrollBars = ScrollBars.Vertical;
            tb.Location = new Point(20, 78);
            tb.Size = new Size(428, 234);
            tb.BackColor = Theme.InputBg;
            tb.ForeColor = Theme.TextC;
            tb.Font = new Font(Settings.UiFont(), 9F);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.Text = BuildReportText().Replace("\n", "\r\n");
            tb.SelectionStart = 0;
            tb.SelectionLength = 0;
            tb.TabStop = false;

            CheckBox chkAuto = new CheckBox();
            chkAuto.Text = Lang.T("以后每次扫描完成都自动显示这份统计");
            chkAuto.AutoSize = true;
            chkAuto.Location = new Point(20, 324);
            chkAuto.ForeColor = Theme.SubText;
            chkAuto.BackColor = Theme.Card;
            chkAuto.Font = new Font(Settings.UiFont(), 8.5F);
            chkAuto.Checked = Settings.AutoReport;

            Button copy = Dialogs.Btn(Lang.T("复制"), Theme.Neutral, Theme.TextC, 92, 32);
            copy.Location = new Point(236, 356);
            copy.Click += delegate
            {
                try
                {
                    Clipboard.SetText(tb.Text);
                    lblStatus.Text = Lang.T("统计已复制到剪贴板。");
                }
                catch { }
            };

            Button close = Dialogs.Btn(Lang.T("关闭"), Theme.Accent, Theme.OnAccent, 100, 32);
            close.Location = new Point(348, 356);
            close.DialogResult = DialogResult.OK;

            f.Controls.Add(lbTitle);
            f.Controls.Add(lbHint);
            f.Controls.Add(tb);
            f.Controls.Add(chkAuto);
            f.Controls.Add(copy);
            f.Controls.Add(close);
            f.AcceptButton = close;
            f.CancelButton = close;
            f.ShowDialog(this);
            Settings.AutoReport = chkAuto.Checked;
            Settings.Save();
            f.Dispose();
        }

        // ---------- 预览与参数对照 ----------
        private ImageMeta MetaOf(FileItem f)
        {
            if (f == null) return null;
            if (f.Meta != null) return f.Meta;
            ImageMeta m = _metaCache.ContainsKey(f.FullPath) ? _metaCache[f.FullPath] : null;
            if (m == null)
            {
                m = ImageHash.Read(f.FullPath, false);
                _metaCache[f.FullPath] = m;
            }
            return m;
        }

        private DupGroup CurrentGroup()
        {
            DataGridViewRow row = dgv.CurrentRow;
            if (row == null) return null;
            FileItem f = row.Tag as FileItem;
            if (f == null) return null;
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                for (int j = 0; j < g.Files.Count; j++)
                    if (g.Files[j] == f) return g;
            }
            return null;
        }

        private void TogglePreview()
        {
            if (_mainGrid == null) return;
            float f = _uiScale > 0 ? _uiScale : 1f;
            RowStyle rs = _mainGrid.RowStyles[2];
            bool collapsed = rs.Height < 100 * f;
            rs.Height = collapsed ? 272f * f : 40f * f;
            btnPreviewToggle.Text = Lang.T(collapsed ? "收起" : "展开");
            dgvExif.Visible = collapsed;
            pnlThumbs.Visible = collapsed;
            if (sbPreview != null) sbPreview.Visible = false;
            LayoutPreview();
            if (collapsed) LoadPreview();
            ScheduleGlass();
        }

        // ---------- 预览区布局：整组全摆，不裁不滚 ----------
        // 1) 先算「一行能摆几张」：每张至少 60（逻辑）宽，整组张数不超过上限 → 全部排一行；
        //    超过上限 → 自动换行（第二行、第三行…），永远不会出现“半张”。
        // 2) 每张宽度 = 一行可用宽度平均分，上限 160（逻辑）：张数少时图更大，张数多时自动收窄。
        // 3) 参数表不再挤在缩略图右边，改到缩略图下方、占满整宽，列宽等分 → 有几个文件就有几列，
        //    等分宽度永远塞得下，右侧信息一定完整（超宽只可能挤窄，不会裁掉）。
        // 缩略图按 4:3 显示（照片本来就是横的，方块等于把两边裁掉），文字压到 2~3 行
        private void CalcTileSize(int tileW, out int picW, out int picH, out int tileH, out bool compact)
        {
            float s = _uiScale > 0 ? _uiScale : 1f;
            picW = tileW;
            picH = (int)Math.Round(tileW * 0.72);
            int capH = (int)(176 * s);
            if (picH > capH) picH = capH;
            if (picH < (int)(36 * s)) picH = (int)(36 * s);
            // 格子窄时只留「名字 + 状态」两行，分辨率/大小交给下面整宽的参数表
            compact = tileW < (int)(150 * s);
            tileH = picH + ((compact) ? (int)(46 * s) : (int)(69 * s));
        }

        private void CalcPreviewLayout(int total, out int perRow, out int tileW, out int gap, out int rows)
        {
            float s = _uiScale > 0 ? _uiScale : 1f;
            gap = (int)(8 * s);
            int minTile = (int)(60 * s);      // 再窄就认不出是同一张图了
            int maxTile = (int)(160 * s);
            int w = (pnlPreview != null && pnlPreview.ClientSize.Width > 200) ? pnlPreview.ClientSize.Width : 900;
            int n = (total > 0) ? total : 1;
            int cap = (w + gap) / (minTile + gap);
            if (cap < 1) cap = 1;
            perRow = (n < cap) ? n : cap;
            int t = (w - (perRow - 1) * gap) / perRow;
            if (t > maxTile) t = maxTile;
            if (t < (int)(40 * s)) t = (int)(40 * s);
            tileW = t;
            rows = (n + perRow - 1) / perRow;
        }

        // 预览条需要多高：一行行的缩略图 + 下方的参数表
        private int PreviewNeedHeight(int total)
        {
            int perRow, tileW, gap, rows, picW, picH, tileH;
            bool compact;
            CalcPreviewLayout(total, out perRow, out tileW, out gap, out rows);
            CalcTileSize(tileW, out picW, out picH, out tileH, out compact);
            int rowH = (dgvExif != null) ? dgvExif.RowTemplate.Height : 24;
            int attrRows = (_previewGroup != null && _previewGroup.Similar) ? 8 : 7;
            int tableRows = ((total > 0) && total >= 15) ? total : attrRows;
            int tableH = 30 + tableRows * rowH + 8;
            int thumbsH = rows * tileH + (rows - 1) * gap;
            return 36 + thumbsH + 14 + tableH + 6;
        }

        // 窗口宽度变了要不要重建缩略图（尺寸变了就得重画）
        private bool NeedRetile()
        {
            DupGroup g = _previewGroup;
            if (g == null || g.Files.Count < 1) return false;
            int perRow, tileW, gap, rows, picW, picH, tileH;
            bool compact;
            CalcPreviewLayout(g.Files.Count, out perRow, out tileW, out gap, out rows);
            CalcTileSize(tileW, out picW, out picH, out tileH, out compact);
            return (perRow != _previewPerRow || compact != _previewCompact || Math.Abs(tileW - _previewTileW) > 2);
        }

        private bool _layingPreview = false;

        private void LayoutPreview()
        {
            if (pnlPreview == null || pnlThumbs == null) return;
            if (_layingPreview) return;
            _layingPreview = true;
            try
            {
                int w = pnlPreview.ClientSize.Width;
                if (btnPreviewToggle != null) btnPreviewToggle.Left = w - btnPreviewToggle.Width - 2;

                DupGroup g = _previewGroup;
                int total = (g == null) ? 0 : g.Files.Count;
                int perRow, tileW, gap, rows, picW, picH, tileH;
                bool compact;
                CalcPreviewLayout(total, out perRow, out tileW, out gap, out rows);
                CalcTileSize(tileW, out picW, out picH, out tileH, out compact);
                _previewPerRow = perRow;
                _previewRows = rows;
                _previewTileW = tileW;
                _previewTileH = tileH;
                _previewGap = gap;
                _previewStep = tileW + gap;
                _previewCols = perRow;
                _previewCompact = compact;

                // 预览条高度跟着内容走；同时保证主表格至少能看见 3~10 行（不至于被挤没）
                int needH = PreviewNeedHeight(total);
                if (_mainGrid != null && _mainGrid.ClientSize.Height > 320)
                {
                    float ff = _uiScale > 0 ? _uiScale : 1f;
                    int rowsMain = (dgv != null) ? dgv.Rows.Count : 3;
                    if (rowsMain < 3) rowsMain = 3;
                    if (rowsMain > 10) rowsMain = 10;
                    int minTableH = 34 + rowsMain * 34;
                    int capH = _mainGrid.ClientSize.Height - (int)((58 + 32 + 70) * ff) - minTableH;
                    if (capH < (int)(120 * ff)) capH = (int)(120 * ff);
                    if (needH > capH) needH = capH;
                    RowStyle rs = _mainGrid.RowStyles[2];
                    if (rs.Height >= 100 * ff && Math.Abs(rs.Height - needH) > 1)
                    {
                        rs.Height = needH;
                        _mainGrid.PerformLayout();
                        pnlPreview.PerformLayout();
                    }
                }

                int thumbsH = rows * tileH + (rows - 1) * gap;
                pnlThumbs.Left = 2;
                pnlThumbs.Top = 36;
                pnlThumbs.Width = Math.Max(60, w - 4);
                pnlThumbs.Height = thumbsH;
                pnlThumbs.AutoScrollMinSize = new Size(0, thumbsH);

                dgvExif.Left = 2;
                dgvExif.Top = 36 + thumbsH + 14;
                dgvExif.Width = Math.Max(120, w - 4);
                int restH = pnlPreview.ClientSize.Height - dgvExif.Top - 6;
                dgvExif.Height = (restH < 70) ? 70 : restH;

                if (sbPreview != null && sbPreview.Visible) sbPreview.Visible = false;

                for (int i = 0; i < pnlThumbs.Controls.Count; i++)
                {
                    Control c = pnlThumbs.Controls[i];
                    if (!(c.Tag is int)) continue;
                    int idx = (int)c.Tag;
                    int r = idx / perRow;
                    int cc = idx % perRow;
                    // 不满一行时整行居中，别让右边空一大片
                    int cnt = (total > 0 ? total : 1) - r * perRow;
                    if (cnt > perRow) cnt = perRow;
                    if (cnt < 1) cnt = 1;
                    int rowW = cnt * tileW + (cnt - 1) * gap;
                    int x0 = (w - 4 - rowW) / 2;
                    if (x0 < 2) x0 = 2;
                    Point np = new Point(x0 + cc * (tileW + gap), r * (tileH + gap));
                    if (c.Location != np) c.Location = np;
                    if (c.Width != tileW || c.Height != tileH) c.Size = new Size(tileW, tileH);
                }
            }
            finally
            {
                _layingPreview = false;
            }
        }

        // 整组全摆进面板了，不再需要横向滚动条
        private void SyncPreviewScrollbar()
        {
            if (sbPreview != null && sbPreview.Visible) sbPreview.Visible = false;
        }

        // 缩略图位置由 LayoutPreview 统一排；这里只负责“参数表按整组重建一次”
        private void ApplyPreviewScroll()
        {
            if (pnlThumbs == null) return;
            DupGroup g = _previewGroup;
            if (g == null) return;
            int total = g.Files.Count;
            if (total < 1) return;
            LayoutPreview();
            if (_previewShownCols != total)
            {
                _previewShownCols = total;
                BuildExifTable(g, 0);
                UpdatePreviewHint(g, 0);
            }
        }

        // 顶部说明：第 N 组 · X 个相同文件（张数多了也只多一句“共几张”）
        private void UpdatePreviewHint(DupGroup g, int first)
        {
            if (lblPreviewHint == null || g == null) return;
            string head = g.Similar
                ? Lang.F("第 {0} 组 · 相似 {1}% · {2} 个文件", g.Id.ToString(), g.Similarity.ToString(), g.Files.Count.ToString())
                : Lang.F("第 {0} 组 · {1} 个相同文件", g.Id.ToString(), g.Files.Count.ToString());
            lblPreviewHint.Text = head;
        }

        // 已经没有横向滚动了：滚轮保留给表格本身
        private void NudgePreviewScroll(int notches)
        {
        }

        private bool PreviewWheelHit()
        {
            return false;
        }

        private class PreviewWheelFilter : IMessageFilter
        {
            private MainForm _owner;
            public PreviewWheelFilter(MainForm owner) { _owner = owner; }
            public bool PreFilterMessage(ref Message m)
            {
                return false;
            }
        }

        private void LoadPreview()
        {
            if (pnlPreview == null || pnlThumbs == null) return;
            DupGroup g = CurrentGroup();
            _previewGroup = g;
            _previewScrollX = 0;
            _previewFirstIdx = -1;
            _previewShownCols = -1;
            pnlThumbs.SuspendLayout();
            try
            {
                for (int i = pnlThumbs.Controls.Count - 1; i >= 0; i--)
                {
                    Control c = pnlThumbs.Controls[i];
                    pnlThumbs.Controls.RemoveAt(i);
                    c.Dispose();
                }
                if (dgvExif.Columns.Count > 0) dgvExif.Columns.Clear();
                dgvExif.Rows.Clear();

                if (g == null)
                {
                    lblPreviewHint.Text = Lang.T("选中一组，这里并排显示缩略图和拍摄参数");
                    Label empty = new Label();
                    empty.Text = Lang.T("在表格里选中一行，即可在这里预览并对照参数");
                    empty.AutoSize = false;
                    empty.Size = new Size(400, 40);
                    empty.Location = new Point(8, 46);
                    empty.ForeColor = CSubText;
                    empty.BackColor = CCard;
                    empty.Font = new Font(UiFont, 9F);
                    pnlThumbs.Controls.Add(empty);
                    LayoutPreview();
                    return;
                }

                string head = g.Similar
                    ? Lang.F("第 {0} 组 · 相似 {1}% · {2} 个文件", g.Id.ToString(), g.Similarity.ToString(), g.Files.Count.ToString())
                    : Lang.F("第 {0} 组 · {1} 个相同文件", g.Id.ToString(), g.Files.Count.ToString());
                lblPreviewHint.Text = head;

                int total = g.Files.Count;
                float s = _uiScale > 0 ? _uiScale : 1f;
                // 整组全摆：张数决定格子宽度，放不下才换行；不会只显示一部分
                int perRow, tileW, gap, rows, picW, picH, tileH;
                bool compact;
                CalcPreviewLayout(total, out perRow, out tileW, out gap, out rows);
                CalcTileSize(tileW, out picW, out picH, out tileH, out compact);
                _previewPerRow = perRow;
                _previewRows = rows;
                _previewTileW = tileW;
                _previewTileH = tileH;
                _previewGap = gap;
                _previewStep = tileW + gap;
                _previewCols = perRow;
                _previewCompact = compact;

                ToolTip tip = new ToolTip();
                tip.AutoPopDelay = 12000;
                tip.InitialDelay = 400;
                for (int i = 0; i < total; i++)
                {
                    FileItem f = g.Files[i];
                    Panel tile = new Panel();
                    tile.Size = new Size(tileW, tileH);
                    tile.Location = new Point((i % perRow) * (tileW + gap), (i / perRow) * (tileH + gap));
                    tile.Tag = i;
                    tile.Margin = new Padding(0);
                    tile.BackColor = CCard;
                    tile.Cursor = Cursors.Hand;
                    tip.SetToolTip(tile, f.FullPath);
                    FileItem tileItem = f;
                    tile.DoubleClick += delegate { OpenPath(tileItem); };

                    PictureBox pic = new PictureBox();
                    pic.Size = new Size(picW, picH);
                    pic.Location = new Point(0, 0);
                    pic.BackColor = CInputBg;
                    pic.SizeMode = PictureBoxSizeMode.CenterImage;
                    pic.BorderStyle = BorderStyle.FixedSingle;
                    Bitmap bmp = ThumbOf(f.FullPath, picW - 2, picH - 2);
                    if (bmp != null) pic.Image = bmp;
                    else
                    {
                        pic.Image = ExtTile(f.FullPath, picW - 2, picH - 2);
                        pic.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                    tile.Controls.Add(pic);

                    Label nm = new Label();
                    nm.AutoSize = false;
                    nm.Size = new Size(tileW, (int)(20 * s));
                    nm.Location = new Point(0, picH + (int)(3 * s));
                    nm.AutoEllipsis = true;
                    nm.ForeColor = CText;
                    nm.BackColor = CCard;
                    nm.Font = new Font(UiFont, 8F * s);
                    // 长文件名用“中间省略”，把能区分副本的尾巴（…-01.jpg）留住；格子窄时前面带上序号
                    nm.Text = FitFileName(compact ? ((i + 1).ToString() + ". " + f.FileName) : f.FileName, nm.Font, tileW);
                    tile.Controls.Add(nm);

                    if (!compact)
                    {
                        Label mt = new Label();
                        ImageMeta m = MetaOf(f);
                        string dim = (m != null && m.DimText.Length > 0) ? m.DimText : Lang.T("—");
                        Font mf = new Font(UiFont, 7.5F * s);
                        // 宽度够就「分辨率 · 大小」一行，不够就只留分辨率（大小下面表格里有）
                        string meta = dim + "  ·  " + f.SizeText;
                        if (TextRenderer.MeasureText(meta, mf).Width > tileW - 4) meta = dim;
                        mt.Text = meta;
                        mt.AutoSize = false;
                        mt.Size = new Size(tileW, (int)(20 * s));
                        mt.Location = new Point(0, picH + (int)(25 * s));
                        mt.AutoEllipsis = true;
                        mt.ForeColor = CSubText;
                        mt.BackColor = CCard;
                        mt.Font = mf;
                        tile.Controls.Add(mt);
                    }

                    Label sz = new Label();
                    bool keeper = (f.Note == "保留这份");
                    string statusText = f.SelectedForDelete ? Lang.T("待删除") : (keeper ? Lang.T("保留这份") : Lang.T("重复"));
                    sz.Text = statusText;
                    sz.AutoSize = false;
                    sz.Size = new Size(tileW, (int)(20 * s));
                    sz.Location = new Point(0, picH + (int)((compact ? 24 : 47) * s));
                    sz.AutoEllipsis = true;
                    sz.ForeColor = f.SelectedForDelete ? CDanger : (keeper ? CGood : CSubText);
                    sz.BackColor = CCard;
                    sz.Font = new Font(UiFont, 8F * s);
                    tile.Controls.Add(sz);
                    pnlThumbs.Controls.Add(tile);
                }

                LayoutPreview();
                ApplyPreviewScroll();   // 参数表按整组一次建好（有几个文件就有几列）
            }
            finally
            {
                pnlThumbs.ResumeLayout();
                if (Theme.Mode == Theme.Custom && Theme.Glass)
                {
                    GlassTextTransparent(pnlPreview);
                    ScheduleGlass();
                }
            }
        }


        // 文件名太长时用“中间省略”（例如 D4E9…3101.jpg），保证副本之间最关键的尾部还能看见
        private string FitFileName(string name, Font font, int maxW)
        {
            if (string.IsNullOrEmpty(name)) return "";
            if (maxW < 12) return name;
            if (TextRenderer.MeasureText(name, font).Width <= maxW) return name;
            for (int keep = name.Length - 1; keep >= 6; keep--)
            {
                int head = keep / 3;                 // 开头少留一点
                int tail = keep - head;              // 尾巴多留一点（副本序号、扩展名都在尾巴上）
                string cand = name.Substring(0, head) + "…" + name.Substring(name.Length - tail);
                if (TextRenderer.MeasureText(cand, font).Width <= maxW) return cand;
            }
            return name;
        }

        // 参数对照表：第一列参数名（固定不动），后面按当前看得见的文件各占一列
        // 参数对照表：第一列参数名，后面整组每个文件一列；列宽 Fill 等分整宽，绝不裁列
        private string AttrLabel(int a)
        {
            if (a == 0) return Lang.T("分辨率");
            if (a == 1) return Lang.T("位深 · 色域");
            if (a == 2) return Lang.T("文件大小");
            if (a == 3) return Lang.T("拍摄时间");
            if (a == 4) return Lang.T("相机");
            if (a == 5) return Lang.T("镜头");
            if (a == 6) return Lang.T("修改时间");
            return Lang.T("画面相似度");
        }

        private string AttrValue(FileItem f, int a)
        {
            ImageMeta m = MetaOf(f);
            string dash = Lang.T("—");
            if (a == 0) return (m != null && m.DimText.Length > 0) ? m.DimText : dash;
            if (a == 1)
            {
                if (m == null || (m.Bits <= 0 && m.ColorSpace.Length == 0)) return dash;
                string bits = m.Bits > 0 ? m.Bits.ToString() + " bit" : "";
                string cs = m.ColorSpace;
                if (bits.Length > 0 && cs.Length > 0) return bits + " · " + cs;
                return bits.Length > 0 ? bits : cs;
            }
            if (a == 2) return f.SizeText;
            if (a == 3) return (m != null && m.TakenText.Length > 0) ? m.TakenText : dash;
            if (a == 4) return (m != null && m.CameraText.Length > 0) ? m.CameraText : dash;
            if (a == 5) return (m != null && m.Lens.Length > 0) ? m.Lens : dash;
            if (a == 6) return f.Modified.ToString("MM-dd HH:mm");
            return f.Similarity.ToString() + "%";
        }

        // 参数对照表。文件不多时：行=参数、列=文件；文件很多（≥15）时转置成
        // “一行一个文件、列=参数”，列宽仍然宽到能读，行数多了就在表内竖直滚动，不裁任何值。
        private void BuildExifTable(DupGroup g, int first)
        {
            if (dgvExif == null) return;
            dgvExif.SuspendLayout();
            try
            {
                dgvExif.Columns.Clear();
                dgvExif.Rows.Clear();
                if (g == null) return;

                float s = _uiScale > 0 ? _uiScale : 1f;
                int total = g.Files.Count;
                if (total < 1) return;
                int attrCount = g.Similar ? 8 : 7;
                bool asRows = total >= 15;

                if (!asRows)
                {
                    DataGridViewTextBoxColumn col0 = new DataGridViewTextBoxColumn();
                    col0.HeaderText = Lang.T("参数");
                    col0.SortMode = DataGridViewColumnSortMode.NotSortable;
                    col0.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col0.Width = (int)(92 * s);
                    dgvExif.Columns.Add(col0);
                    for (int k = 0; k < total; k++)
                    {
                        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                        col.HeaderText = Lang.T("文件") + " " + (k + 1).ToString();
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        col.MinimumWidth = (int)(26 * s);
                        dgvExif.Columns.Add(col);
                    }
                    for (int a = 0; a < attrCount; a++)
                    {
                        List<string> cells = new List<string>();
                        cells.Add(AttrLabel(a));
                        for (int k = 0; k < total; k++) cells.Add(AttrValue(g.Files[k], a));
                        AddExifRow(cells);
                    }
                }
                else
                {
                    DataGridViewTextBoxColumn c0 = new DataGridViewTextBoxColumn();
                    c0.HeaderText = Lang.T("文件");
                    c0.SortMode = DataGridViewColumnSortMode.NotSortable;
                    c0.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    c0.Width = (int)(160 * s);
                    dgvExif.Columns.Add(c0);
                    for (int a = 0; a < attrCount; a++)
                    {
                        DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                        col.HeaderText = AttrLabel(a);
                        col.SortMode = DataGridViewColumnSortMode.NotSortable;
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        col.MinimumWidth = (int)(64 * s);
                        dgvExif.Columns.Add(col);
                    }
                    for (int k = 0; k < total; k++)
                    {
                        List<string> cells = new List<string>();
                        cells.Add((k + 1).ToString() + ". " + g.Files[k].FileName);
                        for (int a = 0; a < attrCount; a++) cells.Add(AttrValue(g.Files[k], a));
                        AddExifRow(cells);
                    }
                }
                dgvExif.ClearSelection();
            }
            finally
            {
                dgvExif.ResumeLayout();
            }
        }



        private void AddExifRow(List<string> cells)
        {
            int ri = dgvExif.Rows.Add();
            for (int i = 0; i < cells.Count && i < dgvExif.Columns.Count; i++)
            {
                dgvExif.Rows[ri].Cells[i].Value = cells[i];
                if (i == 0) dgvExif.Rows[ri].Cells[i].Style.ForeColor = CSubText;
            }
        }

        private Bitmap ThumbOf(string path, int w, int h)
        {
            string key = path + "|" + w + "x" + h;
            if (_thumbCache.ContainsKey(key)) return _thumbCache[key];
            if (_thumbCache.Count > 260) ClearThumbCache();
            Bitmap bmp = ImageHash.Thumb(path, w, h, CInputBg);
            if (bmp != null) _thumbCache[key] = bmp;
            return bmp;
        }

        private void ClearThumbCache()
        {
            foreach (KeyValuePair<string, Bitmap> kv in _thumbCache)
            {
                try { kv.Value.Dispose(); } catch { }
            }
            _thumbCache.Clear();
        }

        // 非图片文件用“扩展名色块”占位
        private Bitmap ExtTile(string path, int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using (Graphics gr = Graphics.FromImage(bmp))
            {
                gr.Clear(CInputBg);
                string ext = System.IO.Path.GetExtension(path);
                ext = (ext == null || ext.Length == 0) ? "FILE" : ext.Substring(1).ToUpperInvariant();
                if (ext.Length > 5) ext = ext.Substring(0, 5);
                using (Font font = new Font(UiFont, ext.Length > 3 ? 12F : 16F, FontStyle.Bold))
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    gr.DrawString(ext, font, new SolidBrush(CSubText), new RectangleF(0, 0, w, h), sf);
                }
            }
            return bmp;
        }

        private void SyncSortCombo()
        {
            _syncing = true;
            cboSort.SetIndexSilent(_sortMode);
            _syncing = false;
        }

        private void SetAllChecked(bool checkedValue)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                DataGridViewRow row = dgv.Rows[i];
                FileItem f = row.Tag as FileItem;
                if (f == null) continue;
                row.Cells[0].Value = checkedValue;
                f.SelectedForDelete = checkedValue;
                row.Cells[CColNote].Value = Lang.T(checkedValue ? "待删除" : f.Note);
                row.Cells[CColNote].Style.ForeColor = checkedValue ? CDanger : (f.Note == "保留这份" ? CGood : CSubText);
            }
            UpdateSummary();
        }

        // ============================================================
        //  行操作
        // ============================================================
        private FileItem CurrentItem()
        {
            if (dgv.CurrentRow == null) return null;
            return dgv.CurrentRow.Tag as FileItem;
        }

        private DupGroup GroupOf(FileItem item)
        {
            if (item == null) return null;
            for (int i = 0; i < _groups.Count; i++)
            {
                if (_groups[i].Files.Contains(item)) return _groups[i];
            }
            return null;
        }

        private void OpenPath(FileItem f)
        {
            if (f == null) return;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = f.FullPath;
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Lang.F("无法打开文件：\n{0}", ex.Message), Lang.T("打开失败"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OpenSelectedFiles()
        {
            if (dgv.SelectedRows.Count > 0)
            {
                for (int i = 0; i < dgv.SelectedRows.Count; i++)
                {
                    OpenPath(dgv.SelectedRows[i].Tag as FileItem);
                }
            }
            else
            {
                OpenPath(CurrentItem());
            }
        }

        private void OpenSelectedFolder()
        {
            FileItem f = null;
            if (dgv.SelectedRows.Count > 0) f = dgv.SelectedRows[0].Tag as FileItem;
            if (f == null && dgv.CurrentRow != null) f = dgv.CurrentRow.Tag as FileItem;
            if (f == null) return;
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "explorer.exe";
                psi.Arguments = "/select,\"" + f.FullPath + "\"";
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Lang.F("无法打开文件夹：\n{0}", ex.Message), Lang.T("打开失败"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopySelectedPaths()
        {
            List<string> paths = new List<string>();
            if (dgv.SelectedRows.Count > 0)
            {
                for (int i = 0; i < dgv.SelectedRows.Count; i++)
                {
                    FileItem f = dgv.SelectedRows[i].Tag as FileItem;
                    if (f != null) paths.Add(f.FullPath);
                }
            }
            else
            {
                FileItem f = CurrentItem();
                if (f != null) paths.Add(f.FullPath);
            }
            if (paths.Count == 0) return;
            try
            {
                Clipboard.SetText(string.Join("\r\n", paths.ToArray()));
                lblStatus.Text = Lang.F("已复制 {0} 条路径到剪贴板。", paths.Count);
            }
            catch
            {
            }
        }

        private void KeepOnlyCurrent()
        {
            FileItem f = CurrentItem();
            if (f == null) return;
            DupGroup g = GroupOf(f);
            if (g == null) return;
            for (int i = 0; i < g.Files.Count; i++)
            {
                g.Files[i].SelectedForDelete = (g.Files[i] != f);
                g.Files[i].Note = (g.Files[i] == f) ? "保留这份" : "重复";
            }
            RebuildGrid();
        }

        private void UncheckGroupOfCurrent()
        {
            FileItem f = CurrentItem();
            if (f == null) return;
            DupGroup g = GroupOf(f);
            if (g == null) return;
            for (int i = 0; i < g.Files.Count; i++)
            {
                g.Files[i].SelectedForDelete = false;
            }
            RebuildGrid();
        }

        // ============================================================
        //  删除
        // ============================================================
        private void DeleteSelected(bool permanent)
        {
            if (_busy) return;
            if (_groups.Count == 0)
            {
                MessageBox.Show(this, Lang.T("还没有可清理的重复文件，请先扫描。"), Lang.T("提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { dgv.EndEdit(); } catch { }
            PersistCheckStates();

            List<FileItem> del = new List<FileItem>();
            long bytes = 0;
            int fullGroups = 0;
            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                int checkedInGroup = 0;
                for (int j = 0; j < g.Files.Count; j++)
                {
                    FileItem f = g.Files[j];
                    if (!f.SelectedForDelete) continue;
                    checkedInGroup++;
                    del.Add(f);
                    bytes += f.Length;
                }
                if (g.Files.Count > 0 && checkedInGroup == g.Files.Count) fullGroups++;
            }

            if (del.Count == 0)
            {
                MessageBox.Show(this, Lang.T("请先勾选要删除的文件。\n可以点“按规则勾选”，让每组自动保留一份、其余勾选删除。"), Lang.T("没有勾选文件"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string note = (fullGroups > 0)
                ? Lang.F("注意：有 {0} 组的所有副本都被勾选，删除后这些内容将一份不剩。\n\n", fullGroups)
                : "";
            string act = permanent
                ? (Lang.IsZhCn ? "永久删除" : Lang.T("__del.perm"))
                : (Lang.IsZhCn ? "移入回收站" : Lang.T("__del.recycle"));
            string body = Lang.F("即将{0} {1} 个文件，释放约 {2}。\n\n{3}确定继续吗？",
                act, Fmt.Number(del.Count), Fmt.Size(bytes), note);

            DialogResult dr = MessageBox.Show(this, body,
                Lang.T(permanent ? "永久删除确认" : "清理确认"),
                MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (dr != DialogResult.OK) return;

            string[] paths = new string[del.Count];
            for (int i = 0; i < del.Count; i++) paths[i] = del[i].FullPath;

            _busy = true;
            SetUiBusy(true);
            lblStatus.Text = permanent ? Lang.T("正在永久删除…") : Lang.T("正在移到回收站…");

            Task.Run(delegate
            {
                int code = 0;
                string exceptionText = null;
                try
                {
                    code = ShellDelete.Run(paths, !permanent, IntPtr.Zero);
                }
                catch (Exception ex) { exceptionText = ex.Message; }

                List<string> stillThere = new List<string>();
                for (int i = 0; i < paths.Length; i++)
                {
                    try
                    {
                        if (File.Exists(paths[i])) stillThere.Add(paths[i]);
                    }
                    catch
                    {
                        stillThere.Add(paths[i]);
                    }
                }

                try
                {
                    this.BeginInvoke(new Action(delegate
                    {
                        _busy = false;
                        SetUiBusy(false);
                        pb.SetValue(0);
                        lblPercent.Text = "";
                        ApplyDeleteResult(stillThere, permanent, code, exceptionText);
                    }));
                }
                catch
                {
                }
            });
        }

        private void ApplyDeleteResult(List<string> stillThere, bool permanent, int code, string exceptionText)
        {
            HashSet<string> failSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < stillThere.Count; i++) failSet.Add(stillThere[i]);

            int ok = 0;
            long freed = 0;
            List<DupGroup> survivors = new List<DupGroup>();

            for (int i = 0; i < _groups.Count; i++)
            {
                DupGroup g = _groups[i];
                List<FileItem> remove = new List<FileItem>();
                for (int j = 0; j < g.Files.Count; j++)
                {
                    FileItem f = g.Files[j];
                    if (!f.SelectedForDelete) continue;
                    if (failSet.Contains(f.FullPath)) continue;
                    remove.Add(f);
                    ok++;
                    freed += f.Length;
                }
                for (int k = 0; k < remove.Count; k++) g.Files.Remove(remove[k]);

                for (int j = 0; j < g.Files.Count; j++)
                {
                    g.Files[j].SelectedForDelete = false;
                    if (g.Files[j].Note == "待删除") g.Files[j].Note = "重复";
                }
                if (g.Files.Count >= 2) survivors.Add(g);
            }
            _groups.Clear();
            _groups.AddRange(survivors);

            RebuildGrid();
            btnExport.Enabled = _groups.Count > 0;
            string act2 = permanent
                ? (Lang.IsZhCn ? "永久删除" : Lang.T("__del.perm"))
                : (Lang.IsZhCn ? "移入回收站" : Lang.T("__del.recycle"));
            lblStatus.Text = Lang.F("清理完成，已{0} {1} 个文件、释放 {2}。{3}", act2, Fmt.Number(ok), Fmt.Size(freed),
                _groups.Count == 0 ? Lang.T("当前没有重复文件。") : "");

            StringBuilder msg = new StringBuilder();
            msg.Append(permanent
                ? Lang.F("已永久删除 {0} 个文件，释放约 {1} 空间。", Fmt.Number(ok), Fmt.Size(freed))
                : Lang.F("已移入回收站 {0} 个文件，释放约 {1} 空间。", Fmt.Number(ok), Fmt.Size(freed)));

            if (exceptionText != null)
            {
                msg.Append("\n\n");
                msg.Append(Lang.T("调用系统删除接口失败："));
                msg.Append(exceptionText);
            }
            if (stillThere.Count > 0)
            {
                msg.Append("\n\n");
                msg.Append(Lang.F("{0} 个文件未能删除，常见原因：正被其他程序占用、没有权限、或位于不支持回收站的网络位置/移动磁盘。\n", stillThere.Count));
                int show = Math.Min(stillThere.Count, 5);
                for (int i = 0; i < show; i++)
                {
                    msg.Append("\n· ");
                    msg.Append(stillThere[i]);
                }
                if (stillThere.Count > show) msg.Append("\n…");
                if (!permanent)
                {
                    msg.Append("\n\n");
                    msg.Append(Lang.T("可以对这几个文件改用“永久删除”再试一次。"));
                }
            }

            MessageBox.Show(this, msg.ToString(),
                Lang.T(stillThere.Count > 0 ? "部分文件未删除" : "清理完成"),
                MessageBoxButtons.OK,
                stillThere.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        // ============================================================
        //  导出
        // ============================================================
        private static string CsvCell(string s)
        {
            if (s == null) return "";
            if (s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\n') >= 0)
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }

        private void ExportCsv()
        {
            if (_groups.Count == 0)
            {
                MessageBox.Show(this, Lang.T("还没有扫描结果可以导出。"), Lang.T("提示"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try { dgv.EndEdit(); } catch { }
            PersistCheckStates();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = Lang.T("导出重复文件清单");
                sfd.Filter = Lang.T("CSV 文件 (*.csv)|*.csv");
                sfd.FileName = Lang.F("重复文件清单_{0}.csv", DateTime.Now.ToString("yyyyMMdd_HHmm"));
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                if (WriteCsvTo(sfd.FileName))
                {
                    lblStatus.Text = Lang.T("清单已导出：") + sfd.FileName;
                    MessageBox.Show(this, Lang.F("已导出到：\n{0}", sfd.FileName), Lang.T("导出完成"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // 把当前结果写成 CSV（1.4：顶部带扫描汇总，行内含创建时间）
        private bool WriteCsvTo(string path)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
                {
                    if (_lastSummary != null)
                    {
                        ScanSummary s = _lastSummary;
                        sw.WriteLine(CsvCell(Lang.T("扫描汇总") + " · " + Lang.T(s.ModeText)));
                        sw.WriteLine(CsvCell(Lang.T("扫描时间") + "：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                        sw.WriteLine(CsvCell(Lang.F("共扫描 {0} 个文件（{1}）", Fmt.Number(s.FilesScanned), Fmt.Size(s.TotalBytes))));
                        sw.WriteLine(CsvCell(Lang.F("重复组：{0} 组（其中相似图片 {1} 组）", Fmt.Number(s.Groups.Count), Fmt.Number(s.SimilarGroups.Count))));
                        sw.WriteLine(CsvCell(Lang.F("重复文件：{0} 项，可释放 {1}", Fmt.Number((int)s.DupFiles), Fmt.Size(s.WastedBytes))));
                        sw.WriteLine(CsvCell(Lang.F("总耗时：{0:F1} 秒", s.Seconds)));
                        sw.WriteLine();
                    }
                    sw.WriteLine(Lang.T("组号,文件名,完整路径,大小(字节),大小,修改时间,创建时间,状态"));
                    for (int i = 0; i < _groups.Count; i++)
                    {
                        DupGroup g = _groups[i];
                        for (int j = 0; j < g.Files.Count; j++)
                        {
                            FileItem f = g.Files[j];
                            sw.WriteLine(string.Join(",", new string[]
                            {
                                g.Id.ToString(),
                                CsvCell(f.FileName),
                                CsvCell(f.FullPath),
                                f.Length.ToString(),
                                f.SizeText,
                                f.ModifiedText,
                                f.CreatedText,
                                Lang.T(f.SelectedForDelete ? "勾选删除" : f.Note)
                            }));
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Lang.F("导出失败：\n{0}", ex.Message), Lang.T("导出失败"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
    }
}











