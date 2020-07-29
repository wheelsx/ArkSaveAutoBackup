using AutomaticFileBackup;

namespace ArkSaveAutoBackup
{
    internal static class FileWatcherFactory
    {
        internal static FileWatcher GetArkFileWatcher(string saveDirectory, string backupDirectory)
        {
            FileWatcher fileWatcher = new FileWatcher
            {
                sourceDirectory = saveDirectory,
                backupDirectory = backupDirectory,

                fileExtensions = new string[] { "*.ark",
                    "*.arktribe",
                    "*.arkprofile" },

                exclusionRegexArray = new string[] { "_(\\d+\\.[_\\d\\.]*)+ark" },
                maximumBackups = Properties.ArkSaveAutoBackup.Default.NumberOfBackups
            };
            fileWatcher.Run();
            return fileWatcher;
        }
    }
}