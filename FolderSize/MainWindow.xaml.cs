using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FolderSize.Models;
using FolderSize.ViewModels;

namespace FolderSize
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            UiTask.Initialize();
            
            _viewModel = new MainViewModel();
            _viewModel.DirTree.Root = new FileEntryItem("", null, 0);
            DataContext = _viewModel;
            _viewModel.PropertyChanged += OnPropertyChanged;
            
            InitializeComponent();

            TreeView.Items.LiveSortingProperties.Add("Totalcount");
            TreeView.Items.SortDescriptions.Add(new SortDescription("Value.TotalFileSize", ListSortDirection.Descending));
            TreeView.Items.IsLiveSorting = _viewModel.IsLiveSorting;

            //ItemsControl.Items.LiveSortingProperties.Add("TotalFileSize");
            //ItemsControl.Items.SortDescriptions.Add(new SortDescription("TotalFileSize", ListSortDirection.Descending));
            //ItemsControl.Items.IsLiveSorting = _viewModel.IsLiveSorting;


            //ItemCollection

            //var cvs = FindResource("cvs") as CollectionViewSource;
            //if (cvs != null)
            //    cvs.Source = _viewModel.DirTree;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsLiveSorting")
            {
                TreeView.Items.IsLiveSorting = _viewModel.IsLiveSorting;
                //ItemsControl.Items.IsLiveSorting = _viewModel.IsLiveSorting;
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var cvs = FindResource("cvs") as CollectionViewSource;
            if (cvs != null)
                cvs.Source = _viewModel.DirTree.RootItem.OrderByDescending(i => i.Value.TotalFileSize).ToArray();
        }
    }
}
