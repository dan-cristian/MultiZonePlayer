/*
 * Copyright (C) 2007 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package com.dc.mzp;

import android.app.ListActivity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.graphics.Color;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnLongClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.CheckBox;
import android.widget.ImageButton;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;
import com.dc.mzp.Metadata.GlobalCommands;
import com.dc.mzp.R;

/**
 * This class provides a basic demonstration of how to write an Android
 * activity. Inside of its window, it places a single view: an EditText that
 * displays and edits some internal text.
 */
public class MainActivity extends ListActivity {
    
    static final private int BACK_ID = Menu.FIRST;
    static final private int SETTINGS_ID = Menu.FIRST + 1;
    static final private int SERVICEON_ID = Menu.FIRST + 2;
    static final private int SERVICEOFF_ID = Menu.FIRST + 3;
    static final private int SERVICECONON_ID = Menu.FIRST + 4;
    
    
    private boolean m_wifiInitialOnState;
    private TextView m_txStatus;
    private StatusTimerUI m_StatusTimer;
    public static MainActivity m_activity;
    private ImageButton m_ServerPower;
    private Metadata.GlobalCommands m_currentCommand = Metadata.GlobalCommands.music;
    //private ServerStatus m_serverStatus = null;
    private Spinner m_spinnerServers;
    private ImageButton m_btMusic;
    private ImageButton m_btVideo;
    private Button m_btAlerts;
    private MultipleSelectionButton m_btAlertsSelection;
    //private int m_currentServerIndex;
    private ZoneAdapter m_zoneAdapter;
    private ProgressBar m_progStatusBar;
    
    private boolean m_ignoreSpinnerEvent = true;
    private boolean mIsBound;
    public LocalService BoundService;
    
    private ServiceConnection mConnection = new ServiceConnection() {
        public void onServiceConnected(ComponentName className, IBinder service) {
            // This is called when the connection with the service has been
            // established, giving us the service object we can use to
            // interact with the service.  Because we have bound to a explicit
            // service that we know is running in our own process, we can
            // cast its IBinder to a concrete class and directly access it.
            BoundService = ((LocalService.LocalBinder)service).getService();
            mIsBound = true;
            Log.i("svc", "Service connected");
            // Tell the user about this for our demo.
            //Toast.makeText(MainActivity.this, "Service connected", Toast.LENGTH_SHORT).show();
            
        }

        public void onServiceDisconnected(ComponentName className) {
            // This is called when the connection with the service has been
            // unexpectedly disconnected -- that is, its process crashed.
            // Because it is running in our same process, we should never
            // see this happen.
        	
        	BoundService = null;
        	mIsBound = false;
            Log.i("svc", "Service disconnected");
            //Toast.makeText(MainActivity.this, "Service disconnected", Toast.LENGTH_SHORT).show();
        }
    };
    
    public MainActivity() {
    }

    /** Called with the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Log.i("main", "On Main Create");
        // Inflate our UI from its XML layout description.
        setContentView(R.layout.main_activity);

        m_activity = this;
        API.setContext(this);
        
        
        
        
        // Find the text editor view inside the layout, because we
        // want to do various programmatic things with it.
        
        m_txStatus = (TextView) findViewById(R.id.txStatus);
        m_ServerPower = (ImageButton) findViewById(R.id.btPower);
        m_ServerPower.setLongClickable(true);
        m_spinnerServers = (Spinner) findViewById(R.id.spinnerServers);
        
        // Hook up button presses to the appropriate event handler.
        m_ServerPower.setOnClickListener(onClickServerPower);
        m_spinnerServers.setOnItemSelectedListener(onItemSelectedSpinnerServer);
        m_btAlerts = (Button) findViewById(R.id.btAlerts);
        m_btAlertsSelection = new MultipleSelectionButton(m_btAlerts, "Alerts", MainActivity.this, alertsListenerCallback, true);
        
        m_btMusic = ((ImageButton) findViewById(R.id.btMusic));
        m_btMusic.setOnClickListener(OnClickMusic);
        
        m_btVideo = ((ImageButton) findViewById(R.id.btVideo));
        m_btVideo.setOnClickListener(OnClickVideo);
        
        
        m_zoneAdapter = new ZoneAdapter(this, onClickZone, onClickZoneAlarm); 
        setListAdapter(m_zoneAdapter);
        
        m_progStatusBar = (ProgressBar) this.findViewById(R.id.progressStatus);
        
        ((ImageButton) findViewById(R.id.btRefresh)).setOnClickListener(onStatusRefresh);
        
        m_wifiInitialOnState = Utilities.getWifiOnState(this);
    }

    /**
     * Called when the activity is about to start interacting with the user.
     */
    @Override
    protected void onResume() {
        super.onResume();
        Utilities.showPopupMessage("On Main Resume");
        Log.i("main", "On Main Resume");
        
        Metadata.Settings.Initialise();
        API.setCurrentServerIndex(Metadata.Settings.getDefaultServerIndex());
        
        if (!Utilities.getWifiOnState(this))
        {
        	Utilities.showStatus(m_txStatus, null, 0, "Start WIFI");
        	Utilities.setWifiState(this, true);
        }
        API.getServerStatus(m_txStatus, m_progStatusBar);
        m_ignoreSpinnerEvent = true;
        MySpinnerAdapter.AddListAdapter(m_spinnerServers, this, Metadata.Settings.GetServerIPList());
        m_spinnerServers.setSelection(API.getCurrentServerIndex());
        if (!Utilities.isServiceRunning(this))
        	startLocalService();
        if (BoundService == null)
        	doBindService();
        
        if (m_StatusTimer==null)
        {
			m_StatusTimer = new StatusTimerUI();
			m_StatusTimer.execute(m_activity);
        }
		
        updateStatus();
    }

    @Override
    protected void onSaveInstanceState(Bundle outState)
    {
    	//Utilities.Vibrate(this);
    	Utilities.showPopupMessage("On Main save");
    	Log.i("main", "On Main save");
    	//outState.putBoolean("POOL", m_chkStatus.isChecked());
    	super.onSaveInstanceState(outState);
    }
    
    @Override
    protected void onRestoreInstanceState(Bundle state)
    {
    	super.onRestoreInstanceState(state);
    	Utilities.showPopupMessage("On Main restore");
    	Log.i("main", "On Main restore");
    	//m_chkStatus.setChecked(state.getBoolean("POOL"));
    	//Utilities.Vibrate(this);
    }
    
    @Override
    protected void onDestroy() {
        super.onDestroy();
        Log.i("main", "On Main destroy");
        doUnbindService();
        if (m_StatusTimer != null)
		{
			m_StatusTimer.cancel(true);
			m_StatusTimer = null;
		}
    }
    
    private void doBindService() {
        // Establish a connection with the service.  We use an explicit
        // class name because we want a specific service implementation that
        // we know will be running in our own process (and thus won't be
        // supporting component replacement by other applications).
        Boolean b = this.bindService(new Intent(MainActivity.this, LocalService.class), mConnection, Context.BIND_AUTO_CREATE);
        
    }

    private void doUnbindService() {
        if (mIsBound) {
        	Log.i("main", "unbinding");
            // Detach our existing connection.
        	this.unbindService(mConnection);
        }
    }
    
    private void startLocalService()
    {
    	startService(new Intent(this,LocalService.class));
    }
    
    private void stopLocalService()
    {
    	stopService(new Intent(this,LocalService.class));
    }
    
    /**
     * Called when your activity's options menu needs to be created.
     */
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        super.onCreateOptionsMenu(menu);

        // We are going to create two menus. Note that we assign them
        // unique integer IDs, labels from our string resources, and
        // given them shortcuts.
        menu.add(0, BACK_ID, 0, R.string.back).setShortcut('0', 'b');
        menu.add(0, SETTINGS_ID, 0, R.string.settings).setShortcut('1', 's');
        menu.add(0, SERVICEON_ID, 0, "Service On").setShortcut('2', 'n');
        menu.add(0, SERVICEOFF_ID, 0, "Service Off").setShortcut('3', 'f');
        menu.add(0, SERVICECONON_ID, 0, "Svc Con On").setShortcut('4', 'o');
        return true;
    }

    /**
     * Called right before your activity's option menu is displayed.
     */
    @Override
    public boolean onPrepareOptionsMenu(Menu menu) {
        super.onPrepareOptionsMenu(menu);

        // Before showing the menu, we need to decide whether the clear
        // item is enabled depending on whether there is text to clear.
        //menu.findItem(CLEAR_ID).setVisible(mEditor.getText().length() > 0);

        return true;
    }

    /**
     * Called when a menu item is selected.
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
        case BACK_ID:
        	SetDefaultWifiState();
            finish();
            break;
        case SETTINGS_ID:
        	Intent myIntent = new Intent(m_activity.getApplicationContext(), SettingsActivity.class);
    		startActivityForResult(myIntent, 0);
    		break;
        case SERVICEON_ID:
        	startLocalService();
        	break;
        case SERVICEOFF_ID:
        	doUnbindService();
        	stopLocalService();
        	break;
        case SERVICECONON_ID:
        	doBindService();      	
        	
        	//m_serviceConnection.BoundService.doSomething();
        	break;
        }
        

        return super.onOptionsItemSelected(item);
    }

    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener mBackListener = new OnClickListener() {
        public void onClick(View v) {
        	SetDefaultWifiState();
            finish();
        }
    };
    
    private void SetDefaultWifiState()
    {
    	Utilities.showStatus(m_txStatus, null, 0, "Set WIFI "+m_wifiInitialOnState);
    	Utilities.setWifiState(this, m_wifiInitialOnState);
    }
    
    
    
    OnClickListener alertsListenerCallback = new OnClickListener() {
		
		public void onClick(View v) {
			// TODO Auto-generated method stub
			ValueList vals = m_btAlertsSelection.GetSelections();
			
			vals.Add(Metadata.GlobalParams.command, GlobalCommands.dismissalert.toString());
    		//param.Add(Metadata.GlobalParams.alertindex, Integer.toString(_alarm.Index));
    		API.sendCommand(m_txStatus, m_progStatusBar, vals);
    		API.m_lastAlertTime = "";//force read
    		updateStatus();
		}
	};
    
    OnClickListener onClickServerPower = new OnClickListener() {
        public void onClick(View v) {
        	if (API.getLastServerStatus().IsServerOn)
        	{
        		Utilities.showStatus(m_txStatus, null, 0, "PC on sleep");
        		API.sendCommand(m_txStatus, m_progStatusBar, new ValueList(Metadata.GlobalParams.command, GlobalCommands.sleep.toString()));
        	}
        	else
        	{
        		Utilities.showStatus(m_txStatus, null, 0, "Wake PC");
        		API.WakeOnLan(v.getContext());
        	}
        	updateStatus();
        }
    };
    
    OnItemSelectedListener onItemSelectedSpinnerServer = new OnItemSelectedListener() {

		public void onItemSelected(AdapterView<?> arg0, View arg1, int arg2,
				long arg3) {
			// TODO Auto-generated method stub
			
			if (!m_ignoreSpinnerEvent)
			{
				API.setCurrentServerIndex(arg2);
				Metadata.Settings.setDefaultServerIndex(arg2);
				Metadata.Settings.Save(-1);
				API.getServerStatus(m_txStatus, m_progStatusBar);
				updateStatus();
			}
			else
				m_ignoreSpinnerEvent = false;
		}

		public void onNothingSelected(AdapterView<?> arg0) {
			// TODO Auto-generated method stub
			
		}
    	
	};
	
	public TextView getStatusTextView()
	{
		return m_txStatus;
	}
	///called on periodic pooling
    public void updateStatus()
    {
    	if (Utilities.isServiceRunning(getApplicationContext()))
        	m_progStatusBar.setBackgroundColor(Color.GREEN);
        else
        	m_progStatusBar.setBackgroundColor(Color.RED);
    	
    	/*
    	if (BoundService == null)
    		Utilities.showStatus(m_txStatus, null, 0, "notsvcconnected");
    	else
    		Utilities.showStatus(m_txStatus, null, 0, "svcconnected");
    	*/
    	//if (BoundService!= null && BoundService.getLocalServiceThread().hasNewData())
    	//{
	    	//m_ServerPower.setPressed(srvStatus.IsServerOn);
	    	ServerStatus srvStatus = API.getLastServerStatus();
	    	
	    	if (srvStatus.IsServerOn)
	    	{
	    		m_ServerPower.setBackgroundColor(Color.GREEN);
	    		//DisplayStatus("ZoneCount="+srvStatus.ZoneDetails.length);
	    	}
	    	else
	    	{
	    		m_ServerPower.setBackgroundColor(Color.RED);
	    		//m_txPowerStatus.setText("Server Off");
	    	}
	    	m_zoneAdapter.updateData(srvStatus);
	    	m_zoneAdapter.notifyDataSetChanged();
	    	
	    	if (API.m_hasNewAlert)
	    	{
	    		m_btAlertsSelection.SetOption(API.m_alertValues, API.m_alertIds);
	    		API.m_hasNewAlert = false;
	    	}
	    	
	    	//m_serverStatus = srvStatus;
	    	if (MusicActivity.m_instance != null)
	    		MusicActivity.m_instance.UpdateStatus();
	    	//BoundService.getLocalServiceThread().consumeData();
    	//}
    	//else
    	//	Log.i("main", "no new data");
    	
    }
    
    
	/**
     * A call-back for when the user presses a button.
     */
    OnClickListener onStatusRefresh = new OnClickListener() {
        public void onClick(View v) {
        	Log.i("main", "Refreshing status");
        	//API.GetServerStatus(m_txStatus, m_progStatusBar);
        	updateStatus();
        }
    };
	
    /**
     * A call-back for when the user presses a button.
     */
    OnClickListener OnClickMusic = new OnClickListener() {
        public void onClick(View v) {
        	m_currentCommand = Metadata.GlobalCommands.music;
        	m_btMusic.setSelected(true);
        	Utilities.showStatus(m_txStatus, null, 0, "Music selected");
        }
    };
    
    
    OnClickListener OnClickVideo = new OnClickListener() {
        public void onClick(View v) {
        	m_currentCommand = Metadata.GlobalCommands.video;
        	//m_btMusic.setPressed(false);
        	m_btVideo.setSelected(true);
        	Utilities.showStatus(m_txStatus, null, 0, "Video selected");
        }
    };
    
    
    
    
    OnLongClickListener onClickZoneAlarm = new OnLongClickListener() {

		public boolean onLongClick(View v) {
			// TODO Auto-generated method stub
			int zoneId = (Integer)v.getTag();
			ValueList params = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(zoneId));
			params.Add(Metadata.GlobalParams.command,GlobalCommands.togglealert.toString());
			API.sendCommand(m_txStatus, m_progStatusBar, params);
			updateStatus();
			return true;
		}
    	
    };
    
    OnLongClickListener onClickZone = new OnLongClickListener() {
        public boolean onLongClick(View v) {
        	int zoneId = (Integer)v.getTag();
        	ZoneDetails zone = API.GetZone(zoneId, API.getLastServerStatus());
        	if (zone == null)
        	{
        		Utilities.showStatus(m_txStatus, null, 0, "Unexpected null zone");
        		
        	}
        	else
        	{
        		Metadata.GlobalCommands actType;
        		if (zone.ActivityType == Metadata.GlobalCommands.nul)
        		{
        			actType = m_currentCommand;
        			
        		}
        		else
        			actType = zone.ActivityType;
        		
        		if (actType != null)
        		{
	        		switch (actType) {
	        		case music:
						startMusic(v.getContext(), zoneId);
						ValueList params = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(zoneId));
	        			params.Add(Metadata.GlobalParams.command,GlobalCommands.selectzone.toString());
	        			params.Add(Metadata.GlobalParams.activity,actType.toString());
	        			CommandResult res = API.sendCommand(m_txStatus, m_progStatusBar, params);
	        			if (!res.Result.equals(Metadata.ResultEnum.OK))
	        			{
	        				Utilities.showStatus(m_txStatus, null, 0, "Error opening zone: " + res.ErrorMessage);
	        			}
						
						break;
					default:
						Utilities.showPopupMessage("Unknown type: " + actType);
						break;
					} 
        		}
        		else
        			Utilities.showPopupMessage("No action selected ");
        	}
        	
        	updateStatus();
        	return true;
        }
		
    };
    
    private void startMusic(Context ctx, int zoneId)
    {
    	Intent myIntent = new Intent(ctx, MusicActivity.class);
		myIntent.putExtra("zoneid", zoneId);
		//myIntent.putExtra("serverindex", m_currentServerIndex);
		startActivityForResult(myIntent, 0);
    }
    
    
    
}

