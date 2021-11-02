using CsvHelper;
using Prism.Commands;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class ResultsViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;
        public ResultsViewModel()
        {
            // initialize bindings to buttons from view
            this.OpenCommand = new RelayCommand(this.OpenFile);

            this.CleanDataCommand = new RelayCommand(this.CleanData);
            this.IVTCommand = new RelayCommand(this.PerformIVT);
            this.GroupFixations = new RelayCommand(this.groupFixations);

            this.OpenVidCommand = new RelayCommand(this.OpenVid);
            this.ExtractFrames = new RelayCommand(this.extractFrames);

            // get path for output
            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }


        #region Open CSV File
        private string selectedFile;

        public string SelectedFile
        {
            get { return selectedFile; }
            set
            {
                selectedFile = value;
                RaisePropertyChanged("SelectedFile");
            }
        }
        
        public void setPath(string filePath)
        {
            SelectedFile = filePath;
        }
        

        public ICommand OpenCommand { get; set; }
        /// <summary>
        /// open csv file
        /// </summary>
        private void OpenFile()
        {
            FileDialogViewModel fd = new FileDialogViewModel();
            fd.Extension = "*.csv";
            fd.Filter = "(.csv) |*.csv";

            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            fd.InitialDirectory = outputFileDirectory;
            
            fd.OpenCommand.Execute(null);

            if (fd.FileName == null)
            {
                SelectedFile = "blank";
            }

            this.SelectedFile = fd.FileName;
        }
        #endregion

        #region Open Video File

        private string selectedVid;

        public string SelectedVid
        {
            get { return selectedVid; }
            set 
            { 
                selectedVid = value;
                RaisePropertyChanged("SelectedVid");
            }
        }

        /// <summary>
        /// get path of videos folder
        /// </summary>
        /// <returns></returns>
        string getParent()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string vidPath = Path.GetFullPath(Path.Combine(currentPath, @"..\..\..\..\..\videos"));
            Console.WriteLine(vidPath);
            return vidPath;
        }


        public ICommand OpenVidCommand { get; set; }
        private void OpenVid()
        {
            FileDialogViewModel fd = new FileDialogViewModel();
            fd.Extension = "*.mp4";
            fd.Filter = "(.mp4) |*.mp4";

            fd.InitialDirectory = getParent();
            fd.OpenCommand.Execute(null);

            if (fd.FileName == null)
            {
                SelectedVid = "blank";
            }

            this.SelectedVid = fd.FileName;
        }


        #endregion

        #region Read/Write CSV
        public List<T> readFile<T>(string outputPath)
        {

            List<T> gazeList = new List<T>();

            // find output foldr/ create if it doesn't exit
            outputFileDirectory = @"" + outputPath;
            
            //reads csv file as a list
            using (var reader = new StreamReader(@"" + outputPath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                gazeList = csv.GetRecords<T>().ToList();
            }

            return gazeList;
        }

        public void writeFile<T>(List<T> data, string outputFileName)
        {
            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }
            Console.WriteLine(exeRuntimeDirectory);
            Console.WriteLine(outputFileDirectory);

            //writes list to csv file
            using (var writer = new StreamWriter(outputFileDirectory + @"\" + outputFileName + ".csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }
        #endregion

        #region Clean Data

        public ICommand CleanDataCommand { get; set; }
        void CleanData()
        {

            /// <summary>
            /// Lists for gaze Data!!!
            /// </summary>
            List<RawGaze> pass1 = new List<RawGaze>(); //remove blanks
            List<RawGaze> pass2 = new List<RawGaze>(); //remove time duplicates
            //List<RawGaze> pass3 = new List<RawGaze>(); //remove excess time
            List<RawGaze> validGazes = new List<RawGaze>(); //write file

            CleanDataViewModel cleanVm = new CleanDataViewModel();

            // functions defined in CleanDataViewModel
            pass1 = readFile<RawGaze>(SelectedFile);
            pass2 = cleanVm.removeBlanks(pass1);
            validGazes = cleanVm.removeDuplicates(pass2);
            writeFile(validGazes, "validGazeData");

        }

        #endregion

        #region Perform IVT
        /// <summary>
        /// see IVTViewModel for function definitions
        /// </summary>
        public ICommand IVTCommand { get; set; }
        void PerformIVT()
        {
            IVTViewModel vm = new IVTViewModel();

            List<RawGaze> preIVT = new List<RawGaze>();

            preIVT = readFile<RawGaze>(SelectedFile);
            List<GazeData> finalGazeData = new List<GazeData>();

            finalGazeData = vm.runIVT(preIVT);
            finalGazeData = vm.fixationGroup(finalGazeData);

            writeFile(finalGazeData, "finalGazeData");
            Console.WriteLine(finalGazeData.Count);
        }

        #endregion

        #region Execute Python Scripts for Grouping Fixations

        public ICommand GroupFixations { get; set; }
        void groupFixations()
        {
            string scriptPath = "../../../../Scripts/groupFixations.py";

            //get file name from selected file path
            string fileName = Path.GetFileName(SelectedFile);

            runScript(scriptPath, fileName);
        }

        #endregion

        #region Extract Frames

        /// <summary>
        /// number of scenes to get based on highest fixation duration (?)
        /// </summary>
        private string numScenes;

        public string NumScenes
        {
            get { return numScenes; }
            set 
            { 
                numScenes = value;
                RaisePropertyChanged("NumScenes");
            }
        }

        public ICommand ExtractFrames { get; set; }

        void extractFrames()
        {
            string scriptPath = "../../../../Scripts/extractFrames.py";

            //TODO: change ui so separate csv file for scene selection and ivt+data processing
            string csvFile = Path.GetFileName(selectedFile);
            string sceneCount = numScenes;
            string vidPath = selectedVid;

            string args = csvFile + " " + sceneCount + " " + vidPath;
            Console.WriteLine(args);
            runScript(scriptPath, args);
        }

        #endregion

        #region run cmd
        void runScript(string pythonScript, string args)
        {

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python37\python.exe", pythonScript + " " + args)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = outputFileDirectory
            };

            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine("Running Python Script...");
            Console.WriteLine(output);
        }

        #endregion

        #region Back to MainWindow 

        private DelegateCommand _backCommand;
        public DelegateCommand BackCommand =>
            _backCommand ?? (_backCommand = new DelegateCommand(CloseWindow));

        void CloseWindow()
        {
            Close?.Invoke();
        }

        public Action Close { get; set; }

        public Action Next { get; set; }

        #endregion

    }



}
