namespace WhileTrue.Facades.SystemDialogs
{
    /// <summary>
    /// 
    /// </summary>
    public class SaveFileDialogResult
    {
        public SaveFileDialogResult(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; private set; }
    }
}