using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class MatchNode
    {
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
    public class HtmlHelper
    {
        public static List<MatchNode> GetExpressionFromElement(string node, int line)
        {
            var matches = Kitsune.Helper.Constants.WidgetRegulerExpression.Matches(node);
            var result  = new List<MatchNode>();
            foreach (Match match in matches)
            {
                result.Add(new MatchNode { Column = match.Index, Line = line, Value = match.Value });
            }
            return result;
        }
        public static List<MatchNode> GetFunctionExpressionFromElement(string node, int line)
        {
            var matches = Kitsune.Helper.Constants.FunctionRegularExpression.Matches(node);
            var result = new List<MatchNode>();
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    result.Add(new MatchNode { Column = match.Index, Line = line, Value = match.Groups[0].Value.Replace("[[", "").Replace("]]", "") });
            }
            return result;
        }
        public static List<MatchNode> GetKDLExpressionFromElement(string node, int line)
        {
            var matches = Kitsune.Helper.Constants.KDLRegularExpression.Matches(node);
            var result = new List<MatchNode>();
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    result.Add(new MatchNode { Column = match.Index, Line = line, Value = match.Groups[0].Value.Replace("[[", "").Replace("]]", "") });
            }
            return result;
        }
        public static List<MatchNode> SetObjectExpressionFromElement(string node, int line)
        {
            var matches = Kitsune.Helper.Constants.SetObjectRegularExpression.Matches(node);
            var result = new List<MatchNode>();
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                    result.Add(new MatchNode { Column = match.Index, Line = line, Value = match.Groups[0].Value });//.Replace("[[", "").Replace("]]", "") });
            }
            return result;
        }
        public static List<string> GetPropertiesFromAttributeValue(string attributeValue)
        {
            var result = new List<string>();
            var matches = Helper.Constants.WidgetRegulerExpression.Matches(attributeValue);
            if (matches != null)
                foreach (var mat in matches)
                {

                    result.AddRange(mat.ToString().Split(' ').Where(x => x.IndexOf('.') != -1).ToList());
                }
            return result;
        }
        public static IEnumerable<HtmlAttribute> GetPropertyAttributes(HtmlNode node)
        {
            return node.Attributes.Where(y => Kitsune.Helper.Constants.WidgetRegulerExpression.Matches(y.Value).Count > 0);
        }
    }
}
