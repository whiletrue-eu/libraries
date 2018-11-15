using System;
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhileTrue.Classes.Forms
{
    /// <remarks>
    ///     public class FadeTransitionAnimationExtension : TransitionAnimationExtensionBase
    ///     {
    ///     protected override async Task StartInAnimationAsync(VisualElement visualElement)
    ///     {
    ///     await visualElement.FadeTo(1, 500, Easing.Linear);
    ///     }
    ///     protected override async Task StartOutAnimationAsync(VisualElement visualElement)
    ///     {
    ///     await visualElement.FadeTo(0, 500, Easing.Linear);
    ///     }
    ///     }
    ///     public class ScaleTransitionAnimationExtension : TransitionAnimationExtensionBase
    ///     {
    ///     protected override async Task StartInAnimationAsync(VisualElement visualElement)
    ///     {
    ///     await visualElement.ScaleTo(1, 250, Easing.CubicIn);
    ///     }
    ///     protected override async Task StartOutAnimationAsync(VisualElement visualElement)
    ///     {
    ///     await visualElement.ScaleTo(0, 250, Easing.CubicOut);
    ///     }
    ///     }
    /// </remarks>
    [PublicAPI]
    public abstract class TransitionAnimationExtensionBase : BindableObject, IMarkupExtension, INotifyPropertyChanged
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(@"Source", typeof(object),
            typeof(TransitionAnimationExtensionBase), null, BindingMode.OneWay, null, SourceChanged);

        private BindableObject bindableObject;
        private IValueConverter imageConverter;
        private bool isInitialValue = true;

        private object value;
        private VisualElement visualElement;

        public object Source
        {
            set
            {
                if (value is BindingBase)
                    SetBinding(SourceProperty, (BindingBase) value);
                else
                    SetValue(SourceProperty, value);
            }
        }

        public bool AnimateInitially { get; set; }
        public IValueConverter Converter { get; set; }

        public object Value
        {
            get => value;
            private set
            {
                this.value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }


        public object ProvideValue(IServiceProvider serviceProvider)
        {
            var Provider = (IProvideValueTarget) serviceProvider.GetService(typeof(IProvideValueTarget));

            visualElement = Provider.TargetObject as VisualElement;
            bindableObject = Provider.TargetObject as BindableObject;
            if (bindableObject != null)
                bindableObject.BindingContextChanged += TransitionAnimationExtension_BindingContextChanged;

            return new Binding("Value", BindingMode.OneWay, source: this, converter: Converter);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void SourceChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((TransitionAnimationExtensionBase) bindable).SetNewValue(newvalue);
        }


        private void SetNewValue(object newvalue)
        {
            if (AnimateInitially == false && isInitialValue)
            {
                isInitialValue = false;
                Value = newvalue;
            }
            else if (visualElement != null)
            {
                Task.Run(async () =>
                {
                    await StartOutAnimationAsync(visualElement);
                    Value = newvalue;
                    await StartInAnimationAsync(visualElement);
                });
            }
            else
            {
                Value = newvalue;
            }
        }

        protected abstract Task StartInAnimationAsync(VisualElement visualElement1);

        protected abstract Task StartOutAnimationAsync(VisualElement visualElement1);

        private void TransitionAnimationExtension_BindingContextChanged(object sender, EventArgs e)
        {
            BindingContext = bindableObject.BindingContext;
        }
    }
}