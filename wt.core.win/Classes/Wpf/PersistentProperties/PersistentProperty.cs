// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WhileTrue.Classes.Wpf
{
    /// <summary>
    ///     Use the attached dependency properties within this class to set up the paths for UI persistence. You can use the Id
    ///     proprty to set an Id on any ui element
    ///     When UI states are persisted using the <see cref="PersistentPropertyExtension" />, the Ids will be concatenated to
    ///     an absoute path.
    ///     As the ID can be bound to the model, you can use names, id's etc. to distinguish between elements that were created
    ///     through a datatemplate
    /// </summary>
    public class PersistentProperty : DependencyObject
    {
        private static readonly DependencyPropertyEventManager idChangedEventManager =
            new DependencyPropertyEventManager();

        private static readonly DependencyPropertyEventManager idPathChangedEventManager =
            new DependencyPropertyEventManager();


        /// <summary>
        ///     Sets/Gets the Id of the ui element used for property persistence
        /// </summary>
        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached("Id", typeof(string),
            typeof(PersistentProperty), new FrameworkPropertyMetadata(null, idChangedEventManager.ChangedHandler));

// ReSharper disable InconsistentNaming
        private static readonly DependencyProperty IdPathProperty = DependencyProperty.RegisterAttached("IdPath",
            typeof(string), typeof(PersistentProperty),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits,
                idPathChangedEventManager.ChangedHandler));
// ReSharper restore InconsistentNaming


        static PersistentProperty()
        {
            idChangedEventManager.Changed += IdChangedEventManagerChanged;
        }

        private static void IdChangedEventManagerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var Element = (DependencyObject) sender;
            // Clear local path, so that parent value is inherited for this element
            ReEvaluateIdPath(Element);
        }

        private static void ReEvaluateIdPath(DependencyObject element)
        {
            ClearIdPath(element);

            //Get (inherited) parent path
            var ParentIdPath = GetIdPath(element);
            var Id = GetId(element);

            if (Id != null) SetIdPath(element, ParentIdPath != null ? $"{ParentIdPath}.{Id}" : Id);
            NotifyChildrenThatIdPathChanged(element);
        }

        private static void NotifyChildrenThatIdPathChanged(DependencyObject element)
        {
            if (element is Visual || element is Visual3D)
                for (var Index = 0; Index < VisualTreeHelper.GetChildrenCount(element); Index++)
                {
                    var Child = VisualTreeHelper.GetChild(element, Index);

                    if (Child.ReadLocalValue(IdPathProperty) != DependencyProperty.UnsetValue)
                        ReEvaluateIdPath(Child);
                    else
                        NotifyChildrenThatIdPathChanged(Child);
                }
        }

        /// <summary>
        ///     Sets the Id of the ui element used for property persistence
        /// </summary>
        public static void SetId(DependencyObject element, string id)
        {
            element.SetValue(IdProperty, id);
        }

        /// <summary>
        ///     Gets the Id of the ui element used for property persistence
        /// </summary>
        public static string GetId(DependencyObject element)
        {
            return (string) element.GetValue(IdProperty);
        }

        private static void SetIdPath(DependencyObject element, string idPath)
        {
            element.SetValue(IdPathProperty, idPath);
        }

        internal static string GetIdPath(DependencyObject element)
        {
            return (string) element.GetValue(IdPathProperty);
        }

        private static void ClearIdPath(DependencyObject element)
        {
            element.ClearValue(IdPathProperty);
        }

        internal static void AddIDPathChangedEventHandler(DependencyObject dependencyObject,
            DependencyPropertyChangedEventHandler handler)
        {
            idPathChangedEventManager.AddEventHandler(dependencyObject, handler);
        }
    }
}