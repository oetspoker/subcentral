using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using SubCentral.Utils;
using SubCentral.Settings;
using MediaPortal.Configuration;

namespace SubCentral.ConfigForm {
    public partial class AdvancedConfigForm : Form {
        public AdvancedConfigForm() {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e) {
            SettingsManager.Properties.GUISettings.PluginName = txtPluginName.Text;
            SettingsManager.Properties.GUISettings.HidePlugin = checkBoxHidePlugin.Checked;
            SettingsManager.Properties.GUISettings.CheckMediaForSubtitlesOnOpen = checkBoxCheckMediaOnOpen.Checked;
            Close();
        }

        private void AdvancedConfigForm_Load(object sender, EventArgs e) {
            txtPluginName.Text = SettingsManager.Properties.GUISettings.PluginName;
            checkBoxHidePlugin.Checked = SettingsManager.Properties.GUISettings.HidePlugin;
            checkBoxCheckMediaOnOpen.Checked = SettingsManager.Properties.GUISettings.CheckMediaForSubtitlesOnOpen;
        }

        private void btnCancel_Click(object sender, EventArgs e) {
            Close();
        }

        private void btnOpenLogFolder_Click(object sender, EventArgs e) {
            string dir = Config.GetFolder(Config.Dir.Log);
            if (Directory.Exists(dir)) {
                Process.Start(dir);
            }
            else {
                MessageBox.Show("Error opening " + dir + ". Directory doesn't exist. It seems that the are some MediaPortal configuration problems.", "Error opening folder!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenLog_Click(object sender, EventArgs e) {
            string logFile = Config.GetFolder(Config.Dir.Log) + @"\SubCentral.log";
            if (File.Exists(logFile)) {
                Process.Start(logFile);
            }
            else {
                MessageBox.Show("Error opening " + logFile + ". The log file doesn't exist. Please start MediaPortal first, to create the log file.", "Error opening file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdvancedConfigForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e) {
            Process.Start(@"http://code.google.com/p/subcentral/wiki/ManualConfiguration#Advanced");
        }
    }
}
