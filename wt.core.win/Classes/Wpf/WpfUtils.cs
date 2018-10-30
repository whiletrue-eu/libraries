using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace WhileTrue.Classes.Wpf
{
    /// <summary />
    public static class WpfUtils
    {
        /// <summary>
        ///     Returns the active window of the application,if any
        /// </summary>
        /// <remarks>
        ///     This function supports calls from other threads as the owning windows ones.
        /// </remarks>
        public static Window FindActiveWindow()
        {
            if (Application.Current != null)
            {
                var Windows = Application.Current.Invoke(application => application.Windows.Cast<Window>().ToArray());

                var ActiveTopmostWindows = from Window Window in Windows
                    where Window.Invoke(window => window.IsActive)
                    where Window.Invoke(window =>
                        Windows.Any(potentionalChild => potentionalChild.Owner == Window) == false)
                    select Window;

                return ActiveTopmostWindows.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        ///     Invokes the given method through the dispatcher and returns the result
        /// </summary>
        /// <remarks>
        ///     in case <c>control</c> is null, the given delegate is called directly
        /// </remarks>
        public static TResult Invoke<TControl, TResult>(this TControl control, Func<TControl, TResult> action)
            where TControl : DispatcherObject
        {
            if (control != null)
            {
                var Result = default(TResult);
                control.Dispatcher.Invoke(delegate { Result = action(control); });
                return Result;
            }

            return action(null);
        }

        /// <summary>
        ///     Invokes the given result-less method through the dispatcher
        /// </summary>
        /// <remarks>
        ///     in case <c>control</c> is null, the given delegate is called directly
        /// </remarks>
        public static void Invoke<TControl>(this TControl control, Action<TControl> action)
            where TControl : DispatcherObject
        {
            if (control != null)
                control.Dispatcher.Invoke(delegate { action(control); });
            else
                action(null);
        }
    }
}