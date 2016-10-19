using System;
using System.IO;

namespace QMora.FamilyPhotos.Common
{
    public class InputFilesListener
    {
        public event EventHandler<InputFileEventArgs> FileRecivedEvent;

        private readonly string _inputDirectory;

        FileSystemWatcher watcher = new FileSystemWatcher();

        public InputFilesListener(string inputDirectory)
        {
            _inputDirectory = inputDirectory;
        }

        public void Start()
        {
            
            watcher.Path = _inputDirectory;
            
            /*
            watcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            watcher.Filter = "*.*";
            */

            // Add event handlers.
            //watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += OnCreate;
            //watcher.Deleted += new FileSystemEventHandler(OnChanged);
            //watcher.Renamed += new RenamedEventHandler(OnRenamed);
            

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (watcher != null)
            {
                watcher.Dispose();
            }
        }

        private void OnCreate(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            //var isLocked = IsFileLocked(fileSystemEventArgs.FullPath);
            //if (isLocked)
            //{
            //    return;
            //}

            var eventArg = new InputFileEventArgs
            {
                ImageName = fileSystemEventArgs.Name,
                ImageFullPath = fileSystemEventArgs.FullPath
            };

            if (FileRecivedEvent != null)
            {
                FileRecivedEvent(this, eventArg);
            }
        }

        private bool IsFileLocked(string fileFullPath)
        {
            FileInfo file = new FileInfo(fileFullPath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }

    public class InputFileEventArgs : EventArgs
    {
        public string ImageFullPath { get; set; }

        public string ImageName { get; set; }
    }
}
