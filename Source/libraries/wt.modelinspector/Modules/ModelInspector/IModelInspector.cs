using System;
using System.Linq.Expressions;
using WhileTrue.Classes.Components;

namespace WhileTrue.Modules.ModelInspector
{
    [ComponentInterface]
    public interface IModelInspector
    {
        void Inspect(object modelRoot, string name);
        void Inspect(Expression<Func<object>> modelRoot, string name);
    }
}