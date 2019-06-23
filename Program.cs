using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdaptiveVectorQuantization
{
    static class Program
    {
        public static FormAVQ form;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Form form = new FormAVQ();
            form = new FormAVQ();
            Application.Run(form);
        }
    }
}
