using System.Collections.Generic;

namespace Kitsune.Models.Nodes
{
    public class BlockNode : INode
    {
        public List<INode> Children { get; internal set; }

        public BlockNode()
        {
            Children = new List<INode>();
        }

        public void AddChild(INode node)
        {
            Children.Add(node);
        }
        public void AddChildRange(List<INode> nodes)
        {
            Children.AddRange(nodes);
        }
        public void RemoveChild(INode node)
        {
            if (Children.Contains(node))
            {
                Children.Remove(node);
            }
        }

        public List<INode> GetNodes()
        {
            return Children;
        }
    }
}
