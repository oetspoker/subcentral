using System;
using System.IO;
using System.Collections.Generic;
using SubCentral.Utils;

namespace SubCentral.PluginHandlers {
    public struct BasicMediaDetail {
        public int? MediaId { get; set; }

        public string ImdbIDStr {
            get {
                return "tt" + ImdbID;
            }
        }
        public string ImdbID {
            get {
                return _imdbID;
            }
            set {
                if (!string.IsNullOrEmpty(value) && value != "tt") {
                    _imdbID = value.Replace("tt", "").PadLeft(7, '0');
                    if (!SubCentralUtils.isImdbIdCorrect("tt" + _imdbID)) {
                        _imdbID = string.Empty;
                    }
                }
                else {
                    _imdbID = string.Empty;
                }
            }
        }
        private string _imdbID;

        public string Title { get; set; }

        public string YearStr {
            get {
                if (SubCentralUtils.isYearCorrect(Year.ToString())) {
                    return Year.ToString();
                }
                return string.Empty;
            }
        }
        public int Year { get; set; }

        public string SeasonStr {
            get {
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(SeasonProper.ToString(), false)) {
                    return string.Format("{0:00}", SeasonProper);
                }
                return string.Empty;
            }
        }
        public int Season { get; set; }

        public string EpisodeStr {
            get {
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(EpisodeProper.ToString(), AbsoluteNumbering)) {
                    return string.Format("{0:00}", EpisodeProper);
                }
                return string.Empty;
            }
        }
        public int Episode { get; set; }
        public int EpisodeAbs { get; set; }

        public int SeasonProper {
            get {
                if (AbsoluteNumbering) return 0;
                else return Season;
            }
        }

        public int EpisodeProper {
            get {
                if (AbsoluteNumbering) return EpisodeAbs;
                else return Episode;
            }
            set {
                if (AbsoluteNumbering) EpisodeAbs = value;
                else Episode = value;
            }
        }

        public bool AbsoluteNumbering { get; set; }

        public string Thumb { get; set; }

        public string FanArt { get; set; }

        public List<FileInfo> Files { get; set; }
    }
}