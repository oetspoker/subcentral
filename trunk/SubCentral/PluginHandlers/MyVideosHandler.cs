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
    internal class MyVideosHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override int ID {
            get { return (int)GUIVideoTitle.Window.WINDOW_VIDEO_TITLE; }
            set { }
        }

        public override string PluginName {
            get { return "My Videos"; }
            set { }
        }

        public override BasicMediaDetail MediaDetail {
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
                //GUIVideoTitle videoTitleWindow = GUIWindowManager.GetWindow(ID) as GUIVideoTitle;
                GUIVideoTitle videoTitleWindow = MPGUIExtensions.GetWindowEx(ID, false) as GUIVideoTitle;  

                GUIFacadeControl videoTitleFacade = null;
                GUIListItem videoTitleSelectedItem = null;
                IMDBMovie videoTitleMovie = null;
                string videoTitleMovieThumb = string.Empty;

                if (videoTitleWindow != null) {
                    videoTitleFacade = videoTitleWindow.FacadeControl();
                }
                if (videoTitleFacade != null) {
                    videoTitleSelectedItem = videoTitleFacade.SelectedListItem;
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
                    #if MP11
                    _mediaDetail.FanArt = string.Empty;
                    #else
                    string fanart = string.Empty;
                    MediaPortal.Util.FanArt.GetFanArtfilename(videoTitleMovie.Title, 0, out fanart);
                    _mediaDetail.FanArt = fanart;
                    #endif

                    ArrayList files = new ArrayList();
                    VideoDatabase.GetFiles(videoTitleMovie.ID, ref files);

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
                logger.ErrorException("Unexpected error when pulling data from MyVideos\n", e);
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
            if (!IsAssemblyAvailable("WindowPlugins", new Version(1, 1, 0)))
                return false;

            return true;
        }
    }
}
