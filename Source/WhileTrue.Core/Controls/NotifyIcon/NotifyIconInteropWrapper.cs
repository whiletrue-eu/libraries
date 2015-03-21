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
        private Bitmap icon = nullIcon;
        private int? notifyIconID;

        public bool IsVisible
        {
            get
            {
                return this.notifyIconID != null;
            }
            set
            {
                if (this.IsVisible != value)
                {
                    if( value )
                    {
                        this.notifyIconID = NotifyIconInteropHelper.CreateNotifyIcon(this.callback, this.icon);
                    }
                    else
                    {
                        NotifyIconInteropHelper.DeleteNotifyIcon(this.notifyIconID.Value);
                        this.notifyIconID = null;
                    }
                }
            }
        }

        public Bitmap Icon
        {
            get { return this.icon; }
            set
            {
                this.icon = value ?? nullIcon;
                if( this.IsVisible )
                {
                    NotifyIconInteropHelper.ChangeNotifyIcon(this.notifyIconID.Value, this.icon);
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
                NotifyIconInteropHelper.SetFocusToNotifyIcon(this.notifyIconID.Value);
            }
        }

        public void Recreate()
        {
            if (this.IsVisible)
            {
                this.notifyIconID = NotifyIconInteropHelper.CreateNotifyIcon(this.callback, this.icon);
            }
        }
    }
}