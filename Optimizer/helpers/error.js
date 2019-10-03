kerror = class {
    constructor(errorMessage, sourceMethod, errorStacktrace , resourceName,  lineNumber , columnNumber){
        this.Message = errorMessage;
        this.SourceMethod = sourceMethod;
        this.ErrorStackTrace = errorStacktrace;
        this.Line = lineNumber;
        this.Column = columnNumber;
        this.SourcePath = resourceName;
    }
}


module.exports = kerror;


