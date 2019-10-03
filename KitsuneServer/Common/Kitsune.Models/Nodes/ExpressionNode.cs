namespace Kitsune.Models.Nodes
{
    public class ExpressionNode : INode
    {
        public string Expression { get; private set; }

        public ExpressionNode(string expression)
        {
            NodeType = NodeType.expressionNode;
            this.Expression = expression;
        }
    }
}
