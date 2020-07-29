using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.Caching;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace AutomaticFileBackup
{
    public class FileWatcher : IDisposable
    {
        public string sourceDirectory;
        public string backupDirectory;
        public string[] fileExtensions;
        public string[] exclusionRegexArray;
        public ushort maximumBackups;


        FileSystemWatcher watcher;

        private ConcurrentDictionary<string, string> fileSystemDictionary;
        private Thread loop;
        private volatile bool isActive = false;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Run()
        {
            fileSystemDictionary = new ConcurrentDictionary<string, string>();

            watcher = new FileSystemWatcher();

            watcher.Path = sourceDirectory;

            watcher.NotifyFilter = NotifyFilters.LastWrite
                                 | NotifyFilters.FileName;

            foreach (string extension in fileExtensions)
            {
                watcher.Filters.Add(extension);
            }

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;

            isActive = true;
            loop = new Thread(new ThreadStart(Loop));
            loop.Start();
        }

        private void Loop()
        {
            while(isActive)
            {
                Thread.Sleep(250);
                ProcessDictionary();
            }
        }

        public void Dispose()
        {
            isActive = false;
            loop.Join(10000);

            watcher.EnableRaisingEvents = false;
            watcher.Changed -= OnChanged;
            watcher.Created -= OnChanged;
            watcher.Renamed -= OnChanged;
            watcher.Dispose();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Debug.WriteLine($"{e.Name} {e.ChangeType}");
            fileSystemDictionary.AddOrUpdate(e.Name, e.FullPath, (k, v) => v);
        }

        private void ProcessDictionary()
        {
            while (!fileSystemDictionary.IsEmpty)
            {
                string sourceFile = fileSystemDictionary.Keys.First();
                string sourceFullPath;
                while (!fileSystemDictionary.TryRemove(sourceFile, out sourceFullPath)) { }

                if (IsOnExclusionRegex(sourceFile))
                {
                    return;
                }

                string destinationDirectory = $"{backupDirectory}\\{sourceFile}";

                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                string newFilePath = GetNewFilePath(sourceFile, destinationDirectory);

                if (File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }

                File.Copy(sourceFullPath, newFilePath);

                RemoveExcessBackups(destinationDirectory);
            }
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void RemoveExcessBackups(string destinationDirectory)
        {
            while (new DirectoryInfo(destinationDirectory).GetFileSystemInfos().Length > maximumBackups)
            {
                string filePath = new DirectoryInfo(destinationDirectory).GetFileSystemInfos().OrderBy(fi => fi.CreationTime).First().FullName;
                File.Delete(filePath);
            }
        }

        private static string GetNewFilePath(string sourceFileName, string destinationDirectory)
        {
            string timeString = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fff");
            string filePath = $"{destinationDirectory}\\{sourceFileName}_bak_{timeString}";
            return filePath;
        }

        private bool IsOnExclusionRegex(string testSubject)
        {
            foreach (string pattern in exclusionRegexArray)
            {
                if (Regex.IsMatch(testSubject, pattern))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
