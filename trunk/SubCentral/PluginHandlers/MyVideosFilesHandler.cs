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
    internal class MyVideosFilesHandler : MyVideosTitleHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override int ID {
            get { return (int)GUIVideoTitle.Window.WINDOW_VIDEOS; }
            set { }
        }

        public override string PluginName {
            get { return "My Videos (Files)"; }
            set { }
        }

        protected override bool GrabFileDetails() {
            try {
                //bool result = base.GrabFileDetails(); // TODO
                if (!result) {
                    GUIVideoFiles videoFilesWindow = GUIWindowManager.GetWindow(ID) as GUIVideoFiles;

                    GUIListItem videoTitleSelectedItem = null;

                    if (videoFilesWindow != null) {
                        //videoTitleSelectedItem = videoFilesWindow.GetSelectedItem(); // TODO
                    }
                    if (videoTitleSelectedItem != null && !videoTitleSelectedItem.IsFolder &&
                        !videoTitleSelectedItem.Path.IsNullOrWhiteSpace() && File.Exists(videoTitleSelectedItem.Path))
                    {
                        _mediaDetail = new BasicMediaDetail();
                        FileInfo fi = new FileInfo(videoTitleSelectedItem.Path);

                        _mediaDetail.Title = fi.Name;
                        _mediaDetail.Thumb = videoTitleSelectedItem.ThumbnailImage;
                        _mediaDetail.FanArt = string.Empty;
                        _mediaDetail.Files = new List<FileInfo>();
                        _mediaDetail.Files.Add(fi);

                        result = true;
                    }
                }

                return result;
            }
            catch (Exception e) {
                logger.ErrorException(string.Format("Unexpected error when pulling data from My Videos (Files){0}", Environment.NewLine), e);
                return false;
            }
        }

        protected override bool IsAvailable() {
            return false;
        }
    }
}
