using Autodesk.Internal.Windows;
using Autodesk.Internal.Windows.ToolBars;
using Autodesk.Private.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Windows;
using FabricationPartBrowser.Modules;
using FabricationPartBrowser.Properties;
using Nice3point.Revit.Toolkit.External;
using System.Collections;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Resources;
using UIFramework;
namespace CODE.Free
{
    public static class Themes
    {
        public static RibbonTheme defaultRibbonTheme;
        public static TabTheme defaultTabTheme;
        public static RibbonTheme halloweenRibbon;
        public static TabTheme halloweenTabs;
    }

    /// <summary>
    ///     External command entry point invoked from the Revit interface
    /// </summary>
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class Halloween : ExternalCommand
    {
        private RibbonTheme ribbonTheme;
        private TabTheme tabtheme;
        public override void Execute()
        {
            //CheckIn.Hello(this);
            try
            {
                //Autodesk.Private.Windows.ToolBars.ToolBarTrayControl
                //Autodesk.Internal.Windows.ToolBars.ToolBarTheme

                Uri uri = new Uri("/CODE.Free;component/Resources/Halloween.xaml", UriKind.Relative);
                ResourceDictionary halloweenDictionary = (ResourceDictionary)System.Windows.Application.LoadComponent(uri);
                ResourceDictionary revitThemeDictionary = halloweenDictionary["RevitThemeDictionary2"] as ResourceDictionary;
                if (revitThemeDictionary != null)
                {
                    ApplicationTheme applicationTheme = revitThemeDictionary["Dark2"] as ApplicationTheme;
                    if (applicationTheme != null)
                    {
                        ApplicationTheme.CurrentTheme = applicationTheme;
                    }
                }
                else
                {
                    UI.Popup("Failed to load RevitThemeDictionary2.");
                }
                
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

                System.Windows.Media.Color oldChrome = new System.Windows.Media.Color() { A = 255, R = 153, G = 153, B = 153 };


                System.Windows.Media.Color gray = new System.Windows.Media.Color() { A = 255, R = 48, G = 48, B = 48 };
                System.Windows.Media.Color darkGray = new System.Windows.Media.Color() { A = 255, R = 37, G = 37, B = 37 };
                System.Windows.Media.Color orange = new System.Windows.Media.Color() { A = 255, R = 255, G = 165, B = 0 };
                System.Windows.Media.Color purple = new System.Windows.Media.Color() { A = 255, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadePurple = new System.Windows.Media.Color() { A = 50, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadeOrange = new System.Windows.Media.Color() { A = 50, R = 97, G = 18, B = 155 };
                System.Windows.Media.Color fadeWhite = new System.Windows.Media.Color() { A = 50, R = 255, G = 255, B = 255 };
                Brush grayBrush = new SolidColorBrush(gray);
                Brush darkGrayBrush = new SolidColorBrush(darkGray);
                Brush orangeBrush = new SolidColorBrush(orange);
                Brush purpleBrush = new SolidColorBrush(purple);
                Brush fadePurpleBrush = new SolidColorBrush(fadePurple);
                Brush fadeOrangeBrush = new SolidColorBrush(fadeOrange);
                Brush fadeWhiteBrush = new SolidColorBrush(fadeWhite);

                ribbonTheme = ComponentManager.CurrentTheme.Ribbon;
                ribbonTheme.TabBarBackground = darkGrayBrush;
                ribbonTheme.TabOverflowArrowIdleBrush = orangeBrush;
                ribbonTheme.ItemStyle.ActiveButtonBackgroundBrush = darkGrayBrush;
                ribbonTheme.ItemStyle.ActiveButtonBorderBrush = orangeBrush;
                ribbonTheme.ItemStyle.RollOverActiveButtonBackgroundBrush = fadePurpleBrush;
                ribbonTheme.ItemStyle.RollOverActiveButtonBorderBrush = fadeWhiteBrush;

                TabTheme mainTabTheme = ribbonTheme.MainTab;
                mainTabTheme.PanelBackground =
                    new LinearGradientBrush(
                        new GradientStopCollection() {
                            new GradientStop(System.Windows.Media.Colors.Black, 0.2),
                            new GradientStop(orange, 1.5) }, 90.0);
                mainTabTheme.TabHeaderForeground = orangeBrush;
                mainTabTheme.PanelBorder = Brushes.Transparent;
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
                appTheme.ChromeBorderColor = oldChrome;

                ToolBarTheme toolBarTheme = ComponentManager.QuickAccessToolBar.Theme;
                toolBarTheme.CurrentBrush = toolBarTheme.ActiveBrush = darkGrayBrush;
                toolBarTheme.InactiveBrush = grayBrush;

                ToolBarItemTheme toolBarItemTheme = ComponentManager.CurrentTheme.ToolbarItem;

                //rollover doesn't change fg, matches bg to active panel
                tabtheme = ribbon.ActiveTab.ThemeInternal;
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
