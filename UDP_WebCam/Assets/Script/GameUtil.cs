using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

public static class GameUtils
{
    #region CancelToken
    public static CancellationTokenSource GetCancelToken()
	{
        CancellationTokenSource cancelToken = new CancellationTokenSource();
        Updater.DestroyEvent += () => 
        {
            cancelToken.Cancel();
            cancelToken.Dispose();
        };
        return cancelToken;
	}
    #endregion
    #region IntPtr
    public static IntPtr GetIntPtr(object Object)
    {
        GCHandle handle = GCHandle.Alloc(Object, GCHandleType.Pinned);
        Updater.DestroyEvent += () =>
        {
			if (handle.IsAllocated)
			{
                handle.Free();
            }
        };
        return handle.AddrOfPinnedObject();
    }
    #endregion
    #region Thread
    const int MAXTHREADS = 4;
    static int m_ThreadNum = 0;

    public static async UniTaskVoid RunOnThreadPool(Action action, CancellationTokenSource cts = null)
    {
        if (cts == null)
        {
            cts = GetCancelToken();
        }
        await UniTask.WaitUntil(() => m_ThreadNum < MAXTHREADS, cancellationToken: cts.Token);

        Interlocked.Increment(ref m_ThreadNum);
        await UniTask.RunOnThreadPool(action, cancellationToken: cts.Token);
        Interlocked.Decrement(ref m_ThreadNum);
    }

    public static async UniTaskVoid RunOnThreadPool<T>(Func<T> func, CancellationTokenSource cts = null)
    {
		if (cts == null)
		{
            cts = GetCancelToken();
		}
        await UniTask.WaitUntil(() => m_ThreadNum < MAXTHREADS, cancellationToken: cts.Token);

        Interlocked.Increment(ref m_ThreadNum);
        await UniTask.RunOnThreadPool(func, cancellationToken: cts.Token);
        Interlocked.Decrement(ref m_ThreadNum);
    }
    #endregion
    #region ByteUtils
    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, int length);

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memset(IntPtr dest, int value, int length);
    
    public static void CopyBytes(byte[] dest, int startdest, byte[] src, int startsrc, int length)
    {
        Buffer.BlockCopy(src, startsrc, dest, startdest, length);
    }
    #endregion
    #region IP
    public static IPAddress GetIp()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        return null;
    }
    #endregion
}
