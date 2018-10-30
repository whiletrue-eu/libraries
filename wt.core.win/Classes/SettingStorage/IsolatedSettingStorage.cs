using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.SettingStorage
{
    /// <summary>
    ///     Provides key-value or file based setting storage within isolated storage
    /// </summary>
    [PublicAPI]
    public static class IsolatedSettingStorage
    {
        private static readonly string baseSettingPath;

        static IsolatedSettingStorage()
        {
            try
            {
                var ApplicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var MainAssemblyTitle = GetMainAssemblyTitle();
                var MainAssemblyCompany = GetMainAssemblyCompany();
                var MainAssemblyVersion = GetMainAssemblyVersion();

                baseSettingPath = Path.Combine(ApplicationDataPath,
                    $@"{MakeValidPathName(MainAssemblyCompany)}\{MakeValidPathName(MainAssemblyTitle)}\{MainAssemblyVersion}");
            }
            catch (Exception Exception)
            {
                Trace.Fail($"{Exception.Message}\n\nStackTrace:\n{Exception.StackTrace}");
                baseSettingPath = Path.GetTempPath();
            }
        }

        private static string GetMainAssemblyVersion()
        {
            var EntryAssembly = Assembly.GetEntryAssembly();
            if (EntryAssembly != null)
            {
                var AssemblyVersion = EntryAssembly.GetName().Version;
                return $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}";
            }

            return "-.-";
        }

        private static string GetMainAssemblyCompany()
        {
            var AssemblyTitleAttribute = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyCompanyAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Company) == false)
                return MakeValidPathName(AssemblyTitleAttribute.Company);
            return "[unknown]";
        }

        private static string GetMainAssemblyTitle()
        {
            return GetCallingAssemblyTitle(Assembly.GetEntryAssembly());
        }

        private static string GetCallingAssemblyTitle(Assembly assembly)
        {
            var AssemblyTitleAttribute = assembly?.GetCustomAttribute<AssemblyTitleAttribute>();
            if (AssemblyTitleAttribute != null && string.IsNullOrEmpty(AssemblyTitleAttribute.Title) == false)
                return MakeValidPathName(AssemblyTitleAttribute.Title);
            return "[unknown]";
        }

        /// <summary>
        ///     Returns a setting store that allows to write/read files
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IFileSettingStore GetFileStore(string name)
        {
            var CallingAssemblyTitle = GetCallingAssemblyTitle(Assembly.GetCallingAssembly());
            var SettingsPath = Path.Combine(baseSettingPath, CallingAssemblyTitle);

            return new FileSettingStore(SettingsPath, name);
        }

        /// <summary>
        ///     Returns a setting store that allows to write7read key/value pairs
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ITagValueSettingStore GetTagValueStore(string name)
        {
            try
            {
                var CallingAssemblyTitle = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()
                    .DbC_AssureNotNull("AssemblyTitleAttribute must be defined exactly once for the calling assembly")
                    .Title;
                var SettingsPath = Path.Combine(baseSettingPath, MakeValidPathName(CallingAssemblyTitle));

                return new TagValueSettingStore(SettingsPath, name);
            }
            catch
            {
                return new TagValueSettingStore(baseSettingPath, name);
            }
        }

        private static string MakeValidPathName(string value)
        {
            return ReplaceChars(value, Path.GetInvalidPathChars(), '_');
        }

        private static string ReplaceChars(string value, char[] invalidChars, char replacement)
        {
            var NewValue = value;
            foreach (var InvalidCharacter in invalidChars) NewValue = NewValue.Replace(InvalidCharacter, replacement);
            return NewValue;
        }
    }
}