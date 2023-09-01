using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Khansho
{
	public partial class Buffer
	{
		private byte[] m_Bytes;
		private int m_Size;
		public int Length;

		private GCHandle m_Handle;
		public IntPtr Ptr { get; private set; }

		public Buffer(int size)
		{
			m_Bytes = new byte[size];
			m_Handle = GCHandle.Alloc(m_Bytes, GCHandleType.Pinned);
			m_Size = size;
			Length = 0;
			Ptr = m_Handle.AddrOfPinnedObject();

			EventDispatcher.DestroyEvent += Release;
		}

		~Buffer()
		{
			Release();
		}

		private void Release()
		{
			EventDispatcher.DestroyEvent -= Release;

			if (m_Handle != null && m_Handle.IsAllocated)
			{
				m_Handle.Free();
			}
		}

		public byte this[int index]
		{
			get
			{
				return m_Bytes[index];
			}
			set
			{
				m_Bytes[index] = value;
				Length = Mathf.Max(Length, index + 1);
			}
		}

		public static implicit operator byte[](Buffer buffer)
		{
			return buffer.m_Bytes;
		}

		public override string ToString()
		{
			return $"BufferLength : {Length}, Values : {m_Bytes.ToStr(Length)}";
		}
	}

	//Utils
	public partial class Buffer
	{
		public void SetFlags(int index, byte flags, bool value)
		{
			if (value)
			{
				m_Bytes[index] |= flags;
			}
			else
			{
				m_Bytes[index] = (byte)(m_Bytes[0] & ~flags);
			}
			Length = Mathf.Max(Length, index + 1);
		}

		public bool HasFlags(int index, byte flags)
		{
			return (m_Bytes[index] & flags) != 0;
		}

		public void Write(int index, short value)
		{
			m_Bytes[index] = (byte)(value & 0xff);
			m_Bytes[index + 1] = (byte)(value >> 8);
			Length = Mathf.Max(Length, index + 2);
		}

		public void Write(int index, ushort value)
		{
			m_Bytes[index] = (byte)(value & 0xff);
			m_Bytes[index + 1] = (byte)(value >> 8);
			Length = Mathf.Max(Length, index + 2);
		}

		public void Write(int index, int value)
		{
			m_Bytes[index] = (byte)(value & 0xff);
			m_Bytes[index + 1] = (byte)((value >> 8) & 0xff);
			m_Bytes[index + 2] = (byte)((value >> 16) & 0xff);
			m_Bytes[index + 3] = (byte)(value >> 24);
			Length = Mathf.Max(Length, index + 4);
		}

		public void Write(int index, uint value)
		{
			m_Bytes[index] = (byte)(value & 0xff);
			m_Bytes[index + 1] = (byte)((value >> 8) & 0xff);
			m_Bytes[index + 2] = (byte)((value >> 16) & 0xff);
			m_Bytes[index + 3] = (byte)(value >> 24);
			Length = Mathf.Max(Length, index + 4);
		}

		public void Write(int index, long value)
		{
			for (int i = 0; i < 8; i++)
			{
				m_Bytes[index + i] = (byte)((value >> i * 8) & 0xff);
			}
			Length = Mathf.Max(Length, index + 8);
		}

		public void Write(int index, ulong value)
		{
			for (int i = 0; i < 8; i++)
			{
				m_Bytes[index + i] = (byte)((value >> i * 8) & 0xff);
			}
			Length = Mathf.Max(Length, index + 8);
		}

		public short ReadShort(int index)
		{
			return (short)((m_Bytes[index]) | (m_Bytes[index + 1] << 8));
		}

		public ushort ReadUShort(int index)
		{
			return (ushort)((m_Bytes[index]) | (m_Bytes[index + 1] << 8));
		}

		public int ReadInt(int index)
		{
			return m_Bytes[index] | (m_Bytes[index + 1] << 8) | (m_Bytes[index + 2] << 16) | (m_Bytes[index + 3] << 24);
		}

		public uint ReadUInt(int index)
		{
			return (uint)(m_Bytes[index] | (m_Bytes[index + 1] << 8) | (m_Bytes[index + 2] << 16) | (m_Bytes[index + 3] << 24));
		}

		public long ReadLong(int index)
		{
			long value = 0;
			for (int i = 0; i < 8; i++)
			{
				value |= (byte)(m_Bytes[index + i] << i * 8);
			}
			return value;
		}

		public ulong ReadULong(int index)
		{
			ulong value = 0;
			for (int i = 0; i < 8; i++)
			{
				value |= (byte)(m_Bytes[index + i] << i * 8);
			}
			return value;
		}

		public void CopyFrom(byte[] src)
		{
			CopyFrom(src, src.Length);
		}

		public void CopyFrom(byte[] src, int length)
		{
			CopyFrom(0, src, 0, length);
		}

		public void CopyFrom(int offsetdst, byte[] src, int offsetsrc, int length)
		{
			if (offsetsrc + length > src.Length | offsetdst + length > m_Size)
			{
				throw new OutOfMemoryException();
			}
			Length = offsetdst + length;
			GameUtils.BlockCopy(m_Bytes, offsetdst, src, offsetsrc, length);
		}

		public void CopyFrom(IntPtr src, int length)
		{
			CopyFrom(0, src, 0, length);
		}

		public void CopyFrom(int offsetdst, IntPtr src, int offsetsrc, int length)
		{
			if (offsetdst + length > m_Size)
			{
				throw new OutOfMemoryException();
			}
			Length = offsetdst + length;
			GameUtils.MemCopy(Ptr, offsetdst, src, offsetsrc, length);
		}

		public void CopyTo(byte[] dst)
		{
			CopyTo(dst, Length);
		}

		public void CopyTo(byte[] dst, int length)
		{
			CopyTo(dst, 0, 0, length);
		}

		public void CopyTo(byte[] dst, int offsetdst, int offsetsrc, int length)
		{
			if (offsetdst + length > dst.Length | offsetsrc + length > m_Size)
			{
				throw new OutOfMemoryException();
			}
			GameUtils.BlockCopy(dst, offsetsrc, m_Bytes, offsetdst, length);
		}

		public void CopyTo(IntPtr dst, int length)
		{
			CopyTo(dst, 0, 0, length);
		}

		public void CopyTo(IntPtr dst, int offsetdst, int offsetsrc, int length)
		{
			if (offsetsrc + length > m_Size)
			{
				throw new OutOfMemoryException();
			}
			GameUtils.MemCopy(dst, offsetdst, Ptr, offsetsrc, length);
		}
	}
}