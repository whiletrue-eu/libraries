using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhileTrue.Classes.Forms
{
    /// <remarks>
    ///public class FadeTransitionAnimationExtension : TransitionAnimationExtensionBase
    ///{
    ///    protected override async Task StartInAnimationAsync(VisualElement visualElement)
    ///    {
    ///        await visualElement.FadeTo(1, 500, Easing.Linear);
    ///    }
    ///    protected override async Task StartOutAnimationAsync(VisualElement visualElement)
    ///    {
    ///        await visualElement.FadeTo(0, 500, Easing.Linear);
    ///    }
    ///}
    ///public class ScaleTransitionAnimationExtension : TransitionAnimationExtensionBase
    ///{
    ///    protected override async Task StartInAnimationAsync(VisualElement visualElement)
    ///    {
    ///        await visualElement.ScaleTo(1, 250, Easing.CubicIn);
    ///    }
    ///    protected override async Task StartOutAnimationAsync(VisualElement visualElement)
    ///    {
    ///        await visualElement.ScaleTo(0, 250, Easing.CubicOut);
    ///    }
    ///}
    /// </remarks>
    [PublicAPI]
    public abstract class TransitionAnimationExtensionBase : BindableObject, IMarkupExtension, INotifyPropertyChanged
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(@"Source", typeof(object), typeof(TransitionAnimationExtensionBase), null, BindingMode.OneWay,null, TransitionAnimationExtensionBase.SourceChanged);

        public object Source
        {
            set
            {
                if (value is BindingBase)
                {
                    this.SetBinding(TransitionAnimationExtensionBase.SourceProperty, (BindingBase)value);
                }
                else
                {
                    this.SetValue(TransitionAnimationExtensionBase.SourceProperty, value);
                }
            }
        }

        public bool AnimateInitially { get; set; }
        public IValueConverter Converter { get; set; }
        private bool isInitialValue = true;

        private object value;
        private BindableObject bindableObject;
        private VisualElement visualElement;
        private IValueConverter imageConverter;

        private static void SourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((TransitionAnimationExtensionBase)bindable).SetNewValue(newvalue);
        }

        public object Value
        {
            get { return this.value; }
            private set
            {
                this.value = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Value)));
            }
        }



        private void SetNewValue(object newvalue)
        {
            if (this.AnimateInitially == false && this.isInitialValue)
            {
                this.isInitialValue = false;
                this.Value = newvalue;
            }
            else if (this.visualElement != null)
            {
                Task.Run(async () =>
                {
                    await this.StartOutAnimationAsync(this.visualElement);
                    this.Value = newvalue;
                    await this.StartInAnimationAsync(this.visualElement);
                });
            }
            else
            {
                this.Value = newvalue;
            }
        }

        protected abstract Task StartInAnimationAsync(VisualElement visualElement1);

        protected abstract Task StartOutAnimationAsync(VisualElement visualElement1);


        public object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget Provider = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));

            this.visualElement = Provider.TargetObject as VisualElement;
            this.bindableObject = Provider.TargetObject as BindableObject;
            if (this.bindableObject != null)
            {
                this.bindableObject.BindingContextChanged += this.TransitionAnimationExtension_BindingContextChanged;
            }
            
            return new Binding("Value", BindingMode.OneWay, source: this, converter: this.Converter);
        }

        private void TransitionAnimationExtension_BindingContextChanged(object sender, EventArgs e)
        {
            this.BindingContext = this.bindableObject.BindingContext;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

