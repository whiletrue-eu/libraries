using System;
using WhileTrue.Classes.Components;

namespace WhileTrue.Facades.UIFeatures
{
    /// <summary>
    /// Supports UI feature Management
    /// </summary>
    [ComponentInterface]
    public interface IUiFeatureManager
    {
        /// <summary/>
        bool IsVisible(string context);
        /// <summary/>
        bool IsEnabled(string context);
        /// <summary/>
        event EventHandler<EventArgs> FeaturesChanged;
    }
}