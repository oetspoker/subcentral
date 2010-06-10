using System;
using System.Collections.Generic;
using NLog;

namespace SubCentral.PluginHandlers {
    internal class PluginHandlerManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, PluginHandler> handlers;

        public PluginHandlerManager() {
            BuildHandlerList();
        }

        public PluginHandler this[int pluginID] {
            get {
                // internal handler takes priority
                if (handlers.ContainsKey(pluginID))
                    return handlers[pluginID];

                // check temporary custom handler
                if (handlers[-1].ID == pluginID)
                    return handlers[-1];

                // manual handler

                return null;
            }
        }

        public PluginHandler this[PluginHandlerType type] {
            get {
                foreach (PluginHandler h in handlers.Values) {
                    if (h.Type == type && h.Available) {
                        return h;
                    }
                }
                return null;
            }
        }

        public void ClearCustomHandlers() {
            if (handlers[-1] != null)
                handlers[-1].Clear();
            if (handlers[-2] != null)
                handlers[-2].Clear();
        }

        private void BuildHandlerList() {
            logger.Info("Checking for supported plugins...");
            handlers = new Dictionary<int, PluginHandler>();

            // load all supported plugin handlers into our dictionary
            foreach (Type t in this.GetType().Assembly.GetTypes())
                if (t.BaseType == typeof(PluginHandler)) {
                    PluginHandler newHandler = (PluginHandler)t.GetConstructor(new Type[] { }).Invoke(null);
                    handlers[newHandler.ID] = newHandler;
                }

            // log all the active plugin handlers
            foreach (PluginHandler h in handlers.Values) {
                if (h.Type == PluginHandlerType.BASIC && h.Available) logger.Info("Enabled Plugin: {0}", h.PluginName);
            }

            // log all the inactive plugin handlers
            foreach (PluginHandler h in handlers.Values)
                if (h.Type == PluginHandlerType.BASIC && !h.Available) logger.Info("Unavailable or Outdated Plugin: {0}", h.PluginName);
        }
    }
}
