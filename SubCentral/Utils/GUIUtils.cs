using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using SubCentral.Enums;
using SubCentral.Structs;
using NLog;

namespace SubCentral.Utils {
    public static class GUIUtils {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private delegate bool ShowCustomYesNoDialogDelegate(string heading, string lines, string yesLabel, string noLabel, bool defaultYes);
        private delegate void ShowOKDialogDelegate(string heading, string lines);
        private delegate List<MultiSelectionItem> ShowMultiSelectionDialogDelegate(string heading, List<MultiSelectionItem> items);
        private delegate void ShowNotifyDialogDelegate(string heading, string text, string image);
        private delegate int ShowMenuDialogDelegate(string heading, List<GUIListItem> items);

        public static readonly string SubtitlesLogoThumbPath = GUIGraphicsContext.Skin + "\\Media\\Logos\\subtitles.png";
        public static readonly string NoSubtitlesLogoThumbPath = GUIGraphicsContext.Skin + "\\Media\\Logos\\nosubtitles.png";

        public static void SetProperty(string property, string value) {
            SetProperty(property, value, false);
        }

        public static void SetProperty(string property, string value, bool log) {
            if (string.IsNullOrEmpty(value))
                value = " ";

            GUIPropertyManager.SetProperty(property, value);

            if (log) {
                if (GUIPropertyManager.Changed)
                    logger.Debug("Set property \"" + property + "\" to \"" + value + "\" successful");
                else
                    logger.Warn("Set property \"" + property + "\" to \"" + value + "\" failed");
            }
        }

        /// <summary>
        /// Displays a yes/no dialog.
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        public static bool ShowYesNoDialog(string heading, string lines) {
            return ShowCustomYesNoDialog(heading, lines, null, null, false);
        }

        /// <summary>
        /// Displays a yes/no dialog.
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        public static bool ShowYesNoDialog(string heading, string lines, bool defaultYes) {
            return ShowCustomYesNoDialog(heading, lines, null, null, defaultYes);
        }

        /// <summary>
        /// Displays a yes/no dialog with custom labels for the buttons.
        /// This method may become obsolete in the future if media portal adds more dialogs.
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        public static bool ShowCustomYesNoDialog(string heading, string lines, string yesLabel, string noLabel) {
            return ShowCustomYesNoDialog(heading, lines, yesLabel, noLabel, false);
        }

        /// <summary>
        /// Displays a yes/no dialog with custom labels for the buttons.
        /// This method may become obsolete in the future if media portal adds more dialogs.
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        public static bool ShowCustomYesNoDialog(string heading, string lines, string yesLabel, string noLabel, bool defaultYes) {
            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowCustomYesNoDialogDelegate d = ShowCustomYesNoDialog;
                return (bool)GUIGraphicsContext.form.Invoke(d, heading, lines, yesLabel, noLabel, defaultYes);
            }

            GUIDialogYesNo dlgYesNo = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            //if (dlgYesNo == null) return;

            try {
                dlgYesNo.Reset();
                dlgYesNo.SetHeading(heading);
                string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);
                if (linesArray.Length > 0) dlgYesNo.SetLine(1, linesArray[0]);
                if (linesArray.Length > 1) dlgYesNo.SetLine(2, linesArray[1]);
                if (linesArray.Length > 2) dlgYesNo.SetLine(3, linesArray[2]);
                if (linesArray.Length > 3) dlgYesNo.SetLine(4, linesArray[3]);
                dlgYesNo.SetDefaultToYes(defaultYes);

                foreach (System.Windows.UIElement item in dlgYesNo.Children) {
                    if (item is GUIButtonControl) {
                        GUIButtonControl btn = (GUIButtonControl)item;
                        if (btn.GetID == 11 && !String.IsNullOrEmpty(yesLabel)) // Yes button
                            btn.Label = yesLabel;
                        else if (btn.GetID == 10 && !String.IsNullOrEmpty(noLabel)) // No button
                            btn.Label = noLabel;
                    }
                }
                dlgYesNo.DoModal(GUIWindowManager.ActiveWindow);
                return dlgYesNo.IsConfirmed;
            }
            finally {
                // set the standard yes/no dialog back to it's original state (yes/no buttons)
                if (dlgYesNo != null) {
                    dlgYesNo.ClearAll();
                }
            }
        }

        /// <summary>
        /// Displays a OK dialog with heading and up to 4 lines.
        /// </summary>
        public static void ShowOKDialog(string heading, string line1, string line2, string line3, string line4) {
            ShowOKDialog(heading, string.Concat(line1, line2, line3, line4));
        }

        /// <summary>
        /// Displays a OK dialog with heading and up to 4 lines split by \n in lines string.
        /// </summary>
        public static void ShowOKDialog(string heading, string lines) {
            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowOKDialogDelegate d = ShowOKDialog;
                GUIGraphicsContext.form.Invoke(d, heading, lines);
                return;
            }

            GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            //if (dlgOK == null) return;

            dlgOK.Reset();

            dlgOK.SetHeading(heading);

            int lineid = 1;
            foreach (string line in lines.Split(new string[] { "\\n" }, StringSplitOptions.None)) {
                dlgOK.SetLine(lineid, line);
                lineid++;
            }
            for (int i = lineid; i <= 4; i++)
                dlgOK.SetLine(i, "");

            dlgOK.DoModal(GUIWindowManager.ActiveWindow);
        }

        /// <summary>
        /// Displays a notification dialog.
        /// </summary>
        public static void ShowNotifyDialog(string heading, string text) {
            ShowNotifyDialog(heading, text, string.Empty);
        }

        /// <summary>
        /// Displays a notification dialog.
        /// </summary>
        public static void ShowNotifyDialog(string heading, string text, string image) {
            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowNotifyDialogDelegate d = ShowNotifyDialog;
                GUIGraphicsContext.form.Invoke(d, heading, text, image);
                return;
            }

            GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            //if (pDlgNotify == null) return;

            pDlgNotify.SetHeading(heading);

            pDlgNotify.SetImage(image);

            pDlgNotify.SetText(text);

            pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);
        }

        /// <summary>
        /// Displays a multi selection dialog.
        /// </summary>
        /// <returns>List of items</returns>
        public static List<MultiSelectionItem> ShowMultiSelectionDialog(string heading, List<MultiSelectionItem> items) {
            List<MultiSelectionItem> result = new List<MultiSelectionItem>();

            if (items == null) return result;

            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowMultiSelectionDialogDelegate d = ShowMultiSelectionDialog;
                return (List<MultiSelectionItem>)GUIGraphicsContext.form.Invoke(d, heading, items);
            }

            GUIDialogMultiSelect dlgMultiSelect = (GUIDialogMultiSelect)GUIWindowManager.GetWindow(2100);
            //if (dlgMultiSelect == null) return;

            dlgMultiSelect.Reset();

            dlgMultiSelect.SetHeading(heading);

            foreach (MultiSelectionItem multiSelectionItem in items) {
                GUIListItem item = new GUIListItem();
                item.Label = multiSelectionItem.ItemTitle;
                item.Label2 = multiSelectionItem.ItemTitle2;
                item.MusicTag = multiSelectionItem.Tag;
                item.Selected = multiSelectionItem.Selected;

                dlgMultiSelect.Add(item);
            }

            dlgMultiSelect.DoModal(GUIWindowManager.ActiveWindow);

            if (dlgMultiSelect.DialogModalResult == ModalResult.OK) {
                for (int i = 0; i < items.Count; i++) {
                    MultiSelectionItem item = items[i];
                    MultiSelectionItem newMultiSelectionItem = new MultiSelectionItem();
                    newMultiSelectionItem.ItemTitle = item.ItemTitle;
                    newMultiSelectionItem.ItemTitle2 = item.ItemTitle2;
                    newMultiSelectionItem.ItemID = item.ItemID;
                    newMultiSelectionItem.Tag = item.Tag;
                    try {
                        newMultiSelectionItem.Selected = dlgMultiSelect.ListItems[i].Selected;
                    }
                    catch {
                        newMultiSelectionItem.Selected = item.Selected;
                    }
                    
                    result.Add(newMultiSelectionItem);
                }
            }
            else
                return null;

            return result;
        }

        /// <summary>
        /// Displays a menu dialog from list of items
        /// </summary>
        /// <returns>Selected item index, -1 if exited</returns>
        public static int ShowMenuDialog(string heading, List<GUIListItem> items) {
            return ShowMenuDialog(heading, items, -1);
        }

        /// <summary>
        /// Displays a menu dialog from list of items
        /// </summary>
        /// <returns>Selected item index, -1 if exited</returns>
        public static int ShowMenuDialog(string heading, List<GUIListItem> items, int selectedItemIndex) {
            if (GUIGraphicsContext.form.InvokeRequired) {
                ShowMenuDialogDelegate d = ShowMenuDialog;
                return (int)GUIGraphicsContext.form.Invoke(d, heading, items);
            }

            //GUIDialogSelect2 dlgSelect =
            //  (GUIDialogSelect2)GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_DIALOG_SELECT2);

            GUIDialogMenu dlgMenu = (GUIDialogMenu)GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_DIALOG_MENU);
            //if (dlgMenu == null) return -1;

            dlgMenu.Reset();

            dlgMenu.SetHeading(heading);
            //dlgSelect.EnableButton(true);
            //dlgSelect.SetButtonLabel(2087); // backwards

            foreach (GUIListItem item in items) {
                dlgMenu.Add(item);
            }

            if (selectedItemIndex >= 0)
                dlgMenu.SelectedLabel = selectedItemIndex;

            dlgMenu.DoModal(GUIWindowManager.ActiveWindow);

            if (dlgMenu.SelectedLabel < 0) {
                return -1;
            }

            return dlgMenu.SelectedLabel;
        }

        /// <summary>
        /// Displays a menu dialog from FolderSelectionItem items
        /// </summary>
        /// <returns>Selected item index, -1 if exited</returns>
        public static int ShowFolderMenuDialog(string heading, List<FolderSelectionItem> items, int selectedItemIndex) {
            List<GUIListItem> listItems = new List<GUIListItem>();

            bool selectedItemSet = false;
            int index = 0;
            foreach (FolderSelectionItem folderSelectionItem in items) {
                GUIListItem listItem = new GUIListItem();
                listItem.Label = folderSelectionItem.FolderName;
                if (folderSelectionItem.WasRelative) {
                    listItem.Label2 = "(" + folderSelectionItem.OriginalFolderName + ")";
                }

                switch (folderSelectionItem.FolderErrorInfo) {
                    case FolderErrorInfo.NonExistant:
                        listItem.IsPlayed = true;
                        break;
                    case FolderErrorInfo.ReadOnly:
                        listItem.IsRemote = true;
                        break;
                    case FolderErrorInfo.OK:
                        if (!selectedItemSet && selectedItemIndex < 0) {
                            selectedItemIndex = index;
                            selectedItemSet = true;
                        }
                        break;
                }

                listItem.MusicTag = folderSelectionItem;

                if (!selectedItemSet && selectedItemIndex >= 0 && index == selectedItemIndex) {
                    selectedItemIndex = index;
                    selectedItemSet = true;

                }

                listItems.Add(listItem);
                index++;
            }

            return ShowMenuDialog(heading, listItems, selectedItemIndex);
        }

        /// <summary>
        /// Gets the input from the virtual keyboard window.
        /// </summary>
        public static bool GetStringFromKeyboard(ref string strLine) {
            VirtualKeyboard keyboard = (VirtualKeyboard)GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
            if (keyboard == null) return false;

            keyboard.Reset();
            keyboard.Text = strLine;
            keyboard.DoModal(GUIWindowManager.ActiveWindow);

            if (keyboard.IsConfirmed) {
                strLine = keyboard.Text;
                return true;
            }

            return false;
        }
    }
}
