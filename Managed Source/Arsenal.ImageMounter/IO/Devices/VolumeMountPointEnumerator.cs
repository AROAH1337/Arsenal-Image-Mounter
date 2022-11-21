﻿using Arsenal.ImageMounter.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using static Arsenal.ImageMounter.IO.Native.NativeConstants;
using static Arsenal.ImageMounter.IO.Native.NativeFileIO;
using static Arsenal.ImageMounter.IO.Native.NativeFileIO.UnsafeNativeMethods;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Arsenal.ImageMounter.IO.Devices;

[SupportedOSPlatform(SUPPORTED_WINDOWS_PLATFORM)]
public class VolumeMountPointEnumerator : IEnumerable<string>
{

    public ReadOnlyMemory<char> VolumePath { get; set; }

    public IEnumerator<string> GetEnumerator() => new Enumerator(VolumePath);

    private IEnumerator IEnumerable_GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => IEnumerable_GetEnumerator();

    public VolumeMountPointEnumerator(ReadOnlyMemory<char> VolumePath)
    {
        this.VolumePath = VolumePath;
    }

    private class Enumerator : IEnumerator<string>
    {

        private readonly ReadOnlyMemory<char> _volumePath;

        public SafeFindVolumeMountPointHandle? SafeHandle { get; private set; }

        private char[] _sb = new char[32767];

        public Enumerator(ReadOnlyMemory<char> VolumePath)
        {
            _volumePath = VolumePath;
        }

        public string Current => disposedValue
                    ? throw new ObjectDisposedException("VolumeMountPointEnumerator.Enumerator")
                    : _sb.AsSpan().ReadNullTerminatedUnicodeString();

        private object IEnumerator_Current => Current;

        object IEnumerator.Current => IEnumerator_Current;

        public bool MoveNext()
        {

            if (disposedValue)
            {
                throw new ObjectDisposedException("VolumeMountPointEnumerator.Enumerator");
            }

            if (SafeHandle is null)
            {
                SafeHandle = FindFirstVolumeMountPointW(_volumePath.MakeNullTerminated(), out _sb[0], _sb.Length);
                if (!SafeHandle.IsInvalid)
                {
                    return true;
                }
                else
                {
                    return Marshal.GetLastWin32Error() == ERROR_NO_MORE_FILES ? false : throw new Win32Exception();
                }
            }
            else if (FindNextVolumeMountPointW(SafeHandle, out _sb[0], _sb.Length))
            {
                return true;
            }
            else
            {
                return Marshal.GetLastWin32Error() == ERROR_NO_MORE_FILES ? false : throw new Win32Exception();
            }
        }

        void IEnumerator.Reset() => throw new NotImplementedException();

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    SafeHandle?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                SafeHandle = null;

                // TODO: set large fields to null.
                _sb = null!;
            }

            disposedValue = true;
        }

        // TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        ~Enumerator()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(false);
        }

        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}