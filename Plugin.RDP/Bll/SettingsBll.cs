using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using AlphaOmega.Bll;
using Plugin.RDP.UI;

namespace Plugin.RDP.Bll
{
	internal class SettingsBll : BllBase<SettingsDataSet,SettingsDataSet.TreeRow>
	{
		private readonly PluginWindows _plugin;

		/// <summary>Событие возникающее при создании или изменении клиента</summary>
		public event EventHandler<TreeRowEventArgs> RdpClientUpdated;

		/// <summary>Событие при изменении статуса подключения к клиенту</summary>
		public event EventHandler<RdpStateEventArgs> RdpClientStateChange;

		public SettingsBll(PluginWindows plugin)
			: base(0)
		{
			this._plugin = plugin;

			using(Stream stream = this._plugin.HostWindows.Plugins.Settings(this._plugin).LoadAssemblyBlob(Constant.SettingsFileName))
				if(stream != null)
					base.DataSet.ReadXml(stream);
		}

		public override void Save()
		{
			using(MemoryStream stream = new MemoryStream())
			{
				base.DataSet.WriteXml(stream);
				this._plugin.HostWindows.Plugins.Settings(this._plugin).SaveAssemblyBlob(Constant.SettingsFileName, stream);
			}
			//base.Save();
		}

		/// <summary>Событие вызываемое когда произошло подключение к клиенту</summary>
		/// <param name="clientName">Идентификатор клиента в дереве</param>
		public void OnClientConnect(Int32 treeId)
			=> this.OnClientChangeState(treeId, RdpStateEventArgs.StateType.Connect);

		/// <summary>Событие вызываемое при разъединении клиента от сервера</summary>
		/// <param name="treeId">Идентификатор клиента в дереве</param>
		public void OnClientDisconnect(Int32 treeId)
			=> this.OnClientChangeState(treeId, RdpStateEventArgs.StateType.Disconnect);

		/// <summary>Событие для изменения или передачи нового статуса для окна</summary>
		/// <param name="treeId">Идентификатор клиента в дереве</param>
		/// <param name="state">Треуемая операция для выполнения в окне клиента</param>
		public void OnClientChangeState(Int32 treeId, RdpStateEventArgs.StateType state)
			=> this.RdpClientStateChange?.Invoke(this, new RdpStateEventArgs(treeId, state));

		/// <summary>Событие при обновлении или добавлении клиента</summary>
		/// <param name="row">Ряд клиента, над которым произошло обновление</param>
		protected void OnClientUpdated(SettingsDataSet.TreeRow row)
			=> this.RdpClientUpdated?.Invoke(this, new TreeRowEventArgs(row));

		/// <summary>Получить узел дерева по идентификатору</summary>
		/// <param name="treeId">Идентификатор узла дерева</param>
		/// <returns>Ряд описывающий узел дерева</returns>
		public SettingsDataSet.TreeRow GetTreeNode(Int32 treeId)
			=> base.DataSet.Tree.FirstOrDefault(p => p.TreeID == treeId);

		/// <summary>Получить все дочерние узлы относительно родителя</summary>
		/// <param name="parentTreeId">Идентификатор родительского узла</param>
		/// <returns>Массив дочерних узлов</returns>
		public SettingsDataSet.TreeRow[] GetTreeNodes(Int32? parentTreeId)
			=> base.DataSet.Tree.Where(p => p.ParentTreeIDI == parentTreeId).OrderBy(p=>p.OrderId).ToArray();

		/// <summary>Получить настройки подключения RDP клиента по идентификатору узла</summary>
		/// <param name="treeId">Идентификатор узла по которому получить настройки подключения</param>
		/// <returns>Ряд описывающий настройки RDP клиента</returns>
		public SettingsDataSet.RdpClientRow GetClientRow(Int32 treeId)
			=> base.DataSet.RdpClient.FirstOrDefault(p => p.TreeID == treeId);

		/// <summary>Получить список компьютеров которые уже есть в настройках</summary>
		/// <returns>Массив компьютеров</returns>
		public String[] GetServerList()
			=> base.DataSet.RdpClient.Select(p => p.Server).Distinct().ToArray();

		/// <summary>Переместить узел по дереву в другую группу</summary>
		/// <param name="treeId">Идентификатор узла для перемещения</param>
		/// <param name="parentTreeId">Родительский идентификатор в который необходимо переместить узел</param>
		public void MoveNode(Int32 treeId, Int32? parentTreeId)
		{
			SettingsDataSet.TreeRow row = this.GetTreeNode(treeId);
			_ = row ?? throw new ArgumentNullException(nameof(treeId));

			if(parentTreeId != null)
			{
				SettingsDataSet.TreeRow parentRow = this.GetTreeNode(parentTreeId.Value)
					?? throw new ArgumentNullException(nameof(parentTreeId));

				if(parentRow.ElementType == ElementType.Client) throw new ArgumentException("Can't move to a client node");
			}

			if(row.ParentTreeIDI != parentTreeId)//Он уже в этом узле
			{
				row.BeginEdit();
				row.ParentTreeIDI = parentTreeId;
				row.AcceptChanges();
			}
		}

		/// <summary>Изменить узел дерева</summary>
		/// <param name="treeId">Идентификатор узла, если необходимо изменить. Или null, если узел необходимо добавить</param>
		/// <param name="parentTreeId">Идентификатор родительского узла</param>
		/// <param name="elementType">Тип добавляемого элемента</param>
		/// <param name="name">Наименование добавляемого элемента</param>
		/// <returns>Указатель на добавленный или изменённый код</returns>
		public SettingsDataSet.TreeRow ModifyTreeNode(Int32? treeId, Int32? parentTreeId, ElementType elementType, String name)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			SettingsDataSet.TreeRow row = treeId == null
				? base.DataSet.Tree.NewTreeRow()
				: this.GetTreeNode(treeId.Value);

			_ = row ?? throw new ArgumentNullException(nameof(treeId));

			row.BeginEdit();
			row.ParentTreeIDI = parentTreeId;
			row.ElementType = elementType;
			row.Name = name.Trim();

			if(row.RowState == DataRowState.Detached)
				base.DataSet.Tree.AddTreeRow(row);
			else
				row.AcceptChanges();

			return row;
		}

		/// <summary>Изменить наименование узла в дереве</summary>
		/// <param name="row">Ряд для изменения</param>
		/// <param name="name">Наименование узла</param>
		/// <returns>Смена узла прошла успешно</returns>
		public Boolean ModifyTreeNodeName(SettingsDataSet.TreeRow row, String name)
		{
			_ = row ?? throw new ArgumentNullException(nameof(row));
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			Boolean result = name != row.Name;
			if(result)
			{
				row.BeginEdit();
				row.Name = name.Trim();
				row.AcceptChanges();
			}
			return result;
		}

		/// <summary>Изменить или добавить настройки клиента</summary>
		/// <param name="treeRow">Ряд дерева настройки клиента которого необходимо изменить</param>
		/// <param name="server">Наименование сервера</param>
		/// <param name="userName">Логин клиента, который подключается к серверу</param>
		public void ModifyClient(SettingsDataSet.TreeRow treeRow, RdpClientDlg dlg)
		{
			if(treeRow.ElementType != ElementType.Client)
				throw new InvalidOperationException();

			SettingsDataSet.RdpClientRow clientRow = this.GetClientRow(treeRow.TreeID);
			if(clientRow == null)
				clientRow = base.DataSet.RdpClient.NewRdpClientRow();

			clientRow.BeginEdit();
			clientRow.TreeID = treeRow.TreeID;

			Type rowType = clientRow.GetType();
			foreach(PropertyInfo property in dlg.GetType().GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
				if(property.IsDefined(typeof(BindableAttribute), false))
				{
					Object value = property.GetValue(dlg, null);
					rowType.InvokeMember(property.Name, BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.SetProperty, null, clientRow, new Object[] { value, });
				}

			if(clientRow.RowState == DataRowState.Detached)
				base.DataSet.RdpClient.AddRdpClientRow(clientRow);
			else clientRow.AcceptChanges();

			this.OnClientUpdated(treeRow);
		}

		/// <summary>Удалить узел дерева со всеми дочерними узлами</summary>
		/// <param name="row">Ряд дерева для удаления</param>
		public void RemoveNode(SettingsDataSet.TreeRow row)
		{
			_ = row ?? throw new ArgumentNullException(nameof(row));

			switch(row.ElementType)
			{
				case ElementType.Tree:
					foreach(SettingsDataSet.TreeRow treeRow in this.GetTreeNodes(row.TreeID))
						this.RemoveNode(treeRow);
					base.DataSet.Tree.RemoveTreeRow(row);
					break;
				case ElementType.Client:
					this.RemoveClient(row);
					break;
				default:
					throw new NotImplementedException($"Element with type {row.ElementType} not implemented");
			}
		}

		/// <summary>Удалить узел клиента и настройки клиента</summary>
		/// <param name="treeRow">Ряд дерева для удаления</param>
		private void RemoveClient(SettingsDataSet.TreeRow treeRow)
		{
			_ = treeRow ?? throw new ArgumentNullException(nameof(treeRow));

			if(treeRow.ElementType == ElementType.Client)
			{
				SettingsDataSet.RdpClientRow clientRow = this.GetClientRow(treeRow.TreeID)
					?? throw new ApplicationException($"Row with client ID {treeRow.TreeID} not found");

				base.DataSet.RdpClient.RemoveRdpClientRow(clientRow);

				base.DataSet.Tree.RemoveTreeRow(treeRow);
			}
		}
	}
}