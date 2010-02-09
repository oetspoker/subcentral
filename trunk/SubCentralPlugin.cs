#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using SubtitleDownloader.Core;

namespace SubCentral
{
    public class SubCentral
    {
        #region private vars

        private static readonly string SubtitlesThumbPath = 
            GUIGraphicsContext.Skin + "\\Media\\Logos\\subtitles.png";
        
        private readonly bool _enabled = false;
        private readonly List<string> _languageCodes;
        private List<string> _providers;
        private Thread _subtitlesDownloaderThread;    

        #endregion 

        #region delegates

        private delegate bool IsCanceledDelegate();
        private delegate bool OnQueryStartingDelegate();
        private delegate void OnProgressDelegate(string line1, string line2, string line3, int percent);
        private delegate bool ShowCustomYesNoDelegate(string heading, string lines, string yesLabel, string noLabel, bool defaultYes);
        private delegate void ShowMessageDelegate(string heading, string line1, string line2, string line3, string line4);
        private delegate void NotifyUserSubtitleDownloadedDelegate(BasicMediaDetail mediaDetail);
        private delegate int PresentUserWithResultsDelegate(BasicMediaDetail mediaDetail, List<SubtitleItem> allResults);

        #endregion

        #region public events

        public delegate void OnSubtitleDownloadErrorDelegate(BasicMediaDetail mediaDetail, string errorMsg);
        public delegate void OnSubtitleDownloadedDelegate (BasicMediaDetail mediaDetail);
        public delegate void OnSubtitleExitDelegate();

        public event OnSubtitleDownloadErrorDelegate OnSubtitleDownloadErrorEvent;
        public event OnSubtitleDownloadedDelegate OnSubtitleDownloadedEvent;
        public event OnSubtitleExitDelegate OnSubtitleExitEvent;

        #endregion
    
        #region public constants

        #endregion

        #region constructors

        public SubCentral(bool enabled, List<string> languageCodes, List<string> providers)
        {
            _providers = providers;
            _enabled = enabled;

            if (languageCodes.Count == 0)
            {
                // Default language is English
                _languageCodes = new List<string>();
                _languageCodes.Add(Languages.GetLanguageCode("English"));
            }
            else
            {
                _languageCodes = languageCodes;
            }
            GUILocalizeStrings.Load(MediaPortal.GUI.Library.GUILocalizeStrings.CurrentLanguage());
        }

        #endregion

        #region public methods

        public static List<string> GetSubtitleDownloaderNames()
        {
            return SubtitleDownloaderFactory.GetSubtitleDownloaderNames();
        }

        public void DownloadSubtitles(List<BasicMediaDetail> mediaDetails)    
        {
            if (_subtitlesDownloaderThread != null && _subtitlesDownloaderThread.IsAlive)
                return;
      
            _subtitlesDownloaderThread = new Thread(DownloadSubtitlesAsynch);
            _subtitlesDownloaderThread.IsBackground = true;
            _subtitlesDownloaderThread.Name = "Subtitles Downloader Thread";
            _subtitlesDownloaderThread.Start(mediaDetails);         
        }

        #endregion

        #region private methods    

        private void DownloadSubtitlesAsynch(object mediaDetailsObj)
        {
            if (!(mediaDetailsObj is List<BasicMediaDetail>))
                throw new ArgumentException("Parameter must be type of List<BasicMediaDetail>!");

            OnQueryStarting();

            List<BasicMediaDetail> mediaDetails = (List<BasicMediaDetail>)mediaDetailsObj;

            Dictionary<string, ISubtitleDownloader> downloaders = HarvestDownloaders();

            //bool isMultipleDiscs = (mediaDetails.Count > 1);

            /*if (isMultipleDiscs && mediaDetail.Number == 1)
      {            
        GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dialog != null)
        {
          dialog.SetHeading(Translation.SubtitlesMultipleDiscs);
          dialog.SetLine(1, Translation.SubtitlesSearchingDisc + " 1-" + Convert.ToString(mediaDetails.Count));
          dialog.SetLine(2, "");
          dialog.DoModal(GUIWindowManager.ActiveWindow);
        }        
      } */

            List<SubtitleItem> allResults = null;

            foreach (BasicMediaDetail mediaDetail in mediaDetails)
            {
                allResults = QueryDownloaders(mediaDetail, downloaders);
            }

            // all results gathered, now report back to the user the list of subs avail,
            // keep showing the menu as long as there is subtitles avail. 
            // and if the user decides to stay on the menu      
            if (allResults != null && allResults.Count > 0)
            {
                while (true)
                {
                    //GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)MediaPortal.GUI.Library.GUIWindow.Window.WINDOW_DIALOG_MENU);
                    int selectedIndex = PresentUserWithResults(mediaDetails[0], allResults);
                    
                    if (selectedIndex < 0)
                    {
                        break; //nothing selected
                    }
                    DownloadSubtitle(mediaDetails, allResults, selectedIndex);
                }
            }
            else
            {
                ShowMessage(GUILocalizeStrings.Get((int)Translation.SubtitlesDownloader), GUILocalizeStrings.Get((int)Translation.NoSubtitlesFound), "", "", "");
            }
        }

        private bool IsCanceled()
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                IsCanceledDelegate d = IsCanceled;
                return (bool)GUIGraphicsContext.form.Invoke(d);
            }

            GUIDialogProgress pDlgProgress =
                (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            return (pDlgProgress.IsCanceled);
        }

        private bool OnQueryStarting()
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                OnQueryStartingDelegate d = OnQueryStarting;
                return (bool)GUIGraphicsContext.form.Invoke(d);
            }

            GUIDialogProgress pDlgProgress =
                (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            pDlgProgress.Reset();
            pDlgProgress.SetHeading(GUILocalizeStrings.Get((int)Translation.SubtitlesDownloader));

            pDlgProgress.SetLine(1, "Initializing");
            pDlgProgress.SetLine(2, string.Empty);
            pDlgProgress.StartModal(GUIWindowManager.ActiveWindow);
            return true;
        }

        private void OnProgress(string line1, string line2, string line3, int percent)
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                OnProgressDelegate d = new OnProgressDelegate(OnProgress);
                GUIGraphicsContext.form.Invoke(d, line1, line2, line3, percent);
                return;
            }

            GUIDialogProgress pDlgProgress =
                (GUIDialogProgress)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_PROGRESS);

            pDlgProgress.ShowProgressBar(true);
            pDlgProgress.SetLine(1, line1);
            pDlgProgress.SetLine(2, line2);
            pDlgProgress.SetLine(3, line3);

            if (percent > 0)
                pDlgProgress.SetPercentage(percent);

            pDlgProgress.Progress();
        }
  
        private void DownloadSubtitle(List<BasicMediaDetail> mediaDetails, List<SubtitleItem> allResults, int selectedIndex)
        {
            ISubtitleDownloader subDownloader = allResults[selectedIndex].Downloader;
            Subtitle subtitle = allResults[selectedIndex].Subtitle;
            
            bool subtitleSaved = false;

            List<FileInfo> subtitleFiles = subDownloader.SaveSubtitle(subtitle);

            int subtitleNr = 0;

            foreach (FileInfo subtitleFile in subtitleFiles)
            {
                // TODO: Commented, is this a bug ???
                //if (mediaDetails.Count <= subtitleNr)
                //    // TODO: Should report error
                //    break; // we found multiple subtitles, but we only have 1 disc.

                BasicMediaDetail mediaDetail = mediaDetails[subtitleNr];
                string videoFileName = mediaDetail.FullPath.ToLower();

                string targetSubtitleFile =
                    SubtitleDownloader.Util.FileUtils.GetFileNameForSubtitle(subtitleFile.Name,
                                                                             subtitle.LanguageCode,
                                                                             videoFileName);
                bool targetFileExists = File.Exists(targetSubtitleFile);

                if (targetFileExists)
                {
                    bool overwriteFile = ShowCustomYesNo(
                        GUILocalizeStrings.Get((int) Translation.SubtitlesDownloader),
                        GUILocalizeStrings.Get((int) Translation.SubtitlesAlreadyExist),
                        null, null, false);

                    if (overwriteFile)
                        File.Delete(targetSubtitleFile);
                }

                File.Move(subtitleFile.FullName, targetSubtitleFile);
                allResults.RemoveAt(selectedIndex);
                subtitleSaved = true;            

                subtitleNr++;
            }

            if (subtitleSaved)
            {
                NotifyUserSubtitleDownloaded(mediaDetails[0]);
            }
        }

        private GUIDialogSelect SetupGUIDialogue(BasicMediaDetail mediaDetail)
        { 
            GUIDialogSelect dlg =
                (GUIDialogSelect)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_SELECT);
            
            if (dlg == null)
                return null;       

            GUIPropertyManager.SetProperty("#selecteditem", mediaDetail.FileName);
            dlg.SetHeading("");
            dlg.EnableButton(true);      
            dlg.SetButtonLabel(2087); // backwards
            return dlg;
        }    

        private void NotifyUserSubtitleDownloaded(BasicMediaDetail mediaDetail)
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                NotifyUserSubtitleDownloadedDelegate d = NotifyUserSubtitleDownloaded;
                GUIGraphicsContext.form.Invoke(d, mediaDetail);
                return;
            }

            GUIDialogNotify pDlgNotify = (GUIDialogNotify)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_NOTIFY);
            
            if (pDlgNotify == null)
                return;

            string caption = mediaDetail.Title;

            if (mediaDetail.Season > 0 && mediaDetail.Episode > 0)
            {
                string episode = "E" + String.Format("{0:00}", mediaDetail.Episode);
                string season = "S" + String.Format("{0:00}", mediaDetail.Season);
                caption += " " + season + episode;
            }

            pDlgNotify.SetHeading(caption);      
            pDlgNotify.SetImage(SubtitlesThumbPath);
            pDlgNotify.SetText(GUILocalizeStrings.Get((int)Translation.SubtitlesDownloaded));
            pDlgNotify.DoModal(GUIWindowManager.ActiveWindow);

            if (OnSubtitleDownloadedEvent!= null)
                OnSubtitleDownloadedEvent(mediaDetail);        
        }    

        private int PresentUserWithResults(BasicMediaDetail mediaDetail, List<SubtitleItem> allResults)
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                PresentUserWithResultsDelegate d = PresentUserWithResults;
                return (int) GUIGraphicsContext.form.Invoke(d, mediaDetail, allResults);        
            }

            const int selectedIndex = -1;

            GUIDialogSelect dlg = SetupGUIDialogue(mediaDetail);        
            
            if (dlg == null)
                return selectedIndex;   

            bool subsFound = false;         

            foreach (SubtitleItem subItem in allResults)
            {
                dlg.Add(subItem.Subtitle.LanguageCode + " " + subItem.Subtitle.FileName);
                subsFound = true;
            }

            if (!subsFound)
                return selectedIndex;         

            dlg.DoModal(GUIWindowManager.ActiveWindow);

            if (dlg.SelectedLabel < 0)
            {
                if (OnSubtitleExitEvent != null)
                {
                    OnSubtitleExitEvent();
                }
                return selectedIndex;
            }

            return dlg.SelectedLabel;
        }

        private List<SubtitleItem> QueryDownloaders(BasicMediaDetail mediaDetail, Dictionary<string, ISubtitleDownloader> downloaders)
        {    
            List<SubtitleItem> allResults = new List<SubtitleItem>();
            EpisodeSearchQuery episodeQuery = null;
            SearchQuery movieQuery = null;
            ImdbSearchQuery queryIMDB = null;

            bool useImdbMovieQuery = !(string.IsNullOrEmpty((mediaDetail.ImdbID)));
            bool useEpisodeQuery = (mediaDetail.Episode > 0 && mediaDetail.Season > 0);

            if (useEpisodeQuery)
            {        
                episodeQuery = new EpisodeSearchQuery(mediaDetail.Title, mediaDetail.Season, mediaDetail.Episode);
                episodeQuery.LanguageCodes = _languageCodes.ToArray();
            }
            else
            {              
                movieQuery = new SearchQuery(mediaDetail.Title);
                movieQuery.Year = mediaDetail.Year;
                movieQuery.LanguageCodes = _languageCodes.ToArray();

                if (useImdbMovieQuery)
                {
                    queryIMDB = new ImdbSearchQuery(mediaDetail.ImdbID);
                    queryIMDB.LanguageCodes = _languageCodes.ToArray();
                }        
            }
                  
            int providerCount = 1;

            foreach (KeyValuePair<string, ISubtitleDownloader> kvp in downloaders)
            {
                if (IsCanceled())
                    break;

                ISubtitleDownloader subsDownloader = kvp.Value;
                string providerName = kvp.Key;

                List<Subtitle> resultsFromDownloader = null;        
                int percent = (100 / downloaders.Count) * providerCount;
                
                OnProgress(GUILocalizeStrings.Get((int)Translation.QueryingSubtitlesProvider) 
                    + " (" + Convert.ToString(providerCount) + " / " 
                    + Convert.ToString(downloaders.Count) + ") :", providerName, 
                    GUILocalizeStrings.Get((int)Translation.SubtitlesFound) + ": " 
                    + Convert.ToString(allResults.Count), percent);

                if (useEpisodeQuery)
                {
                    try
                    {            
                        resultsFromDownloader = subsDownloader.SearchSubtitles(episodeQuery);
                    }
                    catch (Exception e)
                    {
                        if (OnSubtitleDownloadErrorEvent != null)
                        {
                            OnSubtitleDownloadErrorEvent(mediaDetail, e.Message);
                        }
                    }
                }
                else if (useImdbMovieQuery)
                {
                    try
                    {        
                        resultsFromDownloader = subsDownloader.SearchSubtitles(queryIMDB);
                    }
                    catch (NotSupportedException)
                    {
                        try
                        {          
                            resultsFromDownloader = subsDownloader.SearchSubtitles(movieQuery);
                        }
                        catch (Exception e1)
                        {
                            if (OnSubtitleDownloadErrorEvent != null)
                            {
                                OnSubtitleDownloadErrorEvent(mediaDetail, e1.Message);
                            }              
                        }
                    }
                    catch (Exception e2)
                    {
                        if (OnSubtitleDownloadErrorEvent != null)
                        {
                            OnSubtitleDownloadErrorEvent(mediaDetail, e2.Message);
                        }                          
                    }
                }
                else
                {          
                    resultsFromDownloader = subsDownloader.SearchSubtitles(movieQuery);
                }

                if (resultsFromDownloader != null && resultsFromDownloader.Count > 0)
                {
                    foreach (Subtitle subtitle in resultsFromDownloader)
                    {
                        SubtitleItem subItem = new SubtitleItem();
                        subItem.Downloader = subsDownloader;
                        subItem.Subtitle = subtitle;
                        allResults.Add(subItem);
                    }
                }
                providerCount++;
            }

            //lets sort the results by lang. prefs.
            //allResults.Sort(new SubtitleComparer(_languageCodes));
      
            return allResults;
        }

        private Dictionary<string, ISubtitleDownloader> HarvestDownloaders()
        {
            Dictionary<string, ISubtitleDownloader> downloaders = new Dictionary<string, ISubtitleDownloader>();
            
            foreach (string provider in _providers)
            {
                if (!string.IsNullOrEmpty(provider))
                {
                    ISubtitleDownloader downloader = SubtitleDownloaderFactory.GetSubtitleDownloader(provider);
                    downloaders.Add(provider, downloader);
                }
            }
            return downloaders;
        }

        /// <summary>
        /// Displays a yes/no dialog with custom labels for the buttons
        /// This method may become obsolete in the future if media portal adds more dialogs
        /// </summary>
        /// <returns>True if yes was clicked, False if no was clicked</returns>
        private bool ShowCustomYesNo(string heading, string lines, string yesLabel, string noLabel, bool defaultYes)
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                ShowCustomYesNoDelegate d = ShowCustomYesNo;
                return (bool)GUIGraphicsContext.form.Invoke(d, heading, lines, yesLabel, noLabel, defaultYes);
            }

            GUIDialogYesNo dialog = (GUIDialogYesNo)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_YES_NO);
            try
            {
                dialog.Reset();
                dialog.SetHeading(heading);
                string[] linesArray = lines.Split(new string[] { "\\n" }, StringSplitOptions.None);
                if (linesArray.Length > 0) dialog.SetLine(1, linesArray[0]);
                if (linesArray.Length > 1) dialog.SetLine(2, linesArray[1]);
                if (linesArray.Length > 2) dialog.SetLine(3, linesArray[2]);
                if (linesArray.Length > 3) dialog.SetLine(4, linesArray[3]);
                dialog.SetDefaultToYes(defaultYes);

                foreach (System.Windows.UIElement item in dialog.Children)
                {
                    if (item is GUIButtonControl)
                    {
                        GUIButtonControl btn = (GUIButtonControl)item;
                        if (btn.GetID == 11 && !String.IsNullOrEmpty(yesLabel)) // Yes button
                            btn.Label = yesLabel;
                        else if (btn.GetID == 10 && !String.IsNullOrEmpty(noLabel)) // No button
                            btn.Label = noLabel;
                    }
                }
                dialog.DoModal(GUIWindowManager.ActiveWindow);
                return dialog.IsConfirmed;
            }
            finally
            {
                // set the standard yes/no dialog back to it's original state (yes/no buttons)
                if (dialog != null)
                {
                    dialog.ClearAll();
                }
            }
        }

        private void ShowMessage(string heading, string line1, string line2, string line3, string line4)
        {
            if (GUIGraphicsContext.form.InvokeRequired)
            {
                ShowMessageDelegate d = new ShowMessageDelegate(ShowMessage);
                GUIGraphicsContext.form.Invoke(d, heading, line1, line2, line3, line4);
                return;
            }

            GUIDialogOK dialog = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
            dialog.Reset();
            dialog.SetHeading(heading);
            if (line1 != null) dialog.SetLine(1, line1);
            if (line2 != null) dialog.SetLine(2, line2);
            if (line3 != null) dialog.SetLine(3, line3);
            if (line4 != null) dialog.SetLine(4, line4);
            dialog.DoModal(GUIWindowManager.ActiveWindow);
        }

        #endregion

        public struct BasicMediaDetail
        {
            public int? MediaId { get; set; }
            public string ImdbID { get; set; }
            public string FullPath { get; set; }
            public string FileName { get; set; }
            public string Title { get; set; }
            public int Number { get; set; }
            public int Year { get; set; }
            public int Season { get; set; }
            public int Episode { get; set; }
        }

        public struct SubtitleItem
        {
            public Subtitle Subtitle { get; set; }
            public ISubtitleDownloader Downloader { get; set; }
        }

        enum Translation
        {
            SubtitlesAlreadyExist = 0,
            NoSubtitlesFound = 1,
            SubtitlesDownloaded = 2,
            SubtitlesDownloader = 3,
            QueryingSubtitlesProvider = 4,
            SubtitlesFound = 5
        }
    }
}