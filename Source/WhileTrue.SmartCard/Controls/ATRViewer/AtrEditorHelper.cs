using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WhileTrue.Classes.Commanding;
using WhileTrue.Classes.Utilities;

namespace WhileTrue.Controls
{
    class AtrEditorHelper : DependencyObject
    {
        #region SetValueCommand
        public static readonly DependencyProperty AttachSetValueCommandProperty =
            DependencyProperty.RegisterAttached("AttachSetCommandValue", typeof (string), typeof (AtrEditorHelper), new FrameworkPropertyMetadata(default(string), AttachSetValueCommandChanged ));

        private static void AttachSetValueCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuItem)
            {
                d.SetValue(ButtonBase.CommandProperty,new DelegateCommand(()=>DoSetValue((MenuItem) d, (string) e.NewValue)));
            }
        }

        private static void DoSetValue(MenuItem menuItem, string newValue)
        {
            ContextMenu Menu = menuItem.GetVisualAncestor<ContextMenu>();
            Image PlacementTarget = (Image)(Menu != null ? Menu.PlacementTarget : null);
            TextBox TextBox = (TextBox) (PlacementTarget != null ? PlacementTarget.FindName(newValue) : null);

            if (TextBox != null)
            {
                TextBox.SelectAll();
                TextBox.Focus();
            }
            else
            {
#if DEBUG
                throw new InvalidOperationException("Command target not found!");
#endif
            }
        }

        public static void SetAttachSetValueCommand(UIElement element, string value)
        {
            element.SetValue(AttachSetValueCommandProperty, value);
        }

        public static string GetAttachSetValueCommand(UIElement element)
        {
            return (string) element.GetValue(AttachSetValueCommandProperty);
        }
        #endregion
        
        
        #region AttachContextMenuToImage
        public static readonly DependencyProperty AttachContextMenuToImageProperty =
            DependencyProperty.RegisterAttached("AttachContextMenuToImage", typeof(bool), typeof(AtrEditorHelper), new FrameworkPropertyMetadata(default(bool), ContextMenuToImageChanged));

        private static void ContextMenuToImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Image) d).MouseDown += DoOpenContextMenu;
            d.SetValue(UIElement.FocusableProperty, true);
            ((Image) d).KeyDown += DoOpenContextMenu;
        }

        private static void DoOpenContextMenu(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                OpenContextMenu(sender, e, false);
            }
        }

        private static void DoOpenContextMenu(object sender, MouseButtonEventArgs e)
        {
            OpenContextMenu(sender, e, true);
        }

        private static void OpenContextMenu(object sender, RoutedEventArgs e, bool openAtMouseLocation)
        {
            ContextMenu Menu = ((Image) sender).ContextMenu;
            Menu.DataContext = ((Image) sender).DataContext;
            Menu.PlacementTarget = (UIElement) sender;
            if (openAtMouseLocation == false)
            {
                Menu.Placement = PlacementMode.Right;
            }
            Menu.IsOpen = true;
            e.Handled = true;
        }

        public static void SetAttachContextMenuToImage(UIElement element, bool value)
        {
            element.SetValue(AttachContextMenuToImageProperty, value);
        }

        public static bool GetAttachContextMenuToImage(UIElement element)
        {
            return (bool) element.GetValue(AttachContextMenuToImageProperty);
        }
        #endregion

        #region AttachPopupToImage
        public static readonly DependencyProperty AttachPopupToImageProperty =
            DependencyProperty.RegisterAttached("AttachPopupToImage", typeof(Popup), typeof(AtrEditorHelper), new FrameworkPropertyMetadata(default(Popup), PopupToImageChanged));

        private static void PopupToImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Image)d).MouseDown += DoOpenPopup;
            d.SetValue(UIElement.FocusableProperty, true);
            ((Image)d).KeyDown += DoOpenPopup;
        }

        private static void DoOpenPopup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Enter)
            {
                OpenPopup(sender, e);
            }
        }

        private static void DoOpenPopup(object sender, MouseButtonEventArgs e)
        {
            OpenPopup(sender, e);
        }

        private static void OpenPopup(object sender, RoutedEventArgs e)
        {
            FrameworkElement Sender = (FrameworkElement)sender;
            DependencyPropertyChangedEventHandler VisibleChanged=null;

            Popup Popup = GetAttachPopupToImage(Sender);

            VisibleChanged = (placementTarget, _) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                ((FrameworkElement) placementTarget).IsVisibleChanged -= VisibleChanged;
                Popup.IsOpen = false;
            };
            Sender.IsVisibleChanged += VisibleChanged;

            Popup.DataContext = Sender.DataContext;
            Popup.PlacementTarget = Sender;
            Popup.Placement = PlacementMode.Relative;
            Popup.StaysOpen = false;
            Popup.IsOpen = true;
            FocusManager.SetFocusedElement(Popup,Popup.GetVisualDescendantsDepthFirst<IInputElement>().FirstOrDefault());
            e.Handled = true;
        }

        public static void SetAttachPopupToImage(UIElement element, Popup value)
        {
            element.SetValue(AttachPopupToImageProperty, value);
        }

        public static Popup GetAttachPopupToImage(UIElement element)
        {
            return (Popup)element.GetValue(AttachPopupToImageProperty);
        }
        #endregion
    }
}
