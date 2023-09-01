using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Khansho
{
	public class Server : IDisposable
	{
		private RUDP m_RUDP;

		private bool IsConnect = false;

		public Server(int recvport, int sendport)
		{
			m_RUDP = new RUDP();
			m_RUDP.SendStart(IPAddress.Any, sendport);
			m_RUDP.ReceiveStart(recvport);
			m_RUDP.RecvCallBack += OnReceive;
		}

		public void Dispose()
		{
			m_RUDP?.Dispose();
			m_RUDP = null;
		}

		private void OnReceive(Datagram data)
		{
			Debug.Log($"server recv : {data}");
			
			switch (data.DataType)
			{
				case DataType.TryConnect:
					if (!IsConnect)
					{
						IsConnect = true;
						Accept(data.Bytes); 
					}
					else
					{
						Block();
					}
					break;
			}
		}
		#region Send
		public void OnSend(DataType type, byte[] bytes = null)
		{
			Datagram packet = new Datagram()
			{
				DataType = type,
				Bytes = bytes ?? new byte[0]
			};
			OnSend(packet);
		}

		public void OnSend(Datagram data)
		{
			Debug.Log($"server send : {data}");
			m_RUDP.RequestSend(data, true);
		}
		#endregion
		#region Accept
		private void Accept(byte[] bytes)
		{
			var ipbytes = new IPAddress(new byte[] { bytes[0], bytes[1], bytes[2], bytes[3] });
			((IPEndPoint)m_RUDP.SendEndPoint).Address = ipbytes;
			OnSend(DataType.Accept);
		}

		private void Block()
		{
			//컴퓨터가 1대뿐이라 테스트에 어려움이 있음.
			//OnSend(DataType.Block);
		}
		#endregion
		#region ACK
		private const float PING_THRESHOLD = 1f;
		private float m_PingTime = 0;

		private void StartACK()
		{
			EventDispatcher.UpdateEvent += CheckACK;
		}

		private void EndACK()
		{
			EventDispatcher.UpdateEvent -= CheckACK;
		}

		private void ResetACK()
		{
			m_PingTime = 0;
		}

		private void CheckACK()
		{
			if (m_PingTime < PING_THRESHOLD)
			{
				m_PingTime += Time.deltaTime;
			}
			else
			{
				ResetACK();
				OnSend(DataType.Ack);
			}
		}
		#endregion
		#region TimeOut
		private const float TIMEOUT_THRESHOLD = 10f;
		private float m_ElapsedTime = 0;

		private bool m_IsTimeOut = false;

		private void StartTimeOut()
		{
			EventDispatcher.UpdateEvent += CheckTimeOut;
		}

		private void EndTimeOut()
		{
			EventDispatcher.UpdateEvent -= CheckTimeOut;
		}

		private void ResetTimeOut()
		{
			m_ElapsedTime = 0;
		}

		private void CheckTimeOut()
		{
			if (m_ElapsedTime < TIMEOUT_THRESHOLD)
			{
				m_ElapsedTime += Time.deltaTime;
			}
			else
			{
				TimeOut();
			}
		}

		private void TimeOut()
		{
			EndACK();
			EndTimeOut();

			OnSend(DataType.TimeOut);

			m_IsTimeOut = true;

			StartQuit();
		}
		#endregion
		#region Quit
		private async void StartQuit()
		{
			while (m_RUDP.Sending)
			{
				await Task.Yield();
			}
			Dispose();
		}
		#endregion
	}
}