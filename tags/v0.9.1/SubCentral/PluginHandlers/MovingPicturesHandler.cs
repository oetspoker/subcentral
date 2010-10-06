using System;
using System.Collections.Generic;
using System.IO;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.MainUI;
using NLog;

namespace SubCentral.PluginHandlers {
    internal class MovingPicturesHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MovieBrowser browser;
        private DBMovieInfo selectedMovie;

        public override int ID {
            get { return 96742; }
            set { }
        }

        public override string PluginName {
            get { return "Moving Pictures"; }
            set { }
        }

        // The video file details we want to grab subtitles for.
        public override BasicMediaDetail MediaDetail {
            get { return _mediaDetail; }
            set { }
        }
        private BasicMediaDetail _mediaDetail;

        public override bool Modified {
            get { return false; }
            set { }
        }

        // retrieves info from Moving Pictures
        protected override bool GrabFileDetails() {
            try {
                browser = MovingPicturesCore.Browser;
                selectedMovie = browser.SelectedMovie;
                List<DBLocalMedia> localMedia = selectedMovie.LocalMedia;

                _mediaDetail = new BasicMediaDetail();

                _mediaDetail.Title = selectedMovie.Title;
                _mediaDetail.Year = selectedMovie.Year;
                _mediaDetail.ImdbID = selectedMovie.ImdbID;

                _mediaDetail.Thumb = selectedMovie.CoverThumbFullPath;
                _mediaDetail.FanArt = selectedMovie.BackdropFullPath;

                //selectedMovie.LocalMedia[0].HasSubtitles

                _mediaDetail.Files = new List<FileInfo>();
                foreach (DBLocalMedia localMediaItem in localMedia) {
                    _mediaDetail.Files.Add(new FileInfo(localMediaItem.FullPath));
                }

                return true;
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error when pulling data from Moving Pictures\n", e);
                return false;
            }
        }

        public override bool GetHasSubtitles() {
            foreach (DBLocalMedia localMediaItem in selectedMovie.LocalMedia)
                if (localMediaItem.HasSubtitles) return true;
            return false;
        }

        public override void SetHasSubtitles(string fileName, bool value) {
            if (_mediaDetail.Files == null || _mediaDetail.Files.Count == 0) return;

            foreach (DBLocalMedia localMediaItem in selectedMovie.LocalMedia) {
                if (localMediaItem.FullPath == fileName) {
                    localMediaItem.HasSubtitles = value;
                    localMediaItem.Commit();
                }
            }
        }

        protected override bool IsAvailable() {
            if (!IsAssemblyAvailable("MovingPictures", new Version(1, 0, 3)))
                return false;

            return true;
        }
    }
}
