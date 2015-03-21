namespace WhileTrue.Classes.ATR
{
    /// <summary>
    /// 
    /// </summary>
    public class ParseError
    {
        public ParseError(string error, int index)
        {
            this.Error = error;
            this.Index = index;
        }

        public string Error { get; private set; }
        public int Index{ get; private set; }
    }
}