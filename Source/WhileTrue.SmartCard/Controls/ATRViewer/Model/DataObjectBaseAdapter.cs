using System;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRView
{
    /// <summary>
    /// 
    /// </summary>
    public class DataObjectBaseAdapter:ObservableObject
    {
        private static readonly ObjectCache<CompactTLVDataObjectBase, DataObjectBaseAdapter> objectCache = new ObjectCache<CompactTLVDataObjectBase, DataObjectBaseAdapter>(CreateObject);

        private readonly CompactTLVDataObjectBase value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectBaseAdapter, string> dataAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectBaseAdapter, string> dataErrorAdapter;
        private DelegateCommand removeCommand;

        public static DataObjectBaseAdapter GetObject(CompactTLVDataObjectBase value)
        {
            return objectCache.GetObject(value);
        }
        public static DataObjectBaseAdapter CreateObject(CompactTLVDataObjectBase value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is CompactTLVDataObjectApplicationIdentifier)
            {
                return new DataObjectApplicationIdentifierAdapter((CompactTLVDataObjectApplicationIdentifier)value);
            }
            else if (value is CompactTLVDataObjectCardCapabilities)
            {
                return new DataObjectCardCapabilitiesAdapter((CompactTLVDataObjectCardCapabilities)value);
            }
            else if (value is CompactTLVDataObjectCardIssuerData)
            {
                return new DataObjectCardIssuerDataAdapter((CompactTLVDataObjectCardIssuerData)value);
            }
            else if (value is CompactTLVDataObjectCardServiceData)
            {
                return new DataObjectCardServiceDataAdapter((CompactTLVDataObjectCardServiceData)value);
            }
            else if (value is CompactTLVDataObjectCountryCode)
            {
                return new DataObjectCountryCodeAdapter((CompactTLVDataObjectCountryCode)value);
            }
            else if (value is CompactTLVDataObjectInitialAccessData)
            {
                return new DataObjectInitialAccessDataAdapter((CompactTLVDataObjectInitialAccessData)value);
            }
            else if (value is CompactTLVDataObjectIssuerIdentificationNumber)
            {
                return new DataObjectIssuerIdentificationNumberAdapter((CompactTLVDataObjectIssuerIdentificationNumber)value);
            }
            else if (value is CompactTLVDataObjectPreIssuingData)
            {
                return new DataObjectPreIssuingDataAdapter((CompactTLVDataObjectPreIssuingData)value);
            }
            else if (value is CompactTLVDataObjectRFU)
            {
                return new DataObjectRFUAdapter((CompactTLVDataObjectRFU)value);
            }
            else if (value is CompactTLVDataObjectStatusIndicator)
            {
                return new DataObjectStatusIndicatorAdapter((CompactTLVDataObjectStatusIndicator)value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        static DataObjectBaseAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectBaseAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectBaseAdapter>();

            dataAdapter = PropertyFactory.Create(
                @this => @this.Data,
                @this => @this.value.Data.ToHexString(" ")
                );

            dataErrorAdapter = PropertyFactory.Create(
                @this => @this.DataError,
                @this => @this.value.DataError
                );
        }

        public string Data 
        {
            get { return dataAdapter.GetValue(this); }
        }

        public string DataError
        {
            get { return dataErrorAdapter.GetValue(this); }
        }

        public DataObjectBaseAdapter(CompactTLVDataObjectBase value)
        {
            this.value = value;
            this.removeCommand = new DelegateCommand(this.Remove);
        }

        private void Remove()
        {
            this.value.Remove();
        }

        public ICommand RemoveCommand
        {
            get { return this.removeCommand; }
        }
    }
}