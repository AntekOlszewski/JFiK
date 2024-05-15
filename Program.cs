using Antlr4.Runtime;
using JFiK;
using JFiK.Content;

var fileName = "Content\\test.yyz";
var fileContents = File.ReadAllText(fileName);

var inputStream = new AntlrInputStream(fileContents);
var yyzLexer = new yyzLexer(inputStream);
var commonTokenStream = new CommonTokenStream(yyzLexer);
var yyzParser = new yyzParser(commonTokenStream);
var yyzContext = yyzParser.program();
var visitor = new yyzVisitor();

visitor.Visit(yyzContext);