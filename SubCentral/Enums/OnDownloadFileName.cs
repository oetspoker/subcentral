using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubCentral.Enums {
    public enum OnDownloadFileName {
        [StringValue("Use default")]
        UseDefault = 0,

        [StringValue("Always ask")]
        AlwaysAsk = 1,

        [StringValue("Ask if no media file info is found (fe. manual search)")]
        AskIfManual = 2
    }
}
