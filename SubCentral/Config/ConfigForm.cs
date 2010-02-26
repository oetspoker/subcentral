using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using NLog;

namespace SubCentral.ConfigForm {
    public partial class ConfigForm : Form, ISetupForm {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SubCentralCore core = SubCentralCore.Instance;

        public ConfigForm() {
            InitializeComponent();
        }

        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu 
        public string PluginName() {
            return "SubCentral";
        }

        // Returns the description of the plugin is shown in the plugin menu     
        public string Description() {
            return "A plugin that will allow you to download subtitles for movies or TV shows.";
        }

        // Returns the author of the plugin which is shown in the plugin menu     
        public string Author() {
            return "";
        }

        // show the setup dialog    
        public void ShowPlugin() {
            core.Initialize();
            logger.Info("Launching Configuration Screen");
            this.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled      
        public bool CanEnable() {
            return true;
        }

        // get ID of windowplugin belonging to this setup    
        public int GetWindowId() {
            return 84623;
        }

        // Indicates if plugin is enabled by default
        public bool DefaultEnabled() {
            return true;
        }
        
        // indicates if a plugin has its own setup screen     
        public bool HasSetup() {
            return true;
        }

        public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) {
            strButtonText = "SubCentral";
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_subcentral.png";
            return true;
        }

        #endregion
    }
}
