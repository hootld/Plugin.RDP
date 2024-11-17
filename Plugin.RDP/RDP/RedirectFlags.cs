using System;

namespace Plugin.RDP
{
	/// <summary>Флаги переадресации локальных ресурсов</summary>
	[Flags]
	public enum RedirectFlags
	{
		/// <summary>Нет переадресации</summary>
		None = 0x0,
		/// <summary>Переадресовать локальные диски</summary>
		Drives = 0x01,
		/// <summary>Переадресовать порты</summary>
		Ports = 0x02,
		/// <summary>Переадресовать принтеры</summary>
		Printers = 0x04,
		/// <summary>Переадресовать смарт-карты</summary>
		SmartCards = 0x08,
		/// <summary>Переадресовать буфер обмена</summary>
		Clipboard = 0x10,
		/// <summary>Переадресовать устройства Point of Service</summary>
		PointOfService = 0x20,
	}
}