using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// Sources:
/// https://docs.microsoft.com/en-us/windows/desktop/inputdev/wm-keyup
/// https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-keyboard-hook-in-c/
/// https://stackoverflow.com/questions/25046376/global-hotkeys-allows-user-to-hold-down-modifier-keys
/// </summary>

namespace KeyboardWatcher
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;

        private static bool CONTROL_DOWN = false;
        private static bool SHIFT_DOWN = false;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        static void Main(string[] args)
        {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        /// <summary>
        /// Play the sound file
        /// </summary>
        private static void Alert()
        {
            WaveStream mainOutputStream = new WaveFileReader("alert.wav");
            WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
            WaveOutEvent player = new WaveOutEvent();

           player.Init(volumeStream);

            Console.WriteLine("Button pushed, playing alert!");
            player.Play();
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN) // Is it a KEYDOWN event?
            {
                int vkCode = Marshal.ReadInt32(lParam); // Turn the thing into an Int32
                string theKey = ((Keys)vkCode).ToString(); // Turn the code into a keys object

                if (theKey.Contains("ControlKey")) // Is CTRL down?
                {
                    CONTROL_DOWN = true;
                }
                else if (theKey.Contains("ShiftKey")) // Is SHIFT down?
                {
                    SHIFT_DOWN = true;
                }
                else if (CONTROL_DOWN && SHIFT_DOWN && theKey == "F12") // Is it the F10 key combined with CTRL and SHIFT?
                {
                    Alert();
                }
                //Console.Write((Keys)vkCode + " ");
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP) //KeyUP
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string theKey = ((Keys)vkCode).ToString();
                if (theKey.Contains("ControlKey"))
                {
                    CONTROL_DOWN = false;
                }
                if (theKey.Contains("ShiftKey"))
                {
                    SHIFT_DOWN = false;
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
