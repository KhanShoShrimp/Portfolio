using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace Khansho
{
	public partial class RUDP : IDisposable
	{
		public const int UDP_PAYLOAD_MAXSIZE = 65527;

		private Socket m_Socket;
		private int m_BufferSize;

		public event Action<Packet> RecvCallBack = x => { };
		public event Action<Packet> SendCallBack = x => { };

		public RUDP(int buffersize = UDP_PAYLOAD_MAXSIZE - Packet.HEAD_SIZE)
		{
			m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_Socket.ReceiveBufferSize = buffersize;
			m_Socket.SendBufferSize = buffersize;
			m_BufferSize = buffersize;
		}

		public virtual void Dispose()
		{
			ConnectEnd();
			ReceiveEnd();
			SendEnd();

			m_Socket?.Close();
			m_Socket?.Dispose();
			m_Socket = null;
		}
	}

	public partial class RUDP : IDisposable
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
					var packet = m_PacketPool.Dequeue();
					packet.Initialize();
					return packet;
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
		public IPAddress ReceiveAddress
		{
			get
			{
				return ((IPEndPoint)m_SendEndPoint).Address;
			}
			set
			{
				((IPEndPoint)m_SendEndPoint).Address = value;
			}
		}

		private bool m_UseReceive = false;
		public bool Receiving { get; private set; }

		public void ReceiveStart(int port)
		{
			if (m_Thread_RecvLoop != null)
			{
				return;
			}

			m_UseReceive = true;
			m_ReceiveEndPoint = new IPEndPoint(IPAddress.Any, port);
			m_Socket.Bind(m_ReceiveEndPoint);

			m_CancelTokenSource_RecvLoop = GameUtils.GetCancelToken();
			m_Thread_RecvLoop = new Thread(RecvLoop);
			m_Thread_RecvLoop.Start();

			EventDispatcher.UpdateEvent += OnRecv;
		}

		public void ReceiveEnd()
		{
			m_UseReceive = false;
			EventDispatcher.UpdateEvent -= OnRecv;

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
				m_LastRecvTime = 0;
			}
		}

		private void RecvLoop()
		{
			while (!m_CancelTokenSource_RecvLoop.IsCancellationRequested)
			{
				var packet = GetPacket();
				Rececive(packet);
				m_RecvPackets.Enqueue(packet);
			}
			m_CancelTokenSource_RecvLoop?.Dispose();
			m_CancelTokenSource_RecvLoop = null;
		}

		private void Rececive(Packet packet)
		{
			Receiving = true;
			try
			{
				packet.Length = m_Socket.ReceiveFrom(packet, SocketFlags.None, ref m_ReceiveEndPoint);
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
			Receiving = false;
		}
		#endregion
		#region Send
		private CancellationTokenSource m_CancelTokenSource_SendLoop;
		private Thread m_Thread_SendLoop;
		private ConcurrentQueue<Packet> m_SendQueue = new ConcurrentQueue<Packet>();

		private EndPoint m_SendEndPoint;
		public EndPoint SendEndPoint => m_SendEndPoint;
		public IPAddress SendAddress
		{
			get
			{
				return ((IPEndPoint)m_SendEndPoint).Address;
			}
			set
			{
				((IPEndPoint)m_SendEndPoint).Address = value;
			}
		}

		private bool m_UseSend = false;
		public bool Sending { get; private set; }

		public void SendStart(int port)
		{
			if (m_Thread_SendLoop != null)
			{
				return;
			}

			m_UseSend = true;
			m_SendEndPoint = new IPEndPoint(IPAddress.Any, port);
			m_CancelTokenSource_SendLoop = GameUtils.GetCancelToken();
			m_Thread_SendLoop = new Thread(SendLoop);
			m_Thread_SendLoop.Start();
		}

		public void SendEnd()
		{
			m_UseSend = false;
			try
			{
				m_CancelTokenSource_SendLoop?.Cancel();
			}
			catch (ObjectDisposedException) { }
			m_Thread_SendLoop = null;
		}

		public void Send(byte[] bytes, bool isReliable = true)
		{
			var packet = GetPacket();
			packet.CopyFrom(bytes);
			Send(packet, isReliable);
		}

		public void Send(IntPtr ptr, int length, bool isReliable = true)
		{
			var packet = GetPacket();
			packet.CopyFrom(ptr, length);
			Send(packet, isReliable);
		}

		public void Send(Packet packet, bool isReliable = true)
		{
			if (isReliable)
			{
				WriteHeader(packet);
				AddPacketInCheckingList(packet);
			}
			else
			{
				RegistSendQueue(packet);
			}
		}

		private void RegistSendQueue(Packet packet)
		{
			m_SendQueue.Enqueue(packet);
			m_LastSendTime = 0;
		}

		private void SendLoop()
		{
			while (!m_CancelTokenSource_SendLoop.IsCancellationRequested)
			{
				while (m_SendQueue.TryDequeue(out Packet packet))
				{
					Send(packet);

					if (!packet.Reliable)
					{
						ReleasePacket(packet);
					}
				}
			}
			m_CancelTokenSource_SendLoop?.Dispose();
			m_CancelTokenSource_SendLoop = null;
		}

		private void Send(Packet packet)
		{
			Sending = true;
			try
			{
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
			Sending = false;
		}
		#endregion
	}

	public partial class RUDP : IDisposable
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
		private Packet[] m_CheckingList = new Packet[256];
		private byte m_CheckingList_Send_Head = 255;
		private byte m_CheckingList_Send_Tail = 0;
		private byte m_CheckingList_Recv_Index = 0;
		private bool m_Checking = false;

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
			if (packet.Index != m_CheckingList_Recv_Index)
			{
				throw m_IndexMismatchException;
			}

			if (packet.Success)
			{
				m_CheckingList_Recv_Index++;
				RecvCallBack(packet);
			}

			packet.Sender = false;
			RegistSendQueue(packet);
		}

		private void SenderResponse(Packet packet)
		{
			if (packet.Index != m_CheckingList_Send_Head)
			{
				throw m_IndexMismatchException;
			}

			if (packet.Success)
			{
				SendCallBack(m_CheckingList[m_CheckingList_Send_Head]);
				ReleasePacket(m_CheckingList[m_CheckingList_Send_Head]);
				NextCheckPacket();
			}
			else
			{
				RegistSendQueue(m_CheckingList[packet.Index]);
			}
		}

		private void NextCheckPacket()
		{
			var next = (byte)(m_CheckingList_Send_Head + 1);
			if (next != m_CheckingList_Send_Tail)
			{
				m_Checking = true;
				m_CheckingList_Send_Head = next;
				RegistSendQueue(m_CheckingList[m_CheckingList_Send_Head]);
			}
			else
			{
				m_Checking = false;
			}
		}

		private void AddPacketInCheckingList(Packet packet)
		{
			if (m_CheckingList_Send_Head == m_CheckingList_Send_Tail)
			{
				throw new InvalidOperationException("Sending Array is full");
			}

			packet.Index = m_CheckingList_Send_Tail;
			m_CheckingList[m_CheckingList_Send_Tail++] = packet;

			if (!m_Checking)
			{
				NextCheckPacket();
			}
		}
		#endregion
		#region Connect
		private const float TIMEOUT = 5;
		private const float ACK_LOOPTIME = 1;
		private float m_LastSendTime = -1;
		private float m_LastRecvTime = -1;

		public bool IsConnect { get; private set; }

		public void ConnectStart()
		{
			IsConnect = true;
			EventDispatcher.CountingEvent += CountingResendingTime;
		}

		private void ConnectEnd()
		{
			IsConnect = false;
			EventDispatcher.CountingEvent -= CountingResendingTime;
		}

		private void CountingResendingTime(float delta)
		{
			if (m_UseSend && m_LastSendTime < ACK_LOOPTIME)
			{
				m_LastSendTime += delta;
			}
			else
			{
				if (m_Checking)
				{
					RegistSendQueue(m_CheckingList[m_CheckingList_Send_Head]);
				}
				else
				{
					var packet = GetPacket();
					packet.Reliable = false;
					RegistSendQueue(packet);
				}
			}

			if (m_UseReceive && m_LastRecvTime < TIMEOUT)
			{
				m_LastRecvTime += delta;
			}
			else
			{
				m_LastRecvTime = 0;

				ConnectEnd();
				if (m_UseSend)
				{
					SendEnd();
				}
				if (m_UseReceive)
				{
					ReceiveEnd();
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