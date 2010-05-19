
namespace SubCentral.Structs {
    public struct SubtitleItem {
        public SubtitleDownloader.Core.Subtitle Subtitle { get; set; }
        public SubtitleDownloader.Core.ISubtitleDownloader Downloader { get; set; }
        public string ProviderTitle { get; set; }
        public string LanguageName { get; set; }
    }
}
