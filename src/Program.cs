// (c) 2022 BlindEye Softworks. All rights reserved.

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace GetWinPK
{
    internal class Program
    {
        const uint ACPI = 0x41435049,
                   MSDM = 0x4D44534D;

        const int ERROR_NOT_FOUND = 0x00000490,
                  KEY_LENGTH = 0x1D; // Includes dashes in the product key

        static void Main()
        {
            uint bufferSize = GetSystemFirmwareTable(ACPI, MSDM, IntPtr.Zero, 0);

            if (bufferSize == 0)
            {
                int errorCode = Marshal.GetLastWin32Error();

                switch (errorCode)
                {
                    case ERROR_NOT_FOUND:
                        PrintError("The Advanced Configuration & Power Interface (ACPI) Microsoft " +
                            "Data Management (MSDM) table could not be located in the firmware of " +
                            "your computer.", errorCode);
                        break;

                    default:
                        PrintError("An unexpected error has occurred while locating the Advanced " +
                            "Configuration & Power Interface (ACPI) Microsoft Data Management " +
                            "(MSDM) table in the firmware of your computer.", errorCode);
                        break;
                }
            }
            else
            {
                IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
                
                GetSystemFirmwareTable(ACPI, MSDM, buffer, bufferSize);
                
                var productKey = IntPtr.Add(buffer, (int)bufferSize - KEY_LENGTH);
                var data = new byte[KEY_LENGTH];
                
                Marshal.Copy(productKey, data, 0, KEY_LENGTH);
                Marshal.FreeHGlobal(buffer);

                Console.WriteLine(Encoding.UTF8.GetString(data) + '\n');
            }

            Console.Write("Press any key to exit . . .");
            Console.ReadKey();
        }

        static void PrintError(string errorMessage, int errorCode) =>
            Console.WriteLine("{0}\n\nError code: 0x{1}\n", errorMessage, errorCode.ToString("x8"));

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern uint GetSystemFirmwareTable(uint firmwareTableProviderSignature,
            uint firmwareTableID, IntPtr firmwareTableBuffer, uint bufferSize);
    }
}
