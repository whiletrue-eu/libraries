using System.Threading;
using System.Windows.Forms;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    public abstract class FormsOwnerThread : ThreadBase
    {
        private bool abortThread;
        private Form form;

        public FormsOwnerThread()
            : base("FormsOwnerThread", ThreadPriority.Normal, true)
        {
        }

        public Form Form
        {
            get { return this.form; }
        }

        protected override void Initialise()
        {
            this.form = this.CreateForm();
            this.form.Handle.ToString();
            Application.DoEvents();
        }

        protected abstract Form CreateForm();

        protected override void Run()
        {
            while (this.abortThread != true)
            {
                Thread.Sleep(1);
                Win32.DoEvents(this.form);
            }

            this.form.Dispose();
        }

        public override void Stop()
        {
            this.abortThread = true;
        }
    }
}