package com.dc.mzp;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import com.google.gson.Gson;

import android.content.Context;
import android.util.Log;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;


public class API {

	private static ServerStatus m_lastServerStatus = new ServerStatus();
	private static int currentServerIndex = -1;
	public static String m_lastAlertTime="";
	private static Context m_context;
	public static List<String> m_alertIds = new ArrayList<String>();
	public static List<String> m_alertValues = new ArrayList<String>();
	public static boolean m_hasNewAlert = false;
	
	public static void setContext(Context p_context)
	{
		m_context = p_context;
	}
	
	public static int getCurrentServerIndex() {
		return currentServerIndex;
	}



	public static void setCurrentServerIndex(int currentServerIndex) {
		API.currentServerIndex = currentServerIndex;
	}



	public static ServerStatus getLastServerStatus() {
		return m_lastServerStatus;
	}
	
	

	public static ZoneDetails GetZone(int zoneId, ServerStatus srv){
		ZoneDetails zone;
		for (int i=0;i<srv.ZoneDetails.length;i++)
		{
			zone = srv.ZoneDetails[i];
			if (zone.ZoneId == zoneId)
				return zone;
		}
		return null;
	}
	
	
	public synchronized static CommandResult getServerStatus(TextView txStatus, ProgressBar progBar)
	{
		CommandResult res= new CommandResult(Metadata.ResultEnum.ERR, "err");
		HttpResult result;
		try
		{
			//res.ErrorMessage += " try";
			ServerStatus srv ;
			Utilities.showStatus(txStatus, progBar, 0, "stat");
			ValueList val = new ValueList(Metadata.GlobalParams.command, Metadata.GlobalCommands.status.toString());
			//res.ErrorMessage += " http go";
			result = HttpClient.SendHttpGet(txStatus, progBar, Metadata.Settings.GetServerURL(currentServerIndex).toString(), val);
			String json = result.HttpResponse;
			//res.ErrorMessage += " http end";
	        if (json != null)
	        {
	        	res = new Gson().fromJson(json, CommandResult.class);
	        	srv = res.ServerStatus;
	        }
	        else
	        {
	        	srv = new ServerStatus();
	        	srv.IsServerOn = false;
	        	//res.ErrorMessage += " null json";
	        }
	        
	        if (result.HttpError != null)
	        	res.ErrorMessage += "-"+result.HttpError;
	        
	        m_lastServerStatus = srv;
	        Utilities.showStatus(txStatus, progBar, 100, "|");
	        
	        //if (MainActivity.m_activity != null)
	        ///	MainActivity.m_activity.SetStatus();
	        ///res.ErrorMessage += " serv";
	        doServiceActivities();
		}
		catch (Exception ex)
		{
			Utilities.showStatus(txStatus, progBar, 100, "Err1 "+ex.getMessage());
			res= new CommandResult(Metadata.ResultEnum.ERR, "exception " + ex.getMessage() + "|"+res.ErrorMessage);
		}
        
        return res;
	}
	/*
	public static CommandResult SelectZone(String zoneId)
	{
		String json = HttpClient.SendHttpGet(Metadata.Settings.ServerURL.toString(), Metadata.GlobalCommands.selectzone.toString(), zoneId);
		CommandResult res = new Gson().fromJson(json, CommandResult.class);
		Toast.makeText(MainActivity.m_activity.getApplicationContext(), res.Result+":"+res.ErrorMessage, Toast.LENGTH_LONG).show();
		return res;
	}*/
	
	public synchronized static CommandResult sendCommand(TextView txStatus, ProgressBar progBar, ValueList... params)
	{
		CommandResult res= new CommandResult(Metadata.ResultEnum.ERR, "unknown2");
		try
		{
			Utilities.showStatus(txStatus, progBar, 0, ""+params[0].GetValue(Metadata.GlobalParams.command));
			String json = HttpClient.SendHttpGet(txStatus, progBar, Metadata.Settings.GetServerURL(currentServerIndex).toString(), params).HttpResponse;
			if (json != null)
			{
				res = new Gson().fromJson(json, CommandResult.class);
				m_lastServerStatus = res.ServerStatus;
				if (res.Result.equals(Metadata.ResultEnum.ERR))
					Toast.makeText(MainActivity.m_activity.getApplicationContext(), res.Result+":"+res.ErrorMessage, Toast.LENGTH_LONG).show();
				doServiceActivities();
				MainActivity.m_activity.updateStatus();
			}
			else
			{
				Log.i("API", "result was null, unexpected");
				Utilities.showStatus(txStatus, progBar, 100, "Er0 no response");
				res = new CommandResult(Metadata.ResultEnum.ERR, "null json");
			}
		}
		catch (Exception ex)
		{
			Utilities.showStatus(txStatus, progBar, 100, "Er1 "+ex.getMessage());
			ex.printStackTrace();
			res = new CommandResult(Metadata.ResultEnum.ERR, "exception " + ex.getMessage());
		}
		Utilities.showStatus(txStatus, progBar, 100, "|");
		return res;
	}
	
	private static void doServiceActivities()
	{
		//API.getLastServerStatus();
		if (API.getLastServerStatus().CamAlertList != null)
		{
			displayNotificationAlerts();
			
		}
	}
	
	private static void displayNotificationAlerts()
    {
    	CamAlert alarm, newestAlarm;
    	List<CamAlert> alertList = API.getLastServerStatus().CamAlertList;
		
		if (alertList.size()>0 && (!m_lastAlertTime.equals(alertList.get(0).AlarmTime)))
		{
			m_alertIds.clear();
    		m_alertValues.clear();
    		Iterator<CamAlert> iterator = alertList.iterator();
    		newestAlarm = alertList.get(0);
    		while (iterator.hasNext())
    		{
    			alarm = iterator.next();
    			m_alertIds.add(Integer.toString(alarm.Index));
    			m_alertValues.add(alarm.AlarmTime+" | "+ alarm.AlarmSource + " | " + alarm.CustomMessage);
    		}
    		Utilities.vibrate(m_context);
    		Utilities.showNotification("New MZP Alert", newestAlarm.AlarmTime+" | "+ newestAlarm.AlarmSource + " | " + newestAlarm.CustomMessage, m_context);
    		m_hasNewAlert = true;
    		m_lastAlertTime = newestAlarm.AlarmTime;
		}
    	
    }
	
	public synchronized static void WakeOnLan(Context ctx)
	{
		boolean res = WakeOnLan.SendMagicPacket(Metadata.Settings.GetServerIPWOL(currentServerIndex).toString(), Metadata.Settings.GetServerMAC(currentServerIndex).toString());
		
		Toast.makeText(ctx, "WOL Sent: "+res, Toast.LENGTH_LONG).show();
	}
	
	
}
