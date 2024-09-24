using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.UI.Windows.Controls;
using FabricationPartBrowser;
using FabricationPartBrowser.Modules;
using FabSettings;
using Nice3point.Revit.Toolkit.External;
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
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            OverrideFabConfigDialog();
            //CreateRibbon();

            //Application.DialogBoxShowing += Application_DialogBoxShowing;
            //Application.FabricationPartBrowserChanged += Application_FabricationPartBrowserChanged;
        }
        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("CADmep", "CODE");

            panel.AddPushButton<FabSettingsV2>("Execute")
                .SetImage("/SearchFabServicesDialog;component/Resources/Icons/RibbonIcon16.png")
                .SetLargeImage("/SearchFabServicesDialog;component/Resources/Icons/RibbonIcon32.png");
        }

        private static void Application_FabricationPartBrowserChanged(object sender, FabricationPartBrowserChangedEventArgs e)
        {
            //MessageBox.Show(e.Operation.ToString());
        }

        private static void Application_DialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            //UI.Test(2);
            //UI.Test(e.DialogId);
            //MessageBox.Show(e.DialogId);
        }
        public static void RegisterEvents()
        {
            Context.UiApplication.DialogBoxShowing -= Application_DialogBoxShowing;
            Context.UiApplication.DialogBoxShowing += Application_DialogBoxShowing;
            Context.UiApplication.FabricationPartBrowserChanged -= Application_FabricationPartBrowserChanged;
            Context.UiApplication.FabricationPartBrowserChanged += Application_FabricationPartBrowserChanged;
        }


        const string _search = "Search";
        const string _toolTip = "Enter text to filter services list";
        const string _isConnected = "IsConfigConnectedToSource";
        const string _unloadedServices = "UnloadedServicesListView";
        const string _loadedServices = "LoadedServicesListView";
        const string _addButton = "Add";
        const string _searchBox = "SearchTextBox";
        public static void OverrideFabConfigDialog()
        {

            try
            {

                FabPartBrowserPage page = typeof(FabPartUtility).GetPrivateMember("FabBrowserPage") as FabPartBrowserPage;
                Button settings = page.FindName("Settings") as Button;
                settings.Content = $"Settings...";
                //settings.Content = $"Settings {DateTime.Now.Second}";
                RemoveRoutedEventHandlers(settings, Button.ClickEvent);
                settings.Click += Settings_Click;
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
        }
        private static void Settings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnButtonSettingsCLick();
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
        }
        public static BitmapSource GetImageFromPath(string path)
        {
            try
            {
                return new BitmapImage(new Uri(path));
            }
            catch
            {
                return null;
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
        public static void OnButtonSettingsCLick()
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
            CollectionViewSource cvs = null;
            CollectionViewSource cvs2 = null;
            Grid g = null;
            FabSettingsDialog _fabSettingsDialog = null;
            FabricationConfigurationUserControl _configControl = null;
            public string GetName() => nameof(UpdateFabSettings);
            public string GetDescription() => "Update Fabrication Settings Dialog.";
            public void Execute(UIApplication revitApp)
            {
                _fabSettingsDialog = null;
                try
                {
                    _fabSettingsDialog = new FabSettingsDialog(GlobalInfo.CurrentDoc);
                    _fabSettingsDialog.Activated += _fabSettingsDialog_Activated;
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
            private void _fabSettingsDialog_Activated(object sender, EventArgs e)
            {
                _fabSettingsDialog.Activated -= _fabSettingsDialog_Activated;
                _configControl = typeof(FabPartUtility).GetPrivateMember("FabConfigControl") as FabricationConfigurationUserControl;
                if (_configControl != null)
                {
                    Button sync = _configControl.FindName("Sync") as Button;
                    FabPartUtility.SimulateButtonClick(sync);
                    //_fabSettingsDialog.Icon = new BitmapImage(new Uri("/SearchFabServicesDialog;component/Resources/Icons/RibbonIcon16.png"));
                    _fabSettingsDialog.Icon = GetImageFromPath(@"B:\04-OC\02-Plugins\99-Resources\Icons\CODE16x.png");
                    UI.Popup("fix this icon path");
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
            private void _configControl_LayoutUpdated(object sender, EventArgs e)
            {
                unloadedServices = _configControl.FindName(_unloadedServices) as ListView;
                if (unloadedServices != null && unloadedServices.Visibility == System.Windows.Visibility.Visible)
                {
                    _configControl.LayoutUpdated -= _configControl_LayoutUpdated;
                    loadedServices = _configControl.FindName(_loadedServices) as ListView;
                    g = VisualTreeHelper.GetParent(unloadedServices) as Grid;
                    Button btn = _configControl.FindName(_addButton) as Button;
                    TabControl tabs = _configControl.FindName("ContentTabControl") as TabControl;
                    //tabs.Items.Insert(2, new ItemFoldersUserControl(FabricationConfiguration.GetFabricationConfiguration(Context.Document)));
                    //TabItem tab = new TabItem();
                    //tab.Header = "Families";
                    ////tab.Content = new Image() { Source = GetImageFromPath(@"B:\04-OC\00-Admin\Media\LI Post\z-Steven\2024-07-14_22-21-37.png") };
                    //tab.Content = new ItemFoldersUserControl(FabricationConfiguration.GetFabricationConfiguration(Context.Document));
                    //tabs.Items.Insert(2, tab);
                    g.RowDefinitions.Insert(1, new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(unloadedServices, 2);
                    Grid.SetRow(loadedServices, 2);
                    Grid.SetRow(btn.Parent as Grid, 2);
                    g.Children.Add(textBox);
                    Grid.SetRow(textBox, 1);
                    Grid.SetColumn(textBox, 0);
                    g.Children.Add(textBox2);
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
                    //tr2.Setters.Add(new Setter(GridSplitter.BackgroundProperty, Brushes.Red));
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
                    };
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
                    };
                    splitter2.MouseDoubleClick += Splitter_MouseDoubleClick;
                    g.Children.Add(splitter);
                    g.Children.Add(splitter2);
                    Grid.SetRow(splitter, 2);
                    Grid.SetRow(splitter2, 2);
                    Grid.SetColumn(splitter, 1);
                    Grid.SetColumn(splitter2, 1);
                    //g.ColumnDefinitions[1].MinWidth = 40;
                    //g.ColumnDefinitions[1].MaxWidth = 40;
                    //treeUnloadedServices = new ServicesTreeView(unloadedServices.ItemsSource)
                    //{
                    //    DataContext = _configControl.DataContext,
                    //};
                    //treeLoadedServices = new ServicesTreeView(loadedServices.ItemsSource)
                    //{
                    //    DataContext = _configControl.DataContext,
                    //};
                    //g.Children.Add(treeUnloadedServices);
                    //g.Children.Add(treeLoadedServices);
                    //Grid.SetColumn(treeUnloadedServices, Grid.GetColumn(unloadedServices));
                    //Grid.SetColumn(treeLoadedServices, Grid.GetColumn(loadedServices));
                    //Grid.SetRow(treeUnloadedServices, Grid.GetRow(unloadedServices));
                    //Grid.SetRow(treeLoadedServices, Grid.GetRow(loadedServices));
                    //loadedServices.Visibility = Visibility.Hidden;
                    //unloadedServices.Visibility = Visibility.Hidden;

                    //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(treeUnloadedServices.ItemsSource);
                    CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(unloadedServices.ItemsSource);
                    view.Filter = Filter;
                    //CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(treeLoadedServices.ItemsSource);
                    CollectionView view2 = (CollectionView)CollectionViewSource.GetDefaultView(loadedServices.ItemsSource);
                    view2.Filter = Filter2;
                }
            }

            private void Splitter_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            {
                g.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                g.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }

            private void Tb_TextChanged(object sender, TextChangedEventArgs e)
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

            private void SelectedTreeViewItemChanged(
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