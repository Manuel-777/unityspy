namespace HackF5.UnitySpy
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using global::MTGAMemorySpy;

    namespace MTGAMemorySpy
    {
        public class Spy
        {
            private IAssemblyImage image;

            [DllExport]
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
            public string GetUUID()
            {
                var uuid = this.image?["PAPA"]["_instance"]["_accountClient"]["AccountInformation"]["AccountID"];

                return uuid ?? "Not Found";
            }
        }
    }
}
