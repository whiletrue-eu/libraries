namespace WhileTrue.Classes.Installer
{
    internal interface IAdminProcessConnector
    {
        void LaunchProcess();
        void EndIfStartedAndWaitForExit();
        void DoInstallRemote(PrerequisiteBase prerequisite);
    }
}