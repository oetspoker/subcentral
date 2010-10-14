using System;
using System.Reflection;
using NLog;
using SubCentral.Utils;

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

        private TagRank tagRanking = null;
        public TagRank TagRanking {
            get {
                if (tagRanking == null) {
                    tagRanking = new TagRank(MediaDetail);
                }
                return tagRanking;
            }
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
                bool result = GrabFileDetails();
                tagRanking = new TagRank(MediaDetail);
                return result;
            }
            catch (Exception e) {
                logger.ErrorException("Failed updating\n", e);
            }
            return false;
        }

        public void UpdateTags() {
            tagRanking = null; // will update on next get
        }

        // Updates the above properties with current information from the associated plugin.
        protected abstract bool GrabFileDetails();

        // Should return true if all required components for this plugin are present.
        protected abstract bool IsAvailable();

        protected virtual PluginHandlerType GetPluginHandlerType() {
            return PluginHandlerType.BASIC;
        }

        public virtual void Clear() {
            UpdateTags();
        }

        public virtual int GetEmbeddedSubtitles() {
            return -1;
        }

        public virtual bool GetHasSubtitles(bool all) {
            return false;
        }

        public virtual void SetHasSubtitles(string fileName, bool value) {
        }

        // returns true if an assembly with the specified name is loaded
        protected bool IsAssemblyAvailable(string name, Version ver) {
            return SubCentralUtils.IsAssemblyAvailable(name, ver);
        }

    }
}
