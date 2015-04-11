using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sapphire
{
    public class FileWatcher 
    {

        public delegate void OnFileChanged(string path);

        private Dictionary<string, FileSystemWatcher> watchers = new Dictionary<string, FileSystemWatcher>();

        private string rootPath;

        private List<string> _changedFiles = new List<string>();
        private object _changedFilesLock = new object();

        public FileWatcher(string _rootPath)
        {
            rootPath = _rootPath;
        }

        public void WatchFile(string relativeFilePath)
        {
            var filePath = Path.Combine(rootPath, relativeFilePath);

            try
            {
                var path = Path.GetDirectoryName(filePath);
                Debug.LogWarning("adding watcher for path " + path);

                var watcher = new FileSystemWatcher();
                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.Attributes |
                    NotifyFilters.CreationTime |
                    NotifyFilters.FileName |
                    NotifyFilters.LastAccess |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size |
                    NotifyFilters.Security;

                watcher.Filter = "*.*";
                watcher.Changed += (sender, args) =>
                {
                    lock (_changedFilesLock)
                    {
                        _changedFiles.Add(args.FullPath);
                    }
                };

                watcher.EnableRaisingEvents = true;
                watchers.Add(filePath, watcher);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to add watcher for file \"" + filePath + "\": " + ex.ToString());
            }
        }

        public void Dispose()
        {
            foreach (var watcher in watchers.Values)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }

            watchers.Clear();
        }

        public bool CheckForAnyChanges(bool discardChanged = true)
        {
            bool result = false;

            lock (_changedFilesLock)
            {
                result = _changedFiles.Any();
                if (discardChanged)
                {
                    _changedFiles.Clear();
                }
            }

            return result;
        }

    }

}
