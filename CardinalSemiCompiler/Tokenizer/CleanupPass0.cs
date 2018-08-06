using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public partial class TokenStreamCleaner
    {
        public bool StripLineComments { get; set; }
        public bool StripBlockComments { get; set; }

        private Token[] CleanupPass0(Token[] tknArr)
        {
            Queue<Token> tkns = new Queue<Token>();
            Queue<Token> inTkns = new Queue<Token>(tknArr);

            while (inTkns.Count > 0)
            {
                Token curTkn = inTkns.Dequeue();

                //Detect and coalesce all unambigious multi-char operators
                if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Equal)
                    switch (curTkn.TokenType)
                    {
                        case TokenType.Percentage:
                        case TokenType.Tick:
                        case TokenType.Ampersand:
                        case TokenType.Asterisk:
                        case TokenType.Dash:
                        case TokenType.Plus:
                        case TokenType.Pipe:
                        case TokenType.ForwardSlash:
                            {
                                inTkns.Dequeue();
                                tkns.Enqueue(new Token(TokenType.AssignmentOperator, curTkn.TokenValue + "=", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                            }
                            break;
                        case TokenType.Exclamation:
                        case TokenType.OpeningAngle:
                        case TokenType.ClosingAngle:
                        case TokenType.Equal:
                            {
                                inTkns.Dequeue();
                                tkns.Enqueue(new Token(TokenType.ConditionalOperator, curTkn.TokenValue + "=", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                            }
                            break;
                        default:
                            tkns.Enqueue(curTkn);
                            break;
                    }
                //Handle '--' operator
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Dash && curTkn.TokenType == TokenType.Dash)
                {
                    inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "--", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '++' operator
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Plus && curTkn.TokenType == TokenType.Plus)
                {
                    inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "++", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '&&' operator
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Ampersand && curTkn.TokenType == TokenType.Ampersand)
                {
                    inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "&&", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '||' operator
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Pipe && curTkn.TokenType == TokenType.Pipe)
                {
                    inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "||", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '||' operator
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.ClosingAngle && curTkn.TokenType == TokenType.Equal)
                {
                    inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "=>", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '$@' header for string literals
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.At && curTkn.TokenType == TokenType.Dollar)
                {
                    var nTkn = inTkns.Dequeue();
                    tkns.Enqueue(new Token(TokenType.Operator, "$@", curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '//' header for line comments
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.ForwardSlash && curTkn.TokenType == TokenType.ForwardSlash)
                {
                    inTkns.Dequeue();
                    string cmnt = "";
                    while (inTkns.Count > 0 && inTkns.Peek().Line == curTkn.Line)
                    {
                        cmnt += inTkns.Dequeue().TokenValue;
                        cmnt += " ";
                    }
                    if (!StripLineComments)
                        tkns.Enqueue(new Token(TokenType.LineComment, cmnt, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                //Handle '/*' header for line comments
                else if (inTkns.Count > 1 && inTkns.Peek().TokenType == TokenType.Asterisk && curTkn.TokenType == TokenType.ForwardSlash)
                {
                    inTkns.Dequeue();
                    string cmnt = "";
                    while (inTkns.Count > 0)
                    {
                        var cTkn = inTkns.Dequeue();
                        if (inTkns.Count > 1 && cTkn.TokenType == TokenType.Asterisk && inTkns.Peek().TokenType == TokenType.ForwardSlash)
                        {
                            inTkns.Dequeue();
                            break;
                        }
                        cmnt += cTkn.TokenValue;
                        cmnt += " ";

                    }

                    if (!StripBlockComments)
                        tkns.Enqueue(new Token(TokenType.BlockComment, cmnt, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                }
                else
                {
                    //Coalesce all unambigious single-char operators
                    switch (curTkn.TokenType)
                    {
                        case TokenType.Exclamation:
                        case TokenType.Percentage:
                        case TokenType.Tick:
                        case TokenType.Ampersand:
                        case TokenType.Asterisk:
                        case TokenType.Dash:
                        case TokenType.Plus:
                        case TokenType.Pipe:
                        case TokenType.ForwardSlash:
                        case TokenType.Dollar:
                        case TokenType.At:
                            tkns.Enqueue(new Token(TokenType.Operator, curTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                            break;
                        case TokenType.Equal:
                            tkns.Enqueue(new Token(TokenType.AssignmentOperator, curTkn.TokenValue, curTkn.StartPosition, curTkn.Line, curTkn.Column));
                            break;
                        default:
                            tkns.Enqueue(curTkn);
                            break;
                    }
                }
            }

            return tkns.ToArray();
        }
    }
}
