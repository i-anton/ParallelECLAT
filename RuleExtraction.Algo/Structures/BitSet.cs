using System;
using System.Runtime.CompilerServices;

namespace RuleExtraction.Algo.Structures
{
    public class BitSet
    {
        private long[] array;
        private const int elemSizeOf = sizeof(long);

        public BitSet(int length) => array = new long[length / elemSizeOf + 1];
        public BitSet(BitSet set)
        {
            array = new long[set.array.Length];
            set.array.CopyTo(array, 0);
        }

        public bool this[int index] => Get(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index)
        {
            int ByteIdx = index / elemSizeOf;
            int BitIdx = index % elemSizeOf;
            return (array[ByteIdx] & (1L << BitIdx)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index, bool value)
        {
            int ByteIdx = index / elemSizeOf;
            int BitIdx = index % elemSizeOf;
            long bitMask = (1L << BitIdx);
            ref long segment = ref array[ByteIdx];
            if (value)
            {
                segment |= bitMask;
            }
            else
            {
                segment &= ~bitMask;
            }
        }

        public void SetFrom(BitSet set) => set.array.CopyTo(array, 0);

        public void Or(BitSet set)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] |= set.array[i];
        }

        #region Override
        public override bool Equals(object obj)
        {
            if (obj is BitSet set)
            {
                for (int i = 0; i < set.array.Length; i++)
                    if (set.array[i] != array[i])
                        return false;
                return true;
            }
            return false;
        }

        public bool Equals(BitSet set)
        {
            for (int i = 0; i < set.array.Length; i++)
                if (set.array[i] != array[i])
                    return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = 0;
            for (int i = 0; i < array.Length; i++)
            {
                result <<= 2;
                result += unchecked((int)array[i]) + 13;
            }
            return result;
        }
        public override string ToString()
        {
            var s = string.Empty;
            for (int i = 0; i < array.Length; i++)
                s += Convert.ToString(array[i], 16).PadLeft(8, '0');
            return s;
        }

        #endregion
    }
}
