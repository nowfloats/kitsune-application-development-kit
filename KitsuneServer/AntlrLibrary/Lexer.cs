using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using AntlrLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntlrLibrary
{
    public class LexerGenerator
    {
        public static Node Parse(string exp)
        {
            AntlrInputStream input = new AntlrInputStream(exp);
            KitsuneGrammerLexer lexer1 = new KitsuneGrammerLexer(input);
            //lexer1.RemoveErrorListener(ConsoleErrorListener<int>.Instance);
            
            CommonTokenStream tokens = new CommonTokenStream(lexer1);
            KitsuneGrammerParser parser = new KitsuneGrammerParser(tokens);
            parser.RemoveErrorListeners();
            IParseTree tree = parser.prog();
            //ParseTreeWalker sd = new ParseTreeWalker();

            //Create Parse Tree from tree object
            //Console.WriteLine(tree.ToStringTree(parser));


            KitsuneParser visitor = new KitsuneParser();
            Node result = visitor.Visit(tree);

            return result;
        }
    }
}


