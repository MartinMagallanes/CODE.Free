using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Reflection;
using Color = Autodesk.Revit.DB.Color;

namespace CODE.Free
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
        public static FillPatternElement GetSolidPattern(this Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().FirstOrDefault(x => x.GetFillPattern().IsSolidFill);
        }
        public static OverrideGraphicSettings SetColorInView(this ICollection<ElementId> ids, View view, Color surfaceColor, Color lineColor)
        {
            FillPatternElement pattern = view.Document.GetSolidPattern();
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            if (surfaceColor != null)
            {
                ogs.SetSurfaceForegroundPatternColor(surfaceColor);
                ogs.SetSurfaceForegroundPatternVisible(true);
                ogs.SetSurfaceForegroundPatternId(pattern.Id);
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
    }
    public static class UI
    {
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
            //if (INI.IsAdmin())
            //{
            Popup(msg);
            //}
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