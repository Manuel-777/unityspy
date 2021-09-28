namespace HackF5.UnitySpy
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class Spy
    {
        // private IAssemblyImage image;

        [DllExport]
        public static int GetPID(string processName)
        {
            var process = -1;
            try
            {
                var proc = Process.GetProcessesByName(processName).FirstOrDefault();
                process = proc.Id;
            }
            catch
            {
                //
            }
            return process;
        }

        [DllExport]
        public static string GetUUID(int pid)
        {
            var image = AssemblyImageFactory.Create(pid);

            var uuid = image?["PAPA"]["_instance"]["_accountClient"]["<AccountInformation>k__BackingField"]["AccountID"];

            return uuid ?? "Not Found";
        }

    }
}
