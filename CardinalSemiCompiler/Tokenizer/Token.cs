using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public enum TokenType
    {
        Unknown,
        Keyword,
        Identifier,
        Preprocessor,
        Operator,

        StringLiteral,
        MultiLineStringLiteral,
        InterpolatedStringLiteral,
        InterpolatedMultiLineStringLiteral,

        CharLiteral,
        IntegerLiteral,
        HexLiteral,
        BinaryLiteral,

        Semicolon,
        OpeningBracket,
        ClosingBracket,
        OpeningBrace,
        ClosingBrace,
        ClosingParen,
        OpeningParen,
        Dot,
        Colon,
        Comma,
        Ampersand,
        At,
        Dollar,
        Asterisk,
        ForwardSlash,
        Pipe,
        Equal,
        Plus,
        Dash,
        Tick,
        Percentage,
        Exclamation,
        Tilda,
        Question,
        ClosingAngle,
        OpeningAngle,

        LineComment,
        BlockComment,
    }

    public class Token
    {
        public TokenType TokenType { get; private set; }
        public string TokenValue { get; private set; }

        public int StartPosition { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, string value, int startPos, int line, int col)
        {
            TokenType = type;
            TokenValue = value;
            StartPosition = startPos;
            Line = line;
            Column = col;
        }

        public override string ToString()
        {
            return $"({Line,4}, {Column,4}) Type = {TokenType, 35} | Value = {TokenValue}";
        }
    }
}
