using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

public static class GameUtils
{
    #region CancelToken
    public static CancellationTokenSource GetCancelToken()
    {
        CancellationTokenSource cancelToken = new CancellationTokenSource();
        EventDispatcher.DestroyEvent += () =>
        {
			try
            {
                cancelToken.Cancel();
                cancelToken.Dispose();
            }
			catch (ObjectDisposedException) { }
        };
        return cancelToken;
    }
    #endregion
    #region IntPtr
    public static IntPtr ToIntPtr(this object _object)
    {
        GCHandle handle = GCHandle.Alloc(_object, GCHandleType.Pinned);

        EventDispatcher.DestroyEvent += () =>
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        };

        return handle.AddrOfPinnedObject();
    }

    public static IntPtr GetIntPtr(int size)
    {
        var ptr = Marshal.AllocHGlobal(size);

        EventDispatcher.DestroyEvent += () =>
        {
            Marshal.FreeHGlobal(ptr);
        };

        return ptr;
    }
    #endregion
    #region Bytes
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    private static extern IntPtr memcpy(IntPtr dest, IntPtr src, int length);

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    private static extern IntPtr memset(IntPtr dest, int value, int length);

    public static void BlockCopy(byte[] dst, byte[] src)
    {
        Buffer.BlockCopy(src, 0, dst, 0, Math.Min(dst.Length, src.Length));
    }

    public static void BlockCopy(byte[] dst, byte[] src, int length)
    {
        Buffer.BlockCopy(src, 0, dst, 0, length);
    }

    public static void BlockCopy(byte[] dst, int offsetdst, byte[] src, int offsetsrc, int length)
    {
        Buffer.BlockCopy(src, offsetsrc, dst, offsetdst, length);
    }

    public static void MemCopy(IntPtr dst, int offsetdst, IntPtr src, int offsetsrc, int length)
    {
        memcpy(dst + offsetdst, src + offsetsrc, length);
    }

    public static void MemCopy(IntPtr dst, IntPtr src, int length)
    {
        memcpy(dst, src, length);
    }

    public static void MemSet(IntPtr dst, int offsetdst, int value, int length)
    {
        memset(dst + offsetdst, value, length);
    }

    public static void MemSet(IntPtr dst, int value, int length)
    {
        memset(dst, value, length);
    }
    #endregion
}
