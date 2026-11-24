using System;
using System.Drawing;
using System.IO;

namespace FileDedup
{
    // 偏好设置持久化：主题、语言、字体
    internal static class Settings
    {
        public static string FontName = "Microsoft YaHei UI";
        public static double FontPt = 9.0;
        // 扫描层级下拉的选中项：0=仅当前目录 1/2/3=含 N 层 4=全部子目录
        public static int ScanDepth = 4;
        // 全文件扫描（不看扩展名，只按内容哈希比对）
        public static bool ScanAll = false;
        // 相似图片（感知哈希）与阈值（汉明距离 3/6/12）
        public static bool ScanSimilar = false;
        public static int SimilarThreshold = 6;

        // 1.4 新增：表格排序 / 保留规则记忆
        public static int Sort = 2;        // 0 大小↓ 1 大小↑ 2 时间↓ 3 时间↑ 4 路径（默认按修改时间新→旧）
        public static int KeepRule = 1;    // 0 最早 1 最新 2 路径最短 3 分辨率最高 4 文件名最短

        // 1.4 新增：列显示与列宽
        public static bool ShowCreated = false;     // 是否显示“创建时间”列（默认收起，视图→列里可打开）
        public static string ColWidths = "";        // 形如 60,58,146,80,126,126,106
        public static string ColVisible = "";       // 形如 1,1,1,1,1,1,1（组/文件名/路径/大小/修改时间/创建时间/状态）
        // 1.4 新增：列顺序（拖列头换位置后记住）。存的是“第几个显示位放的是哪一列”，形如 0,1,2,3,4,5,6,7
        public static string ColOrder = "";
        // 1.4 新增：窗口大小与最大化状态
        public static int WindowW = 0;
        public static int WindowH = 0;
        public static bool WindowMax = false;
        // 1.4 新增：扫描完成后是否自动弹出统计报告
        public static bool AutoReport = true;

        private static string FilePath
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FileDedupTool");
                return Path.Combine(dir, "settings.ini");
            }
        }

        // 界面实际使用的字体：韩语下雅黑没有谚文字形，换成系统自带的 Malgun Gothic
        public static string UiFont()
        {
            if (Lang.Code == Lang.Ko && (FontName == "Microsoft YaHei UI" || FontName == "Microsoft YaHei" || FontName == "微软雅黑"))
                return "Malgun Gothic";
            return FontName;
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return;
                string[] lines = File.ReadAllLines(FilePath, System.Text.Encoding.UTF8);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line == null) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = line.Substring(0, eq).Trim();
                    string v = line.Substring(eq + 1).Trim();
                    if (k == "theme") Theme.Mode = (v == "dark") ? Theme.Dark
                        : (v == "custom" ? Theme.Custom : (v == "simple" ? Theme.Simple : Theme.Light));
                    else if (k == "bgColor")
                    {
                        string[] p = v.Split(',');
                        if (p.Length == 3)
                        {
                            Theme.CustomBg = Color.FromArgb(Int(p[0]), Int(p[1]), Int(p[2]));
                        }
                    }
                    else if (k == "bgImage") Theme.CustomImage = v;
                    else if (k == "glass") Theme.Glass = (v == "1" || v == "true");
                    else if (k == "glassOpacity")
                    {
                        int o;
                        if (int.TryParse(v, out o) && o >= 0 && o <= 100) Theme.GlassOpacity = o;
                    }
                    else if (k == "lang")
                    {
                        if (v == "zh-TW" || v == "en" || v == "ja" || v == "ru" || v == "ko" || v == "zh-CN") Lang.Code = v;
                    }
                    else if (k == "fontName")
                    {
                        if (v.Length > 0) FontName = v;
                    }
                    else if (k == "fontPt")
                    {
                        double d;
                        if (double.TryParse(v, out d) && d >= 6 && d <= 24)
                        {
                            // “较大(10.5) / 特大(12.0)” 两个菜单选项已移除，旧配置回落到“标准”
                            if (Math.Abs(d - 10.5) < 0.05 || Math.Abs(d - 12.0) < 0.05) d = 9.0;
                            FontPt = d;
                        }
                    }
                    else if (k == "scanDepth")
                    {
                        int d;
                        if (int.TryParse(v, out d) && d >= 0 && d <= 4) ScanDepth = d;
                    }
                    else if (k == "scanAll")
                    {
                        ScanAll = (v == "1" || v == "true");
                    }
                    else if (k == "scanSimilar")
                    {
                        ScanSimilar = (v == "1" || v == "true");
                    }
                    else if (k == "similarThreshold")
                    {
                        int t;
                        if (int.TryParse(v, out t) && (t == 3 || t == 6 || t == 12)) SimilarThreshold = t;
                    }
                    else if (k == "sort")
                    {
                        int s;
                        if (int.TryParse(v, out s) && s >= 0 && s <= 4) Sort = s;
                    }
                    else if (k == "keepRule")
                    {
                        int r;
                        if (int.TryParse(v, out r) && r >= 0 && r <= 4) KeepRule = r;
                    }
                    else if (k == "showCreated") ShowCreated = (v == "1" || v == "true");
                    else if (k == "colWidths") ColWidths = v;
                    else if (k == "colVisible") ColVisible = v;
                    else if (k == "colOrder") ColOrder = v;
                    else if (k == "windowW")
                    {
                        int w;
                        if (int.TryParse(v, out w) && w >= 800 && w <= 6000) WindowW = w;
                    }
                    else if (k == "windowH")
                    {
                        int h;
                        if (int.TryParse(v, out h) && h >= 600 && h <= 6000) WindowH = h;
                    }
                    else if (k == "windowMax") WindowMax = (v == "1" || v == "true");
                    else if (k == "autoReport") AutoReport = (v == "1" || v == "true");
                }
                Theme.Refresh();
            }
            catch
            {
            }
        }

        private static int Int(string s)
        {
            int v;
            if (int.TryParse(s, out v)) return v;
            return 0;
        }

        public static void Save()
        {
            try
            {
                string dir = Path.GetDirectoryName(FilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string theme = (Theme.Mode == Theme.Dark) ? "dark"
                    : (Theme.Mode == Theme.Custom ? "custom" : (Theme.Mode == Theme.Simple ? "simple" : "light"));
                string[] lines = new string[]
                {
                    "theme=" + theme,
                    "bgColor=" + Theme.CustomBg.R + "," + Theme.CustomBg.G + "," + Theme.CustomBg.B,
                    "bgImage=" + Theme.CustomImage,
                    "glass=" + (Theme.Glass ? "1" : "0"),
                    "glassOpacity=" + Theme.GlassOpacity,
                    "lang=" + Lang.Code,
                    "fontName=" + FontName,
                    "fontPt=" + FontPt.ToString("0.0"),
                    "scanDepth=" + ScanDepth,
                    "scanAll=" + (ScanAll ? "1" : "0"),
                    "scanSimilar=" + (ScanSimilar ? "1" : "0"),
                    "similarThreshold=" + SimilarThreshold,
                    "sort=" + Sort,
                    "keepRule=" + KeepRule,
                    "showCreated=" + (ShowCreated ? "1" : "0"),
                    "colWidths=" + ColWidths,
                    "colVisible=" + ColVisible,
                    "colOrder=" + ColOrder,
                    "windowW=" + WindowW,
                    "windowH=" + WindowH,
                    "windowMax=" + (WindowMax ? "1" : "0"),
                    "autoReport=" + (AutoReport ? "1" : "0")
                };
                File.WriteAllLines(FilePath, lines, System.Text.Encoding.UTF8);
            }
            catch
            {
            }
        }
    }
}


