using Autodesk.Revit.UI;
using Autodesk.Windows;
using CODE.Free.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CODE.Free.Models
{
    public class RibbonEditorViewModel : INotifyPropertyChanged
    {
        private readonly RibbonControl _ribbonControl;
        public RibbonEditorViewModel()
        {
            _ribbonControl = UIFramework.RevitRibbonControl.RibbonControl;
            RibbonTabs = new ObservableCollection<RibbonTab>();
            _ribbonControl.Tabs.CollectionChanged += (s, e) => UpdateRibbonTabs();
            UpdateRibbonTabs();
            MoveUpCommand = new RelayCommand<RibbonTab>(MoveUp, CanMoveUp);
            MoveDownCommand = new RelayCommand<RibbonTab>(MoveDown, CanMoveDown);
            EditTabCommand = new RelayCommand<RibbonTab>(EditTab, CanEditTab);
            ToggleVisibilityCommand = new RelayCommand<RibbonTab>(ToggleVisibility);
        }


        private ObservableCollection<RibbonTab> _ribbonTabs;
        public ObservableCollection<RibbonTab> RibbonTabs
        {
            get { return _ribbonTabs; }
            set
            {
                if (_ribbonTabs != value)
                {
                    _ribbonTabs = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand EditTabCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }

        private void MoveUp(RibbonTab tab)
        {
            try
            {
                int index = RibbonTabs.IndexOf(tab);
                if (index > 0)
                {
                    RibbonTabs.Move(index, index - 1);
                    // Update the order in the Revit RibbonControl
                    var ribbonTab = _ribbonControl.Tabs.FirstOrDefault(t => t.Title == tab.Name);
                    if (ribbonTab != null)
                    {
                        int ribbonIndex = _ribbonControl.Tabs.IndexOf(ribbonTab);
                        if (ribbonIndex > 0)
                        {
                            _ribbonControl.Tabs.RemoveAt(ribbonIndex);
                            _ribbonControl.Tabs.Insert(ribbonIndex - 1, ribbonTab);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Error occurred while moving the tab up.", ex);
            }
        }

        private bool CanMoveUp(RibbonTab tab)
        {

            return tab != null && RibbonTabs.Contains(tab) && RibbonTabs.IndexOf(tab) > 0;
        }
        private void UpdateRibbonTabs()
        {
            try
            {
                var existingTabs = new HashSet<string>(RibbonTabs.Select(tab => tab.Name));
                foreach (var ribbonTab in _ribbonControl.Tabs)
                {
                    if (!existingTabs.Contains(ribbonTab.Title))
                    {
                        RibbonTabs.Add(new RibbonTab
                        {
                            Name = ribbonTab.Title,
                            IsVisible = ribbonTab.IsVisible
                        });
                    }
                }

                var ribbonControlTabNames = _ribbonControl.Tabs.Select(t => t.Title).ToHashSet();
                for (int i = RibbonTabs.Count - 1; i >= 0; i--)
                {
                    if (!ribbonControlTabNames.Contains(RibbonTabs[i].Name))
                    {
                        RibbonTabs.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Error occurred while updating RibbonTabs from _ribbonControl.", ex);
            }
        }

        private void MoveDown(RibbonTab tab)
        {
            try
            {
                int index = RibbonTabs.IndexOf(tab);
                if (index < RibbonTabs.Count - 1)
                {
                    // Update the order in the ObservableCollection
                    RibbonTabs.Move(index, index + 1);

                    // Update the order in the Revit RibbonControl
                    var ribbonTab = _ribbonControl.Tabs.FirstOrDefault(t => t.Title == tab.Name);
                    if (ribbonTab != null)
                    {
                        int ribbonIndex = _ribbonControl.Tabs.IndexOf(ribbonTab);
                        if (ribbonIndex < _ribbonControl.Tabs.Count - 1)
                        {
                            _ribbonControl.Tabs.RemoveAt(ribbonIndex);
                            _ribbonControl.Tabs.Insert(ribbonIndex + 1, ribbonTab);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Error occurred while moving the tab down.", ex);
            }
        }

        private bool CanMoveDown(RibbonTab tab)
        {
            return tab != null && RibbonTabs.Contains(tab) && RibbonTabs.IndexOf(tab) < RibbonTabs.Count - 1;
        }

        private void EditTab(RibbonTab tab)
        {
            try
            {
                if (tab == null)
                {
                    throw new ArgumentNullException(nameof(tab), "Tab cannot be null.");
                }

                // Logic to edit the tab, e.g., open a dialog for editing
                // This can be implemented as needed
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Error occurred while editing the tab.", ex);
            }
        }

        private bool CanEditTab(RibbonTab tab)
        {
            return tab != null;
        }
        private void ToggleVisibility(RibbonTab tab)
        {
            try
            {
                if (tab == null)
                {
                    throw new ArgumentNullException(nameof(tab), "Tab cannot be null.");
                }

                tab.IsVisible = !tab.IsVisible;

                // Update the visibility in the Revit RibbonControl
                var ribbonTab = _ribbonControl.Tabs.FirstOrDefault(t => t.Title == tab.Name);
                if (ribbonTab != null)
                {
                    ribbonTab.IsVisible = tab.IsVisible;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new InvalidOperationException("Error occurred while toggling the tab visibility.", ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonTab : INotifyPropertyChanged
    {
        private string _name;
        private bool _isVisible;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            try
            {
                return _canExecute == null || _canExecute((T)parameter);
            }
            catch
            {
                return false; // Prevent exceptions from propagating
            }
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _execute((T)parameter);
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
