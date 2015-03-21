// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using WhileTrue.Classes.Framework;

namespace WhileTrue.Classes.Wpf
{
    public class PersistentProperty : DependencyObject
    {
        private static readonly DependencyPropertyEventManager idChangedEventManager = new DependencyPropertyEventManager();
        private static readonly DependencyPropertyEventManager idPathChangedEventManager = new DependencyPropertyEventManager();


        public static readonly DependencyProperty IDProperty = DependencyProperty.RegisterAttached("ID", typeof (string), typeof(PersistentProperty), new FrameworkPropertyMetadata(null, idChangedEventManager.ChangedHandler));
// ReSharper disable InconsistentNaming
        private static readonly DependencyProperty IDPathProperty = DependencyProperty.RegisterAttached("IDPath", typeof(string), typeof(PersistentProperty), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, idPathChangedEventManager.ChangedHandler));
// ReSharper restore InconsistentNaming


        static PersistentProperty()
        {
            idChangedEventManager.Changed += IDChangedEventManagerChanged;
        }

        static void IDChangedEventManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DependencyObject Element = (DependencyObject) sender;
            // Clear local path, so that parent value is inherited for this element
            ReEvaluateIDPath(Element);
        }

        private static void ReEvaluateIDPath(DependencyObject element)
        {
            ClearIDPath(element);

            //Get (inherited) parent path
            string ParentIDPath = GetIDPath(element);
            string ID = GetID(element);

            if (ID != null)
            {
                //Compute and set new path
                if (ParentIDPath != null)
                {
                    SetIDPath(element, string.Format("{0}.{1}", ParentIDPath, ID));
                }
                else
                {
                    SetIDPath(element, ID);
                }
            }
            NotifyChildrenThatIDPathChanged(element);
        }

        private static void NotifyChildrenThatIDPathChanged(DependencyObject element)
        {
            if (element is Visual || element is Visual3D)
            {
                for (int Index = 0; Index < VisualTreeHelper.GetChildrenCount(element); Index++)
                {
                    DependencyObject Child = VisualTreeHelper.GetChild(element, Index);

                    if (Child.ReadLocalValue(IDPathProperty) != DependencyProperty.UnsetValue)
                    {
                        //there is a Context registered. this means, the context path must be re-evaluated.
                        //re-evaluation of children will be done in the called method
                        ReEvaluateIDPath(Child);
                    }
                    else
                    {
                        //no context is specified here, so check the children
                        NotifyChildrenThatIDPathChanged(Child);
                    }
                }
            }
        }

        public static void SetID(DependencyObject element, string id)
        {
            element.SetValue(IDProperty, id);
        }

        public static string GetID(DependencyObject element)
        {
            return (string) element.GetValue(IDProperty);
        }

        private static void SetIDPath(DependencyObject element, string idPath)
        {
            element.SetValue(IDPathProperty, idPath);
        }

        internal static string GetIDPath(DependencyObject element)
        {
            return (string) element.GetValue(IDPathProperty);
        }

        private static void ClearIDPath(DependencyObject element)
        {
            element.ClearValue(IDPathProperty);
        }

        internal static void AddIDPathChangedEventHandler(DependencyObject dependencyObject, DependencyPropertyChangedEventHandler handler)
        {
            idPathChangedEventManager.AddEventHandler(dependencyObject, handler);
        }
    }
}