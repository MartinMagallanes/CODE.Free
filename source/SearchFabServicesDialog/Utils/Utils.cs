using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using Color = Autodesk.Revit.DB.Color;
using System.IO;
using Nice3point.Revit.Toolkit.External;
using System.Net.Http;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CODE.Free.Utils
{
    public static class Extensions
    {
        public static BitmapSource GetImageFromPath(this string path)
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
        public static T FindChildByName<T>(this DependencyObject parent, string name) where T : DependencyObject
        {
            if (parent == null)
                return null;
            if (parent is T t && (t as FrameworkElement)?.Name == name)
                return t;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                T result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
        public static List<T> FindChildrenByType<T>(this DependencyObject parent) where T : DependencyObject
        {
            var result = new List<T>();
            if (parent == null)
                return result;
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    result.Add(t);
                result.AddRange(FindChildrenByType<T>(child));
            }
            return result;
        }
        public static T FindFirstChild<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                return default(T);
            var obj2 = default(T);
            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, childIndex);
                if (child is not T)
                    obj2 = parent.FindFirstChild<T>();
                if ((object)obj2 != null)
                    break;
            }
            return obj2;
        }
        public static object GetPrivateMember(this Type type, string propertyName)
        {
            foreach (MemberInfo mi in type.GetMembers(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (mi.Name == propertyName)
                {
                    if (mi is FieldInfo fi)
                    {
                        return fi.GetValue(null);
                    }
                    if (mi is PropertyInfo pi)
                    {
                        return pi.GetValue(null);
                    }
                }
            }
            return null;
        }
        public static object GetPrivateMember(this Type type, object instance, string propertyName)
        {
            foreach (MemberInfo mi in type.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (mi.Name == propertyName)
                {
                    if (mi is FieldInfo fi)
                    {
                        return fi.GetValue(instance);
                    }
                    else if (mi is PropertyInfo pi)
                    {
                        return pi.GetValue(instance);
                    }
                    else if (mi is MethodBase me)
                    {
                        return me;
                    }
                }
            }
            return null;
        }
        public static bool IsColorTooLightOrDark(this System.Drawing.Color color)
        {
            double luminance = 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B;
            return luminance < 60 || luminance > 215;
        }
        public static ElementId GetSolidFillPattern(this Document doc)
        {
            Element pattern = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().FirstOrDefault(x => x.GetFillPattern().IsSolidFill);
            if (pattern == null)
            {
                return ElementId.InvalidElementId;
            }
            return pattern.Id;
        }
        public static OverrideGraphicSettings SetColorInView(this ICollection<ElementId> ids, View view, Color surfaceColor, Color lineColor)
        {
            ElementId patternId = view.Document.GetSolidFillPattern();
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            if (surfaceColor != null)
            {
                ogs.SetSurfaceForegroundPatternColor(surfaceColor);
                ogs.SetSurfaceForegroundPatternVisible(true);
                ogs.SetSurfaceForegroundPatternId(patternId);
            }
            if (lineColor != null)
            {
                ogs.SetProjectionLineColor(lineColor);
                ogs.SetProjectionLinePatternId(LinePatternElement.GetSolidPatternId());
            }
            foreach (ElementId id in ids)
            {
                view.SetElementOverrides(id, ogs);
            }
            return ogs;
        }
        public static BitmapSource GetEmbeddedImage(this Assembly assem, string name)
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
        public static string LegalizeString(this string str)
        {
            if (NamingUtils.IsValidName(str)) return str;
            char[] illegalChars = @"[\:{}[]|;<>?`~".ToCharArray();
            foreach (char c in illegalChars)
            {
                str = str.Replace(c, '.');
            }
            return str;
        }
        public static void PerformClick(this ButtonBase sourceButton)
        {
            // Check parameters
            if (sourceButton == null)
                throw new ArgumentNullException(nameof(sourceButton));

            // 1.) Raise the Click-event
            sourceButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

            // 2.) Execute the command, if bound and can be executed
            ICommand boundCommand = sourceButton.Command;
            if (boundCommand != null)
            {
                object parameter = sourceButton.CommandParameter;
                if (boundCommand.CanExecute(parameter) == true)
                    boundCommand.Execute(parameter);
            }
        }
        public static XYZ NormalXY(this Line line)
        {
            XYZ p = line.GetEndPoint(0);
            XYZ q = line.GetEndPoint(1);
            XYZ v = q - p;
            return v.CrossProduct(XYZ.BasisZ).Normalize();
        }
        public static XYZ NormalXZ(this Line line)
        {
            XYZ p = line.GetEndPoint(0);
            XYZ q = line.GetEndPoint(1);
            XYZ v = q - p;
            return v.CrossProduct(XYZ.BasisY).Normalize();
        }
        public static XYZ NormalYZ(this Line line)
        {
            XYZ p = line.GetEndPoint(0);
            XYZ q = line.GetEndPoint(1);
            XYZ v = q - p;
            return v.CrossProduct(XYZ.BasisX).Normalize();
        }
        public static bool IsVertical(this XYZ vector)
        {
            return vector.CrossProduct(XYZ.BasisZ).IsZeroLength();
        }
        public static List<Connector> GetConnectors(this FabricationPart fp)
        {
            return fp.ConnectorManager.Connectors.Cast<Connector>().ToList();
        }
    }
    public static class CheckIn
    {
        const string _hello = "https://licensing.contentorigin.dev/api/Hello";
        const string _goodbye = "https://licensing.contentorigin.dev/api/Hello/goodbye";
        const string _appJson = "application/json";
        public static List<string> Addins = new List<string>();
        static HttpClient _client = new HttpClient();
        /// <summary>
        /// appName is result from type.GetType().FullName
        /// userName is result from Context.Application.Username
        /// </summary>
        /// <param name="appName"></param>
        /// <param name="userName"></param>
        public static void Hello(string appName, string userName)
        {
            try
            {
                if (!Addins.Contains(appName))
                {
                    Addins.Add(appName);
                    _client.PostAsync(_hello, new StringContent(Serialize(userName, appName), Encoding.UTF8, _appJson));
                }
            }
            catch { }
        }
        public static void Hello(this ExternalCommand type)
        {
            try
            {
                string addin = type.GetType().FullName;
                if (!Addins.Contains(addin))
                {
                    Addins.Add(addin);
                    _client.PostAsync(_hello, new StringContent(Serialize(type.Application.Username, addin), Encoding.UTF8, _appJson));
                }
            }
            catch { }
        }
        public static void Goodbye(this ExternalApplication type)
        {
            try
            {
                foreach (string addin in Addins)
                {
                    _client.PostAsync(_goodbye, new StringContent(Serialize(type.UiApplication.Application.Username, addin), Encoding.UTF8, _appJson));
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
    public static class UI
    {
        const bool _debugging = true;
        //const bool _debugging = false;
        private static string AsString(object str)
        {
            string res = "";
            if (str != null)
            {
                if (str is IEnumerable<string> strs)
                {
                    res = Strcat(strs);
                }
                else if (str is IEnumerable<object> objs)
                {
                    res = Strcat(objs.Select(o => o.ToString()));
                }
                else
                {
                    res = str.ToString();
                }
            }
            return res;
        }

        public static void Popup(object title, object msg)
        {
            if (title == null)
            {
                title = "Popup";
            }
            TaskDialog.Show(AsString(title), AsString(msg));
        }
        public static void Popup(object msg)
        {
            Popup(null, AsString(msg));
        }
        public static void Test(object msg)
        {
            if (_debugging)
            {
                DevOutput.Application._outputWindow.Output += "\n";
                DevOutput.Application._outputWindow.Output += AsString(msg);
            }
        }
        public static string Strcat(IEnumerable<string> strs)
        {
            string s = null;
            foreach (string str in strs)
            {
                s += "\n" + str;
            }
            return s;
        }
        public static string Strcat(params string[] strs)
        {
            string s = null;
            for (int i = 0; i < strs.Length; i++)
            {
                s += strs[i];
            }
            return s;
        }
        static Stopwatch _stopwatch = null;
        static bool StopWatchLogActive = false;
        public static void StartTimer()
        {
            if (_stopwatch == null)
            {
                _stopwatch = Stopwatch.StartNew();
            }
            else
            {
                _stopwatch.Restart();
            }
        }
        public static void StartTimer(bool Log)
        {
            StopWatchLogActive = Log;
            StartTimer();
        }
        public static string TimerElapsed()
        {
            if (_stopwatch == null)
            {
                return "Stopwatch has not been initialized.";
            }
            return _stopwatch.Elapsed.ToString();
        }
        public static string TimerElapsed(string message)
        {
            if (_stopwatch == null)
            {
                return "Stopwatch has not been initialized.";
            }
            if (StopWatchLogActive)
            {
                //Log(_stopwatch.Elapsed.ToString() + $": {message}");
            }
            return _stopwatch.Elapsed.ToString();
        }
        public static string StopTimer()
        {
            if (_stopwatch != null)
            {
                var str = TimerElapsed();
                _stopwatch.Stop();
                _stopwatch = null;
                return str;
            }
            else
            {
                return "Stopwatch has not been initialized.";
            }
        }
    }
}