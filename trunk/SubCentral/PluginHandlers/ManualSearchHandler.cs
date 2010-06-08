using NLog;
using SubCentral.Localizations;

namespace SubCentral.PluginHandlers {
    internal class ManualSearchHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override int ID {
            get { return _ID; }
            set { _ID = value; }
        }
        private int _ID = -2;

        public override string PluginName {
            get { return _pluginName; }
            set { _pluginName = value; }
        }
        private string _pluginName = Localization.ManualSearch;

        // The video file details we want to grab subtitles for.
        public override BasicMediaDetail MediaDetail {
            get { return _mediaDetail; }
            set {
                //if (_mediaDetail.ImdbID != value.ImdbID)
                //    _isModified = true;
                //else if (_mediaDetail.Title != value.Title)
                //    _isModified = true;
                //else if (_mediaDetail.Year != value.Year)
                //    _isModified = true;
                //else if (_mediaDetail.Season != value.Season)
                //    _isModified = true;
                //else if (_mediaDetail.Episode != value.Episode)
                //    _isModified = true;
                //else if (_mediaDetail.Thumb != value.Thumb)
                //    _isModified = true;
                //else if (_mediaDetail.FanArt != value.FanArt)
                //    _isModified = true;
                //else if (_mediaDetail.Files == null && value.Files != null)
                //    _isModified = true;
                //else if (_mediaDetail.Files != null && value.Files == null)
                //    _isModified = true;
                //else if (_mediaDetail.Files != null && value.Files != null && _mediaDetail.Files.Count != value.Files.Count)
                //    _isModified = true;
                //else if (_mediaDetail.Files != null && value.Files != null && _mediaDetail.Files.Count != value.Files.Count)
                //    _isModified = true;
                //else if (_mediaDetail.Files != null && value.Files != null && _mediaDetail.Files.Count == value.Files.Count) {
                //    for (int i = 0; i < _mediaDetail.Files.Count; i++) {
                //        FileInfo fileInfo1 = _mediaDetail.Files[i];
                //        FileInfo fileInfo2 = value.Files[i];
                //        if (fileInfo2.FullName != fileInfo2.FullName) {
                //            _isModified = true;
                //            break;
                //        }
                //    }
                //}

                _mediaDetail = value;
                _isModified = true;
            }
        }
        private BasicMediaDetail _mediaDetail;

        public override bool Modified {
            get { return _isModified; }
            set { _isModified = value; }
        }
        private bool _isModified = false;

        // retrieves info from Moving Pictures
        protected override bool GrabFileDetails() {

            return true;

            //try {
            //    // retrieve stuff - fille media details
            //}
            //catch (Exception) {
            //    return false;
            //}
        }

        protected override PluginHandlerType GetPluginHandlerType() {
            return PluginHandlerType.MANUAL;
        }

        public override void Clear() {
            _pluginName = Localization.ManualSearch;
            _ID = -2;
            _mediaDetail = new BasicMediaDetail();
            _isModified = false;
        }

        protected override bool IsAvailable() {
            return true;
        }
    }
}
