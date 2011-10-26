namespace SubCentral.ConfigForm
{
    partial class AdvancedConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedConfigForm));
            this.gbLogging = new System.Windows.Forms.GroupBox();
            this.btnOpenLog = new System.Windows.Forms.Button();
            this.btnOpenLogFolder = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbGeneral = new System.Windows.Forms.GroupBox();
            this.checkBoxHidePlugin = new System.Windows.Forms.CheckBox();
            this.checkBoxCheckMediaOnOpen = new System.Windows.Forms.CheckBox();
            this.txtPluginName = new System.Windows.Forms.TextBox();
            this.labelPluginName = new System.Windows.Forms.Label();
            this.gbSearching = new System.Windows.Forms.GroupBox();
            this.checkBoxShowResultsAfterProgressCancel = new System.Windows.Forms.CheckBox();
            this.laSearching1 = new System.Windows.Forms.Label();
            this.checkBoxUseLanguageCode = new System.Windows.Forms.CheckBox();
            this.numericUpDownSearching1 = new System.Windows.Forms.NumericUpDown();
            this.gbDownloading = new System.Windows.Forms.GroupBox();
            this.labelAfterDownloading = new System.Windows.Forms.Label();
            this.comboBoxAfterDownloading = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.gbLogging.SuspendLayout();
            this.gbGeneral.SuspendLayout();
            this.gbSearching.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSearching1)).BeginInit();
            this.gbDownloading.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbLogging
            // 
            this.gbLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbLogging.Controls.Add(this.btnOpenLog);
            this.gbLogging.Controls.Add(this.btnOpenLogFolder);
            this.gbLogging.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbLogging.Location = new System.Drawing.Point(6, 256);
            this.gbLogging.Name = "gbLogging";
            this.gbLogging.Size = new System.Drawing.Size(376, 53);
            this.gbLogging.TabIndex = 1;
            this.gbLogging.TabStop = false;
            this.gbLogging.Text = "Logging";
            // 
            // btnOpenLog
            // 
            this.btnOpenLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenLog.Image = global::SubCentral.Properties.Resources.PageWhiteText;
            this.btnOpenLog.Location = new System.Drawing.Point(132, 20);
            this.btnOpenLog.Name = "btnOpenLog";
            this.btnOpenLog.Size = new System.Drawing.Size(114, 23);
            this.btnOpenLog.TabIndex = 6;
            this.btnOpenLog.Text = "Open &log";
            this.btnOpenLog.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOpenLog.UseVisualStyleBackColor = true;
            this.btnOpenLog.Click += new System.EventHandler(this.btnOpenLog_Click);
            // 
            // btnOpenLogFolder
            // 
            this.btnOpenLogFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpenLogFolder.Image = global::SubCentral.Properties.Resources.FolderPageWhite;
            this.btnOpenLogFolder.Location = new System.Drawing.Point(12, 20);
            this.btnOpenLogFolder.Name = "btnOpenLogFolder";
            this.btnOpenLogFolder.Size = new System.Drawing.Size(114, 23);
            this.btnOpenLogFolder.TabIndex = 5;
            this.btnOpenLogFolder.Text = "Open log &folder";
            this.btnOpenLogFolder.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOpenLogFolder.UseVisualStyleBackColor = true;
            this.btnOpenLogFolder.Click += new System.EventHandler(this.btnOpenLogFolder_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Image = global::SubCentral.Properties.Resources.OK;
            this.btnSave.Location = new System.Drawing.Point(6, 321);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "&Save";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Image = global::SubCentral.Properties.Resources.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(90, 321);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // gbGeneral
            // 
            this.gbGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbGeneral.Controls.Add(this.checkBoxHidePlugin);
            this.gbGeneral.Controls.Add(this.checkBoxCheckMediaOnOpen);
            this.gbGeneral.Controls.Add(this.txtPluginName);
            this.gbGeneral.Controls.Add(this.labelPluginName);
            this.gbGeneral.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbGeneral.Location = new System.Drawing.Point(6, 3);
            this.gbGeneral.Name = "gbGeneral";
            this.gbGeneral.Size = new System.Drawing.Size(376, 92);
            this.gbGeneral.TabIndex = 0;
            this.gbGeneral.TabStop = false;
            this.gbGeneral.Text = "General";
            // 
            // checkBoxHidePlugin
            // 
            this.checkBoxHidePlugin.AutoSize = true;
            this.checkBoxHidePlugin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxHidePlugin.Location = new System.Drawing.Point(12, 43);
            this.checkBoxHidePlugin.Name = "checkBoxHidePlugin";
            this.checkBoxHidePlugin.Size = new System.Drawing.Size(228, 17);
            this.checkBoxHidePlugin.TabIndex = 5;
            this.checkBoxHidePlugin.Text = "Hide plugin from home and plugins screens";
            this.checkBoxHidePlugin.UseVisualStyleBackColor = true;
            // 
            // checkBoxCheckMediaOnOpen
            // 
            this.checkBoxCheckMediaOnOpen.AutoSize = true;
            this.checkBoxCheckMediaOnOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxCheckMediaOnOpen.Location = new System.Drawing.Point(12, 66);
            this.checkBoxCheckMediaOnOpen.Name = "checkBoxCheckMediaOnOpen";
            this.checkBoxCheckMediaOnOpen.Size = new System.Drawing.Size(245, 17);
            this.checkBoxCheckMediaOnOpen.TabIndex = 4;
            this.checkBoxCheckMediaOnOpen.Text = "Check media for subtitles when entering plugin";
            this.checkBoxCheckMediaOnOpen.UseVisualStyleBackColor = true;
            // 
            // txtPluginName
            // 
            this.txtPluginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPluginName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPluginName.Location = new System.Drawing.Point(167, 17);
            this.txtPluginName.Name = "txtPluginName";
            this.txtPluginName.Size = new System.Drawing.Size(197, 20);
            this.txtPluginName.TabIndex = 1;
            this.txtPluginName.Text = "SubCentral";
            // 
            // labelPluginName
            // 
            this.labelPluginName.AutoSize = true;
            this.labelPluginName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPluginName.Location = new System.Drawing.Point(9, 20);
            this.labelPluginName.Name = "labelPluginName";
            this.labelPluginName.Size = new System.Drawing.Size(147, 13);
            this.labelPluginName.TabIndex = 0;
            this.labelPluginName.Text = "Plugin name on home-screen:";
            // 
            // gbSearching
            // 
            this.gbSearching.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbSearching.Controls.Add(this.checkBoxShowResultsAfterProgressCancel);
            this.gbSearching.Controls.Add(this.laSearching1);
            this.gbSearching.Controls.Add(this.checkBoxUseLanguageCode);
            this.gbSearching.Controls.Add(this.numericUpDownSearching1);
            this.gbSearching.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbSearching.Location = new System.Drawing.Point(6, 101);
            this.gbSearching.Name = "gbSearching";
            this.gbSearching.Size = new System.Drawing.Size(376, 92);
            this.gbSearching.TabIndex = 6;
            this.gbSearching.TabStop = false;
            this.gbSearching.Text = "Searching";
            // 
            // checkBoxShowResultsAfterProgressCancel
            // 
            this.checkBoxShowResultsAfterProgressCancel.AutoSize = true;
            this.checkBoxShowResultsAfterProgressCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxShowResultsAfterProgressCancel.Location = new System.Drawing.Point(12, 66);
            this.checkBoxShowResultsAfterProgressCancel.Name = "checkBoxShowResultsAfterProgressCancel";
            this.checkBoxShowResultsAfterProgressCancel.Size = new System.Drawing.Size(329, 17);
            this.checkBoxShowResultsAfterProgressCancel.TabIndex = 3;
            this.checkBoxShowResultsAfterProgressCancel.Text = "Show already found subtitles after canceling search progress dlg";
            this.checkBoxShowResultsAfterProgressCancel.UseVisualStyleBackColor = true;
            // 
            // laSearching1
            // 
            this.laSearching1.AutoSize = true;
            this.laSearching1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.laSearching1.Location = new System.Drawing.Point(9, 20);
            this.laSearching1.Name = "laSearching1";
            this.laSearching1.Size = new System.Drawing.Size(150, 13);
            this.laSearching1.TabIndex = 1;
            this.laSearching1.Text = "Provider search time out (sec):";
            this.toolTip1.SetToolTip(this.laSearching1, resources.GetString("laSearching1.ToolTip"));
            // 
            // checkBoxUseLanguageCode
            // 
            this.checkBoxUseLanguageCode.AutoSize = true;
            this.checkBoxUseLanguageCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.checkBoxUseLanguageCode.Location = new System.Drawing.Point(12, 43);
            this.checkBoxUseLanguageCode.Name = "checkBoxUseLanguageCode";
            this.checkBoxUseLanguageCode.Size = new System.Drawing.Size(175, 17);
            this.checkBoxUseLanguageCode.TabIndex = 2;
            this.checkBoxUseLanguageCode.Text = "Show language code on results";
            this.checkBoxUseLanguageCode.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSearching1
            // 
            this.numericUpDownSearching1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.numericUpDownSearching1.Location = new System.Drawing.Point(167, 18);
            this.numericUpDownSearching1.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.numericUpDownSearching1.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericUpDownSearching1.Name = "numericUpDownSearching1";
            this.numericUpDownSearching1.Size = new System.Drawing.Size(66, 20);
            this.numericUpDownSearching1.TabIndex = 0;
            this.numericUpDownSearching1.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // gbDownloading
            // 
            this.gbDownloading.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.gbDownloading.Controls.Add(this.labelAfterDownloading);
            this.gbDownloading.Controls.Add(this.comboBoxAfterDownloading);
            this.gbDownloading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbDownloading.Location = new System.Drawing.Point(6, 199);
            this.gbDownloading.Name = "gbDownloading";
            this.gbDownloading.Size = new System.Drawing.Size(376, 49);
            this.gbDownloading.TabIndex = 7;
            this.gbDownloading.TabStop = false;
            this.gbDownloading.Text = "Downloading";
            // 
            // labelAfterDownloading
            // 
            this.labelAfterDownloading.AutoSize = true;
            this.labelAfterDownloading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelAfterDownloading.Location = new System.Drawing.Point(9, 20);
            this.labelAfterDownloading.Margin = new System.Windows.Forms.Padding(3);
            this.labelAfterDownloading.Name = "labelAfterDownloading";
            this.labelAfterDownloading.Size = new System.Drawing.Size(149, 13);
            this.labelAfterDownloading.TabIndex = 3;
            this.labelAfterDownloading.Text = "After downloading all subtitles:";
            // 
            // comboBoxAfterDownloading
            // 
            this.comboBoxAfterDownloading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxAfterDownloading.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAfterDownloading.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.comboBoxAfterDownloading.Items.AddRange(new object[] {
            "Do nothing",
            "Go back to originating plugin"});
            this.comboBoxAfterDownloading.Location = new System.Drawing.Point(167, 17);
            this.comboBoxAfterDownloading.Name = "comboBoxAfterDownloading";
            this.comboBoxAfterDownloading.Size = new System.Drawing.Size(197, 21);
            this.comboBoxAfterDownloading.TabIndex = 2;
            // 
            // AdvancedConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 356);
            this.Controls.Add(this.gbDownloading);
            this.Controls.Add(this.gbSearching);
            this.Controls.Add(this.gbGeneral);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.gbLogging);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdvancedConfigForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SubCentral advanced configuration";
            this.Load += new System.EventHandler(this.AdvancedConfigForm_Load);
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.AdvancedConfigForm_HelpButtonClicked);
            this.gbLogging.ResumeLayout(false);
            this.gbGeneral.ResumeLayout(false);
            this.gbGeneral.PerformLayout();
            this.gbSearching.ResumeLayout(false);
            this.gbSearching.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSearching1)).EndInit();
            this.gbDownloading.ResumeLayout(false);
            this.gbDownloading.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbLogging;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOpenLog;
        private System.Windows.Forms.Button btnOpenLogFolder;
        private System.Windows.Forms.GroupBox gbGeneral;
        private System.Windows.Forms.TextBox txtPluginName;
        private System.Windows.Forms.Label labelPluginName;
        private System.Windows.Forms.CheckBox checkBoxCheckMediaOnOpen;
        private System.Windows.Forms.CheckBox checkBoxHidePlugin;
        private System.Windows.Forms.GroupBox gbSearching;
        private System.Windows.Forms.GroupBox gbDownloading;
        private System.Windows.Forms.Label laSearching1;
        private System.Windows.Forms.NumericUpDown numericUpDownSearching1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox checkBoxUseLanguageCode;
        private System.Windows.Forms.Label labelAfterDownloading;
        private System.Windows.Forms.ComboBox comboBoxAfterDownloading;
        private System.Windows.Forms.CheckBox checkBoxShowResultsAfterProgressCancel;
    }
}