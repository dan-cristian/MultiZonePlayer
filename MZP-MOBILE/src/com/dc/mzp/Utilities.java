package com.dc.mzp;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.ActivityManager.RunningServiceInfo;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.media.Ringtone;
import android.media.RingtoneManager;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.net.Uri;
import android.net.wifi.WifiManager;
import android.os.Vibrator;
import android.util.Log;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

public class Utilities {

	public static boolean setWifiState(Context ctx, boolean onState)
	{
		WifiManager wifi = (WifiManager)ctx.getSystemService(Context.WIFI_SERVICE);
		return wifi.setWifiEnabled(onState);
	}
	
	public static boolean getWifiOnState(Context ctx)
	{
		WifiManager wifi = (WifiManager)ctx.getSystemService(Context.WIFI_SERVICE);
		return (wifi.getWifiState()==WifiManager.WIFI_STATE_ENABLED);
	}
	
	public static void showStatus(TextView txStatus, ProgressBar progBar, int percentComplete, CharSequence message)
	{
		if (txStatus != null)
		{
			CharSequence c = txStatus.getText() + "|" + message;
			int over = Math.max(0, c.length()-70);
			c = c.subSequence(over, c.length());
			txStatus.setText(c);
			if (progBar != null)
				progBar.setProgress(percentComplete);
		}
	}
	
	public static void vibrate(Context ctx)
	{
		Vibrator v = (Vibrator)ctx.getSystemService(Context.VIBRATOR_SERVICE);

		// 1. Vibrate for 1000 milliseconds
		long milliseconds = 2000;
		v.vibrate(milliseconds);

		// 2. Vibrate in a Pattern with 500ms on, 500ms off for 5 times
		//long[] pattern = { 500, 300 };
		//v.vibrate(pattern, 5);
	}
	
	public static void vibrateShort(Context ctx)
	{
		Vibrator v = (Vibrator)ctx.getSystemService(Context.VIBRATOR_SERVICE);
		// 1. Vibrate for 1000 milliseconds
		long milliseconds = 500;
		v.vibrate(milliseconds);
	}
	
	public static void showPopupMessage(String text)
    {
    	Toast.makeText(MainActivity.m_activity.getApplicationContext(), text, Toast.LENGTH_LONG).show();
    }
	
	public static void showNotification(CharSequence title, CharSequence message, Context ctx)
	{
		/*Notification notification = new Notification(android.R.drawable.ic_popup_reminder,
	            message, System.currentTimeMillis());
		notification.defaults |= Notification.DEFAULT_SOUND;
		notification.defaults |= Notification.DEFAULT_VIBRATE;
		*/
		//show notification
		String ns = Context.NOTIFICATION_SERVICE;
		NotificationManager mNotificationManager = (NotificationManager) ctx.getSystemService(ns);
		int icon = R.drawable.ic_action_name;//R.drawable.notification_icon;
		CharSequence tickerText = message;
		long when = System.currentTimeMillis();
		Notification notification = new Notification(icon, tickerText, when);
		CharSequence contentTitle = title;
		CharSequence contentText = message;
		Intent notificationIntent = new Intent(ctx, MainActivity.class);
		PendingIntent contentIntent = PendingIntent.getActivity(ctx, 0, notificationIntent, 0);
		notification.setLatestEventInfo(ctx, contentTitle, contentText, contentIntent);
		final int HELLO_ID = 1;
		mNotificationManager.notify(HELLO_ID, notification);
		
		//play default sound
		Uri notification1 = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);
		Ringtone r = RingtoneManager.getRingtone(ctx, notification1);
		if (r != null) r.play();
		
	}
	
	public static long getPauseIntervalSeconds(Context ctx)
	{
		ConnectivityManager connectManager = (ConnectivityManager)ctx.getSystemService(Context.CONNECTIVITY_SERVICE);
		long pause = 0;
		if (connectManager.getNetworkInfo(ConnectivityManager.TYPE_MOBILE).getState() == NetworkInfo.State.CONNECTED)
			pause = Metadata.Settings.getPoolInterval3G();
		if (connectManager.getNetworkInfo(ConnectivityManager.TYPE_WIFI).getState() == NetworkInfo.State.CONNECTED)
			pause = Metadata.Settings.getPoolIntervalWifi();
		if (pause == 0)
		{
			Log.i("utilities", "warning pause = 0, set to 30");
			pause=30;
		}
		return pause;
	}
	
	public static boolean isServiceRunning(Context ctx)
	{
		ActivityManager manager = (ActivityManager) ctx.getSystemService(Activity.ACTIVITY_SERVICE);
	    for (RunningServiceInfo servicerun : manager.getRunningServices(Integer.MAX_VALUE)) {
	    	//Log.i("util", servicerun.service.getClassName());
	        if (LocalService.class.toString().equals("class "+servicerun.service.getClassName())) {
	            return true;
	        }
	    }
	    return false;
	}
}

/*
class AlarmHandler
{
	private CamAlert _alarm;
	private TextView _txStatus;
	private ProgressBar _progBar;
	private int _serverIndex;
	private static ArrayList<DialogYesNo> m_instanceList = new ArrayList<DialogYesNo>();
	
	public AlarmHandler(CamAlert alarm, TextView txStatus, ProgressBar progBar, int serverIndex)
	{
		_alarm = alarm;
		_txStatus = txStatus;
		_progBar = progBar;
		_serverIndex = serverIndex;
		
		for (int i=0; i<m_instanceList.size();i++) {
			m_instanceList.get(i).InvokeNegativeAction();
		}
		
		DialogYesNo dialog =  new DialogYesNo(txStatus.getContext(), "Dismiss new alarm source="+alarm.AlarmSource 
				+ " Message="+alarm.CustomMessage + " Time="+alarm.AlarmTime + " index="+alarm.Index, 
				alarmDialogClickListener);
		m_instanceList.add(dialog);
		Utilities.Vibrate(MainActivity.m_activity.getApplicationContext());
	}
	
	public DialogInterface.OnClickListener alarmDialogClickListener = new DialogInterface.OnClickListener() {
    //@Override
    public void onClick(DialogInterface dialog, int which) {
    	ValueList param;
    	CommandResult res;
    	switch (which){
        case DialogInterface.BUTTON_POSITIVE:
            //Yes button clicked
        	param = new ValueList(Metadata.GlobalParams.command, GlobalCommands.dismissalert.toString());
    		param.Add(Metadata.GlobalParams.alertindex, Integer.toString(_alarm.Index));
    		res = API.SendCommand(_txStatus, _progBar, _serverIndex, param);
            break;
        case DialogInterface.BUTTON_NEUTRAL:
            //Neutral button clicked - all source
        	param = new ValueList(Metadata.GlobalParams.command, GlobalCommands.dismissalert.toString());
    		param.Add(Metadata.GlobalParams.alertsource, _alarm.AlarmSource);
    		res = API.SendCommand(_txStatus, _progBar, _serverIndex, param);
        case DialogInterface.BUTTON_NEGATIVE:
            //No button clicked
        	
            break;
        }
    }
};

}
*/
