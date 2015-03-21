using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WhileTrue.Classes.ATR;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls.ATRView
{
    public class ProtocolParameterAdapterBase : ObservableObject
    {
        private readonly ProtocolParametersBase protocolParameters;
        private static readonly EnumerablePropertyAdapter<ProtocolParameterAdapterBase, ParameterByte, ProtocolParameterByteAdapterBase> protocolParameterBytes;
        private DelegateCommand removeIndicationCommand;

        public static ProtocolParameterAdapterBase GetObject(ProtocolParametersBase protocolParameters)
        {
            if( protocolParameters is T0ProtocolParameters )
            {
                return new T0ProtocolParameterAdapter((T0ProtocolParameters)protocolParameters);
            }
            else if( protocolParameters is T1ProtocolParameters )
            {
                return new T1ProtocolParameterAdapter((T1ProtocolParameters)protocolParameters);
            }
            else if( protocolParameters is UnknownProtocolParameters )
            {
                return new UnknownProtocolParameterAdapter((UnknownProtocolParameters)protocolParameters);
            }
            else
            {
                throw new ArgumentException();
            }
        }

        static ProtocolParameterAdapterBase()
        {
            IPropertyAdapterFactory<ProtocolParameterAdapterBase> PropertyFactory = ObservableObject.GetPropertyAdapterFactory<ProtocolParameterAdapterBase>();

            protocolParameterBytes = PropertyFactory.Create(
                @this => @this.ProtocolParameterBytes,
                @this => @this.protocolParameters.ParameterBytes,
                (@this,parameterByte)=> ProtocolParameterByteAdapterBase.GetObject(parameterByte)
                );            
        }

        public ProtocolParameterAdapterBase(ProtocolParametersBase protocolParameters)
        {
            this.protocolParameters = protocolParameters;

            this.removeIndicationCommand = new DelegateCommand(this.RemoveIndication, () => this.protocolParameters.IsOnlyIndicatedProtocol==false);
        }

        private void RemoveIndication()
        {
            this.protocolParameters.RemoveIndication();
        }

        public IEnumerable<ProtocolParameterByteAdapterBase> ProtocolParameterBytes
        {
            get { return protocolParameterBytes.GetCollection(this); }
        }

        public ICommand RemoveIndicationCommand
        {
            get { return this.removeIndicationCommand; }
        }
    }
}