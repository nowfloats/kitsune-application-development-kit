using Kitsune.Models.Nodes;
using System.Collections.Generic;

namespace Kitsune.Models
{
    public class KitsunePage
    {
        public string SourcePath { get; private set; }
        public string CollectionIdentifier { get; set; }
        public string Collection { get; set; }
        public string Offset { get; set; }
        public Dictionary<string, int> CustomVariables { get; private set; }

        public List<INode> Nodes { get; private set; }

        public KitsunePage(string sourcePath, string kObject, string offset, string collection = null, string collectionIdentifier = null)
        {
            this.SourcePath = sourcePath;
            this.Offset = offset;
            this.Collection = collection;
            this.CollectionIdentifier = collectionIdentifier;
            if (!string.IsNullOrEmpty(kObject))
            {
                var kobjectParams = kObject.Split(':');
                if(kobjectParams.Length == 2)
                {
                    Collection = kobjectParams[1];
                    CollectionIdentifier = kobjectParams[0];
                }
                else
                {
                    Collection = kObject;
                }
            }
            CustomVariables = new Dictionary<string, int>();
            Nodes = new List<INode>();
        }

        public int AddCustomVariable(string key, int value)
        {
            if (CustomVariables.ContainsKey(key))
            {
                return -1;
            }
            CustomVariables.Add(key, value);
            return 1;
        }

        public void AddNode(INode node)
        {
            Nodes.Add(node);
        }

        public void AddNodes(List<INode> nodeList)
        {
            Nodes.AddRange(nodeList);
        }
    }
}
