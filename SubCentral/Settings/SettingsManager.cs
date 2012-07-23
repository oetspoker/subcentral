using System.IO;
using System.Xml.Serialization;
using MediaPortal.Configuration;
using NLog;
using SubCentral.Settings.Data;
using SubCentral.Utils;

namespace SubCentral.Settings {
    [XmlRoot("Settings")]
    public class SettingsManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();

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

            if (!File.Exists(path)) {
                logger.Info("SubCentral is used the first time. Default settings will be used");
                Save(path);
            }
            using (FileStream fs = new FileStream(path, FileMode.Open)) {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsManager));

                _Settings = (SettingsManager)xmlSerializer.Deserialize(fs);

                SettingsLoaded = true;

                return true;
            }
        }

        public static bool Save() {
            return Save(Config.GetFile(Config.Dir.Config, SubCentralUtils.SettingsFileName));
        }

        public static bool Save(string path) {
 
            if (File.Exists(path)) {
                string backupPath = path + ".bak";
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(path, backupPath);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create)) {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsManager));
                xmlSerializer.Serialize(fs, Properties);
                fs.Close();
                return true;
            }
        }

        public SettingsGroupsAndProviders GeneralSettings = new SettingsGroupsAndProviders();
        public SettingsLanguages LanguageSettings = new SettingsLanguages();
        public SettingsFolders FolderSettings = new SettingsFolders();
        public SettingsGUI GUISettings = new SettingsGUI();
    }
}
