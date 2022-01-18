using System.Linq;
using System.Runtime.InteropServices;

namespace Pomegranate
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct PomegranateNamespace
    {
        public int Length { get; init; }
        public ulong[] HashSet { get; init; }

        internal PomegranateNamespace(ulong[] hashSet)
        {
            HashSet = hashSet;
            Length = hashSet.Length;
        }

        /// <summary>
        /// Determines if a namespace is a subset of another namespace
        /// </summary>
        /// <param name="hash">Supplied namespace</param>
        /// <returns>True if this namespace contains the supplied namespace as a subset</returns>
        public bool Contains(PomegranateNamespace hash)
        {
            for (ushort i = 0; i < Length; i++)
            {
                if (!HashSet[i].Equals(hash.HashSet[i])) { return false; }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return obj is PomegranateNamespace hash && HashSet.SequenceEqual(hash.HashSet);
        }

        /// <summary>
        /// This is generally not safe to use
        /// Only use this in the same runtime as the object
        /// and only keep it for the life of the object.
        /// </summary>
        public override int GetHashCode()
        {
            return HashSet.GetHashCode();
        }

        public static bool operator ==(PomegranateNamespace left, PomegranateNamespace right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PomegranateNamespace left, PomegranateNamespace right)
        {
            return !(left == right);
        }
    }
}
