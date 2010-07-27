using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.Configuration;
using NLog;
using SubCentral.GUI;
using SubCentral.Localizations;
using SubCentral.Settings;
using SubCentral.Settings.Data;
using SubCentral.Utils;
using MediaPortal.GUI.Library;

namespace SubCentral.ConfigForm {
    [PluginIcons("SubCentral.Config.Images.SubCentral_Icon_Enabled.png", "SubCentral.Config.Images.SubCentral_Icon_Disabled.png")]
    public partial class ConfigForm : Form, ISetupForm {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SubCentralCore core = SubCentralCore.Instance;

        //private bool loaded = false;

        public ConfigForm() {
            InitializeComponent();
            splitContainer1.Panel1MinSize = 250;
            splitContainer1.Panel2MinSize = 160;
        }

        #region Load / Save / Cancel
        private void initConfigForm() {
            //loaded = false;

            splitContainer1.Panel2Collapsed = true;
            SetEditGroupButtonText();

            listViewGroupsAndProviders.Items.Clear();
            listViewGroupsAndProviders.Groups[0].Items.Clear();
            listViewGroupsAndProviders.Groups[1].Items.Clear();

            listViewEditGroup.Items.Clear();

            listViewLanguages.Items.Clear();

            listViewFolders.Items.Clear();

            comboBoxPluginLoadWithSearchData.Items.Clear();

            comboBoxWhenDownloading.Items.Clear();

            comboBoxFileName.Items.Clear();

            lblProductVersion.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void ConfigForm_Load(object sender, EventArgs e) {
            if (!core.SubtitleDownloaderInitialized) {
                MessageBox.Show(String.Concat("Unable to load SubtitleDownloader library!", "\n", "Is SubtitleDownloader.dll available?"), "Error loading SubCentral configuration");
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            try {
                // clears the components and stuff
                initConfigForm();

                foreach (OnPluginLoadWithSearchData value in Enum.GetValues(typeof(OnPluginLoadWithSearchData))) {
                    comboBoxPluginLoadWithSearchData.Items.Add(StringEnum.GetStringValue(value) ?? string.Empty);
                }
                comboBoxPluginLoadWithSearchData.SelectedIndex = (int)SettingsManager.Properties.GeneralSettings.PluginLoadWithSearchData;

                foreach (OnDownload value in Enum.GetValues(typeof(OnDownload))) {
                    comboBoxWhenDownloading.Items.Add(StringEnum.GetStringValue(value) ?? string.Empty);
                }
                comboBoxWhenDownloading.SelectedIndex = (int)SettingsManager.Properties.FolderSettings.OnDownload;

                foreach (OnDownloadFileName value in Enum.GetValues(typeof(OnDownloadFileName))) {
                    comboBoxFileName.Items.Add(StringEnum.GetStringValue(value) ?? string.Empty);
                }
                comboBoxFileName.SelectedIndex = (int)SettingsManager.Properties.FolderSettings.OnDownloadFileName;

                checkBoxUseLanguageCode.Checked = SettingsManager.Properties.GeneralSettings.UseLanguageCodeOnResults;
                checkBoxSearchDefaultsWhenFromManualSearch.Checked = SettingsManager.Properties.GeneralSettings.SearchDefaultsWhenFromManualSearch;

                fillProviderGroupsFromSettings();
                setSubItemsForGroups();
                fillProvidersFromSettings();

                fillLanguages();

                fillFolders();
                setSubItemsForFolders();

                if (listViewGroupsAndProviders.SelectedItems.Count > 0)
                    listViewGroupsAndProviders.SelectedItems[0].Selected = false;
                listViewGroupsAndProviders.Items[0].Selected = true;
                listViewGroupsAndProviders.Items[0].Focused = true;
                listViewGroupsAndProviders.Focus();

                //loaded = true;
            }
            finally {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSave_Click(object sender, EventArgs ea) {
            Cursor.Current = Cursors.WaitCursor;
            try {
                bool selectionOK = false;
                foreach (ListViewItem item in listViewGroupsAndProviders.Groups[1].Items) {
                    if (!item.Checked) continue;
                    else {
                        selectionOK = true;
                        break;
                    }
                }

                if (!selectionOK) {
                    foreach (ListViewItem item in listViewGroupsAndProviders.Groups[0].Items) {
                        List<SettingsProvider> settingsProviders = ((SettingsGroup)item.Tag).Providers;

                        if (item.Index == 0 && item.Checked) {
                            selectionOK = true;
                            break;
                        }

                        if (!item.Checked || settingsProviders == null || settingsProviders.Count == 0) continue;

                        foreach (SettingsProvider settingsProvider in settingsProviders) {
                            if (settingsProvider.Enabled) {
                                selectionOK = true;
                                break;
                            }
                        }
                        if (selectionOK)
                            break;
                    }
                }

                if (!selectionOK) {
                    MessageBox.Show("You have to select at least one subtitle provider or group with at least one enabled provider!", "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listViewLanguages.CheckedItems.Count < 1) {
                    MessageBox.Show("You have to select at least one language!", "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (listViewFolders.CheckedItems.Count < 1) {
                    MessageBox.Show("You have to select at least one folder!", "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                saveProviderGroupsAndProviders();

                SettingsManager.Properties.GeneralSettings.PluginLoadWithSearchData = (OnPluginLoadWithSearchData)comboBoxPluginLoadWithSearchData.SelectedIndex;
                SettingsManager.Properties.GeneralSettings.UseLanguageCodeOnResults = checkBoxUseLanguageCode.Checked;
                SettingsManager.Properties.GeneralSettings.SearchDefaultsWhenFromManualSearch = checkBoxSearchDefaultsWhenFromManualSearch.Checked;

                SettingsManager.Properties.FolderSettings.OnDownload = (OnDownload)comboBoxWhenDownloading.SelectedIndex;
                SettingsManager.Properties.FolderSettings.OnDownloadFileName = (OnDownloadFileName)comboBoxFileName.SelectedIndex;

                logger.Info("Saving settings to {0}...", SubCentralUtils.SettingsFileName);
                SettingsManager.Save();
                logger.Info("Saving settings to {0} successful.", SubCentralUtils.SettingsFileName);

                Cursor.Current = Cursors.Default;
                Close();
            }
            catch (Exception e) {
                Cursor.Current = Cursors.Default;
                logger.ErrorException(string.Format("Configuration failed to save settings to {0}\n", SubCentralUtils.SettingsFileName), e);
                MessageBox.Show("There was an error when saving the settings file to " +
                    Config.GetFile(Config.Dir.Config, SubCentralUtils.SettingsFileName) + ".\n\n" + e.Message,
                    "Error saving settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region ISetupForm Members
        // Returns the name of the plugin which is shown in the plugin menu 
        public string PluginName() {
            return SubCentralUtils.PluginName();
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
            ConfigForm config = new ConfigForm();
            config.ShowDialog();
            //this.ShowDialog();
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
            strButtonText = SubCentralUtils.PluginName();
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = "hover_subcentral.png";
            if (SettingsManager.Properties != null && SettingsManager.Properties.GUISettings != null)
                return !SettingsManager.Properties.GUISettings.HidePlugin;
            else
                return true;
        }
        #endregion

        #region Helpers
        private ListViewItemType itemType(ListViewItem item) {
            if (item.Tag != null && item.Tag is SettingsGroup)
                return ListViewItemType.Group;
            if (item.Tag != null && item.Tag is SettingsProvider)
                return ListViewItemType.Provider;
            if (item.Tag != null && item.Tag is SettingsLanguage)
                return ListViewItemType.Language;
            if (item.Tag != null && item.Tag is SettingsFolder)
                return ListViewItemType.Folder;
            return ListViewItemType.Unknown;
        }

        private void checkUncheckAll(ListView listView, bool check) {
            listView.BeginUpdate();
            try {
                foreach (ListViewItem item in listView.Items) {
                    if (check)
                        item.Checked = true;
                    else
                        item.Checked = false;
                }
            }
            finally {
                listView.EndUpdate();
            }
        }

        private void doEnsureVisible(ListView listView, ListViewItem item) {
            int index = listView.Items.IndexOf(item);

            if (index < 0) return;

            if (listView.Items.Count > index + 2)
                listView.EnsureVisible(index + 2);
            else if (listView.Items.Count > index + 1)
                listView.EnsureVisible(index + 1);
            else
                listView.EnsureVisible(index);
        }
        #endregion

        #region ListView Events
        private void listViewGroupsAndProviders_SelectedIndexChanged(object sender, EventArgs e) {
            //if (!loaded) return;

            if (listViewGroupsAndProviders.SelectedIndices.Count > 0) {
                int itemindex = listViewGroupsAndProviders.SelectedIndices[0];
                ListViewItem item = listViewGroupsAndProviders.Items[itemindex];
                int itemindexInGroup = -1;
                if (item.Group != null)
                    itemindexInGroup = item.Group.Items.IndexOf(item);

                switch (itemType(item)) {
                    case ListViewItemType.Provider:
                        splitContainer1.Panel2Collapsed = true;
                        SetEditGroupButtonText();

                        btnGroupsAndProvidersRemoveGroup.Enabled = false;
                        btnGroupsAndProvidersEditGroup.Enabled = false;

                        btnGroupsAndProvidersUp.Enabled = itemindexInGroup > 0;
                        btnGroupsAndProvidersDown.Enabled = itemindexInGroup != listViewGroupsAndProviders.Groups[1].Items.Count - 1;

                        listViewEditGroup.Clear();
                        fillEditGroupSettingsFromGroup(null);
                        groupBoxEditGroup.Enabled = false;
                        break;
                    case ListViewItemType.Group:
                        btnGroupsAndProvidersRemoveGroup.Enabled = (itemindex != 0 && itemindex != 1);
                        btnGroupsAndProvidersEditGroup.Enabled = true;

                        btnGroupsAndProvidersUp.Enabled = itemindexInGroup > 2;
                        btnGroupsAndProvidersDown.Enabled = itemindexInGroup > 1 && itemindexInGroup != listViewGroupsAndProviders.Groups[0].Items.Count - 1;

                        SetGroupBoxEditGroupText();

                        groupBoxEditGroup.Enabled = true;
                        listViewEditGroup.Enabled = (itemindex != 0 && itemindex != 1);

                        listViewEditGroup.SelectedIndexChanged -= listViewEditGroup_SelectedIndexChanged;
                        try {
                            //compareEditGroupProvidersAndAddMissingFromLibrary(item.Tag as SettingsGroup);
                            if (itemindex == 0)
                                (item.Tag as SettingsGroup).Providers = SubCentralUtils.getAllProvidersAsEnabledOrDisabled(true);
                            fillEditGroupProvidersFromGroup(item.Tag as SettingsGroup);
                            fillEditGroupSettingsFromGroup(item.Tag as SettingsGroup);
                        }
                        finally {
                            listViewEditGroup.SelectedIndexChanged += new System.EventHandler(listViewEditGroup_SelectedIndexChanged);
                            if (listViewEditGroup.Items.Count > 0) {
                                listViewEditGroup.Items[0].Selected = false;
                                listViewEditGroup.Items[0].Selected = true;
                                listViewEditGroup.Items[0].Focused = true;
                            }
                        }
                        break;
                }
            }
            else {
                splitContainer1.Panel2Collapsed = true;
                SetEditGroupButtonText();

                btnGroupsAndProvidersRemoveGroup.Enabled = false;
                btnGroupsAndProvidersEditGroup.Enabled = false;
                btnGroupsAndProvidersUp.Enabled = false;
                btnGroupsAndProvidersDown.Enabled = false;
            }
        }

        private void listViewEditGroup_SelectedIndexChanged(object sender, EventArgs e) {
            //if (!loaded) return;

            if (listViewEditGroup.Enabled && listViewEditGroup.SelectedIndices.Count > 0) {
                int itemindex = listViewEditGroup.SelectedIndices[0];
                ListViewItem item = listViewEditGroup.Items[itemindex];

                btnEditGroupUp.Enabled = itemindex != 0;
                btnEditGroupDown.Enabled = itemindex != listViewEditGroup.Items.Count - 1;
            }
            else {
                btnEditGroupUp.Enabled = false;
                btnEditGroupDown.Enabled = false;
            }
        }

        private void listViewLanguages_SelectedIndexChanged(object sender, EventArgs e) {
            //if (!loaded) return;

            if (listViewLanguages.Enabled && listViewLanguages.SelectedIndices.Count > 0) {
                int itemindex = listViewLanguages.SelectedIndices[0];
                ListViewItem item = listViewLanguages.Items[itemindex];

                btnLanguagesUp.Enabled = itemindex != 0;
                btnLanguagesDown.Enabled = itemindex != listViewLanguages.Items.Count - 1;
            }
            else {
                btnLanguagesUp.Enabled = false;
                btnLanguagesDown.Enabled = false;
            }
        }

        private void listViewFolders_SelectedIndexChanged(object sender, EventArgs e) {
            //if (!loaded) return;

            if (listViewFolders.SelectedIndices.Count > 0) {
                int itemindex = listViewFolders.SelectedIndices[0];
                ListViewItem item = listViewFolders.Items[itemindex];
                if (itemType(item) == ListViewItemType.Folder) {
                    checkBoxFoldersDefaultMovies.Enabled = true;
                    checkBoxFoldersDefaultTVShows.Enabled = true;
                    checkBoxFoldersDefaultMovies.Checked = (item.Tag as SettingsFolder).DefaultForMovies;
                    checkBoxFoldersDefaultTVShows.Checked = (item.Tag as SettingsFolder).DefaultForTVShows;
                }
                else {
                    checkBoxFoldersDefaultMovies.Enabled = false;
                    checkBoxFoldersDefaultTVShows.Enabled = false;
                    checkBoxFoldersDefaultMovies.Checked = false;
                    checkBoxFoldersDefaultTVShows.Checked = false;
                }
            }
            else {
                checkBoxFoldersDefaultMovies.Enabled = false;
                checkBoxFoldersDefaultTVShows.Enabled = false;
                checkBoxFoldersDefaultMovies.Checked = false;
                checkBoxFoldersDefaultTVShows.Checked = false;
            }
        }

        private void listViewGroupsAndProviders_BeforeLabelEdit(object sender, LabelEditEventArgs e) {
            if (e.Item < 2)
                e.CancelEdit = true;
        }

        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e) {
            if (!(sender is ListView)) return;

            if (e.Label == null || (e.Label != null && string.IsNullOrEmpty(e.Label.Trim())))
                e.CancelEdit = true;
            else {
                ListViewItem item = (sender as ListView).Items[e.Item];
                switch (itemType(item)) {
                    case ListViewItemType.Group:
                        ((SettingsGroup)item.Tag).Title = e.Label;
                        break;
                    case ListViewItemType.Provider:
                        ((SettingsProvider)item.Tag).Title = e.Label;
                        break;
                }
            }
        }

        private void listView_ItemCheck(object sender, ItemCheckEventArgs e) {
            //if (!loaded) return;

            if (!(sender is ListView)) return;

            ListViewItem item = (sender as ListView).Items[e.Index];

            item.Selected = true;
            item.Focused = true;

            switch (itemType(item)) {
                case ListViewItemType.Group:
                    ((SettingsGroup)item.Tag).Enabled = e.NewValue == CheckState.Checked;
                    break;
                case ListViewItemType.Provider:
                    ((SettingsProvider)item.Tag).Enabled = e.NewValue == CheckState.Checked;
                    break;
                case ListViewItemType.Language:
                    ((SettingsLanguage)item.Tag).Enabled = e.NewValue == CheckState.Checked;
                    break;
                case ListViewItemType.Folder:
                    ((SettingsFolder)item.Tag).Enabled = e.NewValue == CheckState.Checked;
                    break;
            }

            //if (loaded && e.Index < 1)
            //    e.NewValue = e.CurrentValue;
        }
        #endregion

        #region Groups and Providers
        #region Up Down Move
        private void moveUpDown(int itemIndex, MoveItem moveItem) {
            ListViewItem item = listViewGroupsAndProviders.Items[itemIndex];
            ListViewGroup itemGroup = item.Group;
            int itemIndexInGroup = itemGroup.Items.IndexOf(item);
            //List<SettingsProvider> providersInGroupAll = (listViewGroupsAndProviders.Groups[0].Items[0].Tag as SettingsGroup).Providers;
            //List<SettingsProvider> providersInGroupAllEnabled = (listViewGroupsAndProviders.Groups[0].Items[1].Tag as SettingsGroup).Providers;

            int newItemIndex = -1;
            int newItemIndexInGroup = -1;
            bool condition = false;
            if (moveItem == MoveItem.Up) {
                newItemIndex = itemIndex - 1;
                newItemIndexInGroup = itemIndexInGroup - 1;
                condition = (newItemIndex >= 1);
            }
            else if (moveItem == MoveItem.Down) {
                newItemIndex = itemIndex + 1;
                newItemIndexInGroup = itemIndexInGroup + 1;
                condition = (newItemIndex < listViewGroupsAndProviders.Items.Count);
            }

            if (condition) {
                listViewGroupsAndProviders.BeginUpdate();
                listViewGroupsAndProviders.SelectedIndexChanged -= listViewGroupsAndProviders_SelectedIndexChanged;
                try {
                    //listViewGroupsAndProviders.Items.RemoveAt(itemIndex);
                    //itemGroup.Items.Remove(item);
                    //itemGroup.Items.RemoveAt(itemIndexInGroup);

                    //listViewGroupsAndProviders.Items.Insert(newItemIndex, item);
                    //itemGroup.Items.Insert(itemIndexInGroup, item);
                    //itemGroup.Items.Add(item);

                    //MoveInGroup(item, item.Group, newItemIndexInGroup);

                    switch (itemType(item)) {
                        case ListViewItemType.Group:
                            Settings.SettingsManager.Properties.GeneralSettings.Groups.RemoveAt(itemIndexInGroup - 2);
                            Settings.SettingsManager.Properties.GeneralSettings.Groups.Insert(newItemIndexInGroup - 2, item.Tag as SettingsGroup);
                            break;
                        case ListViewItemType.Provider:
                            Settings.SettingsManager.Properties.GeneralSettings.Providers.RemoveAt(itemIndexInGroup);
                            Settings.SettingsManager.Properties.GeneralSettings.Providers.Insert(newItemIndexInGroup, item.Tag as SettingsProvider);
                            break;
                    }

                    listViewGroupsAndProviders.Items.Clear();
                    listViewGroupsAndProviders.Groups[0].Items.Clear();
                    listViewGroupsAndProviders.Groups[1].Items.Clear();
                    fillProviderGroupsFromSettings();
                    setSubItemsForGroups();
                    fillProvidersFromSettings();
                }
                finally {
                    listViewGroupsAndProviders.SelectedIndexChanged += new System.EventHandler(listViewGroupsAndProviders_SelectedIndexChanged);
                    listViewGroupsAndProviders.Items[newItemIndex].Selected = false;
                    listViewGroupsAndProviders.Items[newItemIndex].Selected = true;
                    listViewGroupsAndProviders.Items[newItemIndex].Focused = true;
                    listViewGroupsAndProviders.EndUpdate();
                    //if (newItemIndex >= 0 && newItemIndex < listViewGroupsAndProviders.Items.Count) {
                    //    listViewGroupsAndProviders.EnsureVisible(newItemIndex);
                    //}
                    //listViewGroupsAndProviders.Select();
                    //listViewGroupsAndProviders.Items[newItemIndex].EnsureVisible();
                    doEnsureVisible(listViewGroupsAndProviders, listViewGroupsAndProviders.Items[newItemIndex]);
                }
            }
            listViewGroupsAndProviders.Select();
        }

        private void btnUp_Click(object sender, EventArgs e) {
            if (listViewGroupsAndProviders.SelectedIndices.Count > 0) {
                int itemIndex = listViewGroupsAndProviders.SelectedIndices[0];

                moveUpDown(itemIndex, MoveItem.Up);
            }
        }

        private void btnDown_Click(object sender, EventArgs e) {
            if (listViewGroupsAndProviders.SelectedIndices.Count > 0) {
                int itemIndex = listViewGroupsAndProviders.SelectedIndices[0];

                moveUpDown(itemIndex, MoveItem.Down);
            }
        }
        #endregion

        #region Events
        private void btnGroupsAndProvidersAddGroup_Click(object sender, EventArgs e) {
            ListViewItem item = new ListViewItem(Localization.NewGroup) {
                Tag = new SettingsGroup() { Title = Localization.NewGroup, Enabled = true, DefaultForMovies = false, DefaultForTVShows = false, Providers = SubCentralUtils.getAllProvidersAsEnabledOrDisabled(false) },
                Checked = true
            };

            listViewGroupsAndProviders.BeginUpdate();
            try {
                //listViewGroupsAndProviders.Groups[0].Items.Add(item);
                item.Group = listViewGroupsAndProviders.Groups[0];
                listViewGroupsAndProviders.Items.Insert(listViewGroupsAndProviders.Groups[0].Items.Count - 1, item);
                Settings.SettingsManager.Properties.GeneralSettings.Groups.Add(item.Tag as SettingsGroup);
                item.Checked = true;
                item.Selected = true;
                item.Focused = true;
            }
            finally {
                listViewGroupsAndProviders.EndUpdate();
                doEnsureVisible(listViewGroupsAndProviders, item);
                //listViewGroupsAndProviders.EnsureVisible(listViewGroupsAndProviders.Items.IndexOf(item));
                //item.EnsureVisible();
            }
            listViewGroupsAndProviders.Focus();

            EditGroup(true);

            listViewGroupsAndProviders.FocusedItem.BeginEdit();
        }

        private void btnGroupsAndProvidersRemoveGroup_Click(object sender, EventArgs e) {
            if (listViewGroupsAndProviders.SelectedIndices.Count > 0) {
                listViewGroupsAndProviders.BeginUpdate();
                try {
                    int itemindex = listViewGroupsAndProviders.SelectedIndices[0];
                    ListViewItem item = listViewGroupsAndProviders.Items[itemindex];
                    if (itemType(item) == ListViewItemType.Group) {
                        if (((SettingsGroup)item.Tag).DefaultForMovies)
                            ((SettingsGroup)listViewGroupsAndProviders.Items[0].Tag).DefaultForMovies = true;
                        if (((SettingsGroup)item.Tag).DefaultForTVShows)
                            ((SettingsGroup)listViewGroupsAndProviders.Items[0].Tag).DefaultForTVShows = true;

                        listViewGroupsAndProviders.Items.Remove(item);
                        listViewGroupsAndProviders.Groups[0].Items.Remove(item);

                        setSubItemsForGroups();

                        Settings.SettingsManager.Properties.GeneralSettings.Groups.Remove(item.Tag as SettingsGroup);

                        listViewGroupsAndProviders.Items[itemindex - 1].Selected = true;
                        listViewGroupsAndProviders.Items[itemindex - 1].Focused = true;
                    }
                }
                finally {
                    listViewGroupsAndProviders.EndUpdate();
                }
                listViewGroupsAndProviders.Focus();
            }
        }

        private void checkBoxEditGroupDefault_Click(object sender, EventArgs e) {
            if (!(sender is CheckBox)) return;

            if (!(sender as CheckBox).Checked) (sender as CheckBox).Checked = true;

            if (listViewGroupsAndProviders.SelectedItems.Count > 0 && itemType(listViewGroupsAndProviders.SelectedItems[0]) == ListViewItemType.Group) {
                SettingsGroup settingsGroup = listViewGroupsAndProviders.SelectedItems[0].Tag as SettingsGroup;

                if (sender.Equals(checkBoxEditGroupDefaultMovies)) {
                    foreach (ListViewItem item in listViewGroupsAndProviders.Groups[0].Items) {
                        if (itemType(item) == ListViewItemType.Group) {
                            ((SettingsGroup)item.Tag).DefaultForMovies = false;
                            if (item.SubItems.Count > 1)
                                item.SubItems.RemoveAt(1);
                        }
                    }
                    settingsGroup.DefaultForMovies = checkBoxEditGroupDefaultMovies.Checked;
                }
                else if (sender.Equals(checkBoxEditGroupDefaultTVShows)) {
                    foreach (ListViewItem item in listViewGroupsAndProviders.Groups[0].Items) {
                        if (itemType(item) == ListViewItemType.Group) {
                            ((SettingsGroup)item.Tag).DefaultForTVShows = false;
                            if (item.SubItems.Count > 1)
                                item.SubItems.RemoveAt(1);
                        }
                    }
                    settingsGroup.DefaultForTVShows = checkBoxEditGroupDefaultTVShows.Checked;
                }

                setSubItemsForGroups();
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            listViewGroupsAndProviders.BeginUpdate();
            listViewGroupsAndProviders.ItemCheck -= listView_ItemCheck;
            try {
                foreach (ListViewItem item in listViewGroupsAndProviders.Groups[0].Items) {
                    item.Checked = true;
                }
            }
            finally {
                listViewGroupsAndProviders.ItemCheck += new ItemCheckEventHandler(listView_ItemCheck);

                if (listViewGroupsAndProviders.SelectedItems.Count > 0) {
                    ListViewItem item = listViewGroupsAndProviders.SelectedItems[0];
                    item.Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                }

                listViewGroupsAndProviders.EndUpdate();
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            listViewGroupsAndProviders.BeginUpdate();
            listViewGroupsAndProviders.ItemCheck -= listView_ItemCheck;
            try {
                foreach (ListViewItem item in listViewGroupsAndProviders.Groups[1].Items) {
                    item.Checked = true;
                }
            }
            finally {
                listViewGroupsAndProviders.ItemCheck += new ItemCheckEventHandler(listView_ItemCheck);

                if (listViewGroupsAndProviders.SelectedItems.Count > 0) {
                    ListViewItem item = listViewGroupsAndProviders.SelectedItems[0];
                    item.Selected = false;
                    item.Selected = true;
                    item.Focused = true;
                }

                listViewGroupsAndProviders.EndUpdate();
            }
        }
        #endregion

        private void fillProviderGroupsFromSettings() {
            listViewGroupsAndProviders.BeginUpdate();
            try {
                List<SettingsGroup> groups = SubCentralUtils.getAllProviderGroups();
                //Settings.SettingsManager.Properties.GeneralSettings.Groups;

                if (groups == null || groups.Count == 0) return;

                foreach (SettingsGroup group in groups) {
                    ListViewItem item = new ListViewItem(group.Title) {
                        Tag = group,
                        Checked = group.Enabled
                    };

                    //listViewGroupsAndProviders.Groups[0].Items.Add(item);
                    item.Group = listViewGroupsAndProviders.Groups[0];
                    listViewGroupsAndProviders.Items.Insert(listViewGroupsAndProviders.Groups[0].Items.Count - 1, item);
                }
            }
            finally {
                listViewGroupsAndProviders.EndUpdate();
            }
        }

        private void fillProvidersFromSettings() {
            listViewGroupsAndProviders.BeginUpdate();
            try {
                List<SettingsProvider> providers = SubCentralUtils.getAllProviders();
                //Settings.SettingsManager.Properties.GeneralSettings.Providers;

                if (providers == null || providers.Count == 0) return;

                foreach (SettingsProvider provider in providers) {
                    ListViewItem item = new ListViewItem(provider.Title) {
                        Font = new Font(Font, FontStyle.Regular),
                        Tag = provider,
                        Checked = provider.Enabled
                    };

                    //listViewGroupsAndProviders.Groups[1].Items.Add(item);
                    item.Group = listViewGroupsAndProviders.Groups[1];
                    listViewGroupsAndProviders.Items.Insert(listViewGroupsAndProviders.Groups[0].Items.Count + listViewGroupsAndProviders.Groups[1].Items.Count - 1, item);
                }
            }
            finally {
                listViewGroupsAndProviders.EndUpdate();
            }
        }

        private void setSubItemsForGroups() {
            listViewGroupsAndProviders.BeginUpdate();
            try {
                foreach (ListViewItem item in listViewGroupsAndProviders.Groups[0].Items) {
                    if (itemType(item) == ListViewItemType.Group) {
                        if (item.SubItems.Count > 1)
                            item.SubItems.RemoveAt(1);
                        string subText = getDefaultMoviesTVShowsTextFromGroup(item.Tag as SettingsGroup);
                        if (!string.IsNullOrEmpty(subText))
                            item.SubItems.Add(subText);
                    }
                }
            }
            finally {
                listViewGroupsAndProviders.EndUpdate();
            }
        }

        private string getDefaultMoviesTVShowsTextFromGroup(SettingsGroup group) {
            if (group.DefaultForMovies || group.DefaultForTVShows) {
                string text = string.Empty;
                if (group.DefaultForMovies)
                    text = Localization.Movies;
                if (group.DefaultForTVShows && string.IsNullOrEmpty(text))
                    text = Localization.TVShows;
                else if (group.DefaultForTVShows)
                    text = text + ", " + Localization.TVShows;

                return string.Format(Localization.DefaultFor, text);
            }
            return string.Empty;
        }

        private void saveProviderGroupsAndProviders() {
            Settings.SettingsManager.Properties.GeneralSettings.Groups.Clear();
            int index = -1;
            foreach (ListViewItem groupItem in listViewGroupsAndProviders.Groups[0].Items) {
                SettingsGroup groupItemTag = (SettingsGroup)groupItem.Tag;
                index++;
                if (index == 0) {
                    Settings.SettingsManager.Properties.GeneralSettings.AllProvidersEnabled = groupItemTag.Enabled;
                    Settings.SettingsManager.Properties.GeneralSettings.AllProvidersForMovies = groupItemTag.DefaultForMovies;
                    Settings.SettingsManager.Properties.GeneralSettings.AllProvidersForTVShows = groupItemTag.DefaultForTVShows;
                }
                else if (index == 1) {
                    Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersEnabled = groupItemTag.Enabled;
                    Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersForMovies = groupItemTag.DefaultForMovies;
                    Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersForTVShows = groupItemTag.DefaultForTVShows;
                }
                else {
                    Settings.SettingsManager.Properties.GeneralSettings.Groups.Add(groupItemTag);
                }
            }

            Settings.SettingsManager.Properties.GeneralSettings.Providers.Clear();
            foreach (ListViewItem providerItem in listViewGroupsAndProviders.Groups[1].Items) {
                SettingsProvider settingsProvider = (SettingsProvider)providerItem.Tag;
                Settings.SettingsManager.Properties.GeneralSettings.Providers.Add(settingsProvider);
            }
        }
        #endregion

        #region Group Providers
        private void fillEditGroupProvidersFromGroup(SettingsGroup settingsGroup) {
            listViewEditGroup.BeginUpdate();
            try {
                listViewEditGroup.Clear();
                if (settingsGroup == null || settingsGroup.Providers == null || settingsGroup.Providers.Count == 0) return;
                foreach (SettingsProvider provider in settingsGroup.Providers) {
                    ListViewItem item = new ListViewItem(provider.Title) {
                        Font = new Font(Font, FontStyle.Regular),
                        Checked = provider.Enabled,
                        Tag = provider
                    };

                    listViewEditGroup.Items.Add(item);
                }
            }
            finally {
                listViewEditGroup.EndUpdate();
            }
        }

        private void fillEditGroupSettingsFromGroup(SettingsGroup settingsGroup) {
            if (settingsGroup != null) {
                checkBoxEditGroupDefaultMovies.Checked = settingsGroup.DefaultForMovies;
                checkBoxEditGroupDefaultTVShows.Checked = settingsGroup.DefaultForTVShows;
            }
            else {
                checkBoxEditGroupDefaultMovies.Checked = false;
                checkBoxEditGroupDefaultTVShows.Checked = false;
            }
        }

        private void btnEditGroupUpDown_Click(object sender, EventArgs e) {
            MoveItem moveItem;
            if (sender.Equals(btnEditGroupUp))
                moveItem = MoveItem.Up;
            else if (sender.Equals(btnEditGroupDown))
                moveItem = MoveItem.Down;
            else return;

            if (listViewEditGroup.SelectedIndices.Count > 0 && listViewEditGroup.SelectedItems.Count > 0 &&
                listViewGroupsAndProviders.SelectedItems.Count > 0 && itemType(listViewGroupsAndProviders.SelectedItems[0]) == ListViewItemType.Group) {
                int itemIndex = listViewEditGroup.SelectedIndices[0];
                ListViewItem item = listViewEditGroup.SelectedItems[0];
                List<SettingsProvider> providersInGroup = (listViewGroupsAndProviders.SelectedItems[0].Tag as SettingsGroup).Providers;

                if (providersInGroup.Count != listViewEditGroup.Items.Count) return;

                int newItemIndex = -1;
                bool condition = false;
                if (moveItem == MoveItem.Up) {
                    newItemIndex = itemIndex - 1;
                    condition = (newItemIndex >= 0);
                }
                else if (moveItem == MoveItem.Down) {
                    newItemIndex = itemIndex + 1;
                    condition = (newItemIndex < listViewEditGroup.Items.Count);
                }

                if (condition) {
                    listViewEditGroup.BeginUpdate();
                    listViewEditGroup.SelectedIndexChanged -= listViewEditGroup_SelectedIndexChanged;
                    try {
                        listViewEditGroup.Items.RemoveAt(itemIndex);
                        providersInGroup.RemoveAt(itemIndex);
                        listViewEditGroup.Items.Insert(newItemIndex, item);
                        providersInGroup.Insert(newItemIndex, item.Tag as SettingsProvider);
                    }
                    finally {
                        listViewEditGroup.SelectedIndexChanged += new System.EventHandler(listViewEditGroup_SelectedIndexChanged);
                        listViewEditGroup.Items[newItemIndex].Selected = false;
                        listViewEditGroup.Items[newItemIndex].Selected = true;
                        listViewEditGroup.Items[newItemIndex].Focused = true;
                        listViewEditGroup.EndUpdate();
                        //if (newItemIndex >= 0 && newItemIndex < listViewEditGroup.Items.Count) {
                        //    listViewEditGroup.EnsureVisible(newItemIndex);
                        //}
                        item.EnsureVisible();
                    }
                }
            }
            listViewEditGroup.Select();
        }
        #endregion

        #region Languages
        private void fillLanguages() {
            listViewLanguages.BeginUpdate();
            try {
                List<SettingsLanguage> languages = SubCentralUtils.getAllLanguages();

                if (languages == null || languages.Count == 0) return;

                foreach (SettingsLanguage language in languages) {
                    ListViewItem item = new ListViewItem(language.LanguageName) {
                        Font = new Font(Font, FontStyle.Regular),
                        Tag = language,
                        Checked = language.Enabled
                    };

                    listViewLanguages.Items.Add(item);
                }
            }
            finally {
                if (listViewLanguages.Items.Count > 0) {
                    listViewLanguages.Items[0].Selected = false;
                    listViewLanguages.Items[0].Selected = true;
                    listViewLanguages.Items[0].Focused = true;
                }
                listViewLanguages.EndUpdate();
                if (listViewLanguages.Items.Count > 0) {
                    listViewLanguages.Items[0].EnsureVisible();
                }
            }
        }

        private void btnLanguagesUpDown_Click(object sender, EventArgs e) {
            MoveItem moveItem;
            if (sender.Equals(btnLanguagesUp))
                moveItem = MoveItem.Up;
            else if (sender.Equals(btnLanguagesDown))
                moveItem = MoveItem.Down;
            else return;

            if (listViewLanguages.SelectedIndices.Count > 0 && listViewLanguages.SelectedItems.Count > 0) {
                int itemIndex = listViewLanguages.SelectedIndices[0];
                ListViewItem item = listViewLanguages.SelectedItems[0];

                int newItemIndex = -1;
                bool condition = false;
                if (moveItem == MoveItem.Up) {
                    newItemIndex = itemIndex - 1;
                    condition = (newItemIndex >= 0);
                }
                else if (moveItem == MoveItem.Down) {
                    newItemIndex = itemIndex + 1;
                    condition = (newItemIndex < listViewLanguages.Items.Count);
                }

                if (condition) {
                    listViewLanguages.BeginUpdate();
                    listViewLanguages.SelectedIndexChanged -= listViewLanguages_SelectedIndexChanged;
                    try {
                        listViewLanguages.Items.RemoveAt(itemIndex);
                        listViewLanguages.Items.Insert(newItemIndex, item);
                        Settings.SettingsManager.Properties.LanguageSettings.Languages.RemoveAt(itemIndex);
                        Settings.SettingsManager.Properties.LanguageSettings.Languages.Insert(newItemIndex, item.Tag as SettingsLanguage);
                    }
                    finally {
                        listViewLanguages.SelectedIndexChanged += new System.EventHandler(listViewLanguages_SelectedIndexChanged);
                        listViewLanguages.Items[newItemIndex].Selected = false;
                        listViewLanguages.Items[newItemIndex].Selected = true;
                        listViewLanguages.Items[newItemIndex].Focused = true;
                        listViewLanguages.EndUpdate();
                        item.EnsureVisible();
                    }
                }
            }
            listViewLanguages.Select();
        }
        #endregion

        #region Folders
        private void fillFolders() {
            listViewFolders.BeginUpdate();
            try {
                List<SettingsFolder> folders = SubCentralUtils.AllFolders;

                if (folders == null || folders.Count == 0) return;

                foreach (SettingsFolder folder in folders) {
                    ListViewItem item = new ListViewItem(folder.Folder) {
                        Font = new Font(Font, FontStyle.Regular),
                        Tag = folder,
                        Checked = folder.Enabled
                    };
                    if (Path.IsPathRooted(folder.Folder)) {
                        switch (SubCentralUtils.getFolderErrorInfo(folder.Folder)) {
                            case FolderErrorInfo.NonExistant:
                                item.Font = new Font(Font, FontStyle.Italic);
                                item.ForeColor = Color.Gray;
                                break;
                            case FolderErrorInfo.ReadOnly:
                                item.Font = new Font(Font, FontStyle.Italic);
                                item.ForeColor = Color.DarkRed;
                                break;
                        }
                    }
                    listViewFolders.Items.Add(item);
                }
            }
            finally {
                if (listViewFolders.Items.Count > 0) {
                    listViewFolders.Items[0].Selected = false;
                    listViewFolders.Items[0].Selected = true;
                    listViewFolders.Items[0].Focused = true;
                }
                listViewFolders.EndUpdate();
                if (listViewFolders.Items.Count > 0) {
                    listViewFolders.Items[0].EnsureVisible();
                }
            }
        }

        private void checkBoxFoldersDefault_Click(object sender, EventArgs e) {
            if (!(sender is CheckBox)) return;

            if (listViewFolders.SelectedItems.Count > 0 && itemType(listViewFolders.SelectedItems[0]) == ListViewItemType.Folder) {
                SettingsFolder settingsFolder = listViewFolders.SelectedItems[0].Tag as SettingsFolder;

                if (sender.Equals(checkBoxFoldersDefaultMovies)) {
                    foreach (ListViewItem item in listViewFolders.Items) {
                        if (itemType(item) == ListViewItemType.Folder) {
                            ((SettingsFolder)item.Tag).DefaultForMovies = false;
                            if (item.SubItems.Count > 1)
                                item.SubItems.RemoveAt(1);
                        }
                    }
                    settingsFolder.DefaultForMovies = checkBoxFoldersDefaultMovies.Checked;
                }
                else if (sender.Equals(checkBoxFoldersDefaultTVShows)) {
                    foreach (ListViewItem item in listViewFolders.Items) {
                        if (itemType(item) == ListViewItemType.Folder) {
                            ((SettingsFolder)item.Tag).DefaultForTVShows = false;
                            if (item.SubItems.Count > 1)
                                item.SubItems.RemoveAt(1);
                        }
                    }
                    settingsFolder.DefaultForTVShows = checkBoxFoldersDefaultTVShows.Checked;
                }

                setSubItemsForFolders();
            }
        }

        private void setSubItemsForFolders() {
            listViewFolders.BeginUpdate();
            try {
                foreach (ListViewItem item in listViewFolders.Items) {
                    if (itemType(item) == ListViewItemType.Folder) {
                        if (item.SubItems.Count > 1)
                            item.SubItems.RemoveAt(1);
                        string subText = getDefaultMoviesTVShowsTextFromFolder(item.Tag as SettingsFolder);
                        if (!string.IsNullOrEmpty(subText))
                            item.SubItems.Add(subText);
                    }
                }
            }
            finally {
                listViewFolders.EndUpdate();
            }
        }

        private string getDefaultMoviesTVShowsTextFromFolder(SettingsFolder settingsFolder) {
            if (settingsFolder.DefaultForMovies || settingsFolder.DefaultForTVShows) {
                string text = string.Empty;
                if (settingsFolder.DefaultForMovies)
                    text = Localization.Movies;
                if (settingsFolder.DefaultForTVShows && string.IsNullOrEmpty(text))
                    text = Localization.TVShows;
                else if (settingsFolder.DefaultForTVShows)
                    text = text + ", " + Localization.TVShows;

                return string.Format(Localization.DefaultFor, text);
            }
            return string.Empty;
        }
        #endregion

        private void btnAdvancedConfig_Click(object sender, EventArgs e) {
            AdvancedConfigForm acf = new AdvancedConfigForm();
            acf.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e) {
            EditGroup();
        }

        private void EditGroup() {
            EditGroup(false);
        }

        private void EditGroup(bool persistOpen) {
            splitContainer1.Panel2Collapsed = !persistOpen && !splitContainer1.Panel2Collapsed;
            SetEditGroupButtonText();
            SetGroupBoxEditGroupText();
        }

        private void SetGroupBoxEditGroupText() {
            if (!splitContainer1.Panel2Collapsed && listViewGroupsAndProviders.SelectedItems.Count > 0)
                groupBoxEditGroup.Text = string.Format("Edit group: {0}  ", listViewGroupsAndProviders.SelectedItems[0].Text);
            else
                groupBoxEditGroup.Text = "Edit group ";
        }

        private void SetEditGroupButtonText() {
            if (splitContainer1.Panel2Collapsed)
                btnGroupsAndProvidersEditGroup.Text = "Edit group";
            else
                btnGroupsAndProvidersEditGroup.Text = "Hide edit group";
        }

    }
}
