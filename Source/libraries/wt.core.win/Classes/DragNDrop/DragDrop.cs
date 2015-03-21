using System;
using System.Collections.Generic;
using System.Windows;
using WhileTrue.Classes.DragNDrop.DragDropUIHandler;
using WhileTrue.Classes.Utilities;
using WhileTrue.Classes.Wpf;

namespace WhileTrue.Classes.DragNDrop
{
    ///<summary>
    /// Adds comfortable drag and drop support to MVVM applications.
    ///</summary>
    ///<remarks>
    /// <para>
    /// Drag and drop support is given by marking UI ELements in xaml as either drag sources or drag targets.
    /// At that point, a drag and drop source and target handler are set on the UI control that can be implmented
    /// in the model. The handler interfaces is complete UI less, allowing the clean separation of UI and the logic behind.
    /// </para>
    /// <para>
    /// To be able to handle different kinds of UI elements with different logic (e.g. a single item, a ItemsControl with separate
    /// items that shall allow drop into a specific index, TreeViews, etc), the UI handling logic is again separated
    /// from the drag and drop logic. New UI handlers can be registered in this class with <see cref="RegisterDragDropUISourceHandler"/>
    /// and <see cref="RegisterDragDropUITargetHandler"/> respectively.
    /// </para>
    ///</remarks>
    public class DragDrop
    {
        private static readonly DependencyPropertyEventManager sourceChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager targetChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager activateHelperChangedEventManager = new DependencyPropertyEventManager();

        // ReSharper disable MemberCanBePrivate.Global
        ///<summary>
        /// Register a drag and drop source handler for a given UI element
        ///</summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(IDragDropSource), typeof(DragDrop), new FrameworkPropertyMetadata(null, DragDrop.sourceChangedEventManager.ChangedHandler));
        ///<summary>
        /// Register a drag and drop target handler for a given UI element
        ///</summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached("Target", typeof(IDragDropTarget), typeof(DragDrop), new FrameworkPropertyMetadata(null, DragDrop.targetChangedEventManager.ChangedHandler));
        ///<summary>
        /// Add drag and drop support for the given UI element. Within activated UIElements, DnD "Helper" handler are used
        ///</summary>
        public static readonly DependencyProperty ActivateHelperProperty = DependencyProperty.RegisterAttached("ActivateHelper", typeof(string), typeof(DragDrop), new FrameworkPropertyMetadata(null, DragDrop.activateHelperChangedEventManager.ChangedHandler));
        // ReSharper restore MemberCanBePrivate.Global

        private static readonly Dictionary<DependencyObject, DragDropSourceAdapter> dragDropSources = new Dictionary<DependencyObject, DragDropSourceAdapter>();
        private static readonly Dictionary<DependencyObject, DragDropTargetAdapter> dragDropTargets = new Dictionary<DependencyObject, DragDropTargetAdapter>();
        // ReSharper disable once CollectionNeverQueried.Local
        private static readonly Dictionary<DependencyObject, DragDropHelperAdapter> dragDropHelper = new Dictionary<DependencyObject, DragDropHelperAdapter>();
        
        private static readonly Dictionary<Type, IDragDropUiSourceHandler> dragSourceHandlers = new Dictionary<Type, IDragDropUiSourceHandler>();
        private static readonly Dictionary<Type, IDragDropUiTargetHandler> dragTargetHandlers = new Dictionary<Type, IDragDropUiTargetHandler>();
        private static readonly Dictionary<Type, IDragDropUiHelper> dragHelper = new Dictionary<Type, IDragDropUiHelper>();


        static DragDrop()
        {
            DragDrop.sourceChangedEventManager.Changed += DragDrop.SourceChanged;
            DragDrop.targetChangedEventManager.Changed += DragDrop.TargetChanged;
            DragDrop.activateHelperChangedEventManager.Changed += DragDrop.ActivateHelperChanged;

            DragDrop.RegisterDragDropUISourceHandler(new FrameworkElementDragDropUiHandler());
            DragDrop.RegisterDragDropUISourceHandler(new FrameworkContentElementDragDropUiHandler());

            DragDrop.RegisterDragDropUITargetHandler(new StackPanelDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new VirtualizingStackPanelDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new TabPanelDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new TabControlDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new ItemsControlDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new FrameworkElementDragDropUiHandler());
            DragDrop.RegisterDragDropUITargetHandler(new FrameworkContentElementDragDropUiHandler());

            DragDrop.RegisterDragDropUiHelper(new TabControlDragDropHelper());
            DragDrop.RegisterDragDropUiHelper(new FrameworkElementDragDropUiHelper());

        }

        // ReSharper disable UnusedMember.Global
        ///<summary>
        /// Registers a drag and drop source handler for a given UI element
        ///</summary>
        public static void SetSource(DependencyObject element, IDragDropSource source)
        {
            element.SetValue(DragDrop.SourceProperty, source);
        }

        ///<summary>
        /// Gets the drag and drop source handler registered for the given UI element
        ///</summary>
        public static IDragDropSource GetSource(DependencyObject element)
        {
            return (IDragDropSource)element.GetValue(DragDrop.SourceProperty);
        }

        ///<summary>
        /// Registers a drag and drop target handler for a given UI element
        ///</summary>
        public static void SetTarget(DependencyObject element, IDragDropTarget target)
        {
            element.SetValue(DragDrop.TargetProperty, target);
        }

        ///<summary>
        /// Gets the drag and drop target handler registered for the given UI element
        ///</summary>
        public static IDragDropTarget GetTarget(DependencyObject element)
        {
            return (IDragDropTarget)element.GetValue(DragDrop.TargetProperty);
        }

        ///<summary>
        /// Registers a drag and drop helper for a given UI element
        ///</summary>
        public static void SetActivateHelper(DependencyObject element, string value)
        {
            element.SetValue(DragDrop.ActivateHelperProperty, value);
        }

        ///<summary>
        /// Gets the drag and drop helper registered for the given UI element
        ///</summary>
        public static string GetActivateHelper(DependencyObject element)
        {
            return (string)element.GetValue(DragDrop.ActivateHelperProperty);
        }        // ReSharper restore UnusedMember.Global

        private static void SourceChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = sender as DependencyObject;
            IDragDropSource NewSource = e.NewValue as IDragDropSource;
            IDragDropSource CurrentSource = e.OldValue as IDragDropSource;

            if (Element is UIElement || Element is ContentElement)
            {
                if (CurrentSource != null)
                {
                    DragDropSourceAdapter Adapter;
                    DragDrop.dragDropSources.TryGetValue(Element, out Adapter).DbC_Assure(isSuccess => isSuccess);
                    DragDrop.dragDropSources.Remove(Element);
                    Adapter?.Dispose();
                }

                if (NewSource != null)
                {
                    DragDropSourceAdapter Adapter = DragDropSourceAdapter.Create(NewSource, Element);
                    DragDrop.dragDropSources.Add(Element, Adapter);
                }
            }
            else
            {
                throw new InvalidOperationException("Drag and Drop source can only be registered on UIElements and ContentElements");
            }
        }

        private static void TargetChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = sender as DependencyObject;
            IDragDropTarget NewTarget = e.NewValue as IDragDropTarget;
            IDragDropTarget CurrentTarget = e.OldValue as IDragDropTarget;

            if (Element is UIElement || Element is ContentElement)
            {
                if (CurrentTarget != null)
                {
                    DragDropTargetAdapter Adapter;
                    DragDrop.dragDropTargets.TryGetValue(Element, out Adapter).DbC_Assure(isSuccess => isSuccess);
                    DragDrop.dragDropTargets.Remove(Element);
                    Adapter?.Dispose();
                }

                if (NewTarget != null)
                {
                    DragDropTargetAdapter Adapter = DragDropTargetAdapter.Create(NewTarget, Element, true);
                    DragDrop.dragDropTargets.Add(Element, Adapter);
                }
            }
            else
            {
                throw new InvalidOperationException("Drag and Drop target can only be registered on UIElements and ContentElements");
            }
        }

        private static void ActivateHelperChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = sender as DependencyObject;

            if (Element is UIElement)
            {
                ((UIElement) Element).AllowDrop = true;
                DragDropHelperAdapter Adapter = DragDropHelperAdapter.Create((UIElement) Element);
                DragDrop.dragDropHelper.Add(Element, Adapter);
            }
            else
            {
                throw new InvalidOperationException("Drag and Drop target can only be registered on UIElements and ContentElements");
            }
        }

        // ReSharper disable MemberCanBePrivate.Global
        /// <summary>
        /// Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        /// If a handler was registered before for the exact same type (e.g. one of the default handlers), 
        /// it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUISourceHandler(IDragDropUiSourceHandler handler)
        {
            if (DragDrop.dragSourceHandlers.ContainsKey(handler.Type))
            {
                DragDrop.dragSourceHandlers.Remove(handler.Type);
            }
            DragDrop.dragSourceHandlers.Add(handler.Type, handler);
        }

        /// <summary>
        /// Gets the handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        /// <para>
        /// The handlers are searched to find the most specific handler available for the type. This means, 
        /// first the handler for the exact type is searched for. If nothing is found, the search is repeated
        /// with the base type and so on.
        /// </para>
        /// <para>
        /// if nothing is foud, <c>null</c> is returned
        /// </para>
        /// </remarks>
        public static IDragDropUiSourceHandler GetDragDropUISourceHandler(Type uiElementType)
        {
            return DragDrop.GetDragDropUIHandler(DragDrop.dragSourceHandlers, uiElementType);
        }
        
        /// <summary>
        /// Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        /// If a handler was registered before for the exact same type (e.g. one of the default handlers), 
        /// it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUITargetHandler(IDragDropUiTargetHandler handler)
        {
            if (DragDrop.dragTargetHandlers.ContainsKey(handler.Type))
            {
                DragDrop.dragTargetHandlers.Remove(handler.Type);
            }
            DragDrop.dragTargetHandlers.Add(handler.Type, handler);
        }  
 
        /// <summary>
        /// Gets the handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        /// <para>
        /// The handlers are searched to find the most specific handler available for the type. This means, 
        /// first the handler for the exact type is searched for. If nothing is found, the search is repeated
        /// with the base type and so on.
        /// </para>
        /// <para>
        /// if nothing is foud, <c>null</c> is returned
        /// </para>
        /// </remarks>
        public static IDragDropUiTargetHandler GetDragDropUITargetHandler(Type uiElementType)
        {
            return DragDrop.GetDragDropUIHandler(DragDrop.dragTargetHandlers,uiElementType);
        }       

        /// <summary>
        /// Registeres a handler for the UI element type defined by it 'Type' property
        /// </summary>
        /// <remarks>
        /// If a handler was registered before for the exact same type (e.g. one of the default handlers), 
        /// it is removed and replaced by the handler given
        /// </remarks>
        public static void RegisterDragDropUiHelper(IDragDropUiHelper helper)
        {
            if (DragDrop.dragHelper.ContainsKey(helper.Type))
            {
                DragDrop.dragHelper.Remove(helper.Type);
            }
            DragDrop.dragHelper.Add(helper.Type, helper);
        }

        /// <summary>
        /// Gets all handler for the given UI element type used as drag and drop target
        /// </summary>
        /// <remarks>
        /// <para>
        /// The handlers are searched to find all handler available for the type. This means, 
        /// first the handler for the exact type is searched for, after that the search is continued
        /// with the base type and so on.
        /// </para>
        /// </remarks>
        public static IEnumerable<IDragDropUiHelper> GetDragDropUiHelper(Type uiElementType)
        {
            Type UiElementType = uiElementType;
            while (UiElementType != null)
            {
                if (DragDrop.dragHelper.ContainsKey(UiElementType))
                {
                    yield return DragDrop.dragHelper[UiElementType];
                }

                UiElementType = UiElementType.BaseType;
            }
        }   
        // ReSharper restore MemberCanBePrivate.Global

        private static THandlerType GetDragDropUIHandler<THandlerType>(Dictionary<Type, THandlerType> handler, Type uiElementType) where THandlerType:class
        {
            Type UiElementType = uiElementType;
            while (UiElementType != null)
            {
                if (handler.ContainsKey(UiElementType))
                {
                    return handler[UiElementType];
                }

                UiElementType = UiElementType.BaseType;
            }
            return null;
        }
    }
}
