using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Caliburn.Micro;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TimeRecorder.Main
{
    public class MainViewModel : Screen, IMainViewModel
    {
        private const int TimerIntervalInSeconds = 1;
        private const int DefaultStartHour = 9;

        private int _id;
        private DateTime _statTime;
        private string _currentTime;
        private bool _haveUnsavedData;
        private DispatcherTimer _timer;
        private int _searchNumber;
        private TimeRecord _selectedTimeRecord;
        private BindableCollection<TimeRecord> _timeRecordsInternal;

        private BindableCollection<TimeRecord> TimeRecordsInternal
        {
            get { return _timeRecordsInternal; }

            set
            {
                _timeRecordsInternal = value;

                TimeRecords.Source = TimeRecordsInternal;

                TimeRecords.SortDescriptions.Clear();
                var sd = new SortDescription("Time", ListSortDirection.Descending);
                TimeRecords.SortDescriptions.Add(sd);
            }
        }

        #region Bindable properties

        public CollectionViewSource TimeRecords { get; set; }

        public int CurrentNumber { get; set; }

        public DateTime StartTime
        {
            get { return _statTime; }

            set
            {
                _statTime = value;
                NotifyOfPropertyChange(() => StartTime);
            }
        }

        public string CurrentTime
        {
            get { return _currentTime; }

            set
            {
                _currentTime = value; 
                NotifyOfPropertyChange(() => CurrentTime);
            }
        }

        public TimeRecord SelectedTimeRecord
        {
            get { return _selectedTimeRecord; }

            set
            {
                _selectedTimeRecord = value;
                SearchNumber = _selectedTimeRecord.Number;
                NotifyOfPropertyChange(() => SelectedTimeRecord);
            }
        }

        public int SearchNumber 
        { 
            get { return _searchNumber; } 
            
            set
            {
                _searchNumber = value;
                NotifyOfPropertyChange(() => SearchNumber);
            } 
        }

        public bool CanSearch
        {
            get { return TimeRecordsInternal.Any(); }
        }

        #endregion
               
        public MainViewModel()
        {
            DisplayName = "Time Recorder";

            InitializeTimer();
            StartTime = DateTime.Today.AddHours(DefaultStartHour);
            CurrentTime = DateTime.Now.ToString(Configuration.TimeFormat);
            
            TimeRecords = new CollectionViewSource();
            TimeRecordsInternal = new BindableCollection<TimeRecord>();
        }

        public void RecordTime()
        {
            var existent = TimeRecordsInternal.SingleOrDefault(x => 
                (x.Number == CurrentNumber) && !x.IsPossiblyWrong);
            if (!ReferenceEquals(null, existent))
            {
                var message = string.Format(
                    "Number {0} was already recorded at {1}!", existent.Number, existent.Time);
                
                MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            var record = new TimeRecord(++_id, CurrentNumber, DateTime.Now);
            TimeRecordsInternal.Add(record);
            _haveUnsavedData = true;

            NotifyOfPropertyChange(() => CanSearch);
        }

        public void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                DefaultExt = ".xlsx",
                Filter = "Text documents (.xlsx)|*.xlsx"
            };

            if (dlg.ShowDialog() == true)
            {
                CreateSpreadsheetWorkbook(dlg.FileName);
                _haveUnsavedData = false;
            }
        }
        
        public void Search()
        {
            var result = TimeRecordsInternal.FirstOrDefault(x => (x.Number == SearchNumber) && !x.IsPossiblyWrong);
            if (result == null)
            {
                var message = string.Format(
                    "Runner with number {0} was not yet recorded!", SearchNumber);

                MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            SelectedTimeRecord = result;
        }

        public override void CanClose(Action<bool> callback)
        {
            if (_haveUnsavedData)
                callback(IsUserOkToCloseWithoutSaving());
            else
                base.CanClose(callback);
        }

        #region Private methods

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(TimerIntervalInSeconds);
            _timer.Tick += OnTimerTick;

            _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString(Configuration.TimeFormat);
        }

        private void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                SheetId = 1,
                Name = "TimeRecords",
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
            };
            sheets.Append(sheet);

            InsertRow(worksheetPart);

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        private void InsertRow(WorksheetPart worksheetPart)
        {
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

            var rows = TimeRecordsInternal.Select(x => x.ToRow(StartTime));

            sheetData.Append(rows);
        }

        private bool IsUserOkToCloseWithoutSaving()
        {
            var result = MessageBox.Show(
                "Unsaved data! Do you realy want to close whithout saving?", 
                "Info", 
                MessageBoxButton.OKCancel, 
                MessageBoxImage.Question);
            
            return (result == MessageBoxResult.OK);
        }

        #endregion
    }
}