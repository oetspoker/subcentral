using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using SubCentral.Utils;
using NLog;

namespace SubCentral.Localizations {
    public static class Localization {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Private variables
        private static Dictionary<string, string> _translations;
        private static readonly string _path = string.Empty;
        private static readonly DateTimeFormatInfo _info;
        #endregion

        #region Constructor
        static Localization() {
            try {
                Lang = GUILocalizeStrings.GetCultureName(GUILocalizeStrings.CurrentLanguage());
                _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
            }
            catch (Exception) {
                Lang = CultureInfo.CurrentUICulture.Name;
                _info = DateTimeFormatInfo.GetInstance(CultureInfo.CurrentUICulture);
            }

            logger.Info("Using language: " + Lang);

            _path = Config.GetSubFolder(Config.Dir.Language, "SubCentral");

            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            LoadTranslations();
        }
        #endregion

        #region Public Properties
        // Gets the language actually used (after checking for localization file and fallback).
        public static string Lang { get; private set; }

        /// <summary>
        /// Gets the translated strings collection in the active language
        /// </summary>
        public static Dictionary<string, string> Strings {
            get {
                if (_translations == null) {
                    _translations = new Dictionary<string, string>();
                    Type transType = typeof(Localization);
                    FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
                    foreach (FieldInfo field in fields) {
                        _translations.Add(field.Name, field.GetValue(transType).ToString());
                    }
                }
                return _translations;
            }
        }
        #endregion

        #region Private methods
        private static int LoadTranslations() {
            XmlDocument doc = new XmlDocument();
            Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
            string langPath = "";
            try {
                langPath = Path.Combine(_path, Lang + ".xml");
                doc.Load(langPath);
            }
            catch (Exception e) {
                if (Lang == "en-US")
                    return 0; // otherwise we are in an endless loop!

                if (e.GetType() == typeof(FileNotFoundException))
                    logger.Warn("Cannot find translation file {0}. Failing back to English (US)", langPath);
                else {
                    logger.Error("Error in translation xml file: {0}. Failing back to English (US)", Lang);
                    logger.Error(e);
                }

                Lang = "en-US";
                return LoadTranslations();
            }
            foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes) {
                if (stringEntry.NodeType == XmlNodeType.Element)
                    try {
                        TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
                    }
                    catch (Exception e) {
                        logger.Error("Error in Translation Engine:");
                        logger.Error(e);
                    }
            }

            Type TransType = typeof(Localization);
            FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (FieldInfo fi in fieldInfos) {
                if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
                    TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
                else
                    logger.Info("Translation not found for field: {0}. Using hard-coded English default.", fi.Name);
            }
            return TranslatedStrings.Count;
        }
        #endregion

        #region Public Methods
        public static void Init() {
        }

        public static string GetByName(string name) {
            if (!Strings.ContainsKey(name))
                return name;

            return Strings[name];
        }

        public static string GetByName(string name, params object[] args) {
            return string.Format(GetByName(name), args);
        }

        /// <summary>
        /// Takes an input string and replaces all ${named} variables with the proper translation if available
        /// </summary>
        /// <param name="input">a string containing ${named} variables that represent the translation keys</param>
        /// <returns>translated input string</returns>
        public static string ParseString(string input) {
            Regex replacements = new Regex(@"\$\{([^\}]+)\}");
            MatchCollection matches = replacements.Matches(input);
            foreach (Match match in matches) {
                input = input.Replace(match.Value, GetByName(match.Groups[1].Value));
            }
            return input;
        }

        public static void TranslateSkin() {
            logger.Info("Translation: Translating skin");
            foreach (string name in Localization.Strings.Keys) {
                if (name.StartsWith("SkinTranslation")) {
                    GUIUtils.SetProperty("#SubCentral.Translation." + name.Replace("SkinTranslation", "") + ".Label", Localization.Strings[name], true);
                }
            }
        }

        public static string GetDayName(DayOfWeek dayOfWeek) {
            return _info.GetDayName(dayOfWeek);
        }

        public static string GetShortestDayName(DayOfWeek dayOfWeek) {
            return _info.GetShortestDayName(dayOfWeek);
        }
        #endregion

        #region Translations / Strings
        /// <summary>
        /// These will be loaded with the language files content
        /// if the selected lang file is not found, it will first try to load en(us).xml as a backup
        /// if that also fails it will use the hardcoded strings as a last resort.
        /// </summary>

        // #

        // A
        public static string AllProviders = "All providers";
        public static string AllEnabledProviders = "All enabled providers";
        public static string About = "About";
        public static string AllSubtitlesDownloaded = "All ({0}) subtitles downloaded!";
        public static string AllSubtitlesCanceledDownload = "All ({0}) subtitles canceled download!";
        public static string AboutText = ""; // will be defined in XML file
        public static string All = "All";
        public static string AllSubtitlesDeleted = "All ({0}) subtitles deleted!";

        // B
        public static string Back = "Back";

        // C
        public static string CannotLoadSkin = "Could not load the skin\nfile for SubCentral!";
        public static string Cancel = "Cancel";
        public static string Confirm = "Confirm";
        public static string ClearFiles = "Clear files";
        public static string ClearMedia = "Clear media";
        public static string CanceledDownload = "Canceled download!";
        public static string ContextMenu = "Context menu";
        public static string CannotUseManualSearch = "Cannot start manual search because no\nabsolute download folders are defined.\nPlease use configuration to add some.";
        public static string CannotClearMedia = "Cannot clear media files because no\nabsolute download folders are defined.\nPlease use configuration to add some.";
        public static string Completed = "Completed";

        // D
        public static string DefaultFor = "Default for: {0}";
        public static string DownloadFoderDoesNotExistCreate = "Download folder does not exist.\nCreate it?";
        public static string Download = "Download";
        public static string DownloadTo = "Download to...";
        public static string DefaultFolderNotWritable = "Default folder {0} is not writable! Please select another one.";
        public static string DefaultDownloadFoderDoesNotExistCreate = "Default download folder:\n{0}\ndoes not exist.\nCreate it?";
        public static string Default = "Default";
        public static string DeleteSubtitles = "Delete subtitles";

        // E
        public static string Error = "Error";
        public static string ExternalPlugin = "external plugin";
        public static string ErrorWhileDownloadingSubtitles = "Error(s) downloading subtitles!";
        public static string ErrorWhileDownloadingSubtitlesWithReason = "Error downloading subtitles: {0}";
        public static string ErrorWhileRetrievingSubtitles = "Error retrieving subtitles!";
        public static string ErrorWhileCreatingDirectory = "Error creating directory!";

        // F
        public static string FoundSubtitles = "Found subtitles";
        public static string From = "From: {0}";

        // G
        public static string GroupS = "Group(s)";
        public static string GroupProviderDefault = "Group/Provider default";

        // H

        // I
        public static string Initializing = "Initializing";
        public static string IMDbID = "IMDb ID: {0}";

        // L
        public static string Languages = "Languages";
        public static string Language = "Language";
        public static string Local = "Local (in media folders)";

        // M
        public static string Movies = "Movies";
        public static string ManualSearch = "Manual search";
        public static string ModifySearch = "Modify search";
        public static string MediaFilesDifferFromSubtitleFiles = "Media files count differs from subtitle files count. Wrong subtitles?";
        public static string MovieIMDb = "Movie (IMDb)";
        public static string Movie = "Movie";
        public static string MediaNoSubtitles = "Unable to find any subtitles for media!";
        public static string MediaOnlyInternalSubtitles = "Only embedded (internal) subtitles found for media!";
        public static string MediaWrongMarkHasSubtitles = "Media from {0} is marked with subtitles available\nbut I was unable to find any. Would you like to\ncorrect that and set subtitles unavailable?";
        public static string MediaMaybeInternalSubtitles = "(could still have embedded (internal) subtitles)";
        public static string MediaHasSubtitles = "Media already has subtitles. If you'd like to delete them use the menu button.";
        public static string MediaNoMoreSubtitles = "Unable to find any more subtitles for media.\nWould you like to set subtitles unavailable\nin the provider ({0})?";
        public static string MediaWrongMarkNoSubtitles = "Media from {0} is marked with subtitles unavailable\nbut I was able to find them. Would you like to\ncorrect that and set subtitles available?";

        // N
        public static string NewGroup = "New group";
        public static string NoResultsFound = "No results found";
        public static string NoSubtitlesFound = "No subtitles found";
        public static string NoDownloadFolders = "No valid download folders for this media were found.";
        public static string NotEnoughDataForSearch = "Not enough data for subtitle search";
        public static string Name = "Name";
        public static string NoSorting = "No sorting";
        public static string NoSubtitlesDelete = "No subtitle files found to delete.";

        // O
        public static string OKSearch = "OK/Search";

        // P
        public static string ProviderS = "Provider(s)";

        // Q
        public static string QueryingProviders = "Querying providers";

        // R
        public static string Revert = "Revert";

        // S
        public static string Search = "Search";
        public static string SubtitleDownloaderUnavailable = "Is SubtitleDownloader.dll available?";
        public static string SubtitleSearch = "Subtitle search";
        public static string SearchingSubtitles = "Searching subtitles...";
        public static string SelectLanguages = "Select languages";
        public static string SubtitlesExist = "Subtitle file already exists.\nOverwrite existing subtitle?";
        public static string SubtitlesDownloaded = "Subtitles downloaded!";
        public static string SelectDownloadFolder = "Select download folder";
        public static string SubtitleS = "Subtitle(s)";
        public static string Subtitle = "Subtitle";
        public static string Subtitles = "Subtitles";
        public static string SubtitlesDownloadedCount = "{0} subtitles downloaded";
        public static string SubtitlesDownloadErrorCount = "{0} subtitles failed to download";
        public static string SubtitlesDownloadCanceledCount = "{0} subtitles canceled download";
        public static string SelectFileForSubtitle = "Select media file for '{0}'";
        public static string SearchType = "Search type: {0}";
        public static string SiteDoesNotSupportIMDbIDSearch = "Site does not support IMDb ID search!";
        public static string SiteDoesNotSupportMovieSearch = "Site does not support movie search!";
        public static string SiteDoesNotSupportTVShowSearch = "Site does not support TV show search!";
        public static string Sorting = "Sorting";
        public static string Sort = "Sort";
        public static string SortBy = "Sort by: {0}";
        public static string SortByLanguage = "Sort by subtitle language";
        public static string SortByName = "Sort by subtitle name";
        public static string SubtitleFilesToDelete = "Subtitle files to delete";

        public static string SkinTranslationFrom = "From";
        public static string SkinTranslationSearchType = "Search type";
        public static string SkinTranslationMediaFiles = "Media files";
        public static string SkinTranslationDownloadFolder = "Download folder";
        public static string SkinTranslationSearchData = "Search data";
        public static string SkinTranslationIMDbID = "IMDb ID";
        public static string SkinTranslationTitle = "Title";
        public static string SkinTranslationYear = "Year";
        public static string SkinTranslationSeason = "Season";
        public static string SkinTranslationEpisode = "Episode";
        public static string SkinTranslationMovie = "Movie";
        public static string SkinTranslationTVShow = "TV show";

        // T
        public static string TVShows = "TV shows";
        public static string TVShow = "TV show";

        // U
        public static string UnableToImportDataFrom = "Unable to import data\nfrom {0}.";
        public static string UnableToLoadSubtitleDownloader = "Unable to load SubtitleDownloader library!";
        public static string UnableToDeleteSubtitleFile = "Unable to delete subtitle file!";
        public static string UnableToDeleteSubtitleFiles = "Unable to delete subtitle files:";

        // V

        // W
        public static string Warning = "Warning";
        public static string WrongFormatIMDbID = "Wrong format for IMDb ID (ttNNNNNNN)!";
        public static string WrongFormatYear = "Wrong format for year (1900 - {0})!";
        public static string WrongFormatSeasonEpisode = "{0} must be numeric and greater than 0!";

        // Y
        public static string PleaseSelectLanguages = "You have to select at least one\nlanguage!";
        #endregion
    }
}