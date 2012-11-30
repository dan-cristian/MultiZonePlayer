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

import com.dc.mzp.R;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TabHost;
import android.widget.ViewFlipper;

/**
 * This class provides a basic demonstration of how to write an Android
 * activity. Inside of its window, it places a single view: an EditText that
 * displays and edits some internal text.
 */
public class SettingsActivity extends Activity {
    
    private EditText m_txServerIP;
    private EditText m_txServerURL;
    private EditText m_txServerMAC;
    private EditText m_txServerWOL;
    private EditText m_txPoolWifi;
    private EditText m_txPool3G;
    private Spinner m_spinnerServer;
    private ViewFlipper m_vf1;
    private int m_currentIndex = 0;
    private TabHost mTabHost;
    
    public SettingsActivity() {
    }

    /** Called with the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Inflate our UI from its XML layout description.
        setContentView(R.layout.settings_activity);
        ((Button) findViewById(R.id.btBack)).setOnClickListener(OnClickBack);
        ((Button) findViewById(R.id.btOK)).setOnClickListener(OnClickOK);
        ((Button) findViewById(R.id.btAdd)).setOnClickListener(OnClickAdd);
        ((Button) findViewById(R.id.btRemove)).setOnClickListener(OnClickRemove);
        ((Button) findViewById(R.id.btNext)).setOnClickListener(onClickViewChange);
        
        m_txServerIP = (EditText)findViewById(R.id.txServerIP);
        m_txServerURL = (EditText)findViewById(R.id.txServerURL);
        m_txServerMAC = (EditText)findViewById(R.id.txServerMAC);
        m_txServerWOL = (EditText)findViewById(R.id.txServerWOL);
        m_spinnerServer = (Spinner)findViewById(R.id.spinnerServers);
        m_txPoolWifi = (EditText)findViewById(R.id.txPoolIntervalWifi);
        m_txPool3G = (EditText)findViewById(R.id.txPoolInterval3G);
        
        m_spinnerServer.setOnItemSelectedListener(OnItemSelectedSpinnerServer);
        MySpinnerAdapter.AddListAdapter(m_spinnerServer, this, Metadata.Settings.GetServerIPList());
        
        mTabHost = (TabHost)findViewById(android.R.id.tabhost); 
        mTabHost.setup();
        
        mTabHost.addTab(mTabHost.newTabSpec("Tab 1").setIndicator("Servers").setContent(R.id.tab1));
        mTabHost.addTab(mTabHost.newTabSpec("Tab 2").setIndicator("Others").setContent(R.id.tab2));
        mTabHost.addTab(mTabHost.newTabSpec("Tab 3").setIndicator("TAB 3").setContent(R.id.tab3));
        
        mTabHost.setCurrentTab(0);
        
        SelectServer(0);
    }

    /**
     * Called when the activity is about to start interacting with the user.
     */
    @Override
    protected void onResume() {
        super.onResume();
    }

    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnClickBack = new OnClickListener() {
        public void onClick(View v) {
        	Intent intent = new Intent();
            setResult(RESULT_OK, intent);
            finish();
        }
    };
    
    OnItemSelectedListener OnItemSelectedSpinnerServer = new OnItemSelectedListener() {

		public void onItemSelected(AdapterView<?> arg0, View arg1, int arg2,
				long arg3) {
			// TODO Auto-generated method stub
			SelectServer(arg2);
		}

		public void onNothingSelected(AdapterView<?> arg0) {
			// TODO Auto-generated method stub
			
		}
    	
	};
	
    private void SelectServer(int index)
    {
    	m_txServerIP.setText(Metadata.Settings.GetServerIP(index));
        m_txServerURL.setText(Metadata.Settings.GetServerURL(index));
        m_txServerMAC.setText(Metadata.Settings.GetServerMAC(index));
        m_txServerWOL.setText(Metadata.Settings.GetServerIPWOL(index));
        m_txPool3G.setText(String.valueOf(Metadata.Settings.getPoolInterval3G()));
        m_txPoolWifi.setText(String.valueOf(Metadata.Settings.getPoolIntervalWifi()));
        m_currentIndex = index;
    }
    
    private void SaveServer(int index)
    {
    	Metadata.Settings.SetServerIP(index, m_txServerIP.getText());
    	Metadata.Settings.SetServerURL(index, m_txServerURL.getText());
    	Metadata.Settings.SetServerMAC(index, m_txServerMAC.getText());
    	Metadata.Settings.SetServerIPWOL(index, m_txServerWOL.getText());
    	Metadata.Settings.setPoolInterval3G(Integer.parseInt(m_txPool3G.getText().toString()));
    	Metadata.Settings.setPoolIntervalWifi(Integer.parseInt(m_txPoolWifi.getText().toString()));
    	Metadata.Settings.Save(index);
    }
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnClickOK = new OnClickListener() {
        public void onClick(View v) {
        	
        	SaveServer(m_currentIndex);
        	
        	Intent intent = new Intent();
            setResult(RESULT_OK, intent);
            finish();
        }
    };
    
    OnClickListener OnClickAdd = new OnClickListener() {
        public void onClick(View v) {
        	Metadata.Settings.Add();
        	SelectServer(Metadata.Settings.getServerCount()-1);
        }
    };
    
    OnClickListener OnClickRemove = new OnClickListener() {
        public void onClick(View v) {
        	
        	SaveServer(m_currentIndex);
        	
        	Intent intent = new Intent();
            setResult(RESULT_OK, intent);
            finish();
        }
    };
    
    OnClickListener onClickViewChange = new OnClickListener() {
        public void onClick(View v) {
        	m_vf1.showNext();
        	
        }
    };
}
