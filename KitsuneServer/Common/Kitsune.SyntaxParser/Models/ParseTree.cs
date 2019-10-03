using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AntlrLibrary.Model;

namespace Kitsune.SyntaxParser.Models
{
    public class ParseTress
    {
        public static Token Parse(Node node)
        {
            try
            {
                if (node.Token.Type == TOKENTYPE.Expression)
                {
                    //operand
                    if (node.Token.Value == ACTIONS.OperandEval)
                        node.Token = Evaluator.OperandEval(node.Children[0].Token);
                    //negative number (-1)
                    else if (node.Token.Value == ACTIONS.NegativeNumberEval)
                    {
                        node.Children[1].Token = Parse(node.Children[1]);
                        if (node.Children[1].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[1].Token;
                        node.Token = Evaluator.NegativeNumberEval(node.Children[0].Token, node.Children[1].Token);
                    }
                    //ex: .Length  |  .IsNullOrEmpty
                    else if (node.Token.Value == ACTIONS.PostfixUnaryEval)
                    {
                        node.Children[0].Token = Parse(node.Children[0]);
                        if (node.Children[0].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[0].Token;
                        node.Token = Evaluator.PostfixUnaryEval(node.Children[0].Token, node.Children[1].Token);
                    }
                    //ex: .GetValueAt(int)
                    else if (node.Token.Value == ACTIONS.PostfixUnaryWithArgEval)
                    {
                        node.Children[0].Token = Parse(node.Children[0]);
                        if (node.Children[0].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[0].Token;

                        List<Token> parametersToken = new List<Token>();
                        for (var index = 3; index < node.Children.Count - 1; index++)
                        {
                            if(node.Children[index] != null)
                            {
                                if (node.Children[index].Token.Type == TOKENTYPE.Delimiter)
                                    continue;
                                node.Children[index].Token = Parse(node.Children[index]);
                                if (node.Children[index].Token.Type == TOKENTYPE.ERROR)
                                    return node.Children[index].Token;
                                parametersToken.Add(node.Children[index].Token);
                            }
                            
                        }

                        node.Token = Evaluator.PostfixUnaryWithArgEval(node.Children[0].Token, node.Children[1].Token, parametersToken);
                    }
                    // (!)
                    else if (node.Token.Value == ACTIONS.PrefixUnaryEval)
                    {
                        node.Children[1].Token = Parse(node.Children[1]);
                        if (node.Children[1].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[1].Token;
                        node.Token = Evaluator.PrefixUnaryEval(node.Children[0].Token, node.Children[1].Token);
                    }
                    //( + - / * )
                    else if (node.Token.Value == ACTIONS.Arithmatic)
                    {
                        node.Children[0].Token = Parse(node.Children[0]);
                        if (node.Children[0].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[0].Token;
                        node.Children[2].Token = Parse(node.Children[2]);
                        if (node.Children[2].Token.Type == TOKENTYPE.ERROR)
                            return node.Children[2].Token;
                        node.Token = Evaluator.Arithmetic(node.Children[0].Token, node.Children[1].Token, node.Children[2].Token);
                    }
                    //[ exp , exp , exp ]
                    else if (node.Token.Value == ACTIONS.ArrayEval)
                    {
                        List<dynamic> temp = new List<dynamic>();
                        for (int i = 1; i < node.Children.Count - 1; i++)
                        {
                            if (node.Children[i].Token.Type == TOKENTYPE.Expression)
                            {
                                var result = Parse(node.Children[i]);
                                if (result.Type == TOKENTYPE.ERROR)
                                    return result;
                                temp.Add(result);

                            }
                        }
                        node.Token = new Token { Value = temp, Type = TOKENTYPE.Array };
                    }
                    // exp ? exp : exp 
                    else if (node.Token.Value == ACTIONS.Ternary)
                    {
                        node.Children[0].Token = Parse(node.Children[0]);
                        if (node.Children[0].Token.Type == TOKENTYPE.Boolean)
                        {
                            if (node.Children[0].Token.Value == true)
                                node.Token = Parse(node.Children[2]);
                            else
                                node.Token = Parse(node.Children[4]);

                            if (node.Token.Type == TOKENTYPE.ERROR)
                                return node.Token;
                        }
                        else
                        {
                            node.Token.Type = TOKENTYPE.ERROR;
                            node.Token.Value = "Condition must return boolean";
                            return node.Children[0].Token.Type == TOKENTYPE.ERROR ? node.Children[0].Token : node.Token;
                        }
                    }
                    else
                    {
                        throw new Exception("No matching Action found for the expression.");
                    }
                }
                else if (node.Token.Type == TOKENTYPE.ERROR)
                {
                    throw new Exception(node.Token.Value);
                }
                else
                {
                    throw new Exception("Expression is not declared.");
                }
                return node.Token;
            }
            catch (Exception ex)
            {
                return new Token { Value = ex.ToString(), Type = TOKENTYPE.ERROR };
            }
        }
    }
}
