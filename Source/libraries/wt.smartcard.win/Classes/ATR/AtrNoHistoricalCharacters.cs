namespace WhileTrue.Classes.ATR
{
    public class AtrNoHistoricalCharacters : AtrHistoricalCharactersBase
    {
        public AtrNoHistoricalCharacters(Atr owner) : base(owner)
        {
        }

        protected override void HistoricalCharactersChanged(byte[] historicalCharacters1)
        {
            if (this.HistoricalCharacters.Length == 0)
            {
                this.IsApplicable = true;
            }
            else
            {
                this.IsApplicable = false;
            }
        }
    }
}