using SubCentral.Utils;

namespace SubCentral.Settings.Data
{
    public enum OnDownloadFileName {
        [StringValue("Use default")]
        UseDefault = 0,

        [StringValue("Always ask")]
        AlwaysAsk = 1,

        [StringValue("Ask if no media file info is found (fe. manual search)")]
        AskIfManual = 2
    }
}