using System;
using System.Windows.Input;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.ProgressOutput;

namespace WhileTrue.Controls
{
    ///<summary>
    /// carries the properties for progress indicator
    ///</summary>
    public class Progress : ObservableObject, IProgress
    {
        private string status;

        /// <summary>
        /// Sets/gets the status message shown for progess
        /// </summary>
        public string Status
        {
            get { return this.status; }
            set { this.SetAndInvoke(nameof(this.Status), ref this.status, value); }
        }

        private ICommand cancelCommand;

        /// <summary>
        /// If set, the action can be cancelled. The cancelling is invoked from the UI thread
        /// </summary>
        public ICommand CancelCommand
        {
            get { return this.cancelCommand; }
            set { this.SetAndInvoke(nameof(this.CancelCommand), ref this.cancelCommand, value); }
        }

        private double currentProgress = Double.NaN;

        /// <summary>
        /// Progress indication
        /// </summary>
        /// <remarks>
        /// values from 0 to 1 indicate progress from 0% to 100%. The Value <c>double.NaN</c> indicates no progress
        /// indication is available, <c>double.PositiveInfinity</c> indicates indetermined status.
        /// </remarks>
        public double CurrentProgress
        {
            get { return this.currentProgress; }
            set { this.SetAndInvoke(nameof(this.CurrentProgress), ref this.currentProgress, value); }
        }
    }
}