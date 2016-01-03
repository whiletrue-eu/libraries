using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.UIFeatures
{
    /// <summary>
    /// Implemented by a source of the feature management service
    /// </summary>
    [ComponentInterface]
    public interface IUiFeatureManagerSource
    {
        /// <summary/>
        bool IsVisible(string context);
        /// <summary/>
        bool IsEnabled(string context);
        /// <summary/>
        event EventHandler<EventArgs> FeaturesChanged;
    }
}