namespace Kitsune.Models.Nodes
{
    public class TextNode : INode
    {
        public string Text { get; private set; }

        public TextNode (string text)
        {
            NodeType = NodeType.textNode;
            this.Text = text;
        }
    }
}
