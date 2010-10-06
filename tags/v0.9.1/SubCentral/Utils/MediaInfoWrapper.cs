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

        private List<FileInfo> _subtitleFiles = new List<FileInfo>();
        #endregion

        #region Constructor
        public MediaInfoWrapper(string strFile, bool useMediaInfo, int cachedMISubtitleCount, bool useLocalOnly, bool checkAll) {
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

            _hasExternalSubtitles = checkHasExternalSubtitles(strFile, useLocalOnly, checkAll);

            if (useMediaInfo) {
                try {
                    logger.Debug("MediaInfoWrapper: Trying to use MediaInfo");
                    _mI = new MediaInfo();
                    _mI.Open(strFile);

                    int.TryParse(_mI.Get(StreamKind.General, 0, "TextCount"), out _numSubtitles);
                }
                catch (Exception e) {
                    logger.ErrorException("MediaInfoWrapper: MediaInfo processing failed ('MediaInfo.dll' may be missing)\n", e);
                }
                finally {
                    if (_mI != null) {
                        _mI.Close();
                    }
                }
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
        #endregion

        #region private methods
        private bool checkHasExternalSubtitles(string strFile, bool useLocalOnly, bool checkAll) {
            bool result = false;

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

                    if (string.IsNullOrEmpty(folder) /*|| !NetUtils.uncHostIsAlive(folder)*/) continue;

                    try {
                        foreach (string file in System.IO.Directory.GetFiles(folder, filenameNoExt + "*")) {
                            System.IO.FileInfo fi = new System.IO.FileInfo(file);
                            if (SubCentralUtils.SubtitleExtensions.Contains(fi.Extension.ToLowerInvariant())) {
                                if (checkAll) {
                                    result = result || true;
                                    _subtitleFiles.Add(fi);
                                }
                                else {
                                    return true;
                                }
                            }
                        }
                    }
                    catch (Exception) {
                        // Most likely path not available
                        // There is absolutely no need to print this error
                        // - It's annoying
                        // - Confuses user
                        // - Fills up log file quickly
                        // - Makes log file hard to read
                        // - Does not provide any useful information
                        //logger.WarnException(string.Format("Error checking external subtitles for folder {0}\n", folder), e);
                    }
                }
            }
            catch (Exception e) {
                logger.WarnException(string.Format("Error checking external subtitles for file {0}\n", strFile), e);
            }

            return result;
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

        public List<FileInfo> SubtitleFiles {
            get { return _subtitleFiles; }
        }
        #endregion
    }
}
