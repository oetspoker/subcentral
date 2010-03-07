using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NLog;
using System.Reflection;
using SubCentral.GUI;

namespace SubCentral.PluginHandlers {
    internal abstract class PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // The plugin ID for the plugin this handler implements support for.
        public abstract int ID {
            get;
        }

        // Should return the name of the source Plugin providing the data.
        public abstract string PluginName {
            get;
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

        // Should return the media details for which we want subtitles.
        public abstract BasicMediaDetail MediaDetail {
            get;
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
