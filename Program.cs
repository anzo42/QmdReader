using System.Text;

namespace QmdReader
{
    internal class Program
    {
        private static string DecodeQmd(byte[] bytes)
        {
            var key = new byte[] { 0x06, 0x70, 0x6C, 0x65, 0x70, 0x6C, 0x65, 0x00 };
            var list = new List<byte>();
            var i = 0;
            var arrays = bytes.GroupBy(s => i++ / 251).Select(s => s.ToArray()).ToArray();

            foreach (var array in arrays)
            {
                int counter = 0;
                for (int j = 1; j < array.Length; j++)
                {
                    var temp = array[j];
                    counter++;
                    temp ^= key[counter % 6];

                    if (temp == 0xFE)
                    {
                        break;
                    }
                    list.Add(temp);
                }
            }

            var newList = list.ToArray();
            for (int k = 0; k < newList.Length; k++)
            {
                if (newList[k] == 0x01)
                    newList[k] = 0x0D;
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding(1250);
            return encoding.GetString(newList);
        }

        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("QmdReader.exe [File1] [File2] [File3]...");
                return;
            }
            foreach (string arg in args)
            {
                try
                {
                    var bytes = System.IO.File.ReadAllBytes(arg);
                    var header = bytes.Skip(1).Take(3).ToArray();
                    var fileHeader = Encoding.ASCII.GetString(header);
                    if (fileHeader == "qmd")
                    {
                        File.AppendAllText($"{arg}.txt", DecodeQmd(bytes));
                    }
                    else
                    {
                        Console.WriteLine($"File '{arg}' is not supported");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}