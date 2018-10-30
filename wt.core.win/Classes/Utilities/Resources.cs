using System;
using System.IO;
using System.Windows;

namespace WhileTrue.Classes.Utilities
{
    /// <summary>
    ///     Eases handling of Application Resources (project file type 'Resource')
    /// </summary>
    public static class Resources
    {
        /// <summary>
        ///     Returns a file resources as a stream
        /// </summary>
        public static Stream GetStream(string name)
        {
            var Uri = new Uri(name, UriKind.Relative);
            var Info = Application.GetResourceStream(Uri);
            Info.DbC_AssureNotNull("Resource not found: {0}", name);

            return Info.Stream;
        }

        /// <summary>
        ///     Returns a file resources as a stream
        /// </summary>
        public static string GetTextFile(string name)
        {
            var FileStream = GetStream(name);
            using (var Reader = new StreamReader(FileStream))
            {
                return Reader.ReadToEnd();
            }
        }
    }
}