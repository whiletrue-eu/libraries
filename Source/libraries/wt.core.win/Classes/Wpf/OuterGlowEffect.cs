using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using JetBrains.Annotations;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// IMplements an outer glow effect (just like the obsolete outer glow bitmap effect) on basis of a pixel shader
    /// </summary>
    [PublicAPI]
    public class OuterGlowEffect : ShaderEffect
    {
        /// <summary>
        /// input graphics used to render the outer glow for. Glow is applied for alpha transparent parts of the brush
        /// </summary>
        public static readonly DependencyProperty InputProperty;
        /// <summary>
        /// radius of the outer glow effect
        /// </summary>
        public static readonly DependencyProperty RadiusProperty;
        /// <summary>
        /// Intensity of the outer glow
        /// </summary>
        public static readonly DependencyProperty IntensityProperty;
        /// <summary>
        /// brush used for the color of outer glow
        /// </summary>
        public static readonly DependencyProperty GlowColorProperty;

        static OuterGlowEffect()
        {
            OuterGlowEffect.InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof (OuterGlowEffect), 0);
            OuterGlowEffect.IntensityProperty = DependencyProperty.Register(
                "Intensity",
                typeof(double),
                typeof(OuterGlowEffect),
                new UIPropertyMetadata(1D, ShaderEffect.PixelShaderConstantCallback(0))
                );
            OuterGlowEffect.RadiusProperty = DependencyProperty.Register(
                "Radius",
                typeof (double),
                typeof (OuterGlowEffect),
                new UIPropertyMetadata(1D,
                                       delegate(DependencyObject d, DependencyPropertyChangedEventArgs e)
                                           {
                                               ShaderEffect.PixelShaderConstantCallback(1)(d, e);
                                               ((OuterGlowEffect) d).PaddingLeft = (double) e.NewValue;
                                               ((OuterGlowEffect) d).PaddingTop = (double) e.NewValue;
                                               ((OuterGlowEffect) d).PaddingRight = (double) e.NewValue;
                                               ((OuterGlowEffect) d).PaddingBottom = (double) e.NewValue;
                                           })
                );
            OuterGlowEffect.GlowColorProperty = DependencyProperty.Register(
                "GlowColor",
                typeof (Color),
                typeof (OuterGlowEffect),
                new UIPropertyMetadata(Color.FromArgb(255, 0, 0, 0), ShaderEffect.PixelShaderConstantCallback(2))
                );
        }

        /// <summary/>
        public OuterGlowEffect()
        {
            this.DdxUvDdyUvRegisterIndex = 3;
            PixelShader PixelShader = new PixelShader();
            PixelShader.UriSource = new Uri("/wt.core.win;component/Classes/Wpf/OuterGlow.ps", UriKind.Relative);
            this.PixelShader = PixelShader;

            this.UpdateShaderValue(OuterGlowEffect.InputProperty);
            this.UpdateShaderValue(OuterGlowEffect.RadiusProperty);
            this.UpdateShaderValue(OuterGlowEffect.IntensityProperty);
            this.UpdateShaderValue(OuterGlowEffect.GlowColorProperty);
        }
        /// <summary>
        /// input graphics used to render the outer glow for. Glow is applied for alpha transparent parts of the brush
        /// </summary>
        public Brush Input
        {
            get
            {
                return ((Brush)(this.GetValue(OuterGlowEffect.InputProperty)));
            }
            set
            {
                this.SetValue(OuterGlowEffect.InputProperty, value);
            }
        }
        /// <summary>
        /// radius of the outer glow effect
        /// </summary>
        public double Radius
        {
            get
            {
                return ((double)(this.GetValue(OuterGlowEffect.RadiusProperty)));
            }
            set
            {
                this.SetValue(OuterGlowEffect.RadiusProperty, value);
            }
        }
        /// <summary>Glow intensity</summary>
        public double Intensity
        {
            get
            {
                return ((double)(this.GetValue(OuterGlowEffect.IntensityProperty)));
            }
            set
            {
                this.SetValue(OuterGlowEffect.IntensityProperty, value);
            }
        }
        /// <summary>Glow color</summary>
        public Color GlowColor
        {
            get
            {
                return ((Color)(this.GetValue(OuterGlowEffect.GlowColorProperty)));
            }
            set
            {
                this.SetValue(OuterGlowEffect.GlowColorProperty, value);
            }
        }
    }
}