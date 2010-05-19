using System;
using System.IO;
using System.Collections.Generic;
using SubCentral.Utils;

namespace SubCentral.Structs {
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
                if (!string.IsNullOrEmpty(value)) {
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
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(Season.ToString())) {
                    return String.Format("{0:00}", Season);
                }
                return string.Empty;
            }
        }
        public int Season { get; set; }

        public string EpisodeStr {
            get {
                if (SubCentralUtils.isSeasonOrEpisodeCorrect(Episode.ToString())) {
                    return String.Format("{0:00}", Episode);
                }
                return string.Empty;
            }
        }
        public int Episode { get; set; }
        
        public string Thumb { get; set; }
        
        public string FanArt { get; set; }
        
        public List<FileInfo> Files { get; set; }
    }
}
