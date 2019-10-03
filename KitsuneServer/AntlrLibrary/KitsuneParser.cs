using Antlr4.Runtime.Misc;
using AntlrLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntlrLibrary
{
    class KitsuneParser : KitsuneGrammerBaseVisitor<Node>
    {
        public override Node VisitAddSub([NotNull] KitsuneGrammerParser.AddSubContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.Arithmatic;
            Node left = Visit(context.expr(0));
            Node right = Visit(context.expr(1));
            result.Children.Add(left);
            result.Children.Add(new Node(context.op.Text, TOKENTYPE.Arithmatic));
            result.Children.Add(right);
            return result;
        }

        public override Node VisitMulDiv([NotNull] KitsuneGrammerParser.MulDivContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.Arithmatic;
            Node left = Visit(context.expr(0));
            Node right = Visit(context.expr(1));
            result.Children.Add(left);
            result.Children.Add(new Node(context.op.Text, TOKENTYPE.Arithmatic));
            result.Children.Add(right);
            return result;
        }
        public override Node VisitConditional([NotNull] KitsuneGrammerParser.ConditionalContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.Arithmatic;
            Node left = Visit(context.expr(0));
            Node right = Visit(context.expr(1));
            result.Children.Add(left);
            result.Children.Add(new Node(context.op.Text, TOKENTYPE.Arithmatic));
            result.Children.Add(right);
            return result;
        }
        public override Node VisitConjunctional([NotNull] KitsuneGrammerParser.ConjunctionalContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.Arithmatic;
            Node left = Visit(context.expr(0));
            Node right = Visit(context.expr(1));
            result.Children.Add(left);
            result.Children.Add(new Node(context.op.Text, TOKENTYPE.Arithmatic));
            result.Children.Add(right);
            return result;
        }
        public override Node VisitOperand([NotNull] KitsuneGrammerParser.OperandContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.OperandEval;
            Node child = new Node(context.GetText(), null);
            result.Children.Add(child);
            return result;
        }
        public override Node VisitNegativeNumber([NotNull] KitsuneGrammerParser.NegativeNumberContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.NegativeNumberEval;
            Node negativeSign = new Node("-", TOKENTYPE.Arithmatic);
            Node negativeValue = Visit(context.children[1]);

            result.Children.Add(negativeSign);
            result.Children.Add(negativeValue);
            return result;
        }
        public override Node VisitPrefixUnary([NotNull] KitsuneGrammerParser.PrefixUnaryContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.PrefixUnaryEval;
            Node unarySign = new Node();
            unarySign.Token.Type = TOKENTYPE.Arithmatic;
            unarySign.Token.Value = context.children[0].GetText();
            Node unaryValue = Visit(context.children[1]);

            result.Children.Add(unarySign);
            result.Children.Add(unaryValue);
            return result;
        }
        //public override Node VisitPostfixUnary([NotNull] KitsuneGrammerParser.PostfixUnaryContext context)
        //{
        //    Node result = new Node();
        //    result.Token.Type = TOKENTYPE.Expression;
        //    result.Token.Value = ACTIONS.PostfixUnaryEval;
        //    Node unarySign = new Node();
        //    unarySign.Token.Type = TOKENTYPE.Arithmatic;
        //    unarySign.Token.Value = context.children[1].GetText();
        //    Node unaryValue = Visit(context.children[0]);

        //    result.Children.Add(unaryValue);
        //    result.Children.Add(unarySign);
        //    return result;
        //}
        public override Node VisitPostfixUnaryWithArg([NotNull] KitsuneGrammerParser.PostfixUnaryWithArgContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.PostfixUnaryWithArgEval;

            //get the variable node
            Node variable = Visit(context.expr(0));
            Node functionVariable = new Node(context.op.Text.ToString().ToLower(), TOKENTYPE.Function);

            //get the function name
            Node unarySign = new Node(context.children[1].GetText(), TOKENTYPE.Arithmatic);

            Node openParen = new Node("(", TOKENTYPE.Delimiter);
            Node closeParen = new Node(")", TOKENTYPE.Delimiter);
            Node commaNode = new Node(",", TOKENTYPE.Delimiter);


            result.Children.Add(variable);
            result.Children.Add(functionVariable);
            result.Children.Add(openParen);

            List<KitsuneGrammerParser.ExprContext> exprs = context.expr().ToList();
            if (exprs.Count() == 0)
            {
                //result.Children.Add(closeBrace);
            }
            else
            {
                for (int exprIndex = 1; exprIndex < exprs.Count - 1; exprIndex++)
                {
                    result.Children.Add(Visit(exprs[exprIndex]));
                    result.Children.Add(commaNode);
                }
                result.Children.Add(Visit(exprs.Last()));
            }

            result.Children.Add(closeParen);

            return result;

        }
        public override Node VisitPrefixUnaryFunction([NotNull] KitsuneGrammerParser.PrefixUnaryFunctionContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.PostfixUnaryWithArgEval;

            //get the variable node
            Node variable = Visit(context.expr(0));
            string functionString = context.op.Text.ToString().ToLower();
            if (functionString == "len")
            {
                functionString = "length";
            }
            Node functionVariable = new Node(functionString, TOKENTYPE.Function);

            //get the function name
            Node unarySign = new Node(context.children[1].GetText(), TOKENTYPE.Arithmatic);

            Node openParen = new Node("(", TOKENTYPE.Delimiter);
            Node closeParen = new Node(")", TOKENTYPE.Delimiter);
            Node commaNode = new Node(",", TOKENTYPE.Delimiter);


            result.Children.Add(variable);
            result.Children.Add(functionVariable);
            result.Children.Add(openParen);

            List<KitsuneGrammerParser.ExprContext> exprs = context.expr().ToList();
            if (exprs.Count() == 0)
            {
                //result.Children.Add(closeBrace);
            }
            else
            {
                for (int exprIndex = 1; exprIndex < exprs.Count - 1; exprIndex++)
                {
                    result.Children.Add(Visit(exprs[exprIndex]));
                    result.Children.Add(commaNode);
                }
                result.Children.Add(Visit(exprs.Last()));
            }

            result.Children.Add(closeParen);

            return result;

        }
        public override Node VisitArray([NotNull] KitsuneGrammerParser.ArrayContext context)
        {
            Node result = new Node(ACTIONS.ArrayEval, TOKENTYPE.Expression);

            Node openBrace = new Node("[", TOKENTYPE.Delimiter);
            Node closeBrace = new Node("]", TOKENTYPE.Delimiter);
            Node commaNode = new Node(",", TOKENTYPE.Delimiter);
            result.Children.Add(openBrace);

            List<KitsuneGrammerParser.ExprContext> exprs = context.expr().ToList();
            if (exprs.Count() == 0)
            {
                //result.Children.Add(closeBrace);
            }
            else
            {
                for (int exprIndex = 0; exprIndex < exprs.Count - 1; exprIndex++)
                {
                    result.Children.Add(Visit(exprs[exprIndex]));
                    result.Children.Add(commaNode);
                }
                result.Children.Add(Visit(exprs.Last()));
            }
            result.Children.Add(closeBrace);
            return result;

        }
        public override Node VisitParens([NotNull] KitsuneGrammerParser.ParensContext context)
        {
            return Visit(context.expr());
        }
        public override Node VisitTernary([NotNull] KitsuneGrammerParser.TernaryContext context)
        {
            try
            {
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.Ternary;
                Node left = Visit(context.expr(0));
                Node middle = Visit(context.expr(1));
                Node right = Visit(context.expr(2));
                result.Children.Add(left);
                result.Children.Add(new Node("?", TOKENTYPE.Ternary));
                result.Children.Add(middle);
                result.Children.Add(new Node(":", TOKENTYPE.Ternary));
                result.Children.Add(right);
                return result;
            }
            catch
            {
                throw new Exception("Invalid ternary operator");
            }

        }
        public override Node VisitForLoop([NotNull] KitsuneGrammerParser.ForLoopContext context)
        {
            try
            {
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.Loop;
                Node obj = Visit(context.expr(0));
                Node iterator = Visit(context.expr(1));
                Node start = Visit(context.expr(2));
                Node offset = Visit(context.expr(3));

                result.Children.Add(obj);
                result.Children.Add(new Node(",", TOKENTYPE.Delimiter));
                result.Children.Add(iterator);
                result.Children.Add(new Node(":", TOKENTYPE.Delimiter));
                result.Children.Add(start);
                result.Children.Add(new Node(":", TOKENTYPE.Delimiter));
                result.Children.Add(offset);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid repeat operator");
            }
        }
        public override Node VisitForEachLoop([NotNull] KitsuneGrammerParser.ForEachLoopContext context)
        {
            try
            {
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.InLoop;
                Node obj = Visit(context.expr(0));
                Node targetObject = Visit(context.expr(1));
                result.Children.Add(obj);
                result.Children.Add(new Node("in", TOKENTYPE.Delimiter));
                result.Children.Add(targetObject);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid repeat operator");
            }
        }
        public override Node VisitKObject([NotNull] KitsuneGrammerParser.KObjectContext context)
        {
            try
            {
                var kobjects = (((context.ChildCount - 2) / 2) + 1);
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.KObject;
                var tokenValue = context.expr(0).GetText();
                for(int i = 1; i<kobjects; i++)
                {
                    tokenValue += "," + context.expr(i).GetText();
                }
                Node obj = new Node();
                obj.Token.Type = TOKENTYPE.Object;
                obj.Token.Value = tokenValue;


                Node targetObject = Visit(context.expr(kobjects));
                result.Children.Add(obj);
                result.Children.Add(new Node("in", TOKENTYPE.Delimiter));
                result.Children.Add(targetObject);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("Invalid repeat operator");
            }
        }
        public override Node VisitViewHandle([NotNull] KitsuneGrammerParser.ViewHandleContext context)
        {
            try
            {
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.View;
                Node viewName = Visit(context.expr());
                result.Children.Add(viewName);
                return result;
            }
            catch
            {
                throw new Exception("Invalid view operator");
            }
        }

        public override Node VisitTransformedViewHandle([NotNull] KitsuneGrammerParser.TransformedViewHandleContext context)
        {
            try
            {
                Node result = new Node();
                result.Token.Type = TOKENTYPE.Expression;
                result.Token.Value = ACTIONS.View;
                Node viewName = Visit(context.OPERAND());
                result.Children.Add(viewName);
                return result;
            }
            catch
            {
                throw new Exception("Invalid view operator");
            }
        }

        public override Node VisitViewProperties([NotNull] KitsuneGrammerParser.ViewPropertiesContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.ViewProperty;

            Node property = new Node(context.op.Text.ToString().ToLower(), TOKENTYPE.Expression);
            Node viewName = Visit(context.expr());
            result.Children.Add(viewName);
            result.Children.Add(property);
            return result;

        }

        public override Node VisitPostfixUnaryView([NotNull] KitsuneGrammerParser.PostfixUnaryViewContext context)
        {
            Node result = new Node();
            result.Token.Type = TOKENTYPE.Expression;
            result.Token.Value = ACTIONS.ViewFunction;

            //get the variable node
            Node functionVariable = new Node(context.op.Text.ToString().ToLower(), TOKENTYPE.Function);

            Node viewName = Visit(context.expr(0));
            result.Children.Add(viewName);
            Node openParen = new Node("(", TOKENTYPE.Delimiter);
            Node closeParen = new Node(")", TOKENTYPE.Delimiter);
            Node commaNode = new Node(",", TOKENTYPE.Delimiter);

            result.Children.Add(functionVariable);
            result.Children.Add(openParen);

            List<KitsuneGrammerParser.ExprContext> exprs = context.expr().ToList();
            if (exprs.Count() == 0)
            {
                //result.Children.Add(closeBrace);
            }
            else
            {
                for (int exprIndex = 1; exprIndex < exprs.Count - 1; exprIndex++)
                {
                    result.Children.Add(Visit(exprs[exprIndex]));
                    result.Children.Add(commaNode);
                }
                result.Children.Add(Visit(exprs.Last()));
            }

            result.Children.Add(closeParen);

            return result;

        }
    }
}
