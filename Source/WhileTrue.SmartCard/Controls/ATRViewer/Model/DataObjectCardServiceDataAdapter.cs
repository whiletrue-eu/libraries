using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectCardServiceDataAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectCardServiceData value;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> dataObjectsAvailableInAtrFileAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> dataObjectsAvailableInDirFileAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> supportsApplicationSelectionByFullDfNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, bool> supportsApplicationSelectionByPartialDfNameAdapter;
        private static readonly PropertyAdapter<DataObjectCardServiceDataAdapter, EnumerationAdapter<FileIOServices>> fileIOServicesMethodAdapter;

        static DataObjectCardServiceDataAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCardServiceDataAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCardServiceDataAdapter>();

            dataObjectsAvailableInAtrFileAdapter = PropertyFactory.Create(
                @this => @this.DataObjectsAvailableInAtrFile,
                @this => @this.value.DataObjectsAvailableInAtrFile,
                (@this,value) => @this.value.DataObjectsAvailableInAtrFile=value
                );

            dataObjectsAvailableInDirFileAdapter = PropertyFactory.Create(
                @this => @this.DataObjectsAvailableInDirFile,
                @this => @this.value.DataObjectsAvailableInDirFile,
                (@this,value) => @this.value.DataObjectsAvailableInDirFile=value
                );

            supportsApplicationSelectionByFullDfNameAdapter = PropertyFactory.Create(
                @this => @this.SupportsApplicationSelectionByFullDFName,
                @this => @this.value.SupportsApplicationSelectionByFullDFName,
                (@this,value) => @this.value.SupportsApplicationSelectionByFullDFName=value
                );

            supportsApplicationSelectionByPartialDfNameAdapter = PropertyFactory.Create(
                @this => @this.SupportsApplicationSelectionByPartialDFName,
                @this => @this.value.SupportsApplicationSelectionByPartialDFName,
                (@this,value) => @this.value.SupportsApplicationSelectionByPartialDFName=value
                );

            fileIOServicesMethodAdapter = PropertyFactory.Create(
                @this => @this.FileIOServicesMethod,
                @this => EnumerationAdapter < FileIOServices > .GetInstanceFor(@this.value.FileIOServicesMethod),
                (@this,value) => @this.value.FileIOServicesMethod=value
                );
        }

        public EnumerationAdapter<FileIOServices> FileIOServicesMethod
        {
            get { return fileIOServicesMethodAdapter.GetValue(this); } 
            set { fileIOServicesMethodAdapter.SetValue(this,value); } 
        }

        public IEnumerable<EnumerationAdapter<FileIOServices>> FileIOServicesMethodValues
        {
            get { return EnumerationAdapter<FileIOServices>.Items; }
        }

        public bool SupportsApplicationSelectionByPartialDFName
        {
            get { return supportsApplicationSelectionByPartialDfNameAdapter.GetValue(this); } 
            set { supportsApplicationSelectionByPartialDfNameAdapter.SetValue(this,value); } 
        }

        public bool SupportsApplicationSelectionByFullDFName
        {
            get { return supportsApplicationSelectionByFullDfNameAdapter.GetValue(this); } 
            set { supportsApplicationSelectionByFullDfNameAdapter.SetValue(this,value); } 
        }

        public bool DataObjectsAvailableInDirFile
        {
            get { return dataObjectsAvailableInDirFileAdapter.GetValue(this); } 
            set { dataObjectsAvailableInDirFileAdapter.SetValue(this,value); } 
        }

        public bool DataObjectsAvailableInAtrFile
        {
            get { return dataObjectsAvailableInAtrFileAdapter.GetValue(this); } 
            set { dataObjectsAvailableInAtrFileAdapter.SetValue(this,value); } 
        }

        public DataObjectCardServiceDataAdapter(CompactTLVDataObjectCardServiceData value)
            : base(value)
        {
            this.value = value;
        }
    }
}