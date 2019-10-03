using HtmlAgilityPack;
using Kitsune.Helper;
using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using System.Collections.Generic;
using System.Linq;

namespace Kitsune.Compiler.Helpers
{
    public class DocumentPreprocessor
    {
        /// <summary>
        /// Preprocess document for compilation.
        /// </summary>
        /// <param name="req">compile request</param>
        /// <param name="kentity">language for project</param>
        /// <param name="filePath">file path</param>
        /// <param name="document">document to compile</param>
        /// <param name="nfUXErrors">list of errors</param>
        /// <param name="rootUrl">root url</param>
        public  static void PreprocessDocument(CompileResourceRequest req, KEntity kentity, string filePath, out HtmlDocument document, List<CompilerError> nfUXErrors, out string rootUrl)
        {
            document = GetDocumentObject();
            try
            {
                document.LoadHtml(req.FileContent);
            }
            catch { }
            
            rootUrl = "";
            /*
            if (document.ParseErrors.Count() > 0)
            {
                foreach (var error in document.ParseErrors.ToList())
                {
                    nfUXErrors.Add(CompileResultHelper.GetCompileError(error.Reason, error.Line, error.LinePosition));
                }
            }
            */
            var descendants = document.DocumentNode.Descendants();
            PrepCommentTagsForCompile(ref document, ref descendants);
            SetPageProperties(req, kentity, filePath, ref rootUrl, descendants);
        }

        /// <summary>
        /// Set properties like pageType, IsStatic, rootUrl, URLPattern.
        /// </summary>
        /// <param name="req">compile request</param>
        /// <param name="kentity">k language for project</param>
        /// <param name="filePath">file path</param>
        /// <param name="rootUrl">root url for project</param>
        /// <param name="descendants">descendants of the document to compile</param>
        private static void SetPageProperties(CompileResourceRequest req, KEntity kentity, string filePath, ref string rootUrl, IEnumerable<HtmlNode> descendants)
        {

            //If the page is set to static, don't validate against language
            if (descendants.Any(x => x.Attributes[LanguageAttributes.KPartial.GetDescription()] != null))
                req.PageType = Models.Project.KitsunePageType.PARTIAL;
            //If the page is partial then by default its dynamic
            if (req.PageType == Models.Project.KitsunePageType.PARTIAL)
                req.IsStatic = false;
            else
                req.IsStatic = IsResourceStatic(req.FileContent, req.SourcePath);

            if (!req.IsStatic)
            {
                //Generate valid class name from the page name 
                req.ClassName = GenerateClassName(filePath);
                rootUrl = string.Format("{0}.rootaliasurl.url", kentity?.EntityName?.ToLower() ?? Constants.KPayDefaultSchema.ToLower());
                if (string.IsNullOrEmpty(req.UrlPattern))
                    req.UrlPattern = string.Format("[[{0}]]/{1}", rootUrl, filePath.ToLower());
            }
        }

        /// <summary>
        /// Remove k tags from comments.
        /// </summary>
        /// <param name="document">document to pre process</param>
        /// <param name="descendants">descendants of the document</param>
        private static void PrepCommentTagsForCompile(ref HtmlDocument document, ref IEnumerable<HtmlNode> descendants)
        {
            //Not removing entire comment as it might be useful for doctype and browser conditional also for the accurate line number of the error display
            var commentNodes = document.DocumentNode.SelectNodes("//comment()");
            if (commentNodes != null)
            {
                foreach (HtmlNode comment in commentNodes)
                {
                    comment.InnerHtml = Helper.Constants.WidgetRegulerExpression.Replace(comment.InnerHtml, "");
                }
                var newObjectHtml = document.DocumentNode.OuterHtml;
                document = GetDocumentObject();
                document.LoadHtml(newObjectHtml);
                descendants = document.DocumentNode.Descendants();
            }
        }

        /// <summary>
        /// Generate class name based on PageName.
        /// </summary>
        /// <param name="pagename">pageName used to generate class name</param>
        /// <returns></returns>
        private static string GenerateClassName(string pagename)
        {
            return (pagename.Trim('/')).ToUpper().Replace('.', '_').Replace('/', '_').Replace('-', '_').Replace('(', '_').Replace(')', '_').Replace('+', '_');
        }

        /// <summary>
        /// Check if the file is static or not.
        /// Default : all non html extersion file will be considered as static
        /// All html/html.dl/htm/htm.dl files will be checked for [[]] expression 
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Whether resource is static</returns>
        private static bool IsResourceStatic(string content, string sourcePath)
        {
            if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(sourcePath))
            {
                if (Constants.DynamicFileExtensionRegularExpression.IsMatch(sourcePath.ToLower())
                    && (Constants.WidgetRegulerExpression.IsMatch(content) || IsNotStaticKTagsPresent(content)))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if static KTags are present in content
        /// </summary>
        /// <param name="content">content to verify</param>
        /// <returns>whether static Ktags are present</returns>
        private static bool IsNotStaticKTagsPresent(string content)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionAutoCloseOnEnd = true;
            htmlDoc.LoadHtml(content);
            var descendants = htmlDoc.DocumentNode.Descendants();
            var kDlObject = descendants.Where(s => s.Attributes[LanguageAttributes.KDL.GetDescription()] != null);
            if (kDlObject != null && kDlObject.Any())
                return true;
            var kPayObject = descendants.Where(s => s.Attributes[LanguageAttributes.KPayAmount.GetDescription()] != null);
            if (kPayObject != null && kPayObject.Any())
                return true;

            var csrfObject = Constants.CSRFRegularExpression.IsMatch(content); 
            if (csrfObject)
                return true;

            return false;
        }

        public static HtmlDocument GetDocumentObject()
        {
            var document = new HtmlDocument();
            document.OptionAutoCloseOnEnd = false;
            document.OptionCheckSyntax = false;
            return document;
        }
    }
}
