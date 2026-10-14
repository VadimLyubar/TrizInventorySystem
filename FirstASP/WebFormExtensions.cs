using System;

namespace System.Web.UI
{
	/// <summary>
	/// Универсальные методы расширения для безопасного обмена данными между Web-формами.
	/// Совместимо с .NET 4.0 / C# 4.0.
	/// </summary>
	public static class WebFormExtensions
	{
		/// <summary>Вызов целевой Web-формы с автоматической генерацией ключей возврата на основе типов.</summary>
		public static void CallWebForm(this Page Source, Type SourceWebForm, Type TargetWebForm)
		{
			Source.Response.Redirect(string.Format("{0}?return={1}&key={2}",
				string.Concat(TargetWebForm.Name, ".aspx"),
				string.Concat(SourceWebForm.Name, ".aspx"),
				string.Concat(SourceWebForm.Name, '_', TargetWebForm.Name)));
		}

		/// <summary>Возвращает значение, переданное вызванной Web-формой, и очищает сессию.</summary>
		public static object ResultWebForm(this Page Source, Type SourceWebForm, Type ReturnedWebForm)
		{
			object Result = null;
			string Key = string.Concat(SourceWebForm.Name, '_', ReturnedWebForm.Name);
			if (Source.Session[Key] != null)
			{
				Result = Source.Session[Key];
				Source.Session[Key] = null;
			}
			return Result;
		}

		/// <summary>Записывает значение в сессию и универсально возвращает на вызывающую форму.</summary>
		public static void ReturnWebForm(this Page Source, object Value)
		{
			string ReturnURL = Source.Request.QueryString["return"];
			string Key = Source.Request.QueryString["key"];
			if (!string.IsNullOrEmpty(Key)) Source.Session[Key] = Value;
			if (string.IsNullOrEmpty(ReturnURL)) ReturnURL = "Default.aspx";
			Source.Response.Redirect(ReturnURL);
		}
	}
}