using System.Drawing;
using System.Windows.Forms;

namespace FileDedup
{
    // 让菜单栏跟主题配色一致（去掉默认的渐变色块）
    internal class MenuColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected { get { return Theme.MenuSel; } }
        public override Color MenuItemSelectedGradientBegin { get { return Theme.MenuSel; } }
        public override Color MenuItemSelectedGradientEnd { get { return Theme.MenuSel; } }
        public override Color MenuItemBorder { get { return Theme.MenuSel; } }
        public override Color MenuItemPressedGradientBegin { get { return Theme.MenuSel; } }
        public override Color MenuItemPressedGradientEnd { get { return Theme.MenuSel; } }
        public override Color MenuBorder { get { return Theme.Line; } }
        public override Color ToolStripDropDownBackground { get { return Theme.Card; } }
        public override Color ImageMarginGradientBegin { get { return Theme.Card; } }
        public override Color ImageMarginGradientMiddle { get { return Theme.Card; } }
        public override Color ImageMarginGradientEnd { get { return Theme.Card; } }
        public override Color SeparatorDark { get { return Theme.Line; } }
        public override Color SeparatorLight { get { return Theme.Line; } }
    }
}


