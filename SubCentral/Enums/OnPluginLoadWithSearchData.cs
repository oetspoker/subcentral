using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubCentral.Enums {
    public enum OnPluginLoadWithSearchData {
        [StringValue("Do nothing")]
        DoNothing = 0,

        [StringValue("Search default providers")]
        SearchDefaults = 1
    }
}
