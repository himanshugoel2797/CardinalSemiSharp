using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public partial class Tokenizer
    {
        public Token[] Tokenize(string code)
        {
            List<Token> tkns = new List<Token>();
            char[] delims = new char[] { ';', '(', ')', '[', ']', '{', '}', ':', '.', ',', '$', '@', '&', '*', '~', '!', '%', '^', '-', '+', '=', '|', '/', '?', '<', '>' };
            TokenType[] delims_types = new TokenType[]
            {
                TokenType.Semicolon,
                TokenType.OpeningParen,
                TokenType.ClosingParen,
                TokenType.OpeningBracket,
                TokenType.ClosingBracket,
                TokenType.OpeningBrace,
                TokenType.ClosingBrace,
                TokenType.Colon,
                TokenType.Dot,
                TokenType.Comma,
                TokenType.Dollar,
                TokenType.At,
                TokenType.Ampersand,
                TokenType.Asterisk,
                TokenType.Tilda,
                TokenType.Exclamation,
                TokenType.Percentage,
                TokenType.Tick,
                TokenType.Dash,
                TokenType.Plus,
                TokenType.Equal,
                TokenType.Pipe,
                TokenType.ForwardSlash,
                TokenType.Question,
                TokenType.OpeningAngle,
                TokenType.ClosingAngle,
            };

            char[] skipChars = new char[] { '\r', '\n' };
            char[] stringEscapeChars = new char[] { '\\' };
            char[] stringEscapeTypes = new char[] { 't', 'b', 'n', 'r' };

            string curStr = "";
            int tknStartPos = -1;
            int tknStartLine = -1;
            int tknStartCol = -1;
            TokenType tknType = TokenType.Unknown;

            bool isString = false;
            bool isChar = false;
            bool isEscapeChar = false;

            int pos = 0;
            int line = 0;
            int column = 0;
            do
            {
                if ((char.IsWhiteSpace(code[pos]) || delims.Contains(code[pos])) && !isString && !isChar)
                {
                    if (!string.IsNullOrWhiteSpace(curStr))
                    {
                        //Finish the token
                        if (tknType == TokenType.Unknown)
                        {
                            //Determine the token type from the full string
                            if (IsKeyword(curStr))
                                tknType = TokenType.Keyword;
                            else if (IsPreprocessor(curStr))
                                tknType = TokenType.Preprocessor;
                            else if (IsInteger(curStr))
                                tknType = TokenType.IntegerLiteral;
                            else if (IsHex(curStr))
                                tknType = TokenType.HexLiteral;
                            else if (IsBinary(curStr))
                                tknType = TokenType.BinaryLiteral;
                            else if (IsIdentifier(curStr))
                                tknType = TokenType.Identifier;
                            else
                            {
                                //TODO: unknown token error
                            }
                        }

                        Token tkn = new Token(tknType, curStr, tknStartPos, tknStartLine, tknStartCol);
                        tkns.Add(tkn);

                        curStr = "";
                        tknStartPos = -1;
                        tknType = TokenType.Unknown;
                    }

                    //Generate delimiter tokens
                    if (delims.Contains(code[pos]))
                        for (int i = 0; i < delims.Length; i++)
                            if (delims[i] == code[pos])
                            {
                                tkns.Add(new Token(delims_types[i], delims[i].ToString(), pos, line, column));
                                break;
                            }
                }
                else if (stringEscapeChars.Contains(code[pos]) && (isString | isChar))
                {
                    isEscapeChar = true;
                    curStr += code[pos];
                }
                else if (stringEscapeTypes.Contains(code[pos]) && isEscapeChar && (isString | isChar))
                {
                    //Handle the character
                    isEscapeChar = false;
                    curStr += code[pos];

                    //TODO: Handle unicode characters
                }
                else if (code[pos] == '"' && !isEscapeChar && !isChar)
                {
                    //Build string token
                    if (isString)
                        tknType = TokenType.StringLiteral;
                    isString = !isString;
                }
                else if (code[pos] == '\'' && !isEscapeChar && !isString)
                {
                    //Build string token
                    if (isChar)
                        tknType = TokenType.CharLiteral;
                    isChar = !isChar;
                }
                else if (skipChars.Contains(code[pos]) && !isString && !isChar)
                {
                    //Skip these characters
                }
                else
                {
                    //Build token as a default
                    if (tknStartPos == -1)
                    {
                        tknStartPos = pos;
                        tknStartLine = line;
                        tknStartCol = column;
                    }
                    curStr += code[pos];
                }

                column++;
                if (code[pos] == '\n' && !isString)
                {
                    line++;
                    column = 0;
                }
                pos++;
            }
            while (pos < code.Length);

            return tkns.ToArray();
        }
    }
}
