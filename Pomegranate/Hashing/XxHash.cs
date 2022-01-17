using System;
using System.Runtime.CompilerServices;

namespace Pomegranate.Hashing
{
    internal static class XxHash
    {
        private const int STRIPE = 32;
        private const ulong PRIME_64_1 = 11400714785074694791ul;
        private const ulong PRIME_64_2 = 14029467366897019727ul;
        private const ulong PRIME_64_3 = 1609587929392839161ul;
        private const ulong PRIME_64_4 = 9650029242287828579ul;
        private const ulong PRIME_64_5 = 2870177450012600261ul;

        private static readonly bool _isBigEndian = !BitConverter.IsLittleEndian;

        /// <summary>
        /// XXH64 is a non-crypto endian-safe 64bit hashing algorithm that is super fast
        /// and has great collision properties. In FC6 we use this to precalculate
        /// the hashcode of namespaces to help reduce reliance on string manipulation
        /// and comparisons.
        /// 
        /// https://github.com/Cyan4973/xxHash
        /// </summary>
        /// <param name="buffer">the data buffer to hash</param>
        /// <returns>A 64bit hash of the data provided</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static unsafe ulong Compute(ReadOnlySpan<byte> buffer)
        {
            ulong acc;
            const ulong seed = 0; //we don't really care about this, so we can keep it at 0 (it just has to be the same across all Pomegranate implementations)
            var len = buffer.Length;
            var remainingLen = len;

            fixed (byte* inputPtr = buffer)
            {
                var pInput = inputPtr;
                if (len >= STRIPE)
                {
                    var (acc1, acc2, acc3, acc4) = InitAccumulators64(seed);
                    do
                    {
                        acc = ProcessStripe64(ref pInput, ref acc1, ref acc2, ref acc3, ref acc4, _isBigEndian);
                        remainingLen -= STRIPE;
                    }
                    while (remainingLen >= STRIPE);
                }
                else
                {
                    acc = seed + PRIME_64_5;
                }

                acc += (ulong)len;
                acc = ProcessRemaining64(pInput, acc, remainingLen, _isBigEndian);
            }

            return Avalanche64(acc);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static (ulong, ulong, ulong, ulong) InitAccumulators64(ulong seed) => (seed + PRIME_64_1 + PRIME_64_2, seed + PRIME_64_2, seed, seed - PRIME_64_1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ProcessLane64(ref ulong accn, ref byte* pInput)
        {
            var lane = *(ulong*)pInput;
            accn = Round64(accn, lane);
            pInput += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ProcessLaneBigEndian64(ref ulong accn, ref byte* pInput)
        {
            var lane = *(ulong*)pInput;
            lane = SwapBytes64(lane);
            accn = Round64(accn, lane);
            pInput += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Avalanche64(ulong acc)
        {
            acc ^= acc >> 33;
            acc *= PRIME_64_2;
            acc ^= acc >> 29;
            acc *= PRIME_64_3;
            acc ^= acc >> 32;
            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong Round64(ulong accn, ulong lane)
        {
            accn += lane * PRIME_64_2;
            return RotateLeft(accn, 31) * PRIME_64_1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MergeAccumulator64(ref ulong acc, ulong accn)
        {
            acc ^= Round64(0, accn);
            acc *= PRIME_64_1;
            acc += PRIME_64_4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong ProcessRemaining64(byte* pInput, ulong acc, int remainingLen, bool bigEndian)
        {
            for (; remainingLen >= 8; remainingLen -= 8, pInput += 8)
            {
                var lane = *(ulong*)pInput;
                if (bigEndian) { lane = SwapBytes64(lane); }

                acc ^= Round64(0, lane);
                acc = RotateLeft(acc, 27) * PRIME_64_1;
                acc += PRIME_64_4;
            }

            for (; remainingLen >= 4; remainingLen -= 4, pInput += 4)
            {
                var lane32 = *(uint*)pInput;
                if (bigEndian) { lane32 = SwapBytes32(lane32); }

                acc ^= lane32 * PRIME_64_1;
                acc = RotateLeft(acc, 23) * PRIME_64_2;
                acc += PRIME_64_3;
            }

            for (; remainingLen >= 1; remainingLen--, pInput++)
            {
                var lane8 = *pInput;
                acc ^= lane8 * PRIME_64_5;
                acc = RotateLeft(acc, 11) * PRIME_64_1;
            }

            return acc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong ProcessStripe64(ref byte* pInput, ref ulong acc1, ref ulong acc2, ref ulong acc3, ref ulong acc4, bool bigEndian)
        {
            if (bigEndian)
            {
                ProcessLaneBigEndian64(ref acc1, ref pInput);
                ProcessLaneBigEndian64(ref acc2, ref pInput);
                ProcessLaneBigEndian64(ref acc3, ref pInput);
                ProcessLaneBigEndian64(ref acc4, ref pInput);
            }
            else
            {
                ProcessLane64(ref acc1, ref pInput);
                ProcessLane64(ref acc2, ref pInput);
                ProcessLane64(ref acc3, ref pInput);
                ProcessLane64(ref acc4, ref pInput);
            }

            ulong acc = RotateLeft(acc1, 1) + RotateLeft(acc2, 7) + RotateLeft(acc3, 12) + RotateLeft(acc4, 18);

            MergeAccumulator64(ref acc, acc1);
            MergeAccumulator64(ref acc, acc2);
            MergeAccumulator64(ref acc, acc3);
            MergeAccumulator64(ref acc, acc4);
            return acc;
        }


        #region BIT_UTILS

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotateLeft(ulong value, int bits) => (value << bits) | (value >> (64 - bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong RotateRight(ulong value, int bits) => (value >> bits) | (value << (64 - bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint SwapBytes32(uint num) => (RotateLeft(num, 8) & 0x00FF00FFu) | (RotateRight(num, 8) & 0xFF00FF00u);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateLeft(uint value, int bits) => (value << bits) | (value >> (32 - bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint RotateRight(uint value, int bits) => (value >> bits) | (value << (32 - bits));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong SwapBytes64(ulong num)
        {
            num = (RotateLeft(num, 48) & 0xFFFF0000FFFF0000ul) | (RotateLeft(num, 16) & 0x0000FFFF0000FFFFul);
            return (RotateLeft(num, 8) & 0xFF00FF00FF00FF00ul) | (RotateRight(num, 8) & 0x00FF00FF00FF00FFul);
        }

        #endregion BIT_UTILS
    }
}
