using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace WhileTrue.Controls
{
    ///<summary>
    ///</summary>
    partial class DialogPanel 
    {
        private class PanelSynchronisationRoot
        {
            private readonly List<DialogPanel> members = new List<DialogPanel>();

            public void AddMember(DialogPanel panel)
            {
                this.members.Add(panel);
                panel.IsVisibleChanged += this.PanelIsVisibleChanged;
                panel.LayoutUpdated += this.PanelLayoutChanged;
                this.Update(null);
            }

            public void RemoveMember(DialogPanel panel)
            {
                this.members.Remove(panel);
                panel.IsVisibleChanged -= this.PanelIsVisibleChanged;
                panel.LayoutUpdated -= this.PanelLayoutChanged;
                this.Update(null);
            }

            void PanelIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                this.Update(null);
            }

            private void PanelLayoutChanged(object sender, EventArgs e)
            {
                this.Update((DialogPanel) sender);
            }


            private void Update(DialogPanel sender)
            {
                this.CaptionWidth = (from Panel in this.members where Panel.IsVisible select Panel.CalculatedCaptionWidth).Union(new[]{0d}).Max();

                foreach (DialogPanel Panel in this.members)
                {
                    // Force update on all panels with different captions width. 
                    // Sender is skipped, because it is in middle of calculation and will consider new value automatically
                    if (Panel != sender && Panel.UsedCaptionWidth != this.CaptionWidth)
                    {
                        Panel.InvalidateMeasure();
                    }
                }
            }

            public double CaptionWidth { get; private set; }

            public void NotifyMeasured(DialogPanel dialogPanel)
            {
                this.Update(dialogPanel);
            }
        }
    }
}