using System;
using System.Collections.Generic;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class DataObjectCountryCodeAdapter : DataObjectBaseAdapter
    {
        private readonly CompactTLVDataObjectCountryCode value;
        private static readonly PropertyAdapter<DataObjectCountryCodeAdapter, Country> countryAdapter;
        private static readonly PropertyAdapter<DataObjectCountryCodeAdapter, string> nationalDateAdapter;
        private static readonly ReadOnlyPropertyAdapter<DataObjectCountryCodeAdapter, string> countryTextAdapter;

        static DataObjectCountryCodeAdapter()
        {
            ObservableObject.IPropertyAdapterFactory<DataObjectCountryCodeAdapter> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<DataObjectCountryCodeAdapter>();

            countryAdapter = PropertyFactory.Create(
                @this=>@this.Country,
                @this=>@this.value.Country,
                (@this,value)=>@this.value.Country=value
                );

            countryTextAdapter = PropertyFactory.Create(
                @this=>@this.CountryText,
                @this=>@this.value.Country.ToString()
                );

            nationalDateAdapter= PropertyFactory.Create(
                @this => @this.NationalDate,
                @this => @this.value.NationalDate,
                (@this, value) => @this.value.NationalDate = value
                );
        }

        public string NationalDate
        {
            get { return nationalDateAdapter.GetValue(this); }
            set { nationalDateAdapter.SetValue(this,value);}
        }

        public Country Country
        {
            get { return countryAdapter.GetValue(this); }
            set { countryAdapter.SetValue(this, value); }
        }

        public string CountryText
        {
            get { return countryTextAdapter.GetValue(this); }
            set 
            {
                if (value != Country.ToString())
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

        public IEnumerable<Country> CountryValues
        {
            get { return Country.Countries; }
        }

        public DataObjectCountryCodeAdapter(CompactTLVDataObjectCountryCode value)
            : base(value)
        {
            this.value = value;
        }
    }
}