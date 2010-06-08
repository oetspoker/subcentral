namespace SubCentral.GUI.Items
{
    public struct FolderSelectionItem {
        public string ItemID { get; set; }
        public string FolderName { get; set; }
        public string FolderName2 { get; set; }
        public string OriginalFolderName { get; set; }
        public bool WasRelative { get; set; }
        public FolderErrorInfo FolderErrorInfo { get; set; }
        //public bool Existing { get; set; }
        //public bool Writable { get; set; }
        public object Tag { get; set; }
        public bool DefaultForMovies { get; set; }
        public bool DefaultForTVShows { get; set; }
    }
}