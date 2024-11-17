using System;
using System.Collections.Generic;
using System.Diagnostics;
using Plugin.RDP.Bll;
using Plugin.RDP.UI;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.RDP
{
	/// <summary>http://www.codeproject.com/KB/cs/RemoteDesktop_CSharpNET.aspx</summary>
	public class PluginWindows : IPlugin, IPluginSettings<PluginSettings>
	{
		#region Fields
		private TraceSource _trace;
		private PluginSettings _settings;
		private Dictionary<String, DockState> _documentTypes;
		private RdpClientDlg _properties;
		#endregion Fields
		#region Properties
		internal TraceSource Trace => this._trace ?? (this._trace = PluginWindows.CreateTraceSource<PluginWindows>());

		internal IHostWindows HostWindows { get; }

		/// <summary>Настройки для взаимодействия из хоста</summary>
		Object IPluginSettings.Settings => this.Settings;

		/// <summary>Настройки для взаимодействия из плагина</summary>
		public PluginSettings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new PluginSettings(this);
					this.HostWindows.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
				}
				return this._settings;
			}
		}

		internal IMenuItem RdpClientMenu { get; set; }

		private Dictionary<String, DockState> DocumentTypes
		{
			get
			{
				if(this._documentTypes == null)
					this._documentTypes = new Dictionary<String, DockState>()
					{
						{ typeof(DocumentRdpClient).ToString(), DockState.Document },
						{ typeof(PanelRdpClient).ToString(), DockState.DockLeft },
					};
				return this._documentTypes;
			}
		}

		#endregion Properties
		public PluginWindows(IHostWindows hostWindows)
			=> this.HostWindows = hostWindows ?? throw new ArgumentNullException(nameof(hostWindows));

		public IWindow GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			IHostWindows host = this.HostWindows;
			if(host == null)
				this.Trace.TraceEvent(TraceEventType.Error, 10, "Plugin {0} requires {1}", this, typeof(IHostWindows));
			else
			{
				IMenuItem menuTools = host.MainMenu.FindMenuItem("Tools");
				if(menuTools == null)
					this.Trace.TraceEvent(TraceEventType.Error, 10, "Menu item 'Tools' not found");
				else
				{
					this.RdpClientMenu = menuTools.Create("RDP Client");
					this.RdpClientMenu.Name = "Tools.RdpClient";
					this.RdpClientMenu.Click += (sender, e) => { this.CreateWindow(typeof(PanelRdpClient).ToString(), true); };
					menuTools.Items.Add(this.RdpClientMenu);
					return true;
				}
			}
			return false;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this._properties != null)
			{
				this._properties.Dispose();
				this._properties = null;
			}

			if(this.RdpClientMenu != null)
				this.HostWindows.MainMenu.Items.Remove(this.RdpClientMenu);
			return true;
		}

		/// <summary>Показать окно создания нового RDP клиента</summary>
		/// <param name="parentRow">Родительский узел создаваемого клиента</param>
		internal void InvokeCreateClientDlg(SettingsDataSet.TreeRow parentRow)
		{
			if(this._properties == null)
				this._properties = new RdpClientDlg(this);

			this._properties.ToggleNewClientRow(parentRow);
			this._properties.Show();
			this._properties.Focus();
		}

		/// <summary>Показать окно изменений параметров RDP клиента</summary>
		/// <param name="row">Показать окно изменений параметров клиента</param>
		internal void InvokeModifyClientDlg(SettingsDataSet.TreeRow row)
		{
			if(this._properties == null)
				this._properties = new RdpClientDlg(this);

			this._properties.ToggleRdpClientRow(row);
			this._properties.Show();
			this._properties.Focus();
		}

		internal virtual void OnRowSelected(SettingsDataSet.TreeRow row)
		{
			if(this._properties != null && this._properties.Visible)
				this._properties.ToggleRdpClientRow(row);
		}

		internal IWindow CreateWindow(String typeName, Boolean searchForOpened, Object args = null)
			=> this.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}