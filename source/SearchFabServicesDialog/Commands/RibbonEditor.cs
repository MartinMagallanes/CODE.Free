using Autodesk.Private.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Analysis;
using Autodesk.Revit.UI;
using Autodesk.UI.Themes;
using CODE.Free.Models;
using CODE.Free.Utils;
using CODE.Free.Views;
using Nice3point.Revit.Toolkit.External;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UIFramework;
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
        public override void Execute()
        {
#if RELEASE
            this.Hello();
#endif
            // Access the Revit main window
            var tabs = GetTabs();
            UI.Test($"tabs:{tabs.Count}");
            AddEyeToTabs(tabs);
            RevitRibbonControl ribbonControl = UIFramework.RevitRibbonControl.RibbonControl;

            ribbonControl.LockMode = Autodesk.Internal.Windows.RibbonLockModes.None;
            // RibbonEditorViewModel viewModel = new RibbonEditorViewModel();
            // RibbonEditorView view = new RibbonEditorView(viewModel);
            // view.Owner = UIFramework.MainWindow.getMainWnd();
            // view.ShowDialog();
        }
        List<Button> GetTabs()
        {
            List<Button> tabs = new List<Button>();
            var mainWindow = UIFramework.MainWindow.getMainWnd();
            var buttons = mainWindow.FindChildrenByType<Button>();
            foreach (var button in buttons)
            {
                if (button.Name == "mRibbonTabButton")
                {
                    tabs.Add(button);
                }
            }
            return tabs;
        }
        void AddEyeToTabs(List<Button> tabs)
        {
            foreach (var tab in tabs)
            {
                if (tab.Visibility == System.Windows.Visibility.Collapsed)
                {
                    tab.Opacity = 0.5;
                    tab.Visibility = System.Windows.Visibility.Visible;
                }
                // Check if the EyeButton already exists and remove it
                var existingStackPanel = tab.Content as StackPanel;
                if (existingStackPanel != null)
                {
                    var eyeButtonToRemove = existingStackPanel.Children
                        .OfType<Button>()
                        .FirstOrDefault(b => b.Name == "EyeButton");
                    if (eyeButtonToRemove != null)
                    {
                        existingStackPanel.Children.Remove(eyeButtonToRemove);
                    }
                }
                var eyeButton = new Button()
                {
                    Name = "EyeButton",
                    Content = "\uD83D\uDC41\uFE0F", // Unicode for 👁️
                    Background = System.Windows.Media.Brushes.Transparent,
                    BorderBrush = System.Windows.Media.Brushes.Transparent,
                    Foreground = System.Windows.Media.Brushes.White,
                };
                var stackPanel = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
                if (tab.Content is UIElement existingContent)
                {
                    try
                    {
                        tab.Content = null;
                        stackPanel.Children.Add(existingContent);
                    }
                    catch (Exception ex)
                    {
                        UI.Test($"Error adding existing content: {ex.Message}");
                        return;
                    }
                }
                stackPanel.Children.Add(eyeButton);
                tab.Content = stackPanel;
                eyeButton.Click += EyeButton_Click;
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
                        ribbonTabButton.Visibility = System.Windows.Visibility.Collapsed;
                    }
                    else
                    {
                        ribbonTabButton.Opacity = 1.0;
                        ribbonTabButton.Visibility = System.Windows.Visibility.Visible;
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
