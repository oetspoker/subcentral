using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using SubCentral.GUI.Items;
using SubCentral.Localizations;
using SubCentral.PluginHandlers;
using SubCentral.Settings;
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
        private Thread _subtitlesDownloaderStatusThread = null;
        private bool _isCanceled = false;
        private ThreadStatus _status = ThreadStatus.StatusEnded;
        SubtitlesSearchType _searchType = SubtitlesSearchType.NONE;
        #endregion

        #region Enums
        public enum ThreadStatus {
            StatusRunning,
            StatusStartedWithWaitCursor,
            StatusStartedWithoutWaitCursor,
            StatusEnded
        }
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
        public delegate void OnSubtitlesDownloadErrorDelegate(Exception e);

        public event OnProviderSearchErrorDelegate OnProviderSearchErrorEvent;
        public event OnSubtitlesSearchErrorDelegate OnSubtitlesSearchErrorEvent;
        public event OnSubtitleSearchCompletedDelegate OnSubtitleSearchCompletedEvent;
        public event OnSubtitleDownloadedToTempDelegate OnSubtitleDownloadedToTempEvent;
        public event OnSubtitleDownloadedDelegate OnSubtitleDownloadedEvent;
        public event OnSubtitlesDownloadErrorDelegate OnSubtitlesDownloadErrorEvent;
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
            if (IsCanceled())
                Kill();
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive)
                return;

            _isCanceled = false;
            _status = ThreadStatus.StatusEnded;

            _subtitlesDownloaderThread = new Thread(SearchSubtitlesAsync);
            _subtitlesDownloaderThread.IsBackground = true;
            _subtitlesDownloaderThread.Name = "Subtitles Downloader Thread";
            _subtitlesDownloaderThread.Start(mediaDetail);
        }

        public bool IsCanceled() {
            if (!_progressReportingEnabled) return false;
            //if (!_isStarted) return false;

            if (GUIGraphicsContext.form.InvokeRequired) {
                IsCanceledDelegate d = IsCanceled;
                return (bool)GUIGraphicsContext.form.Invoke(d);
            }

            GUIDialogProgress pDlgProgress = (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            _isCanceled = pDlgProgress.IsCanceled || (Status == ThreadStatus.StatusStartedWithWaitCursor && GUIWindowManager.RoutedWindow != (int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            return (_isCanceled);
        }

        public bool IsRunning() {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive) {
                return true;
            }
            return false;
        }

        public void Kill() {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive) {
                _subtitlesDownloaderThread.Abort();
                _subtitlesDownloaderThread = null;
            }
            if (_subtitlesDownloaderStatusThread != null && _subtitlesDownloaderStatusThread.IsAlive) {
                _subtitlesDownloaderStatusThread.Abort();
                _subtitlesDownloaderStatusThread = null;
            }
        }

        public ThreadStatus Status {
            get {
                return _status;
            }
        }
        #endregion

        #region Private Methods
        private void SubtitleDownloaderSetGUIStatus(object mainThread) {
            try {
                //while (true) {
                while ((mainThread == null) || (mainThread != null && (mainThread as Thread).IsAlive)) {
                    if (Status == ThreadStatus.StatusStartedWithWaitCursor && !GUIWindowManager.IsRouted)
                        SubCentralGUI.ShowWaitCursor();
                    else
                        SubCentralGUI.HideWaitCursor();

                    if (mainThread != null && IsCanceled())
                        (mainThread as Thread).Abort();

                    Thread.Sleep(100);
                }
                SubCentralGUI.HideWaitCursor();
            }
            catch (Exception) {
                SubCentralGUI.HideWaitCursor();
                // silently abort
            }
        }

        private void SearchSubtitlesAsync(object mediaDetailObj) {
            if (!(mediaDetailObj is BasicMediaDetail))
                throw new ArgumentException("Parameter must be type of BasicMediaDetail!", "mediaDetailObj");

            List<SubtitleItem> allResults = new List<SubtitleItem>();

            try {
                _status = ThreadStatus.StatusRunning;

                OnQueryStarting();

                _status = ThreadStatus.StatusStartedWithWaitCursor;

                _subtitlesDownloaderStatusThread = new Thread(SubtitleDownloaderSetGUIStatus);
                _subtitlesDownloaderStatusThread.IsBackground = true;
                _subtitlesDownloaderStatusThread.Name = "Subtitles Downloader Status Thread";
                _subtitlesDownloaderStatusThread.Start(_subtitlesDownloaderThread);

                Dictionary<string, CustomSubtitleDownloader> downloaders = HarvestDownloaders();

                BasicMediaDetail mediaDetail = (BasicMediaDetail)mediaDetailObj;
                QueryDownloaders(mediaDetail, downloaders, ref allResults);

                OnBeforeCompleted(allResults);

                OnCompleted();
            }
            catch (ThreadAbortException) {
                _isCanceled = true;
                OnBeforeCompleted(allResults);
            }
            catch (Exception e) {
                try {
                    OnCompleted();
                }
                catch {
                }
                if (OnSubtitlesSearchErrorEvent != null) {
                    if (GUIGraphicsContext.form.InvokeRequired) {
                        OnSubtitlesSearchErrorDelegate d = OnSubtitlesSearchErrorEvent;
                        GUIGraphicsContext.form.Invoke(d, e);
                    }
                    else {
                        OnSubtitlesSearchErrorEvent(e);
                    }
                }
            }
            _status = ThreadStatus.StatusEnded;
            if (_subtitlesDownloaderStatusThread != null && _subtitlesDownloaderStatusThread.IsAlive)
                _subtitlesDownloaderStatusThread.Abort();
        }

        private bool OnQueryStarting() {
            if (!_progressReportingEnabled) return true;

            if (GUIGraphicsContext.form.InvokeRequired) {
                OnQueryStartingDelegate d = OnQueryStarting;
                return (bool)GUIGraphicsContext.form.Invoke(d);
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

        private List<SubtitleItem> QueryDownloaders(BasicMediaDetail mediaDetail, Dictionary<string, CustomSubtitleDownloader> downloaders, ref List<SubtitleItem> allResults) {
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
                    episodeQuery = new SubtitleDownloader.Core.EpisodeSearchQuery(mediaDetail.Title, mediaDetail.SeasonProper, mediaDetail.EpisodeProper);
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
                    //allResults.Clear();
                    break;
                }

                SubtitleDownloader.Core.ISubtitleDownloader subsDownloader = kvp.Value.downloader;
                string providerName = kvp.Value.downloaderTitle;

                //testing SearchTimeout
                subsDownloader.SearchTimeout = SettingsManager.Properties.GeneralSettings.SearchTimeout;

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
                catch (ThreadAbortException e) {
                    throw e;
                }
                catch (Exception e) {
                    logger.ErrorException(string.Format("Error while querying site {0}\n", providerName), e);
                    if (OnProviderSearchErrorEvent != null) {
                        if (GUIGraphicsContext.form.InvokeRequired) {
                            OnProviderSearchErrorDelegate d = OnProviderSearchErrorEvent;
                            GUIGraphicsContext.form.Invoke(d, mediaDetail, subtitlesSearchType, e);
                        }
                        else {
                            OnProviderSearchErrorEvent(mediaDetail, subtitlesSearchType, e);
                        }
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

        private void OnProgress(string line1, string line2, string line3, int percent) {
            if (!_progressReportingEnabled) return;

            if (GUIGraphicsContext.form.InvokeRequired) {
                OnProgressDelegate d = OnProgress;
                GUIGraphicsContext.form.Invoke(d, line1, line2, line3, percent);
                return;
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

        private void OnBeforeCompleted(List<SubtitleItem> allResults) {
            _status = ThreadStatus.StatusStartedWithoutWaitCursor;

            if (OnSubtitleSearchCompletedEvent != null) {
                if (GUIGraphicsContext.form.InvokeRequired) {
                    OnSubtitleSearchCompletedDelegate d = OnSubtitleSearchCompletedEvent;
                    GUIGraphicsContext.form.Invoke(d, allResults, _isCanceled);
                }
                else {
                    OnSubtitleSearchCompletedEvent(allResults, _isCanceled);
                }
            }
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
            if (IsCanceled())
                Kill();
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive)
                return;

            _isCanceled = false;
            _status = ThreadStatus.StatusEnded;

            DownloadData downloadData = new DownloadData {
                SubtitleItem = subtitleItem,
                MediaDetail = mediaDetail,
                FolderSelectionItem = folderSelectionItem,
                SearchType = searchType,
                SkipDefaults = skipDefaults,
                StatusList = new List<SubtitleDownloadStatus>()
            };

            _subtitlesDownloaderThread = new Thread(DownloadSubtitleAsync);
            _subtitlesDownloaderThread.IsBackground = true;
            _subtitlesDownloaderThread.Name = "Subtitles Downloader Thread";
            _subtitlesDownloaderThread.Start(downloadData);
        }

        public void DownloadSubtitleAsync(object downloadDataObj) {
            DownloadData downloadData = downloadDataObj as DownloadData;
            if (downloadData == null)
                throw new ArgumentException("Parameter must be type of DownloadData!", "downloadDataObj");

            SubtitleItem subtitleItem = downloadData.SubtitleItem;
            BasicMediaDetail mediaDetail = downloadData.MediaDetail;
            FolderSelectionItem folderSelectionItem = downloadData.FolderSelectionItem;
            SubtitlesSearchType searchType = downloadData.SearchType;
            bool skipDefaults = downloadData.SkipDefaults;
            List<SubtitleDownloadStatus> statusList = downloadData.StatusList;

            logger.Info("Downloading subtitles...");

            _status = ThreadStatus.StatusStartedWithWaitCursor;

            _subtitlesDownloaderStatusThread = new Thread(SubtitleDownloaderSetGUIStatus);
            _subtitlesDownloaderStatusThread.IsBackground = true;
            _subtitlesDownloaderStatusThread.Name = "Subtitles Downloader Status Thread";
            _subtitlesDownloaderStatusThread.Start(null);

            List<FileInfo> subtitleFiles = null;

            try {
                SubtitleDownloader.Core.ISubtitleDownloader subDownloader = subtitleItem.Downloader;
                SubtitleDownloader.Core.Subtitle subtitle = subtitleItem.Subtitle;

                try {
                    subtitleFiles = subDownloader.SaveSubtitle(subtitle);
                }
                catch (ThreadAbortException e) {
                  subtitleFiles = null;
                  throw e;
                }
                catch (Exception e) {
                    subtitleFiles = null;
                    OnSubtitlesDownloadError(e);
                    return;
                }

                OnSubtitlesDownloadedToTemp(mediaDetail, subtitleFiles);

                logger.Info("{0} subtitle(s) downloaded to temporary folder.", subtitleFiles != null ? subtitleFiles.Count : 0);

                if (subtitleFiles != null && subtitleFiles.Count > 0) {
                    List<SubtitleMediaMapping> mapping = new List<SubtitleMediaMapping>();

                    if (mediaDetail.Files != null && mediaDetail.Files.Count > 0) {
                        logger.Info("Video files:");
                        foreach (FileInfo fileInfo in mediaDetail.Files) {
                            logger.Info(" {0}", fileInfo.Name);
                        }

                        if (mediaDetail.Files.Count != subtitleFiles.Count) {
                            logger.Warn("Video and subtitle file count mismatch! {0} video files, {1} subtitle files", mediaDetail.Files.Count, subtitleFiles.Count);
                        }

                        if (mediaDetail.Files.Count > subtitleFiles.Count && subtitleFiles.Count == 1) {
                            List<GUIListItem> dlgMenuItems = new List<GUIListItem>();
                            foreach (FileInfo fileInfo in mediaDetail.Files) {
                                GUIListItem listItem = new GUIListItem();
                                listItem.Label = fileInfo.Name;
                                listItem.MusicTag = fileInfo;

                                dlgMenuItems.Add(listItem);
                            }
                            int mediaNr = GUIUtils.ShowMenuDialog(string.Format(Localization.SelectFileForSubtitle, subtitleFiles[0].Name), dlgMenuItems);
                            if (mediaNr < 0) {
                                logger.Debug("User canceled media selection dialog for subtitle {0}", subtitleFiles[0].Name);
                            }
                            else {
                                logger.Debug("User selected media file {0} for subtitle {1}", mediaDetail.Files[mediaNr].Name, subtitleFiles[0].Name);
                            }
                            mapping.Add(new SubtitleMediaMapping() { SubtitleIndex = 0, MediaIndex = mediaNr }); // add to mapping
                        }
                        else if (mediaDetail.Files.Count != subtitleFiles.Count && subtitleFiles.Count > 1) {
                            int subtitleNr = 0;
                            int mediaNr = -1;

                            foreach (FileInfo mediaFileInfo in mediaDetail.Files) {
                                mediaNr++;

                                if (subtitleNr < 0) {
                                    mapping.Add(new SubtitleMediaMapping() { SubtitleIndex = -1, MediaIndex = mediaNr }); // add to mapping
                                    continue;
                                }

                                List<GUIListItem> dlgMenuItems = new List<GUIListItem>();
                                for (int i = 0; i < subtitleFiles.Count; i++) {
                                    FileInfo fileInfo = subtitleFiles[i];

                                    bool subInMapping = false;
                                    foreach (SubtitleMediaMapping mapSM in mapping) {
                                        if (mapSM.SubtitleIndex == i) {
                                            subInMapping = true;
                                            break;
                                        }
                                    }
                                    if (!subInMapping) {
                                        GUIListItem listItem = new GUIListItem();
                                        listItem.Label = fileInfo.Name;
                                        listItem.MusicTag = fileInfo;

                                        dlgMenuItems.Add(listItem);
                                    }
                                }
                                subtitleNr = -1;
                                if (dlgMenuItems.Count > 0) { // if we "used" all the subtitles, mark other media files as cancelled. not really common
                                    subtitleNr = GUIUtils.ShowMenuDialog(string.Format(Localization.SelectSubtitleForFile, mediaFileInfo.Name), dlgMenuItems);
                                }
                                if (subtitleNr < 0)
                                    logger.Debug("User canceled subtitle selection dialog for media {0}", mediaFileInfo.Name);
                                else {
                                    logger.Debug("User selected subtitle file {0} for media {1}", subtitleFiles[subtitleNr].Name, mediaFileInfo.Name);
                                }
                                mapping.Add(new SubtitleMediaMapping() { SubtitleIndex = subtitleNr, MediaIndex = mediaNr }); // add to mapping
                            }
                        }
                        else { // same count
                            for (int i = 0; i < mediaDetail.Files.Count; i++) {
                                mapping.Add(new SubtitleMediaMapping() { SubtitleIndex = i, MediaIndex = i }); // 1:1 mapping
                            }
                        }
                    }
                    else {
                        bool download = subtitleFiles.Count < 4 ? true : GUIUtils.ShowYesNoDialog(Localization.TooManySubtitles, string.Format(Localization.TooManySubtitlesQuestion, subtitleFiles.Count));
                        for (int i = 0; i < subtitleFiles.Count; i++) {
                            mapping.Add(new SubtitleMediaMapping() { SubtitleIndex = download ? i : -1, MediaIndex = -1 }); // 1:1 mapping without media
                        }
                    }

                    foreach (SubtitleMediaMapping mapSM in mapping) {
                        int subtitleNr = mapSM.SubtitleIndex;
                        int mediaNr = mapSM.MediaIndex;

                        SubtitleDownloadStatus newSubtitleDownloadStatus = new SubtitleDownloadStatus() {
                            Index = mediaNr,
                            Error = string.Empty,
                            Status = SubtitleDownloadStatusStatus.Canceled
                        };

                        try {
                            string videoFileName = string.Empty;
                            string targetSubtitleFile = string.Empty;
                            string subtitleFileNameFull = string.Empty;
                            string subtitleFileName = string.Empty;
                            string subtitleFileExt = string.Empty;

                            if (subtitleNr < 0) {
                                newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Canceled;
                                continue;
                            }

                            FileInfo subtitleFile = subtitleFiles[subtitleNr];

                            if (mediaDetail.Files != null && mediaDetail.Files.Count > 0) {
                                if (mediaNr < 0) {
                                    newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Canceled;
                                    continue;
                                }
                                videoFileName = Path.GetFileName(mediaDetail.Files[mediaNr].FullName);
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
                                if (!string.IsNullOrEmpty(SubCentralUtils.SubsLanguages[subtitle.LanguageCode]) && !subtitleFileName.Contains("." + SubCentralUtils.SubsLanguages[subtitle.LanguageCode]))
                                    subtitleFileName = subtitleFileName + "." + SubCentralUtils.SubsLanguages[subtitle.LanguageCode];
                                subtitleFileExt = Path.GetExtension(targetSubtitleFile);
                                subtitleFileNameFull = subtitleFileName + subtitleFileExt;

                                targetSubtitleFile = Path.Combine(folderSelectionItem.FolderName, subtitleFileNameFull);

                                if (Settings.SettingsManager.Properties.FolderSettings.OnDownloadFileName == OnDownloadFileName.AskIfManual) {
                                    if (GUIUtils.GetStringFromKeyboard(ref subtitleFileNameFull) && !string.IsNullOrEmpty(subtitleFileNameFull)) {
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
                                if (GUIUtils.GetStringFromKeyboard(ref subtitleFileNameFull) && !string.IsNullOrEmpty(subtitleFileNameFull)) {
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
                                    catch (ThreadAbortException e) {
                                        throw e;
                                    }
                                    catch (Exception e) {
                                        logger.ErrorException("Error while downloading subtitles\n", e);
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
                                catch (ThreadAbortException e) {
                                    throw e;
                                }
                                catch (Exception e) {
                                    logger.ErrorException("Error while downloading subtitles\n", e);
                                    newSubtitleDownloadStatus.Status = SubtitleDownloadStatusStatus.Error;
                                    newSubtitleDownloadStatus.Error = e.Message;
                                }
                            }
                        }
                        finally {
                            statusList.Add(newSubtitleDownloadStatus);
                        }
                    }
                }
            }
            catch (ThreadAbortException) {
                _isCanceled = true;
                if ((subtitleFiles == null || subtitleFiles.Count < 1) && (mediaDetail.Files == null || mediaDetail.Files.Count < 1)) {
                    logger.Debug("Download thread was aborted");
                    statusList = null;
                    // nothing for now, we have nothing
                }
                else {
                    logger.Debug("Download thread was aborted, counting 'cancelled' downloads");
                    int counter = -1;
                    if (statusList != null && mediaDetail.Files != null && mediaDetail.Files.Count > 0) {
                        counter = mediaDetail.Files.Count;
                    }
                    else if (statusList != null && subtitleFiles != null && subtitleFiles.Count > 0) {
                        counter = subtitleFiles.Count;
                    }
                    for (int i = statusList.Count; i < counter; i++)
                        statusList.Add(new SubtitleDownloadStatus() { Index = -1, Error = string.Empty, Status = SubtitleDownloadStatusStatus.Canceled });
                }
            }
            finally {
                _status = ThreadStatus.StatusEnded;
                if (_subtitlesDownloaderStatusThread != null && _subtitlesDownloaderStatusThread.IsAlive)
                    _subtitlesDownloaderStatusThread.Abort();
            }
            OnSubtitlesDownloaded(mediaDetail, statusList);
        }

        private void OnSubtitlesDownloadError(Exception e) {
            _status = ThreadStatus.StatusStartedWithoutWaitCursor;

            if (OnSubtitlesDownloadErrorEvent != null) {
                if (GUIGraphicsContext.form.InvokeRequired) {
                    OnSubtitlesDownloadErrorDelegate d = OnSubtitlesDownloadErrorEvent;
                    GUIGraphicsContext.form.Invoke(d, e);
                }
                else {
                    OnSubtitlesDownloadErrorEvent(e);
                }
            }
        }

        private void OnSubtitlesDownloadedToTemp(BasicMediaDetail mediaDetail, List<FileInfo> subtitleFiles) {
            if (OnSubtitleDownloadedToTempEvent != null) {
                if (GUIGraphicsContext.form.InvokeRequired) {
                    OnSubtitleDownloadedToTempDelegate d = OnSubtitleDownloadedToTempEvent;
                    GUIGraphicsContext.form.Invoke(d, mediaDetail, subtitleFiles);
                }
                else {
                    OnSubtitleDownloadedToTempEvent(mediaDetail, subtitleFiles);
                }
            }
        }

        private void OnSubtitlesDownloaded(BasicMediaDetail mediaDetail, List<SubtitleDownloadStatus> statusList) {
            _status = ThreadStatus.StatusStartedWithoutWaitCursor;

            if (OnSubtitleDownloadedEvent != null) {
                if (GUIGraphicsContext.form.InvokeRequired) {
                    OnSubtitleDownloadedDelegate d = OnSubtitleDownloadedEvent;
                    GUIGraphicsContext.form.Invoke(d, mediaDetail, statusList);
                }
                else {
                    OnSubtitleDownloadedEvent(mediaDetail, statusList);
                }
            }
        }
        #endregion

        private class CustomSubtitleDownloader {
            public SubtitleDownloader.Core.ISubtitleDownloader downloader { get; set; }
            public string downloaderTitle { get; set; }
        }

        private class SubtitleMediaMapping {
            public int SubtitleIndex { get; set; }
            public int MediaIndex { get; set; }
        }

        private class DownloadData {
            public SubtitleItem SubtitleItem { get; set; }
            public BasicMediaDetail MediaDetail { get; set; }
            public FolderSelectionItem FolderSelectionItem { get; set; }
            public SubtitlesSearchType SearchType { get; set; }
            public bool SkipDefaults { get; set; }
            public List<SubtitleDownloadStatus> StatusList { get; set; }
        }
    }
}