using System;
using System.Linq;
using CardinalSemiCompiler.Tokenizer;

namespace CardinalSemiCompiler.AST {
    public class NumberMath{
        #region int version
        public static ulong HandleBinaryMath(string op, ulong c0, ulong c1, bool c0_sgn, bool c1_sgn, out bool r_sgn){
            r_sgn = false;
            switch(op){
                case "+":
                    if(c0_sgn | c1_sgn){
                        long c0_l = (long)c0;
                        long c1_l = (long)c1;
                        long r_l = c0_l + c1_l;
                        if(r_l < 0)
                            r_sgn = true;
                        return (ulong)r_l;
                    }
                    return c0 + c1;
                case "-":
                    if(c0_sgn | c1_sgn){
                        long c0_l = (long)c0;
                        long c1_l = (long)c1;
                        long r_l = c0_l - c1_l;
                        if(r_l < 0)
                            r_sgn = true;
                        return (ulong)r_l;
                    }
                    return c0 - c1;
                case "/":
                    if(c0_sgn | c1_sgn){
                        long c0_l = (long)c0;
                        long c1_l = (long)c1;
                        long r_l = c0_l / c1_l;
                        if(r_l < 0)
                            r_sgn = true;
                        return (ulong)r_l;
                    }
                    return c0 / c1;
                case "*":
                    if(c0_sgn | c1_sgn){
                        long c0_l = (long)c0;
                        long c1_l = (long)c1;
                        long r_l = c0_l * c1_l;
                        if(r_l < 0)
                            r_sgn = true;
                        return (ulong)r_l;
                    }
                    return c0 * c1;
                case "%":
                    if(c0_sgn | c1_sgn){
                        long c0_l = (long)c0;
                        long c1_l = (long)c1;
                        long r_l = c0_l % c1_l;
                        if(r_l < 0)
                            r_sgn = true;
                        return (ulong)r_l;
                    }
                    return c0 % c1;
                case ">>":
                    return c0 >> (int)c1;
                case "<<":
                    return c0 << (int)c1;
                case "&":
                    return c0 & c1;
                case "^":
                    return c0 ^ c1;
                case "|":
                    return c0 | c1;
            }
            throw new Exception("Unrecognized operator.");
        }

        public static bool HandleBoolMath(string op, ulong c0, ulong c1, bool c0_sgn, bool c1_sgn){
            switch(op){
                case "<":
                    if(!c0_sgn && !c1_sgn)
                        return c0 < c1;
                    else if(c0_sgn && c1_sgn)
                        return c0 > c1;
                    else if(c0_sgn && !c1_sgn)
                        return true;
                    else //if(!c0_sgn && c1_sgn)
                        return false;
                case ">":
                    if(!c0_sgn && !c1_sgn)
                        return c0 > c1;
                    else if(c0_sgn && c1_sgn)
                        return c0 < c1;
                    else if(c0_sgn && !c1_sgn)
                        return false;
                    else //if(!c0_sgn && c1_sgn)
                        return true;
                case "<=":
                    if(!c0_sgn && !c1_sgn)
                        return c0 <= c1;
                    else if(c0_sgn && c1_sgn)
                        return c0 >= c1;
                    else if(c0_sgn && !c1_sgn)
                        return true;
                    else //if(!c0_sgn && c1_sgn)
                        return false;
                case ">=":
                    if(!c0_sgn && !c1_sgn)
                        return c0 >= c1;
                    else if(c0_sgn && c1_sgn)
                        return c0 <= c1;
                    else if(c0_sgn && !c1_sgn)
                        return false;
                    else //if(!c0_sgn && c1_sgn)
                        return true;
                case "==":
                    return c0 == c1 && c0_sgn == c1_sgn;
                case "!=":
                    return !(c0 == c1 && c0_sgn == c1_sgn);
            }
            throw new Exception("Unrecognized operator.");
        }

        public static ulong HandleUnaryMath(string op, ulong c0, bool c0_sgn, out bool r_sgn){
            r_sgn = c0_sgn;
            switch(op){
                case "+":
                    return c0;
                case "-":
                    r_sgn = !c0_sgn;
                    return c0;
                case "~":
                    return ~c0;
                case "++":
                    if(c0 + 1 >= 0)
                        r_sgn = false;
                    else
                        r_sgn = true;
                    return c0 + 1;
                case "--":
                    if(c0 - 1 >= 0)
                        r_sgn = false;
                    else
                        r_sgn = true;
                    return c0 - 1;
            }
            throw new Exception("Unrecognized operator.");
        }
        #endregion

        #region double version
        public static double HandleBinaryMath(string op, double c0, double c1){
            switch(op){
                case "+":
                    return c0 + c1;
                case "-":
                    return c0 - c1;
                case "/":
                    return c0 / c1;
                case "*":
                    return c0 * c1;
                case "%":
                    return c0 % c1;
            }
            throw new Exception("Unrecognized operator.");
        }

        public static bool HandleBoolMath(string op, double c0, double c1){
            switch(op){
                case "<":
                    return c0 < c1;
                case ">":
                    return c0 > c1;
                case "<=":
                    return c0 <= c1;
                case ">=":
                    return c0 >= c1;
                case "==":
                    return c0 == c1;
                case "!=":
                    return c0 != c1;
            }
            throw new Exception("Unrecognized operator.");
        }
        #endregion
    }
}