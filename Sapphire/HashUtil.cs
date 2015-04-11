using System;
using UnityEngine;

namespace Sapphire
{
    public static class HashUtil
    {

        public static long HashRect(Rect rect)
        {
            long state = 18021301;
            Accumulate(ref state, rect.x, 0);
            Accumulate(ref state, rect.y, 0);
            Accumulate(ref state, rect.width, 0);
            Accumulate(ref state, rect.height, 0);
            return state;
        }

        public static string HashToString(long hash)
        {
            return String.Format("{0:X}", hash);
        }

        private static long[] largePrimes = new[]
        {
            8100529L,
            12474907L,
            15485039L,
            21768739L,
            28644467L,
            32452681L
        };

        private static void Accumulate(ref long state, float value, int index)
        {
            state ^= ((long)value) * largePrimes[index];
        }

    }

}
