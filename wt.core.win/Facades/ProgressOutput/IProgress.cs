using System.Windows.Input;

namespace WhileTrue.Facades.ProgressOutput
{
    /// <summary>
    /// The interface IProgressOutput provides status notification for lengthy tasks
    /// </summary>	
    public interface IProgress
    {
        /// <summary>
        /// Sets/gets the status message shown for progess
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// If set, the action can be cancelled. The cancelling is invoked from the UI thread
        /// </summary>
        ICommand CancelCommand { get; set; }

        /// <summary>
        /// Progress indication
        /// </summary>
        /// <remarks>
        /// values from 0 to 1 indicate progress from 0% to 100%. The Value <c>double.NaN</c> indicates no progress
        /// indication is available, <c>double.PositiveInfinity</c> indicates indetermined status.
        /// </remarks>
        double CurrentProgress { get; set; }
    }
}