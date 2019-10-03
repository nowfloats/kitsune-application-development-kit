using Kitsune.Models;
using System;
using System.Collections.Generic;

namespace Kitsune.Compiler.Helpers
{
    public class CompileResultHelper
    {

        /// <summary>
        /// Get CompileResult object for failure scenarios with single error message.
        /// </summary>
        /// <param name="errorMessage">Error message to be populated in CompileResult object</param>
        /// <param name="pageName">PageName to be populated in CompileResult object</param>
        /// <returns></returns>
        public static CompileResult GetErrorCompileResult(String errorMessage, String pageName)
        {
            List<CompilerError> errorList = new List<CompilerError> { GetCompileError(errorMessage) };
            return GetCompileResult(errorList, pageName, false);
        }


        /// <summary>
        /// Get CompileResult object for both failure and success scenarios with list of error messages.
        /// </summary>
        /// <param name="errorList">Error list to be populated in CompileResult Object</param>
        /// <param name="pageName">PageName to be populated in CompileResult object</param>
        /// <param name="success">success state to be set in CompileResult object</param>
        /// <param name="htmlString">CompiledString value, defaults to null if no parameter is passed</param>
        /// <returns></returns>
        public static CompileResult GetCompileResult(List<CompilerError> errorList,
                                                String pageName,
                                                Boolean success,
                                                String htmlString = null)
        {
            return new CompileResult
            {
                Success = success,
                CompiledString = htmlString,
                ErrorMessages = errorList,
                PageName = pageName,
            };
        }

        /// <summary>
        /// Get CompilerError object with errormessage, linenumber and position if available.
        /// </summary>
        /// <param name="errorMessage">error message to be populated in CompilerError object</param>
        /// <param name="lineNumber">line number of error, defaults to 1</param>
        /// <param name="position">position of error, defaults to 1</param>
        /// <returns></returns>
        public static CompilerError GetCompileError(String errorMessage, int lineNumber = 1, int position = 1)
        {
            return new CompilerError { LineNumber = lineNumber, LinePosition = position, Message = errorMessage };
        }

    }
}
