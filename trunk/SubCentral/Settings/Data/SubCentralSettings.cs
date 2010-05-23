using System.Collections.Generic;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;
using SubCentral.Enums;

namespace SubCentral.Settings.Data
{
    #region GroupsAndProviders
    public struct SettingsGroupsAndProviders
    {
        public OnPluginLoadWithSearchData PluginLoadWithSearchData { get; set; }
        public bool UseLanguageCodeOnResults { get; set; }
        public bool SearchDefaultsWhenFromManualSearch { get; set; }
        
        public bool AllProvidersEnabled { get; set; }
        public bool AllProvidersForMovies { get; set; }
        public bool AllProvidersForTVShows { get; set; }

        public bool EnabledProvidersEnabled { get; set; }
        public bool EnabledProvidersForMovies { get; set; }
        public bool EnabledProvidersForTVShows { get; set; }

        [XmlIgnore]
        private List<SettingsGroup> _groups;

        [XmlElement("Group")]
        public List<SettingsGroup> Groups {
            get { return _groups ?? (_groups = new List<SettingsGroup>()); }
            set {
                if (_groups == null) _groups = new List<SettingsGroup>();
                _groups = value;
            }
        }

        [XmlIgnore]
        private List<SettingsProvider> _providers;

        [XmlElement("Provider")]
        public List<SettingsProvider> Providers
        {
            get { return _providers ?? (_providers = new List<SettingsProvider>()); }
            set
            {
                if (_providers == null) _providers = new List<SettingsProvider>();
                _providers = value;
            }
        }
    }

    public class SettingsGroup {
        public string Title { get; set; }
        public bool Enabled { get ; set; }
        public bool DefaultForMovies { get; set; }
        public bool DefaultForTVShows { get; set; }

        [XmlIgnore]
        private List<SettingsProvider> _providers;

        [XmlElement("Provider")]
        public List<SettingsProvider> Providers {
            get { return _providers ?? (_providers = new List<SettingsProvider>()); }
            set {
                if (_providers == null) _providers = new List<SettingsProvider>();
                _providers = value;
            }
        }
    }

    public class SettingsProvider
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }
        public string Title { get; set; }
        public bool Enabled { get; set; }
    }
    #endregion

    #region Languages
    public struct SettingsLanguages {
        [XmlIgnore]
        private List<SettingsLanguage> _languages;

        [XmlElement("Language")]
        public List<SettingsLanguage> Languages {
            get { return _languages ?? (_languages = new List<SettingsLanguage>()); }
            set {
                if (_languages == null) _languages = new List<SettingsLanguage>();
                _languages = value;
            }
        }
    }

    public class SettingsLanguage {
        [XmlAttribute("Code")]
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public bool Enabled { get; set; }
    }
    #endregion

    #region Folders
    public struct SettingsFolders {
        public OnDownload OnDownload { get; set; }
        public OnDownloadFileName OnDownloadFileName { get; set; }

        [XmlIgnore]
        private List<SettingsFolder> _folders;

        [XmlElement("Folder")]
        public List<SettingsFolder> Folders {
            get { return _folders ?? (_folders = new List<SettingsFolder>()); }
            set {
                if (_folders == null) _folders = new List<SettingsFolder>();
                _folders = value;
            }
        }
    }

    public class SettingsFolder {
        public string Folder { get; set; }
        public bool Enabled { get; set; }
        public bool DefaultForMovies { get; set; }
        public bool DefaultForTVShows { get; set; }
    }
    #endregion

    #region GUI
    public struct SettingsGUI {
        public SubtitlesSortMethod SortMethod { get; set; }
        public bool SortAscending { get; set; }
    }
    #endregion

}
