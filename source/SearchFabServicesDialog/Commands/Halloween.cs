using Autodesk.Internal.Windows;
using Autodesk.Internal.Windows.ToolBars;
using Autodesk.Revit.Attributes;
using Autodesk.Windows;
using Nice3point.Revit.Toolkit.External;
using System.Windows.Media;
using CODE.Free.Utils;
using UIFramework;
namespace CODE.Free
{
    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Halloween : ExternalCommand
    {
        static Theme SavedTheme;
        static RibbonTheme SavedRibbonTheme;
        public override void Execute()
        {
            //this.Hello();
            try
            {
                //Autodesk.Private.Windows.ToolBars.ToolBarTrayControl
                //Autodesk.Internal.Windows.ToolBars.ToolBarTheme

                //string filePath = @"B:\01-CO\02-Addins\01-CODE\02-Published\CODE.Free\source\SearchFabServicesDialog\Resources\Halloween.xaml";
                //if (!File.Exists(filePath))
                //{
                //    filePath = @"B:\01-CO\02-Addins\01-CODE\02-Published\CODE.Free\source\SearchFabServicesDialog\Resources\Halloween.xaml";
                //}
                //if (File.Exists(filePath))
                //{
                //    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                //    {
                //        ResourceDictionary halloweenDictionary = (ResourceDictionary)XamlReader.Load(fs);
                //        ResourceDictionary revitThemeDictionary = halloweenDictionary["RevitThemeDictionary2"] as ResourceDictionary;
                //        if (revitThemeDictionary != null)
                //        {
                //            ApplicationTheme applicationTheme = revitThemeDictionary["Dark3"] as ApplicationTheme;
                //            if (applicationTheme != null)
                //            {
                //                ApplicationTheme.CurrentTheme = applicationTheme;
                //                //ComponentManager.CurrentTheme.Ribbon = applicationTheme.RibbonTheme;
                //                UI.Popup("Halloween theme applied.");
                //            }
                //                UI.Popup("Halloween theme applied2.");
                //        }
                //        else
                //        {
                //            UI.Popup("Failed to load RevitThemeDictionary2.");
                //        }
                //    }
                //}

                //this.RibbonTheme.Ribbon.PanelBarBackground = this.ChromeBackgroundGradientBrush;
                //this.RibbonTheme.Ribbon.TabBarBackground = this.ChromeBackgroundBrush;
                //this.RibbonTheme.Ribbon.TabBarBorder = this.ChromeBackgroundBrush;
                //this.RibbonTheme.Ribbon.ItemStyle.GalleryBackgroundFillBrush = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelContentBackground = this.RibbonPanelBackgroundGradientBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelBackground = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelBackgroundVerticalLeft = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelBackgroundVerticalRight = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelTitleBorderBrushVertical = Brushes.Transparent;
                //this.RibbonTheme.Ribbon.MainTab.PanelTitleBackground = this.RibbonPanelBackgroundGradientBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelSeparatorBrush = this.RibbonPanelSeparatorBrush;
                //this.RibbonTheme.Ribbon.MainTab.PanelBorder = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.InnerBorder = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.OuterBorder = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.TabHeaderForeground = this.ChromeForegroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.TabHeaderBackground = this.RibbonPanelBackgroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.SelectedTabHeaderBorder = this.RibbonPanelBackgroundGradientBrush;
                //this.RibbonTheme.Ribbon.MainTab.SelectedTabHeaderBackground = this.RibbonPanelBackgroundGradientBrush;
                //this.RibbonTheme.Ribbon.MainTab.RolloverTabHeaderForeground = this.ChromeForegroundBrush;
                //this.RibbonTheme.Ribbon.MainTab.RolloverTabHeaderBackground = this.RibbonTabRolloverBrush;
                //this.RibbonTheme.Ribbon.MainTab.RolloverTabHeaderBorder = this.RibbonTabRolloverBrush;
                //this.RibbonTheme.Ribbon.MainTab.SlideoutPanelSeparatorColor = Brushes.Transparent;
                //this.RibbonTheme.ToolbarItem.QuickAccessToolBarItemStyle.LoginTextForeground = this.ChromeForegroundBrush;

                //return;

                System.Windows.Media.Color oldChrome = new System.Windows.Media.Color() { A = 255, R = 153, G = 153, B = 153 };


                System.Windows.Media.Color gray = new System.Windows.Media.Color() { A = 255, R = 48, G = 48, B = 48 };
                System.Windows.Media.Color darkGray = new System.Windows.Media.Color() { A = 255, R = 37, G = 37, B = 37 };
                System.Windows.Media.Color orange = new System.Windows.Media.Color() { A = 255, R = 255, G = 165, B = 0 };
                System.Windows.Media.Color purple = new System.Windows.Media.Color() { A = 255, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadePurple = new System.Windows.Media.Color() { A = 50, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadeOrange = new System.Windows.Media.Color() { A = 50, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadeWhite = new System.Windows.Media.Color() { A = 50, R = 255, G = 255, B = 255 };
                Brush grayBrush = new SolidColorBrush(gray);
                Brush darkGrayBrush = new SolidColorBrush(Colors.Red);
                Brush orangeBrush = new SolidColorBrush(orange);
                Brush purpleBrush = new SolidColorBrush(purple);
                Brush fadePurpleBrush = new SolidColorBrush(fadePurple);
                Brush fadeOrangeBrush = new SolidColorBrush(fadeOrange);
                Brush fadeWhiteBrush = new SolidColorBrush(fadeWhite);

                if (SavedRibbonTheme != null)
                {
                    UI.Popup("reloaded");
                    ComponentManager.CurrentTheme = SavedTheme;
                    ComponentManager.CurrentTheme.Ribbon = SavedRibbonTheme;
                    SavedTheme = null;
                    SavedRibbonTheme = null;
                    return;
                }
                SavedRibbonTheme = ComponentManager.CurrentTheme.Ribbon.CloneCurrentValue() as RibbonTheme;
                //Autodesk.Windows.ComponentManager at C:\Program Files\Autodesk\Revit 2024\AdWindows.dll
                SavedTheme = ComponentManager.CurrentTheme.CloneCurrentValue() as Theme;
                UI.Popup($"savedribbontheme:{SavedRibbonTheme != null}\nsavedtheme:{SavedTheme != null}");
                RibbonTheme ribbonTheme = ComponentManager.CurrentTheme.Ribbon;
                ribbonTheme.TabBarBackground = darkGrayBrush;
                ribbonTheme.TabOverflowArrowIdleBrush = orangeBrush;
                ribbonTheme.ItemStyle = new RibbonItemStyle() { ItemStyleProperties = new ItemStyleProperties() };
                //ribbonTheme.ItemStyle.ActiveButtonBackgroundBrush = darkGrayBrush;
                //ribbonTheme.ItemStyle.ActiveButtonBorderBrush = orangeBrush;
                //ribbonTheme.ItemStyle.RollOverActiveButtonBackgroundBrush = fadePurpleBrush;
                //ribbonTheme.ItemStyle.RollOverActiveButtonBorderBrush = fadeWhiteBrush;

                return;
                TabTheme mainTabTheme = ribbonTheme.MainTab;
                mainTabTheme.PanelBackground =
                    new LinearGradientBrush(
                        new GradientStopCollection() {
                            new GradientStop(System.Windows.Media.Colors.Black, 0.2),
                            new GradientStop(orange, 1.5) }, 90.0);
                mainTabTheme.TabHeaderForeground = orangeBrush;
                mainTabTheme.PanelBorder = Brushes.Transparent;
                mainTabTheme.PanelTitleBackground = mainTabTheme.PanelContentBackground;
                mainTabTheme.PanelContentBackground = fadePurpleBrush;
                //Autodesk.Internal.Windows.Themes.Dark.Ribbon.PanelBarBackground
                //ComponentManager.CurrentTheme.ri.dark

                Autodesk.Windows.RibbonControl ribbon = RevitRibbonControl.RibbonControl;
                ribbon.ApplicationMenuButtonBackgroundBrush = Brushes.Black;
                ribbon.ApplicationMenuButtonHoverBackgroundBrush = grayBrush;
                ribbon.ApplicationMenuButtonPressBackgroundBrush = darkGrayBrush;
                ribbon.ApplicationMenuButtonText = "BOO!";
                ComponentManager.Settings.CurrentTheme.ApplicationMenu.Foreground = darkGrayBrush;
                ComponentManager.FontSettings.ComponentFontFamily = new FontFamily("Josefin Sans");
                ComponentManager.FontSettings.ComponentFontSize = 12;
                ApplicationTheme appTheme = UIFramework.ApplicationTheme.CurrentTheme;
                //appTheme.ChromeBorderColor = oldChrome;

                ToolBarTheme toolBarTheme = ComponentManager.QuickAccessToolBar.Theme;
                toolBarTheme.CurrentBrush = toolBarTheme.ActiveBrush = darkGrayBrush;
                toolBarTheme.InactiveBrush = grayBrush;

                //ToolBarItemTheme toolBarItemTheme = ComponentManager.CurrentTheme.ToolbarItem;

                //rollover doesn't change fg, matches bg to active panel
                TabTheme tabtheme = ribbon.ActiveTab.ThemeInternal;
                //tabtheme.SelectedTabHeaderForeground = Brushes.Orange;
                tabtheme.SelectedTabHeaderBorder = orangeBrush;
                tabtheme.RolloverTabHeaderForeground = Brushes.Green;
                //Autodesk.Private.Windows.ToolBars.ToolBarTrayControl
                //Autodesk.Internal.Windows.ToolBars.ToolBarTheme

                //Themes.defaultRibbonTheme = ComponentManager.CurrentTheme.Ribbon;
                //RibbonTheme ribbonTheme = ComponentManager.CurrentTheme.Ribbon.CloneCurrentValue() as RibbonTheme;
                //ribbonTheme.PanelBarBackground = Brushes.Black;
                ////ComponentManager.CurrentTheme.ri.dark
                ////UIFramework.ApplicationTheme.CurrentTheme.TabRolloverBackgroundColor = System.Windows.Media.Colors.Orange;
                //////RibbonTheme th = new Autodesk.Private.Windows.ComponentSettings().CurrentTheme.Ribbon;
                //Themes.halloweenRibbon = ribbonTheme;
                //ComponentManager.CurrentTheme.Ribbon = ribbonTheme;

                ////UIFramework.ApplicationTheme.CurrentTheme.ActiveTabBackgroundColor = System.Windows.Media.Colors.Green;
                //Themes.defaultTabTheme = RevitRibbonControl.RibbonControl.ActiveTab.ThemeInternal;
                //TabTheme tabTheme = Themes.defaultTabTheme.CloneCurrentValue() as TabTheme;

                ////UI.Test("null: " + (Themes.halloweenTabs == null));
                ////rollover doesn't change fg, matches bg to active panel
                //tabTheme.TabHeaderForeground = Brushes.Orange;
                //tabTheme.SelectedTabHeaderForeground = Brushes.Green;
                //tabTheme.SelectedTabHeaderBorder = Brushes.Orange;
                //tabTheme.RolloverTabHeaderForeground = Brushes.White;
                //Themes.halloweenTabs = tabTheme;
                //RevitRibbonControl.RibbonControl.ActiveTab.ri= tabTheme;
            }
            catch (Exception ex)
            {
                UI.Test(ex.Message);
            }
            //new Autodesk.Internal.Windows.TabTheme().TabHeaderForeground
            //var collection = new Autodesk.Private.Windows.RibbonTabList().Items;
            //foreach (UIFramework.RvtRibbonTab tab in collection)
            //{

            //}
        }
    }
}
