using System;
using System.Linq;
using System.Linq.Expressions;
using WhileTrue.Classes.Components;
using WhileTrue.Modules.ModelInspectorWindow;

namespace WhileTrue.Modules.ModelInspector
{
    /*
     * Object               ComplexModelType
     * |-- Property1        SimpleModelType
     * |-- Property2        ComplexModelType
     *     |-- Property21   SimpleModelType
     *     |-- Property22[] EnumerableModelType
     *         |-- [0]      EnumerableModelItem
     * 

     */
    [Component]
    internal class ModelInspector : IModelInspector
    {
        private readonly IModelInspectorModel model;
        private readonly IModelInspectorWindow inspectorWindow;

        public ModelInspector(IModelInspectorModel model, IModelInspectorWindow inspectorWindow)
        {
            this.model = model;
            this.inspectorWindow = inspectorWindow;
        }

        public void Inspect(object modelRoot, string name)
        {
            this.Inspect(()=>modelRoot, name);
        }

        public void Inspect(Expression<Func<object>> modelRoot, string name)
        {
            IModelGroup Group = this.model.Groups.FirstOrDefault(group => group.Name == name) ?? ModelInspector.CreateNewGroup(this.model, name);
            Group.Models.Add(new ModelInfo(modelRoot, name, true));

            //Show on first entry
            if (this.model.Groups.Count == 1)
            {
                this.inspectorWindow.Show();
            }
        }

        private static IModelGroup CreateNewGroup(IModelInspectorModel modelInspectorModel, string name)
        {
            ModelGroup Group = new ModelGroup(name);
            modelInspectorModel.Groups.Add(Group);
            return Group;
        }
    }
}
