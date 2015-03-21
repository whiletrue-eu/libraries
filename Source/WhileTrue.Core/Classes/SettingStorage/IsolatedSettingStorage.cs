using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.SettingStorage
{
    public class IsolatedSettingStorage 
    {
        private readonly static string baseSettingPath;

        static IsolatedSettingStorage()
        {
            try
            {
                string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string MainAssemblyTitle = GetMainAssemblyTitle();
                string MainAssemblyCompany = GetMainAssemblyCompany();
                string MainAssemblyVersion = GetMainAssemblyVersion();

                baseSettingPath = Path.Combine(ApplicationDataPath, string.Format(@"{0}\{1}\{2}", MakeValidPathName(MainAssemblyCompany), MakeValidPathName(MainAssemblyTitle), MainAssemblyVersion));
            }
            catch(Exception Exception)
            {
                Trace.Fail(string.Format("{0}\n\nStackTrace:\n{1}", Exception.Message, Exception.StackTrace));
                baseSettingPath = Path.GetTempPath();
            }
        }

        private static string GetMainAssemblyVersion()
        {
            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            if (EntryAssembly != null)
            {
                Version AssemblyVersion = EntryAssembly.GetName().Version;
                return string.Format("{0}.{1}", AssemblyVersion.Major, AssemblyVersion.Minor);
            }
            else
            {
                return "-.-";
            }
        }

        private static string GetMainAssemblyCompany()
        {
            AssemblyCompanyAttribute AssemblyTitleAttribute = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCompanyAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Company) == false)
            {
                return MakeValidPathName(AssemblyTitleAttribute.Company);
            }
            else
            {
                return "[unknown]";
            }

        }

        private static string GetMainAssemblyTitle()
        {
            return GetCallingAssemblyTitle(Assembly.GetEntryAssembly());
        }

        private static string GetCallingAssemblyTitle(Assembly assembly)
        {
            AssemblyTitleAttribute AssemblyTitleAttribute = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Title) == false)
            {
                return MakeValidPathName(AssemblyTitleAttribute.Title);
            }
            else
            {
                return "[unknown]";
            }
        }

        private IsolatedSettingStorage()
        {
        }

        public static IFileSettingStore GetFileStore(string name)
        {
            string CallingAssemblyTitle = GetCallingAssemblyTitle(Assembly.GetCallingAssembly());
            string SettingsPath = Path.Combine(baseSettingPath, CallingAssemblyTitle);

            return new FileSettingStore(SettingsPath,name);
        }

        public static ITagValueSettingStore GetTagValueStore(string name)
        {
            try
            {
                string CallingAssemblyTitle = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().DbC_AssureNotNull("AssemblyTitleAttribute must be defined exactly once for the calling assembly").Title;
                string SettingsPath = Path.Combine(baseSettingPath, MakeValidPathName(CallingAssemblyTitle));

                return new TagValueSettingStore(SettingsPath, name);
            }
            catch
            {
                return new TagValueSettingStore(baseSettingPath,name);
            }
        }

        private static string MakeValidPathName(string value)
        {
            return ReplaceChars(value,Path.GetInvalidPathChars(),'_');
        }

        private static string ReplaceChars(string value, char[] invalidChars, char replacement)
        {
            string NewValue = value;
            foreach( char InvalidCharacter in invalidChars )
            {
                NewValue.Replace(InvalidCharacter, replacement);
            }
            return NewValue;
        }


    }
}
