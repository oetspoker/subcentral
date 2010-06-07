using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Cornerstone.Extensions.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using System.IO;
using NLog;
using System.Text.RegularExpressions;
using System.Linq;

namespace SubCentral.Utils {
    public class MediaInfoWrapper {
        #region private vars
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private MediaInfo _mI = null;

        private int _numSubtitles = 0;

        private bool _hasSubtitles = false;
        private static List<string> _subTitleExtensions = new List<string>();
        #endregion

        #region Constructor
        public MediaInfoWrapper(string strFile) {
            bool isTV = MediaPortal.Util.Utils.IsLiveTv(strFile);
            bool isRadio = MediaPortal.Util.Utils.IsLiveRadio(strFile);
            bool isDVD = MediaPortal.Util.Utils.IsDVD(strFile);
            bool isVideo = MediaPortal.Util.Utils.IsVideo(strFile);
            bool isAVStream = MediaPortal.Util.Utils.IsAVStream(strFile); //rtsp users for live TV and recordings.

            if (isTV || isRadio || isAVStream) {
                return;
            }

            logger.Debug("MediaInfoWrapper: inspecting media : {0}", strFile);
            try {
                _mI = new MediaInfo();
                _mI.Open(strFile);

                FileInfo fileInfo = strFile.PathToFileInfo();
                DriveInfo driveInfo = fileInfo.GetDriveInfo();

                int.TryParse(_mI.Get(StreamKind.General, 0, "TextCount"), out _numSubtitles);

                if (checkHasExternalSubtitles(strFile)) {
                    _hasSubtitles = true;
                }
                else if (_numSubtitles > 0) {
                    _hasSubtitles = true;
                }
                else {
                    _hasSubtitles = false;
                }
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
        private bool checkHasExternalSubtitles(string strFile) {
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
                foreach (string file in System.IO.Directory.GetFiles(System.IO.Path.GetDirectoryName(strFile), filenameNoExt + "*")) {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    if (_subTitleExtensions.Contains(fi.Extension.ToLower())) return true;
                }
            }
            catch (Exception) {
                // most likley path not available
            }

            return false;
        }
        #endregion

        #region Public Subtitle Properties
        public bool HasSubtitles {
            get { return _hasSubtitles; }
        }

        public int NumSubtitles {
            get { return _numSubtitles; }
        }
        #endregion
    }
}
