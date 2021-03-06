﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public partial class Tokenizer
    {
        #region Keyword Tokens 
        private static string[] Keywords = new string[]
        {
            "abstract",
            "as",
            "atomic",

            "base",
            "bool",
            "break",
            "byte",

            "case",
            "catch",
            "char",
            "checked",
            "class",
            "const",
            "continue",

            "decimal",
            "default",
            "delegate",
            "do",
            "double",

            "else",
            "enum",
            "event",
            "explicit",
            "extern",

            "false",
            "finally",
            "fixed",
            "float",
            "for",
            "foreach",
            "func",

            "goto",
            "get",

            "if",
            "implicit",
            "in",
            "int",
            "interface",
            "internal",
            "is",
            "indexer",

            "lock",
            "long",

            "namespace",
            "new",
            "null",
            "nameof",

            "object",
            "operator",
            "out",
            "override",

            "params",
            "private",
            "protected",
            "public",
            "partial",
            "property",

            "readonly",
            "ref",
            "return",

            "sbyte",
            "sealed",
            "set",
            "short",
            "sizeof",
            "stackalloc",
            "static",
            "string",
            "struct",
            "switch",
            "this",
            "throw",
            "true",
            "try",
            "typeof",

            "uint",
            "ulong",
            "unchecked",
            "unsafe",
            "ushort",
            "using",

            "var",
            "virtual",
            "void",
            "volatile",
            "value",

            "while",
            "where",

            "yield",
        };

        public static bool IsKeyword(string v)
        {
            return Keywords.Contains(v);
        }
        #endregion

        #region Identifier Tokens
        public static bool IsIdentifier(string v)
        {
            char firstChar = v[0];

            if (char.IsLetter(firstChar))
            {
                for (int i = 1; i < v.Length; i++)
                    if (!char.IsLetterOrDigit(v[i]) && v[i] != '_')
                        return false;

                return true;
            }
            return false;
        }
        #endregion

        #region Integer Tokens
        private static string[] NumberDescs = new string[] { "l", "L", "f", "F", "d", "D", "m", "M", "u", "U", "ul", "UL" };
        public static bool IsInteger(string v)
        {
            bool continueParsing = false;
            string lChar = v.Last().ToString();
            string l2Char = v.Length > 1 ? v[v.Length - 2] + lChar : lChar;
            if (NumberDescs.Contains(lChar))
            {
                v = v.Substring(0, v.Length - 1);
                continueParsing = true;
            }
            else if (NumberDescs.Contains(l2Char))
            {
                v = v.Substring(0, v.Length - 2);
                continueParsing = true;
            }
            else if (char.IsDigit(v.Last()))
                continueParsing = true;

            if (!continueParsing)
                return false;

            if (long.TryParse(v, out long r_i))
                return true;
            else if (ulong.TryParse(v, out ulong r_u))
                return true;

            return false;
        }

        public static bool IsBinary(string v)
        {
            if (v[0] == '0' && (v[1] == 'b' || v[1] == 'B'))
            {
                if (v.Length == 2)
                {
                    //TODO: Submit an invalid number error
                    return false;
                }

                for (int i = 2; i < v.Length; i++)
                    if (v[i] != '0' && v[i] != '1' && v[i] != '_')
                        return false;

                return true;
            }

            return false;
        }

        public static bool IsHex(string v)
        {
            if (v[0] == '0' && (v[1] == 'x' || v[1] == 'X'))
            {
                if (v.Length == 2)
                {
                    //TODO: Submit an invalid number error
                    return false;
                }

                string validChars = "0123456789abcdefABCDEF";

                for (int i = 2; i < v.Length; i++)
                    if (!validChars.Contains(v[i]))
                        return false;

                return true;
            }

            return false;
        }
        #endregion

        #region Preprocessor Tokens
        private static string[] PreprocessorTokens = new string[]
        {
            "#region",
            "#endregion",
            "#define",
            "#if",
            "#else",
            "#elif",
            "#endif",
            "#error",
            "#line",
        };

        public static bool IsPreprocessor(string v)
        {
            return PreprocessorTokens.Contains(v);
        }
        #endregion
    }
}
