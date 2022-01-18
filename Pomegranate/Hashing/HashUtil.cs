using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Pomegranate.Hashing
{
    public static class HashUtil
    {
        private static readonly ConcurrentDictionary<string, PomegranateNamespace> _hashMap = new();

        public static unsafe PomegranateNamespace GetHashSet(string path)
        {
            path = path.ToLowerInvariant();

            //why re-calculate hashes we already have?
            //let's throw them into a dictionary for lookup
            if (_hashMap.TryGetValue(path, out var result)) { return result; }

            var origin = path.Trim('/');
            var originIdentifiers = origin.Split('/');
            var len = originIdentifiers.Length;
            var arr = new ulong[len];

            for (ushort i = 0; i < len; i++)
            {
                var ident = originIdentifiers[i];
                var identLen = ident.Length * 2;//remember these are unicode characters and are 2 bytes!!!

                //c# stores strings as unicode characters, it's a waste to use Encoding.Unicode when we can just grab the pointer
                //since we are already in an unsafe context
                fixed (void* ptr = ident)
                {
                    var buffer = new byte[identLen];
                    System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), buffer, 0, identLen);
                    arr[i] = XxHash.Compute(buffer);                 
                }
            }

            var whash = new PomegranateNamespace(arr);
            _hashMap.TryAdd(path, whash);//it's fine if this fails, the ConcurrentDictionary will handle thread access properly
            return whash;
        }

        public static unsafe ulong GetTypeHash(Type type)
        {
            var typeName = type.FullName?.ToLowerInvariant();
            Debug.Assert(typeName is not null, $@"{nameof(typeName)} is null");
            var length = typeName.Length * 2;//remember these are unicode characters and are 2 bytes!!!

            //c# stores strings a unicode characters, it's a waste to use Encoding.Unicode when we can just grab the pointer
            //since the project is in an unsafe context anyway
            fixed (void* ptr = typeName)
            {
                var buffer = new byte[length];
                System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), buffer, 0, length);
                return XxHash.Compute(buffer);
            }
        }
    }
}
