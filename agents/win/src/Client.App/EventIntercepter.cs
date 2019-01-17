using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ActivityTracker.Client.App
{
    internal abstract class EventIntercepter<T> : IDisposable
    {
        private readonly Func<T, Task> _callback;
        private readonly Action<Exception> _unhandledExceptionAction;
        private readonly IntPtr _hookPtr;
        private readonly NativeFunctions.HookProc _hookProc;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ConcurrentQueue<T> _eventQueue = new ConcurrentQueue<T>();
        private readonly Task _backgroundTask;

        public EventIntercepter(
            Func<T, Task> callback,
            Action<Exception> unhandledExceptionAction)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _unhandledExceptionAction = unhandledExceptionAction ?? throw new ArgumentNullException(nameof(unhandledExceptionAction));
            _hookProc = HookCallbackHandler;
            _hookPtr = InitHook();

            _backgroundTask = Run();
        }

        public void Dispose()
        {
            NativeFunctions.UnhookWindowsHookEx(_hookPtr);
            _cts.Cancel();
            _backgroundTask.Wait();
        }

        protected abstract int IdHook { get; }

        private Task Run()
        {
            return Task.Run(async () => {
                try
                {
                    while (!_cts.IsCancellationRequested)
                    {
                        await FlushEventsAsync();
                        await Task.Delay(100);
                    }
                }
                catch (Exception exception)
                {
                    _unhandledExceptionAction(exception);
                }
            });
        }

        private async Task FlushEventsAsync()
        {
            while (_eventQueue.TryDequeue(out var @event))
            {
                await _callback(@event);
            }
        }

        private IntPtr InitHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeFunctions.SetWindowsHookEx(IdHook, _hookProc, NativeFunctions.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallbackHandler(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var @event = HookCallback(nCode, wParam, lParam);
            if (@event != null)
            {
                _eventQueue.Enqueue(@event);
            }

            return NativeFunctions.CallNextHookEx(_hookPtr, nCode, wParam, lParam);
        }

        protected abstract T HookCallback(int nCode, IntPtr wParam, IntPtr lParam);
    }
}