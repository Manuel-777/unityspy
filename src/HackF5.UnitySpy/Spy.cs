namespace HackF5.UnitySpy
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using HackF5.UnitySpy.Detail;

    public static class Spy
    {
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
                // Do nothing
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

        [DllExport]
        public static string GetCards(int pid)
        {
            var image = AssemblyImageFactory.Create(pid);

            var cardsDictionary = image?["WrapperController"]["<Instance>k__BackingField"]["_inventoryServiceWrapper"]["<Cards>k__BackingField"]["entries"];

            // var mgdClass = (ManagedClassInstance)cardsDictionary;
            var dict = (object[])cardsDictionary["entries"];

            // var firstEntry = (ManagedStructInstance)dict.FirstOrDefault();
            var cards = string.Empty;

            foreach (var entry in dict.OfType<ManagedStructInstance>())
            {
                var data = entry.GetData(4 * 4);
                var cardId = BitConverter.ToInt32(data, 0);
                var amount = BitConverter.ToInt32(data, 12);

                cards += cardId + "," + amount + ",";
            }

            return cards;
        }
    }
}
