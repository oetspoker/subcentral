using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using SubCentral.GUI.Items;
using SubCentral.Localizations;
using SubCentral.PluginHandlers;
using SubCentral.Settings.Data;
using SubCentral.Utils;
using NLog;

namespace SubCentral.GUI {
    public class Retriever {

        public static Retriever Instance {
            get {
                if (_instance == null)
                    _instance = new Retriever();

                return _instance;
            }
        }
        private static Retriever _instance = null;

        #region Private Variables
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SubCentralCore core = SubCentralCore.Instance;

        private bool _progressReportingEnabled = false;
        private List<string> _languageCodes = null;
        private Dictionary<string, string> _providerIDsAndtitles = null;
        private Thread _subtitlesDownloaderThread = null;
        private bool _isCanceled = false;
        SubtitlesSearchType _searchType = SubtitlesSearchType.NONE;
        #endregion

        #region Delegates
        private delegate bool IsCanceledDelegate();
        private delegate void OnCompletedDelegate();
        private delegate bool OnQueryStartingDelegate();
        private delegate void OnProgressDelegate(string line1, string line2, string line3, int percent);
        #endregion

        #region Public Events
        public delegate void OnProviderSearchErrorDelegate(BasicMediaDetail mediaDetail, SubtitlesSearchType subtitlesSearchType, Exception e);
        public delegate void OnSubtitlesSearchErrorDelegate(Exception e);
        public delegate void OnSubtitleSearchCompletedDelegate(List<SubtitleItem> subtitleItems, bool canceled);
        public delegate void OnSubtitleDownloadedToTempDelegate(BasicMediaDetail mediaDetail, List<FileInfo> subtitleFiles);
        public delegate void OnSubtitleDownloadedDelegate(BasicMediaDetail mediaDetail, List<SubtitleDownloadStatus> statusList);

        public event OnProviderSearchErrorDelegate OnProviderSearchErrorEvent;
        public event OnSubtitlesSearchErrorDelegate OnSubtitlesSearchErrorEvent;
        public event OnSubtitleSearchCompletedDelegate OnSubtitleSearchCompletedEvent;
        public event OnSubtitleDownloadedToTempDelegate OnSubtitleDownloadedToTempEvent;
        public event OnSubtitleDownloadedDelegate OnSubtitleDownloadedEvent;
        #endregion

        #region Public Constants
        #endregion

        #region Constructors
        private Retriever() {
        }
        #endregion

        #region Public Methods
        public void FillData(bool progressReportingEnabled, List<string> languageCodes, Dictionary<string, string> providerIDsNames, SubtitlesSearchType searchType) {
            _providerIDsAndtitles = providerIDsNames;
            _progressReportingEnabled = progressReportingEnabled;
            _languageCodes = languageCodes;
            _searchType = searchType;
        }

        public void SearchForSubtitles(BasicMediaDetail mediaDetail) {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive)
                return;

            _isCanceled = false;

            _subtitlesDownloaderThread = new Thread(SearchSubtitlesAsynch);
            _subtitlesDownloaderThread.IsBackground = true;
            _subtitlesDownloaderThread.Name = "Subtitles Downloader Thread";
            _subtitlesDownloaderThread.Start(mediaDetail);
        }

        public bool IsRunning() {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive)
                return true;
            return false;
        }

        public void Kill() {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive) {
                _subtitlesDownloaderThread.Abort();
                _subtitlesDownloaderThread = null;
            }
        }
        #endregion

        #region Private Methods
        private void SearchSubtitlesAsynch(object mediaDetailObj) {
            if (!(mediaDetailObj is BasicMediaDetail))
                throw new ArgumentException("Parameter must be type of List<BasicMediaDetail>!");

            try {
                OnQueryStarting();

                Dictionary<string, CustomSubtitleDownloader> downloaders = HarvestDownloaders();

                List<SubtitleItem> allResults = null;

                // foreach {
                BasicMediaDetail mediaDetail = (BasicMediaDetail)mediaDetailObj;
                allResults = QueryDownloaders(mediaDetail, downloaders);
                //}

                if (OnSubtitleSearchCompletedEvent != null) {
                    OnSubtitleSearchCompletedEvent(allResults, _isCanceled);
                }

                OnCompleted();
            }
            catch (ThreadAbortException) {
                _isCanceled = true;
            }
            catch (Exception e) {
                try {
                    OnCompleted();
                }
                catch {
                }
                if (OnSubtitlesSearchErrorEvent != null) {
                    OnSubtitlesSearchErrorEvent(e);
                }
            }
        }

        private bool OnQueryStarting() {
            if (!_progressReportingEnabled) return true;

            if (GUIGraphicsContext.form.InvokeRequired) {
                //OnQueryStartingDelegate d = OnQueryStarting;
                //return (bool)GUIGraphicsContext.form.Invoke(d);
            }

            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            pDlgProgress.Reset();

            pDlgProgress.ShowProgressBar(true);

            pDlgProgress.SetHeading(Localization.SearchingSubtitles);
            pDlgProgress.SetLine(1, Localization.Initializing);
            pDlgProgress.SetLine(2, string.Empty);

            pDlgProgress.SetPercentage(0);

            pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
            return true;
        }

        private Dictionary<string, CustomSubtitleDownloader> HarvestDownloaders() {
            Dictionary<string, CustomSubtitleDownloader> downloaders = new Dictionary<string, CustomSubtitleDownloader>();

            foreach (KeyValuePair<string, string> kvp in _providerIDsAndtitles) {
                if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value)) {
                    SubtitleDownloader.Core.ISubtitleDownloader downloader = SubtitleDownloader.Core.SubtitleDownloaderFactory.GetSubtitleDownloader(kvp.Key);
                    string downloaderTitle = kvp.Value;

                    CustomSubtitleDownloader csd = new CustomSubtitleDownloader();
                    csd.downloader = downloader;
                    csd.downloaderTitle = downloaderTitle;
                    downloaders.Add(kvp.Key, csd);
                }
            }
            return downloaders;
        }

        private List<SubtitleItem> QueryDownloaders(BasicMediaDetail mediaDetail, Dictionary<string, CustomSubtitleDownloader> downloaders) {
            List<SubtitleItem> allResults = new List<SubtitleItem>();

            SubtitleDownloader.Core.EpisodeSearchQuery episodeQuery = null;
            SubtitleDownloader.Core.SearchQuery movieQuery = null;
            SubtitleDownloader.Core.ImdbSearchQuery queryIMDB = null;

            SubtitlesSearchType subtitlesSearchType = _searchType; //SubCentralUtils.getSubtitlesSearchTypeFromMediaDetail(mediaDetail);

            switch (subtitlesSearchType) {
                case SubtitlesSearchType.IMDb:
                    queryIMDB = new SubtitleDownloader.Core.ImdbSearchQuery(mediaDetail.ImdbID);
                    queryIMDB.LanguageCodes = _languageCodes.ToArray();
                    if (SubCentralUtils.canSearchMediaDetailWithType(mediaDetail, SubtitlesSearchType.MOVIE)) {
                        movieQuery = new SubtitleDownloader.Core.SearchQuery(mediaDetail.Title);
                        movieQuery.Year = mediaDetail.Year;
                        movieQuery.LanguageCodes = _languageCodes.ToArray();
                    }
                    break;
                case SubtitlesSearchType.TVSHOW:
                    episodeQuery = new SubtitleDownloader.Core.EpisodeSearchQuery(mediaDetail.Title, mediaDetail.Season, mediaDetail.Episode);
                    episodeQuery.LanguageCodes = _languageCodes.ToArray();
                    break;
                case SubtitlesSearchType.MOVIE:
                    movieQuery = new SubtitleDownloader.Core.SearchQuery(mediaDetail.Title);
                    movieQuery.Year = mediaDetail.Year;
                    movieQuery.LanguageCodes = _languageCodes.ToArray();
                    break;
                case SubtitlesSearchType.NONE:
                    return allResults;
            }

            int providerCount = 1;
            foreach (KeyValuePair<string, CustomSubtitleDownloader> kvp in downloaders) {
                if (IsCanceled()) {
                    allResults.Clear();
                    break;
                }

                SubtitleDownloader.Core.ISubtitleDownloader subsDownloader = kvp.Value.downloader;
                string providerName = kvp.Value.downloaderTitle;

                List<SubtitleDownloader.Core.Subtitle> resultsFromDownloader = null;
                int percent = (100 / downloaders.Count) * providerCount;

                OnProgress(Localization.QueryingProviders + " (" + Convert.ToString(providerCount) + "/" + Convert.ToString(downloaders.Count) + "):",
                           providerName,
                           Localization.FoundSubtitles + ": " + Convert.ToString(allResults.Count),
                           percent);

                try {
                    switch (subtitlesSearchType) {
                        case SubtitlesSearchType.IMDb:
                            bool shouldThrowNotSupportedException = false;
                            try {
                                resultsFromDownloader = subsDownloader.SearchSubtitles(queryIMDB);
                                if (resultsFromDownloader.Count == 0 && movieQuery != null) {
                                    List<SubtitleDownloader.Core.Subtitle> resultsFromDownloaderForMovieQuery = subsDownloader.SearchSubtitles(movieQuery);
                                    if (resultsFromDownloaderForMovieQuery.Count > 0) {
                                        // means that the site does not support imdb queries, should throw later
                                        logger.Debug("Site {0} does not support IMDb ID search, got the results using regular movie search", providerName);
                                        resultsFromDownloader.AddRange(resultsFromDownloaderForMovieQuery);
                                        shouldThrowNotSupportedException = true;
                                    }
                                }
                            }
                            catch (Exception e) {
                                if (e is NotImplementedException || e is NotSupportedException) {
                                    if (movieQuery != null) {
                                        logger.Debug("Site {0} does not support IMDb ID search, will try regular movie search", providerName);
                                        try {
                                            resultsFromDownloader = subsDownloader.SearchSubtitles(movieQuery);
                                        }
                                        catch (Exception e1) { 
                                            // if the error happens now, we're probably searching some sites that support only tv shows? 
                                            // so instead of returning not supported, we're gonna return no results
                                            // perhaps in the future, new exception type should be thrown to indicade that site does not support movie search
                                            // 19.06.2010, SE: it's the future! :)
                                            if (e1 is NotImplementedException || e1 is NotSupportedException) {
                                                subtitlesSearchType = SubtitlesSearchType.MOVIE;
                                                throw new NotSupportedException(string.Format("Site {0} does not support movie search!", providerName));
                                            }
                                            else {
                                                throw e1;
                                            }
                                        }
                                    }
                                    else {
                                        //logger.Debug("Site {0} does not support IMDb ID search", providerName);
                                        throw new NotSupportedException(string.Format("Site {0} does not support IMDb ID search!", providerName));
                                    }
                                }
                                else {
                                    // throw it!
                                    throw e;
                                }
                            }
                            if (shouldThrowNotSupportedException) {
                                // meh, do not throw, since we already have results and it's logged
                                //throw new NotSupportedException("Site {0} does not support IMDb ID search");
                            }
                            break;
                        case SubtitlesSearchType.TVSHOW:
                            resultsFromDownloader = subsDownloader.SearchSubtitles(episodeQuery);
                            break;
                        case SubtitlesSearchType.MOVIE:
                            resultsFromDownloader = subsDownloader.SearchSubtitles(movieQuery);
                            break;
                    }
                }
                catch (Exception e) {
                    logger.ErrorException(string.Format("Error while querying site {0}", providerName), e);
                    if (OnProviderSearchErrorEvent != null) {
                        OnProviderSearchErrorEvent(mediaDetail, subtitlesSearchType, e);
                    }
                }

                if (resultsFromDownloader != null && resultsFromDownloader.Count > 0) {
                    foreach (SubtitleDownloader.Core.Subtitle subtitle in resultsFromDownloader) {
                        SubtitleItem subItem = new SubtitleItem();
                        subItem.Downloader = subsDownloader;
                        subItem.Subtitle = subtitle;
                        subItem.ProviderTitle = kvp.Key;
                        subItem.LanguageName = subtitle.LanguageCode;
                        if (!string.IsNullOrEmpty(SubCentralUtils.SubsLanguages[subtitle.LanguageCode]))
                            subItem.LanguageName = SubCentralUtils.SubsLanguages[subtitle.LanguageCode];
                        allResults.Add(subItem);
                    }
                }
                providerCount++;
            }
            return allResults;
        }

        private bool IsCanceled() {
            if (!_progressReportingEnabled) return false;

            if (GUIGraphicsContext.form.InvokeRequired) {
                IsCanceledDelegate d = IsCanceled;
                return (bool)GUIGraphicsContext.form.Invoke(d);
            }

            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            _isCanceled = pDlgProgress.IsCanceled;

            return (_isCanceled);
        }

        private void OnProgress(string line1, string line2, string line3, int percent) {
            if (!_progressReportingEnabled) return;

            if (GUIGraphicsContext.form.InvokeRequired) {
                //OnProgressDelegate d = new OnProgressDelegate(OnProgress);
                //GUIGraphicsContext.form.Invoke(d, line1, line2, line3, percent);
                //return;
            }

            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            pDlgProgress.ShowProgressBar(true);
            pDlgProgress.SetLine(1, line1);
            pDlgProgress.SetLine(2, line2);
            pDlgProgress.SetLine(3, line3);

            if (percent > 0)
                pDlgProgress.SetPercentage(percent);

            pDlgProgress.Progress();
        }

        private void OnCompleted() {
            if (!_progressReportingEnabled) return;

            if (GUIGraphicsContext.form.InvokeRequired) {
                OnCompletedDelegate d = OnCompleted;
                GUIGraphicsContext.form.Invoke(d);
                return;
            }

            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            if (pDlgProgress != null)
                pDlgProgress.Close();
        }
        #endregion

        #region Download
        public void DownloadSubtitle(SubtitleItem subtitleItem, BasicMediaDetail mediaDetail, FolderSelectionItem folderSelectionItem, SubtitlesSearchType searchType, bool skipDefaults) {
            logger.Info("Downloading subtitles...");

            SubtitleDownloader.Core.ISubtitleDownloader subDownloader = subtitleItem.Downloader; ;
            SubtitleDownloader.Core.Subtitle subtitle = subtitleItem.Subtitle;

            List<SubtitleDownloadStatus> statusList = new List<SubtitleDownloadStatus>();

            List<FileInfo> subtitleFiles = null;
            try {
                subtitleFiles = subDownloader.SaveSubtitle(subtitle);
            }
            catch {
                subtitleFiles = null;
            }

            if (OnSubtitleDownloadedToTempEvent != null)
                OnSubtitleDownloadedToTempEvent(mediaDetail, subtitleFiles);

            int subtitleNr = 0;
            if (subtitleFiles != null && subtitleFiles.Count > 0) {
                logger.Info("{0} subtitle(s) downloaded to temporary folder.", subtitleFiles.Count);

                if (mediaDetail.Files != null && mediaDetail.Files.Count > 0) {
                    logger.Info("Video files:");
                    foreach (FileInfo fileInfo in mediaDetail.Files) {
                        logger.Info(" {0}", fileInfo.Name);
                    }

                    if (mediaDetail.Files.Count != subtitleFiles.Count) {
                        logger.Warn("Video and subtitle file count mismatch! {0} video files, {1} subtitle files", mediaDetail.Files.Count, subtitleFiles.Count);
                    }

                    if (mediaDetail.Files.Count < subtitleFiles.Count || (mediaDetail.Files.Count > subtitleFiles.Count && subtitleFiles.Count > 1)) {
                        GUIUtils.ShowNotifyDialog(Localization.Error, string.Format(Localization.ErrorWhileDownloadingSubtitlesWithReason, Localization.MediaFilesDifferFromSubtitleFiles), GUIUtils.NoSubtitlesLogoThumbPath);
                        return;
                    }
                    else if (mediaDetail.Files.Count > subtitleFiles.Count && subtitleFiles.Count == 1) {
                        List<GUIListItem> dlgMenuItems = new List<GUIListItem>();
                        foreach (FileInfo fileInfo in mediaDetail.Files) {
                            GUIListItem listItem = new GUIListItem();
                            listItem.Label = fileInfo.Name;
                            listItem.MusicTag = fileInfo;

                            dlgMenuItems.Add(listItem);
                        }
                        subtitleNr = GUIUtils.ShowMenuDialog(string.Format(Localization.SelectFileForSubtitle, subtitleFiles[0].Name), dlgMenuItems);
                        if (subtitleNr < 0)
                            logger.Debug("User canceled media selection dialog for subtitle {0}", subtitleFiles[0].Name);
                        else
                            logger.Debug("User selected media file {0} for subtitle {1}", mediaDetail.Files[subtitleNr].Name, subtitleFiles[0].Name);
                    }
                }

                foreach (FileInfo subtitleFile in subtitleFiles) {
                    SubtitleDownloadStatus newSubtitleDownloadStatus = new SubtitleDownloadStatus() {
                        Index = subtitleNr,
                        Error = string.Empty
                    };
                    try {
                        string videoFileName = string.Empty;
                        string targetSubtitleFile = string.Empty;
                        string subtitleFileNameFull = string.Empty;
                        string subtitleFileName = string.Empty;
                        string subtitleFileExt = string.Empty;

                        if (mediaDetail.Files != null && mediaDetail.Files.Count > 0) {
                            if (subtitleNr < 0) {
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Canceled;
                                continue;
                            }
                            videoFileName = Path.GetFileName(mediaDetail.Files[subtitleNr].FullName);
                            videoFileName = Path.Combine(folderSelectionItem.FolderName, videoFileName);
                            targetSubtitleFile = SubtitleDownloader.Util.FileUtils.GetFileNameForSubtitle(subtitleFile.Name,
                                                                                                          subtitle.LanguageCode,
                                                                                                          videoFileName);

                            subtitleFileName = Path.GetFileNameWithoutExtension(targetSubtitleFile);
                            subtitleFileExt = Path.GetExtension(targetSubtitleFile);
                            subtitleFileNameFull = subtitleFileName + subtitleFileExt;
                        }
                        else {
                            targetSubtitleFile = Path.Combine(folderSelectionItem.FolderName, subtitleFile.Name);

                            subtitleFileName = Path.GetFileNameWithoutExtension(targetSubtitleFile);
                            if (!string.IsNullOrEmpty(SubCentralUtils.SubsLanguages[subtitle.LanguageCode]))
                                subtitleFileName = subtitleFileName + "." + SubCentralUtils.SubsLanguages[subtitle.LanguageCode];
                            subtitleFileExt = Path.GetExtension(targetSubtitleFile);
                            subtitleFileNameFull = subtitleFileName + subtitleFileExt;

                            targetSubtitleFile = Path.Combine(folderSelectionItem.FolderName, subtitleFileNameFull);

                            if (Settings.SettingsManager.Properties.FolderSettings.OnDownloadFileName == OnDownloadFileName.AskIfManual) {
                                if (GUIUtils.GetStringFromKeyboard(ref subtitleFileNameFull)) {
                                    targetSubtitleFile = Path.Combine(Path.GetDirectoryName(targetSubtitleFile), subtitleFileNameFull);
                                    subtitleFileName = Path.GetFileNameWithoutExtension(targetSubtitleFile);
                                    subtitleFileExt = Path.GetExtension(targetSubtitleFile);
                                    subtitleFileNameFull = subtitleFileName + subtitleFileExt;
                                }
                                else {
                                    newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Canceled;
                                    continue;
                                }
                            }
                        }

                        if (skipDefaults || Settings.SettingsManager.Properties.FolderSettings.OnDownloadFileName == OnDownloadFileName.AlwaysAsk) {
                            if (GUIUtils.GetStringFromKeyboard(ref subtitleFileNameFull)) {
                                targetSubtitleFile = Path.Combine(Path.GetDirectoryName(targetSubtitleFile), subtitleFileNameFull);
                                subtitleFileName = Path.GetFileNameWithoutExtension(targetSubtitleFile);
                                subtitleFileExt = Path.GetExtension(targetSubtitleFile);
                                subtitleFileNameFull = subtitleFileName + subtitleFileExt;
                            }
                            else {
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Canceled;
                                continue;
                            }
                        }

                        bool targetFileExists = File.Exists(targetSubtitleFile);

                        if (targetFileExists) {
                            bool overwriteFile = GUIUtils.ShowYesNoDialog(Localization.Confirm, Localization.SubtitlesExist);

                            if (overwriteFile) {
                                try {
                                    File.Delete(targetSubtitleFile);
                                    File.Move(subtitleFile.FullName, targetSubtitleFile);
                                    newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Succesful;
                                }
                                catch (Exception e) {
                                    logger.ErrorException("Error while downloading subtitles", e);
                                    newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Error;
                                    newSubtitleDownloadStatus.Error = e.Message;
                                }
                            }
                            else {
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.AlreadyExists;
                            }
                        }
                        else {
                            try {
                                File.Move(subtitleFile.FullName, targetSubtitleFile);
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Succesful;
                            }
                            catch (Exception e) {
                                logger.ErrorException("Error while downloading subtitles", e);
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Error;
                                newSubtitleDownloadStatus.Error = e.Message;
                            }
                        }
                        subtitleNr++;
                    }
                    finally {
                        statusList.Add(newSubtitleDownloadStatus);
                    }
                }
            }

            if (OnSubtitleDownloadedEvent != null)
                OnSubtitleDownloadedEvent(mediaDetail, statusList);
        }
        #endregion

        private class CustomSubtitleDownloader {
            public SubtitleDownloader.Core.ISubtitleDownloader downloader { get; set; }
            public string downloaderTitle { get; set; }
        }
    }
}