using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace streamStore.classes
{
    public static class ArrayExtension
    {
        public static T[] Add<T>(ref T[] originArray, T element)
        {
            Array.Resize(ref originArray, originArray.Length + 1);
            originArray[originArray.Length - 1] = element;
            return originArray;
        }

        public static T[] AddRange<T>(ref T[] originArray, IEnumerable<T> anotherSet)
        {
            T[] anotherArray = anotherSet.ToArray<T>();
            int originLength = originArray.Length;
            Array.Resize(ref originArray, originArray.Length + anotherArray.Length);
            anotherArray.CopyTo(originArray, originLength);
            return originArray;
        }
    }
}
