using System;
using System.Linq;
using WhileTrue.Classes.Components;
using WhileTrue.Classes.Utilities;
using WhileTrue.Facades.UIFeatures;

namespace WhileTrue.Components.UIFeatures
{
    /// <summary>
    /// Implements a feature / permission manager that aggregates its rules from different <see cref="IUiFeatureManagerSource"/> provider,
    /// which can implement different rule sets (e.g. user centric, context specific ect.)
    /// </summary>
    [Component("UI Feature Manager")]
    public class UiFeatureManager : IUiFeatureManager
    {
        private readonly IUiFeatureManagerSource[] sources;

        /// <summary/>
        public UiFeatureManager(IUiFeatureManagerSource[] sources)
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

        /// <summary/>
        public bool IsVisible(string context)
        {
            //Visible if any of the source says so
            return this.sources.Any(source => source.IsVisible(context));
            //todo: caching for performance
        }

        /// <summary/>
        public bool IsEnabled(string context)
        {
            //Enable if one of the sources which have the element visible allows
            IUiFeatureManagerSource[] VisibleSources = this.sources.Where(source => source.IsVisible(context)).ToArray();
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

        /// <summary/>
        public event EventHandler<EventArgs> FeaturesChanged = delegate { };
    }
}
