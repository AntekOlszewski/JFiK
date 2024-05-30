using Antlr4.Runtime.Misc;
using JFiK.Content;
using LLVMSharp;
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
        Stack<(string, string)> stack = new Stack<(string, string)>();
        ScopeService ScopeService;

        public yyzVisitor()
        {
            ScopeService = new ScopeService();
            ScopeService.OpenScope();
        }


        public override object? VisitAssignment([NotNull] yyzParser.AssignmentContext context)
        {
            var isLet = context.LET() != null;

            if (isLet)
            {
                var variableName = context.IDENTIFIER().GetText();
                Visit(context.expression());

                var isGlobal = context.GLOBAL() != null;

                var value = stack.Pop();

                ScopeService.SetVariable(variableName, value.Item1, isGlobal);

                LLVMGenerator.Declare(variableName, value.Item1, isGlobal);
                LLVMGenerator.Assign(variableName, value.Item2, value.Item1, isGlobal);
            }
            else
            {
                var variableName = context.IDENTIFIER().GetText();
                Visit(context.expression());

                var value = stack.Pop();

                var isGlobal = ScopeService.GetVariable(variableName).IsGlobal;

                LLVMGenerator.Assign(variableName, value.Item2, value.Item1, isGlobal);
            }


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
            {
                var booleane = b.GetText() == "true" ? 1 : 0;
                stack.Push(("i1", booleane.ToString()));
                return booleane;
            }
            if (context.STRING() is { } s)
                return s.GetText()[1..^1];
            if (context.NULL() is { } n)
                return null;
            throw new NotImplementedException("Unnown data type");
        }

        public override object? VisitIdentifierExpression([NotNull] yyzParser.IdentifierExpressionContext context)
        {
            var variableName = context.IDENTIFIER().GetText();

            var variable = ScopeService.GetVariable(variableName);
            var type = variable.Type;
            var isGlobal = variable.IsGlobal;

            if (type == "i32")
            {
                stack.Push((type, isGlobal ? "@" : "%" + LLVMGenerator.loadInt(variableName)));
            }
            else if (type == "double")
            {
                stack.Push((type, isGlobal ? "@" : "%" + LLVMGenerator.loadDouble(variableName)));
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
        public override object? VisitCompareExpression([NotNull] yyzParser.CompareExpressionContext context)
        {
            Visit(context.expression(0));
            Visit(context.expression(1));

            string operation = context.compareOperation().COMPARE_OPERATOR().GetText();

            var left = stack.Pop();
            var right = stack.Pop();

            if (left.Item1 == right.Item1)
            {
                string operationText;
                switch (operation)
                {
                    case "==":
                        operationText = "eq";
                        break;
                    case "!=":
                        operationText = "ne";
                        break;
                    case ">":
                        operationText = "slt";
                        break;
                    case "<":
                        operationText = "sgt";
                        break;
                    case "<=":
                        operationText = "sge";
                        break;
                    case ">=":
                        operationText = "sle";
                        break;
                    default:
                        throw new NotImplementedException("Operation not found");
                };

                if (left.Item1 == "i32")
                {
                    LLVMGenerator.compareVariables(left.Item2, right.Item2, operationText, "i32");
                }
                else if (left.Item1 == "double")
                {
                    LLVMGenerator.compareVariables(left.Item2, right.Item2, operationText, "double");
                }
                else if (left.Item1 == "i1")
                {
                    LLVMGenerator.compareVariables(left.Item2, right.Item2, operationText, "i1");
                }
            }
            else
            {
                throw new NotImplementedException("Could not compare different types");
            }

            return null;
        }

        public override object VisitFunctionCall([NotNull] yyzParser.FunctionCallContext context)
        {
            var id = context.IDENTIFIER(0).GetText();
            var fun = ScopeService.GetFunction(id);

            if (fun.Parameters.Count != context.IDENTIFIER().Length - 1)
            {
                throw new Exception("expected " + (context.IDENTIFIER().Length - 1)  + "num of parameters");
            }

            

            var last = false;

            List<(string, string)> arguments = new List<(string, string)>();

            for (var i = 1; i < context.IDENTIFIER().Length; i++)
            {
                var param = fun.Parameters[i - 1];
                var value = ScopeService.GetVariable(context.IDENTIFIER(i).GetText());
                if (value.Type != param.Type)
                {
                    throw new Exception("parameter type not good");
                }
                arguments.Add(((value.IsGlobal ? "@" : "%") + context.IDENTIFIER(i).GetText(), value.Type));
                
            }


            var napale = LLVMGenerator.call(id, fun.Type);

            if (stack.Count > 0)
            {
                var type = stack.Pop().Item1;
                stack.Push((type, "%" + napale));
            }
            else
            {
                stack.Push((fun.Type, "%" + napale));
            }
            


            last = false;
            
            for (var i = 0; i < arguments.Count; i++)
            {
                last = i + 1 == context.IDENTIFIER().Length - 1;
                LLVMGenerator.callparams(arguments[i].Item1, arguments[i].Item2, last);
            }
            
            return null;
        }

        public override object? VisitFunctionDefinition([NotNull] yyzParser.FunctionDefinitionContext context)
        {
            string id = context.IDENTIFIER(0).GetText();
            string type = context.TYPE(0).GetText();
            if (type == "int")
            {
                type = "i32";
            }
            else if (type == "double")
            {
                type = "double";
            }
            else
            {
                throw new Exception("unsupported return parameter");
            }

            List<FunctionParameter> args = new List<FunctionParameter>();
            foreach (var item in context.IDENTIFIER().Skip(1).Select((value, i) => (value, i)))
            {
                var newType = context.TYPE(item.i).GetText() == "int" ? "i32" : "double";
                args.Add(new FunctionParameter { Identifier = item.value.GetText(), Type = newType});
            }

            ScopeService.SetFunction(new Function { Identifier = id, Type = type, Parameters = args });
            LLVMGenerator.functionStart(id, type);
            LLVMGenerator.functionParams(args);

            ScopeService.OpenScope();
            args.ForEach(item =>
            {
                ScopeService.SetVariable(item.Identifier, item.Type);
            });

            Visit(context.functionBody());

            LLVMGenerator.functionEnd(type);

            ScopeService.CloseScope();


            return null;
        }

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
        public override object? VisitBlock([NotNull] yyzParser.BlockContext context)
        {
            ScopeService.OpenScope();
            var lines = context.line();
            base.VisitBlock(context);
            //foreach (var l in lines)
            //{
            //    Visit(l);
            //}
            ScopeService.CloseScope();
            return null;
        }

        public override object? VisitWhileBlock([NotNull] yyzParser.WhileBlockContext context)
        {
            LLVMGenerator.beginWhile();
            Visit(context.expression());
            LLVMGenerator.whileBlock();
            Visit(context.block());
            LLVMGenerator.whileEnd();
            return null;
        }


        public override object? VisitIfStatement([NotNull] yyzParser.IfStatementContext context)
        {
            Visit(context.expression());
            LLVMGenerator.beginIfStatement();
            Visit(context.block());
            LLVMGenerator.beginElseStatement();
            LLVMGenerator.endElse();
            return null;
        }


        //public override object? VisitIfStatement([NotNull] yyzParser.IfBlockContext context)
        //{
            
        //    isGlobal = false;
            


            
            
        //}

        //public override object? VisitWhileBlock([NotNull] yyzParser.WhileBlockContext context)
        //{
        //    //while (IsTrue(Visit(context.expression())))
        //    //{
        //    //    Visit(context.block());
        //    //}
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

            var variable = ScopeService.GetVariable(id);
            var type = variable.Type;
            var isGlobal = variable.IsGlobal;

            if (type == "i32")
            {
                LLVMGenerator.PrintInteger((isGlobal ? "@" : "%") + id);
            }
            else if (type == "double")
            {
                LLVMGenerator.PrintDouble((isGlobal ? "@" : "%") + id);
            }
            else if (type == "i1")
            {
                LLVMGenerator.PrintBoolean((isGlobal ? "@" : "%") + id);
            }

            return null;
        }

        public override object? VisitRead([NotNull] yyzParser.ReadContext context)
        {

            var id = context.IDENTIFIER().GetText();

            var variable = ScopeService.GetVariable(id);
            var type = variable.Type;
            var isGlobal = variable.IsGlobal;

            if (type == "i32")
            {
                LLVMGenerator.ScanInteger((isGlobal ? "@" : "%") + id);
            }
            else if (type == "double")
            {
                LLVMGenerator.ScanDouble((isGlobal ? "@" : "%") + id);
            }
            else if (type == "i1")
            {
                LLVMGenerator.ScanBoolean((isGlobal ? "@" : "%") + id);
            }

            return null;
        }
        #endregion

    }
}
