#if NET35
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using WhileTrue.Classes.CodeInspection;

namespace WhileTrue.Classes.Framework
{
    [NoCoverage]
    internal abstract class ExpressionVisitor
    {
        protected Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMember((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        private MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        private ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> Arguments = this.VisitExpressionList(initializer.Arguments);
            if (Arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, Arguments);
            }
            return initializer;
        }

        private Expression VisitUnary(UnaryExpression u)
        {
            Expression Operand = this.Visit(u.Operand);
            if (Operand != u.Operand)
            {
                return Expression.MakeUnary(u.NodeType, Operand, u.Type, u.Method);
            }
            return u;
        }

        private Expression VisitBinary(BinaryExpression b)
        {
            Expression Left = this.Visit(b.Left);
            Expression Right = this.Visit(b.Right);
            Expression Conversion = this.Visit(b.Conversion);
            if (Left != b.Left || Right != b.Right || Conversion != b.Conversion)
            {
                if (b.NodeType == ExpressionType.Coalesce && b.Conversion != null)
                    return Expression.Coalesce(Left, Right, Conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(b.NodeType, Left, Right, b.IsLiftedToNull, b.Method);
            }
            return b;
        }

        private Expression VisitTypeIs(TypeBinaryExpression b)
        {
            Expression Expr = this.Visit(b.Expression);
            if (Expr != b.Expression)
            {
                return Expression.TypeIs(Expr, b.TypeOperand);
            }
            return b;
        }

        private static Expression VisitConstant(ConstantExpression c)
        {
            return c;
        }

        private Expression VisitConditional(ConditionalExpression c)
        {
            Expression Test = this.Visit(c.Test);
            Expression IfTrue = this.Visit(c.IfTrue);
            Expression IfFalse = this.Visit(c.IfFalse);
            if (Test != c.Test || IfTrue != c.IfTrue || IfFalse != c.IfFalse)
            {
                return Expression.Condition(Test, IfTrue, IfFalse);
            }
            return c;
        }

        private static Expression VisitParameter(ParameterExpression p)
        {
            return p;
        }

        protected virtual Expression VisitMember(MemberExpression m)
        {
            Expression Exp = this.Visit(m.Expression);
            if (Exp != m.Expression)
            {
                return Expression.MakeMemberAccess(Exp, m.Member);
            }
            return m;
        }

        private Expression VisitMethodCall(MethodCallExpression m)
        {
            Expression Obj = this.Visit(m.Object);
            IEnumerable<Expression> Args = this.VisitExpressionList(m.Arguments);
            if (Args == null) throw new NotImplementedException();
            if (Obj != m.Object || Args != m.Arguments)
            {
                return Expression.Call(Obj, m.Method, Args);
            }
            return m;
        }

        private ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> List = null;
            int N = original.Count;
            for (int I = 0; I < N; I++)
            {
                Expression P = this.Visit(original[I]);
                if (List != null)
                {
                    List.Add(P);
                }
                else if (P != original[I])
                {
                    List = new List<Expression>(N);
                    for (int J = 0; J < I; J++)
                    {
                        List.Add(original[J]);
                    }
                    List.Add(P);
                }
            }
            if (List != null)
            {
                return List.AsReadOnly();
            }
            return original;
        }

        private MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression E = this.Visit(assignment.Expression);
            if (E != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, E);
            }
            return assignment;
        }

        private MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> Bindings = this.VisitBindingList(binding.Bindings);
            if (Bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, Bindings);
            }
            return binding;
        }

        private MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> Initializers = this.VisitElementInitializerList(binding.Initializers);
            if (Initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, Initializers);
            }
            return binding;
        }

        private IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> List = null;
            int I = 0;
            int N = original.Count;
            for (; I < N; I++)
            {
                MemberBinding B = this.VisitBinding(original[I]);
                if (List != null)
                {
                    List.Add(B);
                }
                else if (B != original[I])
                {
                    List = new List<MemberBinding>(N);
                    for (int J = 0; J < I; J++)
                    {
                        List.Add(original[J]);
                    }
                    List.Add(B);
                }
            }
            if (List != null)
                return List;
            return original;
        }

        private IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> List = null;
            int N = original.Count;
            for (int I = 0; I < N; I++)
            {
                ElementInit Init = this.VisitElementInitializer(original[I]);
                if (List != null)
                {
                    List.Add(Init);
                }
                else if (Init != original[I])
                {
                    List = new List<ElementInit>(N);
                    for (int J = 0; J < I; J++)
                    {
                        List.Add(original[J]);
                    }
                    List.Add(Init);
                }
            }
            if (List != null)
                return List;
            return original;
        }

        private Expression VisitLambda(LambdaExpression lambda)
        {
            Expression Body = this.Visit(lambda.Body);
            if (Body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, Body, lambda.Parameters);
            }
            return lambda;
        }

        private NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> Args = this.VisitExpressionList(nex.Arguments);
            if (Args != nex.Arguments)
            {
                if (nex.Members != null)
                    return Expression.New(nex.Constructor, Args, nex.Members);
                else
                    return Expression.New(nex.Constructor, Args);
            }
            return nex;
        }

        private Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression N = this.VisitNew(init.NewExpression);
            IEnumerable<MemberBinding> Bindings = this.VisitBindingList(init.Bindings);
            if (N != init.NewExpression || Bindings != init.Bindings)
            {
                return Expression.MemberInit(N, Bindings);
            }
            return init;
        }

        private Expression VisitListInit(ListInitExpression init)
        {
            NewExpression N = this.VisitNew(init.NewExpression);
            IEnumerable<ElementInit> Initializers = this.VisitElementInitializerList(init.Initializers);
            if (N != init.NewExpression || Initializers != init.Initializers)
            {
                return Expression.ListInit(N, Initializers);
            }
            return init;
        }

        private Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> Exprs = this.VisitExpressionList(na.Expressions);
            if (Exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), Exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), Exprs);
                }
            }
            return na;
        }

        private Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> Args = this.VisitExpressionList(iv.Arguments);
            Expression Expr = this.Visit(iv.Expression);
            if (Args != iv.Arguments || Expr != iv.Expression)
            {
                return Expression.Invoke(Expr, Args);
            }
            return iv;
        }
    }
}
#endif