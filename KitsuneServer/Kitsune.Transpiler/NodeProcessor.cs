using HtmlAgilityPack;
using Kitsune.Helper;
using Kitsune.Language.Helper;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.Models.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Kitsune.Transpiler
{
    class NodeProcessor
    {
        private int currentBlockNumber = 1;

        private Dictionary<int, BlockNode> blockNodeReferenceList = new Dictionary<int, BlockNode>();

        private readonly string[] nodeNameBlockDescriptors =
        {
            LanguageAttributes.KScript.GetDescription().ToLower(),
            "img",
            LanguageAttributes.KTag.GetDescription().ToLower()
        };

        private readonly string[] attributeNameBlockDescriptors =
        {
            LanguageAttributes.KShow.GetDescription().ToLower(),
            LanguageAttributes.KHide.GetDescription().ToLower(),
            LanguageAttributes.KRepeat.GetDescription().ToLower(),
            LanguageAttributes.KNoRepeat.GetDescription().ToLower(),
            LanguageAttributes.KPayAmount.GetDescription().ToLower()
        };

        #region internal functions
        private int GetNext()
        {
            return ++currentBlockNumber;
        }

        private string GenerateBlockIdentifier()
        {
            return $"##k-{GetNext()}##";
        }
        #endregion

        public KitsunePage Process(string htmlString, string sourcePath, string kObject, Dictionary<string, int> customVariables = null, string offset = null)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            KitsunePage kitsunePage = new KitsunePage(sourcePath, kObject, offset);
            //Remove KObject and Kdl attribute 
            foreach (var elemm in doc.DocumentNode.DescendantsAndSelf().Where(x => x.Attributes.Any(y => y.Name.ToLower() == LanguageAttributes.KDL.GetDescription().ToLower() || y.Name.ToLower() == LanguageAttributes.KObject.GetDescription().ToLower())))
            {
                elemm.Attributes.Remove("k-dl");
                elemm.Attributes.Remove("k-object");
            }

            kitsunePage.AddNodes(GetNodes(doc.DocumentNode, false));

            if (customVariables != null && customVariables.Any())
            {
                foreach (var variable in customVariables)
                    kitsunePage.AddCustomVariable(variable.Key, variable.Value);
            }

            return kitsunePage;
        }

        private List<INode> GetNodes(HtmlNode node, bool excludeTopNode)
        {
            List<INode> nodeList = new List<INode>();
            ExtractBlocks(node);
            if (excludeTopNode)
            {
                nodeList = SplitNodes(node.InnerHtml);
            }
            else
            {
                nodeList = SplitNodes(node.OuterHtml);
            }
            return nodeList;
        }

        #region extract blocks
        private void ExtractBlocks(HtmlNode node)
        {
            while (node != null)
            {
                #region processing
                if (node != null)
                {
                    node = ProcessExtractBlocksNode(node);
                }
                #endregion

                #region traversal
                if (node.HasChildNodes)
                {
                    node = node.FirstChild;
                }
                else if (node.NextSibling != null)
                {
                    node = node.NextSibling;
                }
                else
                {
                    while (node != null && node.NextSibling == null)
                    {
                        node = node.ParentNode;
                    }
                    if (node != null)
                    {
                        node = node.NextSibling;
                    }
                }
                #endregion
            }
        }

        private HtmlNode ProcessExtractBlocksNode(HtmlNode node)
        {
            HtmlNode returnNode = node;

            bool isKTag = false;
            if (nodeNameBlockDescriptors.Contains(node.Name))
            {
                if (node.Name.Equals(LanguageAttributes.KScript.GetDescription().ToLower()))
                {
                    var apiUrl = decodeExpression(node.GetAttributeValue("get-api", null));
                    var input = node.GetAttributeValue("input", null);
                    var headers = node.GetAttributeValue("headers", null)?.Trim()?.Trim('[')?.Trim(']');
                    string cacheEnabledStr = node.GetAttributeValue("cacheenabled", null);
                    BlockNode bNode = new KScriptNode(apiUrl, input, headers, cacheEnabledStr);
                    HtmlNode tempNode = node.OwnerDocument.CreateTextNode(GenerateBlockIdentifier());
                    returnNode = tempNode;
                    blockNodeReferenceList.Add(currentBlockNumber, bNode);
                    node.ParentNode.InsertAfter(tempNode, node);
                    node.Remove();
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml("<div></div>");
                    HtmlNode newNode = htmlDoc.DocumentNode.FirstChild;
                    newNode.AppendChildren(node.ChildNodes);

                    bNode.Children.AddRange(GetNodes(newNode, true));
                    return tempNode;
                }
                else if (node.Name.Equals(LanguageAttributes.KTag.GetDescription().ToLower()))
                {
                    isKTag = true;
                }
                //Removed the srcset as its domain specific (some cdn was not configured so that it was not working on some domain)
                //else if (node.Name.Equals("img"))
                //{
                //    if (node.Attributes.Any(x => "src" == x.Name?.ToLower()))
                //    {
                //        node.Attributes.Remove("srcset");
                //        node.Attributes.Add("srcset", $"[[getSrcSet({node.Attributes["src"].Value.Replace(@"[[", @"\[\[").Replace(@"]]", @"\]\]")})]]");
                //    }
                //}
            }

            List<HtmlAttribute> attributeList = node.Attributes.Where(x => attributeNameBlockDescriptors.Contains(x.Name)).ToList();
            if (attributeList.Count > 0)
            {
                HtmlAttribute repeatAttribute = null;
                repeatAttribute = attributeList.FirstOrDefault(x => x.Name == LanguageAttributes.KRepeat.GetDescription().ToLower());
                foreach (HtmlAttribute attribute in attributeList)
                {
                    
                   
                        
                        BlockNode blockNode = null;
                        switch (attribute.Name.ToLower())
                        {
                            case "k-show":
                                blockNode = new KShowNode(decodeExpression(attribute.Value.Replace("[[", "").Replace("]]", "")));
                                break;
                            case "k-hide":
                                blockNode = new KHideNode(decodeExpression(attribute.Value.Replace("[[", "").Replace("]]", "")));
                                break;
                            case "k-pay-amount":
                                {
                                    node.Attributes.Add("k-pay-checksum", $"[[getChecksum({attribute.Value.Replace("[[", "").Replace("]]", "")})]]");
                                }
                                break;
                            case "k-norepeat":
                                blockNode = new KNoRepeatNode();
                                break;
                        }
                        if (blockNode != null)
                        {
                            attribute.Remove();
                            if (node.ParentNode != null)
                            {
                                HtmlNode tempNode = node.OwnerDocument.CreateTextNode(GenerateBlockIdentifier());
                                blockNodeReferenceList.Add(currentBlockNumber, blockNode);
                                node.ParentNode.InsertAfter(tempNode, node);
                                node.Remove();
                                //exclude the top node if isKtag to remove ktag node
                                blockNode.Children.AddRange(GetNodes(node, isKTag && repeatAttribute == null ? true : false));
                                return tempNode;
                            }
                            else
                                return HtmlNode.CreateNode("<!-- Invalid node -->");
                        }
                    
                }

                if (repeatAttribute != null)
                {
                    var repeatNode = ProcessRepeatNode(node, repeatAttribute, isKTag);
                    if (repeatNode != null)
                    {
                        return repeatNode;
                    }
                }
            }
            return returnNode;
        }
        private HtmlNode ProcessRepeatNode(HtmlNode node, HtmlAttribute repeatAttribute, bool isKtag)
        {
            BlockNode repeatNode = null;
            AntlrLibrary.Model.Node result = AntlrLibrary.LexerGenerator.Parse(repeatAttribute.Value.Trim('[', ']'));
            if (result?.Token?.Value == AntlrLibrary.Model.ACTIONS.Loop)
            {
                string[] repeatParts = repeatAttribute.Value.Replace("[[", "").Replace("]]", "").Split(":");
                string[] repeatRefParts = repeatParts[0].Split(",");
                string referenceObject = repeatRefParts[0];
                string iterator = repeatRefParts[1];
                string startIndex = repeatRefParts[2];
                string endIndex = repeatParts[1];
                repeatNode = new KRepeatNode(decodeExpression(startIndex), decodeExpression(endIndex), iterator, decodeExpression(referenceObject));
            }
            else if (result?.Token?.Value == AntlrLibrary.Model.ACTIONS.InLoop)
            {
                string[] repeatParts = repeatAttribute.Value.Trim('[', ']').Split(" in ");
                //TODO: Map loopvariable to referenceObject[iterator] in the block scope
                string loopVariable = repeatParts[0].ToLower();
                string iterator = "K_IND_" + currentBlockNumber;//GenerateVariableName(5);
                string referenceObject = repeatParts[1].ToLower();
                string startIndex = "0";
                string endIndex = referenceObject + ".length()";
                repeatNode = new KRepeatNode(startIndex, endIndex, iterator, referenceObject, loopVariable);
            }
            if (repeatNode != null)
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml("<div></div>");
                HtmlNode newNode = htmlDoc.DocumentNode.FirstChild;
                newNode.AppendChildren(node.ChildNodes);

                repeatNode.Children.AddRange(GetNodes(newNode, true));

                HtmlNode tempNode = node.OwnerDocument.CreateTextNode(GenerateBlockIdentifier());
                blockNodeReferenceList.Add(currentBlockNumber, repeatNode);
                node.RemoveAllChildren();
                repeatAttribute.Remove();
                if (isKtag & node.ParentNode != null)
                {
                    node.ParentNode.InsertAfter(tempNode, node);
                    node.Remove();
                }
                else
                {
                    node.AppendChild(tempNode);
                }
                return node;
            }
            return null;
        }
        #endregion

        #region generate node list
        private List<INode> SplitNodes(string htmlText)
        {
            List<INode> returnNodeList = new List<INode>();
            var expressionMatches = Kitsune.Helper.Constants.WidgetRegulerExpression.Matches(htmlText);
            if (expressionMatches != null && expressionMatches.Count > 0)
            {
                string textNode = string.Empty;

                for (var matInd = 0; matInd < expressionMatches.Count; matInd++)
                {
                    textNode = htmlText.Substring(0, htmlText.IndexOf(expressionMatches[matInd].Value));

                    if (!string.IsNullOrEmpty(textNode))
                    {
                        returnNodeList.AddRange(GetAllNodes(textNode));
                    }

                    returnNodeList.Add(GetExpressionNode(expressionMatches[matInd].Value.Replace("[[", "").Replace("]]", "").Replace(@"\[", "[").Replace(@"\]", "]")));

                    htmlText = htmlText.Substring(textNode.Length + expressionMatches[matInd].Value.Length);
                }
            }
            if (!string.IsNullOrEmpty(htmlText))
            {
                returnNodeList.AddRange(GetAllNodes(htmlText));
            }
            return returnNodeList;
        }

        private INode GetExpressionNode(string expression)
        {
            string baseString = expression.Split('.')[0].ToLower();
            if (baseString != "webaction" && baseString != "kresult")
            {
                expression = expression.ToLower();
            }
            //Hanlde the encoded html in expression
            return new ExpressionNode(decodeExpression(expression));
        }

        private string decodeExpression(string expression)
        {
            return !string.IsNullOrEmpty(expression) ? HttpUtility.HtmlDecode(expression) : expression;
        }

        private List<INode> GetAllNodes(string expression)
        {
            List<INode> returnList = new List<INode>();
            string[] splitExpressions = expression.Split("##k-");
            if (!string.IsNullOrEmpty(splitExpressions[0]))
            {
                returnList.Add(new TextNode(splitExpressions[0]));
            }
            for (int i = 1; i < splitExpressions.Length; i++)
            {
                string[] exps = splitExpressions[i].Split("##");
                int.TryParse(exps[0], out int blockNumber);
                returnList.Add(blockNodeReferenceList[blockNumber]);
                if (exps.Count() > 1)
                {
                    returnList.Add(new TextNode(exps[1]));
                }
            }
            return returnList;
        }
        #endregion
    }
}
