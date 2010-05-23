using System.IO;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using SubCentral.Utils;
using SubCentral.Settings.Data;
using SubCentral.Enums;
using System.Runtime.Serialization;

namespace SubCentral.Settings {
    [XmlRoot("Settings")]
    public class SettingsManager {
        private SettingsManager() {
        }

        private static volatile SettingsManager _Settings = null;

        public static bool SettingsLoaded { get; set; }

        public static SettingsManager Properties {
            get {
                // DoubleLock
                if (_Settings == null) {
                    lock (_lock) {
                        if (_Settings == null) {
                            _Settings = new SettingsManager();
                        }
                    }
                }
                return _Settings;
            }

        }

        [XmlIgnore]
        private static object _lock = new object();

        public static bool Load(string path) {
            try {
                if (!System.IO.File.Exists(path)) {
                    Properties.GeneralSettings.AllProvidersEnabled = true;
                    Properties.GeneralSettings.AllProvidersForMovies = true;
                    Properties.GeneralSettings.AllProvidersForTVShows = true;
                    Properties.GeneralSettings.EnabledProvidersEnabled = true;
                    Properties.FolderSettings.OnDownloadFileName = OnDownloadFileName.AskIfManual;
                    Properties.GUISettings.SortMethod = SubtitlesSortMethod.SubtitleLanguage;
                    Properties.GUISettings.SortAscending = true;
                }
                using (FileStream fs = new FileStream(path, FileMode.Open)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsManager));

                    _Settings = (SettingsManager)xmlSerializer.Deserialize(fs);

                    SettingsLoaded = true;

                    return true;
                }
            }
            catch (System.Exception) {
                throw;
            }
        }

        public static bool Save() {
            return Save(Config.GetFile(Config.Dir.Config, SubCentralUtils.SettingsFileName));
        }

        public static bool Save(string path) {
            try {
                using (FileStream fs = new FileStream(path, FileMode.Create)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsManager));
                    xmlSerializer.Serialize(fs, Properties);
                    fs.Close();
                    return true;
                }
            }
            catch {
                throw;
            }
        }

        public SettingsGroupsAndProviders GeneralSettings;
        public SettingsLanguages LanguageSettings;
        public SettingsFolders FolderSettings;
        public SettingsGUI GUISettings;
    }
}
