using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FolderSize.Models;
using Magic;

namespace FolderSize
{
    class FilesystemWalker
    {
        LimitedConcurrencyLevelTaskScheduler _taskScheduler;
        TaskFactory _taskFactory;
        private CancellationTokenSource _cancellationTokenSource;
        private long waitTimeout = 30;
        
        public void Run(int maxDegreeOfParallelism, NestedSetItem<FileEntryItem> root, FileEntryItem item, CancellationToken cancellationToken)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(maxDegreeOfParallelism);
            _taskFactory = new TaskFactory(_cancellationTokenSource.Token, TaskCreationOptions.AttachedToParent,
                TaskContinuationOptions.AttachedToParent, _taskScheduler);

            _taskFactory.StartNew( () => ScanEntry(root, item, _cancellationTokenSource.Token), cancellationToken, TaskCreationOptions.AttachedToParent, _taskScheduler);
        }

        private async void ScanEntry(NestedSetItem<FileEntryItem> root, FileEntryItem item, CancellationToken cancellationToken)
        {
            DirectoryInfo dir = new DirectoryInfo(item.Path);
            try
            {
                foreach (var fileSystemInfo in dir.EnumerateFileSystemInfos())
                {
                    if ((fileSystemInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                        continue;

                    if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        var dirEntry = new FileEntryItem(fileSystemInfo.FullName, item.AddFileSize, item.Level + 1);
                        NestedSetItem<FileEntryItem> nestedSetItem = new NestedSetItem<FileEntryItem>(dirEntry);
                        
                        var t = UiTask.Run(root.Add, nestedSetItem);
                        //var t = UiTask.Run(() =>
                        //{
                        //    var limit = 100;
                        //    for (int i = 0; i < limit; i++)
                        //    {
                        //        if (_queue.IsEmpty) break;
                        //        Tuple<NestedSetItem<FileEntryItem>, NestedSetItem<FileEntryItem>> tuple;
                        //        _queue.TryDequeue(out tuple);
                        //        tuple?.Item1.Add(tuple.Item2);
                        //    }sdfsd
                        //}, cancellationToken);
                        t.Wait(TimeSpan.FromMilliseconds(waitTimeout));
                        var sw = Stopwatch.StartNew();
                        await t;
                        sw.Stop();
                        if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                            break;
                        
                        DoSave(dirEntry);
                          
                        item.AddFileSize(dirEntry.TotalFileSize);
                        //TaskHelper.Run(ScanEntry, nestedSetItem, dirEntry, cancellationToken, cancellationToken, TaskCreationOptions.AttachedToParent, _taskScheduler);
                        var task =_taskFactory.StartNew(ScanEntry, 
                            new Tuple<NestedSetItem<FileEntryItem>, FileEntryItem, CancellationToken>(nestedSetItem, dirEntry,  cancellationToken), cancellationToken);

                        if (cancellationToken.CanBeCanceled && cancellationToken.IsCancellationRequested)
                            break;

                        continue;
                    }
                    item.AddFileSize(((FileInfo)fileSystemInfo).Length);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ScanEntry(object tupleState)
        {
            var tuple = (Tuple<NestedSetItem<FileEntryItem>, FileEntryItem, CancellationToken>)tupleState;
            ScanEntry(tuple.Item1, tuple.Item2, tuple.Item3);
        }

        private void DoSave(FileEntryItem dirEntry)
        {
            Save?.Invoke(dirEntry);
        }

        public Action<FileEntryItem> Save { get; set; }
    }
}
