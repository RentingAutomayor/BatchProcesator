using BatchProcesator.DAO;
using BatchProcesator.Enum;
using BatchProcesator.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BatchProcesator.Controller
{
		public class BatchProcesator
		{
				private DBAccess DatabaseAccess;
				private List<string> lsFiles;
				private string sLine;
				private BatchProcesatorFile bpFile;
				private ProgramConfig programConfig;
				public BatchProcesator(ProgramConfig _ProgramConfig)
				{
						this.programConfig = _ProgramConfig;
						this.DatabaseAccess = DBAccess.GetInstance(_ProgramConfig.GetConnectionString());
						this.lsFiles = this.GetFilesFromDirectory(_ProgramConfig.pathOrigin);
						this.sLine = "--------------------------------------------------------";

				}


				public List<string> GetFilesFromDirectory(string _Path)
				{
						try
						{
								List<string> lsFiles = new List<string>();
								if (Directory.Exists(_Path))
								{
										lsFiles = Directory.GetFiles(_Path).ToList();
								}
								else
								{
										throw new Exception("El directorio configurado no se encuentra disponible o no es accesible");
								}

								return lsFiles;

						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				public void InitProcess()
				{
						try
						{
								Console.WriteLine(this.sLine);
								Console.WriteLine("Inicia procesamiento batch");
								Console.WriteLine(this.sLine);

								foreach (var oFile in this.lsFiles)
								{
										var sFileName = GetFileName(oFile);
										Model.Template oTemplateByFile = GetTemplateConfiguration(sFileName);

										if (oTemplateByFile != null)
										{
												Console.WriteLine("Inicia Validación del archivo");
												List<string> lsDataTmp = LoadFileInMemory(oFile);
												this.bpFile = new BatchProcesatorFile();
												this.bpFile.LoadFile(sFileName, lsDataTmp);
												this.bpFile.ShowDataInFile();

												ExecuteTemplate(oTemplateByFile, this.bpFile);

										}
										else
										{
												Console.WriteLine("Se debe enviar archivo a error");
										}
										Console.WriteLine(this.sLine);

								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}
				public string GetFileName(string _Path)
				{
						return Path.GetFileName(_Path);
				}

				private Model.Template GetTemplateConfiguration(string _FileName)
				{
						try
						{
								Model.Template oTemplateByFile = new Model.Template();
								oTemplateByFile = GetTemplateForFile(_FileName);
								oTemplateByFile.lsTemplateField = new List<Model.TemplateField>();


								if (oTemplateByFile.id != 0)
								{
										//Description of file to process
										Console.WriteLine("Archivo a procesar: {0}\nPrefijo de plantilla: {1}\nNombre de la plantilla: {2}\nDescripción de la plantilla: {3}\nSeparador: {4}\nStored Procedure: {5} ",
																		_FileName,
																		oTemplateByFile.prefix,
																		oTemplateByFile.name,
																		oTemplateByFile.description,
																		oTemplateByFile.separator,
																		oTemplateByFile.storedProcedure);

										oTemplateByFile.lsTemplateField = GetFieldsByTemplate(oTemplateByFile.id);
										//Example to estructure
										string structureExample = "";

										//TODO: Order by the fields by their position
										foreach (var oField in oTemplateByFile.lsTemplateField)
										{
												structureExample += String.Format("{0}{1}", oField.name, oTemplateByFile.separator);
										}
										Console.WriteLine(this.sLine);
										Console.WriteLine("Estructura de ejemplo: ");
										Console.WriteLine(structureExample);
										Console.WriteLine(this.sLine);
										return oTemplateByFile;

								}
								else
								{
										//TODO: Send file to Error directory
										Console.WriteLine("El archivo {0} no cuenta con una plantilla configurada.", _FileName);
										return null;
								}


						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				private Model.Template GetTemplateForFile(string _FileName)
				{
						try
						{
								string sql = "STRPRC_GetConfigTemplateByPrefix";
								List<SqlParameter> lsParam = new List<SqlParameter>();
								Model.Template templateByFile = new Model.Template();
								//TODO: Parametrize SP to extract config of tempate
								var sPrefix = _FileName.Substring(0, 3).ToUpper();
								lsParam.Add(ConfigureParamToExecute("prefix", "VARCHAR", sPrefix));
								DatabaseAccess.OpenConnection();
								ResponseExecution _ResponseExecution = new ResponseExecution();
								DataSet dtResult = DatabaseAccess.ExecuteStoredProcedure(sql, out _ResponseExecution, lsParam);
								DatabaseAccess.CloseConnection();

								foreach (DataRow rowTemplate in dtResult.Tables[0].Rows)
								{
										templateByFile.id = int.Parse(rowTemplate.ItemArray[(int)Enum.Template.ID].ToString());
										templateByFile.prefix = rowTemplate.ItemArray[(int)Enum.Template.PREFIX].ToString();
										templateByFile.name = rowTemplate.ItemArray[(int)Enum.Template.NAME].ToString();
										templateByFile.description = rowTemplate.ItemArray[(int)Enum.Template.DESCRIPTION].ToString();
										templateByFile.storedProcedure = rowTemplate.ItemArray[(int)Enum.Template.STORED_PROCEDURE].ToString();
										templateByFile.separator = rowTemplate.ItemArray[(int)Enum.Template.SEPARATOR].ToString();
										templateByFile.regisrationDate = DateTime.Parse(rowTemplate.ItemArray[(int)Enum.Template.REGISTRATION_DATE].ToString());
								}

								return templateByFile;
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				private List<Model.TemplateField> GetFieldsByTemplate(int _IDTemplate)
				{
						try
						{
								string sql = "STRPRC_GetFielsdByTemplate";
								List<SqlParameter> lsParam = new List<SqlParameter>();
								List<Model.TemplateField> lsTemplateField = new List<Model.TemplateField>();
								lsParam.Add(ConfigureParamToExecute("ID_TEMPLATE", "INT", _IDTemplate.ToString()));

								this.DatabaseAccess.OpenConnection();
								ResponseExecution _ResponseExecution = new ResponseExecution();
								DataSet dtsFieldsByTemplate = this.DatabaseAccess.ExecuteStoredProcedure(sql, out _ResponseExecution, lsParam);
								this.DatabaseAccess.CloseConnection();

								foreach (DataRow dtRowField in dtsFieldsByTemplate.Tables[0].Rows)
								{
										Model.TemplateField oField = new Model.TemplateField();
										oField.id = int.Parse(dtRowField.ItemArray[(int)Enum.TemplateField.ID].ToString());
										oField.name = dtRowField.ItemArray[(int)Enum.TemplateField.NAME].ToString();
										oField.description = dtRowField.ItemArray[(int)Enum.TemplateField.DESCRIPTION].ToString();
										oField.position = int.Parse(dtRowField.ItemArray[(int)Enum.TemplateField.POSITION].ToString());
										oField.parameterName = dtRowField.ItemArray[(int)Enum.TemplateField.PARAMETER_NAME].ToString();
										oField.parameterType = dtRowField.ItemArray[(int)Enum.TemplateField.PARAMETER_TYPE].ToString();
										oField.registrationDate = DateTime.Parse(dtRowField.ItemArray[(int)Enum.TemplateField.REGISTRATION_DATE].ToString());
										lsTemplateField.Add(oField);
								}
								return lsTemplateField;
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}


				private SqlParameter ConfigureParamToExecute(string paramName, string type, string value)
				{
						try
						{
								SqlParameter param = new SqlParameter();
								string parameter = String.Format("@{0}", paramName);
								param.ParameterName = parameter;

								switch (type)
								{
										case "VARCHAR":
												param.SqlDbType = SqlDbType.VarChar;
												if (value != "")
												{
														param.Value = value;
												}
												else
												{
														param.Value = "";
												}
												break;
										case "INT":
												param.SqlDbType = SqlDbType.Int;
												if (value != "")
												{
														int valParsedInt = int.Parse(value);
														if (valParsedInt > 0)
														{
																param.Value = valParsedInt;
														}
														else
														{
																param.Value = 0;
														}
												}
												else
												{
														param.Value = 0;
												}
												break;
										case "BIGINT":
												param.SqlDbType = SqlDbType.BigInt;
												if (value != "")
												{
														long valParsedLong = long.Parse(value);

														if (valParsedLong > 0)
														{
																param.Value = valParsedLong;
														}
														else
														{
																param.Value = 0;
														}
												}
												else
												{
														param.Value = 0;
												}
												break;
										case "DATETIME":
												param.SqlDbType = SqlDbType.DateTime2;
												if (value == "")
												{
														param.Value = "0001-01-01 00:00:00.0000000";
												}
												else
												{
														param.Value = DateTime.Parse(value);
												}
												break;
										case "BOOLEAN":
												param.SqlDbType = SqlDbType.Bit;
												param.Value = bool.Parse(value);
												break;
										default:
												break;
								}

								return param;
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}


				private List<string> LoadFileInMemory(string _Path)
				{
						try
						{
								using (StreamReader stream = new StreamReader(_Path, System.Text.Encoding.UTF8, true))
								{
										List<string> oLsData = new List<string>();

										while (!stream.EndOfStream)
										{
												oLsData.Add(stream.ReadLine());
										}
										return oLsData;
								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

				private void ExecuteTemplate(Model.Template _Template, BatchProcesatorFile _File)
				{
						try
						{
								int line = 1;
								this.DatabaseAccess.OpenConnection();
								List<SqlParameter> oLsParameterToSend = new List<SqlParameter>();								
								List<string> oLsError = new List<string>();

								foreach (var lineOfFile in _File.GetContent())
								{
										line++;
										ResponseExecution _ResponseExecution = new ResponseExecution();
										oLsParameterToSend = PrepareParametersToSendDB(_Template.lsTemplateField, lineOfFile, _Template.separator);
										DataSet dtRta = this.DatabaseAccess.ExecuteStoredProcedure(_Template.storedProcedure, out _ResponseExecution, oLsParameterToSend);
										if (_ResponseExecution.executionHasError)
										{
												Console.WriteLine("Error en la linea: {0} , {1}\nError: {2}", line, lineOfFile, _ResponseExecution.message);
												var sLineErrorTemp = lineOfFile + _Template.separator + line + _Template.separator +  _ResponseExecution.message;
												oLsError.Add(sLineErrorTemp);
										}
										else
										{
												Console.WriteLine("linea archivo: {0}", line);											
										}
										Console.WriteLine("****");

								}

								if (oLsError.Count() > 0) {
										var sHeadersError = _File.Getheaders() + _Template.separator + "Línea de error" + _Template.separator + "Error";
										var sFileName = "LOG_ERROR_" + _File.name;
										WriteLogError(sHeadersError, oLsError, sFileName);
								}
								this.DatabaseAccess.CloseConnection();
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}

				}

				private List<SqlParameter> PrepareParametersToSendDB(List<Model.TemplateField> _LsTemplateField, string _LineContent, string _Seperator)
				{
						try
						{
								List<SqlParameter> lsParameterBD = new List<SqlParameter>();
								var aData = _LineContent.Split(_Seperator.ToCharArray()[0]);

								foreach (var oParameterField in _LsTemplateField)
								{
										ParameterToSend param = new ParameterToSend();
										param.name = oParameterField.parameterName;
										param.dateType = oParameterField.parameterType;
										param.value = aData[oParameterField.position].ToString();
										lsParameterBD.Add(ConfigureParamToExecute(param.name, param.dateType, param.value));
								}


								return lsParameterBD;
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}


				private void WriteLogError(string _Headers, List<string> _Content, string _FileName)
				{
						try
						{
								var _Path = this.programConfig.pathError + "\\" + _FileName;
								using (StreamWriter oFileError = new StreamWriter(_Path))
								{
										oFileError.WriteLine(_Headers);
										foreach (var sLineError in _Content)
										{
												oFileError.WriteLine(sLineError);
										}


								}
						}
						catch (Exception ex)
						{
								Console.WriteLine(ex.Message);
								throw;
						}
				}

		}
}
