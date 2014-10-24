using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.Video;
using MediaPortal.Video.Database;
using NLog;
using SubCentral.GUI.Extensions;

namespace SubCentral.PluginHandlers {
    internal class MyVideosTitleHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override int ID {
            get { return (int)GUIVideoTitle.Window.WINDOW_VIDEO_TITLE; }
            set { }
        }

        public override string PluginName {
            get { return "My Videos (Title)"; }
            set { }
        }

        public override BasicMediaDetail MediaDetail {
            get { return _mediaDetail; }
            set { }
        }
        protected BasicMediaDetail _mediaDetail;

        public override bool Modified {
            get { return false; }
            set { }
        }

        protected override bool GrabFileDetails() {
            try {
                GUIVideoTitle videoTitleWindow = GUIWindowManager.GetWindow(ID) as GUIVideoTitle;

                GUIListItem videoTitleSelectedItem = null;
                IMDBMovie videoTitleMovie = null;
                string videoTitleMovieThumb = string.Empty;

                if (videoTitleWindow != null) {
                    videoTitleSelectedItem = videoTitleWindow.GetSelectedItem();
                }
                if (videoTitleSelectedItem != null && !videoTitleSelectedItem.IsFolder) {
                    videoTitleMovie = videoTitleSelectedItem.AlbumInfoTag as IMDBMovie;
                    videoTitleMovieThumb = videoTitleSelectedItem.ThumbnailImage;
                }
                if (videoTitleMovie != null && videoTitleMovie.ID > 0) {
                    _mediaDetail = new BasicMediaDetail();

                    _mediaDetail.Title = videoTitleMovie.Title;
                    _mediaDetail.Year = videoTitleMovie.Year > 1900 ? videoTitleMovie.Year : 0;
                    _mediaDetail.ImdbID = videoTitleMovie.IMDBNumber;

                    _mediaDetail.Thumb = videoTitleMovieThumb;
                    string fanart = string.Empty;
                    MediaPortal.Util.FanArt.GetFanArtfilename(videoTitleMovie.Title, 0, out fanart);
                    _mediaDetail.FanArt = fanart;

                    ArrayList files = new ArrayList();
                    VideoDatabase.GetFilesForMovie(videoTitleMovie.ID, ref files);

                    _mediaDetail.Files = new List<FileInfo>();
                    foreach (string file in files) {
                        _mediaDetail.Files.Add(new FileInfo(file));
                    }
                    if (_mediaDetail.Files.Count < 1) {
                        _mediaDetail.Files.Add(new FileInfo(videoTitleMovie.File));
                    }

                    return true;
                }
                return false;
            }
            catch (Exception e) {
                logger.ErrorException(string.Format("Unexpected error when pulling data from My Videos (Title){0}", Environment.NewLine), e);
                return false;
            }
        }

        protected override PluginHandlerType GetPluginHandlerType() {
            return PluginHandlerType.BASICWITHOUTDB;
        }

        protected override bool IsAvailable() {
            if (!IsAssemblyAvailable("GUIVideos", new Version(1, 8, 0)))
                return false;

            return true;
        }
    }
}
