using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class AtrCompactTlvHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private readonly Dictionary<byte,CompactTlvDataObjectBase> dataObjects;
        private readonly EnumerablePropertyAdapter<CompactTlvDataObjectBase, CompactTlvDataObjectBase> dataObjectsAdapter;
        private bool isParsing;

        /* ISO 7816-4 ch. 8.3 Optional COMPACT-TLV data objects
     The coding of the COMPACT-TLV data objects is deduced from the basic encoding rules af ASN.1 (see
     ISO/IEC 8825 and annex D) for BER-TLV data objects with tag='4X' and length='0Y'. The coding of
     such data objects is replaced by 'XY' followed by 'Y' bytes of data. In this clause, 'X' is
     referred to as the tag number and 'Y' as the length.
     Besides the data objects defined in this clause, the historical bytes may contain data objects
     defined in part 4 of ISO/IEC 7816. In this case the coding of the tags and length fields defined in
     part 5 shall be modified as above.
     When COMPACT-TLV data objects defined in this clause appear in the ATR file, they shall be encoded
     according to the basic encoding rules of ASN.1 (i.e tag='4X', length='0Y').
     All application-class tags not defined in ISO/IEC 7816 are reserved for ISO.
     
     8.4 Status information
     If the category indicator is valued to '80', then the status information may be present in a
     COMPACT-TLV data object. In this case, the tag number is '8'. When the length is '1', then the
     value is the card life status. When the length is '2', then the value is SW1-SW2. When the length
     is '3', then the value is the card life status followed by SW1-SW2. Other values of the length are
     reserved for ISO.
         */
        public AtrCompactTlvHistoricalCharacters( Atr owner )
            : base(owner)
        {
            this.dataObjects = new Dictionary<byte,CompactTlvDataObjectBase> 
            {
                {0x40, new CompactTlvDataObjectRfu(this, 0x40)},
                {0x41, new CompactTlvDataObjectCountryCode(this)},
                {0x42, new CompactTlvDataObjectIssuerIdentificationNumber(this)},
                {0x43, new CompactTlvDataObjectCardServiceData(this)},
                {0x44, new CompactTlvDataObjectInitialAccessData(this)},
                {0x45, new CompactTlvDataObjectCardIssuerData(this)},
                {0x46, new CompactTlvDataObjectPreIssuingData(this)},
                {0x47, new CompactTlvDataObjectCardCapabilities(this)},
                {0x48, new CompactTlvDataObjectStatusIndicator(this)},
                {0x49, new CompactTlvDataObjectRfu(this, 0x49)},
                {0x4A, new CompactTlvDataObjectRfu(this, 0x4A)},
                {0x4B, new CompactTlvDataObjectRfu(this, 0x4B)},
                {0x4C, new CompactTlvDataObjectRfu(this, 0x4C)},
                {0x4D, new CompactTlvDataObjectRfu(this, 0x4D)},
                {0x4E, new CompactTlvDataObjectRfu(this, 0x4E)},
                {0x4F, new CompactTlvDataObjectApplicationIdentifier(this)}
            };

            this.dataObjectsAdapter = this.CreatePropertyAdapter(
                nameof(this.DataObjects),
                ()=>from DataObject in this.dataObjects.Values where DataObject.IsApplicable select DataObject,
                _=>_
                );
        }

        internal void NotifyChanged()
        {
            if (this.isParsing == false)
            {
                List<byte> Data = new List<byte>();
                bool IncludeStatusInTlv;
                if (this.dataObjects[0x48].IsApplicable)
                {
                    IncludeStatusInTlv = ((CompactTlvDataObjectStatusIndicator) this.dataObjects[0x48]).IncludedInTlv;
                }
                else
                {
                    //if there is a data parsing error, we need to stay at the old setting and can't rely on the status object to ask (as it's in 'not applicable' state)
                    IncludeStatusInTlv = this.HistoricalCharacters.Length >= 1 ? this.HistoricalCharacters[0] == 0x80 : true;
                }
                Data.Add((byte) (IncludeStatusInTlv ? 0x80 : 0x00));
                foreach (KeyValuePair<byte, CompactTlvDataObjectBase> DataObject in this.dataObjects.Where(_=>_.Value.IsApplicable))
                {
                    if (DataObject.Key != 0x48 || IncludeStatusInTlv)
                    {
                        Data.AddRange(DataObject.Value.Data);
                    }
                }
                if (IncludeStatusInTlv == false && this.dataObjects[0x48].IsApplicable)
                {
                    try
                    {
                        Data.AddRange(this.dataObjects[0x48].Data);
                    }
                    catch 
                    {
                        //wrong coding (ignore for now), or status was removed
                    }
                }
                if( Data.Count() == 1 )
                {
                    //no data object -> make sure hist.chars are formatted 'compact TLV'. Above the setting may be set different because of situations with parsing errors
                    Data[0] = 0x80;
                }
                this.UpdateHistoricalCharacters(Data.ToArray());
            }
        }


        protected override void HistoricalCharactersChanged(byte[] HistoricalCharacters)
        {
            this.isParsing = true;
            int Index = 1; //outer scope because it is used in catch block
            try
            {
                this.ParseError = null;
                List<CompactTlvDataObjectBase> NotUpdatedTlVs = new List<CompactTlvDataObjectBase>(this.dataObjects.Values);

                if (HistoricalCharacters.Length > 0 && (HistoricalCharacters[0] & 0x7F) == 0x00)
                {
                    this.IsApplicable = true;

                    bool IncludesStatusInTlv = (HistoricalCharacters[0] & 0x80) == 0x80;
                    if (IncludesStatusInTlv == false && HistoricalCharacters.Length < 3+1/*1: coding byte*/)
                    {
                        this.ParseError = new ParseError("Historical bytes category indicator indicates Compact TLV structure with 3 byte status appended, but does not have enough historical characters.", 0);
                    }
                    else
                    {
                        byte[] TlvData = HistoricalCharacters.GetSubArray(0, IncludesStatusInTlv ? HistoricalCharacters.Length : HistoricalCharacters.Length - 3);

                        while (Index < TlvData.Length)
                        {
                            if (TlvData[Index] != 0x00)
                            {
                                byte TagLength = TlvData[Index];
                                byte Tag = (byte) (0x40 | ((TagLength & 0xF0) >> 4));
                                byte Length = (byte) (TagLength & 0x0F);
                                if (TlvData.Length >= Index + 1 + Length)
                                {
                                    byte[] Data = TlvData.GetSubArray(Index + 1, Length);

                                    switch ((CompactTlvTypes)Tag)
                                    {
                                        case CompactTlvTypes.Rfu40: //RFU/Padding
                                        case CompactTlvTypes.CountryCode: //Country Code
                                        case CompactTlvTypes.IssuerIdentificationNumber: //Issuer Identification number
                                        case CompactTlvTypes.CardServiceData: //Card Service data
                                        case CompactTlvTypes.InitialAccessData: //Initial access data
                                        case CompactTlvTypes.CardIssuerData: //Card issuers data
                                        case CompactTlvTypes.PreIssuingData: //Pre-issuing data
                                        case CompactTlvTypes.CardCapabilities: //Card capabilities
                                        case CompactTlvTypes.Rfu49: //RFU
                                        case CompactTlvTypes.Rfu_4A: //RFU
                                        case CompactTlvTypes.Rfu_4B: //RFU
                                        case CompactTlvTypes.Rfu_4C: //RFU
                                        case CompactTlvTypes.Rfu_4D: //RFU
                                        case CompactTlvTypes.Rfu_4E: //RFU
                                        case CompactTlvTypes.ApplicationIdentifer: //Application Identifier
                                            this.dataObjects[Tag].UpdateTlvData(TagLength, Data);
                                            NotUpdatedTlVs.Remove(this.dataObjects[Tag]);
                                            break;
                                        case CompactTlvTypes.StatusIndicator: //Status indicator
                                            if (IncludesStatusInTlv == false)
                                            {
                                                this.ParseError = new ParseError("Status is included as TLV, but indicated to be outside of Tlv data", Index);
                                            }
                                            this.dataObjects[0x48].UpdateTlvData(TagLength, Data);
                                            NotUpdatedTlVs.Remove(this.dataObjects[0x48]);
                                            ((CompactTlvDataObjectStatusIndicator) this.dataObjects[0x48]).IncludedInTlv = true;
                                            break;
                                    }
                                    Index += (1 + Length);
                                }
                                else
                                {
                                    this.ParseError =
                                        new ParseError(
                                            $"{Index + 1 + Length - TlvData.Length} byte{(Index + 1 + Length - TlvData.Length == 1 ? " is" : "s are")} missing parsing the compact tlv data at index {Index} (Tag 0x{Tag:X}, length {Length}, remaining length {TlvData.Length - Index - 1}{(IncludesStatusInTlv == false ? ", excluding 3 byte status which is appended to TLV data" : "")})",
                                            Index);
                                    Index = TlvData.Length;
                                }
                            }
                            else
                            {
                                Index++;
                                //Padding -> Ignore
                            }
                        }

                        if (IncludesStatusInTlv == false)
                        {
                            this.dataObjects[0x48].UpdateTlvData(null, HistoricalCharacters.GetSubArray(HistoricalCharacters.Length - 3));
                            NotUpdatedTlVs.Remove(this.dataObjects[0x48]);
                            ((CompactTlvDataObjectStatusIndicator)this.dataObjects[0x48]).IncludedInTlv = false;
                        }
                    }

                    NotUpdatedTlVs.ForEach(_ => _.UpdateTlvData(null, null)); //Clear cached data; TLV no longer contained in data
                }
                else
                {
                    this.IsApplicable = false;
                }
            }
            catch (Exception Exception)
            {
                this.ParseError = new ParseError(Exception.Message, Index);
            }
            finally
            {
                this.isParsing = false;
            }
        }

        public IEnumerable<CompactTlvDataObjectBase> DataObjects => this.dataObjectsAdapter.GetCollection();

        public void RemoveDataObject(CompactTlvTypes type)
        {
            this.dataObjects[(byte) type].Remove();
        }

        public void AddDataObject(CompactTlvTypes type)
        {
            this.dataObjects[(byte)type].Add();
        }
    }
}