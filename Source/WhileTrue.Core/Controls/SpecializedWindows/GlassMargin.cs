using System.ComponentModel;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Controls
{
    [TypeConverter(typeof (GlassMarginTypeConverter))]
    public class GlassMargin : ObservableObject
    {
        private double bottom;
        private double left;
        private double right;
        private double top;
        public static readonly GlassMargin Sheet = new GlassMargin(-1);

        public GlassMargin()
        {
            this.left = 0;
            this.top = 0;
            this.right = 0;
            this.bottom = 0;
        }

        public GlassMargin(double all)
        {
            this.left = all;
            this.top = all;
            this.right = all;
            this.bottom = all;
        }

        public GlassMargin(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }


        public double Left
        {
            get { return this.left; }
            set
            {
                if (this.left != value)
                {
                    this.left = value;
                    this.InvokePropertyChanged(()=>Left);
                }
            }
        }

        public double Top
        {
            get { return this.top; }
            set
            {
                if (this.top != value)
                {
                    this.top = value;
                    this.InvokePropertyChanged(()=>Top);
                }
            }
        }

        public double Right
        {
            get { return this.right; }
            set
            {
                if (this.right != value)
                {
                    this.right = value;
                    this.InvokePropertyChanged(()=>Right);
                }
            }
        }

        public double Bottom
        {
            get { return this.bottom; }
            set
            {
                if (this.bottom != value)
                {
                    this.bottom = value;
                    this.InvokePropertyChanged(()=>Bottom);
                }
            }
        }
    }
}