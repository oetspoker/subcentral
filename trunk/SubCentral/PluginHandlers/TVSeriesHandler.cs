using System;
using System.Collections.Generic;
using System.IO;
using WindowPlugins.GUITVSeries;
using NLog;

namespace SubCentral.PluginHandlers {
    internal class TVSeriesHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        DBEpisode episode;
        DBSeries series;

        public override int ID {
            get { return 9811; }
            set { }
        }

        public override string PluginName {
            get { return "TVSeries"; }
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
                episode = WindowPlugins.GUITVSeries.TVSeriesPlugin.m_SelectedEpisode;
                if (episode == null) return false;

                series = Helper.getCorrespondingSeries(episode[DBEpisode.cSeriesID]);
                if (series == null) return false;

                string seriesTitle = series[DBOnlineSeries.cOriginalName];
                
                string seasonIdx = episode[DBEpisode.cSeasonIndex];
                string episodeIdx = episode[DBEpisode.cEpisodeIndex];
                string episodeIdxAbs = episode[DBOnlineEpisode.cAbsoluteNumber];

                bool absolute = false;

                if (series[DBOnlineSeries.cChosenEpisodeOrder] == "Absolute" && !string.IsNullOrEmpty(episodeIdxAbs)) {
                    absolute = true;
                }
                
                int seasonIdxInt = -1;
                int.TryParse(seasonIdx, out seasonIdxInt);
                int episodeIdxInt = -1;
                int.TryParse(episodeIdx, out episodeIdxInt);
                int episodeIdxAbsInt = -1;
                int.TryParse(episodeIdxAbs, out episodeIdxAbsInt);

                string thumb = ImageAllocator.GetSeriesPosterAsFilename(series);
                string fanart = Fanart.getFanart(episode[DBEpisode.cSeriesID]).FanartFilename;
                string episodeFileName = episode[DBEpisode.cFilename];

                _mediaDetail = new BasicMediaDetail();

                _mediaDetail.Title = seriesTitle;

                _mediaDetail.AbsoluteNumbering = absolute;

                _mediaDetail.Season = seasonIdxInt;
                _mediaDetail.Episode = episodeIdxInt;
                _mediaDetail.EpisodeAbs = episodeIdxAbsInt;

                _mediaDetail.Thumb = thumb;
                _mediaDetail.FanArt = fanart;

                _mediaDetail.Files = new List<FileInfo>();
                if (!string.IsNullOrEmpty(episodeFileName))
                    _mediaDetail.Files.Add(new FileInfo(episodeFileName));

                return true;
            }
            catch (Exception e) {
                logger.ErrorException("Unexpected error when pulling data from TVSeries\n", e);
                return false;
            }
        }

        public override int GetEmbeddedSubtitles() {
            int result;
            int.TryParse(episode[DBEpisode.cTextCount], out result);
            if (result == -1) result = 0;
            return result;
        }

        public override bool GetHasSubtitles(bool all) {
            return episode[DBEpisode.cAvailableSubtitles];
        }

        public override void SetHasSubtitles(string fileName, bool value) {
            if (_mediaDetail.Files == null || _mediaDetail.Files.Count == 0) return;

            if (episode[DBEpisode.cFilename] == fileName) {
                episode[DBEpisode.cAvailableSubtitles] = value;
                episode.Commit();
            }
        }

        protected override bool IsAvailable() {
            if (!IsAssemblyAvailable("MP-TVSeries", new Version(2, 6, 0)))
                return false;

            return true;
        }
    }
}
