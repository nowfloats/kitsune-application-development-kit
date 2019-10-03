using AntlrLibrary;
using AntlrLibrary.Model;
using Kitsune.SyntaxParser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.SyntaxParser
{
    public class Parser
    {
        public static object Execute(string expression)
        {
            //if(expression.Contains("[") || expression.Contains("]"))
            //{
            //    expression = expression.Replace("[", "").Replace("]","");
            //}
            Node tree = LexerGenerator.Parse(expression);
            var result = ParseTress.Parse(tree);
            return result.Value;
        }
        public static List<string> GetObjects(string expression)
        {
            List<string> objects = new List<string>();
            Node result = LexerGenerator.Parse(expression);
            SaveObject(objects, result);
            return objects;
        }

        public static void SaveObject(List<string> list, Node token)
        {
            try
            {
                if (token.Children.Count == 0)
                    return;
                if (token.Token.Value == ACTIONS.OperandEval)
                {
                    var s = Evaluator.OperandEval(token.Children[0].Token);
                    if (s.Type == TOKENTYPE.Object)
                        list.Add(token.Children[0].Token.Value);
                }
                if (token.Children.Count != 0)
                    foreach (var child in token.Children)
                    {
                        if(child != null)
                        {
                            //SaveObject(list, child);
                            SaveObjectToList(list, child);
                        }
                        
                    }
            }
            catch (Exception ex)
            {

            }
        }

        private static void SaveObjectToList(List<string> list, Node token)
        {
            try
            {
                if (token.Children.Count == 0 && token.Token.Type == null)
                    return;
                else if (token.Children.Count == 0 && token.Token.Type == TOKENTYPE.Function)
                {
                    if (list != null)
                    {
                        if (token.Token.Value == "length")
                        {
                            var element = list.ElementAt(list.Count - 1);
                            element = element + "." + token.Token.Value;
                            list.RemoveAt(list.Count - 1);
                            list.Add(element + "()");
                        }

                    }
                    return;
                }
                

                if (token.Token.Value == ACTIONS.OperandEval)
                {
                    var s = Evaluator.OperandEval(token.Children[0].Token);
                    if (s.Type == TOKENTYPE.Object)
                        list.Add(token.Children[0].Token.Value);
                }
                if (token.Children.Count != 0)
                    foreach (var child in token.Children)
                    {
                        if (child != null)
                        {
                            ObjectList(list, child);
                        }

                    }
            }
            catch (Exception ex)
            {

            }
        }

        private static void ObjectList(List<string> list, Node token)
        {
            try
            {
                if (token.Children.Count == 0 && token.Token.Type == null)
                    return;
                else if (token.Children.Count == 0 && token.Token.Type == TOKENTYPE.Function)
                {
                    if(list != null)
                    {
                        if(token.Token.Value == "length")
                        {
                            var element = list.ElementAt(list.Count - 1);
                            element = element + "." + token.Token.Value;
                            list.RemoveAt(list.Count - 1);
                            list.Add(element + "()");
                        }
                        
                    }
                    return;
                }
                    
                if (token.Token.Value == ACTIONS.OperandEval)
                {
                    var s = Evaluator.OperandEval(token.Children[0].Token);
                    if (s.Type == TOKENTYPE.Object)
                        list.Add(token.Children[0].Token.Value);
                }
                if (token.Children.Count != 0)
                    foreach (var child in token.Children)
                    {
                        if (child != null)
                        {
                            ObjectList(list, child);
                        }

                    }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
