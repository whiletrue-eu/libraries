using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using AtrEditor;
using AtrEditor.About;
using AtrEditor.MainWindow;
using WhileTrue.Classes.Components;
using WhileTrue.Controls.SplashScreen;
using WhileTrue.Facades.ApplicationLoader;
using WhileTrue.Facades.SplashScreen;
using WhileTrue.Modules.ModelInspector;

namespace AtrParser
{
    /// <summary/>
    partial class App
    {
        protected override void AddComponents(ComponentRepository componentRepository)
        {
#if xDEBUG
            componentRepository.AddComponent<ModelInspectorModule>();
#endif
            componentRepository.AddComponent<ApplicationMain>();
            componentRepository.AddComponent<ApplicationSplashScreen>();
            componentRepository.AddComponent<MainWindow>();
            componentRepository.AddComponent<AboutWindow>();
        }

        [Component]
        public class ApplicationMain : IApplicationMain
        {
            public int Run(ComponentContainer componentContainer)
            {
                DateTime NewestFileDate = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).EnumerateFileSystemInfos().OrderByDescending(_ => _.LastAccessTime).Select(_=>_.LastAccessTime).FirstOrDefault();
                DateTime Now = NewestFileDate > DateTime.Now ? NewestFileDate : DateTime.Now;

                DateTime BuildTime = RetrieveLinkerTimestamp(Assembly.GetExecutingAssembly().Location);

                int DaysLeft = ((BuildTime + new TimeSpan(91, 0, 0, 0)) - Now).Days;

                if (DaysLeft < 0)
                {
                    MessageBox.Show("This BETA build of ATRinaline is too old.\nPlease update to a more recent build.", "ATRinaline", MessageBoxButton.OK, MessageBoxImage.Information);
                    return -1;
                }

                IMainWindow MainWindow = componentContainer.ResolveInstance<IMainWindow>();
                MainWindow.DaysLeft = DaysLeft;
#if xDEBUG
                componentContainer.ResolveInstance<IModelInspector>().Inspect(MainWindow.Atr,"Atr");
                componentContainer.ResolveInstance<IModelInspector>().Inspect(()=>MainWindow.AtrEditor.AtrModel,"Atr editor");
#endif
                MainWindow.ShowDialog();
                return 0;
            }
        }

        /// <summary>
        /// Retrieves the linker timestamp.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        /// <remarks>http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html</remarks>
        private static DateTime RetrieveLinkerTimestamp(string filePath)
        {
            const int peHeaderOffset = 60;
            const int linkerTimestampOffset = 8;
            var b = new byte[2048];
            System.IO.FileStream s = null;
            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                    s.Close();
            }
            var dt = new System.DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(System.BitConverter.ToInt32(b, System.BitConverter.ToInt32(b, peHeaderOffset) + linkerTimestampOffset));
            return dt.AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
        }
    }

    [Component]
    public class ApplicationSplashScreen : ISplashScreen
    {
        public void Show()
        {
            SplashScreenEx.Show<ApplicationSplashScreenWindow>(this);
        }

        public void Hide()
        {
        }

        public void SetStatus(int totalNumber, int currentNumber, string name)
        {
        }
    }
}
