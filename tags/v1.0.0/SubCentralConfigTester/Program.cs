using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SubCentral.ConfigForm;

namespace SubCentralConfigTester {
    static class Program {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();

            ConfigForm config = new ConfigForm();
            config.ShowPlugin();
        }
    }
}
