using Autodesk.Private.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Autodesk.UI.Themes;
using CODE.Free.Models;
using CODE.Free.Utils;
using CODE.Free.Views;
using Nice3point.Revit.Toolkit.External;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using UIFramework;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Primitives;
#if (REVIT2025)
                        //2025
#else
#endif
namespace CODE.Free
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class RibbonEditor : ExternalCommand
    {
        Brush orangeBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 163, 54)); // Color #ffa336
        public override void Execute()
        {
#if RELEASE
            this.Hello();
#endif
            // List<Button> tabs = new List<Button>();
            // var mainWindow = UIFramework.MainWindow.getMainWnd();
            // var buttons = mainWindow.FindChildrenByType<Button>();
            // foreach (var button in buttons)
            // {
            //     if (button.Name == "mRibbonTabButton")
            //     {
            //         // button.Visibility = System.Windows.Visibility.Visible;
            //     }
            // }

            // ValidateTabs();
            // return;
            var ribbonControl = UIFramework.RevitRibbonControl.RibbonControl;
            var minimizeButton = ribbonControl.FindChildrenByType<Button>()
                .FirstOrDefault(b => b.Name == "mMinimizeButtonExecute");
            if (minimizeButton != null)
            {
                // Remove any existing EyeToggleButton with the same name
                var existingEyeToggleButtons = ribbonControl.FindChildrenByType<ToggleButtonControl>()
                    .Where(tb => tb.Name == "EyeToggleButton")
                    .ToList();

                foreach (var existingButton in existingEyeToggleButtons)
                {
                    var parent = VisualTreeHelper.GetParent(existingButton) as DockPanel;
                    if (parent != null)
                    {
                        parent.Children.Remove(existingButton);
                    }
                }
                var eyeToggleButton = new ToggleButtonControl
                {
                    Name = "EyeToggleButton",
                    VerticalAlignment = VerticalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    IsTextVisible = true,
                    Foreground = orangeBrush,
                    FontSize = 9,
                    Image = new BitmapImage(new Uri("pack://application:,,,/CODE.Free;component/Resources/Icons/Viewed & Visible-32-Light.png")),
                };

                eyeToggleButton.Checked += (s, e) =>
                {
                    ShowEyes();
                };

                eyeToggleButton.Unchecked += (s, e) =>
                {
                    HideEyes();
                    HideEyes();
                };

                var parentPanel = VisualTreeHelper.GetParent(minimizeButton) as DockPanel;
                if (parentPanel != null)
                {
                    var eyeToggleIndex = parentPanel.Children.IndexOf(minimizeButton);
                    parentPanel.Children.Insert(eyeToggleIndex, eyeToggleButton);
                }
            }

            // var tabs = GetTabs();
            // AddEyeToTabs(tabs);
            // ribbonControl.LockMode = Autodesk.Internal.Windows.RibbonLockModes.None;
        }
        void AddToContextMenu()
        {
            var ribbonControl = UIFramework.RevitRibbonControl.RibbonControl;
            ribbonControl.ContextMenuOpened += (s, e) =>
            {

                var hideTabMenuItem = new MenuItem
                {
                    Header = "Hide Tab",
                    Foreground = orangeBrush
                };

                hideTabMenuItem.Click += (sender, args) =>
                {
                    // var selectedTab = ribbonControl.SelectedTab;
                    // if (selectedTab != null)
                    // {
                    //     selectedTab.IsVisible = false;
                    // }
                };

                if (e.ContextMenu != null)
                {
                    e.ContextMenu.Items.Add(new Separator());
                    e.ContextMenu.Items.Add(hideTabMenuItem);
                }
            };
        }
        void ValidateTabs()
        {
            VisibleFilterHelper.ValidateTabs(RevitRibbonControl.RibbonControl);
            foreach (var tab in RevitRibbonControl.RibbonControl.Tabs)
            {
                if (!CanActivate(tab))
                {
                    tab.IsVisible = false;
                }
                else
                {
                    tab.IsVisible = true;
                }
            }
        }
        bool CanActivate(RibbonTab tab)
        {
            return tab.IsVisible && (!tab.IsContextualTab || !tab.IsMergedContextualTab);
        }
        List<Button> GetTabs()
        {
            var mainWindow = UIFramework.MainWindow.getMainWnd();
            // Use reflection to access the private method and property
            var tabList = mainWindow.FindChildrenByType<RibbonTabList>().FirstOrDefault();
            object overflowPanel = null;
            if (tabList != null)
            {
                var method = typeof(RibbonTabList).GetMethod("FindRibbonTabListOverflowPanel", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (method != null)
                {
                    overflowPanel = method.Invoke(tabList, null);
                }
            }

            // Get the private property RibbonTabList from the overflowPanel
            if (overflowPanel != null)
            {
                var prop = overflowPanel.GetType().GetProperty("RibbonTabList", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                var ribbonTabList = prop?.GetValue(overflowPanel);
                // You can use ribbonTabList as needed here
            }

            List<Button> tabs = mainWindow.FindChildrenByType<Button>()
                .Where(c => c.Name == "mRibbonTabButton").ToList();
            return tabs;
        }
        void ShowEyes()
        {
            ValidateTabs();
            var tabs = GetTabs();
            foreach (var tab in tabs)
            {
                if (tab.Visibility == System.Windows.Visibility.Collapsed)
                {
                    tab.Opacity = 0.5;
                }
                tab.Visibility = System.Windows.Visibility.Visible;
                // Check if the EyeButton already exists and show it
                if (tab.Content is StackPanel stackPanel)
                {
                    var eyeButton = stackPanel.Children
                        .OfType<Button>()
                        .FirstOrDefault(b => b.Name == "EyeButton");
                    if (eyeButton != null)
                    {
                        stackPanel.Children.Remove(eyeButton);
                    }
                    eyeButton = new Button()
                    {
                        Name = "EyeButton",
                        Content = new Image()
                        {
                            Source = new BitmapImage(new Uri("pack://application:,,,/CODE.Free;component/Resources/Icons/Viewed & Visible-32-Light.png")),
                            Width = 16,
                            Height = 16,
                        },
                        FontSize = 12,
                        Background = System.Windows.Media.Brushes.Transparent,
                        BorderBrush = System.Windows.Media.Brushes.Transparent,
                        Foreground = orangeBrush,
                    };
                    stackPanel.Children.Add(eyeButton);
                    eyeButton.Click += EyeButton_Click;
                }
            }
        }
        void HideEyes()
        {
            var tabs = GetTabs();
            ValidateTabs();
            foreach (var tab in tabs)
            {
                tab.Visibility = tab.Opacity == 1.0 ?
                    System.Windows.Visibility.Visible :
                    System.Windows.Visibility.Collapsed;
                if (tab.Content is StackPanel stackPanel)
                {
                    var eyeButton = stackPanel.Children
                        .OfType<Button>()
                        .FirstOrDefault(b => b.Name == "EyeButton");

                    if (eyeButton != null)
                    {
                        stackPanel.Children.Remove(eyeButton);
                        // if (stackPanel.Children.Count == 1)
                        // {
                        //     var content = stackPanel.Children[0];
                        //     tab.Content = null;
                        //     tab.Content = content;
                        // }
                    }
                }
            }
        }
        private void EyeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button eyeButton)
            {
                var parent = VisualTreeHelper.GetParent(eyeButton);
                while (parent != null && !(parent is Button button && button.Name == "mRibbonTabButton"))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                if (parent is Button ribbonTabButton)
                {
                    if (ribbonTabButton.Opacity == 1.0)
                    {
                        ribbonTabButton.Opacity = 0.5;
                    }
                    else
                    {
                        ribbonTabButton.Opacity = 1.0;
                    }
                }
            }
        }

        public static List<T> FindChildrenByType<T>(DependencyObject parent) where T : DependencyObject
        {
            List<T> children = new List<T>();
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    children.Add((T)child);
                }
                else
                {
                    children.AddRange(FindChildrenByType<T>(child));
                }
            }
            return children;
        }
    }
    public class RibbonEditorAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication app, CategorySet selectedCategories)
        {
            // Check if there is an active document open
            return app.ActiveUIDocument != null;
        }
    }
}
