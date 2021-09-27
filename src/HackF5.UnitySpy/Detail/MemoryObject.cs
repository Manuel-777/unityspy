using System;

namespace HackF5.UnitySpy.Detail
{
    /// <summary>
    /// The base type for all objects accessed in a process' memory. Every object has an address in memory
    /// and all information about that object is accessed via an offset from that address.
    /// </summary>
    public abstract class MemoryObject : IMemoryObject
    {
        protected MemoryObject(AssemblyImage image, IntPtr address)
        {
            this.Image = image;
            this.Address = address;
        }

        public IntPtr GetAddress()
        {
            return this.Address;
        }

        IAssemblyImage IMemoryObject.Image => this.Image;

        public virtual AssemblyImage Image { get; }

        public virtual ProcessFacade Process => this.Image.Process;

        protected IntPtr Address { get; }

        protected int ReadInt32(uint offset) => this.Process.ReadInt32(IntPtr.Add(this.Address, Convert.ToInt32(offset)));

        protected IntPtr ReadPtr(uint offset) => this.Process.ReadPtr(IntPtr.Add(this.Address, Convert.ToInt32(offset)));

        protected string ReadString(uint offset) => this.Process.ReadAsciiStringPtr(IntPtr.Add(this.Address, Convert.ToInt32(offset)));

        protected uint ReadUInt32(uint offset) => this.Process.ReadUInt32(IntPtr.Add(this.Address, Convert.ToInt32(offset)));

        protected ulong ReadUInt64(uint offset) => this.Process.ReadUInt64(IntPtr.Add(this.Address, Convert.ToInt32(offset)));

        protected byte ReadByte(uint offset) => this.Process.ReadByte(IntPtr.Add(this.Address, Convert.ToInt32(offset)));
    }
}