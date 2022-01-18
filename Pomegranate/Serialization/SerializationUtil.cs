using Pomegranate.Contracts;
using Pomegranate.Hashing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Pomegranate.Serialization
{
    public static class SerializationUtil
    {
        private static readonly ConcurrentDictionary<Type, ulong> _signatures = new();
        private static readonly ConcurrentDictionary<ulong, ISerializer> _serializers = new();

        internal static IEnumerable<Type>? GetKnownTypes() => _contractTypes;

        static SerializationUtil()
        {
            _GET_KNOWN_TYPES();
        }

        internal static byte[] Serialize(IPomegranateContract cntrct)
        {
            return Serialize<DefaultSerializer>(cntrct);
        }

        internal static byte[] Serialize<T>(IPomegranateContract cntrct) where T : ISerializer
        {
            CheckType(cntrct);
            if (!_signatures.TryGetValue(typeof(T), out var sig)) { RegisterSerializer<T>(); }
            if (!_signatures.TryGetValue(typeof(T), out sig)) { throw new InvalidSerializerException<T>(); }

            using var buffer = new MemoryStream();
            buffer.Write(BitConverter.GetBytes(sig), 0, 8);//write the serializer's signature to the beginning of the buffer
            if (!_serializers.TryGetValue(sig, out var serializer)) { throw new UnknownSerializerException(sig); }
            return serializer.Serialize(cntrct, buffer);
        }

        internal static IPomegranateContract? Deserialize(byte[] data)
        {
            var sig = new byte[8];
            var dataBuffer = new byte[data.Length - 8];
            Buffer.BlockCopy(data, 0, sig, 0, 8);//separate the serializer signature from the data
            Buffer.BlockCopy(data, 8, dataBuffer, 0, dataBuffer.Length);

            var signature = BitConverter.ToUInt64(sig);

            if (!_serializers.TryGetValue(signature, out var serializer)) { throw new UnknownSerializerException(signature); }
            return serializer.Deserialize(dataBuffer);
        }

        /// <summary>
        /// Allows you to manually register serializers, though serializers should automatically be
        /// registered if they are located in an assembly that is loaded.
        /// </summary>
        /// <typeparam name="T">The ISerializer implementation</typeparam>
        public static void RegisterSerializer<T>() where T : ISerializer
        {
            var type = typeof(T);
            var sig = _signatures.GetOrAdd(type, HashUtil.GetTypeHash(type));//get the type signature

            if (Activator.CreateInstance(type) is not ISerializer serializer) { throw new Exception($@"Failed to activate and register serializer: {type.FullName}"); }
            _serializers.TryAdd(sig, serializer);//create an instance of the serializer
        }

        private static void CheckType(object obj)
        {
            var result = _contractTypes?.Contains(obj.GetType());
            var msg = $@"Fatal contract serialization error, the type: {obj.GetType().FullName} is not a known type.";
            Debug.Assert(result.HasValue && result.Value, msg);
            if (!result.HasValue || result.HasValue && !result.Value) { throw new InvalidOperationException(msg); }
        }

        /// <summary>
        /// interesting stuff down here. The C# XML Serializer requires to know all of the possible types to serialize, so we need to go through all the 
        /// assemblies and pull all the types that implement IPomegranateContract and add them to the _contractTypes arr. which is used during serialization
        /// </summary>
        private static Type[]? _contractTypes;
        private static void _GET_KNOWN_TYPES()//_ALL_UPPER format used to signify this method does something unusual or is for an unusual purpose
        {
            if (_contractTypes != null) return;

            var genericTypes = new List<Type>();
            var nonGenericTypes = new List<Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(IPomegranateContract).IsAssignableFrom(type) || type == typeof(IPomegranateContract)) continue;
                    if (type.ContainsGenericParameters) { genericTypes.Add(type); }
                    else { nonGenericTypes.Add(type); }
                }
            }

            //find all the defined serializers and add them to the lookups
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(ISerializer).IsAssignableFrom(type) || type == typeof(ISerializer)) continue;

                    var sig = _signatures.GetOrAdd(type, HashUtil.GetTypeHash(type));//get the type signature

                    if (Activator.CreateInstance(type) is not ISerializer serializer) { throw new NullReferenceException($@"Failed to activate and register serializer: {type.FullName}"); }
                    _serializers.TryAdd(sig, serializer);//create an instance of the serializer
                }
            }

            var instancedGenericTypes = from genericType in genericTypes
                                        let typePermutationsForGeneric = _GET_TYPE_PERMUTATIONS(genericType.GetGenericArguments(), nonGenericTypes)
                                        from typePermutation in typePermutationsForGeneric
                                        select genericType.MakeGenericType(typePermutation.ToArray());

            _contractTypes = nonGenericTypes.Concat(instancedGenericTypes).ToArray();
        }

        private static IEnumerable<IEnumerable<Type>> _GET_TYPE_PERMUTATIONS(IEnumerable<Type> genericArguments, IEnumerable<Type> candidateTypes)
        {
            var enumerable = genericArguments.ToList();
            switch (enumerable.Count)
            {
                case 0:
                    return new List<List<Type>>();
                case 1:
                    return candidateTypes
                            .Where(ctype => enumerable.First().GetGenericParameterConstraints()
                            .Any(constraint => constraint.IsAssignableFrom(ctype)))
                            .Select(ctype => new List<Type> { ctype });
                default:
                    {
                        var types = candidateTypes.ToList();
                        var ret = _GET_TYPE_PERMUTATIONS(enumerable.Skip(1).ToList(), types)
                                .SelectMany(sublist => types
                                .Where(ctype => enumerable.First().GetGenericParameterConstraints()
                                .Any(constraint => constraint.IsAssignableFrom(ctype)))
                                .Select(ctype => sublist
                                .Concat(new List<Type> { ctype })))
                                .ToList();

                        return ret;
                    }
            }
        }
    }
}
