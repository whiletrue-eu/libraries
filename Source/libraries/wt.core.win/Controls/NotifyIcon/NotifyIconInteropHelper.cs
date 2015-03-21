using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using WhileTrue.Classes.Win32;

namespace WhileTrue.Controls
{
    internal static class NotifyIconInteropHelper
    {
        private static int nextNotifyIconId;

// ReSharper disable InconsistentNaming
        [DllImport("shell32.dll")]
        private static extern bool Shell_NotifyIcon(NotifyMessage message, [In] ref NotifyIconData data);
// ReSharper restore InconsistentNaming

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private extern static bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll", SetLastError=true, CharSet=CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string lpString);


#pragma warning disable 219
#pragma warning disable 169
        // ReSharper disable UnaccessedField.Local
        // ReSharper disable InconsistentNaming
        private struct NotifyIconData
        {
            /// <summary>
            /// Size of this structure, in bytes. 
            /// </summary>
            public int cbSize;

            /// <summary>
            /// Handle to the window that receives notification messages associated with an icon in the 
            /// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on 
            /// when Shell_NotifyIcon is invoked. 
            /// </summary>
            public IntPtr hwnd;

            /// <summary>
            /// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify 
            /// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons 
            /// associated with a single hWnd by assigning each a different uID. 
            /// </summary>
            public int uID;

            /// <summary>
            /// Flags that indicate which of the other members contain valid data. This member can be 
            /// a combination of the NIF_XXX constants.
            /// </summary>
            public int uFlags;

            /// <summary>
            /// Application-defined message identifier. The system uses this identifier to send 
            /// notifications to the window identified in hWnd. 
            /// </summary>
            public int uCallbackMessage;

            /// <summary>
            /// Handle to the icon to be added, modified, or deleted. 
            /// </summary>
            public IntPtr hIcon;

            /// <summary>
            /// String with the text for a standard ToolTip. It can have a maximum of 64 characters including 
            /// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of 
            /// 128 characters, including the terminating NULL.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szTip;

            /// <summary>
            /// State of the icon. 
            /// </summary>
            public int dwState;

            /// <summary>
            /// A value that specifies which bits of the state member are retrieved or modified. 
            /// For example, setting this member to NIS_HIDDEN causes only the item's hidden state to be retrieved. 
            /// </summary>
            public int dwStateMask;

            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters. 
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;

            /// <summary>
            /// NOTE This field is also used for the Timeout value. Specifies whether the Shell notify 
            /// icon interface should use Windows 95 or Windows 2000 
            /// behavior. For more information on the differences in these two behaviors, see 
            /// Shell_NotifyIcon. This member is only employed when using Shell_NotifyIcon to send an 
            /// NIM_VERSION message. 
            /// </summary>
            public int uVersion;

            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface 
            /// above the text. It can have a maximum of 63 characters. 
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;

            /// <summary>
            /// Adds an icon to a balloon ToolTip. It is placed to the left of the title. If the 
            /// szTitleInfo member is zero-length, the icon is not shown. See 
            /// RMUtils.WinAPI.Structs.BalloonIconStyle for more
            /// information.
            /// </summary>
            public int dwInfoFlags;
        }
        // ReSharper enable UnaccessedField.Local
        // ReSharper restore InconsistentNaming
#pragma warning restore 219
#pragma warning restore 169


        private enum NotifyMessage
        {
            /// <summary>
            /// Adds an icon to the status area. The hWnd and uID members of the NotifyIconData structure
            /// pointed to by lpdata will be used to identify the icon in later calls to Shell_NotifyIcon.
            /// </summary>
            Add = 0x00000000,
            /// <summary>
            /// Modifies an icon in the status area. Use the hWnd and uID members of the NotifyIconData structure pointed to by lpdata to identify the icon to be modified.
            /// </summary>
            Modify = 0x00000001,
            /// <summary>
            /// Deletes an icon from the status area. Use the hWnd and uID members of the NotifyIconData structure pointed to by lpdata to identify the icon to be deleted.
            /// </summary>
            Delete = 0x00000002,
            /// <summary>
            /// Shell32.dll version 5.0 and later only. Returns focus to the taskbar notification area. Taskbar icons should use this message when they have completed their user interface operation. For example, if the taskbar icon displays a shortcut menu, but the user presses ESC to cancel it, use NIM_SETFOCUS to return focus to the taskbar notification area.
            /// </summary>
            SetFocus = 0x00000003,
            /// <summary>
            /// Shell32.dll version 5.0 and later only. Instructs the taskbar to behave according to the version number specified in the uVersion member of the structure pointed to by lpdata. This message allows you to specify whether you want the version 5.0 behavior found on Microsoft Windows 2000 systems, or the behavior found on earlier Shell versions. The default value for uVersion is zero, indicating that the original Windows 95 notify icon behavior should be used. For details, see the Remarks section.
            /// </summary>
            SetVersion = 0x00000004,
        }

        private class NotifyIconWindow : HwndSource, IDisposable//, IMouseHookOwner
        {
            private readonly Dictionary<int, Registration> registrations = new Dictionary<int, Registration>();

            private class Registration
            {
                public Registration(INotifyIconCallback callback)
                {
                    this.Callback = callback;
                    this.LastMousePosition = null;
                    this.IsMouseOver = false;
                }

                public INotifyIconCallback Callback { get; }
                public Point? LastMousePosition { get; set; }
                public bool IsMouseOver { get; set; }
            }

            private NotifyIconWindow()
                : base(0,0,0,0,0,"",IntPtr.Zero)
            {
                this.AddHook( this.ProcessMessage );
//                this.mouseHook = new MouseHook(this);
            }

            private readonly uint notificationAreaCreatedMessage = NotifyIconInteropHelper.RegisterWindowMessage("TaskbarCreated");

            private IntPtr ProcessMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                Trace.Assert( hwnd == this.Handle , "Target window unknown");

                if (msg == (int) Win32.Wm.App)
                {
                    int Id = wParam.ToInt32();
                    int Message = lParam.ToInt32();

                    if (this.registrations.ContainsKey(Id))
                    {
                        Registration Registration = this.registrations[Id];
                        INotifyIconCallback Callback = Registration.Callback;
                        try
                        {
                            switch ((Win32.Wm)Message)
                            {
                                case Win32.Wm.Mousemove:
                                    Callback.MouseMoved();
//                                    if (this.mouseHook != null)
//                                    {
//                                        Registration.LastMousePosition = null;
//                                        if (Registration.IsMouseOver == false)
//                                        {
//                                            Registration.IsMouseOver = true;
//                                            Callback.MouseEnter();
//                                        }
//                                    }
                                    break;
                                case Win32.Wm.Rbuttonup:
                                    Callback.MouseButtonUp(MouseButton.Right);
                                    break;
                                case Win32.Wm.Rbuttondown:
                                    Callback.MouseButtonDown(MouseButton.Right);
                                    break;
                                case Win32.Wm.Rbuttondblclk:
                                    Callback.MouseButtonDoubleClick(MouseButton.Right);
                                    break;
                                case Win32.Wm.Lbuttonup:
                                    Callback.MouseButtonUp(MouseButton.Left);
                                    break;
                                case Win32.Wm.Lbuttondown:
                                    Callback.MouseButtonDown(MouseButton.Left);
                                    break;
                                case Win32.Wm.Lbuttondblclk:
                                    Callback.MouseButtonDoubleClick(MouseButton.Left);
                                    break;
                                case Win32.Wm.Mbuttonup:
                                    Callback.MouseButtonUp(MouseButton.Middle);
                                    break;
                                case Win32.Wm.Mbuttondown:
                                    Callback.MouseButtonDown(MouseButton.Middle);
                                    break;
                                case Win32.Wm.Mbuttondblclk:
                                    Callback.MouseButtonDoubleClick(MouseButton.Middle);
                                    break;
                                case Win32.Wm.Contextmenu:
                                    Callback.ContextMenu();
                                    break;
                                default:
                                    //unknown message ignore
                                    break;
                            } handled = true;
                        }
                            // ReSharper disable EmptyGeneralCatchClause
                        catch
                        {
                        }
                        // ReSharper restore EmptyGeneralCatchClause
                        return IntPtr.Zero;
                    }
                    else
                    {
                        Trace.Fail("Notify Icon Message was received for unknown uID!");
                        return IntPtr.Zero;
                    }
                }
                else if (msg == this.notificationAreaCreatedMessage)
                {
                    foreach (Registration Registration in this.registrations.Values)
                    {
                        Registration.Callback.RecreationRequired();
                    }
                    return IntPtr.Zero;
                }
                else
                {
                    return IntPtr.Zero;
                }
            }

            public new void Dispose()
            {
                this.RemoveHook(this.ProcessMessage);

//                if (this.mouseHook != null)
//                {
//                    this.mouseHook.Dispose();
//                }

                base.Dispose();
                GC.SuppressFinalize(this);
            }

            public void AddMessageHook(int id, INotifyIconCallback callback)
            {
                this.registrations.Add(id, new Registration(callback));
            }

            public void RemoveMessageHook(int id)
            {
                this.registrations.Remove(id);
                if (this.registrations.Count == 0)
                {
                    NotifyIconWindow.instance.Dispose();
                    NotifyIconWindow.instance = null;
                }
            }

            private static NotifyIconWindow instance;
            //private readonly MouseHook mouseHook; //TODO: Make mouse hook work on XP, make mouse hook optional with a property for a notifyicon

            public static NotifyIconWindow Instance
            {
                get 
                {
                    if( NotifyIconWindow.instance == null)
                    {
                        NotifyIconWindow.instance = new NotifyIconWindow();
                    }
                    return NotifyIconWindow.instance; 
                }
            }

            public void NotifyMouseMove()
            {
//                if (this.mouseHook != null)
//                {
//                    foreach (Registration Registration in this.registrations.Values)
//                    {
//                        Point? LastPosition = Registration.LastMousePosition;
//                        Point CurrentPosition = Cursor.Position;
//                        if (LastPosition.HasValue && LastPosition.Value != CurrentPosition)
//                        {
//                            // Last Position is set (means, there was a mouse move notify message received
//                            // And last position is not the current one.
//                            // This means, that a mouse hook was received, then NO notify message and another hook 
//                            // message with another coordinate.
//                            // this means, the mouse was moved outside of the notify icon bounds.
//                            if (Registration.IsMouseOver)
//                            {
//                                Registration.IsMouseOver = false;
//                                Registration.Callback.MouseLeave();
//                            }
//                        }
//                        else
//                        {
//                            Registration.LastMousePosition = CurrentPosition;
//                        }
//                    }
//                }
            }

        }

        private static NotifyIconWindow HookWindow => NotifyIconWindow.Instance;

        private static readonly Dictionary<int,IntPtr> iconHandles = new Dictionary<int, IntPtr>();

        public static int CreateNotifyIcon(INotifyIconCallback messageCallback, Bitmap icon)
        {
            int Id = NotifyIconInteropHelper.nextNotifyIconId++;
            NotifyIconInteropHelper.HookWindow.AddMessageHook(Id, messageCallback);
            NotifyIconInteropHelper.iconHandles.Add(Id, icon.GetHicon());

            NotifyIconData IconData = new NotifyIconData{cbSize = Marshal.SizeOf(typeof(NotifyIconData))};
            IconData.hwnd = NotifyIconInteropHelper.HookWindow.Handle;
            IconData.uID = Id;
            IconData.uFlags = 0x00000003; // NIF_Message (1), NIF_ICON (2)
            IconData.uCallbackMessage = (int) Win32.Wm.App;
            IconData.hIcon = NotifyIconInteropHelper.iconHandles[Id];
            IconData.uVersion = 3; //Window 2000 and later (4 would be Vista and later). Changes message handling!

            NotifyIconInteropHelper.Shell_NotifyIcon(NotifyMessage.Add, ref IconData);
            NotifyIconInteropHelper.Shell_NotifyIcon(NotifyMessage.SetVersion, ref IconData);

            return Id;
        }

        public static void ChangeNotifyIcon(int iconId, Bitmap icon)
        {
            NotifyIconInteropHelper.DestroyIcon(NotifyIconInteropHelper.iconHandles[iconId]);
            NotifyIconInteropHelper.iconHandles[iconId] = icon.GetHicon();

            NotifyIconData IconData = new NotifyIconData { cbSize = Marshal.SizeOf(typeof(NotifyIconData)) };
            IconData.hwnd = NotifyIconInteropHelper.HookWindow.Handle;
            IconData.uID = iconId;
            IconData.uFlags = 0x00000002; // NIF_ICON (2)
            IconData.hIcon = NotifyIconInteropHelper.iconHandles[iconId];

            NotifyIconInteropHelper.Shell_NotifyIcon(NotifyMessage.Modify, ref IconData);
        }

        public static void DeleteNotifyIcon(int iconId)
        {
            NotifyIconInteropHelper.DestroyIcon(NotifyIconInteropHelper.iconHandles[iconId]);
            NotifyIconInteropHelper.iconHandles.Remove(iconId);

            NotifyIconData IconData = new NotifyIconData { cbSize = Marshal.SizeOf(typeof(NotifyIconData)) };
            IconData.hwnd = NotifyIconInteropHelper.HookWindow.Handle;
            IconData.uID = iconId;

            NotifyIconInteropHelper.Shell_NotifyIcon(NotifyMessage.Delete, ref IconData);
            NotifyIconInteropHelper.HookWindow.RemoveMessageHook(iconId);
        }

        public static void SetFocusToNotifyIcon(int iconId)
        {
            NotifyIconData IconData = new NotifyIconData { cbSize = Marshal.SizeOf(typeof(NotifyIconData)) };
            IconData.hwnd = NotifyIconInteropHelper.HookWindow.Handle;
            IconData.uID = iconId;

            NotifyIconInteropHelper.Shell_NotifyIcon(NotifyMessage.SetFocus, ref IconData);
        }
    }
}