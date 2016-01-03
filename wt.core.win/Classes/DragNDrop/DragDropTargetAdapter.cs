using System;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    internal class DragDropTargetAdapter : IDragDropTargetAdapter
    {
        private readonly IDragDropTarget targetHandler;
        //private readonly DependencyObject target;
        private readonly IDragDropUiTargetHandlerInstance dragTargetHandler;
        private bool isDragging;
        private readonly DependencyObject target;

        //private readonly Popup popup = new Popup();
        //private Point dragLocation;

        private DragDropTargetAdapter(IDragDropTarget targetHandler, DependencyObject target, bool makeDroppable)
        {
            this.targetHandler = targetHandler;
            this.target = target;
            this.dragTargetHandler = DragDrop.GetDragDropUITargetHandler(target.GetType()).Create(target, this, makeDroppable);

            System.Windows.DragDrop.AddDragEnterHandler(this.target, this.DragEnter);
            System.Windows.DragDrop.AddDragLeaveHandler(this.target, this.DragLeave);
            System.Windows.DragDrop.AddDragOverHandler(this.target, this.DragOver);
            System.Windows.DragDrop.AddDropHandler(this.target, this.Drop);


            /*
                this.popup.AllowsTransparency = true;
                ContentControl Content = new ContentControl();
                Content.SetBinding(ContentControl.ContentProperty, "");
                Border Border = new Border();
                Border.BorderThickness = new Thickness(1);
                Border.CornerRadius = new CornerRadius(3);
                Border.BorderBrush = new SolidColorBrush(SystemColors.HighlightColor) { Opacity = 0.7 };
                Border.Background = new SolidColorBrush(SystemColors.HighlightColor) {Opacity = 0.3};
                Border.Child = Content;
                Border.MinHeight = 10;
                Border.MinWidth = 10;
                this.popup.Child = Border;
                this.popup.Placement = PlacementMode.Custom;
                this.popup.CustomPopupPlacementCallback = this.HandlePopupPlacement;
                this.popup.HorizontalOffset = 10;
                this.popup.VerticalOffset = 10;*/
        }

        /*
            private CustomPopupPlacement[] HandlePopupPlacement(Size popupsize, Size targetsize, Point offset)
            {
                Point Position = this.dragLocation;
                Position.Offset(offset.X, offset.Y);
                return new[] { new CustomPopupPlacement(Position, PopupPrimaryAxis.None), };
            }*/

        private void DragEnter(object sender, DragEventArgs e)
        {
            DragDropEffects Effect = this.GetDropEffect(e.AllowedEffects, e.KeyStates, e.Data);

            this.HandleDragStart(DragDropTargetAdapter.ToDropEffect(Effect));
            this.HandleDragUpdate(DragDropTargetAdapter.ToDropEffect(Effect), new DragPosition(e));

            e.Effects = Effect;
            e.Handled = true;

        }

        private void DragOver(object sender, DragEventArgs e)
        {
            DragDropEffects Effect = this.GetDropEffect(e.AllowedEffects, e.KeyStates, e.Data);

            e.Effects = Effect;
            e.Handled = true;

            this.HandleDragUpdate(DragDropTargetAdapter.ToDropEffect(Effect), new DragPosition(e));


            /*Window Window = Window.GetWindow(this.target);
            this.dragLocation = Window.PointToScreen(e.GetPosition(Window));
                this.popup.PrivateMembers().Call("Reposition");*/
        }

        private void DragLeave(object sender, DragEventArgs e)
        {
            this.HandleDragEnd();
        }

        private void HandleDragStart(DragDropEffect effect)
        {
            if (this.isDragging == false)
            {
                this.OpenPopup();
                this.dragTargetHandler.NotifyDragStarted(effect);

                this.isDragging = true;
            }
        }

        private void HandleDragUpdate(DragDropEffect effect, DragPosition position)
        {
            this.dragTargetHandler.NotifyDragChanged(effect, position);
        }

        private void HandleDragEnd()
        {
            if (this.isDragging)
            {
                this.ClosePopup();
                this.dragTargetHandler.NotifyDragEnded();

                this.isDragging = false;
            }
        }

        private void OpenPopup()
        {
            /*    this.popup.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)delegate
                    {
                        this.popup.DataContext = GetData(dataObject);
                        this.popup.IsOpen = true;
                    });*/
        }

        private static object GetData(IDataObject dataObject)
        {
            foreach( string DataFormat in dataObject.GetFormats() )
            {
                foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) //Reverse because it is mor likely to find the type in the user code that is loaded after the framework
                {
                    Type Type = Assembly.GetType(DataFormat, false);
                    if (Type != null)
                    {
                        return dataObject.GetData(Type);
                    }
                }
            }
            return null;
        }

        private void ClosePopup()
        {
            /*this.popup.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) delegate
                    {
                        this.popup.IsOpen = false;
                        this.popup.DataContext = null;
                    });*/
        }

        private void Drop(object sender, DragEventArgs e)
        {
            e.Effects = this.GetDropEffect(e.AllowedEffects, e.KeyStates, e.Data);

            DragDropEffects DropEffects = this.GetDropEffect(e.Effects, e.KeyStates, e.Data);
            this.targetHandler.DoDrop(e.Data, DragDropTargetAdapter.ToDropEffect(DropEffects), this.dragTargetHandler.GetAdditionalDropInfo(new DragPosition(e)));

            e.Handled = true;

            this.HandleDragEnd();
        }

        private static DragDropEffect ToDropEffect(DragDropEffects dropEffects)
        {
            DragDropEffect DropEffect;
            switch (dropEffects)
            {
                case DragDropEffects.Copy:
                    DropEffect = DragDropEffect.Copy;
                    break;
                case DragDropEffects.Move:
                    DropEffect = DragDropEffect.Move;
                    break;
                case DragDropEffects.Link:
                    DropEffect = DragDropEffect.Link;
                    break;
                case DragDropEffects.Scroll:
                case DragDropEffects.None:
                    DropEffect = DragDropEffect.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return DropEffect;
        }

        private DragDropEffects GetDropEffect(DragDropEffects allowedEffects, DragDropKeyStates keyStates, IDataObject data)
        {
            DragDropEffects AcceptedEffects = this.targetHandler.GetDropEffects(data);
            DragDropKeyStates KeyboardStates = keyStates & (DragDropKeyStates.AltKey | DragDropKeyStates.ShiftKey | DragDropKeyStates.ControlKey);
            DragDropKeyStates MouseStates = keyStates & (DragDropKeyStates.LeftMouseButton|DragDropKeyStates.RightMouseButton|DragDropKeyStates.MiddleMouseButton);

            if (AcceptedEffects == DragDropEffects.Scroll)
            {
                return DragDropEffects.Scroll;
            }

            if (KeyboardStates == DragDropKeyStates.None)
            {
                DragDropEffect DefaultEffect = this.targetHandler.GetDefaultEffect(data);
                if( (allowedEffects & (DragDropEffects)DefaultEffect) != 0)
                {
                    return (DragDropEffects)DefaultEffect;
                }
            }
            else if ((KeyboardStates & DragDropKeyStates.ControlKey) != 0)
            {
                if ((allowedEffects & DragDropEffects.Copy) != 0 && (AcceptedEffects & DragDropEffects.Copy) != 0)
                {
                    return DragDropEffects.Copy;
                }
            }
            else if ((KeyboardStates & DragDropKeyStates.ShiftKey) != 0)
            {
                if ((allowedEffects & DragDropEffects.Move) != 0 && (AcceptedEffects & DragDropEffects.Move) != 0)
                {
                    return DragDropEffects.Move;
                }
            }
            else if ((KeyboardStates & DragDropKeyStates.AltKey) != 0)
            {
                if ((allowedEffects & DragDropEffects.Link) != 0 && (AcceptedEffects & DragDropEffects.Link) != 0)
                {
                    return DragDropEffects.Link;
                }
            }

            return DragDropEffects.None;
        }

        public static DragDropTargetAdapter Create(IDragDropTarget dragDropTarget, DependencyObject dependencyObject, bool makeDroppable)
        {
            return new DragDropTargetAdapter(dragDropTarget,dependencyObject, makeDroppable);
        }

        public void Dispose()
        {
            System.Windows.DragDrop.RemoveDragEnterHandler(this.target, this.DragEnter);
            System.Windows.DragDrop.RemoveDragLeaveHandler(this.target, this.DragLeave);
            System.Windows.DragDrop.RemoveDragOverHandler(this.target, this.DragOver);
            System.Windows.DragDrop.RemoveDropHandler(this.target, this.Drop);

            this.dragTargetHandler.Dispose();
        }
    }
}