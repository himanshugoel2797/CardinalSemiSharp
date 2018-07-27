using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardinalSemiCompiler.Tokenizer
{
    public partial class TokenStreamCleaner
    {
        public const int CleanupPassCount = 2;

        public Token[] Cleanup(Token[] tkns, int passNum)
        {
            if (passNum == 0)
                return CleanupPass0(tkns);
            else if (passNum == 1)
                return CleanupPass1(tkns);

            throw new ArgumentOutOfRangeException();
        }
    }
}
