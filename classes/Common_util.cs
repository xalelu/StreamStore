using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamStore.classes
{
    class Common_util
    {
        public static char[] char_refer = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
        //隨機建立ID
        public static string generalID(int[] bit_count, char gap)
        {
            Random _general = new Random();
            StringBuilder result = new StringBuilder();
            for(int i = 0;i < bit_count.Length; i++)
            {
                if (result.Length > 0) result.Append(gap);
                for(int j = 0;j < bit_count[i]; j++)
                {
                    char onechar = char_refer[_general.Next(0, char_refer.Length)];
                    result.Append(onechar);
                }
            }
            return result.ToString();
        }

    }
}
