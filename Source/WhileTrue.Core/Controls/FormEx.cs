using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace WhileTrue.Controls
{
    /// <summary>
    /// Summary description for FormEx.
    /// </summary>
    [Designer("System.Windows.Forms.Design.FormDocumentDesigner, System.Windows.Forms", typeof (IDesigner))]
    //[Designer("System.Windows.Forms.Design.FormDocumentDesigner,")]
    public class FormEx : Form
    {
        private static FormEx lastActiveWindow;

        /// <summary>
        /// Stores the window that was active before the form was shown, e.g. the parent form of the new form 
        /// </summary>
        public static Form LastActiveWindow
        {
            get
            {
                if (ActiveForm != null)
                {
                    return ActiveForm;
                }
                else
                {
                    if (lastActiveWindow != null)
                    {
                        return lastActiveWindow;
                    }
                    else
                    {
                        return new Form();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the static property LastActiveWindow to this 
        /// used in MessageboxEx
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            lastActiveWindow = this;
            base.OnActivated(e);
        }
    }
}