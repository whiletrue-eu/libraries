using System;
using System.Drawing;

namespace WhileTrue.Controls
{
    internal class NotifyIconInteropWrapper : IDisposable
    {
        private readonly INotifyIconCallback callback;

        public NotifyIconInteropWrapper( INotifyIconCallback callback )
        {
            this.callback = callback;
        }

        private static readonly Bitmap nullIcon = new Bitmap(16, 16);
        private Bitmap icon = NotifyIconInteropWrapper.nullIcon;
        private int? notifyIconId;

        public bool IsVisible
        {
            get
            {
                return this.notifyIconId != null;
            }
            set
            {
                if (this.IsVisible != value)
                {
                    if( value )
                    {
                        this.notifyIconId = NotifyIconInteropHelper.CreateNotifyIcon(this.callback, this.icon);
                    }
                    else
                    {
                        NotifyIconInteropHelper.DeleteNotifyIcon(this.notifyIconId.Value);
                        this.notifyIconId = null;
                    }
                }
            }
        }

        public Bitmap Icon
        {
            get { return this.icon; }
            set
            {
                this.icon = value ?? NotifyIconInteropWrapper.nullIcon;
                if( this.IsVisible )
                {
                    NotifyIconInteropHelper.ChangeNotifyIcon(this.notifyIconId.Value, this.icon);
                }
            }
        }

        public void Dispose()
        {
            this.IsVisible = false;
        }

        public void SetFocus()
        {
            if( this.IsVisible )
            {
                NotifyIconInteropHelper.SetFocusToNotifyIcon(this.notifyIconId.Value);
            }
        }

        public void Recreate()
        {
            if (this.IsVisible)
            {
                this.notifyIconId = NotifyIconInteropHelper.CreateNotifyIcon(this.callback, this.icon);
            }
        }
    }
}