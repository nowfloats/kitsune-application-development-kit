using System.Collections.Generic;
using System;
using Kitsune.Models;
using System.Linq;
using Kitsune.Helper;

namespace Kitsune.Compiler.Helpers
{
    public class MetaGraphBuilder
    {
        public static MetaGraphNode parentNode = new MetaGraphNode();
        public static MetaGraphNode resourceNode = new MetaGraphNode();

        //public bool IsObjectEqual(object from, object to)
        //{
        //    var haveSameData = false;
        //    foreach (PropertyInfo prop in from.GetType().GetProperties())
        //    {
        //        haveSameData = object.Equals(prop.GetValue(from, null), prop.GetValue(to, null));
        //        if (!haveSameData)
        //            break;
        //    }
        //    return haveSameData;
        //}

        public bool IsObjectEqual(Filter from, Filter to)
        {
            if (object.Equals(from.startIndex, to.startIndex) && object.Equals(from.endIndex, to.endIndex))
                return true;
            return false;
        }

        public bool CreatePartArrayGraph(List<ArrayRange> ranges, MetaGraphNode referenceNode, MetaGraphNode fileNode)
        {
            foreach (var range in ranges)
            {
                var rangeExist = referenceNode.Successors.FirstOrDefault(x => IsObjectEqual(x.filter, range.filter));
                if (rangeExist == null)
                {
                    referenceNode.Successors.Add(new MetaGraphNode(range.filter, referenceNode));
                    CreatePartMetaGraph(range.properties, referenceNode.Successors[referenceNode.Successors.Count - 1], fileNode);
                }
                else
                {
                    CreatePartMetaGraph(range.properties, rangeExist, fileNode);
                }
            }
            return true;
        }

        public bool CreatePartMetaGraph(List<ObjectReference> properties, MetaGraphNode referenceNode, MetaGraphNode fileNode)
        {
            if (properties == null || !properties.Any())
            {
                referenceNode.Successors.Add(fileNode);
                fileNode.Predecessors.Add(referenceNode);
                return true;
            }
            foreach (var property in properties)
            {
                var propertyExist = referenceNode.Successors.FirstOrDefault(x => x.name == property.name);
                if (propertyExist == null)
                {
                    referenceNode.Successors.Add(new MetaGraphNode(property.name, property.type, property.dataType, referenceNode));
                    if (property.type == metaPropertyType.array)
                        CreatePartArrayGraph(property.arrayRanges, referenceNode.Successors[referenceNode.Successors.Count - 1], fileNode);
                    else
                        CreatePartMetaGraph(property.properties, referenceNode.Successors[referenceNode.Successors.Count - 1], fileNode);
                }
                else
                {
                    if (property.type == metaPropertyType.array)
                        CreatePartArrayGraph(property.arrayRanges, propertyExist, fileNode);
                    else
                        CreatePartMetaGraph(property.properties, propertyExist, fileNode);
                }
            }
            return true;
        }

        public bool DeleteResourceMetaNode(MetaGraphNode referenceNode)
        {
            var preSuccessorNode = referenceNode.Predecessors.FirstOrDefault(x => x.Successors.Count == 1);
            if (preSuccessorNode != null)
            {
                DeleteResourceMetaNode(preSuccessorNode);
                preSuccessorNode.Successors.Remove(referenceNode);
                referenceNode.Predecessors.Remove(preSuccessorNode);
                return true;
            }
            else
            {
                var lastNode = referenceNode.Predecessors[0];
                lastNode.Successors.Remove(referenceNode);
                referenceNode.Predecessors.Remove(lastNode);
                return true;
            }
        }

        public MetaGraphNode CreateGraph(List<ResourcePropertyPosition> ResourceList, MetaGraphNode existingGraph = null)
        {
            try
            {
                parentNode = new MetaGraphNode(CompilerConstants.GraphParentNode, metaPropertyType.others);
                resourceNode = new MetaGraphNode(CompilerConstants.GraphResourceNode, metaPropertyType.others, parentNode);
                if (existingGraph == null)
                {
                    parentNode.Successors.Add(resourceNode);
                    foreach (var resource in ResourceList)
                    {
                        MetaGraphNode fileNode = new MetaGraphNode(resource.UrlPattern, metaPropertyType.resource, resourceNode);
                        resourceNode.Successors.Add(fileNode);
                        var result = CreatePartMetaGraph(resource.Properties, parentNode, fileNode);
                    }
                }
                else
                {
                    parentNode = existingGraph;
                    resourceNode = parentNode.Successors.FirstOrDefault(x => x.name == CompilerConstants.GraphResourceNode);
                    foreach (var resource in ResourceList)
                    {
                        var resNode = resourceNode.Successors.FirstOrDefault(x => x.name == resource.UrlPattern);
                        if (resNode == null)
                        {
                            MetaGraphNode fileNode = new MetaGraphNode(resource.UrlPattern, metaPropertyType.resource, resourceNode);
                            resourceNode.Successors.Add(fileNode);
                            var result = CreatePartMetaGraph(resource.Properties, parentNode, fileNode);
                        }
                        else
                        {
                            List<MetaGraphNode> propertyToRemove = new List<MetaGraphNode>();
                            foreach (var propertyNode in resNode.Predecessors)
                            {
                                if (propertyNode.type <= metaPropertyType.kstring)
                                {
                                    if (propertyNode.Successors.Count > 1)
                                    {
                                        propertyNode.Successors.Remove(resNode);
                                        propertyToRemove.Add(propertyNode);
                                    }
                                    else
                                    {
                                        DeleteResourceMetaNode(propertyNode);
                                        propertyNode.Successors.Remove(resNode);
                                        propertyToRemove.Add(propertyNode);
                                    }
                                }
                            }
                            foreach (var property in propertyToRemove)
                                resNode.Predecessors.Remove(property);
                            var result = CreatePartMetaGraph(resource.Properties, parentNode, resNode);
                        }
                    }

                }
                return parentNode;
            }
            catch (Exception ex)
            {
                //throw new Exception(string.Format("Faild to create reference Graph : {0}", ex.Message));
                return null;
            }
        }

    }
}
