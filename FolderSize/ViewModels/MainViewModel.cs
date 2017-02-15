using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using FolderSize.Annotations;
using FolderSize.Models;
using Magic;

namespace FolderSize.ViewModels
{
    internal class MainViewModel : BaseViewModel
    {
        private RelayCommand _refreshCommand;
        
        private readonly NestedSet<FileEntryItem> _dirTree = new NestedSet<FileEntryItem>();
        private long _totalLength;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        
        private bool _isLiveSorting = true;


        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new RelayCommand( RefreshAsync)); }
        }
        
        public NestedSet<FileEntryItem> DirTree
        {
            get { return _dirTree; }
        }

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

        private async Task RefreshAsync()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            await Task.Run((Action)Refresh, _cancellationTokenSource.Token);
        }

        private void Refresh()
        {
            _totalLength = 0;
            var drives = Directory.GetLogicalDrives();
            ScanEntry(drives[0]);
        }

        private void ScanEntry(string path)
        {
            var item = new FileEntryItem(path, AddLength, 0);
            UiTask.Run(() => DirTree.Root = item).Wait();
            var fsw = new FilesystemWalker();
            fsw.Run(4, DirTree.RootItem, item, _cancellationTokenSource.Token);
        }
    }
}
