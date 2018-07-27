using CardinalSemiCompiler.Tokenizer;
using System;
using System.Runtime.Serialization;

namespace CardinalSemiCompiler.AST
{
    [Serializable]
    internal class SyntaxException : Exception
    {
        public SyntaxException()
        {
        }

        public SyntaxException(string message, Token tkn) : base($"({tkn.Line}, {tkn.Column}) " + message)
        {
        }

        public SyntaxException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SyntaxException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}