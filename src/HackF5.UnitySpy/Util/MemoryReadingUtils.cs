using HackF5.UnitySpy.Detail;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HackF5.UnitySpy.Util
{
    class MemoryReadingUtils
    {

        //private uint address;

        private ProcessFacade process;

        private List<IntPtr> pointersShown = new List<IntPtr>();

        public MemoryReadingUtils(ProcessFacade process)
        {
            this.process = process;
        }

        public void ReadMemory(IntPtr address, int length, int stepSize = 4, int recursiveDepth = 0)
        {
            StringBuilder strBuilder = new StringBuilder();

            ReadMemoryRecursive(address, length, stepSize, recursiveDepth, strBuilder);

            File.WriteAllText("Memory Dump - " + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".txt", strBuilder.ToString());
        }

        private void ReadMemoryRecursive(IntPtr address, int length, int stepSize, int recursiveDepth, StringBuilder strBuilder)
        {
            for (int i = 0; i < length; i += stepSize)
            {
                SingleReadMemoryRecursive(IntPtr.Add(address, i), length, stepSize, recursiveDepth, strBuilder);
            }
        }

        private void SingleReadMemoryRecursive(IntPtr address, int length, int stepSize, int recursiveDepth, StringBuilder strBuilder)
        {
            string addressStr = address.ToString("X");// + "+" + offset.ToString("X");
            
            strBuilder.AppendLine("========================================== Reading Memory at " + addressStr + " Depth = " + recursiveDepth + " ========================================== ");

            var ptr = Constants.NullPtr;
            try
            {
                ptr = process.ReadPtr(address);
            } 
            catch (Exception ex)
            {
            }

            try
            {
                strBuilder.AppendLine("Value as int32: " + process.ReadInt32(address));
                strBuilder.AppendLine("Value as uint32: " + process.ReadUInt32(address));
                strBuilder.AppendLine("Value as pointer32: " + process.ReadUInt32(address).ToString("X"));
                strBuilder.AppendLine("Value as pointer64: " + process.ReadUInt64(address).ToString("X"));
                byte[] stringBytes = new byte[stepSize];
                for(int i = 0; i < stepSize; i++)
                {
                    stringBytes[i] = process.ReadByte(address + i);
                }
                strBuilder.AppendLine("Value as string: " + stringBytes.ToAsciiString());
                strBuilder.AppendLine("Value as string (Unicode): " + Encoding.Unicode.GetString(stringBytes, 0, stepSize));
                
            }
            catch(Exception ex)
            {
                strBuilder.AppendLine("No posible values found");
                return;
            }
            if (ptr != Constants.NullPtr)
            {

                if (pointersShown.Contains(ptr))
                {
                    strBuilder.AppendLine("Pointer already shown: " + ptr);
                }
                else
                {
                    try
                    {
                        strBuilder.AppendLine("Value as char *: " + process.ReadAsciiString(ptr));
                    }
                    catch (Exception ex)
                    {
                    }
                    if (recursiveDepth > 0)
                    {
                        ReadMemoryRecursive(ptr, length, stepSize, recursiveDepth - 1, strBuilder);
                    }
                    pointersShown.Add(ptr);

                }
            }
        }

    }
}
