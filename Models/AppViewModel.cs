using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PLANET_2
{

    public class AppViewModel : INotifyPropertyChanged
    {
        private Timer _timer;

        private void RefreshProcesses(object state)
        {
            MyProcessList = new ObservableCollection<Process>(Process.GetProcesses().Where(p => p.ProcessName.ToLower().Contains(CurrentFilter)).OrderBy(p => p.ProcessName));
        } 

        private ObservableCollection<Process> _myProcessList;

        public ObservableCollection<Process> MyProcessList
        {
            get { return _myProcessList; }
            set
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _myProcessList.Clear();
                    foreach (Process p in value) _myProcessList.Add(p);
                    if (!_sortAsc) _myProcessList = new ObservableCollection<Process>(_myProcessList.Reverse());
                    NotifyPropertyChanged(nameof(MyProcessList));

                    if(SelectedProcess != null) SelectedProcess = _myProcessList.FirstOrDefault(p => p.Id == SelectedProcess.Id);
                });
            }
        }
        private bool _sortAsc = true;

        private string _refreshMS;
        public string RefreshMS
        {
            get { return _refreshMS; }
            set
            {
                if(int.TryParse(value, out int newVal) && newVal > 0)
                {
                    _refreshMS = value;
                }
                else
                {
                    MessageBox.Show(
                        messageBoxText:"Invalid refresh rate - should be a positive integer",
                        caption:"Error parsing value",
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Error);
                }
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    NotifyPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private Process _selectedProcess;
        public Process SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                if (value != null)
                {
                    _selectedProcess = value;
                    SelectedProcessInfo = new ProcessViewModel(value);
                    Tooltip = SelectedProcess.GetCommandLine();
                    NotifyPropertyChanged(nameof(SelectedProcess));
                }

                ChangePriorityCommand.RaiseCanExecuteChanged();
                KillProcessCommand.RaiseCanExecuteChanged();
            }
        }

        private ProcessViewModel _selectedProcessInfo;
        public ProcessViewModel SelectedProcessInfo
        {
            get { return _selectedProcessInfo; }
            set
            {
                _selectedProcessInfo = value;
                NotifyPropertyChanged(nameof(SelectedProcessInfo));
            }
        }


        private string _tooltip;
        public string Tooltip
        {
            get { return _tooltip; }
            set
            {
                _tooltip = value;
                NotifyPropertyChanged(nameof(Tooltip));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        bool _timerRunning = true;

        public string CurrentFilter = "";
        private string _filterBoxContent = "";
        public string FilterBoxContent {
            get => _filterBoxContent;
            set
            {
                _filterBoxContent = value;
                NotifyPropertyChanged(nameof(FilterBoxContent));
            }
        }

        private ComboBoxItem _selectedPriority;
        public ComboBoxItem SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;
                NotifyPropertyChanged(nameof(FilterBoxContent));
            }
        }

        public RelayCommand StopTimerCommand { get; }
        public RelayCommand StartTimerCommand { get; }
        public RelayCommand RefreshProcessesCommand { get; }
        public RelayCommand ReverseSortCommand { get; }
        public RelayCommand ChangeFilterCommand { get; }
        public RelayCommand ChangePriorityCommand { get; }
        public RelayCommand KillProcessCommand { get; }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timerRunning = false;
            StopTimerCommand.RaiseCanExecuteChanged();
        }
        private void StartTimer()
        {
            int.TryParse(_refreshMS, out int interval);
            _timer.Change(0, interval);
            _timerRunning = true;
            StopTimerCommand.RaiseCanExecuteChanged();
        }
        private void ReverseSort()
        {
            _sortAsc = !_sortAsc;
            RefreshProcesses(null);
        }

        private void ChangeFilter()
        {
            CurrentFilter = _filterBoxContent.ToLower();
            RefreshProcesses(null);
        }

        private void ChangePriority()
        {
            try
            {
                switch ((string)SelectedPriority.Tag)
                {
                    case "Idle":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.Idle;
                        return;
                    case "BelowNormal":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.BelowNormal;
                        return;
                    case "Normal":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.Normal;
                        return;
                    case "AboveNormal":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
                        return;
                    case "High":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.High;
                        return;
                    case "RealTime":
                        SelectedProcess.PriorityClass = ProcessPriorityClass.RealTime;
                        return;
                    default:
                        return;
                }
            }
            catch
            {
                MessageBox.Show(messageBoxText: "Unable to change priority!",
                    caption: "Access denied!",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }

        }

        private void KillProcess()
        {
            try
            {
                SelectedProcess.Kill();
                _selectedProcess = null;
                SelectedProcessInfo = null;
                Tooltip = "";
                ChangePriorityCommand.RaiseCanExecuteChanged();
                KillProcessCommand.RaiseCanExecuteChanged();
            }
            catch
            {
                MessageBox.Show(messageBoxText: "Unable to kill process!",
                    caption:"Access denied!",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Error);
            }
        }

        public AppViewModel()
        {
            StopTimerCommand = new RelayCommand(param => StopTimer(), param => _timerRunning);
            StartTimerCommand = new RelayCommand(param => StartTimer());
            RefreshProcessesCommand = new RelayCommand(param => RefreshProcesses(null));
            ReverseSortCommand = new RelayCommand(param => ReverseSort());
            ChangeFilterCommand = new RelayCommand(param => ChangeFilter());
            ChangePriorityCommand = new RelayCommand(param => ChangePriority(), param => SelectedProcess != null);
            KillProcessCommand = new RelayCommand(param => KillProcess(), param => SelectedProcess != null);

            _myProcessList = new ObservableCollection<Process> { };
            RefreshProcesses(null);
            _timer = new Timer(RefreshProcesses, null, Timeout.Infinite, Timeout.Infinite);
            RefreshMS = 1000.ToString();

            StartTimer();

            
        }

    }
}
