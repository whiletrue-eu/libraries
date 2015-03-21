using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Describes the glass margin of a <see cref="Window"/> that supports aero glass effect
    /// </summary>
    [TypeConverter(typeof (GlassMarginTypeConverter))]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class GlassMargin : ObservableObject
    {
        private double bottom;
        private double left;
        private double right;
        private double top;
        /// <summary>
        /// THh one and only instance that describes that 'glass sheet' mode shall be used
        /// </summary>
        public static readonly GlassMargin Sheet = new GlassMargin(-1);

        /// <summary/>
        public GlassMargin()
        {
            this.left = 0;
            this.top = 0;
            this.right = 0;
            this.bottom = 0;
        }

        /// <summary/>
        public GlassMargin(double all)
        {
            this.left = all;
            this.top = all;
            this.right = all;
            this.bottom = all;
        }

        /// <summary/>
        public GlassMargin(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }


        /// <summary>
        /// Left margin of a glass border
        /// </summary>
        public double Left
        {
            get { return this.left; }
            set
            {
                if (this.left != value)
                {
                    this.left = value;
                    this.InvokePropertyChanged(nameof(this.Left));
                }
            }
        }

        /// <summary>
        /// Top margin of a glass border
        /// </summary>
        public double Top
        {
            get { return this.top; }
            set
            {
                if (this.top != value)
                {
                    this.top = value;
                    this.InvokePropertyChanged(nameof(this.Top));
                }
            }
        }

        /// <summary>
        /// Right margin of a glass border
        /// </summary>
        public double Right
        {
            get { return this.right; }
            set
            {
                if (this.right != value)
                {
                    this.right = value;
                    this.InvokePropertyChanged(nameof(this.Right));
                }
            }
        }

        /// <summary>
        /// Bottom margin of a glass border
        /// </summary>
        public double Bottom
        {
            get { return this.bottom; }
            set
            {
                if (this.bottom != value)
                {
                    this.bottom = value;
                    this.InvokePropertyChanged(nameof(this.Bottom));
                }
            }
        }
    }
}