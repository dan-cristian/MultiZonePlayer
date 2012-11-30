using System;
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

namespace MultiZonePlayer
{
    

    class WebServer
    {
        private static HttpListener m_extlistener = new HttpListener();
        private static HttpListener m_intlistener = new HttpListener();
        private static WebServer m_instance;
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
        private static Random RANDOM = new Random();

        public static void Initialise()
        {
            m_instance = new WebServer();
            String extlistener = "https://*:" + IniFile.PARAM_WEBSERVER_PORT_EXT[1] + "/";
            String intlistener = "http://*:" + IniFile.PARAM_WEBSERVER_PORT_INT[1] + "/";
            
            MLog.Log(null, "Initialising ext web servers " +extlistener);
            m_extlistener.Prefixes.Add(extlistener);
            m_extlistener.AuthenticationSchemes = AuthenticationSchemes.Basic;
            Thread th = new Thread(() => m_instance.RunMainThread(m_extlistener));
            th.Name = "WebListener External";
            th.Start();

            MLog.Log(null, "Initialising int web servers " + intlistener);
            m_intlistener.Prefixes.Add(intlistener);
            m_intlistener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            th = new Thread(() => m_instance.RunMainThread(m_intlistener));
            th.Name = "WebListener Internal";
            th.Start();
        }

        public static void Shutdown()
        {
            m_extlistener.Stop();
            m_intlistener.Stop();
            m_instance = null;
        }

        ~WebServer()
        {
            Shutdown();
        }

        public void RunMainThread(HttpListener listener)
        {
            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                MLog.Log(ex, this, "Error listening");
            }

            while (listener.IsListening)
            {
                try
                {
                    //MLog.Log(null, "Web server waiting for requests");
                    HttpListenerContext ctx = listener.GetContext();
                    //MLog.Log(null, "Web request " + ctx.Request.Url.AbsoluteUri + " from " + ctx.Request.RemoteEndPoint.Address);

                    if (listener.AuthenticationSchemes.Equals(AuthenticationSchemes.Basic))
                    {
                        HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)ctx.User.Identity;
                        if (ctx.Request.RemoteEndPoint.Address.ToString().StartsWith("192.168.0.") ||
                            identity.Name.Equals(IniFile.PARAM_WEB_USER[1]) && (identity.Password.Equals(IniFile.PARAM_WEB_PASS[1])))
                        {
                            Thread th = new Thread(() => ProcessRequest(ctx));
                            th.Name = "Web work thread " + ctx.Request.Url.AbsoluteUri;
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
                        Thread th = new Thread(() => ProcessRequest(ctx));
                        th.Name = "Web work thread " + ctx.Request.Url.AbsoluteUri;
                        th.Start();
                    }
                }
                catch (Exception ex)
                {
                    MLog.Log(ex, "Exception on web server listener");
                    break;
                }
            }
            MLog.Log(null, "Web server listener exit");
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                String cmdResult = "";
                byte[] binaryBuffer = null;
                Metadata.ValueList resvalue=null;
                Metadata.ValueList vals = new Metadata.ValueList(Metadata.CommandSources.Web);

                String requestServer;

                requestServer = Utilities.ExtractServerNameFromURL(request.Url.AbsoluteUri);
                String localPath = request.Url.LocalPath.Replace("..", "");//.Replace("/", "");

                //read command from get url
                Metadata.GlobalParams param;
                foreach (string key in request.QueryString.Keys)
                {
                    if ((key != null) && Enum.IsDefined(typeof(Metadata.GlobalParams), key))
                    {
                        param = (Metadata.GlobalParams)Enum.Parse(typeof(Metadata.GlobalParams), key);
                        vals.Add(param, request.QueryString[key]);
                    }
                    else
                    {
                        MLog.Log(null, "Webserver Unknown parameter received:" + key);
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
                                post = post.Replace("%20", " ");
                                body.Close();
                                reader.Close();
                                break;
                        }
                        String[] atoms = post.Split(new String[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                        String[] vars;
                        foreach (String atom in atoms)
                        {
                            vars = atom.Split(new String[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
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
                            MLog.Log(null, "POST has no entity body, unexpected. " + request.Url.OriginalString);
                        }
                    }

                    WriteResponse("text/html", cmdResult, response, null);

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
            }
            catch (Exception ex)
            {
                MLog.Log(ex, "Error process request " + context.Request.Url.AbsoluteUri);
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
            
            String result = "", localFile="";
            String requestURL = context.Request.RawUrl;

            String[] contentAtoms = requestURL.Split('?');//('/');
            
            localFile = contentAtoms[0];
            if (localFile.Equals("")) localFile = IniFile.PARAM_WEB_DEFAULT_PAGE[1];

            
            //find content type
            String extension;
            String contenttypeentry;
            String[] contentypeatoms,extensionatoms;
                
                

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
                    binaryData = Utilities.ReadBinaryFile("\\webroot\\" + localFile);
                else
                {
                    result = Utilities.ReadFile("\\webroot\\" + localFile);

                    //set client passed params
                    foreach (String param in context.Request.QueryString.Keys)
                    {
                        if ((param != null))
                        {
                            result = result.Replace("%_" + param, context.Request.QueryString[param]);
                        }
                    }
                            
                    //set server variables
                    if (resvalue!=null)
                        result = result.Replace("#HTMLCommandResult#", resvalue.GetValue(Metadata.GlobalParams.msg));
                            
                    String[] varatoms, parameters, methods;
                    object[] methodparams;
                    String property, methodparam, method;
                    object value, objinfo;
                    PropertyInfo propInfo;
                    MethodInfo methInfo;
                            
                    varatoms = result.Split(new String[]{"#"},StringSplitOptions.RemoveEmptyEntries);
                    foreach (String atom in varatoms)
                    {
                        switch (atom[0])
                        {
                            case '!': //property read
                                property = atom.Replace("!", "");
                                propInfo = Type.GetType(this.GetType().FullName).GetProperty(property);
                                if (propInfo != null)
                                {
                                    value = propInfo.GetValue(this, null);
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

                                methodparams = new object[parameters.Length+1];
                                methodparams[0] = context;
                                parameters.CopyTo(methodparams, 1);
                                methInfo = Type.GetType(this.GetType().FullName).GetMethod(method);
                                        
                                if (methInfo != null)
                                {
                                    value = methInfo.Invoke(this, methodparams);
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
                                                        MLog.Log(this, "Unknown call for atom=" + property);
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


        #region used for HTML data
        public Alarm HTMLSystemAlarm(HttpListenerContext context, String methodname)
        {
             return MZPState.Instance.SystemAlarm; 
        }

        public String HTMLZonesOpened
        {
            get { return ControlCenter.GetActiveZonesCount().ToString(); }
        }

        public String HTMLLastAlarmEvents
        {
            get
            {
                String res="";
                var zones = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastMovementDate).ToList();

                foreach (Metadata.ZoneDetails zone in zones)
                {
                    res += zone.ZoneName + "-" + zone.ZoneId + ", Alarm=" + zone.LastAlarmMovementDateTime + ", Cam=" + zone.LastCamAlertDateTime +"<BR>";
                }
                return res;
            }
        }

        public String HTMLRandom
        {
            get { return RANDOM.Next().ToString(); }
        }

        public String HTMLServerTime
        {
            get { return DateTime.Now.ToString("HH:mm:ss"); }
        }

        

        public String HTMLGetCmdValues(HttpListenerContext context, String methodname, String zoneid, String command, String htmldelimiter)
        {
            Metadata.ValueList vals = new Metadata.ValueList(Metadata.CommandSources.Web);
            Metadata.ValueList resvalue;
            String result="";
            
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

        public MZPState HTMLMZPState(HttpListenerContext context, String methodname)
        {
            return MZPState.Instance;
        }

        public Metadata.ZoneDetails HTMLZones(HttpListenerContext context, String methodname, String zoneid)
        {
            return MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
        }

        public Metadata.ZoneDetails HTMLFirstActiveZone(HttpListenerContext context, String methodname)
        {

            List<Metadata.ZoneDetails> zones;
            Metadata.ZoneDetails zone;

            zones = MZPState.Instance.ZoneDetails.OrderByDescending(x => x.LastLocalCommandDateTime).ToList();
            zone = zones.Find(x => x.IsActive == true && (x.ActivityType.Equals(Metadata.GlobalCommands.music)||x.ActivityType.Equals(Metadata.GlobalCommands.streammp3)));
            return zone;
            /*String zonetype = zone!=null?zone.ActivityType.ToString():"status";//return status page if no active zone
            return zonetype;
             */

        }

        public String HTMLDVRServerPort(HttpListenerContext context, String methodname)
        {
            return Utilities.ExtractServerNameFromURL(context.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_ISPY_PORT[1]; 
        }

        public String HTMLWebServerPort(HttpListenerContext context, String methodname)
        {
            return Utilities.ExtractServerNameFromURL(context.Request.Url.AbsoluteUri) + ":" + IniFile.PARAM_WEBSERVER_PORT_EXT[1];
        }

        public String HTMLZoneMoveStatusAsColor(HttpListenerContext context, String methodname, String zoneid)
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

        public String HTMLGetZoneStatusAsColor(HttpListenerContext context, String methodname, String zoneid, String activity)
        {
            Metadata.ZoneDetails zone = MZPState.Instance.ZoneDetails.Find(x => x.ZoneId.Equals(Convert.ToInt16(zoneid)));
            String color = "inherit";
            if ((zone != null) && (zone.ActivityType.ToString().Equals(activity.ToLower())))
            {
                if (zone.IsActive) color = "Red";
            }
            

            return color;
        }

        #endregion
    }


}
