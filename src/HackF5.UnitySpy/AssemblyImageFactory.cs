﻿namespace HackF5.UnitySpy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using HackF5.UnitySpy.Detail;
    using HackF5.UnitySpy.Util;
    using JetBrains.Annotations;

    /// <summary>
    /// A factory that creates <see cref="IAssemblyImage"/> instances that provides access into a Unity application's
    /// managed memory.
    /// SEE: https://github.com/Unity-Technologies/mono.
    /// </summary>
    [PublicAPI]
    public static class AssemblyImageFactory
    {
        /// <summary>
        /// Creates an <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </summary>
        /// <param name="processId">
        /// The id of the Unity process to be inspected.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to be inspected. The default setting of 'Assembly-CSharp' is probably what you want.
        /// </param>
        /// <returns>
        /// An <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </returns>
        public static IAssemblyImage Create(int processId, string assemblyName = "Assembly-CSharp")
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new InvalidOperationException(
                    "This library reads data directly from a process's memory, so is platform specific "
                    + "and only runs under Windows. It might be possible to get it running under macOS, but...");
            }

            var process = new ProcessFacade(processId);
            var monoModule = AssemblyImageFactory.GetMonoModule(process);
            var moduleDump = process.ReadModule(monoModule);

            File.WriteAllBytes("monoDump", moduleDump);

            var rootDomainFunctionAddress = AssemblyImageFactory.GetRootDomainFunctionAddress(moduleDump, monoModule);

            return AssemblyImageFactory.GetAssemblyImage(process, assemblyName, rootDomainFunctionAddress);
        }

        private static AssemblyImage GetAssemblyImage(ProcessFacade process, string name, IntPtr rootDomainFunctionAddress)
        {
            // Offsets taken by decompiling the 64 bits version of mono-2.0-bdwgc.dll
            // mov rax, [rip + 0x46ad39]
            // ret
            var offset = process.ReadInt32(rootDomainFunctionAddress + 3) + 7;

            //// pointer to struct of type _MonoDomain
            var domain = process.ReadPtr(rootDomainFunctionAddress + offset);

            MemoryReadingUtils memReader = new MemoryReadingUtils(process);
            //memReader.ReadMemory(assembly, 220, 1, 1);

            //// pointer to array of structs of type _MonoAssembly
            var assemblyArrayAddress = process.ReadPtr(IntPtr.Add(domain, MonoLibraryOffsets.ReferencedAssemblies));
            for (var assemblyAddress = assemblyArrayAddress;
                assemblyAddress != Constants.NullPtr;
                assemblyAddress = process.ReadPtr(assemblyAddress + 0x8)) //size of pointer
            {
                var assembly = process.ReadPtr(assemblyAddress);
                var assemblyNameAddress = process.ReadPtr(assembly + 0x10); //ponter + int32


//                memReader.ReadMemory(assembly, 220, 4, 0);

                var assemblyName = process.ReadAsciiString(assemblyNameAddress);
                if (assemblyName == name)
                {
                    //memReader.ReadMemory(assembly, 220, 1, 1);

                    return new AssemblyImage(process, process.ReadPtr(IntPtr.Add(assembly, MonoLibraryOffsets.AssemblyImage)));
                    //return new AssemblyImage(process, process.ReadPtr(assembly + 0x44)); -> CollectionManager is null if we have this
                    //return new AssemblyImage(process, process.ReadPtr(assembly + 0x54)); -> CollectionManager is null if we have this
                    //return new AssemblyImage(process, process.ReadPtr(assembly + 0x6c)); -> CollectionManager is null if we have this
                    //return new AssemblyImage(process, process.ReadPtr(assembly + 0xac)); -> CollectionManager is null if we have this
                    //return new AssemblyImage(process, process.ReadPtr(assembly + 0xac));
                }
            }

            throw new InvalidOperationException($"Unable to find assembly '{name}'");
        }

        // https://stackoverflow.com/questions/36431220/getting-a-list-of-dlls-currently-loaded-in-a-process-c-sharp
        private static ModuleInfo GetMonoModule(ProcessFacade process)
        {
            var modulePointers = Native.GetProcessModulePointers(process);

            // Collect modules from the process
            var modules = new List<ModuleInfo>();
            foreach (var modulePointer in modulePointers)
            {
                var moduleFilePath = new StringBuilder(1024);
                var errorCode = Native.GetModuleFileNameEx(
                    process.Process.Handle,
                    modulePointer,
                    moduleFilePath,
                    (uint)moduleFilePath.Capacity);

                if (errorCode == 0)
                {
                    throw new COMException("Failed to get module file name.", Marshal.GetLastWin32Error());
                }

                var moduleName = Path.GetFileName(moduleFilePath.ToString());
                Native.GetModuleInformation(
                    process.Process.Handle,
                    modulePointer,
                    out var moduleInformation,
                    (uint)(IntPtr.Size * modulePointers.Length));

                // Convert to a normalized module and add it to our list
                var module = new ModuleInfo(moduleName, moduleInformation.BaseOfDll, moduleInformation.SizeInBytes);
                modules.Add(module);
            }

            return modules.FirstOrDefault(module => module.ModuleName == "mono-2.0-bdwgc.dll");
        }

        private static IntPtr GetRootDomainFunctionAddress(byte[] moduleDump, ModuleInfo monoModuleInfo)
        {

            var peHeader = new PeNet.PeFile(moduleDump);
            var _if = peHeader.ImportedFunctions;
            var ef = peHeader.ExportedFunctions;


            // offsets taken from https://docs.microsoft.com/en-us/windows/desktop/Debug/pe-format
            // ReSharper disable once CommentTypo
            var startIndex = moduleDump.ToInt32(0x3c); // lfanew



            var exportDirectoryIndex = startIndex + 0x88;// 0x78;
            var exportDirectory = moduleDump.ToInt32(exportDirectoryIndex);

            var numberOfFunctions = moduleDump.ToInt32(exportDirectory + 0x14);
            var functionAddressArrayIndex = moduleDump.ToInt32(exportDirectory + 0x1c);
            var functionNameArrayIndex = moduleDump.ToInt32(exportDirectory + 0x20);

            var sizeOfFunctionEntry = 4;

            IntPtr rootDomainFunctionAddress = (IntPtr) Constants.NullPtr;
            for (var functionIndex = 0;
                functionIndex < (numberOfFunctions * sizeOfFunctionEntry);
                functionIndex += sizeOfFunctionEntry)
            {
                var functionNameIndex = moduleDump.ToInt32(functionNameArrayIndex + functionIndex);
                var functionName = moduleDump.ToAsciiString(functionNameIndex);
                if (functionName == "mono_get_root_domain")
                {
                    rootDomainFunctionAddress = monoModuleInfo.BaseAddress + moduleDump.ToInt32(functionAddressArrayIndex + functionIndex);

                    break;
                }
            }

            if (rootDomainFunctionAddress == (IntPtr) Constants.NullPtr)    
            {
                throw new InvalidOperationException("Failed to find mono_get_root_domain function.");
            }

            return rootDomainFunctionAddress;
        }
    }
}