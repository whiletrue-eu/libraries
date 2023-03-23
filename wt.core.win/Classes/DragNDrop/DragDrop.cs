using System;
using System.Collections.Generic;
using System.Windows;
using WhileTrue.Classes.DragNDrop.DragDropUIHandler;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop
{
    /// <summary>
    ///     Adds comfortable drag and drop support to MVVM applications.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Drag and drop support is given by marking UI Elements in xaml as either drag sources or drag targets.
    ///         At that point, a drag and drop source and target handler are set on the UI control that can be implmented
    ///         in the model. The handler interfaces is complete UI less, allowing the clean separation of UI and the logic
    ///         behind.
    ///     </para>
    ///     <para>
    ///         To be able to handle different kinds of UI elements with different logic (e.g. a single item, a ItemsControl
    ///         with separate
    ///         items that shall allow drop into a specific index, TreeViews, etc), the UI handling logic is again separated
    ///         from the drag and drop logic. New UI handlers can be registered in this class with
    ///         <see cref="RegisterDragDropUISourceHandler" />
    ///         and <see cref="RegisterDragDropUITargetHandler" /> respectively.
    ///     </para>
    /// </remarks>
    public class DragDrop
    {
        private static readonly DependencyPropertyEventManager sourceChangedEventManager =
            new DependencyPropertyEventManager();

        private static readonly DependencyPropertyEventManager targetChangedEventManager =
            new DependencyPropertyEventManager();

        private static readonly DependencyPropertyEventManager activateHelperChangedEventManager =
            new DependencyPropertyEventManager();

        private static readonly Dictionary<DependencyObject, DragDropSourceAdapter> dragDropSources =
            new Dictionary<DependencyObject, DragDropSourceAdapter>();

        private static readonly Dictionary<DependencyObject, DragDropTargetAdapter> dragDropTargets =
            new Dictionary<DependencyObject, DragDropTargetAdapter>();

        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly Dictionary<DependencyObject, DragDropHelperAdapter> dragDropHelper =
            new Dictionary<DependencyObject, DragDropHelperAdapter>();

        private static readonly Dictionary<Type, IDragDropUiSourceHandler> dragSourceHandlers =
            new Dictionary<Type, IDragDropUiSourceHandler>();

        private static readonly Dictionary<Type, IDragDropUiTargetHandler> dragTargetHandlers =
            new Dictionary<Type, IDragDropUiTargetHandler>();

        private static readonly Dictionary<Type, IDragDropUiHelper> dragHelper =
            new Dictionary<Type, IDragDropUiHelper>();


        static DragDrop()
        {
            sourceChangedEventManager.Changed += SourceChanged;
            targetChangedEventManager.Changed += TargetChanged;
            activateHelperChangedEventManager.Changed += ActivateHelperChanged;

            RegisterDragDropUISourceHandler(new FrameworkElementDragDropUiHandler());
            RegisterDragDropUISourceHandler(new FrameworkContentElementDragDropUiHandler());

            RegisterDragDropUITargetHandler(new StackPanelDragDropUiHandler());
            RegisterDragDropUITargetHandler(new VirtualizingStackPanelDragDropUiHandler());
            RegisterDragDropUITargetHandler(new TabPanelDragDropUiHandler());
            RegisterDragDropUITargetHandler(new TabControlDragDropUiHandler());
            RegisterDragDropUITargetHandler(new ItemsControlDragDropUiHandler());
            RegisterDragDropUITargetHandler(new FrameworkElementDragDropUiHandler());
            RegisterDragDropUITargetHandler(new FrameworkContentElementDragDropUiHandler());

            RegisterDragDropUiHelper(new TabControlDragDropHelper());
            RegisterDragDropUiHelper(new FrameworkElementDragDropUiHelper());
        }

        private static void SourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var Element = sender as DependencyObject;
            var NewSource = e.NewValue as IDragDropSource;
            var CurrentSource = e.OldValue as IDragDropSource;

            if (Element is UIElement || Element is ContentElement)
            {
                if (CurrentSource != null)
                {
                    DragDropSourceAdapter Adapter;
                    dragDropSources.TryGetValue(Element, out Adapter).DbC_Assure(isSuccess => isSuccess);
                    dragDropSources.Remove(Element);
                    Adapter?.Dispose();
                }

                if (NewSource != null)
                {
                    var Adapter = DragDropSourceAdapter.Create(NewSource, Element);
                    dragDropSources.Add(Element, Adapter);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "Drag and Drop source can only be registered on UIElements and ContentElements");
            }
        }

        private static void TargetChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var Element = sender as DependencyObject;
            var NewTarget = e.NewValue as IDragDropTarget;
            var CurrentTarget = e.OldValue as IDragDropTarget;

            if (Element is UIElement || Element is ContentElement)
            {
                if (CurrentTarget != null)
                {
                    DragDropTargetAdapter Adapter;
                    dragDropTargets.TryGetValue(Element, out Adapter).DbC_Assure(isSuccess => isSuccess);
                    dragDropTargets.Remove(Element);
                    Adapter?.Dispose();
                }

                if (NewTarget != null)
                {
                    var Adapter = DragDropTargetAdapter.Create(NewTarget, Element, true);
                    dragDropTargets.Add(Element, Adapter);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "Drag and Drop target can only be registered on UIElements and ContentElements");
            }
        }

        private static void ActivateHelperChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var Element = sender as DependencyObject;

            if (Element is UIElement)
            {
                ((UIElement) Element).AllowDrop = true;
                var Adapter = DragDropHelperAdapter.Create((UIElement) Element);
                dragDropHelper.Add(Element, Adapter);
            }
            else
            {
                throw new InvalidOperationException(
                    "Drag and Drop target can only be registered on UIElements and ContentElements");
            }
        }

        private static THandlerType GetDragDropUIHandler<THandlerType>(Dictionary<Type, THandlerType> handler,
            Type uiElementType) where THandlerType : class
        {
            var UiElementType = uiElementType;
            while (UiElementType != null)
            {
                if (handler.ContainsKey(UiElementType)) return handler[UiElementType];

                UiElementType = UiElementType.BaseType;
            }

            return null;
        }

        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        ///     Register a drag and drop source handler for a given UI element
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source",
            typeof(IDragDropSource), typeof(DragDrop),
            new FrameworkPropertyMetadata(null, sourceChangedEventManager.ChangedHandler));

        /// <summary>
        ///     Register a drag and drop target handler for a given UI element
        /// </summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached("Target",
            typeof(IDragDropTarget), typeof(DragDrop),
            new FrameworkPropertyMetadata(null, targetChangedEventManager.ChangedHandler));

        /// <summary>
        ///     Add drag and drop support for the given UI element. Within activated UIElements, DnD "Helper" handler are used
        /// </summary>
        public static readonly DependencyProperty ActivateHelperProperty =
            DependencyProperty.RegisterAttached("ActivateHelper", typeof(string), typeof(DragDrop),
                new FrameworkPropertyMetadata(null, activateHelperChangedEventManager.ChangedHandler));
        // ReSharper restore MemberCanBePrivate.Global

        // ReSharper disable UnusedMember.Global
        /// <summary>
        ///     Registers a drag and drop source handler for a given UI element
        /// </summary>
        public static void SetSource(DependencyObject element, IDragDropSource source)
        {
            element.SetValue(SourceProperty, source);
        }

        /// <summary>
        ///     Gets the drag and drop source handler registered for the given UI element
        /// </summary>
        public static IDragDropSource GetSource(DependencyObject element)
        {
            return (IDragDropSource) element.GetValue(SourceProperty);
        }

        /// <summary>
        ///     Registers a drag and drop target handler for a given UI element
        /// </summary>
        public static void SetTarget(DependencyObject element, IDragDropTarget target)
        {
            element.SetValue(TargetProperty, target);
        }

        /// <summary>
        ///     Gets the drag and drop target handler registered for the given UI element
        /// </summary>
        public static IDragDropTarget GetTarget(DependencyObject element)
        {
            return (IDragDropTarget) element.GetValue(TargetProperty);
        }

        /// <summary>
        ///     Registers a drag and drop helper for a given UI element
        /// </summary>
        public static void SetActivateHelper(DependencyObject element, string value)
        {
            element.SetValue(ActivateHelperProperty, value);
        }

        /// <summary>
        ///     Gets the drag and drop helper registered for the given UI element
        /// </summary>
        public static string GetActivateHelper(DependencyObject element)
        {
            return (string) element.GetValue(ActivateHelperProperty);
        } // ReSharper restore UnusedMember.Global

        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        ///     Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        ///     If a handler was registered before for the exact same type (e.g. one of the default handlers),
        ///     it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUISourceHandler(IDragDropUiSourceHandler handler)
        {
            if (dragSourceHandlers.ContainsKey(handler.Type)) dragSourceHandlers.Remove(handler.Type);
            dragSourceHandlers.Add(handler.Type, handler);
        }

        /// <summary>
        ///     Gets the handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The handlers are searched to find the most specific handler available for the type. This means,
        ///         first the handler for the exact type is searched for. If nothing is found, the search is repeated
        ///         with the base type and so on.
        ///     </para>
        ///     <para>
        ///         if nothing is foud, <c>null</c> is returned
        ///     </para>
        /// </remarks>
        public static IDragDropUiSourceHandler GetDragDropUISourceHandler(Type uiElementType)
        {
            return GetDragDropUIHandler(dragSourceHandlers, uiElementType);
        }

        /// <summary>
        ///     Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        ///     If a handler was registered before for the exact same type (e.g. one of the default handlers),
        ///     it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUITargetHandler(IDragDropUiTargetHandler handler)
        {
            if (dragTargetHandlers.ContainsKey(handler.Type)) dragTargetHandlers.Remove(handler.Type);
            dragTargetHandlers.Add(handler.Type, handler);
        }

        /// <summary>
        ///     Gets the handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The handlers are searched to find the most specific handler available for the type. This means,
        ///         first the handler for the exact type is searched for. If nothing is found, the search is repeated
        ///         with the base type and so on.
        ///     </para>
        ///     <para>
        ///         if nothing is foud, <c>null</c> is returned
        ///     </para>
        /// </remarks>
        public static IDragDropUiTargetHandler GetDragDropUITargetHandler(Type uiElementType)
        {
            return GetDragDropUIHandler(dragTargetHandlers, uiElementType);
        }

        /// <summary>
        ///     Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        ///     If a handler was registered before for the exact same type (e.g. one of the default handlers),
        ///     it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUiHelper(IDragDropUiHelper helper)
        {
            if (dragHelper.ContainsKey(helper.Type)) dragHelper.Remove(helper.Type);
            dragHelper.Add(helper.Type, helper);
        }

        /// <summary>
        ///     Gets all handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The handlers are searched to find all handler available for the type. This means,
        ///         first the handler for the exact type is searched for, after that the search is continued
        ///         with the base type and so on.
        ///     </para>
        /// </remarks>
        public static IEnumerable<IDragDropUiHelper> GetDragDropUiHelper(Type uiElementType)
        {
            var UiElementType = uiElementType;
            while (UiElementType != null)
            {
                if (dragHelper.ContainsKey(UiElementType)) yield return dragHelper[UiElementType];

                UiElementType = UiElementType.BaseType;
            }
        }
        // ReSharper restore MemberCanBePrivate.Global
    }
}