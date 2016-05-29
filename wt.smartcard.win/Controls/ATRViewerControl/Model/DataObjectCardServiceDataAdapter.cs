using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectCardServiceDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectCardServiceData value;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> dataObjectsAvailableInAtrFileAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> dataObjectsAvailableInDirFileAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> supportsApplicationSelectionByFullDfNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> supportsApplicationSelectionByPartialDfNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, EnumerationAdapter<FileIoServices>> fileIoServicesMethodAdapter;

        static DataObjectCardServiceDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardServiceDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardServiceDataAdapter>();

            DataObjectCardServiceDataAdapter.dataObjectsAvailableInAtrFileAdapter = PropertyFactory.Create(
                nameof(DataObjectCardServiceDataAdapter.DataObjectsAvailableInAtrFile),
                instance => instance.value.DataObjectsAvailableInAtrFile,
                (instance,value) => instance.value.DataObjectsAvailableInAtrFile=value
                );

            DataObjectCardServiceDataAdapter.dataObjectsAvailableInDirFileAdapter = PropertyFactory.Create(
                nameof(DataObjectCardServiceDataAdapter.DataObjectsAvailableInDirFile),
                instance => instance.value.DataObjectsAvailableInDirFile,
                (instance,value) => instance.value.DataObjectsAvailableInDirFile=value
                );

            DataObjectCardServiceDataAdapter.supportsApplicationSelectionByFullDfNameAdapter = PropertyFactory.Create(
                nameof(DataObjectCardServiceDataAdapter.SupportsApplicationSelectionByFullDfName),
                instance => instance.value.SupportsApplicationSelectionByFullDfName,
                (instance,value) => instance.value.SupportsApplicationSelectionByFullDfName=value
                );

            DataObjectCardServiceDataAdapter.supportsApplicationSelectionByPartialDfNameAdapter = PropertyFactory.Create(
                nameof(DataObjectCardServiceDataAdapter.SupportsApplicationSelectionByPartialDfName),
                instance => instance.value.SupportsApplicationSelectionByPartialDfName,
                (instance,value) => instance.value.SupportsApplicationSelectionByPartialDfName=value
                );

            DataObjectCardServiceDataAdapter.fileIoServicesMethodAdapter = PropertyFactory.Create(
                nameof(DataObjectCardServiceDataAdapter.FileIoServicesMethod),
                instance => EnumerationAdapter < FileIoServices > .GetInstanceFor(instance.value.FileIoServicesMethod),
                (instance,value) => instance.value.FileIoServicesMethod=value
                );
        }

        public EnumerationAdapter<FileIoServices> FileIoServicesMethod
        {
            get { return DataObjectCardServiceDataAdapter.fileIoServicesMethodAdapter.GetValue(this); } 
            set { DataObjectCardServiceDataAdapter.fileIoServicesMethodAdapter.SetValue(this,value); } 
        }

        public IEnumerable<EnumerationAdapter<FileIoServices>> FileIoServicesMethodValues => EnumerationAdapter<FileIoServices>.Items;

        public bool SupportsApplicationSelectionByPartialDfName
        {
            get { return DataObjectCardServiceDataAdapter.supportsApplicationSelectionByPartialDfNameAdapter.GetValue(this); } 
            set { DataObjectCardServiceDataAdapter.supportsApplicationSelectionByPartialDfNameAdapter.SetValue(this,value); } 
        }

        public bool SupportsApplicationSelectionByFullDfName
        {
            get { return DataObjectCardServiceDataAdapter.supportsApplicationSelectionByFullDfNameAdapter.GetValue(this); } 
            set { DataObjectCardServiceDataAdapter.supportsApplicationSelectionByFullDfNameAdapter.SetValue(this,value); } 
        }

        public bool DataObjectsAvailableInDirFile
        {
            get { return DataObjectCardServiceDataAdapter.dataObjectsAvailableInDirFileAdapter.GetValue(this); } 
            set { DataObjectCardServiceDataAdapter.dataObjectsAvailableInDirFileAdapter.SetValue(this,value); } 
        }

        public bool DataObjectsAvailableInAtrFile
        {
            get { return DataObjectCardServiceDataAdapter.dataObjectsAvailableInAtrFileAdapter.GetValue(this); } 
            set { DataObjectCardServiceDataAdapter.dataObjectsAvailableInAtrFileAdapter.SetValue(this,value); } 
        }

        public DataObjectCardServiceDataAdapter(CompactTlvDataObjectCardServiceData value)
            : base(value)
        {
            this.value = value;
        }
    }
}