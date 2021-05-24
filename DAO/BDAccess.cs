using BatchProcesator.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcesator.DAO
{
		public class DBAccess
		{
				private static DBAccess instance = null;
				private string connectionString { get; set; }
				private SqlConnection connectionBD { get; set; }
				private string sLine;				

				private DBAccess(string _ConnectionString) {
						this.connectionString = _ConnectionString;
						this.connectionBD = new SqlConnection(this.connectionString);
						this.sLine = "------------------------------------------------------------";
				}

				public static DBAccess GetInstance(string _ConnectionString) {
						if (instance == null)
						{
								instance = new DBAccess(_ConnectionString);
						}
						return instance;
				}

				public void OpenConnection() {
						try
						{
								this.connectionBD.Open();
								//Console.WriteLine(this.sLine);
								//Console.WriteLine("Conexión establecida con la base de datos");
								//Console.WriteLine(this.sLine);
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public void CloseConnection() {
						try
						{
								this.connectionBD.Close();								
								//Console.WriteLine(this.sLine);
								//Console.WriteLine("Se ha cerrado la conexión a la BD");
								//Console.WriteLine(this.sLine);
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public void ExecuteQuery(string _Command) {
						try
						{
								SqlCommand command = new SqlCommand(_Command, this.connectionBD);
								SqlDataReader dbReader = command.ExecuteReader();

								while (dbReader.Read()) {
										ReadSingleRow((IDataRecord)dbReader);
								}
							
								dbReader.Close();
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public DataSet ExecuteStoredProcedure(string _ProcedureName,  out ResponseExecution _ResponseExecution, List<SqlParameter> _LsParametes = null) {
						try
						{
								SqlCommand command = new SqlCommand(_ProcedureName, this.connectionBD) { CommandType = CommandType.StoredProcedure };
								if (_LsParametes != null) {
										foreach (var param in _LsParametes) {
												command.Parameters.Add(param);
										}
								}
								SqlDataAdapter adapterDB = new SqlDataAdapter();
								DataSet dtsResult = new DataSet();
								adapterDB.SelectCommand = command;
								adapterDB.Fill(dtsResult);
								_ResponseExecution = new ResponseExecution();
								_ResponseExecution.executionHasError = false;
								return dtsResult;								
						}
						catch (Exception ex)
						{								
								_ResponseExecution = new ResponseExecution();
								_ResponseExecution.executionHasError = true;
								_ResponseExecution.message = ex.Message;
								return null;
						}
				}

				private void ReadSingleRow(IDataRecord record) {
						Console.WriteLine(String.Format("{0},{1}", record[0], record[1]));				
				}

		}
}
