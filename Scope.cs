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

        public Variable GetVariable(string identifier)
        {
            var variable = CurrentScope.GetVariable(identifier);
            if (variable == null)
            {
                throw new Exception($"Variable {identifier} is not declared.");
            }
            return variable;
        }

        public void SetVariable(string identifier, string value, bool isGlobal = false)
        {
            if (!isGlobal)
                CurrentScope.SetVariable(identifier, value, isGlobal);
            else
                GlobalScope.SetVariable(identifier, value, isGlobal);
        }

        public Function GetFunction(string identifier)
        {
            if (GlobalScope.Functions.ContainsKey(identifier))
                return GlobalScope.Functions[identifier];
            throw new Exception($"Function {identifier} is not declared.");
        }

        public void SetFunction(Function function)
        {
            if (GlobalScope.Functions.ContainsKey(function.Identifier))
                throw new Exception($"Function {function.Identifier} is already declared.");
            GlobalScope.Functions[function.Identifier] = function;
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
        public Dictionary<string, Function> Functions { get; set; } = new Dictionary<string, Function>();
        public Scope(Scope? parent)
        {
            this.Parent = parent;
        }
        public Variable GetVariable(string identifier)
        {
            if (Variables.ContainsKey(identifier))
                return Variables[identifier];
            if (Parent != null)
                return Parent.GetVariable(identifier);
            return null;
        }
        public void SetVariable(string identifier, string value, bool isGlobal)
        {
            if (GetVariable(identifier) != null)
            {
                throw new Exception($"Variable already exists");
            }
            Variables[identifier] = new Variable{Type = value, IsGlobal = isGlobal};
        }


    }

    public class Variable
    {
        public string Type { get; set; }
        public bool IsGlobal { get; set; }
    }

    public class Function
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        //public int ParametersCount => Parameters.Count;
        //public FunctionBodyContext FunctionBody { get; set; }
        //public ScopeService FunctionScopeService { get; set; }
    }

    public class FunctionParameter
    {
        public string Identifier { get; set; }
        public string Type { get; set; }
    }
}
