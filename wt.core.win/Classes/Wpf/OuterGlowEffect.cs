using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     IMplements an outer glow effect (just like the obsolete outer glow bitmap effect) on basis of a pixel shader
    /// </summary>
    [PublicAPI]
    public class OuterGlowEffect : ShaderEffect
    {
        /// <summary>
        ///     input graphics used to render the outer glow for. Glow is applied for alpha transparent parts of the brush
        /// </summary>
        public static readonly DependencyProperty InputProperty;

        /// <summary>
        ///     radius of the outer glow effect
        /// </summary>
        public static readonly DependencyProperty RadiusProperty;

        /// <summary>
        ///     Intensity of the outer glow
        /// </summary>
        public static readonly DependencyProperty IntensityProperty;

        /// <summary>
        ///     brush used for the color of outer glow
        /// </summary>
        public static readonly DependencyProperty GlowColorProperty;

        static OuterGlowEffect()
        {
            InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(OuterGlowEffect), 0);
            IntensityProperty = DependencyProperty.Register(
                "Intensity",
                typeof(double),
                typeof(OuterGlowEffect),
                new UIPropertyMetadata(1D, PixelShaderConstantCallback(0))
            );
            RadiusProperty = DependencyProperty.Register(
                "Radius",
                typeof(double),
                typeof(OuterGlowEffect),
                new UIPropertyMetadata(1D,
                    delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
                    {
                        PixelShaderConstantCallback(1)(d, e);
                        ((OuterGlowEffect) d).PaddingLeft = (double) e.NewValue;
                        ((OuterGlowEffect) d).PaddingTop = (double) e.NewValue;
                        ((OuterGlowEffect) d).PaddingRight = (double) e.NewValue;
                        ((OuterGlowEffect) d).PaddingBottom = (double) e.NewValue;
                    })
            );
            GlowColorProperty = DependencyProperty.Register(
                "GlowColor",
                typeof(Color),
                typeof(OuterGlowEffect),
                new UIPropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(2))
            );
        }

        /// <summary />
        public OuterGlowEffect()
        {
            DdxUvDdyUvRegisterIndex = 3;
            var PixelShader = new PixelShader();
            PixelShader.UriSource = new Uri("/wt.core.win;component/Classes/Wpf/OuterGlow.ps", UriKind.Relative);
            this.PixelShader = PixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(RadiusProperty);
            UpdateShaderValue(IntensityProperty);
            UpdateShaderValue(GlowColorProperty);
        }

        /// <summary>
        ///     input graphics used to render the outer glow for. Glow is applied for alpha transparent parts of the brush
        /// </summary>
        public Brush Input
        {
            get => (Brush) GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        ///     radius of the outer glow effect
        /// </summary>
        public double Radius
        {
            get => (double) GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        /// <summary>Glow intensity</summary>
        public double Intensity
        {
            get => (double) GetValue(IntensityProperty);
            set => SetValue(IntensityProperty, value);
        }

        /// <summary>Glow color</summary>
        public Color GlowColor
        {
            get => (Color) GetValue(GlowColorProperty);
            set => SetValue(GlowColorProperty, value);
        }
    }
}