using System;
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


        internal class ComplexData : ObservableObject
        {
            private bool mark;
            public ComplexData()
            {
                new Thread((ThreadStart)delegate
                                            {
                                                while (true)
                                                {
                                                    this.mark = !this.mark;
                                                    this.InvokePropertyChanged(() => SimpleProperty);
                                                    Thread.Sleep(10*1000);
                                                }
                                            }).Start();
            }

            public string SimpleProperty
            {
                get { return string.Format("Hello,World{0}", (this.mark?"!":"")); }
            }
        }

        public IEnumerable<ModelGroupAdapter> Groups
        {
            get { return new ObservableCollection<ModelGroupAdapter>{ new ModelGroupAdapter(null, groups.ToArray()[0]) }; }
        }
    }
}