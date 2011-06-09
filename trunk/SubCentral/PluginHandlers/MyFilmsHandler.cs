using System;
using System.Collections.Generic;
using System.IO;
using MyFilmsPlugin.MyFilms;
using MyFilmsPlugin.MyFilms.MyFilmsGUI;
using NLog;

namespace SubCentral.PluginHandlers {
  internal class MyFilmsHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private MFMovie selectedMovie;

        public override int ID 
        {
            get { return 7987; }
            set { }
        }

        public override string PluginName 
        {
            get { return "MyFilms"; }
            set { }
        }

        public override BasicMediaDetail MediaDetail 
        {
            get { return _mediaDetail; }
            set { }
        }
        private BasicMediaDetail _mediaDetail;

        public override bool Modified {
            get { return false; }
            set { }
        }

        protected override bool GrabFileDetails() {
            try {
                selectedMovie = MyFilmsDetail.GetCurrentMovie();
                
                _mediaDetail = new BasicMediaDetail();

                _mediaDetail.Title = selectedMovie.Title;
                _mediaDetail.Year = selectedMovie.Year;
                _mediaDetail.ImdbID = selectedMovie.IMDBNumber;

                _mediaDetail.Thumb = selectedMovie.Picture;
                _mediaDetail.FanArt = selectedMovie.Fanart;

                _mediaDetail.Files = new List<FileInfo>();

                string[] files = selectedMovie.File.Trim().Split(new Char[] { ';' });
                foreach (string file in files) {
                    _mediaDetail.Files.Add(new FileInfo(file));
                }
                return true;
            }
            catch (Exception e) {
                logger.ErrorException(string.Format("Unexpected error when pulling data from MyFilms{0}", Environment.NewLine), e);
                return false;
            }
        }

        public override bool GetHasSubtitles(bool all) {
            return base.GetHasSubtitles(all);
        }

        public override void SetHasSubtitles(string fileName, bool value) {
            base.SetHasSubtitles(fileName, value);
        }

        protected override PluginHandlerType GetPluginHandlerType() {
            return PluginHandlerType.BASICWITHOUTDB;
        }

        protected override bool IsAvailable() {
          if (!IsAssemblyAvailable("MyFilms", new Version(5, 0, 1)))
              return false;

          return true;
        }
    }
}
