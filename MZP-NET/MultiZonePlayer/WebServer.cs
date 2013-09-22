﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Web.SessionState;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MultiZonePlayer
{


	class WebServer
	{
		private static HttpListener m_extlistener = new HttpListener();
		private static HttpListener m_extlistenersafe = new HttpListener();
		private static HttpListener m_intlistener = new HttpListener();
		private static WebServer m_instance;

		internal static WebServer Instance
		{
			get { return WebServer.m_instance; }
		}
		private static List<String> CONTENT_TYPES = new List<string>(new String[] {
                "ico,image/ico,true",
                "png,image/png,true",
                "gif,image/gif,true",
                "jpeg,image/jpeg,true",
                "jpg,image/jpeg,true",
                "swf,application/x-shockwave-flash,true",
                "js,application/x-javascript,false",
                "wav,audio/wav",
                "css,text/css,false",
                "html,text/html,false",
                "htm,text/html,false"
            });//format is: extension,contentype,isbinary
		

		public static void Initialise()
		{
			m_instance = new WebServer();
			String extlistener = "https://*:" + IniFile.PARAM_WEBSERVER_PORT_EXT[1] + "/";
			String extlistener_safe = "http://*:" + IniFile.PARAM_WEBSERVER_PORT_EXT_SAFE[1] + "/";
			String intlistener = "http://*:" + IniFile.PARAM_WEBSERVER_PORT_INT[1] + "/";
			Thread th;

			/*MLog.Log(null, "Initialising ext web servers " + extlistener);
			m_extlistener.Prefixes.Add(extlistener);
			m_extlistener.AuthenticationSchemes = AuthenticationSchemes.Basic;
			Thread th = new Thread(() => m_instance.RunMainThread(m_extlistener, extlistener));
			th.Name = "WebListener External";
			th.Start();
			
			MLog.Log(null, "Initialising ext safe web servers " + extlistener_safe);
			m_extlistenersafe.Prefixes.Add(extlistener_safe);
			m_extlistenersafe.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			th = new Thread(() => m_instance.RunMainThread(m_extlistenersafe, extlistener_safe));
			th.Name = "WebListener External Safe";
			th.Start();
			*/
			MLog.Log(null, "Initialising int web servers " + intlistener);
			m_intlistener.Prefixes.Add(intlistener);
			m_intlistener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			th = new Thread(() => m_instance.RunMainThread(m_intlistener, intlistener));
			th.Name = "WebListener Internal";
			th.Start();
		}

		public static void Shutdown()
		{
			if (m_extlistener.IsListening) m_extlistener.Stop();
			if (m_intlistener.IsListening) m_intlistener.Stop();
			if (m_extlistenersafe.IsListening) m_extlistenersafe.Stop();
			m_instance = null;
		}

		~WebServer()
		{
			Shutdown();
		}

		public void RunMainThread(HttpListener listener, String desc)
		{
			try
			{
				listener.Start();
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Error listening " + desc);
			}

			while (listener.IsListening)
			{
				try
				{
					//MLog.Log(null, "Web server waiting for requests");
					HttpListenerContext ctx = listener.GetContext();
					//MLog.LogWeb(listener.GetContext().Request);//, "Web request " + ctx.Request.Url.AbsoluteUri + " from " + ctx.Request.RemoteEndPoint.Address);

					if (listener.AuthenticationSchemes.Equals(AuthenticationSchemes.Basic))
					{
						HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)ctx.User.Identity;
						if (ctx.Request.RemoteEndPoint.Address.ToString().StartsWith("192.168.0.") ||
							identity.Name.Equals(IniFile.PARAM_WEB_USER[1]) && (identity.Password.Equals(IniFile.PARAM_WEB_PASS[1])))
						{
							Thread th = new Thread(() => ProcessRequest(ctx));
							th.Name = "Web-work-thread-" + DateTime.Now.ToString() + ":" + ctx.Request.RawUrl;
							th.Start();
						}
						else
						{
							MLog.Log(this, "invalid user=" + identity.Name + " pass=" + identity.Password);
							Thread.Sleep(3000);//anti brute force attack
						}
					}
					else
					{
						bool safe = true;//false;
						if (ctx.Request.LocalEndPoint.Port.ToString().Equals(IniFile.PARAM_WEBSERVER_PORT_EXT_SAFE[1]))
						{
							foreach (String atom in IniFile.PARAM_ACCEPTED_WEB_SAFE_DEVICES_HEADERS[1].Split('|'))
							{
								if (ctx.Request.Headers.ToString().Contains(atom))
								{
									safe = true;
									break;
								}
							}
						}
						else 
							safe = true;

						if (safe)
						{
							Thread th = new Thread(() => ProcessRequest(ctx));
							th.Name = "Web-work-thread-" + DateTime.Now.ToString() + ":" + ctx.Request.RawUrl;
							th.Start();
						}
						else
						{
							MLog.Log(this, "Unsafe device on ext port " + ctx.Request.Headers);
							Thread.Sleep(3000);//anti brute force attack
						}
					}
				}
				catch (Exception ex)
				{
					MLog.Log(ex, this, "Exception on web server listener " + ex.Message);
					break;
				}
			}
			MLog.Log(this, "Web server listener exit");
		}

		private void ProcessRequest(HttpListenerContext context)
		{
			try
			{
				ReflectionInterface.LastContext = context;
				HttpListenerRequest request = context.Request;
				HttpListenerResponse response = context.Response;
				String cmdResult = "";
				byte[] binaryBuffer = null;
				Metadata.ValueList resvalue = null;
				Metadata.ValueList vals = new Metadata.ValueList(Metadata.CommandSources.web);

				String requestServer;

				//MLog.LogWeb(context.Request);
				requestServer = Utilities.ExtractServerNameFromURL(request.Url.AbsoluteUri);
				String localPath = request.Url.LocalPath.Replace("..", "");//.Replace("/", "");

				//read command from get url
				Metadata.GlobalParams param;
				foreach (string key in request.QueryString.Keys)
				{
					if (key != null && key.Trim().Length != 0)
					{
						if (Enum.IsDefined(typeof(Metadata.GlobalParams), key))
						{
							param = (Metadata.GlobalParams)Enum.Parse(typeof(Metadata.GlobalParams), key);
							vals.Add(param, request.QueryString[key]);
						}
						else
						{
							MLog.Log(this, "Webserver Unknown parameter received:" + key);
						}
					}
				}

				//read command from POST form fields if exists
				if (context.Request.HttpMethod.Equals("POST"))
				{
					if (context.Request.HasEntityBody)
					{
						String post = "";
						System.IO.Stream body = context.Request.InputStream;
						switch (context.Request.Headers["Content-Type"])
						{
							case "audio/x-wav":
								binaryBuffer = new byte[context.Request.ContentLength64];
								using (BinaryReader br = new BinaryReader(context.Request.InputStream))
									br.Read(binaryBuffer, 0, binaryBuffer.Length);
								//Utilities.WriteBinaryFile("\\webroot\\direct\\" + file, binaryBuffer);
								vals.BinaryData = binaryBuffer;
								break;
							case "application/x-www-form-urlencoded":
							default:
								System.Text.Encoding encoding = System.Text.Encoding.UTF8;//request.ContentEncoding;
								System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
								post = reader.ReadToEnd();
								post = System.Uri.UnescapeDataString(post);
								post = post.Replace("%20", " ").Replace("+", " ");
								//post = post.Replace("&", "^");
								body.Close();
								reader.Close();
								break;
						}
						String[] atoms = post.Split(new String[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
						String[] vars;
						String item;
						foreach (String atom in atoms)
						{
							item = atom;//.Replace("^", "&");
							vars = item.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
							if (Enum.IsDefined(typeof(Metadata.GlobalParams), vars[0]))
							{
								param = (Metadata.GlobalParams)Enum.Parse(typeof(Metadata.GlobalParams), vars[0]);
								if (param.Equals(Metadata.GlobalParams.selectedindex))
									vals.AddIndexValue(vars[1].Replace('+', ' '));
								else
								{
									if (vars.Length < 2)
										vals.Add(param, "");
									else
										vals.Add(param, vars[1]);
								}
							}
							else
								MLog.Log(this, "Invalid post param " + vars[0]);
						}
					}
				}

				String cmdSource = vals.GetValue(Metadata.GlobalParams.cmdsource);
				Metadata.CommandSources cmdSourceEnum;
				if (cmdSource == null)
				{
					cmdSourceEnum = vals.CommandSource;
				}
				else
				{
					cmdSourceEnum = (Metadata.CommandSources)Enum.Parse(typeof(Metadata.CommandSources), cmdSource);
				}

				if (localPath.Equals("/cmd"))//command from MZP clients
				{
					if (request.HttpMethod.Equals("GET"))
					{
						cmdResult = API.DoCommandFromWeb(vals, out resvalue);
					}

					if (request.HttpMethod.Equals("POST"))
					{
						if (request.HasEntityBody)
						{
							System.IO.Stream body = request.InputStream;
							System.Text.Encoding encoding = System.Text.Encoding.UTF8;//request.ContentEncoding;
							System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
							String post = reader.ReadToEnd();
							post = System.Uri.UnescapeDataString(post);
							post = post.Replace("%20", " ");
							body.Close();
							reader.Close();
							int firstindex, lastindex;
							firstindex = post.IndexOf('{');
							lastindex = post.LastIndexOf('}');
							String json = post.Substring(firstindex, lastindex - firstindex + 1);

							Metadata.ValueList val = fastJSON.JSON.Instance.ToObject<Metadata.ValueList>(json);
							cmdResult = API.DoCommandFromWeb(val, out resvalue);
						}
						else
						{
							MLog.Log(this, "POST has no entity body, unexpected. " + request.Url.OriginalString);
						}
					}
					String contentType;
					byte[] binaryData = null;
					if (resvalue !=null) binaryData = resvalue.BinaryData;
					if (binaryData != null)
					{
						contentType = resvalue.GetValue(Metadata.GlobalParams.contenttype);
					}
					else
						contentType = "text/html";
					WriteResponse(contentType, cmdResult, response, binaryData);
				}
				else
				{//any other html request
					String contentType, json;
					byte[] binaryData;

					if (vals.ContainsKey(Metadata.GlobalParams.command))
						json = API.DoCommandFromWeb(vals, out resvalue);

					String html = ServeDirectHtml(context, requestServer, resvalue, out contentType, out binaryData);
					WriteResponse(contentType, html, response, binaryData);
				}
				MLog.LogWeb(context.Request);
			}
			catch (Exception ex)
			{
				MLog.Log(ex, this, "Error process request " + context.Request.Url.AbsoluteUri);
			}

			//MLog.Log(null, "Web req done " + context.Request.Url.AbsoluteUri);
		}

		private static void WriteResponse(String contentType, String htmlresponse, HttpListenerResponse httpresponse, byte[] binaryData)
		{
			byte[] buffer;

			httpresponse.ContentType = contentType;
			if (binaryData != null)
			{
				buffer = binaryData;
			}
			else
			{
				buffer = new System.Text.UTF8Encoding().GetBytes(htmlresponse);
			}

			httpresponse.ContentLength64 = buffer.Length;
			using (System.IO.Stream outputStream = httpresponse.OutputStream)
			{
				outputStream.Write(buffer, 0, buffer.Length);
			}
		}

		private String ServeDirectHtml(HttpListenerContext context, String requestServer, Metadata.ValueList resvalue, out String contentType, out byte[] binaryData)
		{
			binaryData = null;
			contentType = "text/html";
			Boolean isBinary = false;

			String pass = context.Request.QueryString["PASS"];
			if (pass == null || !pass.Equals(IniFile.PARAM_WEB_PASS[1]))
			{
				//MLog.Log(null, "wrong web pass user id=" + context.User);
				//return "";
			}

			String result = "", localFile = "";
			String requestURL = context.Request.RawUrl;
			String[] contentAtoms = requestURL.Split('?');//('/');

			localFile = contentAtoms[0];
			if (localFile.Equals("")) localFile = IniFile.PARAM_WEB_DEFAULT_PAGE[1];

			//find content type
			String extension;
			String contenttypeentry;
			String[] contentypeatoms, extensionatoms;

			extensionatoms = localFile.Split('.');
			extension = extensionatoms[extensionatoms.Length - 1];

			contenttypeentry = CONTENT_TYPES.Find(x => x.Contains(extension.ToLower()));
			if (contenttypeentry != null)
			{
				contentypeatoms = contenttypeentry.Split(',');
				contentType = contentypeatoms[1];
				isBinary = contentypeatoms[2].Equals("true");
			}
			else
			{
				MLog.Log(this, "Unknown content type=" + extension + ", set content to non-binary");
				isBinary = false;
			}

			try
			{
				if (isBinary)
					binaryData = Utilities.ReadBinaryFileRelativeToAppPath("\\webroot\\" + localFile);
				else
				{
					result = Utilities.ReadFileRelativeToAppPath("\\webroot\\" + localFile);

					//set client passed params
					foreach (String param in context.Request.QueryString.Keys)
					{
						if ((param != null))
						{
							result = result.Replace("%_" + param, context.Request.QueryString[param]);
						}
					}

					//look for server side programming
					GenerateServerSideScript(ref result);


					//set server variables
					if (resvalue != null)
						result = result.Replace("#HTMLCommandResult#", resvalue.GetValue(Metadata.GlobalParams.msg));

					Reflect.GenericReflect(ref result);
				}
			}
			catch (Exception ex)
			{
				MLog.Log(ex, "err read url" + requestURL);
			}


			if (context.Request.HttpMethod.Equals("GET"))
			{
			}

			if (context.Request.HttpMethod.Equals("POST"))
			{
				if (context.Request.HasEntityBody)
				{
					System.IO.Stream body = context.Request.InputStream;

					switch (context.Request.Headers["Content-Type"])
					{
						/*case "audio/x-wav":
							byte[] buffer = new byte[context.Request.ContentLength64];
							using (BinaryReader br = new BinaryReader(context.Request.InputStream))
								br.Read(buffer, 0, buffer.Length);
							Utilities.WriteBinaryFile("\\webroot\\direct\\"+ file, buffer);
							break;*/
						default:
							System.Text.Encoding encoding = System.Text.Encoding.UTF8;//request.ContentEncoding;
							System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
							String post = reader.ReadToEnd();
							post = System.Uri.UnescapeDataString(post);
							post = post.Replace("%20", " ");
							body.Close();
							reader.Close();
							break;
					}
				}
				else
				{
					MLog.Log(null, "POST/DIRECT has no entity body, unexpected. " + context.Request.Url.OriginalString);
				}
			}
			return result;
		}

		public void GenerateServerSideScript(ref String result)
		{
			const string delim_start = "<%", delim_end = "%>";
			String script, target, scriptReflect;

			do
			{
				script = result.Substring(delim_start, delim_end);
				if (script != "")
				{
					target = result.Substring(delim_end, delim_start + delim_end);
					if (target == "")
					{
						MLog.Log(this, "Could not find script target");
						break;
					}
					int resultInsertIndex = result.IndexOf(target);
					scriptReflect = script;
					Reflect.GenericReflect(ref scriptReflect);
					string[] atoms = scriptReflect.Split(',');

					switch (atoms[0])
					{
						case "for":
							if (script.Contains("for"))
							{
								string var, start, end, oper;
								var = atoms[1];
								start = atoms[2];
								oper = atoms[3];
								end = atoms[4];

								int forstart, forend;
								forstart = Convert.ToInt16(start);
								forend = Convert.ToInt16(end);
								String generated;

								switch (oper)
								{
									case "++":
										for (int j = forstart; j < forend; j++)
										{
											generated = target.Replace("%" + var, j.ToString());
											result = result.Insert(resultInsertIndex, generated);
											resultInsertIndex += generated.Length;
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
						case "if":
							string res;
							res = atoms[1];
							result = result.Replace(delim_start + script + delim_end, "");
							if (res.ToLower() == "false")
								result = result.Replace(target, "");
							else
								result = result + "";
							result = result.ReplaceFirst(delim_start + delim_end, "");
							break;
						default:
							break;
					}
				}
			}
			while (script != "");
		}
	}
}
