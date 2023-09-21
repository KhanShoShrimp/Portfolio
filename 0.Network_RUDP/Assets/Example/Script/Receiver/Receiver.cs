using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Ping = System.Net.NetworkInformation.Ping;

namespace Khansho.Example1
{
	public class Receiver : MonoBehaviour
	{
		private const int SERVERPORT = 15000;
		private const int CLIENTPORT = 15001;

		[SerializeField] private ReceiverWebCam m_WebCam;
		[SerializeField] private UnityEvent m_OnConnected;

		private ExampleRUDP m_RUDP;

		private void Start()
		{
			InitRDUP();
		}

		private void OnDestroy()
		{
			DisposeRDUP();
		}

		private void InitRDUP()
		{
			m_RUDP = new ExampleRUDP();

			m_RUDP.SendStart(SERVERPORT);
			m_RUDP.ReceiveStart(CLIENTPORT);
			m_RUDP.RecvCallBack += OnReceive;
		}

		private void DisposeRDUP()
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

				case DataType.BeginStream:
					var bytes = data.Bytes;
					int width = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
					int height = bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24);
					m_WebCam.Resize(width, height);
					m_WebCam.ClearTexture();
					break;

				case DataType.DataStream:
					m_WebCam.WriteTexture(data.Bytes);
					break;

				case DataType.EndStream:
					m_WebCam.UpdateTexture();
					break;

				case DataType.Accept:
					m_OnConnected.Invoke();
					m_RUDP.ConnectStart();
					break;

				case DataType.Block:
					break;
			}
		}

		private void OnSend(byte[] bytes = null)
		{
			OnSend(DataType.DataStream, bytes);
		}

		private void OnSend(DataType type, byte[] bytes = null)
		{
			Datagram data = new Datagram()
			{
				DataType = type,
				Bytes = bytes
			};
			OnSend(data);
		}

		private void OnSend(Datagram data)
		{
			m_RUDP.RequestSend(ref data, true);
		}

		public void TryConnect(IPAddress ipaddress)
		{
			m_RUDP.SendAddress = ipaddress;
			OnSend(DataType.TryConnect, NetworkInfo.ExternalIPAddress.GetAddressBytes());
		}
	}
}