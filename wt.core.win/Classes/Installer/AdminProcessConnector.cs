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
        private Process adminInstall;
        private NamedPipeServerStream pipeServer;
        private StreamReader reader;

        public void LaunchProcess()
        {
            var PipeName = Guid.NewGuid().ToString("X");
            pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);

            var AdminInstallStartInfo = new ProcessStartInfo(Environment.GetCommandLineArgs()[0], PipeName)
            {
                WorkingDirectory = Environment.CurrentDirectory,
                Verb = "runas",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            adminInstall = Process.Start(AdminInstallStartInfo);
            pipeServer.WaitForConnection();
            reader = new StreamReader(pipeServer);
        }

        public void EndIfStartedAndWaitForExit()
        {
            if (adminInstall != null)
            {
                pipeServer.Close();
                adminInstall.WaitForExit();
                if (adminInstall.ExitCode != 0)
                {
                    //TODO: issue error? or don't care -> everything is already installed
                }
            }
        }

        public void DoInstallRemote(PrerequisiteBase prerequisite)
        {
            IFormatter Serializer = new BinaryFormatter();
            Serializer.Serialize(pipeServer, prerequisite);
            pipeServer.WaitForPipeDrain();

            var Result = reader.ReadLine();
            if (string.IsNullOrEmpty(Result) == false) throw new Exception(Result);
        }
    }
}