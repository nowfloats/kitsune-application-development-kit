using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitsune.Models.Project;
namespace Kitsune.Models
{

    public class ResourceCompilationResult
    {
        public bool Success { get; set; }
        public IEnumerable<CompilerError> ErrorMessages { get; set; }
        public List<MultiplePositionProperty> MetaProperty{ get; set; }
        public List<ObjectReference> MetaClass{ get; set; }
    }
   
    public class CompilerError
    {
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int LinePosition { get; set; }
        public CompilerError()
        {
            LineNumber = 1;
            LinePosition = 1;
        }
    }
  
    public class CompilerVersion : MongoEntity
    {
        public int Version { get; set; }
        public List<string> Changes { get; set; }
    }
    public class CompilerServiceSQSModel
    {
        public string ProjectId;
        public string UserEmail;
        public int BuildVersion;
        public CompilerServiceStatus Status;
    }
    public enum CompilerServiceStatus
    {
        Started,
        Compiled,
        Error = -1
    }

    public class CompileResult : ResourceCompilationResult
    {
        public string CompiledString { get; set; }
        public string PageName { get; set; }
        public Dictionary<string, int> CustomVariables { get; set; }
        public KitsunePage KitsunePage { get; set; }
    }
    public class ApplicationBuildQueueModel
    {
        public string ProjectId;
        public string UserEmail;
        public int BuildVersion;
        public string ApplicationId;
        public string ApplicationType;
        public string SourceLocation;
        public Dictionary<string, object> Configuration;
    }
}
