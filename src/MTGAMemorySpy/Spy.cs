namespace HackF5.UnitySpy
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using MTGAMemorySpy;

    public class Spy
    {
        // private IAssemblyImage image;

        [DllExport]
        public static int GetPID(string processName)
        {
            var process = Process.GetProcessesByName(processName).FirstOrDefault();
            return process.Id;
        }

        [DllExport]
        public static string GetUUID()
        {
            var process = Process.GetProcessesByName("MTGA").FirstOrDefault();
            if (process == null)
            {
                throw new InvalidOperationException("Failed to find MTGA executable. Please check that MTGA is running.");
            }

            var image = AssemblyImageFactory.Create(process.Id);

            var uuid = image?["PAPA"]["_instance"]["_accountClient"]["<AccountInformation>k__BackingField"]["AccountID"];

            return uuid ?? "Not Found";
        }

    }
}
