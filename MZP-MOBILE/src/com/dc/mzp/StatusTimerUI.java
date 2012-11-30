package com.dc.mzp;

import android.os.AsyncTask;
import android.util.Log;

public class StatusTimerUI extends AsyncTask<MainActivity, Integer, Integer>
{
	private MainActivity m_activity;
	
	@Override
	protected void onPreExecute() {
        super.onPreExecute();
        
    }

	@Override
	protected Integer doInBackground(MainActivity... activity) {
		m_activity = activity[0];
				
        while (!isCancelled())
        {
        	publishProgress(0);
        	try {
        		Thread.sleep(30000);
			} catch (InterruptedException e) {
				
				Log.i("StatusTimer", "Interrupted, as expected");
			}
        }
        return 1;
    }
	
	@Override
    protected void onProgressUpdate(Integer... i) {
		//API.GetServerStatus(m_activity.getStatusTextView(), null);
		Log.i("timer", "progress update");
		m_activity.updateStatus();
    }

	@Override
    protected void onPostExecute(Integer i) {
        
    }

}
