using ActivityTracker.Client.App;
using System;
using System.Diagnostics;

namespace ActivityTracker.Agent.App
{
    internal abstract class EventIntercepter<T> : IDisposable
    {
        private readonly Action<T> _callback;
        private readonly Action<Exception> _unhandledExceptionAction;
        private readonly IntPtr _hookPtr;
        private readonly NativeFunctions.HookProc _hookProc;

        public EventIntercepter(
            Action<T> callback,
            Action<Exception> unhandledExceptionAction)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _unhandledExceptionAction = unhandledExceptionAction ?? throw new ArgumentNullException(nameof(unhandledExceptionAction));
            _hookProc = HookCallbackHandler;
            _hookPtr = InitHook();
        }

        public void Dispose()
        {
            NativeFunctions.UnhookWindowsHookEx(_hookPtr);
        }

        protected abstract int IdHook { get; }

        private IntPtr InitHook()
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return NativeFunctions.SetWindowsHookEx(IdHook, _hookProc, NativeFunctions.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallbackHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var result = IntPtr.Zero;

            try
            {
                var @event = HookCallback(nCode, wParam, lParam);
                if (@event != null)
                {
                    _callback(@event);
                }

                result = NativeFunctions.CallNextHookEx(_hookPtr, nCode, wParam, lParam);
            }
            catch (Exception exception)
            {
                _unhandledExceptionAction(exception);
            }

            return result;
        }

        protected abstract T HookCallback(int nCode, IntPtr wParam, IntPtr lParam);
    }
}