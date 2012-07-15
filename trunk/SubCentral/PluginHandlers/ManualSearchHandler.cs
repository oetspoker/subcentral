using SubCentral.Localizations;

namespace SubCentral.PluginHandlers {
    internal class ManualSearchHandler : PluginHandler {

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

        public override BasicMediaDetail MediaDetail {
            get { return _mediaDetail; }
            set {
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

        protected override bool GrabFileDetails() {
            return true;
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
