using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace uSource.Formats.Extensions
{
    public static class PathExtension
    {
        public static string PathSeparator = "/";
        static Regex SlashRegex = new Regex(@"([\\/]|(\.+/)|(\.+\\))+", RegexOptions.ExplicitCapture);

        static string m_projectPath = string.Empty;
        public static string ProjectPath
        {
            get
            {
                if (m_projectPath.Length == 0)
                    m_projectPath = NormalizeSlashes(Assembly.GetExecutingAssembly().Location.NormalizeSlashes().GetPath());

                return m_projectPath;
            }
        }

        public static string GetPath(this string filePath)
        {
            return filePath.Remove(filePath.LastIndexOf(PathSeparator) + 1);
        }

        public static string[] GetSubPaths(this string filePath, bool setLastAsEmpty = false)
        {
            string[] subPaths = SlashRegex.Split(filePath);
            if (setLastAsEmpty) subPaths[subPaths.Length - 1] = string.Empty;

            return subPaths;
        }

        public static string GetFileName(this string filePath)
        {
            string[] subPaths = GetSubPaths(filePath);
            return subPaths[subPaths.Length - 1];
        }

        /// <summary>
        /// Get extension file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ext"></param>
        /// <returns>File path without extension</returns>
        public static string GetFileExtension(this string filePath, out string ext, char separator = '\\')
        {
            ext = string.Empty;
            int fileExtPos = filePath.LastIndexOf('.');
            if (fileExtPos > filePath.LastIndexOf(separator))
            {
                int extSize = filePath.Length - fileExtPos;
                ext = filePath.Substring(fileExtPos, extSize);
                if (ext.Length >= 1) filePath = filePath.Remove(fileExtPos, extSize);
                ext = ext.Length > 1 ? ext : string.Empty;
            }

            return filePath;
        }

        public static string NormalizeSlashes(this string input, bool toLower = false)
        {
            return SlashRegex.Replace(toLower ? input.ToLower() : input, PathSeparator);
        }

        /// <summary>
        /// Normalize paths from a painful format "././\materials\models//folder///file.vmt" to "materials/models/folder/file.vmt"
        /// </summary>
        /// <param name="filePath">Input painful path</param>
        /// <param name="ext">Output normalized path</param>
        /// <param name="subFolder">Needed to normalize sub folder in path (ex: materials)</param>
        /// <returns>Normalized path</returns>
        public static string NormalizePath(this string filePath, out string ext, string subFolder)
        {
            ext = string.Empty;
            if (string.IsNullOrEmpty(filePath))
                return ext;

            string[] subPaths = filePath.ToLower().GetSubPaths();

            filePath = string.Empty;
            int subpathLast = subPaths.Length - 1;
            for (int subID = 0; subID < subPaths.Length; subID++)
            {
                if (subID <= 1 && (subPaths[subID].Length == 0 || subPaths[subID] == subFolder))
                    continue;

                filePath += subID != subpathLast ? subPaths[subID] + PathSeparator : subPaths[subID] = GetFileExtension(subPaths[subID], out ext);
            }

            return string.Join(PathSeparator, new string[] { subFolder, filePath }, 0, 2);
        }

        public static string BuildPath(string fileName, string subFolder, string fileExt)
        {
            string ext;
            string filePath = NormalizePath(fileName, out ext, subFolder);
            if (ext != fileExt) ext = fileExt;
            return filePath += ext;
        }
    }
}