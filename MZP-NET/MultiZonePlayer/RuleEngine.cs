using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiZonePlayer
{
	internal static class ExpressionEvaluator
	{
		#region static members
		private static object _evaluator = GetEvaluator();
		private static Type _evaluatorType;
		private const string _evaluatorSourceCode =
			@"package Evaluator
            {
               class Evaluator
               {
                  public function Eval(expr : String) : String 
                  { 
					 var result;
					 eval(expr);
                     return result; 
                  }
				
				public function EvalBool(expr : String) : String 
                  { 
					 var result;
					 eval('result ='  + expr);
                     return result; 
                  }
               }
            }";
		
		#endregion

		#region static methods
		private static object GetEvaluator()
		{
			CompilerParameters parameters;
			parameters = new CompilerParameters();
			parameters.GenerateInMemory = true;

			Microsoft.JScript.JScriptCodeProvider jp = new Microsoft.JScript.JScriptCodeProvider();
			CompilerResults results = jp.CompileAssemblyFromSource(parameters, _evaluatorSourceCode);

			Assembly assembly = results.CompiledAssembly;
			_evaluatorType = assembly.GetType("Evaluator.Evaluator");

			return Activator.CreateInstance(_evaluatorType);
		}

		/// <summary>
		/// Executes the passed JScript Statement and returns the string representation of the result
		/// </summary>
		/// <param name="statement">A JScript statement to execute</param>
		/// <returns>The string representation of the result of evaluating the passed statement</returns>
		public static string EvaluateToString(string statement)
		{
			object o = EvaluateToObject(statement, "Eval");
			return o.ToString();
		}
		public static string EvaluateBoolToString(string statement)
		{
			object o = EvaluateToObject(statement, "EvalBool");
			return o.ToString();
		}

		public static string FunctionToString(string statement)
		{
			object o = EvaluateToObject(statement, "Func");
			return o.ToString();
		}
		/// <summary>
		/// Executes the passed JScript Statement and returns the result
		/// </summary>
		/// <param name="statement">A JScript statement to execute</param>
		/// <returns>The result of evaluating the passed statement</returns>
		public static object EvaluateToObject(string statement, string operation)
		{
			lock (_evaluator)
			{
				try
				{
					return _evaluatorType.InvokeMember(
								operation,
								BindingFlags.InvokeMethod,
								null,
								_evaluator,
								new object[] { statement },
								CultureInfo.CurrentCulture
							 );
				}
				catch (Exception ex)
				{
					MLog.Log(ex, "Error evaluate JS to object");
					return "<ERROR JS>";
				}
			}
		}
		#endregion
	}
	public static class Reflect
	{
		public static String GenericReflect_old(/*Object instance, */ref String result)
		{
			object instance;
			String[] recurrent, parameters, methods;
			object[] methodparams;
			String property, methodparam, method;
			object value, objinfo;
			
			MethodInfo methInfo;
			String[] varatoms;
			
			varatoms = result.Split(new String[] {"#"}, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				foreach (String atomPart in varatoms)
				{
					value = null;
					instance = ReflectionInterface.Instance;
					recurrent = atomPart.Split(new String[] {"."}, StringSplitOptions.RemoveEmptyEntries);
					string atom;
					if (recurrent.Length > 1)
					{
						atom = recurrent[0] + "." + recurrent[1];
						if (atom.StartsWith("DVR"))
						{
							atom += "";
						}
						for (int recIndex = 1; recIndex < recurrent.Length; recIndex++)
						{
							if (value != null)
							{
								instance = value;
								atom = recurrent[recIndex];
							}
							value = GetPropertyField(instance, atom);
							if (value != null)
								result = result.Replace("#" + atomPart + "#", value.ToString());
							else
							{
								methodparam = atom.Replace("~", "");
								parameters = methodparam.Split(',');
								method = parameters[0].ToString();

								methods = method.Split('.');
								if (methods.Length > 1) //have fields
								{
									method = methods[0];
									property = methods[1];
								}
								else
									property = null;

								methodparams = new object[parameters.Length]; // + 1];
								//methodparams[0] = context;
								parameters.CopyTo(methodparams, 0); //1);
								methInfo = Type.GetType(instance.GetType().FullName).GetMethod(method);

								if (methInfo != null)
								{
									try
									{
										if (methodparams.Length > methInfo.GetParameters().Length)
										{
											methodparams = new object[parameters.Length - 1];
											for (int i = 0; i < parameters.Length - 1; i++)
												methodparams[i] = parameters[i];
											MLog.Log(null, "Adjusting method param count");
										}
										value = methInfo.Invoke(instance, methodparams);
									}
									catch (Exception ex)
									{
										//result = ex.Message;
										value = ex.Message;
										result = result.Replace("#" + atom + "#", value.ToString());
										break;
									}

									if (value != null)
									{
										//nested properties, check for the type and read value
										if (property != null)
										{
											objinfo = Type.GetType(value.GetType().FullName).GetField(property);

											if (objinfo == null)
											{
												objinfo = Type.GetType(value.GetType().FullName).GetProperty(property);
												if (objinfo == null)
												{
													objinfo = Type.GetType(value.GetType().FullName).GetMethod(property);
													if (objinfo == null)
														MLog.Log(instance, "Unknown call for atom=" + property);
													else
													{
														MethodInfo meth = (MethodInfo) objinfo;
														if (meth.GetParameters().Length > 0)
														{
															methodparams = new object[meth.GetParameters().Length];
															for (int i = 0; i < meth.GetParameters().Length; i++)
															{
																//String t = meth.GetParameters()[i].GetType().ToString();
																if (meth.GetParameters()[i].ParameterType == typeof (Int32))
																{
																	methodparams[i] = Convert.ToInt32(parameters[i + 2]);
																}
																else
																	methodparams[i] = parameters[i + 2];
															}

															value = meth.Invoke(value, methodparams);
														}
														else
															value = ((PropertyInfo) objinfo).GetValue(value, null);
													}
												}
												else
													value = ((PropertyInfo) objinfo).GetValue(value, null);
											}
											else
												value = ((FieldInfo) objinfo).GetValue(value);
										}
										if (value != null)
											result = result.Replace("#" + atom + "#", value.ToString());
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error reflection");
			}

			return result;
		}

		private static String Clean(string propName)
		{
			return propName.Substring(0, Math.Min(50, propName.Length)).Replace('\n', ' ').Replace('\r', ' ');
		}
		public static void GenericReflect(ref String result)
		{
			object instance = ReflectionInterface.Instance;
			String[] lineAtoms, complexAtoms;
			object value;
			String complexLine, complexAtom;
			int complexLen;
			if (!result.Contains("#"))
				return;

			lineAtoms = result.Split(new String[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (String line in lineAtoms)
			{
				complexLine = line;
				complexLen = line.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length;
				for (int i = 0; i < complexLen-1; i++)
				{
					complexAtoms = complexLine.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
					complexAtom = complexAtoms[complexAtoms.Length-1];
					value = ReflectLine(instance, complexAtom);
					if (value != null)
					{
						complexLine = complexLine.Replace(":" + complexAtom, "."+value.ToString());
					}
				}
				
				value = ReflectLine(instance, complexLine);
				if (value == null)
				{
					//don't do anything as expressions with variables should not be altered
				}
				else
					if (value.GetType() != typeof(Exception))
						result = result.Replace("#" + line + "#", value.ToString());
			}
		}


		private static Object ReflectLine(Object instance, String atom)
		{
			object value;
			String[] methodAtoms, paramAtoms;
			object[] parameters;
			string runMethod;
			methodAtoms = atom.Split(new String[] { "." }, StringSplitOptions.RemoveEmptyEntries);
			value = instance;
			foreach (String method in methodAtoms)
			{
				paramAtoms = method.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
				if (paramAtoms.Length > 1)
				{//first param will be method name
					parameters = new object[paramAtoms.Length - 1];
					for (int i = 0; i < parameters.Length; i++)
						parameters[i] = paramAtoms[i + 1];
					runMethod = paramAtoms[0];
				}
				else
				{//only method name exist
					parameters = null;
					runMethod = method;
				}
				value = GetPropertyField(value, runMethod, parameters);
				if (value == null)
					break;
			}
			//if (value != null)
				return value;//.ToString();
			//else
			//	return null;
		}

		public static Object GetPropertyField(object instance, string propName, params object[] parameters)
		{
			PropertyInfo propInfo;
			FieldInfo fieldInfo;
			MethodInfo methInfo;
			object value;
			Boolean varFound = false;

			if (instance == null)
				return null;

			try
			{
				propInfo = Type.GetType(instance.GetType().FullName).GetProperty(propName);
				if (propInfo != null || propName=="?")
				{
					if (propInfo != null)
						value = propInfo.GetValue(instance, null);
					else
						value = instance;

					if (parameters != null && parameters.Length > 0)
					{
						//MLog.Log(null, "Warning, Property called with parameters");
						if (value.GetType().Name == "List`1")
						{
							if (value.ToString().Contains(typeof(MultiZonePlayer.ZoneDetails).ToString()))
								value = ((List<ZoneDetails>)value)[Convert.ToInt32(parameters[0])];
							else
								if (value.ToString().Contains(typeof(System.String).ToString()))
									value = ((List<String>)value)[Convert.ToInt32(parameters[0])];
								else
									if (value.ToString().Contains(typeof(MultiZonePlayer.Alert).ToString()))
										value = ((List<Alert>)value)[Convert.ToInt32(parameters[0])];
									else
									//"System.Collections.Generic.List`1[System.String]"
									{
										MLog.Log(null, "Unknown secondary type for property index " + Clean(propName) + " type=" + value.ToString());
										value = new Exception();
									}
						}
						else
						{
							MLog.Log(null, "Unknown main type for property index " + Clean(propName) + " type=" + value.ToString());
							value = new Exception();
						}
					}
					
				}
				else
				{
					fieldInfo = instance.GetType().GetField(propName);
					if (fieldInfo != null)
					{
						if (parameters != null && parameters.Length > 0)
							MLog.Log(null, "Warning, Field called with parameters");
						value = fieldInfo.GetValue(instance);
					}
					else
					{
						methInfo = instance.GetType().GetMethod(propName);
						if (methInfo != null)
						{
							int parsLen = parameters != null ? parameters.Length : 0;

							if (methInfo.GetParameters().Length == parsLen)
							{
								for (int p = 0; p < parsLen; p++)//setting param types
								{
									try
									{
										Type paramType = methInfo.GetParameters()[p].ParameterType;
										if (parameters[p].ToString().StartsWith("%"))
											varFound = true;
										if (paramType.IsEnum)
										{
											parameters[p] = Enum.Parse(paramType.UnderlyingSystemType, parameters[p].ToString());
										}
										else
											parameters[p] = Convert.ChangeType(parameters[p], paramType);
									}
									catch (Exception e)
									{
										MLog.Log(null, "Unable to cast prop=" + Clean(propName) + " param="+parameters[p] + " err="+e.Message);
										value = new Exception();
										break;
									}
								}
								try
								{
									value = methInfo.Invoke(instance, parameters);
								}
								catch (Exception ex)
								{
									MLog.Log(ex, "Err invoking method " + Clean(propName));
									value = new Exception();
								}
								if (value == null && !varFound)
								{
									MLog.Log(null, "Warning, null result returned on prop=" + Clean(propName));
								}
							}
							else
							{
								value = new Exception();
								MLog.Log(null, "wrong numbers of method params, meth=" + Clean(propName)
									+ " expected=" + methInfo.GetParameters().Length + " given=" + parsLen);
							}
						}
						else
						{
							//??
							value = new Exception(); 
							//MLog.Log(null, "unknown prop type for prop=" + Clean(propName));
						}
					}
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error reflecting on prop="+Clean(propName));
				value = new Exception();
			}

			return value;
		}

		private static string GetMethodNoFields(object instance, string methodName)
		{
			string value = null, method;
			String[] parameters;
			MethodInfo methInfo;

			parameters = methodName.Split(',');
			method = parameters[0].ToString();

			methInfo = Type.GetType(instance.GetType().FullName).GetMethod(method);

			if (methInfo != null)
			{
					//value = methInfo.Invoke(instance, methodparams);
					//objinfo = Type.GetType(value.GetType().FullName).GetField(property);
					//value = ((FieldInfo)objinfo).GetValue(value);
			}
								
			return value;
		}

		
	}

	public static class Rules
	{
		private static List<RuleEntry> m_ruleList;
		public class RuleEntry
		{
			public string Name;
			public string Trigger;
			public string FilterFieldName = null;
			public string FilterFieldValue = null;
			public List<String> VariableList = null;
			public string JSCode;
		}

		public static void LoadFromIni()
		{
			m_ruleList = new List<RuleEntry>();
			string fileContent = Utilities.ReadFile(IniFile.CurrentPath() + IniFile.RULES_FILE);
			string[] rules = fileContent.Split(new String[] { "};" }, StringSplitOptions.RemoveEmptyEntries);
			string[] atoms;
			RuleEntry entry;
			foreach (string rule in rules)
			{
				try
				{
					entry = new RuleEntry();

					atoms = rule.Split(new String[] { "={" }, StringSplitOptions.RemoveEmptyEntries);
					entry.Name = atoms[0].Trim().Replace("\r\n", "").Replace("\t", "");//.ToLower();

					atoms = atoms[1].Split('|');
					entry.Trigger = atoms[0].Trim().Replace("\r\n", "").Replace("\t", "");
					string[] vars = entry.Trigger.Split(';');
					if (vars.Length > 1)
					{
						entry.Trigger = vars[0];
						string[] fields = vars[1].Split('=');
						if (fields.Length > 1)
						{
							entry.FilterFieldName = fields[0];
							entry.FilterFieldValue = fields[1];
						}
					}
					entry.JSCode = atoms[1];
					//find variables in js code
					MatchCollection matchList;
					matchList = Regex.Matches(entry.JSCode, @"\[(.*?)\]");//Not clear what?
					if (matchList.Count > 0) entry.VariableList = new List<string>();
					foreach (Match m in matchList)
					{
						entry.VariableList.Add(m.Groups[1].Value);
					}
					m_ruleList.Add(entry);
				}
				catch (Exception ex)
				{
					MLog.Log(ex, "Error, rule was not loaded, rule=" + rule);
				}
			}
			MLog.Log(null, "Loaded " + m_ruleList.Count + " rules");
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		public static void ExecuteRule(object callingInstance, params String[] values)
		{
			if (m_ruleList == null) LoadFromIni();
			//string parameters = triggerField.Length>0?triggerField[0].ToString():"";
			string triggerName;
			var currentMethod = System.Reflection.MethodInfo.GetCurrentMethod();
			var callingMethod = new System.Diagnostics.StackTrace(1, false).GetFrame(0).GetMethod();

			int i = callingMethod.Name.IndexOf("set_");
			if (i >= 0)
			{
				triggerName = callingMethod.Name.Substring("set_".Length);
				triggerName = callingMethod.DeclaringType.Name + "." + triggerName;
			}
			else
			{
				i = callingMethod.Name.IndexOf("Set");
				if (i >= 0)
				{
					triggerName = callingMethod.Name.Substring("Set".Length);
					triggerName = callingMethod.DeclaringType.Name + "." + triggerName;
				}
				else
				{
					MLog.Log(null, "Error no triggername found calling method=" + callingMethod.Name);
					return;
				}
			}


			List<RuleEntry> ruleList, filteredList;
			ruleList = m_ruleList.FindAll(x => x.Trigger == triggerName && x.FilterFieldName == null);
			filteredList = m_ruleList.FindAll(x => x.Trigger == triggerName && x.FilterFieldName != null).ToList();

			if (filteredList != null)
			{
				object val;
				foreach (RuleEntry r in filteredList)
				{
					val = Reflect.GetPropertyField(callingInstance, r.FilterFieldName);
					if (val != null && r.FilterFieldValue == val.ToString())
						ruleList.Add(r);
				}
			}

			foreach (RuleEntry rule in ruleList)
			{
				string parsedCode = rule.JSCode;
				//replacing variables
				/*
				if (rule.VariableList != null)
				{
					object value;
					foreach (string variable in rule.VariableList)
					{
						value = Reflect.GetPropertyField(callingInstance, variable);
						if (value != null) //TODO check the need
							parsedCode = parsedCode.Replace("[" + variable + "]", value.ToString());
						//else
						//	MLog.Log(null, "No instance variable found for jscode, var=" + variable);
					}
				}*/

				try
				{
					Reflect.GenericReflect(ref  parsedCode);
					String JSResult = ExpressionEvaluator.EvaluateToString(parsedCode);
					String displayValues = "";
					if (values != null)
					{
						foreach (String v in values)
						{
							displayValues += v + ";";
						}
					}
					MLog.Log(null, "Script "+rule.Name+" values="+displayValues+" returned result=["+JSResult+"]");
					string[] pairs = JSResult.Split(';');
					string[] entry;
					ValueList vals = new ValueList();

					foreach (string pair in pairs)
					{
						entry = pair.Split('=');
						if (entry.Length > 1)
							vals.Add(entry[0].ToLower().Trim(), entry[1].ToLower());
						else
							MLog.Log(null, "Missing parameters for JS command");
					}
					MLog.Log(null, "Execute RuleEngine command=" + rule.Name + " trigger=" + triggerName);
					if (vals.Values.Count > 0)
						API.DoCommand(vals);
					else
						MLog.Log(null, "Not executing JS script command due to missing values");

				}
				catch (Exception ex)
				{
					MLog.Log(ex, "Error reflect / JS / execute");
				}
			}
		}
	}

	public class ReflectionInterface
	{
		#region used for HTML data

		private static Random RANDOM = new Random();
		public static System.Net.HttpListenerContext LastContext;
		public static ReflectionInterface Instance = new ReflectionInterface();

		public Alarm SystemAlarm
		{
			get { return MZPState.Instance.SystemAlarm; }
		}

		public String ZonesOpened
		{
			get { return ControlCenter.GetActiveZonesCount().ToString(); }
		}

		public String LastAlarmEvents
		{
			get
			{
				String res = "";
				var zones = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastMovementDate).ToList();

				foreach (ZoneDetails zone in zones)
				{
					res += zone.ZoneName + "-" + zone.ZoneId + ", Alarm=" + zone.LastAlarmMovementDateTime + ", Cam=" + zone.LastCamAlertDateTime + "<BR>";
				}
				return res;
			}
		}

		public String Random
		{
			get { return RANDOM.Next().ToString(); }
		}

		public String ServerTime
		{
			get { return DateTime.Now.ToString(IniFile.DATETIME_DAYHR_FORMAT); }
		}

		public String GetCmdValues(String zoneid, String command, String htmldelimiter, int maxLines)
		{
			ValueList vals = new ValueList(CommandSources.web);
			//Metadata.ValueList resvalue;
			CommandResult resCmd=null;
			String result = "";

			vals.Add(GlobalParams.zoneid, zoneid);
			vals.Add(GlobalParams.command, command);

			resCmd = API.DoCommandFromWeb(vals);//, out resvalue);

			if (resCmd.ValueList != null)
			{
				if (resCmd.ValueList.IndexList != null)
				{
					if (maxLines<0)
						maxLines = resCmd.ValueList.IndexList.Count;
					for (int i = 0; i < Math.Min(resCmd.ValueList.IndexList.Count, maxLines); i++)
					{
						result += "<" + htmldelimiter + " value=" + resCmd.ValueList.IndexList[i] + ">" + resCmd.ValueList.IndexValueList[i] + "</" + htmldelimiter + ">\r\n";
					}
				}
				else
					if (resCmd.ValueList.IndexValueList != null)
					{
						for (int i = 0; i < resCmd.ValueList.IndexValueList.Count; i++)
						{
							result += "<" + htmldelimiter + ">" + resCmd.ValueList.IndexValueList[i] + "</" + htmldelimiter + ">\r\n";
						}
					}
			}

			return result;
		}

		public MZPState S
		{
			get { return MZPState.Instance; }
		}

		public ReflectionInterface R
		{
			get { return this; }
		}

		public ZoneDetails Zones(String zoneIdentifier)//zoneid or zonename
		{
			int zoneId;
			if (Int32.TryParse(zoneIdentifier, out zoneId))
				return MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneId)));
			else
				return MZPState.Instance.ZoneDetails.Find(x => x.ZoneName.Equals(zoneIdentifier));
		}

		public ZoneDetails FirstActiveZone
		{
			get
			{
				List<ZoneDetails> zones;
				ZoneDetails zone;

				zones = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList();
				zone = zones.Find(x => x.IsActive == true && (x.ActivityType.Equals(GlobalCommands.music) || x.ActivityType.Equals(GlobalCommands.streammp3)));
				return zone;
				/*String zonetype = zone!=null?zone.ActivityType.ToString():"status";//return status page if no active zone
				return zonetype;
				 */
			}
		}

		public String DVRServerPort
		{
			get { return Utilities.ExtractServerNameFromURL(LastContext.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_ISPY_PORT[1]; }
		}

		public String WebServerPort
		{
			get
			{
				return Utilities.ExtractServerNameFromURL(LastContext.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_WEBSERVER_PORT_EXT[1];
			}
		}

		public String ZoneMoveStatusAsColor(String zoneid)
		{
			ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
			String color = "Transparent";
			if (zone != null)
			{
				if (zone.HasImmediateMove) color = "Pink";
				else if (zone.HasRecentMove) color = "Yellow";
				else if (zone.HasPastMove) color = "Aqua";
			}
			else
			{
				MLog.Log(this, "unknown zone statusascolor,id=" + zoneid);
			}

			return color;
		}

		public String GetZoneStatusAsColor(String zoneid, String activity)
		{
			ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
			String color = "inherit";
			if ((zone != null) && (zone.ActivityType.ToString().Equals(activity.ToLower())))
			{
				if (zone.IsActive) color = "Red";
			}

			return color;
		}

		public MediaImageItem CurrentPicture
		{
			get {
				if (MediaLibrary.AllPictureFiles != null && MediaLibrary.AllPictureFiles.CurrentIteratePicture != null)
					return MediaLibrary.AllPictureFiles.CurrentIteratePicture;
				else
					return new MediaImageItem("picture not yet available", "picture not yet available");
			}
		}

		#endregion

		public static void GenerateServerSideScript(ref String result)
		{
			//MLog.Log(this, "Generating SS script");
			const string delim_start = "<%", delim_end = "%>";
			String script, target, scriptReflect;
			int cycles = 0;
			do
			{
				script = result.Substring(delim_start, delim_end);
				if (script != null && script != "")
				{
					scriptReflect = script;
					Reflect.GenericReflect(ref scriptReflect);
					string[] atoms = scriptReflect.Split(';');

					target = result.Substring(delim_end, delim_start + delim_end);
					if (atoms[0] == "?" && target == null)
						target = "";
					if (target == null)
					{
						MLog.Log(null, "Could not find script target");
						break;
					}
					int resultInsertIndex = result.IndexOf(target);
					

					switch (atoms[0])
					{
						case "for":
							if (script.Contains("for"))
							{
								string var, start, end, oper, cond;
								var = atoms[1];
								start = atoms[2];
								oper = atoms[3];
								end = atoms[4];
								cond = atoms[5];

								int forstart, forend;
								string filter;
								forstart = Convert.ToInt16(start);
								forend = Convert.ToInt16(end);
								String generated;

								switch (oper)
								{
									case "++":
										String reseval = "";
										for (int j = forstart; j < forend; j++)
										{
											if (cond != "")
											{
												filter = cond.Replace("%" + var, j.ToString());
												Reflect.GenericReflect(ref filter);
												reseval = ExpressionEvaluator.EvaluateBoolToString(filter);
											}
											if (cond == "" || Convert.ToBoolean(reseval))
											{
												generated = target.Replace("%" + var, j.ToString());
												result = result.Insert(resultInsertIndex, generated);
												resultInsertIndex += generated.Length;
											}
										}
										break;
									case "--":
										for (int j = forstart; j < forend; j--)
										{
											generated = target.Replace("%" + var, j.ToString());
											result = result.Insert(resultInsertIndex, generated);
											resultInsertIndex += generated.Length;
										}
										break;
								}
							}
							result = result.Replace(delim_start + script + delim_end, "").Replace(target + delim_start + delim_end, "");
							break;
						case "iif":
							string eval, casetrue, casefalse;
							eval = atoms[1];
							casetrue = atoms[2];
							casefalse = atoms[3];
							if (eval.ToLower() != "true" && eval.ToLower() != "false")
								eval = ExpressionEvaluator.EvaluateBoolToString(eval);
							result = result.Replace(delim_start + script + delim_end, "");
							if (eval.ToLower() == "true")
								result = result.Replace(target, casetrue);
							else
								result = result.Replace(target, casefalse);
							result = result.ReplaceFirst(delim_start + delim_end, "");
							break;
						case "if":
							string res;
							res = atoms[1];
							if (res.ToLower()!="true" && res.ToLower() != "false")
								res = ExpressionEvaluator.EvaluateBoolToString(res);
							result = result.Replace(delim_start + script + delim_end, "");
							if (res.ToLower() == "false")
								result = result.Replace(target, "");
							else
								result = result + "";
							result = result.ReplaceFirst(delim_start + delim_end, "");
							break;
						case "?":
							string exp = atoms[1], expresult;
							//Reflect.GenericReflect(ref exp);
							expresult = ExpressionEvaluator.EvaluateBoolToString(exp);
							result = result.Replace(delim_start + script + delim_end, expresult);// + target + delim_start + delim_end, expresult);
							break;
						default:
							break;
					}
				}
				cycles++;
			}
			while (script != null && script != "" && cycles <= 100);
			if (cycles > 100)
				MLog.Log(null, "Error, infinite cycles reached");
		}
	}

}
