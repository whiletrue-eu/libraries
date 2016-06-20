using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace WhileTrue.Classes.Installer
{
    /// <summary>
    /// Helper class for prerequisite installation. Designed to support clickonce deployments, but can also be used with all other programs independently from clickonce
    /// </summary>
    public class PrerequisiteInstaller
    {
        /// <summary>
        /// Checks if all prerequisites are present on the system. If not, it will install the prerequisites, eventually launching a child process with admin rights to do installations that need elevation
        /// </summary>
        /// <remarks>
        /// The installer can also handle prerequisites that require admin rights when running in a non-elevated process.
        /// In this case, the process is re-run as child process with admin privileges. If this happens, there is only one command line optins that is formatted as a GUID.
        /// When this is detected, just call this Method again and the installer will run in child process mode. It will exit the process when all installations are completed and will not return. 
        /// </remarks>
        /// <returns><c>true if an installation was performed</c></returns>
        public static bool CheckPrerequisitesInstalled<T>(string[] commandLineArguments, Action<string, Action<string, double>, Action<string>> downloadFunc, params PrerequisiteBase[] prerequisites) where T :Window,new()
        {
            Guid PipeGuid;
            if (commandLineArguments.Length == 1 && Guid.TryParse(commandLineArguments[0], out PipeGuid) )
            {
                //Launched in admin mode to do installs. Connect to pipe to get instructions
                //TODO: Check admin
                HandleAdminInstalls(commandLineArguments);
                Environment.Exit(0);
                return false;
            }
            else
            {
                PrerequisiteBase[] MissingPrerequisites = prerequisites.Where(_ => _.IsAlreadyInstalled==false).ToArray();

                if (MissingPrerequisites.Any())
                {
                    T Window = new T();
                    Window.DataContext = new InstallWindowModel(MissingPrerequisites, downloadFunc);

                    if (Application.Current == null)
                    {
                        Application NewApplication = new Application();
                        NewApplication.Run(Window);
                        NewApplication.MainWindow = null; //Reset main window in case we are running within a WPF app
                    }
                    else
                    {
                        Window.ShowDialog();
                        Application.Current.MainWindow = null; //Reset main window in case we are running within a WPF app
                    }

                    if ( MissingPrerequisites.Any(_ => _.WasInstalled == false))
                    {
                        throw new PrerequisiteException();
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    //All prerequisites are installed -> continue with program execution
                    return true;
                }
            }
        }

        private static void HandleAdminInstalls(string[] commandLineArgs)
        {
            NamedPipeClientStream PipeClient = new NamedPipeClientStream(commandLineArgs[0]);
            PipeClient.Connect(1000);
            if (PipeClient.IsConnected)
            {

                try
                {
                    using (StreamWriter Writer = new StreamWriter(PipeClient))
                    {
                        IFormatter Serializer = new BinaryFormatter();

                        while (true)
                        {
                            PrerequisiteBase Prerequisite = (PrerequisiteBase) Serializer.Deserialize(PipeClient);

                            try
                            {
                                Prerequisite.DoInstall();
                                Writer.WriteLine(string.Empty);
                            }
                            catch (Exception Error)
                            {
                                Writer.WriteLine(Error.Message);
                            }
                            Writer.Flush();
                            PipeClient.WaitForPipeDrain();
                        }

                    }
                }
                catch (IOException)
                {
                    //Pipe was closed remotely - nothing left to do. End
                }
                catch
                {
                    //Oops.. Something went wrong. Just exit, let the caller handle the issue. Must be handled anyway, as the user could also cancel UAC
                }
            }
            else
            {
                //Oops.. Something went wrong. Just exit, let the caller handle the issue. Must be handled anyway, as the user could also cancel UAC
            }
        }
    }
}
