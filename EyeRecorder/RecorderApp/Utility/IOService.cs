using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RecorderApp.Utility
{
    public sealed class IOService
    {
        public string OpenFile(string defaultExtension, string filter, string initialDir)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = defaultExtension;
            fd.Filter = filter;
            // initial directory for open file dialog
            fd.InitialDirectory = initialDir;
            //fd.InitialDirectory = @"D:\tobii\thess\EyeGazeTracker\EyeRecorder\RecorderApp\bin\x86\Debug\Output";

            // checks if a file is selected after dialog pops up
            bool? result = fd.ShowDialog();

            // if result is true, return filename (path)
            return result.Value ? fd.FileName : null;
        }
    }
}
