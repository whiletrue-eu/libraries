namespace WhileTrue.Facades.SystemDialogs
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenFileDialogResult
    {
        public OpenFileDialogResult(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }
    }
}