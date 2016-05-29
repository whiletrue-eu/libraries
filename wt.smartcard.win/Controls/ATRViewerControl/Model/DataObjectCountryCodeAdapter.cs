using System;
using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRViewerControl.Model
{
    public class DataObjectCountryCodeAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTlvDataObjectCountryCode value;
        private static readonly PropertyAdapter<DataObjectCountryCodeAdapter, Country> countryAdapter;
        private static readonly PropertyAdapter<DataObjectCountryCodeAdapter, string> nationalDateAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCountryCodeAdapter, string> countryTextAdapter;

        static DataObjectCountryCodeAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCountryCodeAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCountryCodeAdapter>();

            DataObjectCountryCodeAdapter.countryAdapter = PropertyFactory.Create(
                nameof(DataObjectCountryCodeAdapter.Country),
                instance=>instance.value.Country,
                (instance,value)=>instance.value.Country=value
                );

            DataObjectCountryCodeAdapter.countryTextAdapter = PropertyFactory.Create(
                nameof(DataObjectCountryCodeAdapter.CountryText),
                instance=>instance.value.Country.ToString()
                );

            DataObjectCountryCodeAdapter.nationalDateAdapter= PropertyFactory.Create(
                nameof(DataObjectCountryCodeAdapter.NationalDate),
                instance => instance.value.NationalDate,
                (instance, value) => instance.value.NationalDate = value
                );
        }

        public string NationalDate
        {
            get { return DataObjectCountryCodeAdapter.nationalDateAdapter.GetValue(this); }
            set { DataObjectCountryCodeAdapter.nationalDateAdapter.SetValue(this,value);}
        }

        public Country Country
        {
            get { return DataObjectCountryCodeAdapter.countryAdapter.GetValue(this); }
            set { DataObjectCountryCodeAdapter.countryAdapter.SetValue(this, value); }
        }

        public string CountryText
        {
            get { return DataObjectCountryCodeAdapter.countryTextAdapter.GetValue(this); }
            set 
            {
                if (value != this.Country.ToString())
                {
                    try
                    {
                        this.value.Country = Country.GetFromNumberCode(value);
                    }
                    catch
                    {
                        throw new Exception("type the first characters of a country to select an known country\nor the 3-decimal-digit number code of the country");
                    }
                }//workaround for combobox seting the selected enumeration value as string when user selects something. We ignore that to display th underlying ushort value
            }
        }

        public IEnumerable<Country> CountryValues => Country.Countries;

        public DataObjectCountryCodeAdapter(CompactTlvDataObjectCountryCode value)
            : base(value)
        {
            this.value = value;
        }
    }
}