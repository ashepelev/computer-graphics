using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CG_Task4
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           // Form1 frm = new Form1();
            using (Form1 frm = new Form1())
            {
                frm.Show();
                frm.InitializeGraphics();
                Application.Run(frm);
            }
        }
    }
}
