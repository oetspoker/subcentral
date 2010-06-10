using SubCentral.Utils;

namespace SubCentral.Settings.Data {
    public enum OnPluginLoadWithSearchData {
        [StringValue("Do nothing")]
        DoNothing = 0,

        [StringValue("Search default providers")]
        SearchDefaults = 1
    }
}