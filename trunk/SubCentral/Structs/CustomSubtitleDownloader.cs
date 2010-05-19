
namespace SubCentral.Structs {
    public struct CustomSubtitleDownloader {
        public SubtitleDownloader.Core.ISubtitleDownloader downloader { get; set; }
        public string downloaderTitle { get; set; }
    }
}
