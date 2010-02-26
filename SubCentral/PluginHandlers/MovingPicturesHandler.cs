using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.MainUI;
using MediaPortal.Plugins.MovingPictures.Database;
using NLog;
using System.Reflection;

namespace SubCentral.PluginHandlers {
    internal class MovingPicturesHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MovieBrowser browser;

        public override int ID {
            get { return 96742; }
        }

        public override string PluginName {
            get { return "Moving Pictures"; }
        }

        // The video file we want to grab subtitles for.
        public override FileInfo File {
            get { return _file; }
        }
        private FileInfo _file = null;

        // The name of the movie we are pulling subtitles for.
        public override string Description {
            get { return _description; }
        }
        private string _description = "";

        // retrieves info from Moving Pictures
        protected override bool GrabFileDetails() {
            try {
                browser = MovingPicturesCore.Browser; 
                DBMovieInfo selectedMovie = browser.SelectedMovie;
                List<DBLocalMedia> localMedia = selectedMovie.LocalMedia;
                _description = string.Format("{0} ({1})", selectedMovie.Title, selectedMovie.Year);
                _file = new FileInfo(localMedia[0].FullPath);
                return true;
            }
            catch (Exception) {
                logger.Error("Unexpected error when pulling data from Moving Pictures.");
                return false;
            }

        }

        protected override bool IsAvailable() {
            if (!IsAssemblyAvailable("MovingPictures", new Version(1,0,3)))
                return false;

            return true;
        }
    }
}
