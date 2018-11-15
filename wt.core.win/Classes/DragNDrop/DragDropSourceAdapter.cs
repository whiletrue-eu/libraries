using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace WhileTrue.Classes.DragNDrop
{
    internal class DragDropSourceAdapter : IDragDropSourceAdapter
    {
        private readonly IDragDropUiSourceHandlerInstance dragSourceHandler;
        private readonly DependencyObject source;
        private readonly IDragDropSource sourceHandler;

        private DragDropSourceAdapter(IDragDropSource sourceHandler, DependencyObject source)
        {
            this.sourceHandler = sourceHandler;
            this.source = source;
            dragSourceHandler = DragDrop.GetDragDropUISourceHandler(source.GetType()).Create(source, this);

            System.Windows.DragDrop.AddGiveFeedbackHandler(this.source, GiveFeedback);
            System.Windows.DragDrop.AddQueryContinueDragHandler(this.source, QueryContinueDrag);
        }

        public void DoDragDrop()
        {
            var DragData = sourceHandler.DragData;
            var TypeConverter = TypeDescriptor.GetConverter(DragData.GetType());

            IDataObject DataObject = null;
            if (TypeConverter != null)
                if (TypeConverter.CanConvertTo(typeof(IDataObject)))
                    DataObject = (IDataObject) TypeConverter.ConvertTo(DragData, typeof(IDataObject));

            if (DataObject == null)
            {
                if (DragData.GetType().GetCustomAttributes(typeof(SerializableAttribute), true).Length > 0)
                    DataObject = new DataObject(DragData);
                else
                    DataObject = new DataObject(new DragDropObjectWrapper(new DataObject(DragData)));
            }

            var DropEffect = System.Windows.DragDrop.DoDragDrop(source, DataObject, sourceHandler.DragEffects);
            sourceHandler.NotifyDropped(ToDragDropEffect(DropEffect));
        }

        public void Dispose()
        {
            System.Windows.DragDrop.RemoveGiveFeedbackHandler(source, GiveFeedback);
            System.Windows.DragDrop.RemoveQueryContinueDragHandler(source, QueryContinueDrag);

            dragSourceHandler.Dispose();
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

        private static DragDropEffect ToDragDropEffect(DragDropEffects dropEffect)
        {
            if (dropEffect == DragDropEffects.None || dropEffect == DragDropEffects.Scroll) return DragDropEffect.None;
            if ((dropEffect & DragDropEffects.Copy) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Copy) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Copy;
            }

            if ((dropEffect & DragDropEffects.Move) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Move) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Move;
            }

            if ((dropEffect & DragDropEffects.Link) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Link) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Link;
            }

            if ((dropEffect & DragDropEffects.Link) != 0)
            {
                Trace.Assert((dropEffect ^ DragDropEffects.Link) == 0, "dropEffect has multiple bits set");
                return DragDropEffect.Link;
            }

            Trace.Fail("dropEffect is neither Copy, Move nor Link");
            return DragDropEffect.None;
        }

        public static DragDropSourceAdapter Create(IDragDropSource dragDropSource, DependencyObject dependencyObject)
        {
            return new DragDropSourceAdapter(dragDropSource, dependencyObject);
        }
    }
}