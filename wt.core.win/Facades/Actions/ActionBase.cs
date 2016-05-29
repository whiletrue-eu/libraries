using System;

namespace Mz.Facades.Actions
{
    public abstract class ActionBase : IAction
    {
        private readonly string id;
        private object context;

        protected ActionBase(string id)
        {
            this.id = id;
        }

        #region IAction Members

        public string ID
        {
            get { return this.id; }
        }

        public event EventHandler CanExecuteChanged;


        public abstract void Execute();
        public abstract bool CanExecute { get; }

        public virtual object Context
        {
            set
            {
                if (this.context != value)
                {
                    this.context = value;
                    this.OnContextChanged();
                }
            }
            protected get { return this.context; }
        }

        #endregion

        protected void InvokeCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, new EventArgs());
            }
        }

        protected virtual void OnContextChanged()
        {
            this.InvokeCanExecuteChanged();
        }
    }

    public abstract class ActionBase<ContextType, ParameterType> : ActionBase where ContextType : class
    {
        protected ActionBase(string id) : base(id)
        {
        }

        protected new ContextType Context
        {
            get { return base.Context as ContextType; }
        }

        protected abstract void Execute(ParameterType parameter);
    }
}