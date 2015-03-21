using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Classes.ATR
{
    public class AtrCompactTlvHistoricalCharacters : AtrHistoricalCharactersBase
    {
        private readonly Dictionary<byte,CompactTLVDataObjectBase> dataObjects;
        private readonly EnumerablePropertyAdapter<CompactTLVDataObjectBase, CompactTLVDataObjectBase> dataObjectsAdapter;
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
            this.dataObjects = new Dictionary<byte,CompactTLVDataObjectBase> 
            {
                {0x40, new CompactTLVDataObjectRFU(this, 0x40)},
                {0x41, new CompactTLVDataObjectCountryCode(this)},
                {0x42, new CompactTLVDataObjectIssuerIdentificationNumber(this)},
                {0x43, new CompactTLVDataObjectCardServiceData(this)},
                {0x44, new CompactTLVDataObjectInitialAccessData(this)},
                {0x45, new CompactTLVDataObjectCardIssuerData(this)},
                {0x46, new CompactTLVDataObjectPreIssuingData(this)},
                {0x47, new CompactTLVDataObjectCardCapabilities(this)},
                {0x48, new CompactTLVDataObjectStatusIndicator(this)},
                {0x49, new CompactTLVDataObjectRFU(this, 0x49)},
                {0x4A, new CompactTLVDataObjectRFU(this, 0x4A)},
                {0x4B, new CompactTLVDataObjectRFU(this, 0x4B)},
                {0x4C, new CompactTLVDataObjectRFU(this, 0x4C)},
                {0x4D, new CompactTLVDataObjectRFU(this, 0x4D)},
                {0x4E, new CompactTLVDataObjectRFU(this, 0x4E)},
                {0x4F, new CompactTLVDataObjectApplicationIdentifier(this)}
            };

            this.dataObjectsAdapter = this.CreatePropertyAdapter(
                ()=>DataObjects,
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
                    IncludeStatusInTlv = ((CompactTLVDataObjectStatusIndicator) this.dataObjects[0x48]).IncludedInTlv;
                }
                else
                {
                    //if there is a data parsing error, we need to stay at the old setting and can't rely on the status object to ask (as it's in 'not applicable' state)
                    IncludeStatusInTlv = this.HistoricalCharacters.Length >= 1 ? this.HistoricalCharacters[0] == 0x80 : true;
                }
                Data.Add((byte) (IncludeStatusInTlv ? 0x80 : 0x00));
                foreach (KeyValuePair<byte, CompactTLVDataObjectBase> DataObject in this.dataObjects.Where(_=>_.Value.IsApplicable))
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
                List<CompactTLVDataObjectBase> NotUpdatedTLVs = new List<CompactTLVDataObjectBase>(this.dataObjects.Values);

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
                        byte[] TLVData = HistoricalCharacters.GetSubArray(0, IncludesStatusInTlv ? HistoricalCharacters.Length : HistoricalCharacters.Length - 3);

                        while (Index < TLVData.Length)
                        {
                            if (TLVData[Index] != 0x00)
                            {
                                byte TagLength = TLVData[Index];
                                byte Tag = (byte) (0x40 | ((TagLength & 0xF0) >> 4));
                                byte Length = (byte) (TagLength & 0x0F);
                                if (TLVData.Length >= Index + 1 + Length)
                                {
                                    byte[] Data = TLVData.GetSubArray(Index + 1, Length);

                                    switch ((CompactTLVTypes)Tag)
                                    {
                                        case CompactTLVTypes.RFU_40: //RFU/Padding
                                        case CompactTLVTypes.CountryCode: //Country Code
                                        case CompactTLVTypes.IssuerIdentificationNumber: //Issuer Identification number
                                        case CompactTLVTypes.CardServiceData: //Card Service data
                                        case CompactTLVTypes.InitialAccessData: //Initial access data
                                        case CompactTLVTypes.CardIssuerData: //Card issuers data
                                        case CompactTLVTypes.PreIssuingData: //Pre-issuing data
                                        case CompactTLVTypes.CardCapabilities: //Card capabilities
                                        case CompactTLVTypes.RFU_49: //RFU
                                        case CompactTLVTypes.RFU_4A: //RFU
                                        case CompactTLVTypes.RFU_4B: //RFU
                                        case CompactTLVTypes.RFU_4C: //RFU
                                        case CompactTLVTypes.RFU_4D: //RFU
                                        case CompactTLVTypes.RFU_4E: //RFU
                                        case CompactTLVTypes.ApplicationIdentifer: //Application Identifier
                                            this.dataObjects[Tag].UpdateTlvData(TagLength, Data);
                                            NotUpdatedTLVs.Remove(this.dataObjects[Tag]);
                                            break;
                                        case CompactTLVTypes.StatusIndicator: //Status indicator
                                            if (IncludesStatusInTlv == false)
                                            {
                                                this.ParseError = new ParseError("Status is included as TLV, but indicated to be outside of Tlv data", Index);
                                            }
                                            this.dataObjects[0x48].UpdateTlvData(TagLength, Data);
                                            NotUpdatedTLVs.Remove(this.dataObjects[0x48]);
                                            ((CompactTLVDataObjectStatusIndicator) this.dataObjects[0x48]).IncludedInTlv = true;
                                            break;
                                    }
                                    Index += (1 + Length);
                                }
                                else
                                {
                                    this.ParseError =
                                        new ParseError(
                                            string.Format("{0} byte{1} missing parsing the compact tlv data at index {2} (Tag 0x{3:X}, length {4}, remaining length {5}{6})", Index + 1 + Length - TLVData.Length,
                                                Index + 1 + Length - TLVData.Length == 1 ? " is" : "s are", Index, Tag, Length, TLVData.Length - Index - 1, IncludesStatusInTlv == false ? ", excluding 3 byte status which is appended to TLV data" : ""),
                                            Index);
                                    Index = TLVData.Length;
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
                            NotUpdatedTLVs.Remove(this.dataObjects[0x48]);
                            ((CompactTLVDataObjectStatusIndicator)this.dataObjects[0x48]).IncludedInTlv = false;
                        }
                    }

                    NotUpdatedTLVs.ForEach(_ => _.UpdateTlvData(null, null)); //Clear cached data; TLV no longer contained in data
                }
                else
                {
                    this.IsApplicable = false;
                }
            }
            catch (Exception exception)
            {
                this.ParseError = new ParseError(exception.Message, Index);
            }
            finally
            {
                this.isParsing = false;
            }
        }

        public IEnumerable<CompactTLVDataObjectBase> DataObjects
        {
            get { return this.dataObjectsAdapter.GetCollection(); }
        }

        public void RemoveDataObject(CompactTLVTypes type)
        {
            this.dataObjects[(byte) type].Remove();
        }

        public void AddDataObject(CompactTLVTypes type)
        {
            this.dataObjects[(byte)type].Add();
        }
    }
}