using System;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commands;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class DataObjectBaseAdapter:ObservableObject
    {
        private static readonly ObjectCache<CompactTlvDataObjectBase, DataObjectBaseAdapter> objectCache = new ObjectCache<CompactTlvDataObjectBase, DataObjectBaseAdapter>(DataObjectBaseAdapter.CreateObject);

        private readonly CompactTlvDataObjectBase value;
        private static readonly ReadOnlyPropertyAdapter<DataObjectBaseAdapter, string> dataAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectBaseAdapter, string> dataErrorAdapter;
        private DelegateCommand removeCommand;

        public static DataObjectBaseAdapter GetObject(CompactTlvDataObjectBase value)
        {
            return DataObjectBaseAdapter.objectCache.GetObject(value);
        }
        public static DataObjectBaseAdapter CreateObject(CompactTlvDataObjectBase value)
        {
            if (value == null)
            {
                return null;
            }
            else if (value is CompactTlvDataObjectApplicationIdentifier)
            {
                return new DataObjectApplicationIdentifierAdapter((CompactTlvDataObjectApplicationIdentifier)value);
            }
            else if (value is CompactTlvDataObjectCardCapabilities)
            {
                return new DataObjectCardCapabilitiesAdapter((CompactTlvDataObjectCardCapabilities)value);
            }
            else if (value is CompactTlvDataObjectCardIssuerData)
            {
                return new DataObjectCardIssuerDataAdapter((CompactTlvDataObjectCardIssuerData)value);
            }
            else if (value is CompactTlvDataObjectCardServiceData)
            {
                return new DataObjectCardServiceDataAdapter((CompactTlvDataObjectCardServiceData)value);
            }
            else if (value is CompactTlvDataObjectCountryCode)
            {
                return new DataObjectCountryCodeAdapter((CompactTlvDataObjectCountryCode)value);
            }
            else if (value is CompactTlvDataObjectInitialAccessData)
            {
                return new DataObjectInitialAccessDataAdapter((CompactTlvDataObjectInitialAccessData)value);
            }
            else if (value is CompactTlvDataObjectIssuerIdentificationNumber)
            {
                return new DataObjectIssuerIdentificationNumberAdapter((CompactTlvDataObjectIssuerIdentificationNumber)value);
            }
            else if (value is CompactTlvDataObjectPreIssuingData)
            {
                return new DataObjectPreIssuingDataAdapter((CompactTlvDataObjectPreIssuingData)value);
            }
            else if (value is CompactTlvDataObjectRfu)
            {
                return new DataObjectRfuAdapter((CompactTlvDataObjectRfu)value);
            }
            else if (value is CompactTlvDataObjectStatusIndicator)
            {
                return new DataObjectStatusIndicatorAdapter((CompactTlvDataObjectStatusIndicator)value);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        static DataObjectBaseAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectBaseAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectBaseAdapter>();

            DataObjectBaseAdapter.dataAdapter = PropertyFactory.Create(
                nameof(DataObjectBaseAdapter.Data),
                instance => instance.value.Data.ToHexString(" ")
                );

            DataObjectBaseAdapter.dataErrorAdapter = PropertyFactory.Create(
                nameof(DataObjectBaseAdapter.DataError),
                instance => instance.value.DataError
                );
        }

        public string Data => DataObjectBaseAdapter.dataAdapter.GetValue(this);

        public string DataError => DataObjectBaseAdapter.dataErrorAdapter.GetValue(this);

        public DataObjectBaseAdapter(CompactTlvDataObjectBase value)
        {
            this.value = value;
            this.removeCommand = new DelegateCommand(this.Remove);
        }

        private void Remove()
        {
            this.value.Remove();
        }

        public ICommand RemoveCommand => this.removeCommand;
    }
}