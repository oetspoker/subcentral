
using SubCentral.Enums;

namespace SubCentral.Settings.Data
{
    public enum OnDownload {
        [StringValue("Always ask")]
        AlwaysAsk = 0,

        [StringValue("Try to use default folders")]
        DefaultFolders = 1
    }
}