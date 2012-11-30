package com.dc.mzp;

import com.dc.mzp.Metadata.ResultEnum;

import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

public class LocalService extends Service {
    private NotificationManager mNM;
    private LocalServiceThread m_thread;
    
    
    // Unique Identification Number for the Notification.
    // We use it on Notification start, and to cancel it.
    private int NOTIFICATION = 1234;

    /**
     * Class for clients to access.  Because we know this service always
     * runs in the same process as its clients, we don't need to deal with
     * IPC.
     */
    public class LocalBinder extends Binder {
        LocalService getService() {
            return LocalService.this;
        }
    }

    @Override
    public void onCreate() {
        mNM = (NotificationManager)getSystemService(NOTIFICATION_SERVICE);
        Log.i("localservice","oncreate");
        // Display a notification about us starting.  We put an icon in the status bar.
        //showNotification();
        m_thread = new LocalServiceThread();
        m_thread.setContext(getApplicationContext());
        m_thread.start();
		//m_StatusTimer.execute(m_activity);
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        Log.i("LocalService", "Received start id " + startId + ": " + intent);
        // We want this service to continue running until it is explicitly
        // stopped, so return sticky.
        return START_STICKY;
    }

    @Override
    public void onDestroy() {
    	Log.i("localservice","ondestroy");
        // Cancel the persistent notification.
        mNM.cancel(NOTIFICATION);
        m_thread.stopService();
        
        // Tell the user we stopped.
        //Toast.makeText(this, "Service destroy", Toast.LENGTH_SHORT).show();
    }

    @Override
    public IBinder onBind(Intent intent) {
        return mBinder;
    }

    
    
    // This is the object that receives interactions from clients.  See
    // RemoteService for a more complete example.
    private final IBinder mBinder = new LocalBinder();

    /**
     * Show a notification while this service is running.
     */
    private void showNotification(CharSequence text) {
        // In this sample, we'll use the same text for the ticker and the expanded notification
        

        // Set the icon, scrolling text and timestamp
        Notification notification = new Notification(R.drawable.ic_action_name, text,
                System.currentTimeMillis());

        // The PendingIntent to launch our activity if the user selects this notification
        PendingIntent contentIntent = PendingIntent.getActivity(this, 0,
                new Intent(this, MainActivity.class), 0);

        // Set the info for the views that show in the notification panel.
        notification.setLatestEventInfo(this, "title text",
                       text, contentIntent);

        // Send the notification.
        mNM.notify(NOTIFICATION, notification);
    }
    
    
    
    public LocalServiceThread getLocalServiceThread()
    {
    	return m_thread;
    }
    
    ///////////////////////////////////////////////////
    class LocalServiceThread extends Thread
    {
    	private Boolean m_isActive = false;
    	private Context m_context;
    	private boolean m_hasNewData;
    	
    	@Override
    	public void run()
    	{
    		CommandResult res;
    		boolean hadError = false;
    		m_isActive = true;
    		Utilities.showNotification("MZP Notification", "Local service run", m_context);
    		Log.i("localsvcthread", "started, initial sleep seconds="+Utilities.getPauseIntervalSeconds(m_context));
    		while (m_isActive && (!isInterrupted()))
    		{
    			try {
    				res = API.getServerStatus(null, null);
    				if (res.Result.equals(ResultEnum.OK)){
    					m_hasNewData = true;
    					if (hadError)
    					{
    						Utilities.showNotification("MZP Error Solved", "Error solved", m_context);
    						Utilities.vibrateShort(m_context);
    						hadError = false;
    					}
    				}
    				else {
    					if (!hadError)
    					{
    						Utilities.showNotification("MZP Error", res.ErrorMessage, m_context);
    						Utilities.vibrateShort(m_context);
    						Utilities.vibrateShort(m_context);
    					}
    					hadError = true;
    				}
    				Log.i("localsvth", "sleeping secs=" + Utilities.getPauseIntervalSeconds(m_context));
    				sleep(Utilities.getPauseIntervalSeconds(m_context)*1000);
    			} catch (InterruptedException e) {
    				// TODO Auto-generated catch block
    				Log.i("localsvcthread", "Interrupted");
    			}
    		}
    		Utilities.showNotification("MZP Notification", "Local service exit", m_context);
    		Utilities.vibrateShort(m_context);
    		Log.i("localsvcthread", "Interrupted and exit");
    	}
    	
    	public void setContext(Context p_context)
    	{
    		m_context = p_context;
    	}
    	public void stopService()
    	{
    		m_isActive = false;
    		this.interrupt();
    	}
    	
    	public boolean hasNewData()
    	{
    		return m_hasNewData;
    	}
    	
    	public void consumeData()
    	{
    		m_hasNewData = false;
    	}
    }
}

