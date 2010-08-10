using System;
using System.Collections.Generic;
using MediaPortal.GUI.Library;
using SubCentral.Settings.Data;

namespace SubCentral.GUI {
    public class GUIListItemSubtitleComparer : IComparer<GUIListItem> {
        private SubtitlesSortMethod _sortMethod;
        private bool _sortAsc;

        public GUIListItemSubtitleComparer(SubtitlesSortMethod sortMethod, bool ascending) {
            _sortMethod = sortMethod;
            _sortAsc = ascending;
        }

        public int Compare(GUIListItem item1, GUIListItem item2) {
            if (item1 == item2) return 0;
            if (item1 == null) return -1;
            if (item2 == null) return -1;
            if (item1.IsFolder && item1.Label == "..") return -1;
            if (item2.IsFolder && item2.Label == "..") return -1;
            if (item1.IsFolder && !item2.IsFolder) return -1;
            else if (!item1.IsFolder && item2.IsFolder) return 1;

            SubtitlesSortDetails subtitlesSortDetails1;
            SubtitlesSortDetails subtitlesSortDetails2;
            try {
                subtitlesSortDetails1 = (SubtitlesSortDetails)item1.AlbumInfoTag;
                subtitlesSortDetails2 = (SubtitlesSortDetails)item2.AlbumInfoTag;
            }
            catch {
                return 0;
            }

            switch (_sortMethod) {
                case SubtitlesSortMethod.DefaultNoSort:
                    if (_sortAsc) {
                        if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return 1;
                        if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return -1;
                    }
                    else {
                        if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return -1;
                        if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return 1;
                    }
                    return 0;

                case SubtitlesSortMethod.MediaTags:
                    if (_sortAsc) {
                        if (subtitlesSortDetails1.TagRank == subtitlesSortDetails2.TagRank) {
                            if (subtitlesSortDetails1.LanguagePriority == subtitlesSortDetails2.LanguagePriority) {
                                if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return 1;
                                if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return -1;
                            }
                            else {
                                if (subtitlesSortDetails1.LanguagePriority > subtitlesSortDetails2.LanguagePriority) return 1;
                                if (subtitlesSortDetails1.LanguagePriority < subtitlesSortDetails2.LanguagePriority) return -1;
                            }
                        }
                        else {
                            // different than others since higher rank means higher priority - lower index
                            if (subtitlesSortDetails1.TagRank > subtitlesSortDetails2.TagRank) return -1; 
                            if (subtitlesSortDetails1.TagRank < subtitlesSortDetails2.TagRank) return 1;
                        }
                    }
                    else {
                        if (subtitlesSortDetails1.TagRank == subtitlesSortDetails2.TagRank) {
                            if (subtitlesSortDetails1.LanguagePriority == subtitlesSortDetails2.LanguagePriority) {
                                if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return -1;
                                if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return 1;
                            }
                            else {
                                if (subtitlesSortDetails1.LanguagePriority > subtitlesSortDetails2.LanguagePriority) return -1;
                                if (subtitlesSortDetails1.LanguagePriority < subtitlesSortDetails2.LanguagePriority) return 1;
                            }
                        }
                        else {
                            // different than others since higher rank means higher priority - lower index
                            if (subtitlesSortDetails1.TagRank > subtitlesSortDetails2.TagRank) return 1;
                            if (subtitlesSortDetails1.TagRank < subtitlesSortDetails2.TagRank) return -1;
                        }
                    }
                    return 0;

                case SubtitlesSortMethod.SubtitleLanguage:
                    if (_sortAsc) {
                        if (subtitlesSortDetails1.LanguagePriority == subtitlesSortDetails2.LanguagePriority) {
                            if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return 1;
                            if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return -1;
                        }
                        else {
                            if (subtitlesSortDetails1.LanguagePriority > subtitlesSortDetails2.LanguagePriority) return 1;
                            if (subtitlesSortDetails1.LanguagePriority < subtitlesSortDetails2.LanguagePriority) return -1;
                        }
                    }
                    else {
                        if (subtitlesSortDetails1.LanguagePriority == subtitlesSortDetails2.LanguagePriority) {
                            if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return -1;
                            if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return 1;
                        }
                        else {
                            if (subtitlesSortDetails1.LanguagePriority > subtitlesSortDetails2.LanguagePriority) return -1;
                            if (subtitlesSortDetails1.LanguagePriority < subtitlesSortDetails2.LanguagePriority) return 1;
                        }
                    }
                    return 0;

                case SubtitlesSortMethod.SubtitleName:
                    if (_sortAsc) {
                        if (subtitlesSortDetails1.Name == subtitlesSortDetails2.Name) {
                            if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return 1;
                            if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return -1;
                        }
                        else {
                            return string.Compare(subtitlesSortDetails1.Name, subtitlesSortDetails2.Name, true);
                        }
                    }
                    else {
                        if (subtitlesSortDetails1.Name == subtitlesSortDetails2.Name) {
                            if (subtitlesSortDetails1.ListPosition > subtitlesSortDetails2.ListPosition) return -1;
                            if (subtitlesSortDetails1.ListPosition < subtitlesSortDetails2.ListPosition) return 1;
                        }
                        else {
                            return string.Compare(subtitlesSortDetails2.Name, subtitlesSortDetails1.Name, true);
                        }
                    }
                    return 0;
            }
            return 0;
        }
    }
}

