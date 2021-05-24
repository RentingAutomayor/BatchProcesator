using BatchProcesator.Controller;
using BatchProcesator.DAO;
using BatchProcesator.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BatchProcesator
{
		class Program
		{
				static void Main(string[] args)
				{
						InitComponents();						
				}

				static void InitComponents() {
						ProgramConfig programConfig = new ProgramConfig(
								_PathOrigin: ConfigurationManager.AppSettings["PathOrigin"],
								_PathDestination: ConfigurationManager.AppSettings["PathDestination"],
								_PathError: ConfigurationManager.AppSettings["PathError"],
								_NameFileError: ConfigurationManager.AppSettings["NameFileError"],
								_ConnectionString: ConfigurationManager.AppSettings["ConnectionString"]
						);

						ShowInitialConfig(programConfig);
						BatchProcesator.Controller.BatchProcesator oBatchProcesator = new BatchProcesator.Controller.BatchProcesator(programConfig);
						oBatchProcesator.InitProcess();
						Console.ReadLine();
				}

				public static void ShowInitialConfig(ProgramConfig _ProgramConfig) {
						Console.WriteLine("Ruta origen: {0} \nRuta destino: {1}\nRuta Error: {2}",
														_ProgramConfig.pathOrigin,
														_ProgramConfig.pathDestination,
														_ProgramConfig.pathError);						
				}
		}
}
