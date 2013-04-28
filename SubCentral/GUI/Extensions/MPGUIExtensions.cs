using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Video.Database;
using NLog;

namespace SubCentral.GUI.Extensions {
    public static class MPGUIExtensions {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static IEnumerable GetControlList(this GUIWindow self) {
            PropertyInfo property = GetPropertyInfo<GUIWindow>("Children", BindingFlags.Instance | BindingFlags.Public);
            if (property != null) {
                return property.GetValue(self, null) as IEnumerable;
            }

            return null;
        }

        public static GUIListItem GetSelectedItem(this GUIVideoTitle self) {
            FieldInfo field;
            field = GetFieldInfo<GUIVideoTitle>("currentSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic);
            int? currentSelectedItem = null;
            if (field != null) {
                currentSelectedItem = field.GetValue(self) as int?;
            }
            field = GetFieldInfo<GUIVideoBaseWindow>("handler", BindingFlags.Instance | BindingFlags.NonPublic);
            VideoViewHandler handler = null;
            if (field != null) {
                handler = field.GetValue(self) as VideoViewHandler;
            }
            PropertyInfo property;
            VideoSort.SortMethod? currentSortMethod = null;
            bool? currentSortAsc = null;
            property = GetPropertyInfo<GUIVideoTitle>("CurrentSortMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null) {
                currentSortMethod = property.GetValue(self, null) as VideoSort.SortMethod?;
            }
            property = GetPropertyInfo<GUIVideoTitle>("CurrentSortAsc", BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null) {
                currentSortAsc = property.GetValue(self, null) as bool?;
            }

            if (handler != null && currentSelectedItem != null && currentSelectedItem >= 0 && currentSortMethod != null && currentSortAsc != null) {
                MethodInfo method;
                ArrayList itemList = new ArrayList();
                List<GUIListItem> GUIItemList = new List<GUIListItem>();
                ArrayList movies = ((VideoViewHandler)handler).Execute();
                if (handler.CurrentLevel > 0) {
                    GUIListItem listItem = new GUIListItem("..");
                    listItem.Path = string.Empty;
                    listItem.IsFolder = true;
                    itemList.Add(listItem);
                }
                foreach (IMDBMovie movie in movies) {
                    GUIListItem item = new GUIListItem();
                    item.Label = movie.Title;
                    if (handler.CurrentLevel + 1 < handler.MaxLevels) {
                        item.IsFolder = true;
                    }
                    else {
                        item.IsFolder = false;
                    }
                    item.Path = movie.File;
                    item.Duration = movie.RunTime * 60;
                    item.AlbumInfoTag = movie;
                    item.Year = movie.Year;
                    item.DVDLabel = movie.DVDLabel;
                    item.Rating = movie.Rating;
                    item.IsPlayed = movie.Watched > 0 ? true : false;

                    method = GetMethodInfo<GUIVideoTitle>("CheckItem", BindingFlags.NonPublic | BindingFlags.Instance);
                    bool accept = true;
                    if (method != null && !string.IsNullOrEmpty(item.Path)) {
                        accept = (bool)method.Invoke(self, new object[] { item });
                    }

                    if (accept) {
                        itemList.Add(item);
                    }
                }
                method = GetMethodInfo<GUIVideoTitle>("SetIMDBThumbs", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) {
                    method.Invoke(self, new object[] { itemList });
                }

                foreach (GUIListItem item in itemList) {
                    GUIItemList.Add(item);
                }
                
                VideoSort sorter = new VideoSort((VideoSort.SortMethod)currentSortMethod, (bool)currentSortAsc);
                GUIItemList.Sort(sorter);

                if (GUIItemList.Count > 0 && (int)currentSelectedItem < GUIItemList.Count) {
                  return GUIItemList[(int)currentSelectedItem];
                }
            }
            return null;
        }

        private static Dictionary<string, PropertyInfo> propertyCache = new Dictionary<string, PropertyInfo>();

        public static PropertyInfo GetPropertyInfo<T>(string newName, BindingFlags bindingAttr) {
            PropertyInfo property = null;
            Type type = typeof(T);
            string key = type.FullName + "|" + newName + "|" + bindingAttr;

            if (!propertyCache.TryGetValue(key, out property)) {
                property = type.GetProperty(newName, bindingAttr);
                propertyCache[key] = property;
            }

            return property;
        }

        private static Dictionary<string, MethodInfo> methodCache = new Dictionary<string, MethodInfo>();

        public static MethodInfo GetMethodInfo<T>(string name, BindingFlags bindingAttr) {
            
            MethodInfo method = null;
            
            Type type = typeof(T);

            string key = type.FullName + "|" + name + "|" + bindingAttr;

            if (!methodCache.TryGetValue(key, out method)) {
                method = type.GetMethod(name, bindingAttr);
                methodCache[key] = method;
            }

            return method;
        }

        private static Dictionary<string, FieldInfo> fieldCache = new Dictionary<string, FieldInfo>();

        public static FieldInfo GetFieldInfo<T>(string newName, BindingFlags bindingAttr) {
            FieldInfo field = null;
            Type type = typeof(T);
            string key = type.FullName + "|" + newName + "|" + bindingAttr;

            if (!fieldCache.TryGetValue(key, out field)) {
                field = type.GetField(newName, bindingAttr);
                fieldCache[key] = field;
            }

            return field;
        }
    }
}
