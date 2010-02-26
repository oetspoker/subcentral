using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog.Config;
using NLog.Targets;
using NLog;
using System.IO;
using MediaPortal.Configuration;
using MediaPortal.Services;
using System.Reflection;
using SubCentral.PluginHandlers;


namespace SubCentral {
    public class SubCentralCore {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string logFileName = "subcentral.log";
        private const string oldLogFileName = "subcentral.old.log";

        // Returns instance to this class as we only want to have one 
        // in existance at a time.
        public static SubCentralCore Instance {
            get {
                if (_instance == null)
                    _instance = new SubCentralCore();

                return _instance;
            }
        }
        private static SubCentralCore _instance = null;

        internal PluginHandlerManager PluginHandlers {
            get { return _pluginHandlers; }
        }
        private PluginHandlerManager _pluginHandlers;

        // Returns latests settings from the MediaPortal.xml file. We reload
        // on access to ensure any changes made while the program runs are honored.
        public MediaPortal.Profile.Settings MediaPortalSettings {
            get {
                MediaPortal.Profile.Settings mpSettings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
                return mpSettings;
            }
        }

        // Constructor. Private because we are a singleton.
        private SubCentralCore() { }

        // Should be the first thing that is run whenever the plugin launches, either
        // from the GUI or the Config Screen.
        public void Initialize() {
            InitLogger();
            LogStartupBanner();

            _pluginHandlers = new PluginHandlerManager(); 
        }

        // Initializes the logging system.
        private void InitLogger() {
            string fullLogFilePath = Config.GetFile(Config.Dir.Log, logFileName);
            string fullOldLogFilePath = Config.GetFile(Config.Dir.Log, oldLogFileName);
            

            // backup the old log file if it exists
            try {
                if (File.Exists(fullLogFilePath)) File.Copy(fullLogFilePath, fullOldLogFilePath, true);
                File.Delete(fullLogFilePath);
            }
            catch (Exception) { }

            // build logging rules for our logger
            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, logFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            LoggingConfiguration config = new LoggingConfiguration();
            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = MediaPortalSettings;
            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0)) {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

            // if the plugin was compiled in DEBUG mode, always default to debug logging
            #if DEBUG
            logLevel = LogLevel.Debug;
            #endif

            // add the previously defined rules and targets to the logging configuration
            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
        }

        // Logs a startup message to the log files.
        private void LogStartupBanner() {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            logger.Info("SubCentral (" + ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision + ")");
            logger.Info("Plugin Launched");
        }
    }
}
