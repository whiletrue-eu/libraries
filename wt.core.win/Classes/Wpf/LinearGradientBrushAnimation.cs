using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     Provides animation capability for a <see cref="LinearGradientBrush" />.
    /// </summary>
    /// <remarks>
    ///     Derived classes:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>
    ///                 <see cref="LinearGradientBrushAnimation" />
    ///             </term>
    ///         </item>
    ///     </list>
    /// </remarks>
    public abstract class LinearGradientBrushAnimationBase : AnimationTimeline
    {
        /// <summary />
        public override Type TargetPropertyType => typeof(LinearGradientBrush);

        /// <summary />
        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue,
            AnimationClock animationClock)
        {
            if (defaultOriginValue is LinearGradientBrush && defaultDestinationValue is LinearGradientBrush)
                return
                    GetCurrentValueCore((LinearGradientBrush) defaultOriginValue,
                        (LinearGradientBrush) defaultDestinationValue, animationClock);
            return defaultDestinationValue;
        }

        /// <summary />
        protected abstract LinearGradientBrush GetCurrentValueCore(LinearGradientBrush defaultOriginValue,
            LinearGradientBrush defaultDestinationValue, AnimationClock animationClock);
    }

    /// <summary>
    ///     Provides animation capability for a <see cref="LinearGradientBrush" />.
    /// </summary>
    /// <remarks>
    ///     Limitations:
    ///     <list type="bullet">
    ///         <item>
    ///             <term>You can only animate linear gradients with the same number of gradient stops.</term>
    ///         </item>
    ///     </list>
    ///     The following values are animated:
    ///     <list type="bullet">
    ///         <item>
    ///             <term><see cref="LinearGradientBrush" />.<see cref="LinearGradientBrush.StartPoint" /></term>
    ///         </item>
    ///         <item>
    ///             <term><see cref="LinearGradientBrush" />.<see cref="LinearGradientBrush.EndPoint" /></term>
    ///         </item>
    ///         <item>
    ///             <term><see cref="GradientStop" />.<see cref="GradientStop.Offset" /></term>
    ///         </item>
    ///         <item>
    ///             <term><see cref="GradientStop" />.<see cref="GradientStop.Color" /></term>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class LinearGradientBrushAnimation : LinearGradientBrushAnimationBase
    {
        private PointAnimation endPointAnimator;
        private GradientStopAnimator[] gradientStopAnimations;
        private PointAnimation startPointAnimator;
        private LinearGradientBrush to;

        /// <summary>
        ///     Gets/Sets the linear gradient brush the animation shall end with.
        /// </summary>
        public LinearGradientBrush To
        {
            get => to;
            set
            {
                to = value;
                UpdateGradientAnimation();
            }
        }

        /// <summary />
        public override bool IsDestinationDefault => false;

        /// <summary />
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == DurationProperty) UpdateGradientAnimation();
        }

        private void UpdateGradientAnimation()
        {
            if (to != null)
            {
                startPointAnimator = new PointAnimation(to.StartPoint, Duration);
                endPointAnimator = new PointAnimation(to.EndPoint, Duration);

                gradientStopAnimations = new GradientStopAnimator[to.GradientStops.Count];

                for (var GradientStopIndex = 0; GradientStopIndex < to.GradientStops.Count; GradientStopIndex++)
                    gradientStopAnimations[GradientStopIndex] =
                        new GradientStopAnimator(to.GradientStops[GradientStopIndex], Duration);
            }
            else
            {
                startPointAnimator = null;
                endPointAnimator = null;
                gradientStopAnimations = null;
            }
        }


        /// <summary />
        protected override Freezable CreateInstanceCore()
        {
            return new LinearGradientBrushAnimation();
        }

        /// <summary />
        protected override LinearGradientBrush GetCurrentValueCore(LinearGradientBrush defaultOriginValue,
            LinearGradientBrush defaultDestinationValue, AnimationClock animationClock)
        {
            if (defaultOriginValue.GradientStops.Count != (to ?? defaultDestinationValue).GradientStops.Count)
                throw new InvalidOperationException(
                    "When using linear gradient animation, make sure both gradients have the same number of gradient stops");

            var GradientBrush = new LinearGradientBrush();
            GradientBrush.StartPoint = startPointAnimator.GetCurrentValue(defaultOriginValue.StartPoint,
                defaultDestinationValue.StartPoint, animationClock);
            GradientBrush.EndPoint = endPointAnimator.GetCurrentValue(defaultOriginValue.EndPoint,
                defaultDestinationValue.EndPoint, animationClock);

            for (var GradientStopIndex = 0; GradientStopIndex < gradientStopAnimations.Length; GradientStopIndex++)
            {
                var GradientStop = gradientStopAnimations[GradientStopIndex].GetCurrentValue(
                    defaultOriginValue.GradientStops[GradientStopIndex],
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
                offsetAnimator = new DoubleAnimation(gradientStop.Offset, duration);
                colorAnimator = new ColorAnimation(gradientStop.Color, duration);
            }

            public GradientStop GetCurrentValue(GradientStop defaultOriginValue, GradientStop defaultDestinationvalue,
                AnimationClock animationClock)
            {
                return new GradientStop(
                    colorAnimator.GetCurrentValue(defaultOriginValue.Color, defaultDestinationvalue.Color,
                        animationClock),
                    offsetAnimator.GetCurrentValue(defaultOriginValue.Offset, defaultDestinationvalue.Offset,
                        animationClock)
                );
            }
        }

        #endregion
    }
}