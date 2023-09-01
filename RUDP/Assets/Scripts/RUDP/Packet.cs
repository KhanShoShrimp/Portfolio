using System;
using System.Net;
using System.Text;
using UnityEngine;

namespace Khansho
{
	public partial class Packet
	{
		public const int HEAD_SIZE = 8;

		private Buffer m_Buffer;

		public Packet(int size)
		{
			m_Buffer = new Buffer(size + HEAD_SIZE);
		}

		public static implicit operator byte[](Packet packet)
		{
			return packet.m_Buffer;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Reliable : ");
			sb.Append(Reliable);
			sb.Append(", Sender : ");
			sb.Append(Sender);
			sb.Append(", Success : ");
			sb.Append(Success);
			sb.Append(", Length : ");
			sb.Append(Length);
			sb.Append(", ");
			sb.Append(m_Buffer);
			return sb.ToString();
		}
	}

	public partial class Packet
	{
		private const int HEAD_FLAG = 0;
		private const int HEAD_INDEX = 1;
		private const int HEAD_LENGTH = 2;
		private const int HEAD_CRC16 = 6;
		private const int HEAD_SENDADDRESS = 8;
		private const int HEAD_RECVADDRESS = 12;

		private const byte FLAG_RELIABLE = 1 << 0;
		private const byte FLAG_SENDER = 1 << 1;
		private const byte FLAG_SUCCESS = 1 << 2;

		public IntPtr Ptr => m_Buffer.Ptr + HEAD_SIZE;

		public bool Reliable
		{
			get
			{
				return m_Buffer.HasFlags(HEAD_FLAG, FLAG_RELIABLE);
			}
			set
			{
				m_Buffer.SetFlags(HEAD_FLAG, FLAG_RELIABLE, value);
			}
		}

		public bool Sender
		{
			get
			{
				return m_Buffer.HasFlags(HEAD_FLAG, FLAG_SENDER);
			}
			set
			{
				m_Buffer.SetFlags(HEAD_FLAG, FLAG_SENDER, value);
			}
		}

		public bool Success
		{
			get
			{
				return m_Buffer.HasFlags(HEAD_FLAG, FLAG_SUCCESS);
			}
			set
			{
				m_Buffer.SetFlags(HEAD_FLAG, FLAG_SUCCESS, value);
			}
		}

		public byte Index
		{
			get
			{
				return m_Buffer[HEAD_INDEX];
			}
			set
			{
				m_Buffer[HEAD_INDEX] = value;
			}
		}

		public int Length
		{
			get
			{
				return m_Buffer.Length;
			}
			set
			{
				m_Buffer.Length = value;
			}
		}

		//public IPAddress SendAddress
		//{
		//	get
		//	{
		//		return new IPAddress(new byte[] {
		//			m_Buffer[HEAD_SENDADDRESS],
		//			m_Buffer[HEAD_SENDADDRESS + 1],
		//			m_Buffer[HEAD_SENDADDRESS + 2],
		//			m_Buffer[HEAD_SENDADDRESS + 3]});
		//	}
		//	set
		//	{
		//		var bytes = value.GetAddressBytes();
		//		m_Buffer[HEAD_SENDADDRESS] = bytes[0];
		//		m_Buffer[HEAD_SENDADDRESS + 1] = bytes[1];
		//		m_Buffer[HEAD_SENDADDRESS + 2] = bytes[2];
		//		m_Buffer[HEAD_SENDADDRESS + 3] = bytes[3];
		//	}
		//}

		//public IPAddress RecvAddress
		//{
		//	get
		//	{
		//		return new IPAddress(new byte[] {
		//			m_Buffer[HEAD_RECVADDRESS],
		//			m_Buffer[HEAD_RECVADDRESS + 1],
		//			m_Buffer[HEAD_RECVADDRESS + 2],
		//			m_Buffer[HEAD_RECVADDRESS + 3]});
		//	}
		//	set
		//	{
		//		var bytes = value.GetAddressBytes();
		//		m_Buffer[HEAD_RECVADDRESS] = bytes[0];
		//		m_Buffer[HEAD_RECVADDRESS + 1] = bytes[1];
		//		m_Buffer[HEAD_RECVADDRESS + 2] = bytes[2];
		//		m_Buffer[HEAD_RECVADDRESS + 3] = bytes[3];
		//	}
		//}

		public void WriteLength()
		{
			m_Buffer.Write(HEAD_LENGTH, m_Buffer.Length);
		}

		public void WriteCRC16()
		{
			var crc16 = ((byte[])m_Buffer).CRC16_byte(HEAD_SIZE, m_Buffer.Length - HEAD_SIZE);
			m_Buffer.Write(HEAD_CRC16, crc16);
		}

		public bool CheckLength()
		{
			return m_Buffer.ReadInt(HEAD_LENGTH) != m_Buffer.Length;
		}

		public bool CheckCRC16()
		{
			var read = m_Buffer.ReadUShort(HEAD_CRC16);
			var calc = ((byte[])m_Buffer).CRC16_byte(HEAD_SIZE, m_Buffer.Length - HEAD_SIZE);
			return read != calc;
		}

		public void CopyFrom(byte[] bytes)
		{
			m_Buffer.CopyFrom(HEAD_SIZE, bytes, 0, bytes.Length);
		}

		public void CopyFrom(IntPtr ptr, int length)
		{
			m_Buffer.CopyFrom(HEAD_SIZE, ptr, 0, length);
		}

		public void CopyTo(byte[] bytes)
		{
			m_Buffer.CopyTo(bytes, 0, HEAD_SIZE, m_Buffer.Length - HEAD_SIZE);
		}

		public void CopyTo(IntPtr ptr)
		{
			m_Buffer.CopyTo(ptr, 0, HEAD_SIZE, m_Buffer.Length - HEAD_SIZE);
		}
	}
}