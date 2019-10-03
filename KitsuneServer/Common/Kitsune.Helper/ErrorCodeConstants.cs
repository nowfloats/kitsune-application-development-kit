namespace Kitsune.Helper
{
    public class ErrorCodeConstants
    {
        public static string BaseClassAsKObjectVariable = "variable is not allowed in k-object";
        public static string BaseClassAsKRepeatVariable = "variable is not allowed in k-repeat";
        public static string BaseClassAsKRepeatIterator = "iterator is not allowed in k-repeat";
        public static string ClassDoesNotExistInSchema = "'{0}' Class dose not exist in the schema";
        public static string CompilationError = "Compilation error, please check the error in the editor.";
        public static string FunctionUsedLikeProperty = "{0} is function but used like property";
        public static string InvalidKObjectSyntax = "Invalid Object tag";
        public static string InvalidKObjectClass = "Invalid k-object class";
        public static string InvalidKRepeatSyntax = "Invalid k-repeat format";
        public static string KRepeatInvalidVariable = "Invalid object to iterate";
        public static string KRepeatInvalidIterator = "Invalid iterator";
        public static string KRepeatVariableAlreadyInUse = "k-repeat variable already in use.";
        public static string KRepeatVariableNotArray = "Object to iterate on needs to be an array";
        public static string KScriptGetApiNoURL = "get-api attribute expects url";
        public static string KScriptMultipleApi = "k-script tag can have only one of get-api or post-api attribute";
        public static string KScriptNested = "k-script tag cannot be nested inside another k-script tag";
        public static string KScriptNoApi = "k-script tag needs either get-api or post-api attribute";
        public static string KScriptPostApiNoURL = "post-api attribute expects url";
        public static string NoDLTagInDLPage = "DL pages must specify k-dl attribute";
        public static string PropertyDoesNotExist = "Property {0} does not exist in the class ";
        public static string UnableToGetPreview = "Something went wrong, unable to get the preview";
        public static string UnrecognizedProperty = "Unrecognized property '{0}'";
        public static string UnrecognizedType = "Unrecognized type '{0}'";
        public static string UnrecognizedView = "Unrecognized view '{0}'";
        public static string UnrecognizedClass = "Unrecognized Class of type '{0}'";
        public static string InvalidKitsuneTagValue = "Invalid value for tag '{0}'";
        public static string InvalidFunctionName = "Invalid function name";
        public static string MissingUniqueIndexForDetailsPage = "Unique index paramater missing in k-dl for details view of '{0}'";

    }
}
