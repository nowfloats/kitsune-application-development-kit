using System;
using System.Collections.Generic;
using System.Linq;
using Kitsune.AntlrLibrary;
using Kitsune.AntlrLibrary.Model;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;

namespace Kitsune.Compiler.TagProcessors
{
    public class KSearchProcessor : Processor
    {
        /// <summary>
        /// Process nodes with k-object tags.
        /// </summary>
        /// <param name="request">Incoming request</param>
        /// <param name="compileErrors">list of errors during compilation</param>
        /// <param name="kentity">project language</param>
        /// <param name="customVariables">output list to hold custom variables</param>
        /// <param name="rootUrl">root url</param>
        /// <param name="filePath">file path</param>
        /// <param name="classNameAlias">aliases used either with k-repeat or k-object</param>
        /// <param name="classNameAliasdepth">depth based storage of aliases</param>
        /// <param name="depth">depth of the node in DOM</param>
        /// <param name="dynamicAttribute">key-value pair of the k-tag</param>
        public override void ProcessNode(CompileResourceRequest request, List<CompilerError> compileErrors, Dictionary<string, int> customVariables, string rootUrl, string filePath, Dictionary<string, string> classNameAlias, Dictionary<int, string> classNameAliasdepth, int depth, HtmlNode node, HtmlAttribute dynamicAttribute, List<MatchNode> objectNamesToValidate, DocumentValidator documentValidator)
        {
            if (Kitsune.Helper.Constants.WidgetRegulerExpression.IsMatch(dynamicAttribute.Value))
            {
                var property = Kitsune.Helper.Constants.WidgetRegulerExpression.Match(dynamicAttribute.Value).Groups[1].Value;
                KClass kClassOb;
                if (!IsSearchableProperty(property, out kClassOb, documentValidator))
                {
                    compileErrors.Add(new CompilerError { Message = "Invalid search property", LineNumber = node.Line, LinePosition = node.LinePosition });
                }
                request.PageType = Models.Project.KitsunePageType.SEARCH;
            }
        }
    }
}
