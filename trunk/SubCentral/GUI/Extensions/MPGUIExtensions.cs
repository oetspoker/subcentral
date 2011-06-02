using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;

namespace SubCentral.GUI.Extensions {
    public static class MPGUIExtensions {
        /// <summary>
        /// Same as Children and controlList but used for backwards compatibility between mediaportal 1.1 and 1.2
        /// </summary>
        /// <param name="self"></param>
        /// <returns>IEnumerable of GUIControls</returns>
        public static IEnumerable GetControlList(this GUIWindow self) {
            PropertyInfo property = GetPropertyInfo<GUIWindow>("Children", null);
            return (IEnumerable)property.GetValue(self, null);
        }

        public static GUIWindow GetWindowEx(int ID, bool reloadSkin) {
            #if MP11
            FieldInfo field = GetFieldInfo<GUIWindowManager>("_listWindows", "_listWindows", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null) {
                GUIWindow[] windows = field.GetValue(null) as GUIWindow[];
                if (windows != null) {
                    for (int x = 0; x < windows.Length-1; x++) {
                        if (windows[x] != null && windows[x].GetID == ID) {
                            if (reloadSkin) {
                                windows[x].DoRestoreSkin();
                            }
                            return windows[x];
                        }
                    }
                }
            }
            #else
            MethodInfo method = GetMethodInfo<GUIWindowManager>("GetWindow", "GetWindow", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null) {
                return method.Invoke(null, new object[] { ID, reloadSkin }) as GUIWindow;
            }
            #endif
            return null;
        }

        public static GUIFacadeControl FacadeControl(this GUIVideoTitle self) {
            #if MP11
            FieldInfo field = GetFieldInfo<GUIVideoBaseWindow>("facadeView", "facadeView", BindingFlags.Instance | BindingFlags.NonPublic);
            #else
            FieldInfo field = GetFieldInfo<WindowPlugins.WindowPluginBase>("facadeLayout", "facadeLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            #endif
            if (field != null) {
                return field.GetValue(self) as GUIFacadeControl;
            }
            return null;
        }

        private static Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();

        /// <summary>
        /// Gets the property info object for a property using reflection.
        /// The property info object will be cached in memory for later requests.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newName">The name of the property in 1.2</param>
        /// <param name="oldName">The name of the property in 1.1</param>
        /// <returns>instance PropertyInfo or null if not found</returns>
        public static PropertyInfo GetPropertyInfo<T>(string newName, string oldName) {
            PropertyInfo property = null;
            Type type = typeof(T);
            string key = type.FullName + "|" + newName;

            if (!propertyCache.TryGetValue(key, out property)) {
                property = type.GetProperty(newName);
                if (property == null) {
                    property = type.GetProperty(oldName);
                }

                propertyCache[key] = property;
            }

            return property;
        }

        private static Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

        public static MethodInfo GetMethodInfo<T>(string newName, string oldName, BindingFlags bindingAttr) {
            MethodInfo method = null;
            Type type = typeof(T);
            string key = type.FullName + "|" + newName + "|" + bindingAttr;

            if (!methodCache.TryGetValue(key, out method)) {
                method = type.GetMethod(newName, bindingAttr);
                if (method == null) {
                    method = type.GetMethod(oldName, bindingAttr);
                }

                methodCache[key] = method;
            }

            return method;
        }

        private static Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();

        public static FieldInfo GetFieldInfo<T>(string newName, string oldName, BindingFlags bindingAttr) {
            FieldInfo field = null;
            Type type = typeof(T);
            string key = type.FullName + "|" + newName + "|" + bindingAttr;

            if (!fieldCache.TryGetValue(key, out field)) {
                field = type.GetField(newName, bindingAttr);
                if (field == null) {
                    field = type.GetField(oldName, bindingAttr);
                }

                fieldCache[key] = field;
            }

            return field;
        }
    }
}
