using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.SettingStorage
{
    /// <summary>
    /// Provides key-value or file based setting storage within isolated storage
    /// </summary>
    [PublicAPI]
    public static class IsolatedSettingStorage 
    {
        private static readonly string baseSettingPath;

        static IsolatedSettingStorage()
        {
            try
            {
                string ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string MainAssemblyTitle = IsolatedSettingStorage.GetMainAssemblyTitle();
                string MainAssemblyCompany = IsolatedSettingStorage.GetMainAssemblyCompany();
                string MainAssemblyVersion = IsolatedSettingStorage.GetMainAssemblyVersion();

                IsolatedSettingStorage.baseSettingPath = Path.Combine(ApplicationDataPath, $@"{IsolatedSettingStorage.MakeValidPathName(MainAssemblyCompany)}\{IsolatedSettingStorage.MakeValidPathName(MainAssemblyTitle)}\{MainAssemblyVersion}");
            }
            catch(Exception Exception)
            {
                Trace.Fail($"{Exception.Message}\n\nStackTrace:\n{Exception.StackTrace}");
                IsolatedSettingStorage.baseSettingPath = Path.GetTempPath();
            }
        }

        private static string GetMainAssemblyVersion()
        {
            Assembly EntryAssembly = Assembly.GetEntryAssembly();
            if (EntryAssembly != null)
            {
                Version AssemblyVersion = EntryAssembly.GetName().Version;
                return $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}";
            }
            else
            {
                return "-.-";
            }
        }

        private static string GetMainAssemblyCompany()
        {
            AssemblyCompanyAttribute AssemblyTitleAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Company) == false)
            {
                return IsolatedSettingStorage.MakeValidPathName(AssemblyTitleAttribute.Company);
            }
            else
            {
                return "[unknown]";
            }

        }

        private static string GetMainAssemblyTitle()
        {
            return IsolatedSettingStorage.GetCallingAssemblyTitle(Assembly.GetEntryAssembly());
        }

        private static string GetCallingAssemblyTitle(Assembly assembly)
        {
            AssemblyTitleAttribute AssemblyTitleAttribute = assembly?.GetCustomAttribute<AssemblyTitleAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Title) == false)
            {
                return IsolatedSettingStorage.MakeValidPathName(AssemblyTitleAttribute.Title);
            }
            else
            {
                return "[unknown]";
            }
        }

        /// <summary>
        /// Returns a setting store that allows to write/read files
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFileSettingStore GetFileStore(string name)
        {
            string CallingAssemblyTitle = IsolatedSettingStorage.GetCallingAssemblyTitle(Assembly.GetCallingAssembly());
            string SettingsPath = Path.Combine(IsolatedSettingStorage.baseSettingPath, CallingAssemblyTitle);

            return new FileSettingStore(SettingsPath,name);
        }

        /// <summary>
        /// Returns a setting store that allows to write7read key/value pairs
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ITagValueSettingStore GetTagValueStore(string name)
        {
            try
            {
                string CallingAssemblyTitle = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().DbC_AssureNotNull("AssemblyTitleAttribute must be defined exactly once for the calling assembly").Title;
                string SettingsPath = Path.Combine(IsolatedSettingStorage.baseSettingPath, IsolatedSettingStorage.MakeValidPathName(CallingAssemblyTitle));

                return new TagValueSettingStore(SettingsPath, name);
            }
            catch
            {
                return new TagValueSettingStore(IsolatedSettingStorage.baseSettingPath,name);
            }
        }

        private static string MakeValidPathName(string value)
        {
            return IsolatedSettingStorage.ReplaceChars(value,Path.GetInvalidPathChars(),'_');
        }

        private static string ReplaceChars(string value, char[] invalidChars, char replacement)
        {
            string NewValue = value;
            foreach( char InvalidCharacter in invalidChars )
            {
                NewValue=NewValue.Replace(InvalidCharacter, replacement);
            }
            return NewValue;
        }


    }
}
