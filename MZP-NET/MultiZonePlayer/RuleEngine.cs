using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
		public static String GenericReflect(Object instance, ref String result)
		{
			String[] parameters, methods;
			object[] methodparams;
			String property, methodparam, method;
			object value, objinfo;
			PropertyInfo propInfo;
			MethodInfo methInfo;
			String[] varatoms;

			varatoms = result.Split(new String[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
			try
			{
				foreach (String atom in varatoms)
				{
					switch (atom[0])
					{
						case '!': //property read
							property = atom.Replace("!", "");
							propInfo = Type.GetType(instance.GetType().FullName).GetProperty(property);
							if (propInfo != null)
							{
								value = propInfo.GetValue(instance, null);
								result = result.Replace("#" + atom + "#", value.ToString());
							}
							break;
						case '~'://method invoke
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
									value = methInfo.Invoke(instance, methodparams);
								}
								catch (Exception ex)
								{
									result = ex.Message;
									value = null;
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
							break;
						case '*'://multiple values

							break;
					}
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "Error reflection");
			}

			return result;
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

		public Metadata.ZoneDetails Zones(String methodname, String zoneid)
		{
			return MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
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
				if (zone.HasImmediateMove) color = "Orange";
				else if (zone.HasRecentMove) color = "Pink";
				else if (zone.HasPastMove) color = "Yellow";
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
