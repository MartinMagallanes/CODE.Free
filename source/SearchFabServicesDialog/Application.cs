using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.WPFFramework;
using Autodesk.UI.Windows.Controls;
using FabricationPartBrowser;
using FabricationPartBrowser.Modules;
using FabricationPartBrowser.ViewModels;
using FabSettings;
using Nice3point.Revit.Toolkit.External;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
#if (REVIT2025)
//2025
#else
using System.Windows.Interactivity;
#endif
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
            CheckIn.Hello(this);
            CreateRibbon();
            OverrideFabConfigDialog();
        }
        private void CreateRibbon()
        {
            RevitCommandId cmd = RevitCommandId.LookupCommandId("ID_EXPORT_FABRICATION_PCF");
            if (cmd != null)
            {
                UI.Popup($"can bind:{cmd.CanHaveBinding},has bind:{cmd.HasBinding},id:{cmd.Id},name:{cmd.Name}");
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

            //RibbonPanel panel = Application.CreatePanel(_panelName, _tabName);
            //var btn = panel.AddPushButton<FabSettingsV2>(_btnText)
            //    .SetImage(_img)
            //    .SetLargeImage(_largeImg);
            //btn.LongDescription = _description;

            //var panel = Application.CreatePanel("Commands", "Tag3D");

            //panel.AddPushButton<>("Execute")
            //    .SetImage("/Tag3D;component/Resources/Icons/RibbonIcon16.png")
            //    .SetLargeImage("/Tag3D;component/Resources/Icons/RibbonIcon32.png");
        }
        public override void OnShutdown()
        {
            CheckIn.Goodbye();
            base.OnShutdown();
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
        const string _contentTabControl = "ContentTabControl";
        const string _itemFolders = "UI";
        const string _loadedServices = "LoadedServicesListView";
        const string _addButton = "Add";
        const string _addAllButton = "AddAll";
        const string _removeButton = "Remove";
        const string _removeAllButton = "RemoveAll";
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
                //settings.Content = $"Settings2...";
                DockPanel panel = settings.Parent as DockPanel;
                //Button reload = new Button()
                //{
                //    Content = new Image()
                //    {
                //        Source = GetEmbeddedImage(Assembly.GetExecutingAssembly(), "CODE.Free.Resources.Icons.Refresh.ico"),
                //        //Source = GetImageFromPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources\\Icons", "Refresh.ico")),
                //        Stretch = Stretch.None,
                //    },
                //    Background = Brushes.Transparent,
                //    BorderBrush = Brushes.Transparent,
                //    BorderThickness = new Thickness(0),
                //    HorizontalAlignment = HorizontalAlignment.Left,
                //    HorizontalContentAlignment = HorizontalAlignment.Center,
                //    ToolTip = "Refresh the configuration",
                //    Margin = new Thickness(0),
                //    Padding = new Thickness(2),
                //    VerticalAlignment = VerticalAlignment.Center,
                //    VerticalContentAlignment = VerticalAlignment.Center,
                //};
                Binding visBind = new Binding
                {
                    Path = new PropertyPath("Visibility"),
                    ElementName = "RoutingExclusionsBtn"
                };
                //reload.SetBinding(ButtonBase.VisibilityProperty, visBind);
                Binding enBind = new Binding
                {
                    Path = new PropertyPath("IsEnabled"),
                    ElementName = "RoutingExclusionsBtn"
                };
                //reload.SetBinding(ButtonBase.IsEnabledProperty, enBind);
                //reload.Click += (s, e) =>
                //{
                //    FabricationConfiguration.GetFabricationConfiguration(Context.ActiveDocument).ReloadConfiguration();
                //};
                //ToolTipService.SetInitialShowDelay(reload, 200);

                //panel.Children.Insert(2, reload);
                //DockPanel.SetDock(reload, Dock.Left);
                panel.Children.Remove(settings);
                reloadCheckBox = new CheckBox()
                {
                    Content = "Reload on Open",
                    ToolTip = "Automatically Reload Configuration when the Settings dialog is opened",
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    IsChecked = FabSettingsAutoReload,
                    Visibility = CODE.Free.Application.FabSettingsAutoReload ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
                };
                ToolTipService.SetInitialShowDelay(reloadCheckBox, 200);
                reloadCheckBox.SetBinding(ButtonBase.VisibilityProperty, visBind);
                reloadCheckBox.SetBinding(ButtonBase.IsEnabledProperty, enBind);

                Grid newGrid = new Grid();
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //for reload checkbox
                newGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); //for settings
                newGrid.Children.Add(reloadCheckBox);
                newGrid.Children.Add(settings);
                Grid.SetColumn(reloadCheckBox, 0);
                Grid.SetColumn(settings, 1);
                panel.Children.Insert(panel.Children.Count - 1, newGrid);
                DockPanel.SetDock(newGrid, Dock.Right);
                reloadCheckBox.Unchecked += (s, e) =>
                {
                    FabSettingsAutoReload = false;
                    reloadCheckBox.Visibility = System.Windows.Visibility.Collapsed;
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
            TextBox textBox = null;
            TextBox textBox2 = null;
            CheckBox checkBox = null;
            Grid mainGrid = null;
            FabSettingsDialog _fabSettingsDialog = null;
            FabricationConfigurationUserControl _configControl = null;
            TabControl _tabControl = null;
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
            //private void OnClosing(object sender, CancelEventArgs e)
            //{
            //    if (!ViewModelStore.Instance.ConfigurationsViewModel.CanClose)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //    ViewModelStore.Instance.SettingsViewModel.HandleDialogClose();
            //}
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
                            Visibility = CODE.Free.Application.FabSettingsAutoReload ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed,
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
                    checkBox.SetBinding(TextBox.IsEnabledProperty, b);
                    _configControl.LayoutUpdated += _configControl_LayoutUpdated;
                }
            }
            void SearchAllFiles2(string searchText)
            {
                string text = searchText;
                if (string.IsNullOrWhiteSpace(text))
                {
                    _partsModel.SearchItems.Clear();
                    return;
                }

                text = text.Trim();
                List<string> words = (from x in text.Trim().Split(' ')
                                      select x.Trim() into x
                                      where !string.IsNullOrWhiteSpace(x)
                                      select x).ToList();
                if (words.Count == 0)
                {
                    return;
                }

                IEnumerable<string> source = words.Where((string x) => x.Length >= 1);
                if (!source.Any())
                {
                    return;
                }

                List<ItemFile> matchingFiles = new List<ItemFile>();
                _partsModel.AllItemFiles.ForEach(delegate (ItemFile x)
                {
                    bool flag2 = true;
                    foreach (string item in words)
                    {
                        if (x.IsLoaded || x.Identifier.IndexOf(item, StringComparison.OrdinalIgnoreCase) < 0)
                        //if (x.IsLoaded || x.Name.IndexOf(item, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            flag2 = false;
                            break;
                        }
                    }

                    if (flag2)
                    {
                        matchingFiles.Add(x);
                    }
                });
                List<ItemFolder> matchingFolders = new List<ItemFolder>();
                allItemFolders.ToList().ForEach(delegate (ItemFolder x)
                {
                    bool flag = true;
                    foreach (string item2 in words)
                    {
                        if (x.GetItemFolderPath().IndexOf(item2, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            flag = false;
                            break;
                        }
                    }

                    if (flag)
                    {
                        matchingFolders.Add(x);
                    }
                });
                List<Item> list = new List<Item>();
                list.AddRange(matchingFolders.OrderBy((ItemFolder x) => x.GetItemFolderPath()));
                list.AddRange(matchingFiles.OrderBy((ItemFile x) => x.Name));
                if (list.Any())
                {
                    _partsModel.SearchItems.Clear();
                    list.ForEach(delegate (Item x)
                    {
                        _partsModel.SearchItems.Add(x);
                    });
                }
            }
            HashSet<ItemFolder> allItemFolders
            {
                get
                {
                    if (_partsModel == null)
                    {
                        return new HashSet<ItemFolder>();
                    }
                    return
                        typeof(ItemFoldersViewModel)
                        .GetField("m_allItemFolders", BindingFlags.Instance | BindingFlags.NonPublic)?
                        .GetValue(_partsModel) as HashSet<ItemFolder> ?? new HashSet<ItemFolder>();
                }
            }
            ItemFoldersViewModel _partsModel { get; set; } = null;
            ItemFoldersUserControl _ifuc { get; set; } = null;
            void SearchCommandHandler2(object param)
            {
                string searchText = param as string;
                SearchAllFiles2(searchText);
            }
            private void _tabControl_ItemsTabSelected(object sender, EventArgs e)
            {
                //UI.Popup($"_tabcontrol.selecteditem:{_tabControl.SelectedItem}");  
                if (_tabControl.SelectedItem is TabItem ti && ti.Content is ContentControl cc)
                {
                    _ifuc = cc.Content as ItemFoldersUserControl;
                    if (_ifuc != null)
                    {
                        _tabControl.LayoutUpdated -= _tabControl_ItemsTabSelected;
                        _partsModel = _ifuc.DataContext as ItemFoldersViewModel;
                        SearchTextBox tb = _ifuc.FindName("SearchTextBox") as SearchTextBox;
                        //might need to just replace this SearchTextBox  
                        RemoveRoutedEventHandlers(tb, SearchTextBox.GotFocusEvent);

                        tb.GotFocus += (s, e) =>
                        {
                            SearchAllFiles2(tb.Text);
                        };
#if (REVIT2025)
                        //2025
#else
                        Interaction.GetBehaviors(tb).Add(
                            new EventTriggerBehavior
                            {
                                EventName = "Search",
                                Command = new DelegateCommand(delegate (object param)
                                {
                                    SearchAllFiles2(tb.Text);
                                }),
                            });
#endif

                        //the following updates the item presentation in the listbox
                        //try to write xaml resource and load that instead

                        //ListBox lb = _ifuc.FindName("SearchListBox") as ListBox;

                        // Update the DataTemplate for the items in lb  
                        //DataTemplate itemTemplate = new DataTemplate(typeof(ItemFile));
                        //FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
                        //FrameworkElementFactory rowDef = new FrameworkElementFactory(typeof(RowDefinition));
                        //FrameworkElementFactory rowDef2 = new FrameworkElementFactory(typeof(RowDefinition));
                        //rowDef.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
                        //rowDef2.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
                        //gridFactory.AppendChild(rowDef);
                        //gridFactory.AppendChild(rowDef2);

                        //FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(Label));
                        //FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
                        //image.SetBinding(Image.SourceProperty, new Binding("ImageDirect"));
                        //labelFactory.SetBinding(Label.ContentProperty, new Binding("Identifier"));
                        //labelFactory.SetValue(Grid.RowProperty, 0);
                        //image.SetValue(Grid.RowProperty, 1);
                        //gridFactory.AppendChild(labelFactory);
                        //gridFactory.AppendChild(image);

                        //itemTemplate.VisualTree = gridFactory;
                        //lb.Resources.Add("itemfileTemplate", itemTemplate);
                        //lb.ItemTemplateSelector = new SearchListTemplateSelector()
                        //{
                        //    ItemFileTemplate = itemTemplate,
                        //    ItemFolderTemplate = lb.ItemTemplate,
                        //};
                        //

                        //DataTemplate itemTemplate = new DataTemplate(typeof(ItemFile));
                        //FrameworkElementFactory gridFactory = new FrameworkElementFactory(typeof(Grid));
                        //FrameworkElementFactory rowDef = new FrameworkElementFactory(typeof(RowDefinition));
                        //FrameworkElementFactory colDef = new FrameworkElementFactory(typeof(ColumnDefinition));
                        //rowDef.SetValue(RowDefinition.HeightProperty, new GridLength(20));
                        //colDef.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
                        //gridFactory.AppendChild(rowDef);
                        //gridFactory.AppendChild(rowDef);
                        //gridFactory.AppendChild(colDef);
                        //gridFactory.AppendChild(colDef);
                        //gridFactory.AppendChild(colDef);
                        //gridFactory.SetValue(Grid.ToolTipProperty, new Binding("ToolTip"));
                        //FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
                        //image.SetBinding(Image.SourceProperty, new Binding("ImageDirect"));
                        //image.SetValue(Grid.ColumnProperty, 0);
                        //image.SetValue(Grid.RowProperty, 0);
                        //gridFactory.AppendChild(image);

                        //FrameworkElementFactory labelFactory = new FrameworkElementFactory(typeof(Label));
                        //labelFactory.SetBinding(Label.ContentProperty, new Binding("ToolTip")); //'identifier' is full path
                        //labelFactory.SetValue(Grid.ColumnProperty, 2);
                        //labelFactory.SetValue(Grid.RowProperty, 0);
                        //gridFactory.AppendChild(labelFactory);

                        //FrameworkElementFactory cmb = new FrameworkElementFactory(typeof(SearchComboTextBlock));
                        //cmb.SetBinding(SearchComboTextBlock.InlineCollectionProperty, new Binding() { Converter = lb.FindResource("highlightSearchConverter") as IValueConverter });
                        //
                    }
                }
            }
            void _configControl_LayoutUpdated(object sender, EventArgs e)
            {
                unloadedServices = _configControl.FindName(_unloadedServices) as ListView;
                if (unloadedServices != null && unloadedServices.Visibility == System.Windows.Visibility.Visible)
                {
                    _configControl.LayoutUpdated -= _configControl_LayoutUpdated;
                    _tabControl = _configControl.FindName(_contentTabControl) as TabControl;
                    _tabControl.LayoutUpdated += _tabControl_ItemsTabSelected;

                    loadedServices = _configControl.FindName(_loadedServices) as ListView;
                    mainGrid = VisualTreeHelper.GetParent(unloadedServices) as Grid;
                    Button addBtn = _configControl.FindName(_addButton) as Button;
                    Button removeBtn = _configControl.FindName(_removeButton) as Button;
                    addBtn.Margin = new Thickness(0);
                    removeBtn.Margin = new Thickness(0, 10, 0, 0);
                    Grid addRemoveGrid = addBtn.Parent as Grid;

                    Style addAllStyle = new Style(typeof(Button));
                    addAllStyle.BasedOn = addBtn.Style;
                    addAllStyle.Setters.Add(new Setter(Button.IsEnabledProperty, true));
                    Style removeAllStyle = new Style(typeof(Button));
                    removeAllStyle.BasedOn = addBtn.Style;
                    removeAllStyle.Setters.Add(new Setter(Button.IsEnabledProperty, true));
                    DataTrigger dt = new DataTrigger()
                    {
                        Value = 0,
                        Binding = new Binding
                        {
                            Path = new PropertyPath("Items.Count"),
                            ElementName = _unloadedServices,
                        },
                        Setters = { new Setter(Button.IsEnabledProperty, false) }
                    };
                    DataTrigger dt2 = new DataTrigger()
                    {
                        Value = 0,
                        Binding = new Binding
                        {
                            Path = new PropertyPath("Items.Count"),
                            ElementName = _loadedServices,
                        },
                        Setters = { new Setter(Button.IsEnabledProperty, false) }
                    };
                    addAllStyle.Triggers.Add(dt);
                    removeAllStyle.Triggers.Add(dt2);
                    Button addAllButton = new Button()
                    {
                        Name = _addAllButton,
                        Width = addBtn.Width,
                        Height = addBtn.Height,
                        Foreground = addBtn.Foreground,
                        Background = addBtn.Background,
                        BorderBrush = addBtn.BorderBrush,
                        BorderThickness = addBtn.BorderThickness,
                        Padding = addBtn.Padding,
                        Margin = new Thickness(0, 35, 0, 0),
                        Style = addAllStyle,
                        ToolTip = "Add All",
                        Content = new Image()
                        {
                            Source = GetEmbeddedImage(Assembly.GetExecutingAssembly(), "CODE.Free.Resources.Icons.AddAll.png"),
                            Stretch = Stretch.None,
                        },
                    };
                    addAllButton.Click += (s, e) =>
                    {
                        unloadedServices.SelectAll();
                        FabPartUtility.SimulateButtonClick(addBtn);
                        ReloadFilters();
                    };
                    Button removeAllBtn = new Button()
                    {
                        Name = _removeAllButton,
                        Width = addBtn.Width,
                        Height = addBtn.Height,
                        Foreground = addBtn.Foreground,
                        Background = addBtn.Background,
                        BorderBrush = addBtn.BorderBrush,
                        BorderThickness = addBtn.BorderThickness,
                        Padding = addBtn.Padding,
                        Margin = new Thickness(0, 10, 0, 0),
                        Style = removeAllStyle,
                        ToolTip = "Remove All",
                        Content = new Image()
                        {
                            Source = GetEmbeddedImage(Assembly.GetExecutingAssembly(), "CODE.Free.Resources.Icons.RemoveAll.png"),
                            Stretch = Stretch.None,
                        },
                    };
                    removeAllBtn.Click += (s, e) =>
                    {
                        loadedServices.SelectAll();
                        FabPartUtility.SimulateButtonClick(removeBtn);
                        ReloadFilters();
                    };
                    ToolTipService.SetInitialShowDelay(addAllButton, 200);
                    ToolTipService.SetInitialShowDelay(removeAllBtn, 200);
                    ToolTipService.SetShowOnDisabled(addAllButton, true);
                    ToolTipService.SetShowOnDisabled(removeAllBtn, true);
                    addRemoveGrid.RowDefinitions.Insert(2, new RowDefinition() { Height = GridLength.Auto });
                    addRemoveGrid.RowDefinitions.Insert(3, new RowDefinition() { Height = GridLength.Auto });
                    addRemoveGrid.Children.Insert(2, addAllButton);
                    addRemoveGrid.Children.Insert(3, removeAllBtn);
                    Grid.SetRow(addAllButton, 2);
                    Grid.SetRow(removeAllBtn, 3);
                    TabControl tabs = _configControl.FindName("ContentTabControl") as TabControl;
                    mainGrid.RowDefinitions.Insert(1, new RowDefinition() { Height = GridLength.Auto });
                    Grid.SetRow(unloadedServices, 2);
                    Grid.SetRow(loadedServices, 2);
                    Grid.SetRow(addRemoveGrid, 2);
                    mainGrid.Children.Add(textBox);
                    Grid.SetRow(textBox, 1);
                    Grid.SetColumn(textBox, 0);
                    mainGrid.Children.Add(textBox2);
                    Grid.SetRow(textBox2, 1);
                    Grid.SetColumn(textBox2, 2);

                    mainGrid.ColumnDefinitions[0].MinWidth = 130;
                    mainGrid.ColumnDefinitions[2].MinWidth = 130;

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
                ReloadFilters();
            }
            void ReloadFilters()
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

#if (REVIT2025)
//2025
#else
    //implement Behavior
    public class EventTriggerBehavior : Behavior<FrameworkElement>
    {
        public string EventName { get; set; }
        public DelegateCommand Command { get; set; }

        protected override void OnAttached()
        {
            EventInfo eventInfo = AssociatedObject.GetType().GetEvent(EventName);
            if (eventInfo == null)
                throw new InvalidOperationException($"Could not find any event named {EventName} on associated object.");
            MethodInfo methodInfo = GetType().GetMethod("ExecuteCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            Delegate del = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
            eventInfo.AddEventHandler(AssociatedObject, del);
        }
        void ExecuteCommand(object sender, EventArgs e)
        {
            if (Command.CanExecute(null))
                Command.Execute(null);
        }
    }
#endif
    class SearchListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemFileTemplate { get; set; }
        public DataTemplate ItemFolderTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ItemFile)
            {
                return ItemFileTemplate;
            }
            else if (item is ItemFolder)
            {
                return ItemFolderTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
    public static class CheckIn
    {
        const string _hello = "https://licensing.contentorigin.dev/api/Hello";
        const string _goodbye = "https://licensing.contentorigin.dev/api/Hello/goodbye";
        const string _appJson = "application/json";
        public static List<string> Addins = new List<string>();
        static HttpClient _client = new HttpClient();
        public static void Hello(object type)
        {
            try
            {
                string addin = type.GetType().FullName;
                if (!Addins.Contains(addin))
                {
                    Addins.Add(addin);
                }
                _client.PostAsync(_hello, new StringContent(Serialize(Context.Application.Username, addin), Encoding.UTF8, _appJson));
            }
            catch { }
        }
        public static void Goodbye()
        {
            try
            {
                foreach (string addin in Addins)
                {
                    _client.PostAsync(_goodbye, new StringContent(Serialize(Context.Application.Username, addin), Encoding.UTF8, _appJson));
                }
            }
            catch { }
        }
        static string Serialize(string userName, string productId)
        {
            return
            $"{{\"AutodeskUsername\":\"{userName}\"," +
            $"\"ProductIds\":[\"{productId}\"]," +
            $"\"ActiveProductIds\":[\"{productId}\"]}}";
        }
    }
}