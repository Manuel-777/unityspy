﻿namespace HackF5.UnitySpy
{
    using System;
    using System.Diagnostics;
    using System.Linq;
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
        /// <param name="processName">
        /// The name of the Unity process to be inspected. For example 'Hearthstone'.
        /// </param>
        /// <param name="assemblyName">
        /// The name of the assembly to be inspected. The default setting of 'Assembly-CSharp' is probably what you want.
        /// </param>
        /// <returns>
        /// An <see cref="IAssemblyImage"/> that provides access into a Unity application's managed memory.
        /// </returns>
        public static IAssemblyImage Create(string processName, string assemblyName = "Assembly-CSharp")
        {
            var process = new ProcessFacade(processName);
            var monoModule = AssemblyImageFactory.GetMonoModule(process);
            var moduleDump = process.ReadModule(monoModule);
            var rootDomainFunctionAddress = AssemblyImageFactory.GetRootDomainFunctionAddress(moduleDump, monoModule);

            return AssemblyImageFactory.GetAssemblyImage(process, assemblyName, rootDomainFunctionAddress);
        }

        private static AssemblyImage GetAssemblyImage(ProcessFacade process, string name, int rootDomainFunctionAddress)
        {
            var domainAddress = process.ReadPtr((uint)rootDomainFunctionAddress + 1);
            //// pointer to struct of type _MonoDomain
            var domain = process.ReadPtr(domainAddress);

            //// pointer to array of structs of type _MonoAssembly
            var assemblyArrayAddress = process.ReadPtr(domain + 0x70);
            for (var assemblyAddress = assemblyArrayAddress;
                assemblyAddress != Constants.NullPtr;
                assemblyAddress = process.ReadPtr(assemblyAddress + 0x4))
            {
                var assembly = process.ReadPtr(assemblyAddress);
                var assemblyNameAddress = process.ReadPtr(assembly + 0x8);
                var assemblyName = process.ReadAsciiString(assemblyNameAddress);
                if (assemblyName == name)
                {
                    return new AssemblyImage(process, process.ReadPtr(assembly + 0x40));
                }
            }

            throw new InvalidOperationException($"Unable to find assembly '{name}'");
        }

        private static ProcessModule GetMonoModule(ProcessFacade process)
        {
            return process.Process.Modules.Cast<ProcessModule>().FirstOrDefault(m => m.ModuleName == "mono.dll")
                ?? throw new InvalidOperationException("Unable to find module 'mono.dll'.");
        }

        private static int GetRootDomainFunctionAddress(byte[] moduleDump, ProcessModule monoModule)
        {
            // offsets taken from https://docs.microsoft.com/en-us/windows/desktop/Debug/pe-format
            // ReSharper disable once CommentTypo
            var startIndex = moduleDump.ToInt32(0x3c); // lfanew

            var exportDirectoryIndex = startIndex + 0x78;
            var exportDirectory = moduleDump.ToInt32(exportDirectoryIndex);

            var numberOfFunctions = moduleDump.ToInt32(exportDirectory + 0x14);
            var functionAddressArrayIndex = moduleDump.ToInt32(exportDirectory + 0x1c);
            var functionNameArrayIndex = moduleDump.ToInt32(exportDirectory + 0x20);

            var rootDomainFunctionAddress = Constants.NullPtr;
            for (var functionIndex = Constants.NullPtr;
                functionIndex < (numberOfFunctions * Constants.SizeOfPtr);
                functionIndex += (int)Constants.SizeOfPtr)
            {
                var functionNameIndex = moduleDump.ToInt32(functionNameArrayIndex + functionIndex);
                var functionName = moduleDump.ToAsciiString(functionNameIndex);
                if (functionName == "mono_get_root_domain")
                {
                    rootDomainFunctionAddress = monoModule.BaseAddress.ToInt32()
                        + moduleDump.ToInt32(functionAddressArrayIndex + functionIndex);

                    break;
                }
            }

            if (rootDomainFunctionAddress == Constants.NullPtr)
            {
                throw new InvalidOperationException("Failed to find mono_get_root_domain function.");
            }

            return rootDomainFunctionAddress;
        }
    }
}