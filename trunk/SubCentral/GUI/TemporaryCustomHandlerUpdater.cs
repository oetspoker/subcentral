using System.Collections.Generic;
using System.IO;
using MediaPortal.GUI.Library;
using NLog;
using SubCentral.Enums;
using SubCentral.PluginHandlers;
using SubCentral.Utils;

namespace SubCentral.GUI {
    public static class TemporaryCustomHandlerUpdater {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void SetupUpdater() {
            GUIWindowManager.Receivers += new SendMessageHandler(GUIWindowManager_Receivers);
        }

        private static void GUIWindowManager_Receivers(GUIMessage message) {
            if (message.Message == GUIMessage.MessageType.GUI_MSG_USER) {
                if (message.TargetWindowId == SubCentralGUI.SubCentralGUIID) {
                    if (message.Object != null && message.Object is List<string> &&
                        message.SenderControlId >= 0 /*&& !MainWindowIDs.Contains(message.SenderControlId)*/) {
                        logger.Info("Received a custom provider message");

                        List<string> msgObj = message.Object as List<string>;

                        AddTemporaryCustomPluginHandler(message.SenderControlId, msgObj);

                        //return true;
                    }
                }
            }
        }

        private static void AddTemporaryCustomPluginHandler(int ID, List<string> pluginData) {
            TemporaryCustomPluginHandler customTemporaryPluginHandler = SubCentralCore.Instance.PluginHandlers[PluginHandlerType.CUSTOM] as TemporaryCustomPluginHandler;

            logger.Debug("Adding temporary handler for plugin ID {0} from sent data", ID);

            // we have to get our custom provider and data from the provider should be 9 or more
            // plugin_name, imdb, title, year, season, episode, thumb, fanart, filename1, filename2, ...
            if (customTemporaryPluginHandler != null && pluginData.Count >= 9) {

                string pluginName = pluginData[0].Trim();

                int pluginID = ID;

                string imdbID = pluginData[1].Trim();

                string title = pluginData[2].Trim();

                int year = -1;
                int.TryParse(pluginData[3].Trim(), out year);

                int season = -1;
                int.TryParse(pluginData[4].Trim(), out season);
                int episode = -1;
                int.TryParse(pluginData[5].Trim(), out episode);

                string thumb = pluginData[6].Trim();

                string fanart = pluginData[7].Trim();

                List<FileInfo> fileList = new List<FileInfo>();
                for (int i = 8; i < pluginData.Count; i++) {
                    if (!string.IsNullOrEmpty(pluginData[i].Trim())) {
                        try {
                            FileInfo fi = new FileInfo(pluginData[i].Trim());
                            fileList.Add(fi);
                        }
                        catch {
                        }
                    }
                }

                if (string.IsNullOrEmpty(pluginName) || fileList.Count == 0) return;

                BasicMediaDetail mediaDetail = new BasicMediaDetail();
                mediaDetail.ImdbID = imdbID;
                mediaDetail.Title = title;
                if (SubCentralUtils.isYearCorrect(year.ToString()))
                    mediaDetail.Year = year;
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(season.ToString()))
                    mediaDetail.Season = season;
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(episode.ToString()))
                    mediaDetail.Episode = episode;
                mediaDetail.Thumb = thumb;
                mediaDetail.FanArt = fanart;
                mediaDetail.Files = fileList;

                if (SubCentralUtils.getSubtitlesSearchTypeFromMediaDetail(mediaDetail) == SubtitlesSearchType.NONE) return;

                // all ok!
                customTemporaryPluginHandler.ID = pluginID;
                customTemporaryPluginHandler.PluginName = pluginName;
                customTemporaryPluginHandler.MediaDetail = mediaDetail;
            }
        }

    }
}
