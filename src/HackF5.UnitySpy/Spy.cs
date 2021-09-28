namespace HackF5.UnitySpy
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    public class Spy
    {
        private IAssemblyImage image;

        public void Init()
        {
            var process = Process.GetProcessesByName("MTGA").FirstOrDefault();
            if (process == null)
            {
                throw new InvalidOperationException("Failed to find MTGA executable. Please check that MTGA is running.");
            }

            this.image = AssemblyImageFactory.Create(process.Id);
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
