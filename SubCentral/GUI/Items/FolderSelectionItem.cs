using System.Collections.Generic;

namespace SubCentral.GUI.Items {
    public struct FolderSelectionItem {
        public string ItemID { get; set; }
        public string FolderName1 { get; set; }
        // list of folder names count matching video files
        public List<string> FolderNames { get; set; }
        public string OriginalFolderName { get; set; }
        public bool WasRelative { get; set; }
        public FolderErrorInfo FolderErrorInfo { get; set; }
        private Dictionary<string, FolderErrorInfo> folderErrorInfos;
        public Dictionary<string, FolderErrorInfo> FolderErrorInfos { 
            get {
                if (folderErrorInfos == null) {
                    folderErrorInfos = new Dictionary<string, FolderErrorInfo>();
                }
                return folderErrorInfos;
            }
        }
        //public bool Existing { get; set; }
        //public bool Writable { get; set; }
        public object Tag { get; set; }
        public bool DefaultForMovies { get; set; }
        public bool DefaultForTVShows { get; set; }
    }
}