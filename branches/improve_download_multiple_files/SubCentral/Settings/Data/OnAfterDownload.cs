using SubCentral.Utils;

namespace SubCentral.Settings.Data {
    public enum OnAfterDownload {
        [StringValue("Do nothing")]
        DoNothing = 0,

        [StringValue("Go back to originating plugin")]
        BackToOriginalPlugin = 1
    }
}