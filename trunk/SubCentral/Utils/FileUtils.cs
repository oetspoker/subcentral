using System;
using System.Collections.Generic;
using System.IO;
using NLog;

namespace SubCentral.Utils {
    public sealed class FileUtils {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly object syncRoot = new object();

        private static Dictionary<string, DriveInfo> driveInfoPool;

        public static bool IsUncPath(FileSystemInfo fsi) {
            if (fsi == null || string.IsNullOrEmpty(fsi.FullName)) return false;
            Uri uri = new Uri(fsi.FullName);
            return uri.IsUnc;
        }

        public static bool IsReparsePoint(FileSystemInfo fsi) {
            if (IsUncPath(fsi))
                return false;

            try {
                if ((fsi.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                    return true;
            }
            // ignore the exceptions that can occur
            catch (ArgumentException) { }
            catch (System.Security.SecurityException) { }
            catch (IOException) { }

            return false;
        }

        public static bool IsAccessible(DirectoryInfo di) {
            if (di.Exists) {

                if (!IsReparsePoint(di))
                    return true;

                try {
                    di.GetDirectories();
                    // directory access successful, directory is available
                    return true;
                }
                // ignore the exception, failure means it is not available 
                catch (DirectoryNotFoundException) { }
            }
            return false;
        }

        public static bool mediaIsAvailable(List<FileInfo> files)
        {
            if (files == null || files.Count == 0) return false;

            foreach (FileInfo fi in files)
            {
                if (fi.Exists) return true;
            }

            return false;
        }

        public static bool pathExists(string path, out bool hostAlive, out bool pathDriveReady) {
            hostAlive = true;
            pathDriveReady = true;

            if (string.IsNullOrEmpty(path)) return false;

            if (!Path.IsPathRooted(path)) return true;

            if (!NetUtils.uncHostIsAlive(path)) {
                logger.Debug("UNC host not alive: path {0}", path);
                hostAlive = false;
                return false;
            }

            if (!pathDriveIsReady(path)) {
                pathDriveReady = false;
                return false;
            }

            return Directory.Exists(path);
        }

        public static bool pathIsWritable(string path) {
            if (string.IsNullOrEmpty(path)) return false;

            if (!Path.IsPathRooted(path)) return true;

            string fileName = string.Concat(path, Path.DirectorySeparatorChar, Path.GetRandomFileName());
            FileInfo fileInfo = new FileInfo(fileName);

            FileStream stream = null;
            try {
                stream = fileInfo.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
            catch {
                return false;
            }
            finally {
                if (stream != null) {
                    try {
                        stream.Close();
                        fileInfo.Delete();
                    }
                    catch {
                    }
                }
            }
            return true;
        }

        public static bool pathDriveIsDVD(string path) {
            if (string.IsNullOrEmpty(path) || new Uri(path).IsUnc) return false;

            DriveInfo di = GetDriveInfoFromPath(path);

            if (di != null && di.DriveType == DriveType.CDRom) return true;

            return false;
        }

        public static bool pathDriveIsReady(string path) {
            if (string.IsNullOrEmpty(path) || new Uri(path).IsUnc) return true;

            DriveInfo di = GetDriveInfoFromPath(path);

            if (di != null && di.DriveType == DriveType.Network) return true;

            if (di == null || !di.IsReady) return false;

            return true;
        }

        public static bool pathIsDrive(string path) {
            if (string.IsNullOrEmpty(path)) return false;

            path = ensureBackSlash(path);
            if (path.Length == 3 && path.EndsWith(@":\")) return true;
            return false;
        }

        public static int uncPathDepth(string path) {
            if (string.IsNullOrEmpty(path) || !new Uri(path).IsUnc) return 0;

            path = ensureBackSlash(path);
            path = path.Substring(0, path.Length - 1);

            string[] check = path.Substring(2).Split(new char[] { Path.DirectorySeparatorChar });
            return check.Length;
        }

        public static DriveInfo GetDriveInfoFromFileInfo(FileInfo fileInfo) {
            string driveletter = FileInfoToDriveLetter(fileInfo);
            return GetDriveInfo(driveletter);
        }

        public static DriveInfo GetDriveInfoFromPath(string path) {
            string driveletter = PathToDriveLetter(path);
            return GetDriveInfo(driveletter);
        }

        public static string FileInfoToDriveLetter(FileInfo fileInfo) {
            return PathToDriveLetter(fileInfo.FullName);
        }

        public static string PathToDriveLetter(string path) {
            Uri uri = new Uri(path);

            // if the path is UNC return null
            if (uri.IsUnc)
                return null;

            // return the first 2 characters
            if (path.Length > 1)
                return path.Substring(0, 2).ToUpper();
            else // or if only a letter was given add colon
                return path.ToUpper() + ":";
        }

        public static DriveInfo GetDriveInfo(string drive) {
            if (drive == null)
                return null;

            lock (syncRoot) {
                // if this is the first request create the driveinfo collection cache
                if (driveInfoPool == null)
                    driveInfoPool = new Dictionary<string, DriveInfo>();

                if (!driveInfoPool.ContainsKey(drive)) {
                    try {
                        driveInfoPool.Add(drive, new DriveInfo(drive));
                    }
                    catch (Exception e) {
                        logger.ErrorException(string.Format("Error retrieving DriveInfo object for '{0}'{1}", drive, Environment.NewLine), e);
                        return null;
                    }
                }
            }
            return driveInfoPool[drive];
        }

        public static bool fileNameIsValid(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return false;

            foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars()) {
                if (fileName.Contains(lDisallowed.ToString()))
                    return false;
            }

            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars()) {
                if (fileName.Contains(lDisallowed.ToString()))
                    return false;
            }

            return true;
        }

        public static bool pathNameIsValid(string path) {
            if (string.IsNullOrEmpty(path)) return false;

            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars()) {
                if (path.Contains(lDisallowed.ToString()))
                    return false;
            }

            return true;
        }

        public static string fixInvalidFileName(string fileName) {
            string result = fileName;

            if (string.IsNullOrEmpty(fileName)) return result;

            foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars()) {
                if (fileName.Contains(lDisallowed.ToString()))
                    result = result.Replace(lDisallowed, '_');
            }
            
            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars()) {
                if (fileName.Contains(lDisallowed.ToString()))
                    result = result.Replace(lDisallowed, '_');
            }

            return result;
        }

        public static string fixInvalidPathName(string path) {
            string result = path;

            if (string.IsNullOrEmpty(path)) return result;

            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars()) {
                if (path.Contains(lDisallowed.ToString()))
                    result = result.Replace(lDisallowed, '_');
            }
            
            return result;
        }

        public static string ensureBackSlash(string path) {
            if (string.IsNullOrEmpty(path)) return null;

            if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path;
            else
                return path + Path.DirectorySeparatorChar.ToString();
        }

        public static string ResolveRelativePath(string relativePath, string referencePath) {
            if (string.IsNullOrEmpty(referencePath)) {
                throw new ArgumentNullException("basePath");
            }

            if (string.IsNullOrEmpty(relativePath)) {
                throw new ArgumentNullException("relativePath");
            }

            var result = referencePath;

            if (Path.IsPathRooted(relativePath)) {
                return relativePath;
            }

            if (relativePath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                relativePath = relativePath.Substring(0, relativePath.Length - 1);
            }

            if (relativePath == ".") {
                return referencePath;
            }

            if (relativePath.StartsWith(@".\")) {
                relativePath = relativePath.Substring(2);
            }

            relativePath = relativePath.Replace(@"\.\", @"\");
            if (!relativePath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                relativePath = relativePath + Path.DirectorySeparatorChar.ToString();
            }

            while (!string.IsNullOrEmpty(relativePath)) {
                int lengthOfOperation = relativePath.IndexOf(Path.DirectorySeparatorChar.ToString()) + 1;
                var operation = relativePath.Substring(0, lengthOfOperation - 1);
                relativePath = relativePath.Remove(0, lengthOfOperation);

                if (operation == @"..") {
                    Uri uri = new Uri(Path.Combine(result, operation));
                    if (uri.IsUnc && Path.GetDirectoryName(result) == null) {
                        result = uri.LocalPath;
                    }
                    else {
                        result = Path.GetDirectoryName(result);
                    }
                }
                else {
                    result = Path.Combine(result, operation);
                }

                if (result == null) return result;
            }

            if (uncPathDepth(result) == 1)
                return null;
            return result;
        }
    }
}
