using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static JFiK.Content.yyzParser;

namespace JFiK
{
    public class ScopeService
    {
        public Scope GlobalScope { get; set; }
        public Scope CurrentScope { get; set; }
        public ScopeService()
        {
            GlobalScope = new Scope(null);
            CurrentScope = new Scope(GlobalScope);
        }
        public ScopeService(Scope globalScope)
        {
            GlobalScope = globalScope;
            CurrentScope = new Scope(GlobalScope);
        }

        public object GetVariable(string identifier)
        {
            return CurrentScope.GetVariableValue(identifier);
        }

        public void SetVariable(string identifier, object? value, bool isConstant = false, bool isTyped = false, bool isGlobal = false)
        {
            if (!isGlobal)
                CurrentScope.SetVariable(identifier, value, isConstant, isTyped);
            else
                GlobalScope.SetVariable(identifier, value, isConstant, isTyped);
        }

        public Function GetFunction(string identifier, int parametersCount)
        {
            if (GlobalScope.Functions.ContainsKey((identifier, parametersCount)))
                return GlobalScope.Functions[(identifier, parametersCount)];
            throw new Exception($"Function {identifier} with {parametersCount} parameters is not declared.");
        }

        public void SetFunction(Function function)
        {
            if (GlobalScope.Functions.ContainsKey((function.Identifier, function.ParametersCount)))
                throw new Exception($"Function {function.Identifier} with {function.ParametersCount} parameters is already declared.");
            GlobalScope.Functions[(function.Identifier, function.ParametersCount)] = function;
        }

        public void OpenScope()
        {
            var scope = new Scope(CurrentScope);
            CurrentScope = scope;
        }
        public void CloseScope()
        {
            CurrentScope = CurrentScope.Parent;
        }
    }
    public class Scope
    {
        public Scope? Parent { get; set; }
        public Dictionary<string, Variable> Variables { get; set; } = new Dictionary<string, Variable>();
        public Dictionary<(string, int), Function> Functions { get; set; } = new Dictionary<(string, int), Function> ();
        public Scope(Scope? parent)
        {
            this.Parent = parent;
        }
        public object? GetVariableValue(string identifier)
        {
            if(Variables.ContainsKey(identifier))
                return GetVariable(identifier).Value;
            if(Parent != null)
                return Parent.GetVariableValue(identifier);
             throw new Exception($"Variable {identifier} is not declared.");
        }
        private Variable? GetVariable(string identifier)
        {
            if (Variables.ContainsKey(identifier))
                return Variables[identifier];
            if (Parent != null)
                return Parent.GetVariable(identifier);
            return null;
        }
        public void SetVariable(string identifier, object? value, bool isConstant, bool isTyped)
        {
            Variable? variable = GetVariable(identifier);
            if (variable == null)
            {
                Variables[identifier] = new Variable
                {
                    Identifier = identifier,
                    Value = value,
                    IsConstant = isConstant,
                    IsTyped = isTyped,
                };
                return;
            }
            if (variable.IsConstant)
            {
                throw new Exception($"Cannot assign value to {identifier} constant");
            }
            if (variable.IsTyped && (value?.GetType() != variable.Value?.GetType()))
            {
                throw new Exception($"Cannot assign diffenrent type to typed variable");
            }
            variable.Value = value;
            variable.IsConstant = isConstant;
            variable.IsTyped = isTyped;
            return;
        }
    }

    public class Variable
    {
        public string Identifier { get; set; }
        public object? Value { get; set; }
        public bool IsConstant { get; set; }
        public bool IsTyped { get; set; }
    }

    public class Function
    {
        public string Identifier { get; set; }
        public List<string> Parameters { get; set; } = new List<string>();
        public int ParametersCount => Parameters.Count;
        public FunctionBodyContext FunctionBody { get; set; }
        public ScopeService FunctionScopeService { get; set; }
    }
}
