using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class IdeApiHelper
    {
        private static AssetChildren GenerateAssetTree(List<AssetChildren> children, string remainingPath, string filePath, bool isKitsune)
        {
            var search = remainingPath.TrimStart('/').IndexOf('/');
            if (remainingPath.StartsWith("/") && search != -1)
                search += 1;
            List<AssetChildren> firstCheck = null;
            string current, remaining;
            AssetChildren addObj;
            if (search != -1)
            {
                current = remainingPath.Substring(0, search);
                remaining = remainingPath.Substring(search + 1, (remainingPath.Length - search - 1));
            }
            else
            {
                current = remainingPath;
                remaining = "";
            }

            if (current != "")
            {
                firstCheck = children.Where(x => x.name == current.TrimStart('/'))?.ToList();
                if (firstCheck != null && firstCheck.Any())
                {
                    return GenerateAssetTree(firstCheck[0].children, remaining, filePath, isKitsune);
                }
                else
                {
                    addObj = new AssetChildren
                    {
                        name = current.TrimStart('/'),
                        Path = filePath.Substring(0, (filePath.IndexOf(current) + current.Length)),
                        children = new List<AssetChildren>(),
                        IsKitsune = isKitsune
                    };
                    children.Insert(0, addObj);
                    if (remaining != "")
                    {
                        return GenerateAssetTree(children[0].children, remaining, filePath, isKitsune);
                    }
                }
            }
            return null;
        }
        public static List<AssetChildren> OrderLeaves(List<AssetChildren> children)
        {
            if (children != null)
            {
                children = children.OrderByDescending(x => x.children.Count > 0 ? 1 : 0).ThenBy(x => x.name).ToList();

                foreach (var child in children)
                {
                    if (child.children != null && child.children.Any())
                        child.children = OrderLeaves(child.children);
                    else
                        child.children = null;
                }
            }
            return children;
        }
        public static AssetChildren ProjectResourceTree(List<string> resources, string projectName)
        {
            var final = new AssetChildren
            {
                children = new List<AssetChildren>(),
                name = projectName,
                toggled = false,
                IsKitsune = true
            };
            string data, filePath;

            for (var i = 0; i < resources.Count; i++)
            {
                data = resources[i];
                filePath = resources[i];
                GenerateAssetTree(final.children, filePath, filePath, true);
            }
            final.children = OrderLeaves(final.children);
            // RemoveEmptyChildren(final);
            return final;
        }
        public static string GenerateClassName(string pagename)
        {
            return (pagename.IndexOf('.') > 0 ? pagename.Substring(0, (pagename.IndexOf('.'))) : pagename).ToUpper().Replace('/', '_').Replace('-', '_').Replace('(', '_').Replace(')', '_').Replace('+', '_');
        }
    }
}
