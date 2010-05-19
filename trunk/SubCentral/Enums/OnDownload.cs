using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubCentral.Enums {
    public enum OnDownload {
        [StringValue("Always ask")]
        AlwaysAsk = 0,

        [StringValue("Try to use default folders")]
        DefaultFolders = 1
    }
}
