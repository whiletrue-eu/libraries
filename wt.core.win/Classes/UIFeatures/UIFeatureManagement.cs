// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WhileTrue.Classes.Wpf;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    /// <summary>
    /// Use the attached dependency properties within this class to set up the paths for UI feature management / permission management.
    /// You can use the Context proprty to set an Id on any ui element
    /// When UI permission values are evaluated, using the <see cref="FeatureEnabledExtension"/>, <see cref="FeatureReadOnlyExtension"/> 
    /// and <see cref="FeatureVisibleExtension"/>, the Contexts will be concatenated to an absoute path.
    /// As the Context can be bound to the model, you can use names, id's etc. to distinguish between elements that were created through a datatemplate.
    /// The above extensions will use the <see cref="IUiFeatureManager"/> that is registered within the UI hierarchy using the <see cref="ManagerProperty"/>
    /// </summary>
    public class UiFeatureManagement : DependencyObject
    {
        private static readonly DependencyPropertyEventManager contextChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager managerChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager contextPathChangedEventManager = new DependencyPropertyEventManager();


        /// <summary>
        /// Sets/Gets the context of the ui element used for ui permission
        /// </summary>
        public static readonly DependencyProperty ContextProperty = DependencyProperty.RegisterAttached("Context", typeof (string), typeof(UiFeatureManagement), new FrameworkPropertyMetadata(null, UiFeatureManagement.contextChangedEventManager.ChangedHandler));
        /// <summary>
        /// attaches the permission manager to the complete ui subhierarchy
        /// </summary>
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.RegisterAttached("Manager", typeof(IUiFeatureManager), typeof(UiFeatureManagement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, UiFeatureManagement.managerChangedEventManager.ChangedHandler));
// ReSharper disable InconsistentNaming
        private static readonly DependencyProperty ContextPathProperty = DependencyProperty.RegisterAttached("ContextPath", typeof(string), typeof(UiFeatureManagement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, UiFeatureManagement.contextPathChangedEventManager.ChangedHandler));
// ReSharper restore InconsistentNaming


        static UiFeatureManagement()
        {
            UiFeatureManagement.contextChangedEventManager.Changed += UiFeatureManagement.ContextChanged;
        }

        private static void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = (DependencyObject) sender;
            // Clear local path, so that parent value is inherited for this element
            UiFeatureManagement.ReEvaluateContextPath(Element);
        }

        private static void ReEvaluateContextPath(DependencyObject element)
        {
            UiFeatureManagement.ClearContextPath(element);

            //Get (inherited) parent path
            string ParentContextPath = UiFeatureManagement.GetContextPath(element);
            string Context = UiFeatureManagement.GetContext(element);

            if (Context != null)
            {
                //Compute and set new path
                UiFeatureManagement.SetContextPath(element, ParentContextPath != null ? $"{ParentContextPath}/{Context}" : Context);
            }
            UiFeatureManagement.NotifyChildrenThatContextPathChanged(element);
        }

        private static void NotifyChildrenThatContextPathChanged(DependencyObject element)
        {
            if (element is Visual || element is Visual3D)
            {
                for (int Index = 0; Index < VisualTreeHelper.GetChildrenCount(element); Index++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(element, Index);

                    if (Child.ReadLocalValue(UiFeatureManagement.ContextPathProperty) != DependencyProperty.UnsetValue)
                        //if ( UIFeatureManagement.GetContext(Child) != null)
                    {
                        //there is a Context registered. this means, the context path must be re-evaluated.
                        //re-evaluation of children will be done in the called method
                        UiFeatureManagement.ReEvaluateContextPath(Child);
                    }
                    else
                    {
                        //no context is specified here, so check the children
                        UiFeatureManagement.NotifyChildrenThatContextPathChanged(Child);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the context of the ui element used for ui permission
        /// </summary>
        public static void SetContext(DependencyObject element, string id)
        {
            element.SetValue(UiFeatureManagement.ContextProperty, id);
        }

        /// <summary>
        /// Gets the context of the ui element used for ui permission
        /// </summary>
        public static string GetContext(DependencyObject element)
        {
            return (string) element.GetValue(UiFeatureManagement.ContextProperty);
        }

        /// <summary>
        /// attaches the permission manager to the complete ui subhierarchy
        /// </summary>
        public static void SetManager(DependencyObject element, IUiFeatureManager manager)
        {
            element.SetValue(UiFeatureManagement.ManagerProperty, manager);
        }

        /// <summary>
        /// attaches the permission manager to the complete ui subhierarchy
        /// </summary>
        public static IUiFeatureManager GetManager(DependencyObject element)
        {
            return (IUiFeatureManager)element.GetValue(UiFeatureManagement.ManagerProperty);
        }

        private static void SetContextPath(DependencyObject element, string idPath)
        {
            element.SetValue(UiFeatureManagement.ContextPathProperty, idPath);
        }

        internal static string GetContextPath(DependencyObject element)
        {
            return (string) element.GetValue(UiFeatureManagement.ContextPathProperty);
        }

        private static void ClearContextPath(DependencyObject element)
        {
            element.ClearValue(UiFeatureManagement.ContextPathProperty);
        }

        internal static void AddContextPathChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            UiFeatureManagement.contextPathChangedEventManager.AddEventHandler(dependencyObject, handler);
        }

        internal static void AddManagerChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            UiFeatureManagement.managerChangedEventManager.AddEventHandler(dependencyObject, handler);
        }
    }
}