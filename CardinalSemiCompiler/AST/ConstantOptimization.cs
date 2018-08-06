using System;
using System.Linq;
using CardinalSemiCompiler.Tokenizer;

namespace CardinalSemiCompiler.AST {
    public class ConstantOptimization {
        public static void Optimize(SyntaxNode root){
            root = OptimizeNode(root);
        }
        private static string[] NumberDescs = new string[] { "l", "L", "f", "F", "d", "D", "m", "M", "u", "U", "ul", "UL" };
        private static void ParseNum(string c0, TokenType c0_t, out ulong hex_c0, out bool neg_c0){
            hex_c0 = 0;
            neg_c0 = false;

            if(c0_t == TokenType.HexLiteral)
                hex_c0 = Convert.ToUInt64(c0, 16);

            if(c0_t == TokenType.BinaryLiteral){
                hex_c0 = Convert.ToUInt64(c0, 2);
            }

            if(c0_t == TokenType.IntegerLiteral){

                /*if(c0.First() == '-'){
                    neg_c0 = true;
                    c0 = c0.Substring(1, c0.Length - 1);
                }else if(c0.First() == '+'){
                    neg_c0 = false;
                    c0 = c0.Substring(1, c0.Length - 1);
                }*/

                string lChar = c0.Last().ToString();
                string l2Char = c0.Length > 1 ? c0[c0.Length - 2] + lChar : lChar;
                if (NumberDescs.Contains(lChar))
                {
                    c0 = c0.Substring(0, c0.Length - 1);
                    hex_c0 = UInt64.Parse(c0);
                }
                else if (NumberDescs.Contains(l2Char))
                {
                    c0 = c0.Substring(0, c0.Length - 2);
                    hex_c0 = UInt64.Parse(c0);
                }else if(char.IsDigit(lChar[0])){
                    hex_c0 = UInt64.Parse(c0);
                }else if(char.IsDigit(l2Char[0])){
                    hex_c0 = UInt64.Parse(c0);
                }
            }

        }
        private static string HandleNumberMath(string op, string c0, string c1, TokenType c0_t, TokenType c1_t){
            ulong hex_c0 = 0;
            ulong hex_c1 = 0;
            bool neg_c0 = false;
            bool neg_c1 = false;

            ParseNum(c0, c0_t, out hex_c0, out neg_c0);
            ParseNum(c1, c1_t, out hex_c1, out neg_c1);

            ulong r_val = NumberMath.HandleBinaryMath(op, hex_c0, hex_c1, neg_c0, neg_c1, out bool r_sgn);
            if(r_sgn)
                return "-" + r_val.ToString();
            else
                return r_val.ToString();
        }

        private static double ParseDouble(string c0){
            if(c0.Last() == 'f' | c0.Last() == 'd')
                c0 = c0.Substring(0, c0.Length - 1);
            return double.Parse(c0);
        }

        private static string HandleFloatMath(string op, string c0, string c1, TokenType c0_t, TokenType c1_t){
            double d_c0 = ParseDouble(c0);
            double d_c1 = ParseDouble(c1);

            string term = c0.Last().ToString();
            if(char.IsDigit(term[0])){
                term = "d";
            }else{
                if(term != "d"){
                    term = c1.Last().ToString();
                    if(char.IsDigit(term[0]))
                        term = "d";
                }
            }

            return NumberMath.HandleBinaryMath(op, d_c0, d_c1).ToString() + term;
        }

        private static SyntaxNode OptimizeNode(SyntaxNode n){
            for(int i = 0; i < n.ChildNodes.Count; i++){
                n.ChildNodes[i] = OptimizeNode(n.ChildNodes[i]);
            }

            if(n is OperatorSyntaxNode){
                var n_op = n as OperatorSyntaxNode;

                TokenType[] tkn_int_types = new TokenType[] { TokenType.IntegerLiteral, TokenType.HexLiteral, TokenType.BinaryLiteral };
                TokenType[] tkn_flt_types = new TokenType[] { TokenType.FloatLiteral };
                TokenType[] tkn_str_types = new TokenType[] { TokenType.StringLiteral };
                string[] arithmetic_ops = new string[] {"+", "-", "*", "/", "%"};

                if(n_op.NodeType != SyntaxNodeType.UnaryNode){
                    var n_c0 = n_op.ChildNodes[0];
                    if(n_c0.NodeType != SyntaxNodeType.ConstantNode)
                        return n;

                    for(int i = 0; i < n_op.Operator.Count; i++){
                        var n_c1 = n_op.ChildNodes[i + 1];

                        if(arithmetic_ops.Contains(n_op.Operator[i].TokenValue)){
                            if(n_c0.Token.TokenType == TokenType.FloatLiteral && n_c1.Token.TokenType == TokenType.IntegerLiteral){
                                n_c1.Token.TokenType = TokenType.FloatLiteral;
                            }
                        
                            if(n_c0.Token.TokenType == TokenType.IntegerLiteral && n_c1.Token.TokenType == TokenType.FloatLiteral){
                                n_c0.Token.TokenType = TokenType.FloatLiteral;
                            }
                        }

                        if(n_c1.NodeType == SyntaxNodeType.ConstantNode)
                            switch(n_op.Operator[i].TokenValue){
                                case "+":
                                    {
                                        var n_val = "";
                                        if(tkn_int_types.Contains(n_c0.Token.TokenType) && tkn_int_types.Contains(n_c1.Token.TokenType)){
                                            n_val = HandleNumberMath(n_op.Operator[i].TokenValue, n_c0.Token.TokenValue, n_c1.Token.TokenValue, n_c0.Token.TokenType, n_c1.Token.TokenType);
                                        }else if(tkn_flt_types.Contains(n_c0.Token.TokenType) && tkn_flt_types.Contains(n_c1.Token.TokenType)){
                                            n_val = HandleFloatMath(n_op.Operator[i].TokenValue, n_c0.Token.TokenValue, n_c1.Token.TokenValue, n_c0.Token.TokenType, n_c1.Token.TokenType);
                                        }else if(tkn_str_types.Contains(n_c1.Token.TokenType)){
                                            n_val = n_c0.Token.TokenValue + n_c1.Token.TokenValue;
                                        }else break;

                                        var n_r = new SyntaxNode(SyntaxNodeType.ConstantNode, new Token(n_c0.Token, n_val));
                                        n_c0 = n_r;
                                        n_op.ChildNodes[0] = n_c0;
                                        n_op.ChildNodes[i + 1] = null;
                                        n_op.Operator[i] = null;
                                    }
                                    break;
                                case "-":
                                case "*":
                                case "/":
                                case "%":
                                    {
                                        var n_val = "";
                                        if(tkn_int_types.Contains(n_c0.Token.TokenType) && tkn_int_types.Contains(n_c1.Token.TokenType)){
                                            n_val = HandleNumberMath(n_op.Operator[i].TokenValue, n_c0.Token.TokenValue, n_c1.Token.TokenValue, n_c0.Token.TokenType, n_c1.Token.TokenType);
                                        }else if(tkn_flt_types.Contains(n_c0.Token.TokenType) && tkn_flt_types.Contains(n_c1.Token.TokenType)){
                                            n_val = HandleFloatMath(n_op.Operator[i].TokenValue, n_c0.Token.TokenValue, n_c1.Token.TokenValue, n_c0.Token.TokenType, n_c1.Token.TokenType);
                                        }else break;

                                        var n_r = new SyntaxNode(SyntaxNodeType.ConstantNode, new Token(n_c0.Token, n_val));
                                        n_c0 = n_r;
                                        n_op.ChildNodes[0] = n_c0;
                                        n_op.ChildNodes[i + 1] = null;
                                        n_op.Operator[i] = null;
                                    }
                                    break;
                                case ">>":
                                case "<<":
                                case "&":
                                case "^":
                                case "|":
                                    {
                                        var n_val = "";
                                        if(tkn_int_types.Contains(n_c0.Token.TokenType) && tkn_int_types.Contains(n_c1.Token.TokenType)){
                                            n_val = HandleNumberMath(n_op.Operator[i].TokenValue, n_c0.Token.TokenValue, n_c1.Token.TokenValue, n_c0.Token.TokenType, n_c1.Token.TokenType);
                                        }else if(tkn_flt_types.Contains(n_c0.Token.TokenType) | tkn_flt_types.Contains(n_c1.Token.TokenType))
                                            throw new SyntaxException("Operator '" + n_op.Operator[i].TokenValue +"' is not defined for floating point/decimal.", n.Token);
                                        else break;

                                        var n_r = new SyntaxNode(SyntaxNodeType.ConstantNode, new Token(n_c0.Token, n_val));
                                        n_c0 = n_r;
                                        n_op.ChildNodes[0] = n_c0;
                                        n_op.ChildNodes[i + 1] = null;
                                        n_op.Operator[i] = null;
                                    }
                                    break;
                            }
                    }

                    for(int i = 0; i < n_op.Operator.Count; i++)
                        if(n_op.Operator[i] == null){
                            n_op.Operator.RemoveAt(i);
                            n_op.ChildNodes.RemoveAt(i + 1);
                            i--;
                        }

                    if(n_op.Operator.Count == 0)
                        return n.ChildNodes[0];
                }
            }

            if(n.ChildNodes.Count == 1 && n.ChildNodes[0].NodeType == SyntaxNodeType.ConstantNode && n.NodeType == SyntaxNodeType.SpecialStatement && (n.Token.TokenType == TokenType.OpeningParen | n.Token.TokenType == TokenType.IntegerLiteral | n.Token.TokenType == TokenType.FloatLiteral | n.Token.TokenType == TokenType.StringLiteral | n.Token.TokenType == TokenType.CharLiteral) ){
                n.NodeType = SyntaxNodeType.ConstantNode;
                n.Token.TokenValue = n.ChildNodes[0].Token.TokenValue;
                n.Token.TokenType = n.ChildNodes[0].Token.TokenType;
                n.ChildNodes.Clear();
            }

            return n;
        }
    }
}