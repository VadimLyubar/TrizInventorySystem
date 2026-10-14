using System;
using System.Collections.Generic;

namespace TrizDBViewer
{
	public class AssetRow
	{
		public int Set_id { get; set; } // Уникальный абстрактный ключ сущности ОМИ
		public string QrCodeText { get; set; }
		public string InventoryNumber { get; set; }
		public string AssetsName { get; set; }
		public int ResponsiblePersonSetId { get; set; } // ID связанной сущности МОЛ
		public string ResponsiblePersonFio { get; set; } // Читаемое ФИО МОЛ
		public string Users { get; set; }

		// Список объектов истории перемещений с метаданными времени
		public List<LocationHistoryItem> LocationHistory { get; set; }

		public AssetRow()
		{
			LocationHistory = new List<LocationHistoryItem>();
		}
	}

	public class LocationHistoryItem
	{
		public int ValueRecordId { get; set; } // ID записи в ValuesTable для правки текста
		public string LocationText { get; set; }
		public string TimestampText { get; set; }
	}
}