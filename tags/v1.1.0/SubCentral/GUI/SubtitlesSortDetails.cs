using System;

namespace SubCentral.GUI {
    public struct SubtitlesSortDetails {
        public int LanguagePriority { get; set; }
        public int ListPosition { get; set; }
        private double tagRank;
        public double TagRank {
            get {
                return Math.Round(tagRank, 4);
            }
            set {
                tagRank = value;
            }
        }
        public string Name { get; set; }
        public string Provider { get; set; }
        public string Language { get; set; }
    }
}