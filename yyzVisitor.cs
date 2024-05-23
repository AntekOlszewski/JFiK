using Antlr4.Runtime.Misc;
using JFiK.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JFiK
{
    public class yyzVisitor : yyzBaseVisitor<object?>
    {
        Dictionary<string, string> variables = new Dictionary<string, string>();
        Stack<(string, string)> stack = new Stack<(string, string)>();


        public override object? VisitAssignment([NotNull] yyzParser.AssignmentContext context)
        {
            var variableName = context.IDENTIFIER().GetText();
            Visit(context.expression());
            var global = context.GLOBAL() != null;

            if (variables.ContainsKey(variableName))
            {
                throw new Exception("variable already initialized");
            }

            var value = stack.Pop();

            variables.Add(variableName, value.Item1);
            LLVMGenerator.Declare(variableName, value.Item1);
            LLVMGenerator.Assign(variableName, value.Item2, value.Item1, global);

            return null;
        }

        public override object? VisitConstant([NotNull] yyzParser.ConstantContext context)
        {
            if (context.INTEGER() is { } i)
            {
                var inte = int.Parse(i.GetText());
                stack.Push(("i32", inte.ToString()));
                return inte;
            }
            if (context.DOUBLE() is { } d)
            {
                var doublee = double.Parse(d.GetText(), CultureInfo.InvariantCulture);

                var valueString = doublee.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
                if (!valueString.Contains("."))
                {
                    valueString += ".0";
                }

                stack.Push(("double", valueString));
                return doublee;
            }
            if (context.BOOL() is { } b)
                return b.GetText() == "true";
            if (context.STRING() is { } s)
                return s.GetText()[1..^1];
            if (context.NULL() is { } n)
                return null;
            throw new NotImplementedException("Unnown data type");
        }

        public override object? VisitIdentifierExpression([NotNull] yyzParser.IdentifierExpressionContext context)
        {
            var variableName = context.IDENTIFIER().GetText();
            if (!variables.ContainsKey(variableName))
            {
                throw new Exception("variable not exists");
            }

            var type = variables[variableName];

            if (type == "i32")
            {
                stack.Push((type, "%" + LLVMGenerator.loadInt(variableName)));
            }
            else if (type == "double")
            {
                stack.Push((type, "%" + LLVMGenerator.loadDouble(variableName)));
            }
            

            return null;
        }
        public override object? VisitBracketExpression([NotNull] yyzParser.BracketExpressionContext context)
        {
            Visit(context.expression());

            return null;
        }
        #region additiveOperations
        public override object? VisitAdditiveExpression([NotNull] yyzParser.AdditiveExpressionContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));

            var right = stack.Pop();
            var left = stack.Pop();
            
            var additiveOperator = context.additiveOperation().GetText();
            return additiveOperator switch
            {
                "+" => Add(left, right),
                "-" => Subtract(left, right),
                _ => throw new NotImplementedException("Unnown operator")
            };
        }

        private object? Add((string, string) left, (string, string) right)
        {
            if (left.Item1 == "i32" && right.Item1 == "i32")
            {
                LLVMGenerator.AddInteger(left.Item2, right.Item2, "i32");
                stack.Push(("i32", "%" + (LLVMGenerator.reg - 1)));
                return "i32";
            }
            else if (left.Item1 == "double" && right.Item1 == "double")
            {
                LLVMGenerator.AddDouble(left.Item2, right.Item2, "double");
                stack.Push(("double", "%" + (LLVMGenerator.reg - 1)));
                return "double";
            }
            throw new NotImplementedException("Cannot add values of type int and double");
        }

        private object? Subtract((string, string) left, (string, string) right)
        {
            if (left.Item1 == "i32" && right.Item1 == "i32")
            {
                LLVMGenerator.SubInteger(left.Item2, right.Item2);
                stack.Push(("i32", "%" + (LLVMGenerator.reg - 1)));
                return "i32";
            }
            else if (left.Item1 == "double" && right.Item1 == "double")
            {
                LLVMGenerator.SubDouble(left.Item2, right.Item2);
                stack.Push(("double", "%" + (LLVMGenerator.reg - 1)));
                return "double";
            }
            throw new NotImplementedException("Cannot subtract values of type int and double");
        }

        //private object? Subtract(object? left, object? right)
        //{
        //    if (left is null || right is null)
        //        throw new ArgumentNullException("Cannot preform subtraction if either of values is null.");
        //    if (left is double l && right is double r)
        //        return l - r;
        //    if (left is int li && right is int ri)
        //        return li - ri;
        //    if (left is double lDouble && right is int rInt)
        //        return lDouble - rInt;
        //    if (left is int lInt && right is double rDouble)
        //        return lInt - rDouble;
        //    throw new NotImplementedException($"Cannot subtract values of type {left.GetType()} and {right.GetType()}");
        //}
        #endregion
        #region multiplicatioveOperations
        public override object? VisitMultiplicativeExpression([NotNull] yyzParser.MultiplicativeExpressionContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));

            var right = stack.Pop();
            var left = stack.Pop();

            var multiplicativeOperator = context.multiplicativeOperation().GetText();

            return multiplicativeOperator switch
            {
                "*" => Multiply(left, right),
                "/" => Divide(left, right),
                _ => throw new NotImplementedException("Unnown operator")
            };
        }

        private object? Multiply((string, string) left, (string, string) right)
        {
            if (left.Item1 == "i32" && right.Item1 == "i32")
            {
                LLVMGenerator.MultInteger(left.Item2, right.Item2);
                stack.Push(("i32", "%" + (LLVMGenerator.reg - 1)));
                return "i32";
            }
            else if (left.Item1 == "double" && right.Item1 == "double")
            {
                LLVMGenerator.MultDouble(left.Item2, right.Item2);
                stack.Push(("double", "%" + (LLVMGenerator.reg - 1)));
                return "double";
            }
            throw new NotImplementedException("Cannot subtract values of type int and double");
        }

        private object? Divide((string, string) left, (string, string) right)
        {
            if (left.Item1 == "i32" && right.Item1 == "i32")
            {
                LLVMGenerator.DivideInteger(left.Item2, right.Item2);
                stack.Push(("i32", "%" + (LLVMGenerator.reg - 1)));
                return "i32";
            }
            else if (left.Item1 == "double" && right.Item1 == "double")
            {
                LLVMGenerator.DivideDouble(left.Item2, right.Item2);
                stack.Push(("double", "%" + (LLVMGenerator.reg - 1)));
                return "double";
            }
            throw new NotImplementedException("Cannot subtract values of type int and double");
        }
        #endregion
        #region compareOperations
        //public override object VisitCompareExpression([NotNull] yyzParser.CompareExpressionContext context)
        //{
        //    var left = Visit(context.expression(0));
        //    var right = Visit(context.expression(1));
        //    var compareOperator = context.compareOperation().GetText();
        //    return compareOperator switch
        //    {
        //        "==" => EqualTo(left, right),
        //        "!=" => !EqualTo(left, right),
        //        ">" => GreaterThan(left, right),
        //        "<" => GreaterThan(right, left),
        //        ">=" => EqualTo(left, right) || GreaterThan(left, right),
        //        "<=" => EqualTo(left, right) || GreaterThan(right, left),
        //        _ => throw new NotImplementedException("Unnown operator")
        //    };
        //}

        //private bool EqualTo(object? left, object? right)
        //{
        //    if (left is null || right is null)
        //        throw new ArgumentNullException("Cannot preform comparison if either of values is null.");
        //    if (left is double l && right is double r)
        //        return l == r;
        //    if (left is int li && right is int ri)
        //        return li == ri;
        //    if (left is double lDouble && right is int rInt)
        //        return lDouble == rInt;
        //    if (left is int lInt && right is double rDouble)
        //        return lInt == rDouble;
        //    throw new NotImplementedException($"Cannot compare values of type {left.GetType()} and {right.GetType()}");
        //}

        //private bool GreaterThan(object? left, object? right)
        //{
        //    if (left is null || right is null)
        //        throw new ArgumentNullException("Cannot preform comparison if either of values is null.");
        //    if (left is double l && right is double r)
        //        return l > r;
        //    if (left is int li && right is int ri)
        //        return li > ri;
        //    if (left is double lDouble && right is int rInt)
        //        return lDouble > rInt;
        //    if (left is int lInt && right is double rDouble)
        //        return lInt > rDouble;
        //    throw new NotImplementedException($"Cannot compare values of type {left.GetType()} and {right.GetType()}");
        //}
        #endregion
        #region booleanOperations
        //public override object VisitBooleanExpression([NotNull] yyzParser.BooleanExpressionContext context)
        //{
        //    var booleanOperator = context.booleanOperation().GetText();
        //    var left = IsTrue(Visit(context.expression(0)));
        //    if(booleanOperator == "AND" && !left)
        //        return false;
        //    if(booleanOperator == "OR" && left)
        //        return true;
        //    var right = IsTrue(Visit(context.expression(1)));

        //    return booleanOperator switch
        //    {
        //        "AND" => And(left, right),
        //        "OR" => Or(left, right),
        //        "XOR" => Xor(left, right),
        //        _ => throw new NotImplementedException("Unnown operator")
        //    };
        //}

        //public override object VisitNegationExpression([NotNull] yyzParser.NegationExpressionContext context)
        //{
        //    var left = IsTrue(Visit(context.expression()));
        //    return Neg(left);
        //}

        //public bool And(bool left, bool right)
        //{
        //    return left && right;
        //}
        //public bool Or(bool left, bool right)
        //{
        //    return left || right;
        //}
        //public bool Xor(bool left, bool right)
        //{
        //    return left != right;
        //}

        //public bool Neg(bool left)
        //{
        //    return !left;
        //}
        //private bool IsTrue(object? value)
        //{
        //    if (value is bool b)
        //        return b;
        //    throw new NotImplementedException($"Cannot preform bool operations on type {value?.GetType().ToString() ?? "null"}");
        //}
        #endregion
        #region block
        //public override object? VisitBlock([NotNull] yyzParser.BlockContext context)
        //{
        //    GetScopeService().OpenScope();
        //    base.VisitBlock(context);
        //    GetScopeService().CloseScope();
        //    return null;
        //}

        //public override object? VisitIfBlock([NotNull] yyzParser.IfBlockContext context)
        //{
        //    if (IsTrue(Visit(context.expression())))
        //    {
        //        Visit(context.block());
        //    }
        //    return null;
        //}

        //public override object? VisitWhileBlock([NotNull] yyzParser.WhileBlockContext context)
        //{
        //    while (IsTrue(Visit(context.expression())))
        //    {
        //        Visit(context.block());
        //    }
        //    return null;
        //}
        #endregion
        #region functions
        //public override object? VisitFunctionDefinition([NotNull] yyzParser.FunctionDefinitionContext context)
        //{
        //    var identifiersCount = context.IDENTIFIER().Count();
        //    List<string> parameters = new List<string>();
        //    for(int i = 1; i < identifiersCount; i++)
        //    {
        //        parameters.Add(context.IDENTIFIER(i).GetText());
        //    }
        //    var function = new Function
        //    {
        //        Identifier = context.IDENTIFIER(0).GetText(),
        //        Parameters = parameters,
        //        FunctionBody = context.functionBody()
        //    };
        //    scopeService.SetFunction(function);
        //    return null;
        //}

        //public override object? VisitFunctionCall([NotNull] yyzParser.FunctionCallContext context)
        //{
        //    var parametersCount = context.expression().Count();
        //    var identifier = context.IDENTIFIER().GetText();
        //    var function = scopeService.GetFunction(identifier, parametersCount);
        //    function.FunctionScopeService = new ScopeService(scopeService.GlobalScope);
        //    for(int i = 0; i < parametersCount; i++)
        //    {
        //        var parameterName = function.Parameters[i];
        //        var parameterValue = Visit(context.expression(i));
        //        function.FunctionScopeService.SetVariable(parameterName, parameterValue);
        //    }
        //    functionStack.Push(function);
        //    var result = Visit(function.FunctionBody);
        //    functionStack.Pop();
        //    return result;
        //}

        public override object? VisitProgram([NotNull] yyzParser.ProgramContext context)
        {
            base.VisitProgram(context);

            LLVMGenerator.CloseMain();

            return null;
        }

        //public override object? VisitFunctionBody([NotNull] yyzParser.FunctionBodyContext context)
        //{
        //    foreach(var line in context.line())
        //    {
        //        Visit(line);
        //    }
        //    if(context.RETURN() != null)
        //    {
        //        return Visit(context.expression());
        //    }

        //    return null;
        //}
        #endregion
        #region IO
        public override object? VisitPrint([NotNull] yyzParser.PrintContext context)
        {
            var id = context.IDENTIFIER().GetText();

            if (!variables.ContainsKey(id))
            {
                throw new Exception("Variable not initialized");
            }
            var type = variables[id];

            if (type == "i32")
            {
                LLVMGenerator.PrintInteger(id);
            }
            else if (type == "double")
            {
                LLVMGenerator.PrintDouble(id);
            }

            return null;
        }

        public override object? VisitRead([NotNull] yyzParser.ReadContext context)
        {

            var id = context.IDENTIFIER().GetText();
            if (!variables.ContainsKey(id))
            {
                throw new Exception("Variable not initialized");
            }

            var type = variables[id];

            if (type == "i32")
            {
                LLVMGenerator.ScanInteger(id);
            }
            else if (type == "double")
            {
                LLVMGenerator.ScanDouble(id);
            }

            return null;
        }
        #endregion
    }
}
