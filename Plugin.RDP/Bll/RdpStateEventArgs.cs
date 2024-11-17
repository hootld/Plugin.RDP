using System;

namespace Plugin.RDP.Bll
{
	/// <summary>Аргументы подключения или отключения клиента</summary>
	internal class RdpStateEventArgs : EventArgs
	{
		public enum StateType
		{
			Connect,
			Disconnect,
			Focus,
		}

		/// <summary>Идентификатор клиента</summary>
		public Int32 TreeId { get; private set; }

		/// <summary>Клиента надо подключить, отключить или сфокусироваться</summary>
		public StateType State { get; private set; }

		public RdpStateEventArgs(Int32 treeId, StateType state)
		{
			this.TreeId = treeId;
			this.State = state;
		}
	}
}