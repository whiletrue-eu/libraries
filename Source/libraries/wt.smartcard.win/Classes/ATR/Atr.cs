using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using WhileTrue.Classes.ATR.Tokenized;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class Atr : ObservableObject
    {
        private byte[] atr;

        private TokenizedAtr tokenizedAtr;
        private readonly ProtocolParametersBase[] protocolParameters;
        private readonly AtrHistoricalCharactersBase[] historicalCharactersTypes;
        private readonly AtrInvalidHistoricalCharacters invalidHistoricalCharacters;

        public Atr( byte[] atr)
        {
            this.atr = atr;
            this.TokenizedAtr = new TokenizedAtr(this, this.atr);

            this.GlobalInterfaceBytes = new GlobalInterfaceBytes(this);

            List<ProtocolParametersBase> ProtocolParameters = new List<ProtocolParametersBase> {new T0ProtocolParameters(this), new T1ProtocolParameters(this)};
            ProtocolParameters.AddRange(new[]{
                ProtocolType.T2, ProtocolType.T3, ProtocolType.T4, ProtocolType.T5, ProtocolType.T6, ProtocolType.T7, ProtocolType.T8, ProtocolType.T9, ProtocolType.T10, ProtocolType.T11, ProtocolType.T12, ProtocolType.T13, ProtocolType.T14
            }.Select(type => new UnknownProtocolParameters(this, type)));

            this.protocolParameters = ProtocolParameters.ToArray();

            this.historicalCharactersTypes = new AtrHistoricalCharactersBase[]
            {
                new AtrNoHistoricalCharacters(this),
                new AtrCompactTlvHistoricalCharacters(this),
                new AtrDirDataReferenceHistoricalCharacters(this),
                new AtrRfuHistoricalCharacters(this),
                new AtrProprietaryHistoricalCharacters(this),
                new AtrInvalidHistoricalCharacters(this)
            };
            this.invalidHistoricalCharacters = (AtrInvalidHistoricalCharacters) this.historicalCharactersTypes.Last();
            this.historicalCharactersTypes.ForEach(_ => _.NotifyAtrChanged());
        }

        private void InvokeChanged()
        {
            this.InvokePropertyChanged(nameof(Atr.Bytes));

            this.GlobalInterfaceBytes.NotifyAtrChanged();
            this.InvokePropertyChanged(nameof(this.GlobalInterfaceBytes));

            this.protocolParameters.ForEach(_ => _.NotifyAtrChanged());
            this.InvokePropertyChanged(nameof(this.ProtocolParameters));

            this.historicalCharactersTypes.ForEach(_ => _.NotifyAtrChanged());
            this.InvokePropertyChanged(nameof(this.HistoricalCharacters));
        }

        public void NotifyChanged()
        {
            this.atr = this.TokenizedAtr.GetBytes();
            this.InvokeChanged();
        }

        public byte[] Bytes
        {
            get { return this.atr; }
            set
            {
                this.TokenizedAtr = new TokenizedAtr(this, value);
                this.InvokeChanged();
                this.SetAndInvoke(ref this.atr, value);
            }

        }

        public TokenizedAtr TokenizedAtr
        {
            get { return this.tokenizedAtr; }
            private set { this.SetAndInvoke(ref this.tokenizedAtr, value); }
        }

        public GlobalInterfaceBytes GlobalInterfaceBytes { get; }

        public IEnumerable<ProtocolParametersBase> ProtocolParameters
        {
            get { return this.protocolParameters.Where(_=>_.IsApplicable); }
        }

        [UsedImplicitly]
        public ProtocolParametersBase GetProtocolParameters(ProtocolType protocolType)
        {
            return (from Parameters in this.ProtocolParameters
                where Parameters.ProtocolType == protocolType
                select Parameters
                ).FirstOrDefault();
        }


        public AtrHistoricalCharactersBase HistoricalCharacters
        {
            get
            {
                AtrHistoricalCharactersBase ApplicableType = this.historicalCharactersTypes.FirstOrDefault(_ => _.IsApplicable);
                ParseError ParseError = ApplicableType?.ParseError;
                if (ParseError != null)
                {
                    this.invalidHistoricalCharacters.UpdateError(ParseError);
                    return this.invalidHistoricalCharacters;
                }
                else
                {
                    return ApplicableType;
                }
            }
        }

        public void IndicateProtocol(ProtocolType type)
        {
            this.protocolParameters.First(_ => _.ProtocolType == type).AddIndication();
        }

        public void SetHistoricalCharactersType(HistoricalCharacterTypes type)
        {
            switch (type)
            {
                case HistoricalCharacterTypes.CompactTlv:
                    this.tokenizedAtr.HistoricalCharacters.HistoricalCharacters=new byte[]{0x80};
                    break;
                case HistoricalCharacterTypes.DirDataReference:
                    this.tokenizedAtr.HistoricalCharacters.HistoricalCharacters=new byte[]{0x10};
                    break;
                case HistoricalCharacterTypes.Proprietary:
                    this.tokenizedAtr.HistoricalCharacters.HistoricalCharacters=new byte[]{0x01};
                    break;
                case HistoricalCharacterTypes.Rfu:
                    this.tokenizedAtr.HistoricalCharacters.HistoricalCharacters = new byte[] { 0x81 };
                    break;
                case HistoricalCharacterTypes.No:
                    this.tokenizedAtr.HistoricalCharacters.HistoricalCharacters = new byte[0];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
