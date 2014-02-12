using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Ocronet.Dynamic.Utils
{
    public class DirPattern
    {
        string dirName;
        string entryPattern;
        List<string> dirFiles;

        /// <summary>
        /// Scan directory for regex pattern.
        /// You can use brackets () to saved substring.
        /// </summary>
        /// <param name="DirName"></param>
        /// <param name="Pattern"></param>
        public DirPattern(string DirName, string EntryPattern)
        {
            this.dirName = DirName;
            this.entryPattern = EntryPattern;
            Regex regex = new Regex(entryPattern + "$", RegexOptions.Compiled);
            dirFiles = new List<string>();
            string[] entries = {};
            try
            {
                entries = Directory.GetFileSystemEntries(dirName);
            }
            catch (DirectoryNotFoundException) { }
            foreach (string entry in entries)
            {
                string fileName = Path.GetFileName(entry);
                Match match = regex.Match(entry);
                if (match.Success && fileName == match.Value)
                    dirFiles.Add(match.Groups[match.Groups.Count - 1].Value);
            }
        }

        public static bool Exist(string DirName, string EntryPattern)
        {
            bool result = false;
            Regex regex = new Regex(EntryPattern + "$");
            string[] entries = { };
            try
            {
                entries = Directory.GetFileSystemEntries(DirName);
            }
            catch (DirectoryNotFoundException) { }
            foreach (string entry in entries)
            {
                string fileName = Path.GetFileName(entry);
                Match match = regex.Match(entry);
                if (match.Success && fileName == match.Value)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }

        public static bool Exist(string DirName, string SubDirPattern, string EntryPattern)
        {
            bool result = false;
            Regex regexSubDir = new Regex(SubDirPattern + "$");
            Regex regexEntry = new Regex(EntryPattern + "$");

            string[] subEntriesDirName = { };
            try { subEntriesDirName = Directory.GetFileSystemEntries(DirName); }
            catch (DirectoryNotFoundException) { }
            // iterate DirName entries
            foreach (string subentry in subEntriesDirName)
            {
                string subEntryName = Path.GetFileName(subentry);
                Match match = regexSubDir.Match(subentry);
                if (match.Success && subEntryName == match.Value)
                {
                    string[] entriesSubDir = { };
                    try { entriesSubDir = Directory.GetFileSystemEntries(DirName + Path.DirectorySeparatorChar + subEntryName); }
                    catch (DirectoryNotFoundException) { }
                    catch (IOException) { }
                    // iterate entriesSubDir entries
                    foreach (string entry in entriesSubDir)
                    {
                        string entryName = Path.GetFileName(entry);
                        Match matchEntry = regexEntry.Match(entryName);
                        if (matchEntry.Success && entryName == matchEntry.Value)
                        {
                            result = true;
                            break;
                        }
                    }
                    if (result)
                        break;
                }
            }
            return result;
        }

        public int Length
        {
            get { return dirFiles.Count; }
        }

        public string this[int i]
        {
            get { return dirFiles[i]; }
        }
    }
}
