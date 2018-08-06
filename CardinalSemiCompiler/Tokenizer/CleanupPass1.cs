using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public partial class TokenStreamCleaner
    {
        private Token[] CleanupPass1(Token[] tknArr)
        {
            Queue<Token> tkns = new Queue<Token>();
            Queue<Token> inTkns = new Queue<Token>(tknArr);

            while (inTkns.Count > 0)
            {
                Token curTkn = inTkns.Dequeue();

                //Detect and resolve string literal types
                if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.StringLiteral && curTkn.TokenType == TokenType.Operator)
                {
                    var nTkn = inTkns.Peek();
                    if (curTkn.TokenValue == "$")
                    {
                        tkns.Enqueue(new Token(TokenType.InterpolatedStringLiteral, nTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                        inTkns.Dequeue();
                    }
                    else if (curTkn.TokenValue == "@")
                    {
                        tkns.Enqueue(new Token(TokenType.MultiLineStringLiteral, nTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                        inTkns.Dequeue();
                    }
                    else if (curTkn.TokenValue == "$@")
                    {
                        tkns.Enqueue(new Token(TokenType.InterpolatedMultiLineStringLiteral, nTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                        inTkns.Dequeue();
                    }
                }
                //Handle identifiers starting with '@'
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Identifier && curTkn.TokenType == TokenType.Operator && curTkn.TokenValue == "@")
                {
                    var nTkn = inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Identifier, "@" + nTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                else if(curTkn.TokenType == TokenType.OpeningAngle && inTkns.Peek().TokenType == TokenType.ConditionalOperator && inTkns.Peek().TokenValue == "<=")
                {
                    var nTkn = inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.AssignmentOperator, "<<=", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                else if(curTkn.TokenType == TokenType.ClosingAngle && inTkns.Peek().TokenType == TokenType.ConditionalOperator && inTkns.Peek().TokenValue == ">=")
                {
                    var nTkn = inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.AssignmentOperator, ">>=", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                else
                {
                    tkns.Enqueue(curTkn);
                }
            }

            return tkns.ToArray();
        }
    }
}
