using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ActivityTracker.Agent.App;

namespace ActivityTracker.Client.App
{
    internal class MouseIntercepter : EventIntercepter<MouseEvent>
    {
        private readonly HashSet<MouseEventType> _listeningEvents;

        public MouseIntercepter(
            Action<MouseEvent> callback,
            Action<Exception> unhandledExceptionAction,
            params MouseEventType[] listeningEvents)
        : base(callback, unhandledExceptionAction)
        {
            _listeningEvents = new HashSet<MouseEventType>(listeningEvents);
        }

        protected override MouseEvent HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var mouseEventType = ToMouseEventType((MouseMessages)wParam);

            if (nCode >= 0 && _listeningEvents.Contains(mouseEventType))
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                var point = hookStruct.pt;

                return new MouseEvent(mouseEventType, new MousePoint(point.x, point.y));
            }

            return null;
        }

        private const int WH_MOUSE_LL = 14;

        protected override int IdHook => WH_MOUSE_LL;

        private static MouseEventType ToMouseEventType(MouseMessages mouseMessage)
        {
            switch (mouseMessage)
            {
                case MouseMessages.WM_LBUTTONDOWN:
                return MouseEventType.LeftButtonDown;

                case MouseMessages.WM_LBUTTONUP:
                return MouseEventType.LeftButtonUp;

                case MouseMessages.WM_RBUTTONDOWN:
                return MouseEventType.RightButtonDown;

                case MouseMessages.WM_RBUTTONUP:
                return MouseEventType.RightButtonUp;

                case MouseMessages.WM_MOUSEMOVE:
                return MouseEventType.MouseMove;

                case MouseMessages.WM_MOUSEWHEEL:
                return MouseEventType.MouseWheel;
            }

            return MouseEventType.Undefined;
        }

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
    }
}