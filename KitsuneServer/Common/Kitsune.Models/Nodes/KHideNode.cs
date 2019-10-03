namespace Kitsune.Models.Nodes
{
    public class KHideNode : BlockNode
    {
        public string Expression { get; private set; }

        public KHideNode(string expression)
        {
            NodeType = NodeType.kHideNode;
            this.Expression = expression;
        }
    }
}
