using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class AtrHistoricalBytesAdapterBase : ObservableObject
    {
        private readonly InterpretedAtrAdapter interpretedAtr;
        private static readonly ObjectCache<ObjectCacheKey<AtrHistoricalCharactersBase, InterpretedAtrAdapter>, AtrHistoricalBytesAdapterBase> objectCache= new ObjectCache<ObjectCacheKey<AtrHistoricalCharactersBase, InterpretedAtrAdapter>, AtrHistoricalBytesAdapterBase>(AtrHistoricalBytesAdapterBase.CreateObject);

        private readonly ReadOnlyPropertyAdapter<string> historicalCharactersAdapter;
        private readonly ReadOnlyPropertyAdapter<IEnumerable<EnumerationAdapter<HistoricalCharacterTypes>>> historicalCharacterTypesAdapter;
        private readonly DelegateCommand<EnumerationAdapter<HistoricalCharacterTypes>> setHistoricalCharacterTypeCommand;
        private string editingIssue;

        public AtrHistoricalBytesAdapterBase(AtrHistoricalCharactersBase historicalCharacters, InterpretedAtrAdapter interpretedAtr)
        {
            this.interpretedAtr = interpretedAtr;
            this.historicalCharactersAdapter = this.CreatePropertyAdapter(
                nameof(this.HistoricalCharacters),
                ()=>historicalCharacters.HistoricalCharacters.ToHexString(" ")
                );
            this.historicalCharacterTypesAdapter = this.CreatePropertyAdapter(
                nameof(this.HistoricalCharacterTypes),
                () => AtrHistoricalBytesAdapterBase.GetHistoricalCharacterTypes(this.interpretedAtr.HistoricalCharacters)
                );
            this.PropertyChanged+=this.AtrHistoricalBytesAdapterBase_PropertyChanged;


            this.setHistoricalCharacterTypeCommand = new DelegateCommand<EnumerationAdapter<HistoricalCharacterTypes>>(this.SetHistoricalCharacterType);
        }

        private void SetHistoricalCharacterType(EnumerationAdapter<HistoricalCharacterTypes> type)
        {
            this.interpretedAtr.SetHistoricalCharacterType(type.Value);
        }

        public IEnumerable<EnumerationAdapter<HistoricalCharacterTypes>> HistoricalCharacterTypes => this.historicalCharacterTypesAdapter.GetValue();

        public ICommand SetHistoricalCharacterTypeCommand => this.setHistoricalCharacterTypeCommand;

        private static IEnumerable<EnumerationAdapter<HistoricalCharacterTypes>> GetHistoricalCharacterTypes(AtrHistoricalBytesAdapterBase currentHistoricalCharacters)
        {
            if (currentHistoricalCharacters is AtrCompactTlvHistoricalBytesAdapter == false)
            {
                yield return Classes.ATR.HistoricalCharacterTypes.CompactTlv;
            }
            if (currentHistoricalCharacters is AtrDirDataReferenceHistoricalBytesAdapter == false)
            {
                yield return Classes.ATR.HistoricalCharacterTypes.DirDataReference;
            }
            if (currentHistoricalCharacters is AtrProprietaryHistoricalBytesAdapter == false)
            {
                yield return Classes.ATR.HistoricalCharacterTypes.Proprietary;
            }
            if (currentHistoricalCharacters is AtrRfuHistoricalBytesAdapter == false)
            {
                yield return Classes.ATR.HistoricalCharacterTypes.Rfu;
            }
            if (currentHistoricalCharacters is AtrNoHistoricalBytesAdapter == false)
            {
                yield return Classes.ATR.HistoricalCharacterTypes.No;
            }
        }

        public string HistoricalCharacters => this.historicalCharactersAdapter.GetValue();

        public static AtrHistoricalBytesAdapterBase GetObject(AtrHistoricalCharactersBase atrHistoricalCharactersBase, InterpretedAtrAdapter interpretedAtr)
        {
            return AtrHistoricalBytesAdapterBase.objectCache.GetObject(new ObjectCacheKey<AtrHistoricalCharactersBase, InterpretedAtrAdapter>(atrHistoricalCharactersBase, interpretedAtr));
        }

        public static AtrHistoricalBytesAdapterBase CreateObject(ObjectCacheKey<AtrHistoricalCharactersBase, InterpretedAtrAdapter> objectCacheKey)
        {
            AtrHistoricalCharactersBase AtrHistoricalCharactersBase = objectCacheKey.Param1;
            var InterpretedAtr = objectCacheKey.Param2;
            if (AtrHistoricalCharactersBase == null)
            {
                return null;
            }
            else if (AtrHistoricalCharactersBase is AtrCompactTlvHistoricalCharacters)
            {
                return new AtrCompactTlvHistoricalBytesAdapter((AtrCompactTlvHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else if (AtrHistoricalCharactersBase is AtrDirDataReferenceHistoricalCharacters)
            {
                return new AtrDirDataReferenceHistoricalBytesAdapter((AtrDirDataReferenceHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else if (AtrHistoricalCharactersBase is AtrInvalidHistoricalCharacters)
            {
                return new AtrInvalidHistoricalBytesAdapter((AtrInvalidHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else if (AtrHistoricalCharactersBase is AtrProprietaryHistoricalCharacters)
            {
                return new AtrProprietaryHistoricalBytesAdapter((AtrProprietaryHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else if (AtrHistoricalCharactersBase is AtrRfuHistoricalCharacters)
            {
                return new AtrRfuHistoricalBytesAdapter((AtrRfuHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else if (AtrHistoricalCharactersBase is AtrNoHistoricalCharacters)
            {
                return new AtrNoHistoricalBytesAdapter((AtrNoHistoricalCharacters)AtrHistoricalCharactersBase, InterpretedAtr);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public string EditingIssue
        {
            get { return this.editingIssue; }
            set { this.SetAndInvoke( ref this.editingIssue, value); }
        }

        protected void HandleException(Exception exception)
        {
            this.EditingIssue = exception.GetExceptionMessage();
        }

        private void AtrHistoricalBytesAdapterBase_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.HistoricalCharacters))
            {
                this.EditingIssue = null;
            }
        }
    }
}