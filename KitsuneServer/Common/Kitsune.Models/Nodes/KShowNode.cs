namespace Kitsune.Models.Nodes
{
    public class KShowNode : BlockNode
    {
        public string Expression { get; private set; }

        public KShowNode(string expression)
        {
            NodeType = NodeType.kShowNode;
            this.Expression = expression;
        }
    }
}
