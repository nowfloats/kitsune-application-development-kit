using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class BaseErrorModel
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
    public class PropertyDetails
    {
        public string Property { get; set; }
        public int? StartRange { get; set; }
        public int? EndRange { get; set; }
        public List<List<int>> usage { get; set; }
        public List<PropertyDetails> SubProperty { get; set; }
    }
    public class ResourceMetaInfo : BaseErrorModel
    {
        //public List<PropertyDetails> Property { get; set; }
        public List<MultiplePositionProperty> Property { get; set; }
        public List<ObjectReference> metaClass { get; set; }
    }
    public class SinglePositionProperty
    {
        public string Property { get; set; }
        public position Position { get; set; }
    }
    public class ResourceMultiplePostionProperty
    {
        public string SourcePath { get; set; }
        public List<MultiplePositionProperty> Property { get; set; }
    }
    public class MultiplePositionProperty
    {
        public string Property { get; set; }
        public List<position> Positions { get; set; }
    }
    public class ResourcePropertyPosition
    {
        public string UrlPattern { get; set; }
        public string SourcePath { get; set; }
        public List<ObjectReference> Properties { get; set; }
    }
    public class MetaPropertyType
    {
        public string Property { get; set; }
        public List<MetaPartPropertyType> Type { get; set; }
    }
    public class MetaPartPropertyType
    {
        public string DataType { get; set; }
        public metaPropertyType Type { get; set; }
    }
    public class MetaWithClass : MultiplePositionProperty
    {
        public metaPropertyType datatype { get; set; }
    }
    public class position
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class range
    {
        public int start { get; set; }
        public int end { get; set; }
        public MultiplePositionProperty property { get; set; }
    }

    public class multiPropertyRange
    {
        public int start { get; set; }
        public int end { get; set; }
        public List<MultiplePositionProperty> properties { get; set; }
    }
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public enum metaPropertyType
    {
        str = 0,
        array = 1,
        number = 2,
        boolean = 3,
        date = 4,
        obj = 5,
        function = 6,
        kstring = 7,
        resource = 8,
        phonenumber = 9,
        others = -1
    }

    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class ObjectReference
    {
        public string name { get; set; }
        public string dataType { get; set; }
        public metaPropertyType? type { get; set; }
        public List<ObjectReference> properties { get; set; }
        public List<ArrayRange> arrayRanges { get; set; }

    }
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class ArrayRange
    {
        public List<ObjectReference> properties { get; set; }
        public Filter filter { get; set; }
    }
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class Filter
    {
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public List<SortQuery> sort { get; set; }
    }
    [Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    public class SortQuery
    {
        public string property { get; set; }
        public int direction { get; set; }
    }
    [Serializable]
    [ProtoContract]
    public class MetaGraphNode
    {
        [ProtoMember(1)]
        public string name { get; set; }
        [ProtoMember(2)]
        public string dataType { get; set; }
        [ProtoMember(3)]
        public metaPropertyType? type { get; set; }
        [ProtoMember(4)]
        public Filter filter { get; set; }
        [ProtoMember(5, AsReference = true)]
        public List<MetaGraphNode> Predecessors { get; set; }
        [ProtoMember(6, AsReference = true)]
        public List<MetaGraphNode> Successors { get; set; }
        public MetaGraphNode()
        {
            Predecessors = new List<MetaGraphNode>();
            Successors = new List<MetaGraphNode>();
        }
        public MetaGraphNode(string name, metaPropertyType? type, string dataType = null, Filter filter = null) : this()
        {
            this.name = name;
            this.type = type;
            this.dataType = dataType;
            this.filter = filter;
        }
        public MetaGraphNode(string name) : this()
        {
            this.name = name;
        }
        public MetaGraphNode(string name, MetaGraphNode predecessor) : this()
        {
            this.name = name;
            this.Predecessors.Add(predecessor);
        }
        public MetaGraphNode(string name, metaPropertyType? type, MetaGraphNode predecessor) : this()
        {
            this.name = name;
            this.type = type;
            this.Predecessors.Add(predecessor);
        }
        public MetaGraphNode(Filter filter, MetaGraphNode predecessor) : this()
        {
            this.filter = filter;
            this.Predecessors.Add(predecessor);
        }
        public MetaGraphNode(string name, metaPropertyType? type, string dataType, MetaGraphNode predecessor) : this()
        {
            this.name = name;
            this.type = type;
            this.dataType = dataType;
            this.Predecessors.Add(predecessor);
        }
    }
}
