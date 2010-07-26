using System;
using System.Reflection;
using NLog;

namespace SubCentral.PluginHandlers {
    internal abstract class PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PluginHandlerType Type {
            get {
                return GetPluginHandlerType();
            }
        }

        // The plugin ID for the plugin this handler implements support for.
        public abstract int ID {
            get;
            set;
        }

        // Should return the name of the source Plugin providing the data.
        public abstract string PluginName {
            get;
            set;
        }

        // Should return the media details for which we want subtitles.
        public abstract BasicMediaDetail MediaDetail {
            get;
            set;
        }

        public abstract bool Modified {
            get;
            set;
        }

        // Should return true if all required components for this plugin are present.
        // See IsActive() abstract method below.
        public bool Available {
            get {
                try {
                    return IsAvailable();
                }
                catch (Exception) { }
                return false;
            }
        }

        // Updates the above properties with current information from the associated plugin.
        // See abstract GrabFileDetails() method below.
        public bool Update() {
            try {
                return GrabFileDetails();
            }
            catch (Exception e) {
                logger.ErrorException("Failed updating.", e);
            }
            return false;
        }

        // Updates the above properties with current information from the associated plugin.
        protected abstract bool GrabFileDetails();

        // Should return true if all required components for this plugin are present.
        protected abstract bool IsAvailable();

        protected virtual PluginHandlerType GetPluginHandlerType() {
            return PluginHandlerType.BASIC;
        }

        public virtual void Clear() {
        }

        public virtual int GetEmbeddedSubtitles() {
            return -1;
        }

        public virtual bool GetHasSubtitles() {
            return false;
        }

        public virtual void SetHasSubtitles(string fileName, bool value) {
        }

        // returns true if an assembly with the specified name is loaded
        protected bool IsAssemblyAvailable(string name, Version ver) {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly a in assemblies)
                if (a.GetName().Name == name && a.GetName().Version >= ver)
                    return true;

            return false;
        }

    }
}
