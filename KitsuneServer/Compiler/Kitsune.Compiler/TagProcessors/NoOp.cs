using System.Collections.Generic;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.SyntaxParser;

namespace Kitsune.Compiler.TagProcessors
{
    public class NoOp : Processor
    {
        public override void ProcessNode(CompileResourceRequest request, List<CompilerError> compileErrors, Dictionary<string, int> customVariables, string rootUrl, string filePath, Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasdepth, int depth, HtmlNode node, HtmlAttribute dynamicAttribute, List<MatchNode> objectNamesToValidate, DocumentValidator documentValidator)
        {
            List<string> objects = Parser.GetObjects(dynamicAttribute.Value);
            foreach (string obj in objects)
            {
                objectNamesToValidate.Add(new MatchNode { Value = obj.Replace("[[", "").Replace("]]", "").ToLower(), Line = dynamicAttribute.Line, Column = dynamicAttribute.LinePosition });
            }
            dynamicAttribute.Value = "";
        }
    }
}
