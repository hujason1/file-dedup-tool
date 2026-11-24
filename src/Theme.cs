using System;
using System.Drawing;

namespace FileDedup
{
    // 主题：简约（原 1.4 界面）/ 白色 / 深色 / 自定义（背景色或背景图片）
    internal static class Theme
    {
        public const int Light = 0;
        public const int Dark = 1;
        public const int Custom = 2;
        // 简约模式 = 1.4 原来的界面（蓝色渐变大横幅、方角卡片、原配色），作为第三套皮肤保留
        public const int Simple = 3;

        public static int Mode = Light;
        public static Color CustomBg = Color.FromArgb(238, 242, 248);
        public static string CustomImage = "";
        // 自定义背景图时，卡片用“毛玻璃”半透明效果（背景图模糊后透出）
        public static bool Glass = true;
        // 毛玻璃透明度（0 = 完全不透明，100 = 完全透明）
        public static int GlassOpacity = 25;
        // 自定义主题时，卡片是否跟随暗色（由背景明暗自动判断）
        public static bool UseDarkCards;

        public static Color Page;
        public static Color Card;
        public static Color TextC;
        public static Color SubText;
        public static Color Line;
        public static Color Band;
        public static Color Accent;
        public static Color AccentSoft;
        public static Color Neutral;
        public static Color Danger;
        public static Color DangerSoft;
        public static Color Good;
        public static Color InputBg;
        public static Color StatusBg;
        public static Color MenuBg;
        public static Color MenuSel;
        public static Color HeaderFrom;
        public static Color HeaderTo;
        public static Color SubOnHeader;
        public static Color GridLine;
        public static Color GridHeaderBg;
        public static Color GridSelBg;
        public static Color GridSelBorder;
        public static Color OnAccent;
        // 输入框/列表框的描边色（比分隔线深一档，参考图的输入框是明显的细边）
        public static Color FieldLine;
        // 顶部是否用“大横幅”（简约模式的蓝横幅 + 白色主题的青灰渐变横幅）；深色/自定义仍是 64px 细顶栏
        public static bool Banner;

        public static bool IsSimple()
        {
            return Mode == Simple;
        }

        public static void Refresh()
        {
            bool dark;
            if (Mode == Dark) dark = true;
            else if (Mode == Custom) dark = IsDarkColor(CustomBg);
            else dark = false;

            if (Mode == Simple)
            {
                // ---- 简约模式：完全沿用 1.4 原配色（蓝色横幅那一套） ----
                Page = Color.FromArgb(241, 244, 249);
                Card = Color.White;
                TextC = Color.FromArgb(28, 36, 48);
                SubText = Color.FromArgb(108, 120, 136);
                Line = Color.FromArgb(226, 231, 240);
                Band = Color.FromArgb(245, 249, 255);
                Accent = Color.FromArgb(43, 103, 226);
                AccentSoft = Color.FromArgb(232, 239, 253);
                Neutral = Color.FromArgb(242, 244, 249);
                Danger = Color.FromArgb(209, 56, 62);
                DangerSoft = Color.FromArgb(252, 233, 234);
                Good = Color.FromArgb(22, 122, 82);
                InputBg = Color.FromArgb(252, 253, 255);
                StatusBg = Color.White;
                MenuBg = Color.FromArgb(250, 251, 254);
                MenuSel = Color.FromArgb(232, 239, 253);
                HeaderFrom = Color.FromArgb(36, 92, 214);
                HeaderTo = Color.FromArgb(58, 126, 236);
                SubOnHeader = Color.FromArgb(214, 229, 255);
                GridLine = Color.FromArgb(238, 241, 246);
                GridHeaderBg = Color.FromArgb(250, 251, 253);
                GridSelBg = Color.FromArgb(222, 234, 255);
                GridSelBorder = Color.FromArgb(43, 103, 226);
                OnAccent = Color.White;
                FieldLine = Color.FromArgb(226, 231, 240);
                Banner = true;
            }
            else if (!dark)
            {
                // ---- 浅色配色：青灰（teal）主题，按参考图取色 ----
                if (Mode != Custom) Page = Color.FromArgb(241, 250, 252);      // #F1FAFC 淡青白页面底
                Card = Color.FromArgb(255, 255, 255);
                TextC = Color.FromArgb(23, 50, 60);                            // #17323C
                SubText = Color.FromArgb(90, 118, 131);                        // #5A7683
                Line = Color.FromArgb(198, 217, 224);                          // #C6D9E0 卡片/表格描边
                Band = Color.FromArgb(226, 237, 241);                          // #E2EDF1
                Accent = Color.FromArgb(23, 94, 112);                          // #175E70 深青：按钮填充/选中态
                AccentSoft = Color.FromArgb(227, 238, 241);                    // #E3EEF1
                Neutral = Color.FromArgb(238, 245, 247);
                Danger = Color.FromArgb(176, 64, 52);
                DangerSoft = Color.FromArgb(251, 234, 231);
                Good = Color.FromArgb(31, 122, 92);
                InputBg = Color.FromArgb(255, 255, 255);
                StatusBg = Color.FromArgb(255, 255, 255);
                MenuBg = Color.FromArgb(255, 255, 255);
                MenuSel = Color.FromArgb(227, 238, 241);
                HeaderFrom = Color.FromArgb(91, 143, 163);                     // 横幅左端 #5B8FA3
                HeaderTo = Color.FromArgb(228, 241, 245);                      // 横幅右端 #E4F1F5（左深右浅）
                SubOnHeader = Color.FromArgb(240, 250, 252);
                GridLine = Color.FromArgb(226, 237, 241);                      // 行线极浅
                GridHeaderBg = Color.FromArgb(217, 226, 231);                  // #D9E2E7 灰青表头
                GridSelBg = Color.FromArgb(214, 231, 239);                     // #D6E7EF 选中行
                GridSelBorder = Color.FromArgb(23, 94, 112);
                OnAccent = Color.White;
                FieldLine = Color.FromArgb(159, 184, 194);                     // #9FB8C2 输入框描边
                Banner = (Mode != Custom);
            }
            else
            {
                // ---- 深色配色 ----
                if (Mode != Custom) Page = Color.FromArgb(20, 22, 26);
                Card = Color.FromArgb(30, 33, 38);
                TextC = Color.FromArgb(232, 234, 238);
                SubText = Color.FromArgb(152, 160, 172);
                Line = Color.FromArgb(44, 48, 56);
                Band = Color.FromArgb(35, 38, 44);
                Accent = Color.FromArgb(53, 200, 219);
                AccentSoft = Color.FromArgb(18, 51, 59);
                Neutral = Color.FromArgb(35, 38, 44);
                Danger = Color.FromArgb(255, 107, 94);
                DangerSoft = Color.FromArgb(58, 35, 33);
                Good = Color.FromArgb(79, 209, 165);
                InputBg = Color.FromArgb(26, 29, 34);
                StatusBg = Color.FromArgb(23, 26, 31);
                MenuBg = Color.FromArgb(26, 29, 34);
                MenuSel = Color.FromArgb(18, 51, 59);
                HeaderFrom = Color.FromArgb(30, 33, 38);
                HeaderTo = Color.FromArgb(30, 33, 38);
                SubOnHeader = Color.FromArgb(152, 160, 172);
                GridLine = Color.FromArgb(38, 42, 49);
                GridHeaderBg = Color.FromArgb(26, 29, 34);
                GridSelBg = Color.FromArgb(18, 51, 59);
                GridSelBorder = Color.FromArgb(53, 200, 219);
                OnAccent = Color.FromArgb(10, 20, 22);
                FieldLine = Color.FromArgb(44, 48, 56);
                Banner = false;
            }
            if (Mode == Custom) Page = CustomBg;
            UseDarkCards = dark;
        }

        // 由背景色亮度判断该用浅色卡片还是深色卡片
        public static bool IsDarkColor(Color c)
        {
            double lum = (0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
            return lum < 140;
        }

        // 让窗口标题栏跟随主题（深色主题不再是一条白色标题栏）
        public static void ApplyTitleBar(System.Windows.Forms.Form f)
        {
            if (f == null) return;
            f.HandleCreated += delegate { DarkCaption(f); };
            f.Shown += delegate { DarkCaption(f); };
        }

        private static void DarkCaption(System.Windows.Forms.Form f)
        {
            try
            {
                int dark = (Mode == Dark || (Mode == Custom && UseDarkCards)) ? 1 : 0;
                int v = dark;
                int hr = DwmSetWindowAttribute(f.Handle, 20, ref v, 4);
                if (hr != 0) DwmSetWindowAttribute(f.Handle, 19, ref v, 4);
                // Windows 11 上再加一道保险：直接指定标题栏底色与文字色
                int cap = dark == 1 ? 0x242020 : unchecked((int)0xFFFFFFFF);   // R32 G32 B36
                int txt = dark == 1 ? 0xFFFFFF : unchecked((int)0xFFFFFFFF);
                DwmSetWindowAttribute(f.Handle, 35, ref cap, 4);
                DwmSetWindowAttribute(f.Handle, 36, ref txt, 4);
            }
            catch { }
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);
    }
}



