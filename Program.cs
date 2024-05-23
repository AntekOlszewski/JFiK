using Antlr4.Runtime;
using JFiK;
using JFiK.Content;
using LLVMSharp.Interop;

var fileName = "Content\\test2.yyz";
var fileContents = File.ReadAllText(fileName);

var inputStream = new AntlrInputStream(fileContents);
var yyzLexer = new yyzLexer(inputStream);
var commonTokenStream = new CommonTokenStream(yyzLexer);
var yyzParser = new yyzParser(commonTokenStream);
var yyzContext = yyzParser.program();
var visitor = new yyzVisitor();

try
{
    visitor.Visit(yyzContext);
    LLVMGenerator.generateLLVMFile();
}
catch(Exception e)
{
    Console.WriteLine(e.Message);
}

