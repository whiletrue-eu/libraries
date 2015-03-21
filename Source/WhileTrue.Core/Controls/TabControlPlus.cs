using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WhileTrue.Controls
{
    public class TabControlPlus : TabControl
    {
       #region dependency / attached properties

        /// <summary/>
        public static readonly DependencyProperty TabPanelCommandControlsProperty;
        private static readonly DependencyPropertyKey tabPanelCommandControlsPropertyKey;

        static TabControlPlus()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabControlPlus), new FrameworkPropertyMetadata(typeof(TabControlPlus)));

            tabPanelCommandControlsPropertyKey = DependencyProperty.RegisterReadOnly(
                "TabPanelCommandControls",
                typeof (ObservableCollection<Control>),
                typeof(TabControlPlus),
                new FrameworkPropertyMetadata(new ObservableCollection<Control>())
                );
            TabPanelCommandControlsProperty = tabPanelCommandControlsPropertyKey.DependencyProperty;
        }


        #endregion

        /// <summary>
        /// Gets/Sets the list of dialog buttons.
        /// </summary>
        public ObservableCollection<Control> TabPanelCommandControls
        {
            get { return (ObservableCollection<Control>)this.GetValue(TabPanelCommandControlsProperty); }
            set { this.SetValue(TabPanelCommandControlsProperty, value); }
        }
    }
}
