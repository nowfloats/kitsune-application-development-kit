namespace Kitsune.Models.Nodes
{
    public class KScriptNode : BlockNode
    {
        public string API { get; private set; }
        public string Input { get; private set; }
        public string Headers { get; private set; }
        public dynamic Response { get; internal set; }
        public string IsCacheEnabledString { get; internal set; }
        public bool IsCacheEnabled { get; internal set; }

        public KScriptNode(string api, string input, string headers, string isCacheEnabledStr)
        {
            NodeType = NodeType.kScriptNode;
            this.API = api;
            this.Input = input;
            this.Headers = headers;
            this.IsCacheEnabledString = isCacheEnabledStr;
        }
    }
}
