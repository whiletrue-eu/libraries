using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WhileTrue.Classes.Installer
{
    internal class AdminProcessConnector : IAdminProcessConnector
    {
        private NamedPipeServerStream pipeServer;
        private Process adminInstall;
        private StreamReader reader;

        public void LaunchProcess()
        {
            string PipeName = Guid.NewGuid().ToString("X");
            this.pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

            ProcessStartInfo AdminInstallStartInfo = new ProcessStartInfo(Environment.GetCommandLineArgs()[0], PipeName)
                                                     {
                                                         WorkingDirectory = Environment.CurrentDirectory,
                                                         Verb = "runas",
                                                         CreateNoWindow = true,
                                                         WindowStyle = ProcessWindowStyle.Hidden
                                                     };
            this.adminInstall = Process.Start(AdminInstallStartInfo);
            this.pipeServer.WaitForConnection();
            this.reader = new StreamReader(this.pipeServer);
        }

        public void EndIfStartedAndWaitForExit()
        {
            if (this.adminInstall != null)
            {
                this.pipeServer.Close();
                this.adminInstall.WaitForExit();
                if (this.adminInstall.ExitCode != 0)
                {
                    //TODO: issue error? or don't care -> everything is already installed
                }
            }
        }

        public void DoInstallRemote(PrerequisiteBase prerequisite)
        {
            IFormatter Serializer = new BinaryFormatter();
            Serializer.Serialize(this.pipeServer, prerequisite);
            this.pipeServer.WaitForPipeDrain();

            string Result = this.reader.ReadLine();
            if (string.IsNullOrEmpty(Result) == false)
            {
                throw new Exception(Result);
            }
        }
    }
}