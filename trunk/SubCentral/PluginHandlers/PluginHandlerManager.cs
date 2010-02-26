using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Reflection;

namespace SubCentral.PluginHandlers {
    internal class PluginHandlerManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, PluginHandler> handlers;
        
        public PluginHandlerManager() {
            BuildHandlerList();
        }

        public PluginHandler this[int pluginID] {
            get {
                if (handlers.ContainsKey(pluginID))
                    return handlers[pluginID];

                return null;
            }
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
            foreach (PluginHandler h in handlers.Values) 
                if (h.Available) logger.Info("Enabled Plugin: {0}", h.PluginName);

            // log all the inactive plugin handlers
            foreach (PluginHandler h in handlers.Values) 
                if (!h.Available) logger.Info("Unavailable or Outdated Plugin: {0}", h.PluginName);
        }
    }
}
