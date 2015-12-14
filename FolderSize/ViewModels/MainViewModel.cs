using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderSize.Models;
using Magic;

namespace FolderSize.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private RelayCommand _refreshCommand;
        private long _totalLength;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _isLiveSorting;


        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new RelayCommand(RefreshAsync));

        public Collection<FileEntryItem> Dirs { get; } = new ObservableCollection<FileEntryItem>();
        public NestedSet<FileEntryItem> DirTree { get; } = new NestedSet<FileEntryItem>();

        public long TotalLength
        {
            get { return _totalLength; }
            private set
            {
                if (value == _totalLength) return;
                _totalLength = value;
                OnPropertyChanged();
            }
        }

        public bool IsLiveSorting
        {
            get { return _isLiveSorting; }
            set
            {
                if (value == _isLiveSorting) return;
                _isLiveSorting = value;
                OnPropertyChanged();
            }
        }

        private void AddLength(long length)
        {
            TotalLength = _totalLength + length;
        }

        private void Refresh()
        {
            var drives = Directory.GetLogicalDrives();
            ScanEntry(drives[0]);
        }

        private async Task RefreshAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run((Action)Refresh, _cancellationTokenSource.Token);
        }

        private void ScanEntry(string path)
        {
            var item = new FileEntryItem(path, AddLength);
            UiTask.Run(Dirs.Add, item).Wait();
            UiTask.Run(() => DirTree.Root = item).Wait();
            ScanEntry(DirTree.RootItem, item, CancellationToken.None);
        }

        private void ScanEntry(NestedSetItem<FileEntryItem> root, FileEntryItem item, CancellationToken cancellationToken)
        {
            DirectoryInfo dir = new DirectoryInfo(item.Path);
            try
            {
                foreach (var fileSystemInfo in dir.EnumerateFileSystemInfos())
                {
                    //if ((enumerateFileSystemInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                    //    continue;
                    
                    if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        var dirEntry = new FileEntryItem(fileSystemInfo.FullName, item.AddLength);
                        var t = UiTask.Run(root.Add, dirEntry);
                        t.Wait(cancellationToken);
                        if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                            break;
                        NestedSetItem<FileEntryItem> nestedSetItem = t.Result;
                        UiTask.Run(Dirs.Add, dirEntry).Wait(cancellationToken);
                        item.AddLength(dirEntry.TotalLength);
                        ScanEntry(nestedSetItem, dirEntry, cancellationToken);
                        //item.TotalLength += dirEntry.TotalLength;
                        if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                            break;
                        continue;
                    }
                    item.AddLength(((FileInfo)fileSystemInfo).Length);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
