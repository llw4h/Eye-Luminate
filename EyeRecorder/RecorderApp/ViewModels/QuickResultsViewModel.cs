using CsvHelper;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RecorderApp.Models;
using RecorderApp.Utility;
using RecorderApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace RecorderApp.ViewModels
{
    public class QuickResultsViewModel : BindableBase, IControlWindows
    {
        string exeRuntimeDirectory;
        string outputFileDirectory;
        DirectoryInfo dir;
        private readonly BackgroundWorker worker;
        int runCount;

        public bool getAll { get; set; }
        FileInfo[] Files;
        public QuickResultsViewModel(IEventAggregator ea)
        {
            runCount = 0;
            ea.GetEvent<SavePathEvent>().Subscribe(GetVidPath);
            //this.OpenCommand = new RelayCommand(this.OpenFile);
            this.OpenVidCommand = new RelayCommand(this.OpenVid);

            this.ChooseDestPath = new RelayCommand(this.ChooseFolder);

            this.SelectScenesCommand = new RelayCommand(this.SelectScenes);
            //this.SelectScenesCommand = new RelayCommand(this.StartProcess);
            //this.SelectScenesCommand = new RelayCommand(this.StartProcess);
            //backgroundworker
            //this.instigateWorkCommand = new RelayCommand(o => this.worker.RunWorkerAsync(), o => !this.worker.IsBusy);

            worker = new BackgroundWorker()
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            //this.worker.DoWork += this.DoWork;
            this.worker.ProgressChanged += this.ProgressChanged;

            this.worker.RunWorkerCompleted += this.RunWorkerCompleted;

            // get path for output
            exeRuntimeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            // gaze output directory
            string gazeOutputPath = Path.Combine(exeRuntimeDirectory, "Output", "GazeTrackerOutput");
            if (!System.IO.Directory.Exists(gazeOutputPath))
            {
                System.IO.Directory.CreateDirectory(gazeOutputPath);
            }

            dir = new DirectoryInfo(gazeOutputPath); //Assuming Test is your Folder
         

            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            selectedPath = Path.Combine(outputFileDirectory, "Clips");
            //InitLoad();

            ea.GetEvent<RecStatusEvent>().Subscribe(GetTrackingStatus);
            ea.GetEvent<ListboxWatchEvent>().Subscribe(ChangeVidPath);
        }

        #region Event Aggregators
        /// <summary>
        /// change Vid Path based on the selected item in listbox (receive change trigger)
        /// </summary>
        /// <param name="obj"></param>
        private void ChangeVidPath(string obj)
        {
            // 1. get the filename from SelectedCSV ,/
            string fn = obj;

            // 2. substring the name ,/
            int br = fn.IndexOf('_');
            string name = fn.Substring(0, br);

            // 3. find name from directory of clips if it exists
            string default_vidfolder = getParent();
            DirectoryInfo defaultDir = new DirectoryInfo(default_vidfolder);
            FileInfo[] matched = defaultDir.GetFiles(name+"*");

            // 4. assign path to SelectedVid
            if (matched.Any())
            {

                Console.WriteLine(matched.First().FullName);
                SelectedVid = matched.First().FullName;
            }         

        }

        /// <summary>
        /// Receives the saved path of selected video from experiment
        /// </summary>
        /// <param name="obj"></param>
        private void GetVidPath(string obj)
        {
            SelectedVid = obj;
            Console.WriteLine("nagwork ba huhu?" + SelectedVid);
        }

        /// <summary>
        /// Checks if view is triggered before/after a gazetracking session
        /// if before, list of all files in directory is shown
        /// if after, the latest recorded session is shown
        /// </summary>
        /// <param name="done"></param>
        private void GetTrackingStatus(bool done)
        {
            Console.WriteLine("un nasend: " + done);
            if (done)
            {
                getLast();
            } 
            else
            {
                getFileList();
            }
        }
        #endregion

        #region Listbox
        public void getFileList()
        {
            //FileInfo[] Files = dir.GetFiles("*.csv"); //Getting CSV files
            Files = dir.GetFiles("*.csv");
            // sort by last write time
            Array.Sort(Files, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
            // reverse array to sort by descending order
            Array.Reverse(Files);

            foreach (FileInfo file in Files)
            {
                Console.WriteLine(file.Name);
                FileList.Add(file);
            }

            //SelectedCSV = (FileInfo)FileList.FirstOrDefault();
            
        }

        public void getLast()
        {
            Files = dir.GetFiles("*.csv");
            // sort by last write time
            Array.Sort(Files, (f1, f2) => f1.LastWriteTime.CompareTo(f2.LastWriteTime));
            // reverse array to sort by descending order
            Array.Reverse(Files);
            FileList.Clear();
            FileList.Add(Files[0]);
            Console.WriteLine(Files[0]);
        }

        private ObservableCollection<Object> _fileList = new ObservableCollection<Object>();

        public ObservableCollection<Object> FileList
        {
            get { return _fileList; }
            set
            {
                _fileList = value;
                RaisePropertyChanged("FileList");
            }
        }

        #endregion

        #region UI bindings

        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                RaisePropertyChanged("CurrentProgress");
            }
        }

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

        private ObservableCollection<VideoClip> clipData = new ObservableCollection<VideoClip>();
        
        public ObservableCollection<VideoClip> ClipData
        {
            get { return clipData; }
            set 
            {

                clipData = value;
                RaisePropertyChanged("ClipData");
            }
        }

        private FileInfo selectedCSV;

        public FileInfo SelectedCSV
        {
            get { return selectedCSV; }
            set 
            {
                //selectedCSV = value;
                //RaisePropertyChanged("SelectedCSV");
                SetProperty(ref selectedCSV, value);
            }
        }


        private VideoClip selectedItem;

        public VideoClip SelectedItem
        {
            get { return selectedItem; }
            set 
            {

                selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public void Load(string csvPath)
        {
            List<VideoClip> dataList = readFile<VideoClip>(csvPath);

            foreach (VideoClip obj in dataList)
            {
                App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                {
                    clipData.Add(obj);
                });
            }
        }

        
        #endregion

        #region BackgroundWorker

        private int _currentProgress;

        private bool _progressVisibility = true;

        public bool ProgressVisibility
        {
            get { return _progressVisibility; }
            set 
            {
                _progressVisibility = value;
                RaisePropertyChanged("ProgressVisibility");
            }
        }

        private string _output;

        public string Output
        {
            get { return _output; }
            set 
            {
                SetProperty(ref _output, value);
            }
        }

        private bool _startEnabled = true;      

        public bool StartEnabled
        {
            get { return _startEnabled = true; }
            set 
            { 
                _startEnabled = value;
                RaisePropertyChanged("StartEnabled");
            }
        }

        private bool _cancelEnabled = true;

        public bool CancelEnabled
        {
            get { return _cancelEnabled = true; }
            set
            {
                _cancelEnabled = value;
                RaisePropertyChanged("CancelEnabled");
            }
        }
        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.CurrentProgress = e.ProgressPercentage;
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StartEnabled = !worker.IsBusy;
            CancelEnabled = worker.IsBusy;
        }

        public ICommand StartButton { get; set; }
        public void StartProcess()
        {
            Output = "";
            // selected csv from listbox filepath
            SelectedFile = SelectedCSV.FullName;
            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
                Console.WriteLine("hey");
            }

            StartEnabled = !worker.IsBusy;
            CancelEnabled = worker.IsBusy;
        }

        public void CancelProcess()
        {
            worker.CancelAsync();
        }

        
        #endregion

        /// <summary>
        /// refresh listbox
        /// </summary>
        public void reload()
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                clipData.Clear();
            });
            _currentProgress = 0;
        }

        #region Task Async



        #endregion


        #region Choose Destination Folder for output

        private string selectedPath;

        public string SelectedPath
        {
            get { return selectedPath; }
            set
            {
                selectedPath = value;
                RaisePropertyChanged("SelectedPath");
            }
        }

        public ICommand ChooseDestPath { get; set; }
        private void ChooseFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SelectedPath = fbd.SelectedPath;
            }
        }

        #endregion

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

        public void setDestPath(string filePath)
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

            fd.InitialDirectory = outputFileDirectory;


            fd.OpenCommand.Execute(null);

            if (fd.FileName == null)
            {
                SelectedFile = "blank";
            }

            this.SelectedFile = fd.FileName;
            Console.WriteLine("Open file");
        }
        #endregion

        #region Open Video File

        private string selectedVid;

        public string SelectedVid
        {
            get { return selectedVid; }
            set
            {
                if (SelectedCSV != null)
                {
                    Console.WriteLine("something is selected right now");
                }
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

        #region Clean Data

        private List<RawGaze> CleanData()
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

            return validGazes;
        }

        #endregion

        #region Perform IVT
        /// <summary>
        /// see IVTViewModel for function definitions
        /// </summary>
        List<GazeData> PerformIVT()
        {
            IVTViewModel vm = new IVTViewModel();

            List<RawGaze> preIVT = new List<RawGaze>();

            preIVT = readFile<RawGaze>(SelectedFile);
            List<GazeData> finalGazeData = new List<GazeData>();

            finalGazeData = vm.runIVT(preIVT);
            finalGazeData = vm.fixationGroup(finalGazeData);

            return finalGazeData;

            //Console.WriteLine(finalGazeData.Count);
        }

        #endregion

        #region Extract Scenes
        void extractScenes()
        {
            string scriptPath = "../../../../Scripts/extractScenes.py";

            //TODO: change ui so separate csv file for scene selection and ivt+data processing
            string csvFile = Path.GetFileName(SelectedFile);
            string sceneCount = numScenes;
            string vidPath = selectedVid;

            string args = csvFile + " " + sceneCount + " " + vidPath;
            Console.WriteLine(args);
            Console.WriteLine("chosen path: " + SelectedPath);
            runScript(scriptPath, args, selectedPath);
        }

        #endregion

        #region run cmd
        void runScript(string pythonScript, string args, string destFolder)
        {
            Console.WriteLine("argument: " + @"C:\Python37\python.exe" + " " + pythonScript + " " + args + " " + destFolder);
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python37\python.exe", pythonScript + " " + args + " " + destFolder)
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

        public string writeFile<T>(List<T> data, string outputFileName)
        {
            outputFileDirectory = Path.Combine(exeRuntimeDirectory, "Output");
            if (!System.IO.Directory.Exists(outputFileDirectory))
            {
                System.IO.Directory.CreateDirectory(outputFileDirectory);
            }
            Console.WriteLine(exeRuntimeDirectory);
            Console.WriteLine(outputFileDirectory);

            //writes list to csv file
            string fullOutput = outputFileDirectory + @"\" + outputFileName + ".csv";
            using (var writer = new StreamWriter(fullOutput))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }

            return fullOutput;
        }
        #endregion

        #region Scene Selection

        public ICommand SelectScenesCommand { get; set; }

        private async Task cleanDataAsync()
        {

            //clean data and write validGazeData csv
            List<RawGaze> validGazeData = new List<RawGaze>();
            Output = "Cleaning Data...";
            validGazeData = await Task.Run(() => CleanData());

            SelectedFile = writeFile(validGazeData, "validGazeData");
        }

        private async Task doIVTAsync()
        {
            List<GazeData> finalGazeData = new List<GazeData>();
            Output = "Performing IVT...";
            finalGazeData = await Task.Run(() => PerformIVT());

            SelectedFile = writeFile(finalGazeData, "finalGazeData");
        }

        private async Task getScenes()
        {
            Output = "Extracting Scenes...";
            await Task.Run(() => extractScenes());
        }
        
        private async void SelectScenes()
        {
            if (runCount != 0)
            {
                reload();
            }
            runCount++;

            Thread.Sleep(100);
            worker.ReportProgress(_currentProgress+=5);
            SelectedFile = SelectedCSV.FullName;

            Thread.Sleep(100);
            worker.ReportProgress(_currentProgress += 15);
            await cleanDataAsync();
            
            //perform IVT algo
            Thread.Sleep(100);
            worker.ReportProgress(_currentProgress += 25);
            await doIVTAsync();


            Thread.Sleep(100);
            worker.ReportProgress(_currentProgress += 25);
            //group fixations and extract scenes
            await getScenes();

            Output = "Loaded Clips...";
            //TODO: modify and make not brute-force
            string infoDir = Path.Combine(outputFileDirectory, "selectedClipInfo.csv");
            Console.WriteLine(infoDir);
            if (File.Exists(infoDir))
            {
                Console.WriteLine("file exists");
                Load(infoDir);
            }
            Thread.Sleep(100);
            worker.ReportProgress(_currentProgress=100);
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
