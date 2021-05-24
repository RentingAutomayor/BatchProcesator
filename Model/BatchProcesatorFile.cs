using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcesator.Model
{
		public class BatchProcesatorFile
		{
				public string name;
				private  string headers;
				private  List<string> content;

				public void LoadFile(string _Name,List<string> _LsData) {
						try
						{
								this.name = _Name;
								this.headers = _LsData.First().ToString();
								_LsData.RemoveAt(0);
								this.content = _LsData;
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public void ShowDataInFile() {
						try
						{
								Console.WriteLine("Headers: {0}", this.headers);
								Console.WriteLine("Contenido ...");
								foreach (var oLine in this.content) {
										Console.WriteLine(oLine);
								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public List<string> GetContent() {
						return this.content;
				}

				public string Getheaders() {
						return this.headers;
				}
		}
}
