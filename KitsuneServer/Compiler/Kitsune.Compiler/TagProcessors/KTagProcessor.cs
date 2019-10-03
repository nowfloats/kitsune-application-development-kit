using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Kitsune.Compiler.Helpers;
using Kitsune.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;

namespace Kitsune.Compiler.TagProcessors
{
    public class KTagProcessor : Processor
    {
        /// <summary>
        /// Process nodes with k-script tags.
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
            Regex urlPattern = new Regex("http(s)?://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);
            string getApiURL = node.Attributes["get-api"]?.Value;
            string postApiURL = node.Attributes["post-api"]?.Value;
            if (getApiURL == null && postApiURL == null)
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KScriptNoApi, node.Line, node.LinePosition));
            }
            else if (getApiURL != null && postApiURL != null)
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KScriptMultipleApi, node.Line, node.LinePosition));
            }
            else if (getApiURL != null && !urlPattern.IsMatch(getApiURL))
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KScriptGetApiNoURL, node.Line, node.LinePosition));
            }
            else if (postApiURL != null && !urlPattern.IsMatch(postApiURL))
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KScriptPostApiNoURL, node.Line, node.LinePosition));
            }
            else if (classNameAlias.ContainsKey("kresult"))
            {
                compileErrors.Add(CompileResultHelper.GetCompileError(ErrorCodeConstants.KScriptNested, node.Line, node.LinePosition));
            }
            else
            {
                classNameAlias.Add("kresult", "-1");
                
                if (classNameAliasdepth.ContainsKey(depth))
                {
                    string oldAliasDepth = classNameAliasdepth[depth];
                    classNameAliasdepth.Remove(depth);
                    classNameAliasdepth.Add(depth, classNameAliasdepth[depth] + ",kresult");
                }
                else
                {
                    classNameAliasdepth.Add(depth, "kresult");
                }
            }
        }
    }
}
