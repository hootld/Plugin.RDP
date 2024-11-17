using System;
using System.Drawing;

namespace Plugin.RDP.Bll
{
	internal class DesktopSizeParser
	{
		private String _desktopSize;

		/// <summary>Развернуть на весь экран</summary>
		public Boolean FullScreen
		{
			get => this._desktopSize == "1";
			set => this._desktopSize = "1";
		}

		/// <summary>Не больше размера клиента</summary>
		public Boolean SameAsClient
		{
			get => this._desktopSize == "0";
			set => this._desktopSize = "0";
		}

		/// <summary>Размер экрана</summary>
		public Size Size
		{
			get
			{
				if(this.FullScreen || this.SameAsClient)
					return Size.Empty;
				else
				{
					String[] arr = this._desktopSize.Split('x');
					return new Size(Int32.Parse(arr[0]), Int32.Parse(arr[1]));
				}
			}
			set
			{
				if(value.IsEmpty)
					this.SameAsClient = true;
				else
					this._desktopSize = String.Format("{0}x{1}", value.Width, value.Height);
			}
		}

		public DesktopSizeParser()
			: this(String.Empty)
			=> this.SameAsClient = true;

		public DesktopSizeParser(String desktopSize)
			=> this._desktopSize = desktopSize;

		public override String ToString()
			=> this._desktopSize;
	}
}