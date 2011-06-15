using System.Collections.Generic;
using System.IO;

namespace SubCentral.Utils {
    public class MediaTags {
        private FileInfo mediaFile = null;
        public List<string> tagsHigh = new List<string>();
        public List<string> tagsLow = new List<string>();
        public string group = string.Empty;

        public string File {
            get {
                string result = string.Empty;
                if (mediaFile == null) return result;
                // TODO MS 
                // - if settings return full, else just name (no extension)
                //Path.Combine(Path.GetDirectoryName(mediaFile), Path.GetFileNameWithoutExtension(file));
                result = Path.GetFileNameWithoutExtension(mediaFile.Name);
                result.Replace("_", "."); // workaround for regexp issue when parsing media tags
                return result;
            }
        }

        public List<string> AllTagsCombined {
            get {
                List<string> result = new List<string>();

                result.AddRange(tagsHigh);
                result.AddRange(tagsLow);
                result.Add(group);

                return result;
            }
        }

        public MediaTags() {
            this.mediaFile = null;
        }

        public MediaTags(FileInfo mediaFile) {
            this.mediaFile = mediaFile;
        }

        public void Clear() {
            mediaFile = null;
            tagsHigh.Clear();
            tagsLow.Clear();
            group = string.Empty;
        }
    }
}
