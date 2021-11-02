using EyeXFramework.Wpf;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace RecorderApp.Views
{
    /// <summary>
    /// Interaction logic for ResultsView.xaml
    /// </summary>
    public partial class ResultsView : Window, IView2
    {

        ResultsViewModel resVm = new ResultsViewModel();
        public ResultsView()
        {
            InitializeComponent();
            this.DataContext = resVm;

            Loaded += ResultsView_Loaded;
        }

        /// <summary>
        /// back to main menu button
        /// </summary>
        private void ResultsView_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow mainView = new MainWindow();
            if (DataContext is IControlWindows vm)
            {
                vm.Close += () =>
                {
                    this.Close();
                    mainView.Show();
                };

            }
        }


        #region Show New Window Methods
        public bool? OpenRes()
        {
            return this.ShowDialog();
        }


        public bool? Open(string filePath)
        {
            //videoWindow.Source = new Uri(filePath);
            // choose file to open (send filepath string to ResultsViewModel)
            resVm.setPath(filePath);
            return this.ShowDialog();
        }

        #endregion

        public bool? Back()
        {
            this.Close();
            MainWindow menuVw = new MainWindow();
            return menuVw.ShowDialog();
        }

    }

    public interface IView2
    {
        bool? OpenRes();
        //bool? Back();
        bool? Open(string filePath);

        //bool? Close();
    }
}
