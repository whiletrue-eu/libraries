using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace WhileTrue.Classes.Components
{
    internal static class ComponentContainerUtils
    {
        private static object DoDynamicCast(Type parameterType, object value)
        {
            LambdaExpression Lambda = Expression.Lambda(
                parameterType,
                Expression.Convert(Expression.Constant(value),parameterType)
            );
            return ((Func<object>)Lambda.Compile())();
        }

        public static object DoDynamicCastAsync(Type taskType, Task<object> value)
        {
            Type EncapsulatedType = taskType.GenericTypeArguments[0]; //parametertype is some Task<T>, encapsulated type is T
            Type CompletionSourceType = typeof(TaskCompletionSource<>).MakeGenericType(EncapsulatedType);
            ParameterExpression CompletionSource = Expression.Variable(CompletionSourceType,@"completionSource");
            ParameterExpression CompletionSourceTask = Expression.Variable(taskType,@"task");
            ParameterExpression ContinueWithArg = Expression.Parameter(typeof(Task<object>),@"continueWithResult");


            /*
             * Expression Code below:
             * NewCompletionSource       CompletionSource = new TaskCompletionSource<T>()
             * ContinueWith / ..lambda   value.ContinueWith(_=>CompletionSource.SetResult((T)_))
             * GetTask / ReturnTask      return CommpletionsSource.Task
             */

            BinaryExpression NewCompletionSource = Expression.Assign(
                CompletionSource,
                Expression.New(CompletionSourceType)
            );
            LambdaExpression ContinueWithLambda = Expression.Lambda(
                typeof(Action<Task<object>>),
                Expression.Call(CompletionSource, nameof(TaskCompletionSource<object>.SetResult), null,
                    Expression.Convert(
                        Expression.Property(ContinueWithArg, nameof(Task<object>.Result)),
                        EncapsulatedType)
                ),
                ContinueWithArg
            );
            MethodCallExpression ContinueWith = Expression.Call(Expression.Constant(value), nameof(Task<object>.ContinueWith), null,
                ContinueWithLambda
            );
            BinaryExpression GetTask = Expression.Assign(
                CompletionSourceTask,
                Expression.Property(CompletionSource, CompletionSourceType.GetRuntimeProperty(nameof(TaskCompletionSource<object>.Task)))
            );
            ParameterExpression ReturnTask = CompletionSourceTask;


            LambdaExpression Lambda = Expression.Lambda(
                typeof(Func<>).MakeGenericType(taskType),
                Expression.Block(
                    taskType,
                    new[] { CompletionSource, CompletionSourceTask},
                    NewCompletionSource, ContinueWith, GetTask, ReturnTask
                )
            );
            return ((Func<object>)Lambda.Compile())();
        }
    }
}