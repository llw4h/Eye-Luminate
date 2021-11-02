using Prism.Commands;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class FileDialogViewModel : BindableBase
    {

        public FileDialogViewModel()
        {
            OpenCommand = new RelayCommand(this.OpenFile);
        }

        public Stream Stream { get; set; }

        public string Extension { get; set; }

        public string Filter { get; set; }

        public string FileName { get; set; }

        public string InitialDirectory { get; set; }

        public ICommand OpenCommand { get; set; }

        private void OpenFile()
        {
            IOService ioService = new IOService();
            this.FileName = ioService.OpenFile(this.Extension, this.Filter, this.InitialDirectory);
        }
    }
}
