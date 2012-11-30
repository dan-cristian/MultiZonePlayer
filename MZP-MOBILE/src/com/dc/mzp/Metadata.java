package com.dc.mzp;

import java.util.ArrayList;
import java.util.List;

import com.dc.mzp.Metadata.ResultEnum;

import android.content.SharedPreferences;
import android.util.Log;

public class Metadata {
	
    
    public static enum ResultEnum
    {
    	ERR, OK
    }
    public static class Settings
    {
    	private static CharSequence[] ServerIP;
    	private static CharSequence[] ServerIPWOL;
    	private static CharSequence[] ServerURL;
    	private static CharSequence[] ServerMAC;
    	private static int m_serverCount;
    	private static int m_poolIntervalWifi;
    	private static int m_poolInterval3G;
    	private static int m_defaultServerIndex;
    	private static String PREF_NAME = "MZP";
    	
    	private static String Key_ServerIPWOL="ServerIPWOL";
    	private static String Key_ServerURL="ServerURL";
    	private static String Key_ServerIP="ServerIP";
    	private static String Key_ServerMAC="ServerMAC";
    	private static String Key_ServersCount="ServersCount";
    	private static String Key_PoolIntervalWifi="PoolIntervalWifi";
    	private static String Key_PoolInterval3G="PoolInterval3G";
    	private static String Key_DefaultServerIndex="DefaultServerIndex";
    	
    	public static void Initialise()
    	{
    		// Restore preferences
    	       SharedPreferences settings = MainActivity.m_activity.getSharedPreferences(PREF_NAME, 0);
    	       
    	       m_serverCount = settings.getInt(Key_ServersCount, 3);
    	       ServerIPWOL = new CharSequence[m_serverCount];
    	       ServerURL = new CharSequence[m_serverCount];
    	       ServerIP = new CharSequence[m_serverCount];
    	       ServerMAC = new CharSequence[m_serverCount];
    	       
    	       for (int i=0; i< m_serverCount; i++)
    	       {
    	    	   ServerIPWOL[i] = settings.getString(Key_ServerIPWOL+i, "192.168.0.255");
    	    	   ServerURL[i] = settings.getString(Key_ServerURL+i, "http://192.168.0.10:12345/cmd");
    	    	   ServerIP[i] = settings.getString(Key_ServerIP+i, "192.168.0.10");
    	    	   ServerMAC[i] = settings.getString(Key_ServerMAC+i, "00:1F:D0:9F:E5:B6");
    	       }
    	       
    	       m_poolIntervalWifi = settings.getInt(Key_PoolIntervalWifi, 15);
    	       m_poolInterval3G = settings.getInt(Key_PoolInterval3G, 60);
    	       m_defaultServerIndex = settings.getInt(Key_DefaultServerIndex, 0);
    	}
    	
    	public static void Save(int index)
    	{
    		// We need an Editor object to make preference changes.
    	      // All objects are from android.context.Context
    	      SharedPreferences settings = MainActivity.m_activity.getSharedPreferences(PREF_NAME, 0);
    	      SharedPreferences.Editor editor = settings.edit();
    	      if (index >=0)
    	      {
	    	      editor.putString(Key_ServerIPWOL+index, ServerIPWOL[index].toString());
	    	      editor.putString(Key_ServerURL+index, ServerURL[index].toString());
	    	      editor.putString(Key_ServerIP+index, ServerIP[index].toString());
	    	      editor.putString(Key_ServerMAC+index, ServerMAC[index].toString());
    	      }
    	      editor.putInt(Key_ServersCount, m_serverCount);
    	      editor.putInt(Key_DefaultServerIndex, m_defaultServerIndex);
    	      editor.putInt(Key_PoolIntervalWifi, m_poolIntervalWifi);
    	      editor.putInt(Key_PoolInterval3G, m_poolInterval3G);
    	      
    	      // Commit the edits!
    	      editor.commit();
    	}
    	
    	public static void Add()
    	{
    		m_serverCount++;
    		Save (-1);
    		Initialise();
    	}
    	
    	public static void Remove(int index)
    	{
    		
    	}
    	
    	public static int getServerCount()
    	{
    		return m_serverCount;
    	}
    	public static CharSequence[] GetServerIPList()
    	{
    		return ServerIP;
    	}
    	public static CharSequence GetServerURL(int index)
    	{
    		if (index < m_serverCount)
    			return ServerURL[index];
    		else
    			Log.i("metadata settings", "out of bounds index="+index);
    		return "";
    	}
    	public static CharSequence GetServerIP(int index)
    	{
    		if (index < m_serverCount)
    			return ServerIP[index];
    		else
    			Log.i("metadata settings", "out of bounds index="+index);
    		return "";
    	}
    	public static CharSequence GetServerMAC(int index)
    	{
    		if (index < m_serverCount)
    			return ServerMAC[index];
    		else
    			Log.i("metadata settings", "out of bounds index="+index);
    		return "";
    	}
    	public static CharSequence GetServerIPWOL(int index)
    	{
    		if (index < m_serverCount)
    			return ServerIPWOL[index];
    		else
    			Log.i("metadata settings", "out of bounds index="+index);
    		return "";
    	}
    	
    	public static int getPoolIntervalWifi()
    	{
    		return m_poolIntervalWifi;
    	}
    	
    	public static int getPoolInterval3G()
    	{
    		return m_poolInterval3G;
    	}
    	
    	public static int getDefaultServerIndex()
    	{
    		return m_defaultServerIndex;
    	}
    	public static void setDefaultServerIndex(int index)
    	{
    		m_defaultServerIndex = index;
    	}
    	public static void SetServerIP(int index, CharSequence value)
    	{
    		ServerIP[index] = value;
    	}
    	public static void SetServerMAC(int index, CharSequence value)
    	{
    		ServerMAC[index] = value;
    	}
    	public static void SetServerURL(int index, CharSequence value)
    	{
    		ServerURL[index] = value;
    	}
    	public static void SetServerIPWOL(int index, CharSequence value)
    	{
    		ServerIPWOL[index] = value;
    	}
    	public static void setPoolIntervalWifi(int interval)
    	{
    		m_poolIntervalWifi = interval;
    	}
    	public static void setPoolInterval3G(int interval)
    	{
    		m_poolInterval3G = interval;
    	}
    }
    
	public static enum GlobalCommands
    {
		nul,
		status,
        selectzone,
        sleep,
        shutdown,
        music,
		video,
		play,
		stop,
		pause,
		next,
		previous,
		rewind,
		ffwd,
		up,
		down,
		left,
		right,
		volumeset,
		volumedown,
		volumeup,
		mute,
		repeat,
		fullscreen,
		ratingset,
		enter,
		record,
		guide,
        chup,
        chdown,
        photo,
        dvd,
        radio,
        back,
        musicclone,
        microphone,
        genrelist,
        setgenrelist,
        sleeptimer,
        artistlist,
        setartistlist,
        setwaketimer,
        medialist,
        setmediaitem,
        motionalert,
        dismissalert,
        togglealert,
        setmoodmusic,
        getmoodmusiclist,
        searchmediaitem
    }
	
	public static enum GlobalParams
    {
        command,
        zoneid,
        msg,
        result,
        selectedindex,
        datetime,
        volumelevel,
        ratingvalue,
        indexarray,
        activity,
        alertsource,
        alertindex,
        oid,//=camid
        pass,
        searchvalue
        
    }
}

class CommandResult
{
	public ResultEnum Result;
	public String ErrorMessage="default value";
	public ServerStatus ServerStatus;
	public ValueList ValueList;
	
	public CommandResult(ResultEnum result, String message)
	{
		Result = result;
		ErrorMessage = message;
	}
}

class VolumeLevels
{
    public static int VolumeFull = 0;
    public static int VolumeDefault = -4500;
    public static int VolumeSilence = -6000;
}

enum ZoneState
{
    NotInitialised,
    NotStarted,
    Running,
    Paused,
    Closing,
    Undefined
}

class ValueList
{
	public List<String> Values;
	public List<String> Keys;
	public List<String> IndexList;
	public List<String> IndexValueList;
	
	public ValueList()
	{
		Values = new ArrayList<String>();
		Keys = new ArrayList<String>();
	}
	
	/*
	public ValueList(String firstParamValue)
    {
        Values = new ArrayList<String>();
        Values.add(firstParamValue);
    }*/
	
	public ValueList(Metadata.GlobalParams firstParamKey, String firstParamValue)
    {
        Add(firstParamKey, firstParamValue);
    }
	/*
	public void Add(String s)
	{
		Values.add(s);
	}*/
	
	public void Add(Metadata.GlobalParams key, String value) 
    {
		if (Keys == null) Keys = new ArrayList<String>();
		if (Values == null) Values = new ArrayList<String>();
        if (Values.size() == Keys.size())
        {
            Values.add(value);
            Keys.add(key.toString());
        }
        else
            Log.i("ValueList", "Error - Number of values is not equal to number of ids, incorrect use");
    }
	
	public String GetValue(Metadata.GlobalParams key)
    {
        int index = Keys.indexOf(key.toString());
        if (index >= 0)
            return Values.get(index);
        else
            return null;
    }
	
	public List<String> GetIndexList()
	{
		return IndexList;
	}
	
	public List<String> GetIndexValueList()
	{
		return IndexValueList;
	}
	
	public void AddIndexPair(String index, String indexValue)
	{
		if (IndexList==null) IndexList = new ArrayList<String>();
		if (IndexValueList==null) IndexValueList = new ArrayList<String>();
		IndexList.add(index);
		IndexValueList.add(indexValue);
	}
}

class ZoneDetails
{
	public int ZoneId = 0;
    public Boolean IsActive = false;
    public int MinutesUntilSleep = -1;
    public String SleepHourMin = "";//format HH:MM
    public String ZoneName = "na";
    public ZoneState ZoneState;
    public int VolumeLevel = VolumeLevels.VolumeSilence;
    public long Position = 0;
    public Metadata.GlobalCommands ActivityType = Metadata.GlobalCommands.status;
    public String Title = "na";
    public int Rating = 0;
    public int Playcount = 0;
    public String Author = "";
    public String Genre = "";
    public String Album = "";
    public String Year = "";
    public String WakeTime="";
    public String CameraId = "";
    public Boolean CameraAlertActive = false;

    public ZoneDetails(int p_zoneId, String p_zoneName)
    {
        ZoneId = p_zoneId;
        ZoneName = p_zoneName;
        ZoneState = com.dc.mzp.ZoneState.NotInitialised;
    }
}

class CamAlert
{
	public int Index;
    public String AlarmTime;
    public String AlarmSource;
    public String CustomMessage;
    public boolean WasAcknowledged;
}

class ServerStatus
{
    public Boolean IsServerOn = true;
    public ZoneDetails[] ZoneDetails = null;
    public List<CamAlert> CamAlertList = null;
}