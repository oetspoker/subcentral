using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using NLog;
using SubCentral.PluginHandlers;

namespace SubCentral.GUI {
    public class SubCentralGUI : GUIWindow {
        public enum ViewMode {
            MAIN,
            SEARCH
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SubCentralCore core = SubCentralCore.Instance;

        private PluginHandler currentHandler = null;

        #region GUI Controls
        
        [SkinControl(200)]
        protected GUIButtonControl searchButton = null;

        [SkinControl(201)]
        protected GUIButtonControl cancelButton = null;
        
        #endregion

        #region Dummy GUI Controls

        [SkinControl(100)]
        protected GUILabelControl mainViewIndicator = null;

        [SkinControl(101)]
        protected GUILabelControl fileViewIndicator = null;
        
        #endregion

        public ViewMode View {
            get {
                return _viewMode;
            }
            set {
                _viewMode = value;
                if (mainViewIndicator != null) mainViewIndicator.Visible = (_viewMode == ViewMode.MAIN);
                if (fileViewIndicator != null) fileViewIndicator.Visible = (_viewMode == ViewMode.SEARCH);
            }
        }
        private ViewMode _viewMode;
        
        #region GUIWindow Methods

        public override int GetID {
            get {
                return 84623;
            }
        }

        public override bool Init() {
            base.Init();
            core.Initialize();
            logger.Info("Initializing GUI");

            // check if we can load the skin
            bool success = Load(GUIGraphicsContext.Skin + @"\subcentral.xml");

            return success;
        }

        public override void DeInit() {
            base.DeInit();

        }

        protected override void OnPageLoad() {
            base.OnPageLoad();

            // if the component didn't load properly we probably have a bad skin file
            if (searchButton == null) {
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("Error");
                dialog.SetLine(1, "Could not load the skin");
                dialog.SetLine(2, "file for SubCentral!");
                dialog.DoModal(GetID);

                GUIWindowManager.ShowPreviousWindow();
                logger.Error("Failed to load all components from skin file. Skin is outdated or invalid.");
                return;
            }

            // get last active module  
            int lastActiveWindow = GUIWindowManager.GetPreviousActiveWindow();

            // just entering from the home screen, show the main screen
            if (lastActiveWindow == 0) {
                logger.Debug("Entered plugin from Home Screen");
                View = ViewMode.MAIN;
                currentHandler = null;
            }
            
            // entering from a plugin
            if (lastActiveWindow != 0) {
                //try to load data from it and if we fail return back to the calling plugin.
                if (!LoadFromPlugin(lastActiveWindow)) {
                    logger.Error("Entered plugin from ID #{0}, but failed to load an appropriate handler.", lastActiveWindow);
                    GUIWindowManager.ShowPreviousWindow();
                    return;
                }

                // we loaded details successfully, we are now in file view.
                else {
                    logger.Debug("Entered plugin from {0} ({1})", currentHandler.PluginName, currentHandler.ID);
                    View = ViewMode.SEARCH;
                    PublishSearchProperties();
                }
            }
        }

        protected override void OnPageDestroy(int new_windowId) {
            base.OnPageDestroy(new_windowId);
        }

        protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType) {
            base.OnClicked(controlId, control, actionType);
            
            switch (controlId) {
                case 200: // searchButton
                    break;

                case 201: // cancel button
                    GUIWindowManager.ShowPreviousWindow();
                    break;
            }
        }

        public override void OnAction(MediaPortal.GUI.Library.Action action) {
            switch (action.wID) {
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PARENT_DIR:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_HOME:
                case MediaPortal.GUI.Library.Action.ActionType.ACTION_PREVIOUS_MENU:
                    GUIWindowManager.ShowPreviousWindow();
                    break;
                default:
                    base.OnAction(action);
                    break;
            }
        }

        public override bool OnMessage(GUIMessage message) {
            return base.OnMessage(message);
        }

        #endregion

        private bool LoadFromPlugin(int pluginID) {
            bool success = false;            
            
            // try to grab our handler
            currentHandler = core.PluginHandlers[pluginID];
            if (currentHandler != null) 
                success = currentHandler.Update();

            // if we failed, show a popup notifying the user
            if (!success) {
                View = ViewMode.MAIN;
                GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
                dialog.Reset();
                dialog.SetHeading("Error");
                dialog.SetLine(1, "Unable to import data");
                dialog.SetLine(2, string.Format("from {0}!", currentHandler == null ? "external plugin" : currentHandler.PluginName));
                dialog.DoModal(GetID);
            }

            return success;
        }

        private void PublishSearchProperties() {
            if (currentHandler != null) {
                SetProperty("#subcentral.search.file.name", currentHandler.File.Name);
                SetProperty("#subcentral.search.file.path", currentHandler.File.FullName);
                SetProperty("#subcentral.search.file.description", currentHandler.Description);
                SetProperty("#subcentral.search.file.source.name", currentHandler.PluginName);
                SetProperty("#subcentral.search.file.source.id", currentHandler.ID.ToString());
            }
        }

        private void SetProperty(string property, string value) {
            if (String.IsNullOrEmpty(value))
                GUIPropertyManager.SetProperty(property, " ");
            else
                GUIPropertyManager.SetProperty(property, value);

            logger.Debug("{0}: \"{1}\"", property, value);
        }

    }
}
