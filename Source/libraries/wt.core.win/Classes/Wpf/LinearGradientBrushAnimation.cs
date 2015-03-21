using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    /// Provides animation capability for a <see cref="LinearGradientBrush"/>.
    /// </summary>
    /// <remarks>
    /// Derived classes:
    /// <list type="bullet">
    /// <item>
    /// <term><see cref="LinearGradientBrushAnimation"/></term>
    /// </item>
    /// </list>
    /// </remarks>
    public abstract class LinearGradientBrushAnimationBase : AnimationTimeline
    {
        /// <summary/>
        public override Type TargetPropertyType => typeof (LinearGradientBrush);

        /// <summary/>
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue is LinearGradientBrush && defaultDestinationValue is LinearGradientBrush)
            {
                return
                    this.GetCurrentValueCore((LinearGradientBrush) defaultOriginValue,
                                             (LinearGradientBrush) defaultDestinationValue, animationClock);
            }
            else
            {
                //Fallback: do not animate
                return defaultDestinationValue;
            }
        }

        /// <summary/>
        protected abstract LinearGradientBrush GetCurrentValueCore(LinearGradientBrush defaultOriginValue, LinearGradientBrush defaultDestinationValue, AnimationClock animationClock);
    }

    /// <summary>
    /// Provides animation capability for a <see cref="LinearGradientBrush"/>.
    /// </summary>
    /// <remarks>
    /// Limitations:
    /// <list type="bullet">
    /// <item>
    /// <term>You can only animate linear gradients with the same number of gradient stops.</term>
    /// </item>
    /// </list>
    /// The following values are animated:
    /// <list type="bullet">
    /// <item><term><see cref="LinearGradientBrush"/>.<see cref="LinearGradientBrush.StartPoint"/></term></item>
    /// <item><term><see cref="LinearGradientBrush"/>.<see cref="LinearGradientBrush.EndPoint"/></term></item>
    /// <item><term><see cref="GradientStop"/>.<see cref="GradientStop.Offset"/></term></item>
    /// <item><term><see cref="GradientStop"/>.<see cref="GradientStop.Color"/></term></item>
    /// </list>
    /// </remarks>
    public class LinearGradientBrushAnimation : LinearGradientBrushAnimationBase
    {
        private PointAnimation endPointAnimator;
        private GradientStopAnimator[] gradientStopAnimations;
        private PointAnimation startPointAnimator;
        private LinearGradientBrush to;

        /// <summary>
        /// Gets/Sets the linear gradient brush the animation shall end with.
        /// </summary>
        public LinearGradientBrush To
        {
            get { return this.to; }
            set
            {
                this.to = value;
                this.UpdateGradientAnimation();
            }
        }

        /// <summary/>
        public override bool IsDestinationDefault => false;

        /// <summary/>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == Timeline.DurationProperty)
            {
                this.UpdateGradientAnimation();
            }
        }

        private void UpdateGradientAnimation()
        {
            if (this.to != null)
            {
                this.startPointAnimator = new PointAnimation(this.to.StartPoint, this.Duration);
                this.endPointAnimator = new PointAnimation(this.to.EndPoint, this.Duration);

                this.gradientStopAnimations = new GradientStopAnimator[this.to.GradientStops.Count];

                for (int GradientStopIndex = 0; GradientStopIndex < this.to.GradientStops.Count; GradientStopIndex++)
                {
                    this.gradientStopAnimations[GradientStopIndex] = new GradientStopAnimator(this.to.GradientStops[GradientStopIndex], this.Duration);
                }
            }
            else
            {
                this.startPointAnimator = null;
                this.endPointAnimator = null;
                this.gradientStopAnimations = null;
            }
        }


        /// <summary/>
        protected override Freezable CreateInstanceCore()
        {
            return new LinearGradientBrushAnimation();
        }

        /// <summary/>
        protected override LinearGradientBrush GetCurrentValueCore(LinearGradientBrush defaultOriginValue, LinearGradientBrush defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue.GradientStops.Count != (this.to??defaultDestinationValue).GradientStops.Count)
            {
                throw new InvalidOperationException("When using linear gradient animation, make sure both gradients have the same number of gradient stops");
            }

            LinearGradientBrush GradientBrush = new LinearGradientBrush();
            GradientBrush.StartPoint = this.startPointAnimator.GetCurrentValue(defaultOriginValue.StartPoint, defaultDestinationValue.StartPoint, animationClock);
            GradientBrush.EndPoint = this.endPointAnimator.GetCurrentValue(defaultOriginValue.EndPoint, defaultDestinationValue.EndPoint, animationClock);

            for (int GradientStopIndex = 0; GradientStopIndex < this.gradientStopAnimations.Length; GradientStopIndex++)
            {
                GradientStop GradientStop = this.gradientStopAnimations[GradientStopIndex].GetCurrentValue(defaultOriginValue.GradientStops[GradientStopIndex],
                                                                                                           defaultDestinationValue.GradientStops[GradientStopIndex], animationClock);
                GradientBrush.GradientStops.Add(GradientStop);
            }

            return GradientBrush;
        }

        #region Nested type: GradientStopAnimator

        private class GradientStopAnimator
        {
            private readonly ColorAnimation colorAnimator;
            private readonly DoubleAnimation offsetAnimator;

            public GradientStopAnimator(GradientStop gradientStop, Duration duration)
            {
                this.offsetAnimator = new DoubleAnimation(gradientStop.Offset, duration);
                this.colorAnimator = new ColorAnimation(gradientStop.Color, duration);
            }

            public GradientStop GetCurrentValue(GradientStop defaultOriginValue, GradientStop defaultDestinationvalue, AnimationClock animationClock)
            {
                return new GradientStop(
                    this.colorAnimator.GetCurrentValue(defaultOriginValue.Color, defaultDestinationvalue.Color, animationClock),
                    this.offsetAnimator.GetCurrentValue(defaultOriginValue.Offset, defaultDestinationvalue.Offset, animationClock)
                    );
            }
        }

        #endregion
    }
}