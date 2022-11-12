using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public enum MessageSort : byte
{
    None = 0,
    NewConnect = 1,
    AcceptConnect = 2,
    Request = 3,
    Image = 4
}

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packet
{
    public MessageSort Sort;
    public byte Index;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65505)]
    public byte[] Bytes;
}

public class BaseUDP : IDisposable
{
    protected const int SERVERPORT = 5555;
    protected const int CLIENTPORT = 5556;
    protected const int PACKETSIZE = 65507;
    protected const int DATASIZE = 65505;

    public BaseUDP()
    {
        CreateSocket();
    }
    #region Socket
    private Socket m_Socket;

    private void CreateSocket()
    {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        Updater.DestroyEvent += Dispose;
    }

    public void Dispose()
    {
        m_Socket.Blocking = false;
        m_Socket?.Close();
        m_Socket?.Dispose();
    }
    #endregion
    #region EndPoint
    protected EndPoint m_LocalEndPoint;
    protected EndPoint m_RemoteEndPoint;

    protected void InitEndPoint(int localPort, int remotePort, IPAddress remoteIPAddress = null)
    {
        SetLocalEndPoint(IPAddress.Any, localPort);
        SetRemoteEndPoint(remoteIPAddress ?? IPAddress.Any, remotePort);
        Bind();
    }

    protected void SetLocalEndPoint(IPAddress ipAddress, int port)
    {
        m_LocalEndPoint = new IPEndPoint(ipAddress, port);
    }

    protected void SetRemoteEndPoint(IPAddress ipAddress, int port)
    {
        m_RemoteEndPoint = new IPEndPoint(ipAddress, port);
    }

    protected void Bind()
    {
        if (m_LocalEndPoint == null) { return; }

        m_Socket.Bind(m_LocalEndPoint);
    }
    #endregion
    #region Send/Recv
    protected void Send(byte[] bytes)
    {
        try
        {
            m_Socket.SendTo(bytes, PACKETSIZE, SocketFlags.None, m_RemoteEndPoint);
        }
        catch (Exception)
        {
        }
    }

    protected void Recv(byte[] bytes)
    {
        try
        {
            m_Socket.ReceiveFrom(bytes, PACKETSIZE, SocketFlags.None, ref m_RemoteEndPoint);
        }
        catch (Exception)
        {
        }
    }
    #endregion
}

public abstract class CustomUDP : BaseUDP
{
    public CustomUDP() : base()
    {
        StartSend();
        StartReceive();
    }
    #region Send
    private Queue<byte[]> m_SendBytes = new Queue<byte[]>();

    private async void StartSend()
    {
        await UniTask.WaitUntil(() => m_RemoteEndPoint != null);

        var cancelToken = GameUtils.GetCancelToken();

        GameUtils.RunOnThreadPool(() =>
        {
            while (!cancelToken.IsCancellationRequested)
            {
                lock (m_SendBytes)
                {
                    if (m_SendBytes.Count > 0)
                    {
                        Send(m_SendBytes.Dequeue());
                    }
                }
            }
        }, cancelToken).Forget();
    }

    protected void EnqueueSendBytes(byte[] bytes)
    {
        const int SORT = 0;
        const int INDEX = 1;
        const int BYTES = 2;

        lock (m_SendBytes)
        {
            foreach (var send in m_SendBytes)
            {
                if (send[SORT] == bytes[SORT] && send[INDEX] == bytes[INDEX])
                {
                    GameUtils.CopyBytes(send, BYTES, bytes, BYTES, DATASIZE);
                    return;
                }
            }
            m_SendBytes.Enqueue(bytes);
        }
    }
    #endregion
    #region Recv
    private byte[] m_Recv;
    protected IntPtr m_RecvPointer;

    private async void StartReceive()
    {
        await UniTask.WaitUntil(() => m_RemoteEndPoint != null);

        var cancelToken = GameUtils.GetCancelToken();
        m_Recv = new byte[PACKETSIZE];
        m_RecvPointer = GameUtils.GetIntPtr(m_Recv);

        GameUtils.RunOnThreadPool(() =>
        {
            while (!cancelToken.IsCancellationRequested)
            {
                GameUtils.memset(m_RecvPointer, 0, PACKETSIZE);
                Recv(m_Recv);
                ReceiveBytes(m_Recv);
            }
        }, cancelToken).Forget();
    }

    protected abstract void ReceiveBytes(byte[] bytes);
    #endregion
}

public class SendWebCam : CustomUDP
{
    public SendWebCam(WebCamTexture webCamTexture)
    {
        InitWebCam(webCamTexture);
        InitEndPoint(SERVERPORT, CLIENTPORT);
    }
    #region WebCam
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Count { get; private set; }
    public int Length { get; private set; }

    private WebCamTexture m_WebCamTexture;

    private void InitWebCam(WebCamTexture webCamTexture)
    {
        m_WebCamTexture = webCamTexture;

        Width = m_WebCamTexture.width;
        Height = m_WebCamTexture.height;
        Length = Width * Height * 3;
        Count = Length / DATASIZE + 1;

        InitPixel();
    }
    #region Pixel
    private Color32[] m_Pixel;
    private byte[] m_RGB;
    private IntPtr m_RGBPointer;
    private NativeArray<byte> m_RGBNative;

    private void InitPixel()
    {
        m_Pixel = new Color32[Width * Height];
        m_RGB = new byte[Length];
        m_RGBPointer = GameUtils.GetIntPtr(m_RGB);
        m_RGBNative = new NativeArray<byte>(m_RGB, Allocator.Persistent);
        Updater.DestroyEvent += () => { m_RGBNative.Dispose(); };
        Updater.UpdateEvent += () =>
        {
            if (m_WebCamTexture.isPlaying)
            {
                m_WebCamTexture.GetPixels32(m_Pixel);
                SubAlpha();
            }
        };
    }

    private void SubAlpha()
    {
        JobSubAlpha jobSubAlpha = new JobSubAlpha()
        {
            RGBA = new NativeArray<Color32>(m_Pixel, Allocator.TempJob),
            RGB = m_RGBNative
        };
        var handle = jobSubAlpha.Schedule(Length, 2);

        handle.Complete();

        jobSubAlpha.RGB.CopyTo(m_RGB);
        jobSubAlpha.RGBA.Dispose();
    }

    [BurstCompile]
    private struct JobSubAlpha : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color32> RGBA;
        [WriteOnly] public NativeArray<byte> RGB;

        public void Execute(int index)
        {
            switch (index % 3)
            {
                case 0:
                    RGB[index] = RGBA[index / 3].r;
                    break;
                case 1:
                    RGB[index] = RGBA[index / 3].g;
                    break;
                case 2:
                    RGB[index] = RGBA[index / 3].b;
                    break;
            }
        }
    }
    #endregion
    #endregion
    #region UDP
    public bool IsConneted { get; private set; }
    public byte[] RemoteIP { get; private set; }

    #region Recv
    protected override void ReceiveBytes(byte[] bytes)
    {
        switch ((MessageSort)bytes[0])
        {
            default:
            case MessageSort.None:
                break;
            case MessageSort.NewConnect:
                IsConneted = true;
                RemoteIP = new byte[4] { bytes[2], bytes[3], bytes[4], bytes[5] };

                byte[] data = new byte[PACKETSIZE];
                data[0] = (byte)MessageSort.AcceptConnect;
                data[1] = 0;
                GameUtils.CopyBytes(data, 2, BitConverter.GetBytes(Width), 0, 4);
                GameUtils.CopyBytes(data, 6, BitConverter.GetBytes(Height), 0, 4);
                EnqueueSendBytes(data);

                InitSendImage();
                break;
            case MessageSort.Request:
                GameUtils.memcpy(m_RequestsPointer, m_RecvPointer + 2, DATASIZE);
                break;
        }
    }
    #endregion
    #region Send
    private byte[] m_Requests;
    private IntPtr m_RequestsPointer;

    private byte[][] m_SendByte;
    private IntPtr[] m_SendBytePointer;

    private void InitSendImage()
    {
        m_Requests = new byte[DATASIZE];
        m_RequestsPointer = GameUtils.GetIntPtr(m_Requests);

        m_SendByte = new byte[Count][];
        m_SendBytePointer = new IntPtr[Count];
        for (byte i = 0; i < Count; i++)
        {
            m_SendByte[i] = new byte[PACKETSIZE];
            m_SendByte[i][0] = 4;
            m_SendByte[i][1] = i;
            m_SendBytePointer[i] = GameUtils.GetIntPtr(m_SendByte[i]);
        }

        Updater.UpdateEvent += () =>
        {
            for (byte i = 0; i < Count; ++i)
            {
                if (m_Requests[i] == 0)
                {
                    GameUtils.memcpy(m_SendBytePointer[i] + 2, m_RGBPointer + DATASIZE * i, Math.Min(Length - DATASIZE * i, DATASIZE));
                    EnqueueSendBytes(m_SendByte[i]);
                }
            }
        };
    }
    #endregion
    #endregion
}

public class RecvWebCam : CustomUDP
{
    public bool IsConneted = false;

    public RecvWebCam(byte[] serverIP, RawImage rawImage)
    {
        m_RawImage = rawImage;

        InitEndPoint(CLIENTPORT, SERVERPORT, new IPAddress(serverIP));

        TryConnect();
    }
    #region Texture2D
    private Texture2D m_Texture;
    private RawImage m_RawImage;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Length { get; private set; }
    public int Count { get; private set; }

    private void InitTexture()
    {
        InitPixel();
        Updater.UpdateEvent += MakeTexture;
        Updater.UpdateEvent += () =>
        {
            AddAlpha();
            m_Texture.SetPixels32(m_Pixel);
            m_Texture.Apply();
        };
    }

    private void MakeTexture()
    {
        m_Texture = new Texture2D(Width, Height);
        m_RawImage.texture = m_Texture;
        m_RawImage.rectTransform.sizeDelta = new Vector2(1000, 1000f * Height / Width);

        Updater.UpdateEvent -= MakeTexture;
    }
    #region Pixel
    Color32[] m_Pixel;
    NativeArray<Color32> m_PixelNative;

    byte[] m_RGB;
    IntPtr m_RGBPointer;

    private void InitPixel()
    {
        m_Pixel = new Color32[Width * Height];

        m_RGB = new byte[Length];
        m_RGBPointer = GameUtils.GetIntPtr(m_RGB);
        m_PixelNative = new NativeArray<Color32>(m_Pixel, Allocator.Persistent);
        Updater.DestroyEvent += () => { m_PixelNative.Dispose(); };
    }

    private void AddAlpha()
    {
        JobAddAlpha jobAddAlpha = new JobAddAlpha()
        {
            RGBA = m_PixelNative,
            RGB = new NativeArray<byte>(m_RGB, Allocator.TempJob)
        };
        jobAddAlpha.Schedule(Width * Height, 2).Complete();
        jobAddAlpha.RGBA.CopyTo(m_Pixel);
        jobAddAlpha.RGB.Dispose();
    }

    [BurstCompile]
    private struct JobAddAlpha : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> RGBA;
        [ReadOnly] public NativeArray<byte> RGB;

        public void Execute(int index)
        {
            RGBA[index] = new Color32(RGB[index * 3], RGB[index * 3 + 1], RGB[index * 3 + 2], 255);
        }
    }
    #endregion
    #endregion
    #region UDP
    private byte[] m_SendBytes;
    private IntPtr m_SendBytesPointer;

    protected override void ReceiveBytes(byte[] bytes)
    {
        switch ((MessageSort)bytes[0])
        {
            default:
            case MessageSort.None:
                break;
            case MessageSort.AcceptConnect:
                IsConneted = true;

                Width = BitConverter.ToInt32(bytes, 2);
                Height = BitConverter.ToInt32(bytes, 6);
                Length = Width * Height * 3;
                Count = Length / DATASIZE + 1;

                InitTexture();

                RequestImage();
                break;

            case MessageSort.Image:
                int index = bytes[1];
                GameUtils.memcpy(m_RGBPointer + DATASIZE * index, m_RecvPointer + 2, Math.Min(DATASIZE, Length - DATASIZE * index));
                lock (m_SendBytes)
                {
                    m_SendBytes[index + 2] = 1;
                }
                break;
        }
    }

    private async void TryConnect()
    {
        byte[] bytes = new byte[PACKETSIZE];
        bytes[0] = (byte)MessageSort.NewConnect;
        GameUtils.CopyBytes(bytes, 2, GameUtils.GetIp().GetAddressBytes(), 0, 4);

        while (!IsConneted)
        {
            EnqueueSendBytes(bytes);

            await UniTask.Delay(1000);
        }
    }

    private void RequestImage()
    {
        const int CLEARCOUNT = 4;

        m_SendBytes = new byte[PACKETSIZE];
        m_SendBytesPointer = GameUtils.GetIntPtr(m_SendBytes);

        Updater.UpdateEvent += () =>
        {
            lock (m_SendBytes)
            {
                int count = 0;
                for (int i = 0; i < Count; i++)
                {
                    if (m_SendBytes[i] == 0)
                    {
                        count++;
                    }
                }

                if (count <= CLEARCOUNT)
                {
                    GameUtils.memset(m_SendBytesPointer + 2, 0, Count);
                }

                m_SendBytes[0] = 3;
                m_SendBytes[1] = 0;
                EnqueueSendBytes(m_SendBytes);
            }
        };
    }
    #endregion
}