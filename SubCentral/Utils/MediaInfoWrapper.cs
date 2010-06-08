using System;
using System.Collections.Generic;
using System.IO;
using MediaPortal.Player;
using NLog;

namespace SubCentral.Utils {
    public class MediaInfoWrapper {
        #region private vars
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MediaInfo _mI = null;

        private int _numSubtitles = 0;

        private bool _hasSubtitles = false;
        private bool _hasExternalSubtitles = false;

        private static List<string> _subTitleExtensions = new List<string>();
        #endregion

        #region Constructor
        public MediaInfoWrapper(string strFile, bool useMediaInfo, int cachedMISubtitleCount, bool useLocalOnly) {
            bool isTV = MediaPortal.Util.Utils.IsLiveTv(strFile);
            bool isRadio = MediaPortal.Util.Utils.IsLiveRadio(strFile);
            bool isDVD = MediaPortal.Util.Utils.IsDVD(strFile);
            bool isVideo = MediaPortal.Util.Utils.IsVideo(strFile);
            bool isAVStream = MediaPortal.Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.

            if (isTV || isRadio || isAVStream) {
                return;
            }

            logger.Debug("MediaInfoWrapper: Inspecting media : {0}", strFile);

            if (cachedMISubtitleCount > -1) {
                _numSubtitles = cachedMISubtitleCount;
                useMediaInfo = false;
            }

            if (useLocalOnly) {
                _numSubtitles = 0;
                useMediaInfo = false;
            }

            _hasExternalSubtitles = checkHasExternalSubtitles(strFile, useLocalOnly);

            try {
                if (useMediaInfo) {
                    _mI = new MediaInfo();
                    _mI.Open(strFile);

                    int.TryParse(_mI.Get(StreamKind.General, 0, "TextCount"), out _numSubtitles);
                }

                if (_hasExternalSubtitles) {
                    _hasSubtitles = true;
                }
                else if (_numSubtitles > 0) {
                    _hasSubtitles = true;
                }
                else {
                    _hasSubtitles = false;
                }

                logger.Debug("MediaInfoWrapper: HasExternalSubtitles : {0}", _hasExternalSubtitles);
                logger.Debug("MediaInfoWrapper: HasSubtitles : {0}", _hasSubtitles);
                logger.Debug("MediaInfoWrapper: NumSubtitles : {0}", _numSubtitles);
            }
            catch (Exception ex) {
                logger.Error("MediaInfoWrapper: MediaInfo processing failed ('MediaInfo.dll' may be missing): {0}", ex.Message);
            }
            finally {
                if (_mI != null) {
                    _mI.Close();
                }
            }
        }
        #endregion

        #region private methods
        private bool checkHasExternalSubtitles(string strFile, bool useLocalOnly) {
            if (_subTitleExtensions.Count == 0) {
                // load them in first time
                _subTitleExtensions.Add(".aqt");
                _subTitleExtensions.Add(".asc");
                _subTitleExtensions.Add(".ass");
                _subTitleExtensions.Add(".dat");
                _subTitleExtensions.Add(".dks");
                _subTitleExtensions.Add(".js");
                _subTitleExtensions.Add(".jss");
                _subTitleExtensions.Add(".lrc");
                _subTitleExtensions.Add(".mpl");
                _subTitleExtensions.Add(".ovr");
                _subTitleExtensions.Add(".pan");
                _subTitleExtensions.Add(".pjs");
                _subTitleExtensions.Add(".psb");
                _subTitleExtensions.Add(".rt");
                _subTitleExtensions.Add(".rtf");
                _subTitleExtensions.Add(".s2k");
                _subTitleExtensions.Add(".sbt");
                _subTitleExtensions.Add(".scr");
                _subTitleExtensions.Add(".smi");
                _subTitleExtensions.Add(".son");
                _subTitleExtensions.Add(".srt");
                _subTitleExtensions.Add(".ssa");
                _subTitleExtensions.Add(".sst");
                _subTitleExtensions.Add(".ssts");
                _subTitleExtensions.Add(".stl");
                _subTitleExtensions.Add(".sub");
                _subTitleExtensions.Add(".txt");
                _subTitleExtensions.Add(".vkt");
                _subTitleExtensions.Add(".vsf");
                _subTitleExtensions.Add(".zeg");
            }

            string filenameNoExt = System.IO.Path.GetFileNameWithoutExtension(strFile);

            try {
                List<string> folders = new List<string>();
                if (useLocalOnly) {
                    folders.Add(System.IO.Path.GetDirectoryName(strFile));
                }
                else {
                    folders = SubCentralUtils.getEnabledAndValidFolderNamesForMedia(new FileInfo(strFile), true, true);
                    logger.Debug("MediaInfoWrapper: Got {0} folders for media {1}", folders.Count, strFile);
                }

                foreach (string folder in folders) {
                    //if (folder.FolderErrorInfo == SubCentral.Enums.FolderErrorInfo.NonExistant) continue;

                    //if (!SubCentralUtils.pathExists(folder.FolderName)) continue;

                    if (string.IsNullOrEmpty(folder) || !NetUtils.uncHostIsAlive(folder)) continue;

                    try {
                        foreach (string file in System.IO.Directory.GetFiles(folder, filenameNoExt + "*")) {
                            System.IO.FileInfo fi = new System.IO.FileInfo(file);
                            if (_subTitleExtensions.Contains(fi.Extension.ToLower())) return true;
                        }
                    }
                    catch (Exception e) {
                        // Most likely path not available
                        logger.Warn("Error checking external subtitles for folder " + folder, e);
                    }
                }
            }
            catch (Exception e) {
                logger.Warn("Error checking external subtitles for file " + strFile, e);
            }

            return false;
        }
        #endregion

        #region Public Subtitle Properties
        public bool HasSubtitles {
            get { return _hasSubtitles; }
        }

        public bool HasExternalSubtitles {
            get { return _hasExternalSubtitles; }
        }

        public int NumSubtitles {
            get { return _numSubtitles; }
        }
        #endregion
    }
}
