using AntlrLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Kitsune.SyntaxParser
{
    public class Evaluator
    {
        public static Token OperandEval(Token operand)
        {
            try
            {
                Token result = new Token();
                long longNumber;
                double doubleNumber;
                bool boolValue;
                if (operand.Type == TOKENTYPE.NoData)
                {
                    return operand;
                }
                if (operand.Type == TOKENTYPE.Array)
                {
                    return operand;
                }
                if (operand.Value.GetType() == typeof(string))
                {
                    if (long.TryParse(operand.Value, out longNumber))
                    {
                        result.Type = TOKENTYPE.Long;
                        result.Value = longNumber;
                    }
                    else if (double.TryParse(operand.Value, out doubleNumber))
                    {
                        result.Type = TOKENTYPE.Double;
                        result.Value = doubleNumber;
                    }
                    else if (Boolean.TryParse(operand.Value, out boolValue))
                    {
                        result.Type = TOKENTYPE.Boolean;
                        result.Value = boolValue;
                    }
                    else if (operand.Value == "")
                    {
                        result.Type = TOKENTYPE.String;
                        result.Value = "";
                    }
                    else if (operand.Value.Trim()[0] == '\'' && operand.Value.Trim()[operand.Value.Length - 1] == '\'')
                    {
                        result.Type = TOKENTYPE.String;
                        result.Value = operand.Value.Substring(1, operand.Value.Length - 2);
                    }
                    else
                    {
                        result.Type = TOKENTYPE.Object;
                        result.Value = operand.Value;
                    }
                }
                else if (operand.Value.GetType() == typeof(Decimal) || operand.Value.GetType() == typeof(Double) || (double.TryParse(operand.Value.ToString(), out double testDoubleNumber) && operand.Value.ToString() == testDoubleNumber.ToString()))
                {
                    string val = operand.Value.ToString();

                    if (long.TryParse(val, out longNumber) && val == longNumber.ToString())
                    {
                        result.Type = TOKENTYPE.Long;
                        result.Value = longNumber;
                    }
                    else if (double.TryParse(val, out doubleNumber))
                    {
                        result.Type = TOKENTYPE.Double;
                        result.Value = doubleNumber;
                    }
                }
                else if (operand.Value.GetType() == typeof(Boolean))
                {
                    result.Type = TOKENTYPE.Boolean;
                    result.Value = operand.Value;
                }
                else
                {
                    result.Type = TOKENTYPE.Object;
                    result.Value = operand.Value;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token { Value = ex.Message, Type = TOKENTYPE.ERROR };
            }
        }

        public static Token NegativeNumberEval(Token opt, Token operand)
        {
            try
            {
                Token result = new Token();
                if (!opt.Value.Equals("-"))
                    return new Token { Type = TOKENTYPE.ERROR, Value = "Only negative number can be handled." };
                switch (operand.Type)
                {
                    case TOKENTYPE.Long:
                        result.Value = -1 * operand.Value;
                        result.Type = TOKENTYPE.Long;
                        break;
                    case TOKENTYPE.Double:
                        result.Value = -1 * operand.Value;
                        result.Type = TOKENTYPE.Double;
                        break;
                    default:
                        result.Type = TOKENTYPE.ERROR;
                        result.Value = "Invalid Operand";
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Token PostfixUnaryEval(Token operand, Token opt)
        {
            try
            {
                Token result = new Token();
                switch (operand.Type)
                {
                    case TOKENTYPE.String:
                        if (opt.Value.Equals("length"))
                        {
                            result.Value = operand.Value.Length;
                            result.Type = TOKENTYPE.Long;
                        }
                        else if (opt.Value.Equals("Length"))
                        {
                            result.Value = operand.Value.Length;
                            result.Type = TOKENTYPE.Long;
                        }
                        else if (opt.Value.Equals(".IsNullOrEmpty"))
                        {
                            result.Value = operand.Value == null || operand.Value.Equals("") ? true : false;
                            result.Type = TOKENTYPE.Boolean;
                        }
                        else if (opt.Value.Equals("isnullorempty"))
                        {
                            result.Value = operand.Value == null || operand.Value.Equals("") ? true : false;
                            result.Type = TOKENTYPE.Boolean;
                        }
                        else if (opt.Value.Equals("tolower"))
                        {
                            result.Value = operand.Value.ToLower();
                            result.Type = TOKENTYPE.String;
                        }
                        else if (opt.Value.Equals("toupper"))
                        {
                            result.Value = operand.Value.ToUpper();
                            result.Type = TOKENTYPE.String;
                        }
                        else if (opt.Value.Equals("tostring"))
                        {
                            result.Value = operand.Value.ToString();
                            result.Type = TOKENTYPE.String;
                        }
                        else if (opt.Value.Equals("tonumber"))
                        {
                            double doubleRes = 0;
                            if(double.TryParse(operand.Value.ToString(), out doubleRes))
                            {
                                result.Value = doubleRes;
                            }
                            else
                            {
                                result.Value = null;
                            }
                           
                            result.Type = TOKENTYPE.Double;
                        }
                        else
                        {
                            result.Value = "Invalid Postfixexpression";
                            result.Type = TOKENTYPE.ERROR;
                        }
                        break;
                    case TOKENTYPE.Array:
                        if (opt.Value.Equals(".length"))
                        {
                            result.Value = operand.Value._total ?? operand.Value.Count;
                            result.Type = TOKENTYPE.Long;
                        }
                        if (opt.Value.Equals("length"))
                        {
                            result.Value = operand.Value._total ?? operand.Value.Count;
                            result.Type = TOKENTYPE.Long;
                        }
                        else if (opt.Value.Equals(".isnullorempty"))
                        {
                            result.Value = operand.Value == null || (operand.Value._total ?? operand.Value.Count) == 0 ? true : false;
                            result.Type = TOKENTYPE.Boolean;
                        }
                        else if (opt.Value.Equals("isnullorempty"))
                        {
                            result.Value = operand.Value == null || (operand.Value._total ?? operand.Value.Count) == 0 ? true : false;
                            result.Type = TOKENTYPE.Boolean;
                        }
                        else
                        {
                            result.Value = "Invalid Postfixexpression";
                            result.Type = TOKENTYPE.ERROR;
                        }
                        break;
                    case TOKENTYPE.Object:
                        if (operand.Value._total != null)
                        {
                            if (opt.Value.Equals("length") || opt.Value.Equals(".length"))
                            {
                                result.Value = operand.Value._total;
                                result.Type = TOKENTYPE.Long;
                                break;
                            }
                            else if (opt.Value.Equals("isnullorempty") || opt.Value.Equals(".isnullorempty"))
                            {
                                result.Value = operand.Value == null || operand.Value._total == 0 ? true : false;
                                result.Type = TOKENTYPE.Boolean;
                                break;
                            }
                        }
                        result.Value = "This expression can only be applied on String or Array;";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                    case TOKENTYPE.NoData:
                        result.Value = "No Data available for the object";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                    default:
                        result.Value = "This expression can only be applied on String or Array;";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Token PostfixUnaryWithArgEval(Token operand, Token opt, List<Token> parameters = null)
        {
            try
            {
                Token result = new Token();
                Token firstParam = new Token();
                if (parameters != null && parameters.Any())
                {
                    foreach (Token parameter in parameters)
                    {
                        if (parameter.Type == TOKENTYPE.NoData)
                        {
                            result.Value = "";
                            result.Type = TOKENTYPE.NoData;
                            return result;
                        }
                    }
                    if (parameters[0].Value.GetType() == typeof(string))
                        parameters[0].Value = parameters[0].Value.Replace("\\n", "\n");
                    firstParam = parameters[0];
                }
                if (operand.Type == TOKENTYPE.NoData)
                {
                    result = operand;
                }
                else if (operand.Type == TOKENTYPE.String)
                {
                    if (opt.Value.Equals("getvalueat"))
                    {
                        if (operand.Value.Length <= firstParam.Value)
                            return new Token { Type = TOKENTYPE.String, Value = "" };
                        if (firstParam.Value < 0)
                            return new Token { Type = TOKENTYPE.String, Value = "" };
                        result.Value = operand.Value[Convert.ToInt32(firstParam.Value)];
                        result.Type = TOKENTYPE.String;
                    }
                    if (opt.Value.Equals("substr"))
                    {
                        try
                        {
                            var firstParamValue = Convert.ToInt32(firstParam.Value);
                            if (parameters.Count >= 2)
                            {
                                var secondParamValue = Convert.ToInt32(parameters[1].Value);
                                if (operand.Value.Length - firstParamValue < secondParamValue)
                                    result.Value = operand.Value.Substring(firstParamValue);
                                else
                                    result.Value = operand.Value.Substring(firstParamValue, secondParamValue);
                            }
                            else
                            {
                                result.Value = operand.Value.Substring(firstParamValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("split"))
                    {
                        try
                        {

                            if (parameters.Count == 1)
                            {
                                var firstParamValue = firstParam.Value.ToString();
                                result.Value = operand.Value.Split(new string[] { firstParamValue }, StringSplitOptions.None);
                            }

                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Array;
                    }
                    else if (opt.Value.Equals("indexof"))
                    {
                        try
                        {

                            if (parameters.Count == 1)
                            {
                                var firstParamValue = firstParam.Value.ToString();
                                result.Value = operand.Value.IndexOf(firstParamValue);
                            }
                            else if (parameters.Count == 2)
                            {
                                var firstParamValue = firstParam.Value.ToString();
                                int index = 0;
                                var secondParamValue = int.TryParse(parameters[1].Value.ToString(), out index);
                                result.Value = operand.Value.IndexOf(firstParamValue, index);
                            }

                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Double;
                    }
                    else if (opt.Value.Equals("replace"))
                    {
                        try
                        {

                            if (parameters.Count == 2)
                            {
                                var firstParamValue = firstParam.Value.ToString();
                                var secondParamValue = parameters[1].Value.ToString();

                                result.Value = operand.Value.Replace(firstParamValue, secondParamValue);
                            }

                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("contains"))
                    {
                        try
                        {
                            var firstParamValue = firstParam.Value.ToString();
                            result.Value = operand.Value.Contains(firstParamValue);
                        }
                        catch (Exception ex)
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                    }
                    else if (opt.Value.Equals("tolower"))
                    {
                        try
                        {
                            result.Value = operand.Value.ToLower();
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("toupper"))
                    {
                        try
                        {
                            result.Value = operand.Value.ToUpper();
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("tostring"))
                    {
                        result.Value = operand.Value.ToString();
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("tonumber"))
                    {
                        double doubleRes = 0;
                        if (double.TryParse(operand.Value.ToString(), out doubleRes))
                        {
                            result.Value = doubleRes;
                        }
                        else
                        {
                            result.Value = null;
                        }

                        result.Type = TOKENTYPE.Double;
                    }
                    else if (opt.Value.Equals("urlencode"))
                    {
                        try
                        {
                            result.Value = HttpUtility.UrlPathEncode(System.Text.RegularExpressions.Regex.Replace(operand.Value.ToString().ToLower(), "[^0-9a-zA-Z]+", " ").Replace(" ", "-"));
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("decode"))
                    {
                        try
                        {
                            result.Value = HttpUtility.HtmlDecode(operand.Value);
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.String;
                    }
                    else if (opt.Value.Equals("length"))
                    {
                        try
                        {
                            result.Value = operand.Value.Length;
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Long;
                    }

                }
                else if (operand.Type == TOKENTYPE.Array)
                {
                    if (opt.Value.Equals("getvalueat"))
                    {
                        if (operand.Value.Count <= firstParam.Value)
                            return new Token { Type = TOKENTYPE.ERROR, Value = "Invalid: Index out of Range" };
                        if (firstParam.Value < 0)
                            return new Token { Type = TOKENTYPE.ERROR, Value = "Invalid: Index is negative." };
                        result.Value = operand.Value[Convert.ToInt16(firstParam.Value)].Value;
                        result.Type = operand.Value[Convert.ToInt16(firstParam.Value)].Type;
                    }
                    else if (opt.Value.Equals("contains"))
                    {
                        try
                        {
                            var firstParamValue = firstParam.Value.ToString();
                            result.Value = operand.Value.Contains(firstParamValue);
                        }
                        catch (Exception ex)
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                    }
                    else if (opt.Value.Contains("length"))
                    {
                        try
                        {
                            result.Value = operand.Value.Count;
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Long;
                    }
                    else if (opt.Value.Equals("distinct"))
                    {
                        try
                        {
                            result.Value = operand.Value.Distinct().ToArray();
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Array;
                    }
                }
                else if (operand.Type == TOKENTYPE.Object)
                {
                    if (opt.Value.Contains("length"))
                    {
                        try
                        {
                            result.Value = operand.Value.Length;
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Long;
                    }
                }

                else if (operand.Type == TOKENTYPE.Long || operand.Type == TOKENTYPE.Double)
                {
                    if (opt.Value.Contains("urlencode"))
                    {
                        try
                        {
                            result.Value = HttpUtility.UrlPathEncode(System.Text.RegularExpressions.Regex.Replace(operand.Value.ToString().ToLower(), "[^0-9a-zA-Z]+", " ").Replace(" ", "-"));
                        }
                        catch (Exception ex)
                        {
                            result.Value = "";
                        }
                        result.Type = TOKENTYPE.Long;
                    }
                    else
                    {
                        operand.Type = TOKENTYPE.String;
                        operand.Value = operand.Value.ToString();
                        result = PostfixUnaryWithArgEval(operand, opt, parameters);
                    }
                }
                else
                {
                    result.Value = "This Expression cannot be applied to this type.";
                    result.Type = TOKENTYPE.ERROR;
                }
                return result;

            }
            catch (Exception ex)
            {
                return new Token { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token PrefixUnaryEval(Token opt, Token operand)
        {
            try
            {
                Token result = new Token();
                if (operand.Type == TOKENTYPE.NoData)
                {
                    operand.Value = false;
                    operand.Type = TOKENTYPE.Boolean;
                }
                else if (opt.Value.Equals("!") && operand.Type == TOKENTYPE.Boolean)
                {
                    result.Value = !operand.Value;
                    result.Type = TOKENTYPE.Boolean;
                }
                else
                {
                    result.Value = "This Expression cannot be applied to this operand.";
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Token Arithmetic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                if (operand1.Type == TOKENTYPE.NoData && operand2.Type == TOKENTYPE.NoData)
                    return NullArithmatic(operand1, opt, operand2);
                else if (operand1.Type == TOKENTYPE.NoData || operand2.Type == TOKENTYPE.NoData)
                        return PartialNullArithmatic(operand1, opt, operand2);
                else if (operand1.Type == TOKENTYPE.Array && operand2.Type == TOKENTYPE.Array)
                    return ArrayArithmatic(operand1, opt, operand2);
                else if (opt.Value == "&&" || opt.Value == "||" || operand1.Type == TOKENTYPE.Boolean || operand2.Type == TOKENTYPE.Boolean)
                    return BooleanArithmatic(operand1, opt, operand2);
                else if (operand1.Type == TOKENTYPE.String || operand2.Type == TOKENTYPE.String)
                    return StringArithmetic(operand1, opt, operand2);
                else if (operand1.Type == TOKENTYPE.Object && operand2.Type == TOKENTYPE.Object)
                    return ObjectArithmatic(operand1, opt, operand2);
                else
                    return NumberArithmetic(operand1, opt, operand2);
            }
            catch (Exception ex)
            {
                return new Token { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token StringArithmetic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                result.Type = TOKENTYPE.String;
                string tempVal = opt.Value;

                switch (tempVal)
                {
                    case "+":
                        result.Value = (operand1.Value + operand2.Value);
                        break;
                    case "==":
                        result.Value = operand1.Value.Equals(operand2.Value);
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "!=":
                        result.Value = !operand1.Value.Equals(operand2.Value);
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case ">":
                        if (operand1.Value.GetType() == operand2.Value.GetType())
                        {
                            result.Value = operand1.Value > operand2.Value;
                        }
                        else
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "<":
                        if (operand1.Value.GetType() == operand2.Value.GetType())
                        {
                            result.Value = operand1.Value < operand2.Value;
                        }
                        else
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case ">=":
                        if (operand1.Value.GetType() == operand2.Value.GetType())
                        {
                            result.Value = operand1.Value >= operand2.Value;
                        }
                        else
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "<=":
                        if (operand1.Value.GetType() == operand2.Value.GetType())
                        {
                            result.Value = operand1.Value <= operand2.Value;
                        }
                        else
                        {
                            result.Value = false;
                        }
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    default:
                        result.Value = "Invalid Operator.";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Value = ex.Message, Type = TOKENTYPE.ERROR };
            }
        }

        public static Token NumberArithmetic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token
                {
                    Type = TOKENTYPE.Double
                };

                if (operand1.Type == TOKENTYPE.NoData)
                {
                    operand1.Value = 0;
                    operand1.Type = TOKENTYPE.Double;
                }
                if (operand2.Type == TOKENTYPE.NoData)
                {
                    operand2.Value = 0;
                    operand2.Type = TOKENTYPE.Double;
                }
                double op1 = (double)(operand1.Value);
                double op2 = (double)(operand2.Value);

                operand1.Type = TOKENTYPE.Double;
                operand1.Value = op1;
                operand2.Type = TOKENTYPE.Double;
                operand2.Value = op2;

                string tempVal = opt.Value;
                switch (tempVal)
                {
                    case "+":
                        var divisionValue = operand1.Value + operand2.Value;
                        result.Value = Math.Round((double)divisionValue, 2);
                        break;
                    case "-":
                        divisionValue = operand1.Value - operand2.Value;
                        result.Value = Math.Round((double)divisionValue, 2);
                        break;
                    case "*":
                        divisionValue = operand1.Value * operand2.Value;
                        result.Value = Math.Round((double)divisionValue, 2);
                        break;
                    case "/":
                        divisionValue = operand1.Value / operand2.Value;
                        result.Value = Math.Round((double)divisionValue, 2);
                        break;
                    case "%":
                        result.Value = operand1.Value % operand2.Value;
                        break;
                    case "==":
                        result.Value = operand1.Value == operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "!=":
                        result.Value = operand1.Value != operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case ">":
                        result.Value = operand1.Value > operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "<":
                        result.Value = operand1.Value < operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case ">=":
                        result.Value = operand1.Value >= operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    case "<=":
                        result.Value = operand1.Value <= operand2.Value;
                        result.Type = TOKENTYPE.Boolean;
                        break;
                    default:
                        result.Value = "Invalid Operator.";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Value = ex.Message, Type = TOKENTYPE.ERROR };
            }
        }

        public static Token BooleanArithmatic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                result.Type = TOKENTYPE.Boolean;
                string tempVal = opt.Value;
                if (operand1.Type == TOKENTYPE.String)
                {
                    operand1.Value = !string.IsNullOrEmpty(operand1.Value);
                    operand1.Type = TOKENTYPE.Boolean;
                }
                else if (operand1.Type != TOKENTYPE.Boolean)
                {
                    operand1.Value = Convert.ToBoolean(operand1.Value);
                    operand1.Type = TOKENTYPE.Boolean;
                }

                if (operand2.Type == TOKENTYPE.String)
                {
                    operand2.Value = !string.IsNullOrEmpty(operand2.Value);
                    operand2.Type = TOKENTYPE.Boolean;
                }
                else if (operand2.Type != TOKENTYPE.Boolean)
                {
                    operand2.Value = Convert.ToBoolean(operand2.Value);
                    operand2.Type = TOKENTYPE.Boolean;
                }

                switch (tempVal)
                {
                    case "&&":
                        result.Value = operand1.Value && operand2.Value;
                        break;
                    case "||":
                        result.Value = operand1.Value || operand2.Value;
                        break;
                    case "==":
                        result.Value = operand1.Value == operand2.Value;
                        break;
                    case "!=":
                        result.Value = operand1.Value != operand2.Value;
                        break;
                    default:
                        result.Value = "Invalid Operator.";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token ObjectArithmatic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                result.Type = TOKENTYPE.Boolean;
                string tempVal = opt.Value;
                switch (tempVal)
                {
                    case "==":
                        if (operand1.Type == TOKENTYPE.NoData || operand2.Type == TOKENTYPE.NoData)
                        {
                            result.Value = false;
                        }
                        else
                        {
                            result.Value = CompareObjects(operand1.Value, operand2.Value);
                        }
                        break;
                    case "!=":
                        if (operand1.Type == TOKENTYPE.NoData || operand2.Type == TOKENTYPE.NoData)
                        {
                            result.Value = true;
                        }
                        else
                        {
                            result.Value = !CompareObjects(operand1.Value, operand2.Value);
                        }
                        break;
                    default:
                        result.Value = "Invalid Operator.";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token NullArithmatic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                switch (opt.Value)
                {
                    case "==":
                        result.Type = TOKENTYPE.Boolean;
                        result.Value = true;
                        break;
                    case "!=":
                    case "&&":
                    case "||":
                    case ">":
                    case ">=":
                    case "<":
                    case "<=":
                        result.Type = TOKENTYPE.Boolean;
                        result.Value = false;
                        break;
                    default:
                        result = operand1;
                        break;

                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token PartialNullArithmatic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                dynamic nonNullType = null;
                if (operand1.Type == TOKENTYPE.NoData)
                {
                    nonNullType = operand2.Type;
                }
                else
                {
                    nonNullType = operand1.Type;
                }
                switch (nonNullType)
                {
                    case TOKENTYPE.Boolean:
                        if (operand1.Type == TOKENTYPE.NoData)
                        {
                            operand1.Value = false;
                            operand1.Type = TOKENTYPE.Boolean;
                        }
                        else
                        {
                            operand2.Value = false;
                            operand2.Type = TOKENTYPE.Boolean;
                        }
                        return BooleanArithmatic(operand1, opt, operand2);
                    case TOKENTYPE.String:
                        if (operand1.Type == TOKENTYPE.NoData)
                        {
                            operand1.Value = "";
                            operand1.Type = TOKENTYPE.String;
                        }
                        else
                        {
                            operand2.Value = "";
                            operand2.Type = TOKENTYPE.String;
                        }
                        return StringArithmetic(operand1, opt, operand2);
                    case TOKENTYPE.Float:
                    case TOKENTYPE.Integer:
                    case TOKENTYPE.Long:
                        if (operand1.Type == TOKENTYPE.NoData)
                        {
                            operand1.Value = 0;
                            operand1.Type = TOKENTYPE.Long;
                        }
                        else
                        {
                            operand2.Value = 0;
                            operand2.Type = TOKENTYPE.Long;
                        }
                        return NumberArithmetic(operand1, opt, operand2);
                    case TOKENTYPE.Object:
                    case TOKENTYPE.Array:
                        return ObjectArithmatic(operand1, opt, operand2);
                    default:
                        Token result = new Token();
                        result.Type = TOKENTYPE.NoData;
                        result.Value = "";
                        return result;
                }
            }
            catch (Exception ex)
            {
                return new Token() { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        public static Token ArrayArithmatic(Token operand1, Token opt, Token operand2)
        {
            try
            {
                Token result = new Token();
                result.Type = TOKENTYPE.Array;
                string tempVal = opt.Value;
                switch (tempVal)
                {
                    case "+":
                        result.Value = new List<dynamic>();//operand1.Value.AddRange(operand2.Value);
                        result.Value.AddRange(operand1.Value);
                        result.Value.AddRange(operand2.Value);
                        break;
                    default:
                        result.Value = "Invalid Operator.";
                        result.Type = TOKENTYPE.ERROR;
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                return new Token() { Type = TOKENTYPE.ERROR, Value = ex.Message };
            }
        }

        /// <summary>
        /// Compare two objects and return true if both operand values are true.
        /// </summary>
        /// <param name="operand1"></param>
        /// <param name="operand2"></param>
        /// <returns></returns>
        private static bool CompareObjects(dynamic operand1, dynamic operand2)
        {
            string obj1_Kid = GetPropertyDynamic(operand1, "_kid");
            string obj2_Kid = GetPropertyDynamic(operand2, "_kid");
            string obj1_ReflectionId = GetPropertyDynamic(operand1, "_reflectionId");
            string obj2_ReflectionId = GetPropertyDynamic(operand2, "_reflectionId");
            if (obj1_Kid != null && ((obj2_Kid != null && obj1_Kid == obj2_Kid)
                                        || (obj2_ReflectionId != null && obj1_Kid == obj2_ReflectionId)))
            {
                return true;
            }
            if (obj1_ReflectionId != null && ((obj2_Kid != null && obj1_ReflectionId == obj2_Kid)
                                        || (obj2_ReflectionId != null && obj1_ReflectionId == obj2_ReflectionId)))
            {
                return true;
            }
            return Convert.ToString(operand1).Equals(Convert.ToString(operand2));
        }

        private static dynamic GetPropertyDynamic(dynamic obj, string property)
        {
            dynamic val = null;
            try
            {
                val = obj[property];
            }
            catch { }
            return val;
        }
    }
}
