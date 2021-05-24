using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcesator.Model
{
		public class ProgramConfig
		{
				public string pathOrigin { get; set; }
				public string pathDestination { get; set; }
				public string pathError { get; set; }
				public string nameFileError { get; set; }
				private string connectionString { get; set; }

				public ProgramConfig(string _PathOrigin, string _PathDestination, string _PathError, string _NameFileError, string _ConnectionString) {
						this.pathOrigin = _PathOrigin;
						this.pathDestination = _PathDestination;
						this.pathError = _PathError;
						this.nameFileError = _NameFileError;
						this.connectionString = _ConnectionString;
				}

				public string GetConnectionString() {
						return this.connectionString;
				}

		}
}
