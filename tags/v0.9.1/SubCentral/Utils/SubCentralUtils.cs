using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using NLog;
using SubCentral.GUI;
using SubCentral.GUI.Items;
using SubCentral.Localizations;
using SubCentral.PluginHandlers;
using SubCentral.Settings;
using SubCentral.Settings.Data;
using System.Linq;

namespace SubCentral.Utils {
    public static class SubCentralUtils {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private delegate List<MultiSelectionItem> ShowMultiSelectionDialogDelegate(string heading, List<MultiSelectionItem> items);

        public static readonly string SettingsFileName = "SubCentral.xml";
        public static string LogFileName = "SubCentral.log";
        public static string OldLogFileName = "SubCentral.log.bak";
        private static readonly object syncRoot = new object();

        public static List<string> SubtitleExtensions = new List<string> { ".aqt", ".asc", ".ass", ".dat", ".dks", ".js", ".jss", 
                                                                           ".lrc", ".mpl", ".ovr", ".pan", ".pjs", ".psb", ".rt", 
                                                                           ".rtf", ".s2k", ".sbt", ".scr", ".smi", ".son", ".srt", 
                                                                           ".ssa", ".sst", ".ssts", ".stl", ".sub", ".txt", ".vkt", 
                                                                           ".vsf", ".zeg" };
        public static List<string> AdditionalExtensions = new List<string> { ".rar", ".zip" };

        private static List<string> extensions = new List<string>();
        public static List<string> Extensions {
            get {
                if (extensions == null || extensions.Count < 1) {
                    if (extensions == null)
                        extensions = new List<string>();
                    extensions.AddRange(SubtitleExtensions);
                    extensions.AddRange(AdditionalExtensions);
                }
                return extensions;
            }
        }
        public static string PluginName() {
            if (SettingsManager.Properties == null || SettingsManager.Properties.GUISettings == null) return "SubCentral";
            return string.IsNullOrEmpty(SettingsManager.Properties.GUISettings.PluginName) ? "SubCentral" : SettingsManager.Properties.GUISettings.PluginName;
        }

        public static Dictionary<string, string> SubsLanguages {
            get {
                return _subsLanguages;
            }
            set {
                _subsLanguages = value;
            }
        }
        private static Dictionary<string, string> _subsLanguages = null;

        public static List<string> SubsDownloaderNames {
            get {
                return _subsDownloaderNames;
            }
            set {
                _subsDownloaderNames = value;
            }
        }
        private static List<string> _subsDownloaderNames = null;

        public static List<SettingsFolder> AllFolders {
            get {
                if (_allFolders == null)
                    _allFolders = getAllFolders();
                return _allFolders;
            }
        }
        private static List<SettingsFolder> _allFolders = null;

        public static List<SettingsGroup> getAllProviderGroups() {
            List<SettingsGroup> result = new List<SettingsGroup>();

            // default groups
            SettingsGroup newSettingsGroup = null;

            newSettingsGroup = new SettingsGroup() {
                Title = Localization.AllProviders,
                Providers = getAllProvidersAsEnabledOrDisabled(true),
                Enabled = Settings.SettingsManager.Properties.GeneralSettings.AllProvidersEnabled,
                DefaultForMovies = Settings.SettingsManager.Properties.GeneralSettings.AllProvidersForMovies,
                DefaultForTVShows = Settings.SettingsManager.Properties.GeneralSettings.AllProvidersForTVShows
            };
            result.Add(newSettingsGroup);

            newSettingsGroup = new SettingsGroup() {
                Title = Localization.AllEnabledProviders,
                Providers = getAllProviders(),
                Enabled = Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersEnabled,
                DefaultForMovies = Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersForMovies,
                DefaultForTVShows = Settings.SettingsManager.Properties.GeneralSettings.EnabledProvidersForTVShows
            };
            result.Add(newSettingsGroup);

            result.AddRange(Settings.SettingsManager.Properties.GeneralSettings.Groups);

            if (!groupsHaveDefaultForMovies(result)) {
                result[0].DefaultForMovies = true;
            }

            if (!groupsHaveDefaultForTVShows(result)) {
                result[0].DefaultForTVShows = true;
            }

            return result;
        }

        public static List<SettingsGroup> getEnabledProviderGroups() {
            List<SettingsGroup> result = getAllProviderGroups();
            List<SettingsGroup> toRemove = new List<SettingsGroup>();

            foreach (SettingsGroup settingsGroup in result) {
                if (!settingsGroup.Enabled)
                    toRemove.Add(settingsGroup);
            }

            foreach (SettingsGroup settingsGroup in toRemove) {
                result.Remove(settingsGroup);
            }

            return result;
        }

        public static List<SettingsProvider> getEnabledProvidersFromGroup(SettingsGroup settingsGroup) {
            List<SettingsProvider> result = new List<SettingsProvider>();
            List<SettingsProvider> toRemove = new List<SettingsProvider>();

            if (settingsGroup == null || settingsGroup.Providers == null || settingsGroup.Providers.Count == 0) return result;

            foreach (SettingsProvider settingsProvider in settingsGroup.Providers) {
                if (settingsProvider.Enabled)
                    result.Add(settingsProvider);
            }

            foreach (SettingsProvider settingsProvider in result) {
                if (!SubsDownloaderNames.Contains(settingsProvider.ID))
                    toRemove.Add(settingsProvider);
            }
            foreach (SettingsProvider settingsProvider in toRemove) {
                result.Remove(settingsProvider);
            }

            return result;
        }

        public static SettingsGroup getDefaultGroupForSearchType(SubtitlesSearchType searchType) {
            SettingsGroup result = null;

            List<SettingsGroup> allProviderGroups = getAllProviderGroups();

            if (allProviderGroups == null || allProviderGroups.Count == 0) return result;

            foreach (SettingsGroup settingsGroup in allProviderGroups) {
                switch (searchType) {
                    case SubtitlesSearchType.IMDb:
                    case SubtitlesSearchType.MOVIE:
                        if (settingsGroup.Enabled && settingsGroup.DefaultForMovies && groupHasEnabledProviders(settingsGroup))
                            return settingsGroup;
                        break;
                    case SubtitlesSearchType.TVSHOW:
                        if (settingsGroup.Enabled && settingsGroup.DefaultForTVShows && groupHasEnabledProviders(settingsGroup))
                            return settingsGroup;
                        break;
                }
            }
            return result;
        }

        private static bool groupsHaveDefaultForMovies(List<SettingsGroup> settingsGroups) {
            if (settingsGroups == null || settingsGroups.Count == 0) return false;

            foreach (SettingsGroup settingsGroup in settingsGroups) {
                if (settingsGroup.DefaultForMovies) {
                    return true;
                }
            }

            return false;
        }

        private static bool groupsHaveDefaultForTVShows(List<SettingsGroup> settingsGroups) {
            if (settingsGroups == null || settingsGroups.Count == 0) return false;

            foreach (SettingsGroup settingsGroup in settingsGroups) {
                if (settingsGroup.DefaultForTVShows) {
                    return true;
                }
            }

            return false;
        }

        public static bool groupHasEnabledProviders(SettingsGroup settingsGroup) {
            return getEnabledProvidersFromGroup(settingsGroup).Count > 0;
        }

        public static List<SettingsProvider> getAllProviders() {
            List<SettingsProvider> result = new List<SettingsProvider>();
            List<SettingsProvider> toRemove = new List<SettingsProvider>();

            result.AddRange(Settings.SettingsManager.Properties.GeneralSettings.Providers);

            foreach (SettingsProvider settingsProvider in result) {
                if (!SubsDownloaderNames.Contains(settingsProvider.ID))
                    toRemove.Add(settingsProvider);
            }

            foreach (SettingsProvider settingsProvider in toRemove) {
                result.Remove(settingsProvider);
            }

            foreach (string provider in SubsDownloaderNames) {
                bool found = false;
                foreach (SettingsProvider settingsProvider in result) {
                    if (settingsProvider.ID == provider)
                        found = true;
                }
                if (!found) {
                    SettingsProvider newSettingsProvider = new SettingsProvider() {
                        ID = provider,
                        Title = provider,
                        Enabled = true // enabled by default
                    };

                    result.Add(newSettingsProvider);
                }
            }

            Settings.SettingsManager.Properties.GeneralSettings.Providers.Clear();
            Settings.SettingsManager.Properties.GeneralSettings.Providers.AddRange(result);

            return result;
        }

        public static List<SettingsProvider> getAllProvidersAsEnabledOrDisabled(bool enabled) {
            List<SettingsProvider> result = new List<SettingsProvider>();

            foreach (SettingsProvider settingsProvider in getAllProviders()) {
                SettingsProvider newSettingsProvider = new SettingsProvider() {
                    ID = settingsProvider.ID,
                    Title = settingsProvider.Title,
                    Enabled = enabled // enabled by default
                };
                result.Add(newSettingsProvider);
            }

            return result;
        }

        public static List<SettingsProvider> getEnabledProviders() {
            List<SettingsProvider> result = getAllProviders();
            List<SettingsProvider> toRemove = new List<SettingsProvider>();

            foreach (SettingsProvider settingsProvider in result) {
                if (!settingsProvider.Enabled)
                    toRemove.Add(settingsProvider);
            }

            foreach (SettingsProvider settingsProvider in toRemove) {
                result.Remove(settingsProvider);
            }

            return result;
        }

        public static List<string> getProviderIDs(List<SettingsProvider> settingsProviders) {
            List<string> result = new List<string>();

            if (settingsProviders == null || settingsProviders.Count == 0) return result;

            foreach (SettingsProvider settingsProvider in settingsProviders) {
                result.Add(settingsProvider.ID);
            }

            return result;
        }

        public static Dictionary<string, string> getProviderIDsAndTitles(List<SettingsProvider> settingsProviders) {
            Dictionary<string, string> result = new Dictionary<string, string>();

            if (settingsProviders == null || settingsProviders.Count == 0) return result;

            foreach (SettingsProvider settingsProvider in settingsProviders) {
                result.Add(settingsProvider.ID, settingsProvider.Title);
            }

            return result;
        }

        public static List<SettingsLanguage> getAllLanguages() {
            List<SettingsLanguage> result = new List<SettingsLanguage>();
            List<SettingsLanguage> toRemove = new List<SettingsLanguage>();

            result.AddRange(Settings.SettingsManager.Properties.LanguageSettings.Languages);

            foreach (SettingsLanguage settingsLanguage in result) {
                if (!SubsLanguages.ContainsKey(settingsLanguage.LanguageCode))
                    toRemove.Add(settingsLanguage);
            }

            foreach (SettingsLanguage settingsLanguage in toRemove) {
                result.Remove(settingsLanguage);
            }

            foreach (KeyValuePair<string, string> kvp in SubsLanguages) {
                bool found = false;
                foreach (SettingsLanguage settingsLanguage in result) {
                    if (settingsLanguage.LanguageCode == kvp.Key)
                        found = true;
                }
                if (!found) {
                    SettingsLanguage newSettingsLanguage = new SettingsLanguage() {
                        LanguageCode = kvp.Key,
                        LanguageName = kvp.Value,
                        Enabled = false // enabled by default
                    };

                    result.Add(newSettingsLanguage);
                }
            }

            if (!hasEnabledLanguage(result)) {
                enableDefaultLanguage(result);
            }

            Settings.SettingsManager.Properties.LanguageSettings.Languages.Clear();
            Settings.SettingsManager.Properties.LanguageSettings.Languages.AddRange(result);

            return result;
        }

        public static bool hasEnabledLanguage(List<SettingsLanguage> languages) {
            if (languages == null || languages.Count == 0) return false;

            foreach (SettingsLanguage settingsLanguage in languages) {
                if (settingsLanguage.Enabled)
                    return true;
            }
            return false;
        }

        public static bool enableDefaultLanguage(List<SettingsLanguage> languages) {
            if (languages == null || languages.Count == 0) return false;

            foreach (SettingsLanguage settingsLanguage in languages) {
                if (settingsLanguage.LanguageName == getUILanguage()) {
                    settingsLanguage.Enabled = true;
                    return true;
                }
            }
            return false;
        }

        public static string getUILanguage() {
            string result = string.Empty;

            try {
                result = GUILocalizeStrings.CurrentLanguage();
            }
            catch {
                try {
                    result = CultureInfo.CurrentUICulture.Name.Substring(0, 2);
                    CultureInfo ci = CultureInfo.GetCultureInfo(result);
                    result = ci.EnglishName;
                }
                catch {
                    result = string.Empty;
                }
            }

            if (!SubsLanguages.ContainsValue(result) || string.IsNullOrEmpty(result))
                return "English";

            return result;
        }

        public static List<string> getSelectedLanguageNames() {
            List<string> result = new List<string>();
            List<SettingsLanguage> allLanguages = getAllLanguages();

            foreach (SettingsLanguage settingsLanguage in allLanguages) {
                if (settingsLanguage.Enabled)
                    result.Add(settingsLanguage.LanguageName);
            }

            return result;
        }

        public static List<string> getSelectedLanguageCodes() {
            List<string> result = new List<string>();
            List<SettingsLanguage> allLanguages = getAllLanguages();

            foreach (SettingsLanguage settingsLanguage in allLanguages) {
                if (settingsLanguage.Enabled)
                    result.Add(settingsLanguage.LanguageCode);
            }

            return result;
        }

        public static int getLanguagePriorityByCode(string languageCode) {
            int result = int.MaxValue;

            List<SettingsLanguage> allLanguages = SubCentralUtils.getAllLanguages();

            if (allLanguages == null || allLanguages.Count == 0) return result;

            for (int i = 0; i < allLanguages.Count; i++) {
                SettingsLanguage settingsLanguage = allLanguages[i];
                if (settingsLanguage.LanguageCode.Equals(languageCode))
                    return i + 1;
            }
            return result;
        }

        public static List<MultiSelectionItem> getLanguageNamesForMultiSelection() {
            List<MultiSelectionItem> result = new List<MultiSelectionItem>();
            List<SettingsLanguage> allLanguages = getAllLanguages();

            foreach (SettingsLanguage settingsLanguage in allLanguages) {
                MultiSelectionItem multiSelectionItem = new MultiSelectionItem();
                multiSelectionItem.ItemID = settingsLanguage.LanguageCode;
                multiSelectionItem.ItemTitle = settingsLanguage.LanguageName;
                multiSelectionItem.Selected = settingsLanguage.Enabled;

                result.Add(multiSelectionItem);
            }

            return result;
        }

        public static void setLanguageNamesFromMultiSelection(List<MultiSelectionItem> selectedLanguages) {
            if (selectedLanguages == null || selectedLanguages.Count == 0) return;

            List<SettingsLanguage> allLanguages = getAllLanguages();

            foreach (MultiSelectionItem multiSelectionItem in selectedLanguages) {
                foreach (SettingsLanguage settingsLanguage in Settings.SettingsManager.Properties.LanguageSettings.Languages) {
                    if (settingsLanguage.LanguageCode == multiSelectionItem.ItemID) {
                        if (multiSelectionItem.Selected)
                            settingsLanguage.Enabled = true;
                        else
                            settingsLanguage.Enabled = false;
                    }
                }
            }
        }

        public static int getSelectedLanguagesCountFromMultiSelection(List<MultiSelectionItem> selectedLanguages) {
            if (selectedLanguages == null) return 0;

            int result = 0;

            foreach (MultiSelectionItem multiSelectionItem in selectedLanguages) {
                if (multiSelectionItem.Selected) {
                    result++;
                }
            }

            return result;
        }

        private static List<SettingsFolder> getAllFolders() {
            List<SettingsFolder> result = new List<SettingsFolder>();
            List<SettingsFolder> toRemove = new List<SettingsFolder>();
            List<string> subtitlesPathsMP = new List<string>();
            int index;

            MediaPortal.Profile.Settings mpSettings = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
            string subtitlesPathsSetting = mpSettings.GetValueAsString("subtitles", "paths", @".\");
            foreach (string subtitlesPath in subtitlesPathsSetting.Split(new string[] { "," }, StringSplitOptions.None)) {
                string subtitlesPathToAdd = subtitlesPath.Trim();
                if (FileUtils.pathNameIsValid(subtitlesPathToAdd))
                    subtitlesPathsMP.Add(FileUtils.ensureBackSlash(subtitlesPathToAdd));
            }

            foreach (string subtitlesPath in subtitlesPathsMP) {
                if (containsPath(Settings.SettingsManager.Properties.FolderSettings.Folders, subtitlesPath, out index)) {
                    SettingsFolder settingsFolder = Settings.SettingsManager.Properties.FolderSettings.Folders[index];
                    SettingsFolder newSettingsFolder = new SettingsFolder() {
                        Folder = settingsFolder.Folder,
                        Enabled = settingsFolder.Enabled,
                        //Existing = pathExists(settingsFolder.Folder),
                        //Writable = pathIsWritable(settingsFolder.Folder),
                        DefaultForMovies = settingsFolder.DefaultForMovies,
                        DefaultForTVShows = settingsFolder.DefaultForTVShows
                    };

                    result.Add(newSettingsFolder);
                }
                else {
                    SettingsFolder newSettingsFolder = new SettingsFolder() {
                        Folder = subtitlesPath,
                        Enabled = true,
                        //Existing = pathExists(subtitlesPath),
                        //Writable = pathIsWritable(subtitlesPath),
                        DefaultForMovies = false,
                        DefaultForTVShows = false
                    };

                    result.Add(newSettingsFolder);
                }
            }

            // ensure path .\ if empty - default
            if (result.Count == 0) {
                SettingsFolder newSettingsFolder = new SettingsFolder() {
                    Folder = @".\",
                    Enabled = true,
                    DefaultForMovies = true,
                    DefaultForTVShows = true
                };

                result.Insert(0, newSettingsFolder);
            }

            Settings.SettingsManager.Properties.FolderSettings.Folders.Clear();
            Settings.SettingsManager.Properties.FolderSettings.Folders.AddRange(result);

            return result;
        }

        public static List<FolderSelectionItem> getEnabledAndValidFoldersForMedia(FileInfo fileInfo, bool includeReadOnly) {
            return getEnabledAndValidFoldersForMedia(fileInfo, includeReadOnly, false);
        }

        public static List<FolderSelectionItem> getEnabledAndValidFoldersForMedia(FileInfo fileInfo, bool includeReadOnly, bool skipErrorInfo) {
            List<FolderSelectionItem> result = new List<FolderSelectionItem>();
            List<SettingsFolder> allFolders = AllFolders;
            List<SettingsFolder> toRemove = new List<SettingsFolder>();

            // remove not enabled and if fileinfo is null all relative paths
            foreach (SettingsFolder settingsFolder in allFolders) {
                if (!settingsFolder.Enabled) {
                    toRemove.Add(settingsFolder);
                }
                else {
                    if (fileInfo == null && !Path.IsPathRooted(settingsFolder.Folder)) {
                        toRemove.Add(settingsFolder);
                    }
                }
            }

            foreach (SettingsFolder settingsFolder in allFolders) {
                if (toRemove.Contains(settingsFolder)) continue;
                string folder = settingsFolder.Folder;
                if (fileInfo != null && !Path.IsPathRooted(settingsFolder.Folder)) {
                    folder = FileUtils.ResolveRelativePath(settingsFolder.Folder, Path.GetDirectoryName(fileInfo.FullName));
                }
                if (folder != null) {
                    FolderSelectionItem newFolderSelectionItem = new FolderSelectionItem() {
                        FolderName = folder,
                        //FolderErrorInfo = getFolderErrorInfo(folder),
                        OriginalFolderName = settingsFolder.Folder,
                        WasRelative = !Path.IsPathRooted(settingsFolder.Folder),
                        DefaultForMovies = settingsFolder.DefaultForMovies,
                        DefaultForTVShows = settingsFolder.DefaultForTVShows
                    };
                    if (skipErrorInfo)
                        newFolderSelectionItem.FolderErrorInfo = FolderErrorInfo.OK;
                    else
                        newFolderSelectionItem.FolderErrorInfo = getFolderErrorInfo(folder);

                    if (!includeReadOnly) {
                        if (newFolderSelectionItem.FolderErrorInfo == FolderErrorInfo.ReadOnly)
                            continue;
                    }

                    result.Add(newFolderSelectionItem);
                }
            }

            return result;
        }

        public static List<string> getEnabledAndValidFolderNamesForMedia(FileInfo fileInfo, bool includeReadOnly, bool skipErrorInfo) {
            List<FolderSelectionItem> result = getEnabledAndValidFoldersForMedia(fileInfo, includeReadOnly, skipErrorInfo);

            return result.Select(r => r.FolderName).ToList();
        }

        public static FolderErrorInfo getFolderErrorInfo(string path) {
            FolderErrorInfo result = FolderErrorInfo.OK;

            bool hostAlive;
            bool pathDriveReady;

            if (!FileUtils.pathExists(path, out hostAlive, out pathDriveReady))
                result = FolderErrorInfo.NonExistant;
            else if (!FileUtils.pathIsWritable(path))
                result = FolderErrorInfo.ReadOnly;

            int iUncPathDepth = FileUtils.uncPathDepth(path);
            if (result == FolderErrorInfo.NonExistant &&
                (FileUtils.pathDriveIsDVD(path) || !pathDriveReady /*!pathDriveIsReady(path)*/|| !hostAlive
                /*!uncHostIsAlive(path)*/|| (iUncPathDepth > 0 && iUncPathDepth < 3)))
                result = FolderErrorInfo.ReadOnly;

            return result;
        }

        private static bool foldersHaveDefaultForMovies(List<SettingsFolder> settingsFolders) {
            if (settingsFolders == null || settingsFolders.Count == 0) return false;

            foreach (SettingsFolder settingsFolder in settingsFolders) {
                if (settingsFolder.DefaultForMovies) {
                    return true;
                }
            }

            return false;
        }

        private static bool foldersHaveDefaultForTVShows(List<SettingsFolder> settingsFolders) {
            if (settingsFolders == null || settingsFolders.Count == 0) return false;

            foreach (SettingsFolder settingsFolder in settingsFolders) {
                if (settingsFolder.DefaultForTVShows) {
                    return true;
                }
            }

            return false;
        }

        public static bool containsPath(List<SettingsFolder> settingsFolders, string path, out int index) {
            index = -1;

            if (settingsFolders == null || settingsFolders.Count == 0) return false;

            int tempIndex = 0;
            foreach (SettingsFolder settingsFolder in settingsFolders) {
                if (settingsFolder.Folder.Equals(path)) {
                    index = tempIndex;
                    return true;
                }
                tempIndex++;
            }

            return false;
        }

        public static SubtitlesSearchType getSubtitlesSearchTypeFromMediaDetail(BasicMediaDetail basicMediaDetail) {
            SubtitlesSearchType result = SubtitlesSearchType.NONE;

            bool useImdbMovieQuery = !(string.IsNullOrEmpty((basicMediaDetail.ImdbID)));
            bool useTitle = !(string.IsNullOrEmpty((basicMediaDetail.Title)));
            bool useMovieQuery = useTitle && !(string.IsNullOrEmpty((basicMediaDetail.YearStr)));
            bool useEpisodeQuery = useTitle && !(string.IsNullOrEmpty((basicMediaDetail.SeasonStr))) && !(string.IsNullOrEmpty((basicMediaDetail.EpisodeStr)));

            if (useEpisodeQuery) {
                result = SubtitlesSearchType.TVSHOW;
            }
            if (useImdbMovieQuery) {
                result = SubtitlesSearchType.IMDb;
            }
            else if (useMovieQuery) {
                result = SubtitlesSearchType.MOVIE;
            }

            return result;
        }

        public static bool canSearchMediaDetailWithType(BasicMediaDetail basicMediaDetail, SubtitlesSearchType subtitlesSearchType) {
            bool useImdbMovieQuery = !(string.IsNullOrEmpty((basicMediaDetail.ImdbID)));
            bool useTitle = !(string.IsNullOrEmpty((basicMediaDetail.Title)));
            bool useMovieQuery = useTitle && !(string.IsNullOrEmpty((basicMediaDetail.YearStr)));
            bool useEpisodeQuery = useTitle && !(string.IsNullOrEmpty((basicMediaDetail.SeasonStr))) && !(string.IsNullOrEmpty((basicMediaDetail.EpisodeStr)));

            switch (subtitlesSearchType) {
                case SubtitlesSearchType.IMDb:
                    return useImdbMovieQuery;
                case SubtitlesSearchType.TVSHOW:
                    return useEpisodeQuery;
                case SubtitlesSearchType.MOVIE:
                    return useMovieQuery;
                default:
                    return false;
            }
        }

        public static bool isImdbIdCorrect(string imdbId) {
            if (string.IsNullOrEmpty(imdbId)) return false;

            return imdbId.Length == 9 && Regex.Match(imdbId, @"tt\d{7}").Success;
        }

        public static bool isYearCorrect(string year) {
            if (string.IsNullOrEmpty(year)) return false;

            //bool result = year.Length == 4 && Regex.Match(year, @"\d{4}").Success;

            //if (result) {
            int intYear = -1;
            if (int.TryParse(year, out intYear)) {
                if (intYear < 1900 || intYear > yearToRange()) {
                    return false;
                }
            }
            else {
                return false;
            }

            return true;
        }

        public static int yearToRange() {
            //return System.DateTime.Now.Year + 100;
            // TODO MS
            return 2050;
        }

        public static bool isSeasonOrEpisodeCorrect(string seasonOrEpisode) {
            if (string.IsNullOrEmpty(seasonOrEpisode)) return false;

            //bool result = seasonOrEpisode.Length > 0 && seasonOrEpisode.Length < 4 && Regex.Match(seasonOrEpisode, @"\d{1,3}").Success;

            //if (result) {
            int intSeasonOrEpisode = -1;
            if (int.TryParse(seasonOrEpisode, out intSeasonOrEpisode)) {
                if (intSeasonOrEpisode < 1 || intSeasonOrEpisode > 999) {
                    return false;
                }
            }
            else {
                return false;
            }

            return true;
        }

        public static bool IsAssemblyAvailable(string name, Version ver) {
            return IsAssemblyAvailable(name, ver, false);
        }

        public static bool IsAssemblyAvailable(string name, Version ver, bool reflectionOnly) {
            Assembly[] assemblies = null;

            if (!reflectionOnly) 
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            else
                assemblies = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

            if (assemblies != null) {
                foreach (Assembly a in assemblies) {
                    try {
                        if (a.GetName().Name == name && a.GetName().Version >= ver)
                            return true;
                    }
                    catch (Exception e) {
                        logger.ErrorException(string.Format("Assembly.GetName() call failed for '{0}'!\n", a.Location), e);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks the media files for subtitles
        /// </summary>
        /// <param name="fiFiles">list of files to examine</param>
        /// <returns>true  : ANY of the input files have subtitles
        ///          false : some or all of the input files don't have subtitles</returns>
        public static bool MediaHasSubtitles(List<FileInfo> fiFiles) {
            return MediaHasSubtitles(fiFiles, true, -1, false);
        }

        /// <summary>
        /// Checks the media files for subtitles
        /// </summary>
        /// <param name="fiFiles">list of files to examine</param>
        /// <param name="useMediaInfo">use media info?</param>
        /// <returns>true  : ANY of the input files have subtitles
        ///          false : some or all of the input files don't have subtitles</returns>
        public static bool MediaHasSubtitles(List<FileInfo> fiFiles, bool useMediaInfo) {
            return MediaHasSubtitles(fiFiles, useMediaInfo, -1, false);
        }

        /// <summary>
        /// Checks the media files for subtitles
        /// </summary>
        /// <param name="fiFiles">list of files to examine</param>
        /// <param name="useMediaInfo">use media info?</param>
        /// <param name="cachedMISubtitleCount">cached media info text count, -1 for default, if set mediainfo won't be used</param>
        /// <returns>true  : ANY of the input files have subtitles
        ///          false : some or all of the input files don't have subtitles</returns>
        public static bool MediaHasSubtitles(List<FileInfo> fiFiles, bool useMediaInfo, int cachedMISubtitleCount) {
            return MediaHasSubtitles(fiFiles, useMediaInfo, cachedMISubtitleCount, false);
        }

        /// <summary>
        /// Checks the media files for subtitles
        /// </summary>
        /// <param name="fiFiles">list of files to examine</param>
        /// <param name="useMediaInfo">use media info?</param>
        /// <param name="cachedMISubtitleCount">cached media info text count, -1 for default, if set mediainfo won't be used</param>
        /// <param name="useLocalOnly">only media folder will be looked for subtitles, if set mediainfo won't be used</param>
        /// <returns>true  : ANY of the input files have subtitles
        ///          false : some or all of the input files don't have subtitles</returns>
        public static bool MediaHasSubtitles(List<FileInfo> fiFiles, bool useMediaInfo, int cachedMISubtitleCount, bool useLocalOnly) {
            List<FileInfo> subtitleFiles = null;
            return MediaHasSubtitles(fiFiles, useMediaInfo, cachedMISubtitleCount, useLocalOnly, ref subtitleFiles);
        }

        public static bool MediaHasSubtitles(List<FileInfo> fiFiles, bool useMediaInfo, int cachedMISubtitleCount, bool useLocalOnly, ref List<FileInfo> subtitleFiles) {
            bool result = false;

            if (fiFiles == null || fiFiles.Count == 0) return result;

            foreach (FileInfo fiFile in fiFiles) {
                MediaInfoWrapper miWrapper = new MediaInfoWrapper(fiFile.FullName, useMediaInfo, cachedMISubtitleCount, useLocalOnly, subtitleFiles != null);
                result = result || miWrapper.HasSubtitles;
                if (result && subtitleFiles != null)
                    subtitleFiles.AddRange(miWrapper.SubtitleFiles);
            }


            return result;
        }

        /// <summary>
        /// Displays a menu dialog from FolderSelectionItem items
        /// </summary>
        /// <returns>Selected item index, -1 if exited</returns>
        public static int ShowFolderMenuDialog(string heading, List<FolderSelectionItem> items, int selectedItemIndex) {
            List<GUIListItem> listItems = new List<GUIListItem>();

            bool selectedItemSet = false;
            int index = 0;
            foreach (FolderSelectionItem folderSelectionItem in items) {
                GUIListItem listItem = new GUIListItem();
                listItem.Label = folderSelectionItem.FolderName;
                if (folderSelectionItem.WasRelative) {
                    listItem.Label2 = "(" + folderSelectionItem.OriginalFolderName + ")";
                }

                switch (folderSelectionItem.FolderErrorInfo) {
                    case FolderErrorInfo.NonExistant:
                        listItem.IsRemote = true;
                        listItem.IsDownloading = true;
                        break;
                    case FolderErrorInfo.ReadOnly:
                        listItem.IsRemote = true;
                        break;
                    case FolderErrorInfo.OK:
                        if (!selectedItemSet && selectedItemIndex < 0) {
                            selectedItemIndex = index;
                            selectedItemSet = true;
                        }
                        break;
                }

                listItem.MusicTag = folderSelectionItem;

                if (!selectedItemSet && selectedItemIndex >= 0 && index == selectedItemIndex) {
                    selectedItemIndex = index;
                    selectedItemSet = true;

                }

                listItems.Add(listItem);
                index++;
            }

            return GUIUtils.ShowMenuDialog(heading, listItems, selectedItemIndex);
        }

        /// <summary>
        /// Displays a multi selection dialog.
        /// </summary>
        /// <returns>List of items</returns>
        public static List<MultiSelectionItem> ShowMultiSelectionDialog(string heading, List<MultiSelectionItem> items) {
            List<MultiSelectionItem> result = new List<MultiSelectionItem>();

            if (items == null) return result;

            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowMultiSelectionDialogDelegate d = ShowMultiSelectionDialog;
                return (List<MultiSelectionItem>)GUIGraphicsContext.form.Invoke(d, heading, items);
            }

            GUIWindow dlgMultiSelectOld = (GUIWindow)GUIWindowManager.GetWindow(2100);
            GUIDialogMultiSelect dlgMultiSelect = new GUIDialogMultiSelect();
            dlgMultiSelect.Init();
            GUIWindowManager.Replace(2100, dlgMultiSelect);

            try {
                //GUIDialogMultiSelect dlgMultiSelect = (GUIDialogMultiSelect)GUIWindowManager.GetWindow(2100);
                //if (dlgMultiSelect == null) return;

                dlgMultiSelect.Reset();

                dlgMultiSelect.SetHeading(heading);

                foreach (MultiSelectionItem multiSelectionItem in items) {
                    GUIListItem item = new GUIListItem();
                    item.Label = multiSelectionItem.ItemTitle;
                    item.Label2 = multiSelectionItem.ItemTitle2;
                    item.MusicTag = multiSelectionItem.Tag;
                    item.Selected = multiSelectionItem.Selected;

                    dlgMultiSelect.Add(item);
                }

                dlgMultiSelect.DoModal(GUIWindowManager.ActiveWindow);

                if (dlgMultiSelect.DialogModalResult == ModalResult.OK) {
                    for (int i = 0; i < items.Count; i++) {
                        MultiSelectionItem item = items[i];
                        MultiSelectionItem newMultiSelectionItem = new MultiSelectionItem();
                        newMultiSelectionItem.ItemTitle = item.ItemTitle;
                        newMultiSelectionItem.ItemTitle2 = item.ItemTitle2;
                        newMultiSelectionItem.ItemID = item.ItemID;
                        newMultiSelectionItem.Tag = item.Tag;
                        try {
                            newMultiSelectionItem.Selected = dlgMultiSelect.ListItems[i].Selected;
                        }
                        catch {
                            newMultiSelectionItem.Selected = item.Selected;
                        }

                        result.Add(newMultiSelectionItem);
                    }
                }
                else
                    return null;

                return result;
            }
            finally {
                GUIWindowManager.Replace(2100, dlgMultiSelectOld);
            }
        }

        public static string CleanSubtitleFile(string subtitleFile) {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(subtitleFile)) {
                result = Regex.Replace(subtitleFile, @"[-_\.]", " ");
                result = result.ToLowerInvariant();
            }


            return result;
        }

        public static void EnsureProperSubtitleFile(ref string subtitleFile) {
            if (string.IsNullOrEmpty(subtitleFile)) return;

            subtitleFile = FileUtils.fixInvalidFileName(subtitleFile);
            subtitleFile = subtitleFile.Trim();

            foreach (string extension in Extensions) {
                if (subtitleFile.ToLowerInvariant().EndsWith(extension)) {
                    subtitleFile = subtitleFile.Substring(0, subtitleFile.Length - extension.Length);
                }
            }

            if (string.IsNullOrEmpty(subtitleFile)) return;

            subtitleFile = @"c:\" + subtitleFile; // add drive (directory) to avoid default executing directory

            if (subtitleFile.Length < 256)
                subtitleFile += ".srt";
            else
                subtitleFile = subtitleFile.Substring(0, 255) + ".srt"; // file name length limit is 260
        }

        public static bool IsValidAlphaNumeric(string inputStr) {
            if (string.IsNullOrEmpty(inputStr))
                return false;

            for (int i = 0; i < inputStr.Length; i++) {
                if (!(char.IsLetter(inputStr[i])) && (!(char.IsNumber(inputStr[i]))))
                    return false;
            }
            return true;
        }

        public static string TrimNonAlphaNumeric(string inputStr) {
            string result = string.Empty;

            if (string.IsNullOrEmpty(inputStr))
                return result;

            for (int i = 0; i < inputStr.Length; i++) {
                if (char.IsLetter(inputStr[i]) || char.IsNumber(inputStr[i]))
                    result += inputStr[i];
            }
            return result;
        }

    }

    class MPR {
        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

        const int ERROR_MORE_DATA = 234;
        const int NOERROR = 0;

        [DllImport("mpr.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
            string lpLocalPath,
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
            IntPtr lpBuffer,
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        public static string GetUniversalName(string localPath) {
            // The return value.
            string retVal = null;

            // The pointer in memory to the structure.
            IntPtr buffer = IntPtr.Zero;

            // Wrap in a try/catch block for cleanup.
            try {
                // First, call WNetGetUniversalName to get the size.
                int size = 0;

                // Make the call.
                // Pass IntPtr.Size because the API doesn't like null, even though
                // size is zero.  We know that IntPtr.Size will be
                // aligned correctly.
                int apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                // If the return value is not ERROR_MORE_DATA, then
                // raise an exception.
                if (apiRetVal != ERROR_MORE_DATA)
                    // Throw an exception.
                    throw new Win32Exception(apiRetVal);

                // Allocate the memory.
                buffer = Marshal.AllocCoTaskMem(size);

                // Now make the call.
                apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                // If it didn't succeed, then throw.
                if (apiRetVal != NOERROR)
                    // Throw an exception.
                    throw new Win32Exception(apiRetVal);

                // Now get the string.  It's all in the same buffer, but
                // the pointer is first, so offset the pointer by IntPtr.Size
                // and pass to PtrToStringAuto.
                retVal = Marshal.PtrToStringAnsi(new IntPtr(buffer.ToInt64() + IntPtr.Size));
            }
            finally {
                // Release the buffer.
                Marshal.FreeCoTaskMem(buffer);
            }

            // First, allocate the memory for the structure.

            // That's all folks.
            return retVal;
        }
    }
}
