namespace TrizDB
{
	public enum Names
	{
		// Свойства Объекта Материального Имущества (ОМИ)
		QRCode = 1,
		InventoryNumber = 2,
		AssetsName = 3,
		Photo = 4,
		ResponsibleAssets = 5,      // Ссылка на Set_id сущности МОЛ из этого же списка
		AssetLocationID = 6,        // Ссылка на Set_id сущности Местоположения из этого же списка
		AssetUsers = 7,

		// Свойства Материально Ответственного Лица (МОЛ)
		LastName = 8,
		FirstName = 9,
		MiddleName = 10,
		ResponsibleAssetsPosition = 11, // Главный маркер-определитель пачки МОЛ

		// Свойства Сущности Местоположения
		AssetLocationText = 12,
		LocationDateTime = 13
	}
}