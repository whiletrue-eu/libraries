using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using WhileTrue.Classes.Framework;
using WhileTrue.Modules.ModelInspector;
using WhileTrue.Modules.ModelInspectorWindow.Model;

namespace WhileTrue.Modules.ModelInspectorWindow
{
    internal class DesignData : IModelInspectorWindowModel
    {
        private readonly IModelGroupCollection groups = new ModelGroupCollection();

        public DesignData()
        {
            ModelGroup Group = new ModelGroup("Group");
            Group.Models.Add(new ModelInfo(new ComplexData(), "DesignData", true));
            this.groups.Add(Group);
        }


        private class ComplexData : ObservableObject
        {
            private bool mark;
            public ComplexData()
            {
                new Thread((ThreadStart)delegate
                                            {
                                                while (true)
                                                {
                                                    this.mark = !this.mark;
                                                    this.InvokePropertyChanged(nameof(this.SimpleProperty));
                                                    Thread.Sleep(10*1000);
                                                }
                                                // ReSharper disable once FunctionNeverReturns
                                            }).Start();
            }

            private string SimpleProperty => $"Hello,World{(this.mark ? "!" : "")}";
        }

        public IEnumerable<ModelGroupAdapter> Groups => new ObservableCollection<ModelGroupAdapter>{ new ModelGroupAdapter(null, this.groups.ToArray()[0]) };
    }
}