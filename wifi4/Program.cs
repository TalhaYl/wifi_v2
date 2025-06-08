using System;
using System.Windows.Forms;
using wifi4;

namespace YourAppNamespace
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Burada Form1 yerine MainMenuForm baþlatýlýyor
            Application.Run(new MainMenuForm());
        }
    }
}
