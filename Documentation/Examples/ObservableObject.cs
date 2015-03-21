using System;
using System.Collections.Generic;
using System.Linq;
using WhileTrue.Classes.Framework;
using WhileTrue.Classes.Utilities;

namespace DocExamples
{
    public class ObservableObject1 : WhileTrue.Classes.Framework.ObservableObject
    {
        #region Invoke

        private string myProperty;

        public string MyProperty
        {
            get { return this.myProperty; }
            set
            {
                this.InvokePropertyChanging(() => this.MyProperty);
                this.myProperty = value;
                this.InvokePropertyChanged(() => this.MyProperty);
            }
        }

        #endregion
    }

    public class ObservableObject2 : WhileTrue.Classes.Framework.ObservableObject
    {
        #region SetAndInvoke

        private string myProperty;

        public string MyProperty
        {
            get { return this.myProperty; }
            set { this.SetAndInvoke(() => this.MyProperty, ref this.myProperty, value); }
        }

        #endregion
    }

    public class ObservableObject3 : WhileTrue.Classes.Framework.ObservableObject
    {
        #region SetAndInvokeWithCustomEvents

        private string myProperty;

        public string MyProperty
        {
            get { return this.myProperty; }
            set
            {
                this.SetAndInvoke(
                    () => this.MyProperty,
                    ref this.myProperty,
                    value,
                    _ => this.MyPropertyChanging(this, EventArgs.Empty),
                    _ => this.MyPropertyChanged(this, EventArgs.Empty));
            }
        }

        #endregion

        private void MyPropertyChanged(ObservableObject3 observableObject3, EventArgs empty)
        {
            throw new NotImplementedException();
        }

        private void MyPropertyChanging(ObservableObject3 observableObject3, EventArgs empty)
        {
            throw new NotImplementedException();
        }

    }

    public class ObservableObject4
    {

        public class ModelAdapter : WhileTrue.Classes.Framework.ObservableObject
        {

            public interface IModel
            {
                byte[] SourceProperty { get; }
            }

            #region ReadonlyPropertyAdapter

            // public interface IModel
            // {
            //    byte[] SourceProperty { get; }
            // }

            private readonly ReadOnlyPropertyAdapter<string> readOnlyPropertyAdapter;

            public ModelAdapter(IModel model)
            {
                this.readOnlyPropertyAdapter = this.CreatePropertyAdapter(
                    () => this.ReadOnlyProperty,
                    () => model.SourceProperty.ToHexString(" ")
                    );
            }

            public string ReadOnlyProperty
            {
                get { return this.readOnlyPropertyAdapter.GetValue(); }
            }

            #endregion
        }
    }

    public class ObservableObject5
    {

        public class ModelAdapter : WhileTrue.Classes.Framework.ObservableObject
        {

            public interface IModel
            {
                byte[] SourceProperty { get; set; }
            }

            #region PropertyAdapter

            // public interface IModel
            // {
            //    byte[] SourceProperty { get; set; }
            // }

            private readonly PropertyAdapter<string> propertyAdapter;

            public ModelAdapter(IModel model)
            {
                this.propertyAdapter = this.CreatePropertyAdapter(
                    () => this.Property,
                    () => model.SourceProperty.ToHexString(" "),
                    value => model.SourceProperty = value.ToByteArray()
                    );
            }

            public string Property
            {
                get { return this.propertyAdapter.GetValue(); }
                set { this.propertyAdapter.SetValue(value); }
            }

            #endregion
        }
    }

    public class ObservableObject6
    {

        public class ModelAdapter : WhileTrue.Classes.Framework.ObservableObject
        {
            public interface IModel
            {
                byte[] SourceProperty { get; }
            }

            public class SourcePropertyElementAdapter : ObservableObject
            {
                public SourcePropertyElementAdapter(byte item)
                {
                }

                public static SourcePropertyElementAdapter GetObject(byte item)
                {
                    throw new NotImplementedException();
                }
            }

            #region EnumerablePropertyAdapter

            // public interface IModel
            // {
            //    byte[] SourceProperty { get; }
            // }

            private readonly EnumerablePropertyAdapter<byte, SourcePropertyElementAdapter> enumerablePropertyAdapter;

            public ModelAdapter(IModel model)
            {
                this.enumerablePropertyAdapter = this.CreatePropertyAdapter(
                    () => this.EnumerableProperty,
                    () => from Byte in model.SourceProperty
                          where Byte != 0x00
                          orderby Byte.GetHiNibble() ascending
                          select Byte,
                    item => SourcePropertyElementAdapter.GetObject(item)
                    );
            }

            public IEnumerable<SourcePropertyElementAdapter> EnumerableProperty
            {
                get { return this.enumerablePropertyAdapter.GetCollection(); }
            }

            #endregion
        }
    }
}