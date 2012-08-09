using System;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace SubCentral.PluginHandlers {
    internal class PluginHandlerManager {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Dictionary<int, PluginHandler> handlers;
        private List<PluginHandler> naHandlers;

        public PluginHandlerManager() {
            BuildHandlerList();
        }

        public PluginHandler this[int pluginID] {
            get {
                // internal handler takes priority
                PluginHandler handler;
                //if (handlers.ContainsKey(pluginID) && handlers[pluginID].Available)
                if (handlers.TryGetValue(pluginID, out handler) && handler.Available)
                    return handler;

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
            naHandlers = new List<PluginHandler>();

            // load all supported plugin handlers into our dictionary
            foreach (Type t in this.GetType().Assembly.GetTypes())
                //if (t.BaseType == typeof(PluginHandler)) {
                if (t.IsSubclassOf(typeof(PluginHandler))) {
                    PluginHandler newHandler = (PluginHandler)t.GetConstructor(new Type[] { }).Invoke(null);
                    if (newHandler.Available) {
                        AddToActiveHandlers(newHandler);
                    }
                    else {
                        AddToInactiveHandlers(newHandler);
                    }
                }

            // log all the active plugin handlers
            foreach (PluginHandler h in handlers.Values.OrderBy(h => h.PluginName)) {
                if (h.Type == PluginHandlerType.BASIC || h.Type == PluginHandlerType.BASICWITHOUTDB)
                    logger.Info("Enabled plugin: {0} [{1}]", h.PluginName, h.ID);
            }

            // log all the inactive plugin handlers
            foreach (PluginHandler h in naHandlers.OrderBy(h => h.PluginName))
                if (h.Type == PluginHandlerType.BASIC || h.Type == PluginHandlerType.BASICWITHOUTDB)
                    logger.Info("Unavailable or outdated plugin: {0} [{1}]", h.PluginName, h.ID);
        }

        private void AddToActiveHandlers(PluginHandler handler) {
            PluginHandler existingHandler;
            if (handlers.TryGetValue(handler.ID, out existingHandler)) {
                logger.Info("Plugin handler conflict - handler {0} [{1}] will replace {2} [{3}]", handler.PluginName, handler.ID, existingHandler.PluginName, existingHandler.ID);
                AddToInactiveHandlers(existingHandler);
            }
            handlers[handler.ID] = handler;
        }

        private void AddToInactiveHandlers(PluginHandler handler) {
            naHandlers.Add(handler);
        }
    }
}
