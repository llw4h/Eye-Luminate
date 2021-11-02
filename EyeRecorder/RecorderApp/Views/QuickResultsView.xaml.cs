using Prism.Events;
using RecorderApp.Utility;
using RecorderApp.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for QuickResultsView.xaml
    /// </summary>
    public partial class QuickResultsView : Window, IView3
    {
        IEventAggregator _ea;
        public QuickResultsView(IEventAggregator ea)
        {
            InitializeComponent();
            _ea = ea;
            Loaded += QuickResultsView_Loaded;
            lbFiles.SelectionChanged += LbFiles_SelectionChanged; ;
        }

        private void LbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ea.GetEvent<ListboxWatchEvent>().Publish(lbFiles.SelectedItem.ToString());
        }

        private void QuickResultsView_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mView = new MainWindow();
            if (DataContext is IControlWindows vm)
            {
                vm.Close += () =>
                {
                    this.Close();
                    mView.Show();
                };

            }
        }

        #region Show New Window Methods
        public bool? OpenQRes()
        {
            return this.ShowDialog();
        }


        public bool? Open(string filePath)
        {
            // choose file to open (send filepath string to ResultsViewModel)
            //qResVm.setPath(filePath);
            return this.ShowDialog();
        }

        #endregion

        public bool? Back()
        {
            this.Close();
            MainWindow menuVw = new MainWindow();
            return menuVw.ShowDialog();
        }

        private void VidPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            //VidPlayer.SetCurrentValue(VidPlayer.Source, new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg"));
            VidPlayer.Source = new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg");
        }

        private void VidPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            VidPlayer.Source = new Uri(@"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\Assets\blank.jpg");
            
        }

        private void VidPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            VidPlayer.Visibility = Visibility.Visible;
        }


    }


    public interface IView3
    {
        bool? OpenQRes();
        //bool? Back();
        bool? Open(string filePath);

        //bool? Close();
    }
}
