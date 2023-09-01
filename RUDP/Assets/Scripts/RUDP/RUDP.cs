using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Khansho
{
	public class RUDP : BaseRUDP
	{
		public new event Action<Datagram> RecvCallBack = x => { };

		private readonly IntPtr m_StructPtr;

		public RUDP() : base(Datagram.SIZE)
		{
			m_StructPtr = GameUtils.GetIntPtr(Datagram.SIZE);

			base.RecvCallBack += Receive;
		}

		private void Receive(Packet packet)
		{
			Datagram data = Marshal.PtrToStructure<Datagram>(packet.Ptr);
			RecvCallBack(data);
		}

		public void RequestSend(Datagram data, bool reliable = true)
		{
			Marshal.StructureToPtr(data, m_StructPtr, true);
			RegistSendQueue(m_StructPtr, Datagram.HEADLENGTH + data.Bytes.Length, reliable);
		}
	}
}