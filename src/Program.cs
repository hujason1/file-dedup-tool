using System;
using System.Windows.Forms;

namespace FileDedup
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Settings.Load();
                Theme.Refresh();
                Application.Run(new MainForm(args));
            }
            catch (Exception ex)
            {
                MessageBox.Show("程序启动失败：" + ex.Message, "文件查重工具", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


