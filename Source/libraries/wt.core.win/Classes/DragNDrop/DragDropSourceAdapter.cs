using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    internal class DragDropSourceAdapter : IDragDropSourceAdapter
    {
        private readonly IDragDropSource sourceHandler;
        private readonly DependencyObject source;
        private readonly IDragDropUiSourceHandlerInstance dragSourceHandler;

        private DragDropSourceAdapter(IDragDropSource sourceHandler, DependencyObject source)
        {
            this.sourceHandler = sourceHandler;
            this.source = source;
            this.dragSourceHandler = DragDrop.GetDragDropUISourceHandler(source.GetType()).Create(source, this);

            System.Windows.DragDrop.AddGiveFeedbackHandler(this.source, DragDropSourceAdapter.GiveFeedback);
            System.Windows.DragDrop.AddQueryContinueDragHandler(this.source, DragDropSourceAdapter.QueryContinueDrag);
        }

        private static void GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
        }

        private static void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }
            
        public void DoDragDrop()
        {
            object DragData = this.sourceHandler.DragData;
            TypeConverter TypeConverter = TypeDescriptor.GetConverter(DragData.GetType());

            IDataObject DataObject = null;
            if (TypeConverter != null)
            {
                if (TypeConverter.CanConvertTo(typeof(IDataObject)))
                {
                    DataObject = (IDataObject) TypeConverter.ConvertTo(DragData, typeof (IDataObject));
                }
            }

            if (DataObject == null)
            {
                if (DragData.GetType().GetCustomAttributes(typeof (SerializableAttribute), true).Length > 0)
                {
                    DataObject = new DataObject(DragData);
                }
                else
                {
                    DataObject = new DataObject(new DragDropObjectWrapper( new DataObject(DragData)));
                    /*throw new InvalidOperationException(
                        string.Format(
                            "The object type '{0}' that shall be dragged & dropped is not serialiasable. You have to mark it with the [Serializable] attribute. Non-serializable objects cannot be moved outside the window border. If you don't want or can't provide real serializability,or don't want to have the item available outside the application anyway, add the ISerializable interface with an empty implementation",
                            DragData.GetType()));*/
                }
            }

            DragDropEffects DropEffect = System.Windows.DragDrop.DoDragDrop(this.source, DataObject, this.sourceHandler.DragEffects);
            this.sourceHandler.NotifyDropped(DragDropSourceAdapter.ToDragDropEffect(DropEffect));
        }

        private static DragDropEffect ToDragDropEffect(DragDropEffects dropEffect)
        {
            if (dropEffect == DragDropEffects.None || dropEffect == DragDropEffects.Scroll)
            {
                return DragDropEffect.None;
            }
            if ((dropEffect & DragDropEffects.Copy) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Copy) == 0,"dropEffect has multiple bits set");
                return DragDropEffect.Copy;
            }
            else if ((dropEffect & DragDropEffects.Move) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Move) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Move;
            }
            else if ((dropEffect & DragDropEffects.Link) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Link) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Link;
            }
            else if ((dropEffect & DragDropEffects.Link) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Link) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Link;
            }
            else
            {
                Trace.Fail("dropEffect is neither Copy, Move nor Link");
                return DragDropEffect.None;
            }
        }

        public static DragDropSourceAdapter Create(IDragDropSource dragDropSource, DependencyObject dependencyObject)
        {
            return new DragDropSourceAdapter(dragDropSource,dependencyObject);
        }

        public void Dispose()
        {
            System.Windows.DragDrop.RemoveGiveFeedbackHandler(this.source, DragDropSourceAdapter.GiveFeedback);
            System.Windows.DragDrop.RemoveQueryContinueDragHandler(this.source, DragDropSourceAdapter.QueryContinueDrag);

            this.dragSourceHandler.Dispose();
        }
    }
}