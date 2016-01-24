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
            private set
            {
                if (value == _totalFileSize) return;
                _totalFileSize = value;
                OnPropertyChanged();
            }
        }

        public void AddLength(long length)
        {
            Interlocked.Add(ref _totalFileSize, length);
            OnPropertyChanged("TotalFileSize");
            _call(length);
        }
    }
}