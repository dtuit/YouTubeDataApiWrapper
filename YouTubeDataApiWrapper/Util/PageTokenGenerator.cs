using System;
using System.Collections.Generic;
using System.Linq;

namespace YouTubeDataApiWrapper.Util
{
    public class PageTokenGenerator
    {
        public int TokenToNumber(string token)
        {
            return new PageTokenObject(token).PageTokenInteger;
        }

        public string NumberToToken(int number)
        {
            return new PageTokenObject(number).PageToken;
        }

        public string NumberToPrevToken(int number)
        {
            return new PageTokenObject(number, isPrevious: true).PageToken;
        }

        public PageTokenObject GetTokenObject(int number, bool isPrevious = false)
        {
            return new PageTokenObject(number, isPrevious);
        }

        public PageTokenObject GetTokenObject(string token)
        {
            return new PageTokenObject(token);
        }
    }

    /// <summary>
    /// Enables the transformation of YouTube Api page tokens to and from their integer form.
    /// property <see cref="PageToken"/> Represents the pageToken
    /// property <see cref="PageTokenInteger"/> represents the integer form of the pageToken
    /// <!--
    /// A PageToken Consists of
    /// "C" + CHAR_16 + CHAR_1 + (optional)[CHAR_128 + CHAR_65536] + CHAR_16384 + direction
    /// 
    /// Base 64 Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_"
    /// 
    /// Where CHAR_1 represents the number of 1s in the number
    ///       CHAR_16, the number of 16ths etc.
    /// 
    /// All CHAR_* are taken from subsequences of the base 64 alphabet,
    ///      the sequence changes on different intervals for each position.
    /// 
    /// CHAR_1 :  
    ///     base : 16 (steps every 16)
    ///     alpahbets : 'AEIMQUYcgkosw048', 'BFJNRVZdhlptx159', 'CGKOSWaeimquy26-', 'DHLPTXbfjnrvz37_' (every fourth in b64 + offset[0,1,2,3])
    /// 
    /// CHAR_16 :
    ///     base : 8 (steps every 16)
    ///     alpahbets : 'ABCDEFGH' , 'IJKLMNOP'
    /// 
    /// CHAR_128
    ///     base : 64 (steps every 128)
    /// 
    /// CHAR_16384
    ///     base : 4 (steps every 16384)
    ///     alphabets : 'BRhx' (every 16th + 1) (special cases "Q" and "E")
    /// 
    /// CHAR_65536
    ///     base : 64 (steps every 65536)
    /// 
    /// WEIGHT_8192 is used to calculate the offset for CHAR_1, 
    /// -->
    /// </summary>
    public class PageTokenObject
    {
        public readonly string PageToken;
        public readonly int PageTokenInteger;

        private readonly Base64Alphabet _b64 = new Base64Alphabet();
        private readonly int[] _weights = { 65536, 16384, 8192, 128, 16, 1 };

        private readonly int
            _weight1,
            _weight16,
            _weight128,
            _weight8192,
            _weight16384,
            _weight65536;

        private readonly char 
            _char1,
            _char16,
            _char128,
            _char16384, // Suffix character
            _char65536;

        private const char Prefix = 'C';
        private const string NextToken = "AA";
        private const string PrevToken = "A_";

        public bool IsPreviousPageToken;

        public PageTokenObject(string token)
        {
            if ( _b64.HasIllegalChars(token)  || token.Length < 6 )
                throw new ArgumentException("Not A Valid PageToken");

            //Remove suffix and filler and previous/next tags
            var t = token.Remove(0, 1);
            if (t[t.Length - 1] != 'A')
                IsPreviousPageToken = true;
            t = t.Remove(t.Length - 2);

            //Get Suffix Char and Remove it
            _char16384 = t[t.Length - 1];
            t = t.Remove(t.Length - 1);

            _char1 = t[1];
            _char16 = t[0];

            if (t.Length > 2)
                _char128 = t[2];

            if (t.Length > 3)
                _char65536 = t[3];

            var pos = _b64.GetIndexOf(_char1);
            _weight1 = (pos - (pos % 4)) / 4; // Converts these sequences to [0,..,16]. 'AEIMQUYcgkosw048', 'BFJNRVZdhlptx159', 'CGKOSWaeimquy26-', 'DHLPTXbfjnrvz37_'

            if (pos % 4 == 3 || pos % 4 == 1) // every odd 8192,
                _weight8192 = 1;

            pos = _b64.GetIndexOf(_char16);
            _weight16 = pos % 8;

            if (t.Length > 2)
            {
                pos = _b64.GetIndexOf(_char128);
                _weight128 = pos;
            }
            if (t.Length > 3)
            {
                pos = _b64.GetIndexOf(_char16384);
                _weight16384 = (pos - 1) / 16;
                _weight65536 = _b64.GetIndexOf(_char65536);
            }
            this.PageTokenInteger = this.AsNumber();
            this.PageToken = this.AsToken();
        }

        public PageTokenObject(int index, bool isPrevious = false)
        {
            if (index < 0)
            {
                index = 0;
            }

            IsPreviousPageToken = isPrevious;

            int[] x = GetWeights(_weights, index);
            _weight1 = x[5];
            _weight16 = x[4];
            _weight128 = x[3];
            _weight8192 = x[2];
            _weight16384 = x[1];
            _weight65536 = x[0];

            var b8Offset = index < 128 ? 0 : 8;
            var b16Offset = (_weight8192 % 2) + 2; // 2,3,2,3 ...

            var suffixPos = (_weight16384 * 16 + 1) % 64; // [B,R,h,x]
            _char16384 = _b64.GetCharAt(suffixPos);

            if (index < 16384)
            {
                _char16384 = 'E';
                b16Offset = 1;
            }
            if (index < 8192)
            {
                b16Offset = 0;
            }
            if (index < 128)
            {
                _char16384 = 'Q';
            }

            _char1 = _b64.GetCharAt(_weight1 * 4 + b16Offset);
            _char16 = _b64.GetCharAt(_weight16 + b8Offset);
            _char128 = index >= 128 ? _b64.GetCharAt(_weight128) : default(char);
            _char65536 = index >= 16384 ? _b64.GetCharAt(_weight65536) : default(char);

            this.PageTokenInteger = this.AsNumber();
            this.PageToken = this.AsToken();
        }

        private int[] GetWeights(IReadOnlyList<int> weights, int num)
        {
            int n = num;
            int[] res = new int[weights.Count];

            for (int i = 0; i < weights.Count; i++)
            {
                int r, q = Math.DivRem(n, weights[i], out r);
                res[i] = q;
                n = r;
            }
            return res;
        }

        private int AsNumber()
        {
            var retval = 1 * _weight1
                    + 16 * _weight16
                    + 128 * _weight128
                    + 8192 * _weight8192
                    + 16384 * _weight16384
                    + 65536 * _weight65536;

            //return isPreviousPageToken ? retval + 1 : retval;
            return retval;
        }

        private string AsToken()
        {
            return String.Format("{0}{1}{2}{3}{4}{5}{6}", new object[] {
                    Prefix,
                    _char16,
                    _char1,
                    _char128,
                    _char65536,
                    _char16384,
                    IsPreviousPageToken ? PrevToken : NextToken }).Replace("\0", String.Empty);
        }
    }

    public class Base64Alphabet
    {
        public const string b64Symbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        public bool HasIllegalChars(string token)
        {
            return token.Any(c => b64Symbols.Contains(c) == false);
        }

        public int GetIndexOf(char c)
        {
            return b64Symbols.IndexOf(c);
        }

        public char GetCharAt(int i)
        {
            if (i > b64Symbols.Length)
                throw new IndexOutOfRangeException();
            return b64Symbols[i];
        }

    }
}
