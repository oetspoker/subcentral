using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NLog;
using SubCentral.Utils;
using SubCentral.PluginHandlers;

namespace SubCentral.Utils {
    public class TagRank {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private const string regexpTags = @"(?:(?:[\(\{\[]|\b)(?:(?:576|720|1080)[pi](?:\d{2})?|dir(?:ector[']?s[\s\.])?cut|dvd[-]?(?:[r59]|rip|scr(?:eener)?)|(?:bd|sat|dvb)[-]?(?:rip|scr(?:eener)?)|(?:avc)?hd|wmv|ntsc|pal|mpeg|dsr|r[1-5]|bd[59]|dts|ac3|blu[-]?ray|[hp]dtv|stv|hd[-]?dvd|xvid|divx|x264|dxva|remux|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|RETA[Ii]L|EXTENDED|REMASTERED|UNRATED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[Ii]NTERNAL|[DS]UBBED|SCREENER|TELE(?:CINE|SYNC)|L[Ii]NE|OAR|AVC|[\[\(\{\s\.]T[CS][\]\)\}\s\.])(?:[\]\)\}]|\b)()?)";
        private const string regexpTagsGroup = @"(?:(?:[\(\{\[]|\b)(?:(?:576|720|1080)[pi](?:\d{2})?|dir(?:ector[']?s[\s\.])?cut|dvd[-]?(?:[r59]|rip|scr(?:eener)?)|(?:bd|sat|dvb)[-]?(?:rip|scr(?:eener)?)|(?:avc)?hd|wmv|ntsc|pal|mpeg|dsr|r[1-5]|bd[59]|dts|ac3|blu[-]?ray|[hp]dtv|stv|hd[-]?dvd|xvid|divx|x264|dxva|remux|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|RETA[Ii]L|EXTENDED|REMASTERED|UNRATED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[Ii]NTERNAL|[DS]UBBED|SCREENER|TELE(?:CINE|SYNC)|L[Ii]NE|OAR|AVC|[\[\(\{\s\.]T[CS][\]\)\}\s\.])(?:[\]\)\}]|\b)(?<group>-[^\s]+$)?)";

        private const string regexpTagsHigh = @"(?:(?:[\(\{\[]|\b)(?:(?:576|720|1080)[pi](?:\d{2})?|dvd[-]?(?:[r59]|rip|scr(?:eener)?)|(?:bd|sat|dvb)[-]?(?:rip|scr(?:eener)?)|dsr|r[1-5]|bd[59]|dts|ac3|blu[-]?ray|[hp]dtv|hd[-]?dvd|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|UNRATED|[Ii]NTERNAL|SCREENER|TELE(?:CINE|SYNC)|[\[\(\{\s\.]T[CS][\]\)\}\s\.])(?:[\]\)\}]|\b)()?)";
        private const string regexpTagsHighGroup = @"(?:(?:[\(\{\[]|\b)(?:(?:576|720|1080)[pi](?:\d{2})?|dvd[-]?(?:[r59]|rip|scr(?:eener)?)|(?:bd|sat|dvb)[-]?(?:rip|scr(?:eener)?)|dsr|r[1-5]|bd[59]|dts|ac3|blu[-]?ray|[hp]dtv|hd[-]?dvd|[WF]S|PROPER|REPACK|RER[Ii]P|REAL|UNRATED|[Ii]NTERNAL|SCREENER|TELE(?:CINE|SYNC)|[\[\(\{\s\.]T[CS][\]\)\}\s\.])(?:[\]\)\}]|\b)(?<group>-[^\s]+$)?)";

        private const string regexpTagsLow = @"(?:(?:[\(\{\[]|\b)(?:dir(?:ector[']?s[\s\.])?cut|(?:avc)?hd|wmv|ntsc|pal|mpeg|dts|ac3|stv|hd[-]?dvd|xvid|divx|x264|dxva|remux|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|RETA[Ii]L|EXTENDED|REMASTERED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[DS]UBBED|L[Ii]NE|OAR|AVC|)(?:[\]\)\}]|\b)()?)";
        private const string regexpTagsLowGroup = @"(?:(?:[\(\{\[]|\b)(?:dir(?:ector[']?s[\s\.])?cut|(?:avc)?hd|wmv|ntsc|pal|mpeg|dts|ac3|stv|hd[-]?dvd|xvid|divx|x264|dxva|remux|(?-i)FEST[Ii]VAL|L[iI]M[iI]TED|RETA[Ii]L|EXTENDED|REMASTERED|CHRONO|THEATR[Ii]CAL|DC|SE|UNCUT|[DS]UBBED|L[Ii]NE|OAR|AVC|)(?:[\]\)\}]|\b)(?<group>-[^\s]+$)?)";

        // TODO MS: commented is improved RegEx for MP-TVSeries
        //private const string regexpSeries = @"^.*?\\?(?<series>[^\\$]+?)[ .-]+(?:[s]?(?<season>\d+)[ .-]?[ex](?<episode>\d+)|(?:\#|\-\s)(?<season>\d+)\.(?<episode>\d+))(?:(?:[ .-]+[s]?\k<season>[ .-]?[ex](?<episode2>\d+)|(?:\#|\-\s)\k<season>\.(?<episode2>\d+))|(?:[ .-]?[ex+-]+(?<episode2>\d+)))*[ .-]*(?<title>(?![^\\]*?sample[ .-])[^$]*?)\.(?<ext>[^.]*)$";
        private const string regexpSeries = @"^.*?\\?(?<series>[^\\$]+?)[ .-]+(?:[s]?(?<season>\d+)[ .-]?[ex](?<episode>\d+)|(?:\#|\-\s)(?<season>\d+)\.(?<episode>\d+))(?:(?:[ .-]+[s]?\k<season>[ .-]?[ex](?<episode2>\d+)|(?:\#|\-\s)\k<season>\.(?<episode2>\d+))|(?:[ .-]?[ex+-]+(?<episode2>\d+)))*[ .-]*(?<other>[^$]*?)$";

        private BasicMediaDetail mediaDetail;
        private List<FileInfo> mediaFiles = new List<FileInfo>();

        private MediaTags mediaTagsFile = new MediaTags();

        private int firstPercentage = 70;
        private int secondPercentage = 20;
        private int thirdPercentage = 10;

        // TODO MS
        // - Check for most recent regular expression

        public BasicMediaDetail MediaDetail {
            get {
                return mediaDetail;
            }
            set {
                mediaDetail = value;
                MediaFiles = mediaDetail.Files;
            }
        }

        public List<FileInfo> MediaFiles {
            get {
                return mediaFiles;
            }
            set {
                mediaFiles = value;
                FillTagsFromFiles();
                FillPercentages();
            }
        }

        public MediaTags MediaTags {
            get {
                return mediaTagsFile;
            }
        }

        public TagRank(BasicMediaDetail mediaDetail) {
            MediaDetail = mediaDetail;
        }

        private void FillTagsFromFiles() {
            mediaTagsFile.Clear();

            if (MediaFiles == null || MediaFiles.Count < 1) return;

            Dictionary<FileInfo, MediaTags> tagsByFile = new Dictionary<FileInfo, MediaTags>();

            // get tags for all files, file by file
            foreach (FileInfo fiMedia in MediaFiles) {
                tagsByFile.Add(fiMedia, GetTagsForFile(fiMedia));
            }

            // now we have the tags for all files.. we need to find all the common tags for all files
            Dictionary<string, int> tagsByCount = new Dictionary<string, int>();

            tagsByCount.Clear();
            foreach (KeyValuePair<FileInfo, MediaTags> kvp in tagsByFile) {
                foreach(string tag in kvp.Value.tagsHigh) {
                    if (string.IsNullOrEmpty(tag)) continue;

                    if (tagsByCount.ContainsKey(tag))
                        tagsByCount[tag]++;
                    else
                        tagsByCount[tag] = 1;
                }
            }
            foreach (KeyValuePair<string, int> kvp in tagsByCount) {
                // do we have the tag in all files?
                if (kvp.Value == MediaFiles.Count) {
                    mediaTagsFile.tagsHigh.Add(kvp.Key);
                }
            }

            tagsByCount.Clear();
            foreach (KeyValuePair<FileInfo, MediaTags> kvp in tagsByFile) {
                foreach (string tag in kvp.Value.tagsLow) {
                    if (string.IsNullOrEmpty(tag)) continue;

                    if (tagsByCount.ContainsKey(tag))
                        tagsByCount[tag]++;
                    else
                        tagsByCount[tag] = 1;
                }
            }
            foreach (KeyValuePair<string, int> kvp in tagsByCount) {
                // do we have the tag in all files?
                if (kvp.Value == MediaFiles.Count) {
                    mediaTagsFile.tagsLow.Add(kvp.Key);
                }
            }

            string group = null;
            foreach (KeyValuePair<FileInfo, MediaTags> kvp in tagsByFile) {
                if (group == null) {
                    group = kvp.Value.group;
                    continue;
                }

                // if one file misses or has different group, skip the group alltogether
                if (kvp.Value.group != group) {
                    group = string.Empty;
                    break;
                }
            }
            if (group != null)
                mediaTagsFile.group = group;
        }

        private MediaTags GetTagsForFile(FileInfo mediaFile) {
            MediaTags result = new MediaTags(mediaFile);

            if (mediaFile == null) return result;

            result.tagsHigh.AddRange(GetTagsHighForFile(result.File));
            result.tagsLow.AddRange(GetTagsLowForFile(result.File));
            result.group = GetGroupForFile(result.File);

            return result;
        }

        private List<string> GetTagsHighForFile(string file) {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(file)) return result;

            Regex rExHigh = new Regex(regexpTagsHigh, RegexOptions.IgnoreCase);

            MatchCollection matchRegEx = rExHigh.Matches(file);

            foreach (Match m in matchRegEx) {
                foreach (Group g in m.Groups) {
                    if (!string.IsNullOrEmpty(g.Value)) {
                        string groupValue = SubCentralUtils.TrimNonAlphaNumeric(g.Value);
                        if (!string.IsNullOrEmpty(groupValue))
                            result.Add(groupValue.ToLowerInvariant());
                    }
                }
            }

            return result;
        }

        private List<string> GetTagsLowForFile(string file) {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(file)) return result;

            Regex rExLow = new Regex(regexpTagsLow, RegexOptions.IgnoreCase);

            MatchCollection matchRegEx = rExLow.Matches(file);

            foreach (Match m in matchRegEx) {
                foreach (Group g in m.Groups) {
                    if (!string.IsNullOrEmpty(g.Value)) {
                        string groupValue = SubCentralUtils.TrimNonAlphaNumeric(g.Value);
                        if (!string.IsNullOrEmpty(groupValue))
                            result.Add(groupValue.ToLowerInvariant());
                    }
                }
            }

            return result;
        }

        private string GetGroupForFile(string file) {
            string result = string.Empty;
            
            if (string.IsNullOrEmpty(file))return result;
            
            Regex rExGroup = new Regex(regexpTagsGroup, RegexOptions.IgnoreCase);

            MatchCollection matchRegEx = rExGroup.Matches(file);
            
            foreach (Match m in matchRegEx) {
                result = m.Groups["group"].ToString();
                if (result.StartsWith("-"))
                    result = result.TrimStart(new char[] { '-' });
            }

            return result.ToLowerInvariant();
        }

        private void FillPercentages() {
            if (mediaTagsFile.tagsHigh.Count < 1) {
                firstPercentage = 0;
                if ((mediaTagsFile.tagsLow.Count > 0) && (!string.IsNullOrEmpty(mediaTagsFile.group))) {
                    secondPercentage = 50;
                    thirdPercentage = 50;
                }
                else if ((mediaTagsFile.tagsLow.Count > 0) && (string.IsNullOrEmpty(mediaTagsFile.group))) {
                    secondPercentage = 100;
                    thirdPercentage = 0;
                }
                else if ((mediaTagsFile.tagsLow.Count < 1) && (!string.IsNullOrEmpty(mediaTagsFile.group))) {
                    secondPercentage = 0;
                    thirdPercentage = 100;
                }
                else {
                    secondPercentage = 0;
                    thirdPercentage = 0;
                }
            }
            else if (mediaTagsFile.tagsLow.Count < 1) {
                secondPercentage = 0;
                if ((mediaTagsFile.tagsHigh.Count > 0) && (!string.IsNullOrEmpty(mediaTagsFile.group))) {
                    firstPercentage = 80;
                    thirdPercentage = 20;
                }
                else if ((mediaTagsFile.tagsHigh.Count > 0) && (string.IsNullOrEmpty(mediaTagsFile.group))) {
                    firstPercentage = 100;
                    thirdPercentage = 0;
                }
                else if ((mediaTagsFile.tagsHigh.Count < 1) && (!string.IsNullOrEmpty(mediaTagsFile.group))) {
                    firstPercentage = 0;
                    thirdPercentage = 100;
                }
                else {
                    firstPercentage = 0;
                    thirdPercentage = 0;
                }
            }
            else if (string.IsNullOrEmpty(mediaTagsFile.group)) {
                thirdPercentage = 0;
                if ((mediaTagsFile.tagsHigh.Count > 0) && (mediaTagsFile.tagsLow.Count > 0)) {
                    firstPercentage = 75;
                    secondPercentage = 25;
                }
                else if ((mediaTagsFile.tagsHigh.Count > 0) && (mediaTagsFile.tagsLow.Count < 1)) {
                    firstPercentage = 100;
                    secondPercentage = 0;
                }
                else if ((mediaTagsFile.tagsHigh.Count < 1) && (mediaTagsFile.tagsLow.Count > 0)) {
                    firstPercentage = 0;
                    secondPercentage = 100;
                }
                else {
                    firstPercentage = 0;
                    secondPercentage = 0;
                }
            }
        }

        private void GetCounts(List<string> list1, List<string> list2, out int same, out int added, out int missing) {
            same = 0;
            added = 0;
            missing = 0;

            foreach (string listString in list1) {
                foreach (string list2String in list2) {
                    if (string.Compare(listString, list2String, StringComparison.CurrentCultureIgnoreCase) == 0) {
                        same++;
                        break;
                    }
                }
            }

            foreach (string listString in list2) {
                bool found = false;
                foreach (string list1String in list1) {
                    if (string.Compare(listString, list1String, StringComparison.CurrentCultureIgnoreCase) == 0) {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    added++;
            }

            missing = list1.Count - same;
        }

        private double GetRank(MediaTags fileTags, MediaTags subtitleTags) {
            double result = 0.0;

            int sameHight, addedHigh, missingHigh;
            GetCounts(fileTags.tagsHigh, subtitleTags.tagsHigh, out sameHight, out addedHigh, out missingHigh);

            int sameLow, addedLow, missingLow;
            GetCounts(fileTags.tagsLow, subtitleTags.tagsLow, out sameLow, out addedLow, out missingLow);

            if (firstPercentage == 0 && secondPercentage == 0 && thirdPercentage == 0)
                return result; // nothing to do

            double highPoints = ((double)sameHight / (double)fileTags.tagsHigh.Count) * (double)firstPercentage;
            double lowPoints = ((double)sameLow / (double)fileTags.tagsLow.Count) * (double)secondPercentage;
            double groupPoints = 0.0;
            if (string.IsNullOrEmpty(subtitleTags.group) && fileTags.group.Length > 2 && subtitleTags.File != null)
                groupPoints = (subtitleTags.File.Contains(fileTags.group, StringComparison.CurrentCultureIgnoreCase) ? 1 : 0) * (double)thirdPercentage;
            else
                groupPoints = ((fileTags.group == subtitleTags.group) ? 1 : 0) * (double)thirdPercentage;
            highPoints = highPoints.CorrectNaN();
            lowPoints = lowPoints.CorrectNaN();

            result = highPoints + lowPoints + groupPoints;

            double highMissingPoints = 0.0;
            double lowMissingPoints = 0.0;
            if (subtitleTags.tagsHigh.Count > 0)
                highMissingPoints = (((double)missingHigh / (double)fileTags.tagsHigh.Count) * (double)firstPercentage) * 0.75;
            if (subtitleTags.tagsLow.Count > 0)
                lowMissingPoints = (((double)missingLow / (double)fileTags.tagsLow.Count) * (double)secondPercentage) * 0.75;
            highMissingPoints = highMissingPoints.CorrectNaN();
            lowMissingPoints = lowMissingPoints.CorrectNaN();

            result = result - highMissingPoints - lowMissingPoints;

            double highAddedPoints = (((double)addedHigh / (double)fileTags.tagsHigh.Count) * (double)firstPercentage) * 0.75;
            double lowAddedPoints = (((double)addedLow / (double)fileTags.tagsLow.Count) * (double)secondPercentage) * 0.75;
            highAddedPoints = highAddedPoints.CorrectNaN();
            lowAddedPoints = lowAddedPoints.CorrectNaN();

            result = result - highAddedPoints - lowAddedPoints;

            return result;
        }

        private bool SubtitleFileNameMatchesTVShowMedia(string subtitleFile) {
            bool result = false;

            SubCentralUtils.EnsureProperSubtitleFile(ref subtitleFile);

            if (string.IsNullOrEmpty(subtitleFile) || string.IsNullOrEmpty(MediaDetail.SeasonStr) || string.IsNullOrEmpty(MediaDetail.EpisodeStr)) return result;

            Regex rExSeries = new Regex(regexpSeries, RegexOptions.IgnoreCase);

            Match matchResults = rExSeries.Match(subtitleFile);

            if (matchResults.Success) {
                Group seasonGroup = matchResults.Groups["season"];
                Group episodeGroup = matchResults.Groups["episode"];
                Group episode2Group = matchResults.Groups["episode2"];
                if (seasonGroup == null || episodeGroup == null)
                    return result;

                string seasonGroupValue = seasonGroup.Value;
                string episodeGroupValue = episodeGroup.Value;
                string episode2GroupValue = episode2Group != null ? episode2Group.Value : string.Empty;
                if (string.IsNullOrEmpty(seasonGroupValue) || string.IsNullOrEmpty(episodeGroupValue))
                    return result;

                seasonGroupValue = seasonGroupValue.Trim();
                episodeGroupValue = episodeGroupValue.Trim();
                episode2GroupValue = episode2GroupValue.Trim();
                if (string.IsNullOrEmpty(seasonGroupValue) || string.IsNullOrEmpty(episodeGroupValue))
                    return result;

                try {
                    int mediaSeason = int.Parse(MediaDetail.SeasonStr);
                    int mediaEpisode = int.Parse(MediaDetail.EpisodeStr);

                    int subtitleSeason = int.Parse(seasonGroupValue);
                    int subtitleEpisode = int.Parse(episodeGroupValue);
                    int subtitleEpisode2 = 0;
                    if (!string.IsNullOrEmpty(episode2GroupValue)) {
                        subtitleEpisode2 = int.Parse(episode2GroupValue);
                        if (!SubCentralUtils.isSeasonOrEpisodeCorrect(subtitleEpisode2.ToString()))
                            subtitleEpisode2 = 0;
                    }

                    if (!SubCentralUtils.isSeasonOrEpisodeCorrect(subtitleSeason.ToString()))
                        return result;
                    if (!SubCentralUtils.isSeasonOrEpisodeCorrect(subtitleEpisode.ToString()))
                        return result;

                    if (subtitleEpisode2 != 0 && subtitleEpisode2 <= subtitleEpisode)
                        subtitleEpisode2 = 0;

                    if (mediaSeason != subtitleSeason)
                        return result;
                    if (subtitleEpisode2 != 0) {
                        if (mediaEpisode > subtitleEpisode2 || mediaEpisode < subtitleEpisode)
                            return result;
                    }
                    else {
                        if (mediaEpisode != subtitleEpisode)
                            return result;
                    }

                    result = true;

                }
                catch {
                    return result;
                }
            }
            return result;
        }

        public bool SubtitleFileNameMatchesMedia(string subtitleFile) {
            bool result = false;

            SubCentralUtils.EnsureProperSubtitleFile(ref subtitleFile);

            if (string.IsNullOrEmpty(subtitleFile) || MediaFiles == null || MediaFiles.Count < 1) return result;

            foreach (FileInfo fi in MediaFiles) {
                string subtitleFileProper = Path.GetFileNameWithoutExtension(subtitleFile).ToLowerInvariant();
                subtitleFileProper = Regex.Replace(subtitleFileProper, @"[-_\.]", " ");
                string fileNameProper = Path.GetFileNameWithoutExtension(fi.Name).ToLowerInvariant();
                fileNameProper = Regex.Replace(fileNameProper, @"[-_\.]", " ");

                if (subtitleFileProper == fileNameProper || subtitleFileProper.Contains(fileNameProper)) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public double GetSubtitleFileRank(string subtitleFile) {
            double result = 0.0;

            if (SubtitleFileNameMatchesMedia(subtitleFile))
                result = 1000.0;
            else if (SubtitleFileNameMatchesTVShowMedia(subtitleFile))
                result = 500.0;

            // TODO MS remove logging before final
            logger.Debug(string.Format("Calculating media tag rank for subtitle file {0} ...", subtitleFile));

            SubCentralUtils.EnsureProperSubtitleFile(ref subtitleFile); // default extension to maintain compatibility with MediaTags class

            if (string.IsNullOrEmpty(subtitleFile)) return result;

            MediaTags mediaTagsSubtitleFile = GetTagsForFile(new FileInfo(subtitleFile));

            result += GetRank(mediaTagsFile, mediaTagsSubtitleFile);

            // TODO MS remove logging before final
            logger.Debug(string.Format("... has media tag rank of {0}", result));

            return result;
        }
    }
}
