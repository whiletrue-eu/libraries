using System;
using System.Linq;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Components.UIFeatures
{
    [Component("UI Feature Manager")]
    public class UIFeatureManager : IUIFeatureManager
    {
        private readonly IUIFeatureManagerSource[] sources;

        public UIFeatureManager(IUIFeatureManagerSource[] sources)
        {
            this.sources = sources;
            this.sources.ForEach(source => source.FeaturesChanged += this.SourceFeaturesChanged);
        }

        void SourceFeaturesChanged(object sender, EventArgs e)
        {
            this.InvokeFeaturesChanged();
        }

        private void InvokeFeaturesChanged()
        {
            this.FeaturesChanged(this, EventArgs.Empty);
        }

        public bool IsVisible(string context)
        {
            //Visible if any of the source says so
            return sources.Any(source => source.IsVisible(context));
            //todo: caching for performance
        }

        public bool IsEnabled(string context)
        {
            //Enable if one of the sources which have the element visible allows
            IUIFeatureManagerSource[] VisibleSources = sources.Where(source => source.IsVisible(context)).ToArray();
            if (VisibleSources.Length > 0)
            {
                return VisibleSources.All(source => source.IsEnabled(context));
            }
            else
            {
                return false;
            }
            //todo: caching for performance
        }

        public event EventHandler<EventArgs> FeaturesChanged = delegate { };
    }
}
