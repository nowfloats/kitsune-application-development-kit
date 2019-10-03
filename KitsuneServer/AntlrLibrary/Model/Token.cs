using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntlrLibrary.Model
{
    public enum TOKENTYPE { Integer, String, Array, Delimiter, Comparator, Arithmatic, Object, BitOperator, Keyword, Double, Long, Float, ERROR, Boolean, Expression, Ternary, Function, NoData };
    public class Token
    {
        public Token()
        {
        }
        public Token(dynamic tokenValue, TOKENTYPE? tokenType)
        {
            Value = tokenValue;
            Type = tokenType;
        }
        public dynamic Value { get; set; }
        public TOKENTYPE? Type { get; set; }
        //evaluate
    }
}
