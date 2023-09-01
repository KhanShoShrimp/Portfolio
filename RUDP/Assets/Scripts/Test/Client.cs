using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;
using Ping = System.Net.NetworkInformation.Ping;

namespace Khansho
{
	public class Client : IDisposable
	{
		private RUDP m_RUDP;
		public event Action AcceptCallback = () => { };
		public event Action<byte[]> RecvCallback = x => { };

		public Client(int recvport, int sendport)
		{
			m_RUDP = new RUDP();
			m_RUDP.SendStart(IPAddress.Any, sendport);
			m_RUDP.ReceiveStart(recvport);
			m_RUDP.RecvCallBack += OnReceive;
		}

		public void Dispose()
		{
			m_RUDP?.Dispose();
			m_Ping?.Dispose();
			m_RUDP = null;
			m_Ping = null;
		}

		private void OnReceive(Datagram data)
		{
			Debug.Log($"client recv : {data}");

			switch (data.DataType)
			{
				case DataType.None:
					RecvCallback(data.Bytes);
					break;

				case DataType.Accept:
					AcceptCallback();
					break;
				case DataType.Block:
					break;

				case DataType.TimeOut:
					Debug.Log("TimeOut");
					Dispose();
					break;
			}
		}
		#region Send
		public void OnSend(DataType type, byte[] bytes = null)
		{
			Datagram packet = new Datagram()
			{
				DataType = type,
				Bytes = bytes
			};
			OnSend(packet);
		}

		public void OnSend(Datagram data)
		{
			Debug.Log($"client send : {data}");
			m_RUDP.RequestSend(data, true);
		}
		#endregion
		#region Connect
		private Ping m_Ping = new Ping();

		private bool Ping(IPAddress ipaddress)
		{
			var result = m_Ping.Send(ipaddress, 500);
			return result.Status == System.Net.NetworkInformation.IPStatus.Success;
		}

		public bool TryConnect(IPAddress ipaddress)
		{
			if (Ping(ipaddress))
			{
				((IPEndPoint)m_RUDP.SendEndPoint).Address = ipaddress;
				OnSend(DataType.TryConnect, NetworkInfo.ExternalIPAddress.GetAddressBytes()); //new byte[] { 127, 0, 0, 1 });
				return true;
			}
			else
			{
				Debug.Log($"ping {ipaddress} : false");
				return false;
			}
		}
		#endregion
	}
}