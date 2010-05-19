using System.IO;
using System.Collections.Generic;

namespace SubCentral.Structs {
    public struct SubtitleDownloadStatus {
        public bool Succesful { get; set; }
        public bool Canceled { get; set; }
        public string Error { get; set; }
    }
}
