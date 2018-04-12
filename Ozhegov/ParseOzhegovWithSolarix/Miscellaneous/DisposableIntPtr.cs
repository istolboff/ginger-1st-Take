using System;

namespace ParseOzhegovWithSolarix.Miscellaneous
{
    internal sealed class DisposableIntPtr : IDisposable 
    {
        public DisposableIntPtr(IntPtr handle, Action<IntPtr> dispose, string objectName, IntPtr? nullValue = null)
        {
            Require.NotNull(dispose, nameof(dispose));

            if (handle == (nullValue ?? IntPtr.Zero))
            {
                throw new InvalidOperationException("Could not create " + objectName);
            }

            _handle = handle;
            _dispose = dispose;
            _objectName = objectName;
            _nullValue = (nullValue ?? IntPtr.Zero);
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            _dispose(_handle);
            _handle = _nullValue;
        }

        public static implicit operator IntPtr(DisposableIntPtr @this)
        {
            if (@this.Disposed)
            {
                throw new ObjectDisposedException(@this._objectName);
            }

            return @this._handle;
        }

        private bool Disposed => _handle == _nullValue;

        private readonly Action<IntPtr> _dispose;
        private readonly string _objectName;
        private readonly IntPtr _nullValue;
        private IntPtr _handle;
    }
}
