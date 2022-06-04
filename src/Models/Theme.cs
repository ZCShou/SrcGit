using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace SrcGit.Models
{
    /// <summary>
    ///  主题
    /// </summary>
    public class Theme
    {
        public string Name
        {
            get;
            set;
        }
        public string Resource
        {
            get;
            set;
        }

        public Theme(string name, string res)
        {
            Name = name;
            Resource = res;
        }

        public static List<Theme> Supported = new List<Theme>()
        {
            new Theme("System", "System"),
            new Theme("Light", "Light"),
            new Theme("Dark", "Dark"),
        };

        public static void Change()
        {
            var theme = Preference.Instance.General.Theme;

            if (theme == "System")
            {
                theme = GetSystemTheme();
            }

            foreach (var rs in App.Current.Resources.MergedDictionaries)
            {
                if (rs.Source != null && rs.Source.OriginalString.StartsWith("pack://application:,,,/Resources/Themes/", StringComparison.Ordinal))
                {
                    rs.Source = new Uri($"pack://application:,,,/Resources/Themes/{theme}.xaml", UriKind.Absolute);
                    break;
                }
            }
        }

        private static string GetSystemTheme()
        {
            var reg = RegistryKey.OpenBaseKey(
              RegistryHive.CurrentUser,
              Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            var git = reg.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");

            if (git != null)
            {
                var str = git.GetValue("SystemUsesLightTheme");

                if (str == null)
                {
                    str = git.GetValue("AppsUseLightTheme");
                }

                if (str != null)
                {
                    if (str.ToString().Equals("1"))
                    {
                        return "Light";
                    }
                    else
                    {
                        return "Dark";
                    }
                }
            }

            return "Light";
        }
    }
}
