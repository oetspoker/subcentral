using NLog;

namespace SubCentral.PluginHandlers {
    internal class TemporaryCustomPluginHandler : PluginHandler {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override int ID {
            get { return _ID; }
            set { _ID = value; }
        }
        private int _ID = -1;

        public override string PluginName {
            get { return _pluginName; }
            set { _pluginName = value; } 
        }
        private string _pluginName = "";

        // The video file details we want to grab subtitles for.
        public override BasicMediaDetail MediaDetail {
            get { return _mediaDetail; }
            set { _mediaDetail = value; }
        }
        private BasicMediaDetail _mediaDetail;

        public override bool Modified {
            get { return false; }
            set { }
        }

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
            return PluginHandlerType.CUSTOM;
        }

        public override void Clear() {
            _pluginName = "";
            _ID = -1;
            _mediaDetail = new BasicMediaDetail();
        }

        protected override bool IsAvailable() {
            return true;
        }
    }
}
