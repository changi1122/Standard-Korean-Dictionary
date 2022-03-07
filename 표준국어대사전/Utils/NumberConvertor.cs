using System;
using System.Text;


namespace 표준국어대사전.Utils
{
    internal class NumberConvertor
    {
        /// <summary>
        /// 위 첨자 유니코드 형태의 숫자 문자열을 일반 숫자 문자열로 변환합니다.
        /// </summary>
        /// <param name="sup_no">위 첨자 유니코드 숫자 문자열</param>
        /// <returns>일반 숫자 문자열</returns>
        public static string SupToNumber(string sup_no)
        {
            StringBuilder numString = new StringBuilder(sup_no);
            for (int i = 0; i < numString.Length; i++)
            {
                switch (numString[i])
                {
                    case '¹': numString[i] = '1'; break;
                    case '²': numString[i] = '2'; break;
                    case '³': numString[i] = '3'; break;
                    case '⁴': numString[i] = '4'; break;
                    case '⁵': numString[i] = '5'; break;
                    case '⁶': numString[i] = '6'; break;
                    case '⁷': numString[i] = '7'; break;
                    case '⁸': numString[i] = '8'; break;
                    case '⁹': numString[i] = '9'; break;
                    case '⁰': numString[i] = '0'; break;
                    case '⁻': numString[i] = '-'; break;
                    case '⁽': numString[i] = '('; break;
                    case '⁾': numString[i] = ')'; break;
                    case '⁺': numString[i] = '+'; break;
                    default: break;
                }
            }
            return numString.ToString();
        }

        /// <summary>
        /// 숫자를 위 첨자 유니코드 형태 문자열로 변환합니다.
        /// </summary>
        /// <param name="number">정수</param>
        /// <returns>위 첨자 유니코드 숫자 문자열</returns>
        public static string ToSup(int number)
        {
            return ToSup(number.ToString());
        }

        /// <summary>
        /// 숫자 문자열을 위 첨자 유니코드 형태 문자열로 변환합니다.
        /// </summary>
        /// <param name="number">숫자 문자열</param>
        /// <returns>위 첨자 유니코드 숫자 문자열</returns>
        public static string ToSup(string number)
        {
            StringBuilder numString = new StringBuilder(number);
            for (int i = 0; i < numString.Length; i++)
            {
                switch (numString[i])
                {
                    case '1': numString[i] = '¹'; break;
                    case '2': numString[i] = '²'; break;
                    case '3': numString[i] = '³'; break;
                    case '4': numString[i] = '⁴'; break;
                    case '5': numString[i] = '⁵'; break;
                    case '6': numString[i] = '⁶'; break;
                    case '7': numString[i] = '⁷'; break;
                    case '8': numString[i] = '⁸'; break;
                    case '9': numString[i] = '⁹'; break;
                    case '0': numString[i] = '⁰'; break;
                    case '-': case '－': numString[i] = '⁻'; break;
                    case '(': numString[i] = '⁽'; break;
                    case ')': numString[i] = '⁾'; break;
                    case '+': numString[i] = '⁺'; break;
                    default: break;
                }
            }
            return numString.ToString();
        }

        /// <summary>
        /// 숫자를 그리스 숫자 문자열로 변환합니다.
        /// </summary>
        /// <param name="number">정수</param>
        /// <returns>그리스 숫자 문자열</returns>
        public static string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value between 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900);
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }

        /// <summary>
        /// word에 어깨번호가 포함되어 있으면, 단어 이름과 어깨번호를 분리 후 word를 단어 이름으로, number를 숫자 문자열로 대입한다.
        /// </summary>
        /// <param name="word">단어 이름+어깨번호 -> 단어 이름</param>
        /// <param name="number">빈 문자열 -> 어깨번호</param>
        /// <returns>word가 어깨번호를 포함하는지 여부</returns>
        public static bool SplitWord(ref string word, ref string number)
        {
            int numberStartIndex;
            for (numberStartIndex = word.Length - 1; 0 <= numberStartIndex; numberStartIndex--)
            {
                if (!char.IsDigit(word[numberStartIndex]))
                    break;
            }

            if (numberStartIndex < word.Length - 1)
            {
                number = word.Substring(numberStartIndex + 1).TrimStart(new char[] { '0' });
                word = word.Substring(0, numberStartIndex + 1);
                return true;
            }
            return false;
        }
    }
}
