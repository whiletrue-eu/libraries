using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace WhileTrue.Classes.Wpf
{
    public class OuterGlowEffect : ShaderEffect
    {
        public static readonly DependencyProperty InputProperty;
        public static readonly DependencyProperty RadiusProperty;
        public static readonly DependencyProperty IntensityProperty;
        public static readonly DependencyProperty GlowColorProperty;

        static OuterGlowEffect()
        {
            InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof (OuterGlowEffect), 0);
            IntensityProperty = DependencyProperty.Register(
                "Intensity",
                typeof(double),
                typeof(OuterGlowEffect),
                new UIPropertyMetadata(1D, PixelShaderConstantCallback(0))
                );
            RadiusProperty = DependencyProperty.Register(
                "Radius",
                typeof (double),
                typeof (OuterGlowEffect),
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
                typeof (Color),
                typeof (OuterGlowEffect),
                new UIPropertyMetadata(Color.FromArgb(255, 0, 0, 0), PixelShaderConstantCallback(2))
                );
        }

        public OuterGlowEffect()
        {
            this.DdxUvDdyUvRegisterIndex = 3;
            PixelShader pixelShader = new PixelShader();
            pixelShader.UriSource = new Uri("/whiletrue.core;component/Classes/Wpf/OuterGlow.ps", UriKind.Relative);
            this.PixelShader = pixelShader;

            this.UpdateShaderValue(InputProperty);
            this.UpdateShaderValue(RadiusProperty);
            this.UpdateShaderValue(IntensityProperty);
            this.UpdateShaderValue(GlowColorProperty);
        }
        public Brush Input
        {
            get
            {
                return ((Brush)(this.GetValue(InputProperty)));
            }
            set
            {
                this.SetValue(InputProperty, value);
            }
        }
        /// <summary>Glow radius</summary>
        public double Radius
        {
            get
            {
                return ((double)(this.GetValue(RadiusProperty)));
            }
            set
            {
                this.SetValue(RadiusProperty, value);
            }
        }
        /// <summary>Glow intensity</summary>
        public double Intensity
        {
            get
            {
                return ((double)(this.GetValue(IntensityProperty)));
            }
            set
            {
                this.SetValue(IntensityProperty, value);
            }
        }
        /// <summary>Glow color</summary>
        public Color GlowColor
        {
            get
            {
                return ((Color)(this.GetValue(GlowColorProperty)));
            }
            set
            {
                this.SetValue(GlowColorProperty, value);
            }
        }
    }
}