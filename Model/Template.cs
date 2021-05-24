using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcesator.Model
{
		public class Template
		{
				public int id;
				public string prefix;
				public string name;
				public string description;
				public string storedProcedure;
				public string separator;
				public DateTime regisrationDate;
				public List<TemplateField> lsTemplateField;
		}
}
