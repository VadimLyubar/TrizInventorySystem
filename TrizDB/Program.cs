using System;
using System.ServiceProcess;

namespace TrizDB
{
	static class Program
	{
		/// <summary>
		/// Главная точка входа для приложения.
		/// </summary>
		static void Main()
		{
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] { new TrizDBService() };
			ServiceBase.Run(ServicesToRun);
		}
	}
}