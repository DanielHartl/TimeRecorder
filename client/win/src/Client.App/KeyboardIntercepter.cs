using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ActivityTracker.Client.App
{
    internal class KeyboardIntercepter : EventIntercepter<char?>
    {
        public KeyboardIntercepter(
            Func<char?, Task> callback,
            Action<Exception> unhandledExceptionAction)
            : base(callback, unhandledExceptionAction)
        {
        }

        protected override char? HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = (KBLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBLLHOOKSTRUCT));
                return (char)hookStruct.vkCode;
            }

            return null;
        }

        private const int WH_KEYBOARD_LL = 13;

        protected override int IdHook => WH_KEYBOARD_LL;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}