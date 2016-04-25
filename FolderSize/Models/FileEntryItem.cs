using System;
using System.Diagnostics;
using System.Threading;
using FolderSize.ViewModels;

namespace FolderSize.Models
{
    [DebuggerDisplay("{TotalFileSize} {Path}")]
    internal class FileEntryItem : BaseViewModel
    {
        private readonly Action<long> _call;
        private long _totalFileSize;
        private long _totalFileCount;

        public FileEntryItem(string path, Action<long> call, int level)
        {
            DisplayText = path;
            _call = call;
            Level = level;
        }

        public string DisplayText { get; }

        public string Path
        {
            get { return DisplayText; }
        }

        public int Level { get; }

        public long TotalFileSize
        {
            get { return _totalFileSize; }
            //private set
            //{
            //    if (value == _totalFileSize) return;
            //    _totalFileSize = value;
            //    OnPropertyChanged();
            //}
        }

        public long TotalFileCount { get { return _totalFileCount; } }

        public void AddFileSize(long fileSize)
        {
            Interlocked.Add(ref _totalFileSize, fileSize);
            Interlocked.Add(ref _totalFileCount, 1L);
            OnPropertyChanged("TotalFileSize");
            OnPropertyChanged("TotalFileCount");
            _call(fileSize);
        }
    }
}