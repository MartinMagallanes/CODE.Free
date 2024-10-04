using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.UI.Windows.Controls;
using FabricationPartBrowser;
using FabricationPartBrowser.Modules;
using FabricationPartBrowser.ViewModels;
using FabSettings;
using Nice3point.Revit.Toolkit.External;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Binding = System.Windows.Data.Binding;
using Grid = System.Windows.Controls.Grid;
using TextBox = System.Windows.Controls.TextBox;

namespace CODE.Free
{
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            OverrideFabConfigDialog();
        }
        void B_CanExecute(object obj, CanExecuteEventArgs avgs)
        {
            if (avgs.ActiveDocument == null || !avgs.ActiveDocument.Application.IsMechanicalAnalysisEnabled || avgs.ActiveDocument.IsModifiable)
                avgs.CanExecute = false;
            else
                avgs.CanExecute = true;
        }
        void B_Executed(object sender, ExecutedEventArgs e)
        {
            try
            {
                OnButtonSettingsClick();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error: FabSettingsV2", $"B_Executed():\n{ex.GetType()}: {ex.Message}");
            }
        }
        const string _search = "Search";
        const string _toolTip = "Enter text to filter services list";
        const string _isConnected = "IsConfigConnectedToSource";
        const string _unloadedServices = "UnloadedServicesListView";
        const string _loadedServices = "LoadedServicesListView";
        const string _addButton = "Add";
        public static bool FabSettingsAutoReload = false;
        public static CheckBox reloadCheckBox = null;
        void OverrideFabConfigDialog()
        {
            FabSettingsAutoReload = CODE.Free.Properties.Settings.Default.FabSettingsAutoReload;
            try
            {
                RevitCommandId cmd = RevitCommandId.LookupCommandId("ID_MEP_FABRICATION_SETTINGS");
                if (cmd != null)
                {
                    try
                    {
                        UiApplication.RemoveAddInCommandBinding(cmd);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {

                    }
                    AddInCommandBinding b = UiApplication.CreateAddInCommandBinding(cmd);
                    if (b != null)
                    {
                        b.CanExecute += new EventHandler<CanExecuteEventArgs>(B_CanExecute);
                        b.Executed += new EventHandler<ExecutedEventArgs>(B_Executed);
                    }
                }

                FabPartBrowserPage page = typeof(FabPartUtility).GetPrivateMember("FabBrowserPage") as FabPartBrowserPage;
                Button settings = page.FindName("Settings") as Button;
                settings.Content = $"Settings...";
                DockPanel panel = settings.Parent as DockPanel;
                panel.Children.Remove(settings);
                reloadCheckBox = new CheckBox()
                {
                    Content = "Reload on Open",
                    ToolTip = "Automatically Reload Configuration when this dialog is opened",
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    IsChecked = FabSettingsAutoReload,
                };
                ToolTipService.SetInitialShowDelay(reloadCheckBox, 200);
                Grid newGrid = new Grid();
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto});
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto});
                newGrid.Children.Add(settings);
                newGrid.Children.Add(reloadCheckBox);
                Grid.SetColumn(reloadCheckBox, 0);
                Grid.SetColumn(settings, 1);
                panel.Children.Insert(panel.Children.Count - 1, newGrid);
                DockPanel.SetDock(newGrid, Dock.Right);
                reloadCheckBox.Checked += (s, e) =>
                {
                    FabSettingsAutoReload = true;
                    SaveFabSettingsAutoReload();
                };
                reloadCheckBox.Unchecked += (s, e) =>
                {
                    FabSettingsAutoReload = false;
                    SaveFabSettingsAutoReload();
                };
                RemoveRoutedEventHandlers(settings, Button.ClickEvent);
                settings.Click += Settings_Click;
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
        }
        static void SaveFabSettingsAutoReload()
        {
            CODE.Free.Properties.Settings.Default.FabSettingsAutoReload = FabSettingsAutoReload;
            CODE.Free.Properties.Settings.Default.Save();
        }
        static void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnButtonSettingsClick();
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
        }
        public static BitmapSource GetEmbeddedImage(Assembly assem, string name)
        {
            try
            {
                Stream s = assem.GetManifestResourceStream(name);
                return BitmapFrame.Create(s);
            }
            catch
            {
                return null;
            }
        }
        public static void OnButtonSettingsClick()
        {
            if (!ServiceButtonBase.CanTakeoff)
                return;
            FabPartBrowserExternalEventHandlerManager.Instance.RaiseEvent((IExternalEventWorker)new UpdateFabSettings());
        }
        public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            if (eventHandlersStore == null) return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            foreach (var routedEventHandler in routedEventHandlers)
                element.RemoveHandler(routedEvent, routedEventHandler.Handler);
        }
        public static Window GetWindow(WindowCollection list, IntPtr hwnd)
        {
            foreach (Window wnd in list)
            {
                if (new WindowInteropHelper(wnd).Handle == hwnd)
                {
                    return wnd;
                }
            }
            return null;
        }
        public class UpdateFabSettings : IExternalEventWorker
        {
            ListView unloadedServices = null;
            ListView loadedServices = null;
            TreeView treeUnloadedServices = null;
            TreeView treeLoadedServices = null;
            TextBox textBox = null;
            TextBox textBox2 = null;
            CheckBox checkBox = null;
            CollectionViewSource cvs = null;
            CollectionViewSource cvs2 = null;
            Grid mainGrid = null;
            FabSettingsDialog _fabSettingsDialog = null;
            FabricationConfigurationUserControl _configControl = null;
            public string GetName() => nameof(UpdateFabSettings);
            public string GetDescription() => "Upgrade Fabrication Settings Dialog.";
            public void Execute(UIApplication revitApp)
            {
                _fabSettingsDialog = null;
                try
                {
                    _fabSettingsDialog = new FabSettingsDialog(GlobalInfo.CurrentDoc);
                    _fabSettingsDialog.Activated += _fabSettingsDialog_Activated;
                    _fabSettingsDialog.Closing += _updateAutoReload;
                    _fabSettingsDialog.SetParent(revitApp.MainWindowHandle);
                    _fabSettingsDialog.ShowDialog();
                }
                catch (Autodesk.Revit.Exceptions.InvalidOperationException ex)
                {
                    if (!(ex.Message == "Failed to launch the Fabrication Settings Dialog."))
                        throw ex;
                    if (_fabSettingsDialog == null || !_fabSettingsDialog.IsVisible)
                        return;
                    _fabSettingsDialog.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            private void OnClosing(object sender, CancelEventArgs e)
            {
                if (!ViewModelStore.Instance.ConfigurationsViewModel.CanClose)
                {
                    e.Cancel = true;
                    return;
                }
                ViewModelStore.Instance.SettingsViewModel.HandleDialogClose();
            }
            private void _updateAutoReload(object sender, CancelEventArgs e)
            {
                if (checkBox != null)
                {
                    CODE.Free.Application.FabSettingsAutoReload = checkBox.IsChecked ?? false;
                }
                reloadCheckBox.IsChecked = CODE.Free.Application.FabSettingsAutoReload;
                SaveFabSettingsAutoReload();
            }
            void _fabSettingsDialog_Activated(object sender, EventArgs e)
            {
                _fabSettingsDialog.Activated -= _fabSettingsDialog_Activated;
                _configControl = typeof(FabPartUtility).GetPrivateMember("FabConfigControl") as FabricationConfigurationUserControl;
                if (_configControl != null)
                {
                    Button sync = _configControl.FindName("Sync") as Button;
                    Grid syncGrid = sync.Parent as Grid;
                    if (syncGrid != null)
                    {
                        checkBox = new CheckBox()
                        {
                            Content = "Reload on Open",
                            Margin = new Thickness(15, 4, 4, 4),
                            ToolTip = "Automatically Reload Configuration when this dialog is opened",
                            IsChecked = CODE.Free.Application.FabSettingsAutoReload,
                        };
                        syncGrid.RowDefinitions.Insert(0, new RowDefinition() { Height = GridLength.Auto });
                        syncGrid.RowDefinitions.Insert(1, new RowDefinition() { Height = GridLength.Auto });
                        syncGrid.Children.Add(checkBox);
                        Grid.SetRow(checkBox, 1);
                        Grid.SetColumn(checkBox, 0);
                        ToolTipService.SetInitialShowDelay(checkBox, 200);
                    }
                    if (CODE.Free.Application.FabSettingsAutoReload)
                    {
                        FabPartUtility.SimulateButtonClick(sync);
                    }
                    _fabSettingsDialog.Icon = GetEmbeddedImage(Assembly.GetExecutingAssembly(), "CODE.Free.Resources.Icons.Code Icon.ico");
                    textBox = new SearchTextBox()
                    {
                        LabelText = _search,
                        Margin = new Thickness(15, 0, 0, 5),
                        ToolTip = _toolTip,
                    };
                    textBox.TextChanged += Tb_TextChanged;
                    textBox2 = new SearchTextBox()
                    {
                        LabelText = _search,
                        Margin = new Thickness(0, 0, 15, 5),
                        ToolTip = _toolTip,
                    };
                    textBox2.TextChanged += Tb_TextChanged;
                    Binding b = new Binding
                    {
                        Path = new PropertyPath(_isConnected)
                    };
                    textBox.SetBinding(TextBox.IsEnabledProperty, b);
                    textBox2.SetBinding(TextBox.IsEnabledProperty, b);
                    _configControl.LayoutUpdated += _configControl_LayoutUpdated;
                }
            }
            void _configControl_LayoutUpdated(object sender, EventArgs e)
            {
                unloadedServices = _configControl.FindName(_unloadedServices) as ListView;
                if (unloadedServices != null && unloadedServices.Visibility == System.Windows.Visibility.Visible)
                {
                    _configControl.LayoutUpdated -= _configControl_LayoutUpdated;
                    loadedServices = _configControl.FindName(_loadedServices) as ListView;
                    mainGrid = VisualTreeHelper.GetParent(unloadedServices) as Grid;
                    Button btn = _configControl.FindName(_addButton) as Button;
                    TabControl tabs = _configControl.FindName("ContentTabControl") as TabControl;
                    mainGrid.RowDefinitions.Insert(1, new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(unloadedServices, 2);
                    Grid.SetRow(loadedServices, 2);
                    Grid.SetRow(btn.Parent as Grid, 2);
                    mainGrid.Children.Add(textBox);
                    Grid.SetRow(textBox, 1);
                    Grid.SetColumn(textBox, 0);
                    mainGrid.Children.Add(textBox2);
                    Grid.SetRow(textBox2, 1);
                    Grid.SetColumn(textBox2, 2);

                    Style style = new Style(typeof(GridSplitter));
                    style.Setters.Add(new Setter(GridSplitter.BackgroundProperty, Brushes.Gray));
                    Trigger tr = new Trigger();
                    tr.Value = true;
                    tr.Property = GridSplitter.IsMouseOverProperty;
                    tr.Setters.Add(new Setter(GridSplitter.BackgroundProperty, Brushes.Black));
                    style.Triggers.Add(tr);
                    Trigger tr2 = new Trigger();
                    tr2.Value = true;
                    tr2.Property = GridSplitter.IsDraggingProperty;
                    tr2.Setters.Add(new Setter(GridSplitter.WidthProperty, 1.0));
                    tr2.Setters.Add(new Setter(GridSplitter.BorderThicknessProperty, new Thickness(0)));
                    tr2.Setters.Add(new Setter(GridSplitter.MarginProperty, new Thickness(0)));
                    style.Triggers.Add(tr2);

                    GridSplitter splitter = new GridSplitter()
                    {
                        Opacity = 0.5,
                        Width = 14,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Style = style,
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(4, 0, 8, 0),
                        ShowsPreview = true,
                        Margin = new Thickness(0, 0, 0, 15),
                        ToolTip = "Double-click to reset",
                    };
                    ToolTipService.SetInitialShowDelay(splitter, 200);
                    splitter.MouseDoubleClick += Splitter_MouseDoubleClick;
                    GridSplitter splitter2 = new GridSplitter()
                    {
                        Opacity = 0.5,
                        Width = 14,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Style = style,
                        BorderBrush = Brushes.Transparent,
                        BorderThickness = new Thickness(8, 0, 4, 0),
                        ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                        ShowsPreview = true,
                        Margin = new Thickness(0, 0, 0, 15),
                        ToolTip = "Double-click to reset",
                    };
                    ToolTipService.SetInitialShowDelay(splitter2, 200);
                    splitter2.MouseDoubleClick += Splitter_MouseDoubleClick;
                    mainGrid.Children.Add(splitter);
                    mainGrid.Children.Add(splitter2);
                    Grid.SetRow(splitter, 2);
                    Grid.SetRow(splitter2, 2);
                    Grid.SetColumn(splitter, 1);
                    Grid.SetColumn(splitter2, 1);

                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(unloadedServices.ItemsSource);
                    view.Filter = Filter;
                    CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(loadedServices.ItemsSource);
                    view2.Filter = Filter2;
                }
            }
            void Splitter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                mainGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                mainGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            void Tb_TextChanged(object sender, TextChangedEventArgs e)
            {
                CollectionViewSource.GetDefaultView(unloadedServices.ItemsSource).Refresh();
                CollectionViewSource.GetDefaultView(loadedServices.ItemsSource).Refresh();
            }
            bool Filter(object sender)
            {
                return
                    string.IsNullOrEmpty(textBox.Text) ||
                    textBox.Text == _search ||
                     (sender as FabricationPartBrowser.Modules.Service).Name.ToLower().Contains(textBox.Text.ToLower());
            }
            bool Filter2(object sender)
            {
                return
                    string.IsNullOrEmpty(textBox2.Text) ||
                    textBox2.Text == _search ||
                     (sender as FabricationPartBrowser.Modules.Service).Name.ToLower().Contains(textBox2.Text.ToLower());
            }
        }
        public class ItemFoldersTreeView : TreeView
        {
            public static readonly DependencyProperty SelectedTreeViewItem_Property = DependencyProperty.Register(nameof(SelectedTreeViewItem), typeof(Item), typeof(ItemFoldersTreeView), (PropertyMetadata)new UIPropertyMetadata((PropertyChangedCallback)null));

            public ItemFoldersTreeView() => this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(this.SelectedTreeViewItemChanged);

            void SelectedTreeViewItemChanged(
              object sender,
              RoutedPropertyChangedEventArgs<object> e)
            {
                if (this.SelectedItem == null)
                    return;
                this.SetValue(ItemFoldersTreeView.SelectedTreeViewItem_Property, this.SelectedItem);
            }

            public Item SelectedTreeViewItem
            {
                get => this.GetValue(ItemFoldersTreeView.SelectedTreeViewItem_Property) as Item;
                set => this.SetValue(ItemFoldersTreeView.SelectedTreeViewItem_Property, (object)value);
            }
        }
    }
}