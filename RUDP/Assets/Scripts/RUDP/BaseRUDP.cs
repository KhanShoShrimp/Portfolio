using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEditor.Sprites;
using UnityEngine;

namespace Khansho
{
	public partial class BaseRUDP : IDisposable
	{
		public const int UDP_PAYLOAD_MAXSIZE = 65527;

		private Socket m_Socket;
		private int m_BufferSize;

		public event Action<Packet> RecvCallBack = x => { };

		public BaseRUDP(int buffersize = UDP_PAYLOAD_MAXSIZE - Packet.HEAD_SIZE)
		{
			m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_Socket.ReceiveBufferSize = buffersize;
			m_Socket.SendBufferSize = buffersize;
			m_BufferSize = buffersize;

			EventDispatcher.UpdateEvent += Update;
		}

		public virtual void Dispose()
		{
			EventDispatcher.UpdateEvent -= Update;

			ReceiveEnd();
			SendEnd();

			m_Socket?.Close();
			m_Socket?.Dispose();
			m_Socket = null;
		}

		private void Update()
		{
			OnRecv();
			CountingResendingTime();
		}
	}

	public partial class BaseRUDP : IDisposable
	{
		private CircularQueue<Packet> m_PacketPool = new CircularQueue<Packet>(256);

		private Packet GetPacket()
		{
			lock (m_PacketPool)
			{
				if (m_PacketPool.IsEmpty())
				{
					return new Packet(m_BufferSize);
				}
				else
				{
					return m_PacketPool.Dequeue();
				}
			}
		}

		private void ReleasePacket(Packet packet)
		{
			lock (m_PacketPool)
			{
				m_PacketPool.Enqueue(packet);
			}
		}
		#region Rececive
		private CancellationTokenSource m_CancelTokenSource_RecvLoop;
		private Thread m_Thread_RecvLoop;
		private ConcurrentQueue<Packet> m_RecvPackets = new ConcurrentQueue<Packet>();
		private EndPoint m_ReceiveEndPoint;

		public EndPoint ReceiveEndPoint => m_ReceiveEndPoint;
		public bool Receiving { get; private set; }

		public void ReceiveStart(int port)
		{
			if (m_Thread_RecvLoop != null)
			{
				return;
			}

			m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, port);
			m_Socket.Bind(m_ReceiveEndPoint);

			m_CancelTokenSource_RecvLoop = GameUtils.GetCancelToken();
			m_Thread_RecvLoop = new Thread(RecvLoop);
			m_Thread_RecvLoop.Start();
		}

		public void ReceiveEnd()
		{
			try
			{
				m_CancelTokenSource_RecvLoop?.Cancel();
			}
			catch (ObjectDisposedException) { }
			m_Thread_RecvLoop = null;
		}

		private void OnRecv()
		{
			while (m_RecvPackets.TryDequeue(out Packet packet))
			{
				if (packet.Reliable)
				{
					ReadHeader(packet);
					Response(packet);
				}
				else
				{
					RecvCallBack(packet);
				}
				ReleasePacket(packet);
			}
		}

		private void RecvLoop()
		{
			while (!m_CancelTokenSource_RecvLoop.IsCancellationRequested)
			{
				var packet = GetPacket();
				Rececive(packet);
				m_RecvPackets.Enqueue(packet);
				Receiving = true;
			}
			m_CancelTokenSource_RecvLoop?.Dispose();
			m_CancelTokenSource_RecvLoop = null;
		}

		private void Rececive(Packet packet)
		{
			try
			{
				packet.Length = m_Socket.ReceiveFrom(packet, SocketFlags.None, ref m_ReceiveEndPoint);
				Debug.Log($"recv : {packet}");
			}
			catch (SocketException e)
			{
				switch (e.SocketErrorCode)
				{
					default:
						Debug.Log(e.SocketErrorCode);
						break;
					case SocketError.Interrupted:
						break;
				}
			}
		}
		#endregion
		#region Send
		private CancellationTokenSource m_CancelTokenSource_SendLoop;
		private Thread m_Thread_SendLoop;
		private ConcurrentQueue<Packet> m_SendQueue = new ConcurrentQueue<Packet>();
		private EndPoint m_SendEndPoint;

		public EndPoint SendEndPoint => m_SendEndPoint;
		public bool Sending { get; private set; }

		public void SendStart(IPAddress ipAddress, int port)
		{
			if (m_Thread_SendLoop != null)
			{
				return;
			}

			m_SendEndPoint = new IPEndPoint(ipAddress, port);
			m_CancelTokenSource_SendLoop = GameUtils.GetCancelToken();
			m_Thread_SendLoop = new Thread(SendLoop);
			m_Thread_SendLoop.Start();
		}

		public void SendEnd()
		{
			try
			{
				m_CancelTokenSource_SendLoop?.Cancel();
			}
			catch (ObjectDisposedException) { }
			m_Thread_SendLoop = null;
		}

		public void RegistSendQueue(byte[] bytes, bool isReliable = true)
		{
			var packet = GetPacket();
			packet.CopyFrom(bytes);
			RegistSendQueue(packet, isReliable);
		}

		public void RegistSendQueue(IntPtr ptr, int length, bool isReliable = true)
		{
			var packet = GetPacket();
			packet.CopyFrom(ptr, length);
			RegistSendQueue(packet, isReliable);
		}

		public void RegistSendQueue(Packet packet, bool isReliable = true)
		{
			if (isReliable)
			{
				WriteHeader(packet);
				EnqueuePacketInCheckings(packet);
			}
			m_SendQueue.Enqueue(packet);
		}

		private void SendLoop()
		{
			while (!m_CancelTokenSource_SendLoop.IsCancellationRequested)
			{
				while (m_SendQueue.TryDequeue(out Packet packet))
				{
					Sending = true;
					Send(packet);
					if (!packet.Reliable)
					{
						ReleasePacket(packet);
					}
				}
				Sending = false;
			}
			m_CancelTokenSource_SendLoop?.Dispose();
			m_CancelTokenSource_SendLoop = null;
		}

		private void Send(Packet packet)
		{
			try
			{
				Debug.Log($"send : {packet}");
				m_Socket.SendTo(packet, packet.Length, SocketFlags.None, m_SendEndPoint);
			}
			catch (SocketException e)
			{
				switch (e.SocketErrorCode)
				{
					default:
						Debug.Log(e.SocketErrorCode);
						break;
					case SocketError.AddressNotAvailable:
						//잘못된 IP 에러 처리 작업.
						break;
				}
			}
		}
		#endregion
	}

	public partial class BaseRUDP : IDisposable
	{
		private void WriteHeader(Packet packet)
		{
			packet.Reliable = true;
			packet.Sender = true;
			packet.WriteLength();
			packet.WriteCRC16();
		}

		private void ReadHeader(Packet packet)
		{
			try
			{
				if (packet.Length - Packet.HEAD_SIZE > m_BufferSize)
				{
					Debug.Log("failed overflow");
					throw m_BufferOverflowException;
				}
				if (packet.CheckLength())
				{
					Debug.Log("failed length");
					throw m_LengthMismatchException;
				}
				if (packet.CheckCRC16())
				{
					Debug.Log("failed CRC16");
					throw m_CRCMismatchException;
				}
				packet.Success = true;
			}
			catch
			{
				packet.Success = false;
			}
		}
		#region CheckingTransmission
		private Packet[] m_CheckingSendPackets = new Packet[256];
		private byte m_Checking_Send_Head = 255;
		private byte m_Checking_SendTail = 0;
		private byte m_Checking_Recv_Index = 0;

		private void Response(Packet packet)
		{
			if (packet.Sender)
			{
				ReceiverResponse(packet);
			}
			else
			{
				SenderResponse(packet);
			}
		}

		private void ReceiverResponse(Packet packet)
		{
			if (packet.Success)
			{
				RecvCallBack(packet);
			}
			packet.Sender = false;
			m_SendQueue.Enqueue(packet);
		}

		private void SenderResponse(Packet packet)
		{
			if (packet.Index != m_Checking_Send_Head)
			{
				throw m_IndexMismatchException;
			}

			if (packet.Success)
			{
				ReleasePacket(m_CheckingSendPackets[m_Checking_Send_Head]);
				NextCheckPacket();
			}
			else
			{
				m_SendQueue.Enqueue(m_CheckingSendPackets[packet.Index]);
			}
		}

		private void EnqueuePacketInCheckings(Packet packet)
		{
			if (m_Checking_SendTail == m_Checking_Send_Head)
			{
				throw new InvalidOperationException("Sending Array is full");
			}

			packet.Index = m_Checking_SendTail;
			m_CheckingSendPackets[m_Checking_SendTail++] = packet;

			if (!Resend)
			{
				NextCheckPacket();
			}
		}

		private void NextCheckPacket()
		{
			var next = (byte)(m_Checking_Send_Head + 1);
			if (next != m_Checking_SendTail)
			{
				m_Checking_Send_Head = next;
				Resend = true;
			}
			else
			{
				Resend = false;
			}
		}
		#endregion
		#region TimeOut
		private const float TIMEOUT = 1;
		private float m_Time = -1;

		private bool Resend = false;

		//private bool Switch_Resend
		//{
		//	get
		//	{
		//		return m_ResendingTime >= 0;
		//	}
		//	set
		//	{
		//		m_ResendingTime = value ? 0 : -1;
		//	}
		//}

		private void CountingResendingTime()
		{
			if (m_Time < TIMEOUT)
			{
				m_Time += Time.deltaTime;
			}
			else
			{
				m_Time = 0;

				if (Resend)
				{
					m_SendQueue.Enqueue(m_CheckingSendPackets[m_Checking_Send_Head]);
				}
				else
				{
					var packet = GetPacket();
					packet.Reliable = false;
					packet.CopyFrom(new byte[1] { 0 });
					m_SendQueue.Enqueue(packet);
				}
			}
		}
		#endregion
		#region Exception
		private IndexMismatchException m_IndexMismatchException = new IndexMismatchException();
		public class IndexMismatchException : Exception
		{
			public override string Message => "Index is incorrect.";
		}

		private LengthMismatchException m_LengthMismatchException = new LengthMismatchException();
		public class LengthMismatchException : Exception
		{
			public override string Message => "Length is incorrect.";
		}

		private CheckSumMismatchException m_CheckSumMismatchException = new CheckSumMismatchException();
		public class CheckSumMismatchException : Exception
		{
			public override string Message => "CheckSum is incorrect.";
		}

		private CRCMismatchException m_CRCMismatchException = new CRCMismatchException();
		public class CRCMismatchException : Exception
		{
			public override string Message => "CRC is incorrect.";
		}

		private BufferOverflowException m_BufferOverflowException = new BufferOverflowException();
		public class BufferOverflowException : Exception
		{
			public override string Message => "Data length exceeds buffer capacity.";
		}
		#endregion
	}
}