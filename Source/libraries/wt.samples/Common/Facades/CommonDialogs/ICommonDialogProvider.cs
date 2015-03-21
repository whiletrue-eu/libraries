using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.Common.Facades.CommonDialogs
{
    ///<summary>
    /// Provides UI independency to common dialog functiona, such as displaying messages and File dialogs
    ///</summary>
    [ComponentInterface]
    public interface ICommonDialogProvider
    {
        ///<summary>
        /// Shows the exception message as a error message box
        ///</summary>
        void ShowError(Exception exception);
    }
}
