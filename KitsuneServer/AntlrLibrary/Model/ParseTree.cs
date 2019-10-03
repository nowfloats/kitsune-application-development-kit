using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntlrLibrary.Model
{
    public enum ACTIONS { OperandEval, NegativeNumberEval, PostfixUnaryEval, PostfixUnaryWithArgEval, PrefixUnaryEval, Arithmatic, ConjunctionalEval, ArrayEval, Ternary, Loop, InLoop, View, ViewProperty, ViewFunction, KObject }
    public class Node
    {
        public Node()
        {
            Children = new List<Node>();
            Token = new Token();
        }
        public Node(dynamic tokenValue, TOKENTYPE? tokenType)
        {
            Children = new List<Node>();
            Token = new Token(tokenValue, tokenType); 
        }
        public Token Token { get; set; }
        public List<Node> Children { get; set; }
    }
}
