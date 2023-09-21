using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Khansho.Example1
{
	public class Sender : MonoBehaviour
	{
		private const int SERVERPORT = 15000;
		private const int CLIENTPORT = 15001;

		[SerializeField] private SenderWebCam m_WebCam;
		[SerializeField] private UnityEvent m_OnConnected;

		private ExampleRUDP m_RUDP;
		private IntPtr m_DataStreamPtr;

		private void Start()
		{
			InitRUDP();
		}

		private void OnDestroy()
		{
			DisposeRUDP();
		}

		private void InitRUDP()
		{
			m_RUDP = new ExampleRUDP();
			m_RUDP.SendStart(CLIENTPORT);
			m_RUDP.ReceiveStart(SERVERPORT);
			m_RUDP.RecvCallBack += OnReceive;
			m_RUDP.SendCallBack += x =>
			{
				if (x.DataType == DataType.EndStream)
				{
					SendTexture();
				}
			};

			m_DataStreamPtr = GameUtils.GetIntPtr(Datagram.SIZE);
			GameUtils.MemSet(m_DataStreamPtr, 0, (byte)DataType.DataStream, 1);
		}

		private void DisposeRUDP()
		{
			m_RUDP?.Dispose();
			m_RUDP = null;
		}

		private void OnReceive(Datagram data)
		{
			switch (data.DataType)
			{
				default:
					break;

				case DataType.TryConnect:
					Debug.Log("TryConnect");
					if (!m_RUDP.IsConnect)
					{
						Accept(data.Bytes);
						m_OnConnected.Invoke();
					}
					else
					{
						Block();
					}
					break;
			}
		}

		public void SendTexture()
		{
			if (m_WebCam.TryUpdateBytes())
			{
				try
				{
					SendBeginStream();

					var div = m_WebCam.Length / Datagram.BUFFERLENGTH;
					for (int i = 0; i < div; i++)
					{
						SendDataStream(m_WebCam.Ptr + i * Datagram.BUFFERLENGTH, Datagram.BUFFERLENGTH);
					}

					var rem = m_WebCam.Length % Datagram.BUFFERLENGTH;
					if (rem > 0)
					{
						SendDataStream(m_WebCam.Ptr + div * Datagram.BUFFERLENGTH, rem);
					}

					SendEndStream();
				}
				catch (NullReferenceException)
				{
					SendEndStream();
				}
			}
		}

		private void SendBeginStream()
		{
			int width = m_WebCam.Width;
			int height = m_WebCam.Height;
			var bytes = new byte[8];

			bytes[0] = (byte)(width & 0xff);
			bytes[1] = (byte)((width >> 8) & 0xff);
			bytes[2] = (byte)((width >> 16) & 0xff);
			bytes[3] = (byte)(width >> 24);

			bytes[4] = (byte)(height & 0xff);
			bytes[5] = (byte)((height >> 8) & 0xff);
			bytes[6] = (byte)((height >> 16) & 0xff);
			bytes[7] = (byte)(height >> 24);

			SendDatagram(DataType.BeginStream, bytes);
		}

		private void SendEndStream()
		{
			SendDatagram(DataType.EndStream, null);
		}

		private void SendDataStream(IntPtr ptr, int length)
		{
			GameUtils.MemCopy(m_DataStreamPtr + Datagram.HEADLENGTH, ptr, length);
			var data = Marshal.PtrToStructure<Datagram>(m_DataStreamPtr);
			m_RUDP.RequestSend(ref data, Datagram.HEADLENGTH + length);
		}

		private void SendDataStream(byte[] bytes = null)
		{
			SendDatagram(DataType.DataStream, bytes);
		}

		private void SendDatagram(DataType type, byte[] bytes = null)
		{
			Datagram data = new Datagram()
			{
				DataType = type,
				Bytes = bytes
			};
			SendDatagram(data);
		}

		private void SendDatagram(Datagram data)
		{
			m_RUDP.RequestSend(ref data, true);
		}

		private void Accept(byte[] bytes)
		{
			var ipbytes = new IPAddress(new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] });
			m_RUDP.SendAddress = ipbytes;
			m_RUDP.ConnectStart();
			SendDatagram(DataType.Accept);
		}

		private void Block()
		{
			//OnSend(DataType.Block);
		}
	}
}