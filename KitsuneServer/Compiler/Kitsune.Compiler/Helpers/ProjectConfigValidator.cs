//using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.Compiler.Helpers
{
    public class JsonValidationResult
    {
        public bool IsError { get; set; }
        public List<ValidationError> Errors { get; set; }
    }

    public class ProjectConfigValidator
    {
        private static ProjectConfigValidator JsonValidatorInsatnce=null;
        private static JsonSchema4 JsonSchema = null;

        private ProjectConfigValidator(string schema)
        {
            JsonSchema = JsonSchema4.FromJsonAsync(schema).Result;
        }
        private ProjectConfigValidator(Uri schemaUrl)
        {
            JsonSchema = JsonSchema4.FromUrlAsync(schemaUrl.AbsoluteUri).Result;
        }

        public static ProjectConfigValidator GetInstance(string schema)
        {
            if (String.IsNullOrEmpty(schema))
                throw new ArgumentNullException(nameof(schema));
            if(JsonValidatorInsatnce==null)
            {
                try
                {
                    JsonValidatorInsatnce = new ProjectConfigValidator(schema);
                    if (JsonValidatorInsatnce == null)
                        throw new Exception("Error Generating ProjectConfigValidatorObject");
                }
                catch(Exception ex)
                {
                    new Exception("Error Creating Schema Object of Project Configuration", ex);
                }
            }
            return JsonValidatorInsatnce;
        }
        public static ProjectConfigValidator GetInstance(Uri SchemaUri)
        {
            if (JsonValidatorInsatnce == null)
            {
                try
                {
                    JsonValidatorInsatnce = new ProjectConfigValidator(SchemaUri);
                    if (JsonValidatorInsatnce == null)
                        throw new Exception("Error Generating ProjectConfigValidatorObject");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error Creating Schema Object of Project Configuration", ex);
                }
            }
            return JsonValidatorInsatnce;
        }
        
        public JsonValidationResult ValidateJson(JToken jsonToValidate)
        {
            if (jsonToValidate==null)
                throw new ArgumentNullException(nameof(jsonToValidate));
            if (JsonValidatorInsatnce == null)
                throw new Exception("Json Validator's instance not initialised, Initialise JsonValidatorInsatnce with schema");

            ICollection<ValidationError> errors = JsonSchema.Validate(jsonToValidate);
            if (errors == null)
                throw new Exception("Error Validating JSON");

            return new JsonValidationResult { Errors=errors.ToList(), IsError=errors.Any()};
        }
    }
}
