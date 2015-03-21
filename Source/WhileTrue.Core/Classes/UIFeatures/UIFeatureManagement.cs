// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WhileTrue.Classes.Framework;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Classes.UIFeatures
{
    public class UIFeatureManagement : DependencyObject
    {
        private static readonly DependencyPropertyEventManager contextChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager managerChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager contextPathChangedEventManager = new DependencyPropertyEventManager();


        public static readonly DependencyProperty ContextProperty = DependencyProperty.RegisterAttached("Context", typeof (string), typeof(UIFeatureManagement), new FrameworkPropertyMetadata(null, contextChangedEventManager.ChangedHandler));
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.RegisterAttached("Manager", typeof(IUIFeatureManager), typeof(UIFeatureManagement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, managerChangedEventManager.ChangedHandler));
// ReSharper disable InconsistentNaming
        private static readonly DependencyProperty ContextPathProperty = DependencyProperty.RegisterAttached("ContextPath", typeof(string), typeof(UIFeatureManagement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, contextPathChangedEventManager.ChangedHandler));
// ReSharper restore InconsistentNaming


        static UIFeatureManagement()
        {
            contextChangedEventManager.Changed += ContextChanged;
        }

        private static void ContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = (DependencyObject) sender;
            // Clear local path, so that parent value is inherited for this element
            ReEvaluateContextPath(Element);
        }

        private static void ReEvaluateContextPath(DependencyObject element)
        {
            ClearContextPath(element);

            //Get (inherited) parent path
            string ParentContextPath = GetContextPath(element);
            string Context = GetContext(element);

            if (Context != null)
            {
                //Compute and set new path
                if (ParentContextPath != null)
                {
                    SetContextPath(element, string.Format("{0}/{1}", ParentContextPath, Context));
                }
                else
                {
                    SetContextPath(element, Context);
                }
            }
            NotifyChildrenThatContextPathChanged(element);
        }

        private static void NotifyChildrenThatContextPathChanged(DependencyObject element)
        {
            if (element is Visual || element is Visual3D)
            {
                for (int Index = 0; Index < VisualTreeHelper.GetChildrenCount(element); Index++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(element, Index);

                    if (Child.ReadLocalValue(ContextPathProperty) != DependencyProperty.UnsetValue)
                        //if ( UIFeatureManagement.GetContext(Child) != null)
                    {
                        //there is a Context registered. this means, the context path must be re-evaluated.
                        //re-evaluation of children will be done in the called method
                        ReEvaluateContextPath(Child);
                    }
                    else
                    {
                        //no context is specified here, so check the children
                        NotifyChildrenThatContextPathChanged(Child);
                    }
                }
            }
        }

        public static void SetContext(DependencyObject element, string id)
        {
            element.SetValue(ContextProperty, id);
        }

        public static string GetContext(DependencyObject element)
        {
            return (string) element.GetValue(ContextProperty);
        }

        public static void SetManager(DependencyObject element, IUIFeatureManager manager)
        {
            element.SetValue(ManagerProperty, manager);
        }

        public static IUIFeatureManager GetManager(DependencyObject element)
        {
            return (IUIFeatureManager)element.GetValue(ManagerProperty);
        }

        private static void SetContextPath(DependencyObject element, string idPath)
        {
            element.SetValue(ContextPathProperty, idPath);
        }

        internal static string GetContextPath(DependencyObject element)
        {
            return (string) element.GetValue(ContextPathProperty);
        }

        private static void ClearContextPath(DependencyObject element)
        {
            element.ClearValue(ContextPathProperty);
        }

        internal static void AddContextPathChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            contextPathChangedEventManager.AddEventHandler(dependencyObject, handler);
        }

        internal static void AddManagerChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            managerChangedEventManager.AddEventHandler(dependencyObject, handler);
        }
    }
}