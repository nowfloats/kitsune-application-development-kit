namespace Kitsune.Models.Nodes
{
    public enum NodeType { textNode, expressionNode, kRepeatNode, kShowNode, kHideNode, kNoRepeatNode, kScriptNode };

    public class INode
    {
        public NodeType NodeType { get; internal set; }
    }
}
