﻿namespace HackF5.UnitySpy.Detail
{
    using JetBrains.Annotations;

    /// <summary>
    /// Represents a class instance in managed memory.
    /// Mono and .NET don't necessarily use the same layout scheme, but assuming it is similar this article provides
    /// some useful information:
    /// https://web.archive.org/web/20080919091745/http://msdn.microsoft.com:80/en-us/magazine/cc163791.aspx.
    /// </summary>
    [PublicAPI]
    public class ManagedClassInstance : MemoryObject, IManagedObjectInstance
    {
        private readonly uint definitionAddress;

        private readonly uint vtable;

        public ManagedClassInstance(AssemblyImage image, uint address)
            : base(image, address)
        {
            // the address of the class instance points directly back the the classes VTable
            this.vtable = this.ReadPtr(0x0);

            // The VTable points to the class definition itself.
            this.definitionAddress = image.Process.ReadPtr(this.vtable);
        }

        ITypeDefinition IManagedObjectInstance.TypeDefinition => this.TypeDefinition;

        public TypeDefinition TypeDefinition => this.Image.GetClassDefinition(this.definitionAddress);

        public TValue GetValue<TValue>(string fieldName) =>
            this.TypeDefinition.GetField(fieldName).GetValue<TValue>(this.Address);

        public TValue GetValue<TValue>(string fieldName, string typeFullName) =>
            this.TypeDefinition.GetField(fieldName, typeFullName).GetValue<TValue>(this.Address);
    }
}