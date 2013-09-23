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
				return _evaluatorType.InvokeMember(
							operation,
							BindingFlags.InvokeMethod,
							null,
							_evaluator,
							new object[] { statement },
							CultureInfo.CurrentCulture
						 );
			}
		}
		#endregion
	}
	public static class Reflect
	{
		public static String GenericReflect(/*Object instance, */ref String result)
		{
			object instance = ReflectionInterface.Instance;

			String[] parameters, methods;
			object[] methodparams;
			String property, methodparam, method;
			object value, objinfo;
			
			MethodInfo methInfo;
			String[] varatoms;

			varatoms = result.Split(new String[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				foreach (String atom in varatoms)
				{
					value = GetPropertyField(instance, atom);
					if (value != null)
						result = result.Replace("#" + atom + "#", value.ToString());
					else
					{
							
						methodparam = atom.Replace("~", "");
						parameters = methodparam.Split(',');
						method = parameters[0].ToString();

						methods = method.Split('.');
						if (methods.Length > 1)//have fields
						{
							method = methods[0];
							property = methods[1];
						}
						else
							property = null;

						methodparams = new object[parameters.Length];// + 1];
						//methodparams[0] = context;
						parameters.CopyTo(methodparams, 0);//1);
						methInfo = Type.GetType(instance.GetType().FullName).GetMethod(method);

						if (methInfo != null)
						{
							try
							{
								if (methodparams.Length > methInfo.GetParameters().Length)
								{
									methodparams = new object[parameters.Length-1];
									for (int i = 0; i < parameters.Length-1; i++)
										methodparams[i] = parameters[i+1];
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
												value = ((PropertyInfo)objinfo).GetValue(value, null);
											}
										}
										else
											value = ((PropertyInfo)objinfo).GetValue(value, null);
									}
									else
										value = ((FieldInfo)objinfo).GetValue(value);
								}
								if (value != null)
									result = result.Replace("#" + atom + "#", value.ToString());
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

		public static string GetPropertyField(object instance, string propName)
		{
			PropertyInfo propInfo;
			FieldInfo fieldInfo;
			object value;
			string result = null;

			propInfo = Type.GetType(instance.GetType().FullName).GetProperty(propName);
			if (propInfo != null)
				value = propInfo.GetValue(instance, null);
			else
			{
				fieldInfo = instance.GetType().GetField(propName);
				if (fieldInfo != null)
					value = fieldInfo.GetValue(instance);
				else
				{
					//method
					value = null;
				}
			}
			if (value != null)
				result = value.ToString();
			return result;
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
				if (rule.VariableList != null)
				{
					string value;
					foreach (string variable in rule.VariableList)
					{
						value = Reflect.GetPropertyField(callingInstance, variable);
						if (value != null)
							parsedCode = parsedCode.Replace("[" + variable + "]", value);
						else
							MLog.Log(null, "No instance variable found for jscode, var=" + variable);
					}
				}

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
					Metadata.ValueList vals = new Metadata.ValueList();

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

		public Alarm SystemAlarm(String methodname)
		{
			return MZPState.Instance.SystemAlarm;
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

				foreach (Metadata.ZoneDetails zone in zones)
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

		public String GetCmdValues(String methodname, String zoneid, String command, String htmldelimiter)
		{
			Metadata.ValueList vals = new Metadata.ValueList(Metadata.CommandSources.web);
			Metadata.ValueList resvalue;
			String result = "";

			vals.Add(Metadata.GlobalParams.zoneid, zoneid);
			vals.Add(Metadata.GlobalParams.command, command);


			API.DoCommandFromWeb(vals, out resvalue);

			if (resvalue != null)
			{
				if (resvalue.IndexList != null)
				{
					for (int i = 0; i < resvalue.IndexList.Count; i++)
					{
						result += "<" + htmldelimiter + " value=" + resvalue.IndexList[i] + ">" + resvalue.IndexValueList[i] + "</" + htmldelimiter + ">\r\n";
					}
				}
				else
					if (resvalue.IndexValueList != null)
					{
						for (int i = 0; i < resvalue.IndexValueList.Count; i++)
						{
							result += "<" + htmldelimiter + ">" + resvalue.IndexValueList[i] + "</" + htmldelimiter + ">\r\n";
						}
					}
			}

			return result;
		}

		public MZPState S(String methodname)
		{
			return MZPState.Instance;
		}

		public Metadata.ZoneDetails Zones(String methodname, String zoneIdentifier)//zoneid or zonename
		{
			int zoneId;
			if (Int32.TryParse(zoneIdentifier, out zoneId))
				return MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneId)));
			else
				return MZPState.Instance.ZoneDetails.Find(x => x.ZoneName.Equals(zoneIdentifier));
		}

		public Metadata.ZoneDetails FirstActiveZone(String methodname)
		{
			List<Metadata.ZoneDetails> zones;
			Metadata.ZoneDetails zone;

			zones = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList();
			zone = zones.Find(x => x.IsActive == true && (x.ActivityType.Equals(Metadata.GlobalCommands.music) || x.ActivityType.Equals(Metadata.GlobalCommands.streammp3)));
			return zone;
			/*String zonetype = zone!=null?zone.ActivityType.ToString():"status";//return status page if no active zone
			return zonetype;
			 */
		}

		public String DVRServerPort(String methodname)
		{
			return Utilities.ExtractServerNameFromURL(LastContext.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_ISPY_PORT[1];
		}

		public String WebServerPort(String methodname)
		{
			return Utilities.ExtractServerNameFromURL(LastContext.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_WEBSERVER_PORT_EXT[1];
		}

		public String ZoneMoveStatusAsColor(String methodname, String zoneid)
		{
			Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
			String color = "Transparent";
			if (zone != null)
			{
				if (zone.HasImmediateMove) color = "Pink";
				else if (zone.HasRecentMove) color = "Yellow";
				else if (zone.HasPastMove) color = "Cyan";
			}
			else
			{
				MLog.Log(this, "unknown zone statusascolor,id=" + zoneid);
			}

			return color;
		}

		public String GetZoneStatusAsColor(String methodname, String zoneid, String activity)
		{
			Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
			String color = "inherit";
			if ((zone != null) && (zone.ActivityType.ToString().Equals(activity.ToLower())))
			{
				if (zone.IsActive) color = "Red";
			}

			return color;
		}

		public MediaImageItem CurrentPicture(string methodname)
		{
			return MediaLibrary.AllPictureFiles.CurrentIteratePicture;
		}

		#endregion
	}

}
