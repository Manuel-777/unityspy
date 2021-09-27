// ReSharper disable IdentifierTypo
namespace HackF5.UnitySpy.Detail
{
    // Remember that here pointers are 4 bytes
    internal static class MonoLibraryOffsets
    {
        public const int AssemblyImage = 0x44 + 0x1c;

        public const int ReferencedAssemblies = 0x6c + 0x5c;

        public const int ImageClassCache = 0x354 + 0x16c;

        public const int HashTableSize = 0xc + 0xc;

        public const int HashTableTable = 0x14 + 0xc;

        public const int TypeDefinitionBitFields = 0x14 + 0xc;

        public const int TypeDefinitionByValArg = 0x74 + 0x48 + 0x18;

        public const int TypeDefinitionFieldCount = 0xa4 + 0x34 + 0x10 + 0x18;

        public const int TypeDefinitionFields = 0x60 + 0x20 + 0x18;        // 0x80

        public const int TypeDefinitionFieldSize = 0x10 + 0x10;

        public const int TypeDefinitionName = 0x2c + 0x1c;          // 0x48

        public const int TypeDefinitionNamespace = 0x30 + 0x20;     // 0x50

        public const int TypeDefinitionNestedIn = 0x24 + 0x14;      // 0x38

        public const int TypeDefinitionNextClassCache = 0xa8 + 0x34 + 0x10 + 0x18 + 0x4;

        public const int TypeDefinitionParent = 0x20 + 0x10;        // 0x30

        public const int TypeDefinitionRuntimeInfo = 0x84 + 0x34 + 0x18;   // 0xB8

        public const int TypeDefinitionSize = 0x5c + 0x20;

        public const int TypeDefinitionRuntimeInfoDomainVtables = 0x4 + 0x4;

        public const int TypeDefinitionVTableSize = 0x38 + 0x24;

        public const int TypeDefinitionClassKind = 0x1e + 0xc;

        public const int TypeDefinitionSizeOf = 0x94 + 0x34 + 0x18 + 0x10;

        public const int VTable = 0x28 + 0x18;
    }
}