namespace Kitsune.Models.Nodes
{
    public class KRepeatNode : BlockNode
    {
        public string StartIndex { get; set;  } 
        public string EndIndex { get; set; }
        public string Iterator { get; private set; }
        public string Collection { get; private set; }
        public string CollectionAlias { get; private set; }

        public KRepeatNode(string startIndex, string endIndex, string iterator, string collection, string CollectionAlias = null)
        {
            NodeType = NodeType.kRepeatNode;
            this.StartIndex = startIndex;
            this.EndIndex = endIndex;
            this.Iterator = iterator;
            this.Collection = collection;
            this.CollectionAlias = CollectionAlias;
        }
    }
}
