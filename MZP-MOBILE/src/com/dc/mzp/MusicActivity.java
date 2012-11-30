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

import java.util.List;

import android.app.Activity;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.text.InputType;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnTouchListener;
import android.view.animation.AnimationUtils;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageButton;
import android.widget.ProgressBar;
import android.widget.RatingBar;
import android.widget.TabHost;
import android.widget.TabHost.OnTabChangeListener;
import android.widget.ViewFlipper;
import android.widget.RatingBar.OnRatingBarChangeListener;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.TextView;

import com.dc.mzp.Metadata.GlobalCommands;
import com.dc.mzp.R;

/**
 * This class provides a basic demonstration of how to write an Android
 * activity. Inside of its window, it places a single view: an EditText that
 * displays and edits some internal text.
 */
public class MusicActivity extends Activity {
    
    SeekBar m_seekVolume;
    RatingBar m_ratingBar;
    TextView m_txSongName;
    TextView m_txAuthor;
    TextView m_txGenre, m_txAlbum, m_txYear, m_txPlaycount;
    private TabHost mTabHost;
    private TextView m_txStatus;
    private TextView m_txSleepMinutes;
    private TextView m_txWake;
    private MultipleSelectionButton m_genreSelectButton, m_artistSelectButton, m_playlistSelectButton, m_moodSelectButton;
    private Button m_genreBaseButton, m_artistBaseButton, m_playlistBaseButton, m_moodBaseButton, m_btSearch;
    private Integer m_zoneId;
    //private Integer m_serverIndex;
    private ImageButton m_btSleep, m_btWake;
    private DialogWithInputBox m_dlgInput;
    private ProgressBar m_progStatusBar;
    private boolean m_genreLoaded = false;
    private final String TAB_TAG_CONTROLS="tab_controls";
    private final String TAB_TAG_SELECTION="tab_selection";
    
    public static MusicActivity m_instance=null;
    
    public MusicActivity() {
    }

    /** Called with the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Inflate our UI from its XML layout description.
        setContentView(R.layout.music_activity);
        m_instance = this;
        
        m_zoneId = this.getIntent().getIntExtra("zoneid", 0);
        //m_serverIndex = this.getIntent().getIntExtra("serverindex", 0);
        
        m_seekVolume = (SeekBar) findViewById(R.id.seekVolume);
        m_seekVolume.setMax(-VolumeLevels.VolumeSilence);
        m_seekVolume.setOnSeekBarChangeListener(OnSeekVolume);
        
        m_ratingBar = (RatingBar) findViewById(R.id.ratingBar);
        m_ratingBar.setOnRatingBarChangeListener(onRatingBarChange);
        m_txSongName = (TextView)findViewById(R.id.txSongName);
        m_txAuthor = (TextView)findViewById(R.id.txAuthor);
        m_txGenre = (TextView)findViewById(R.id.txGenre);
        m_txAlbum = (TextView)findViewById(R.id.txAlbum);
        m_txYear = (TextView)findViewById(R.id.txYear);
        m_txPlaycount = (TextView)findViewById(R.id.txPlaycount);
        m_txSleepMinutes = (TextView)findViewById(R.id.txSleepMinutes);
        m_txWake = (TextView)findViewById(R.id.txWake);
        
        m_txStatus = (TextView)findViewById(R.id.txStatus);
        
        m_btSleep = ((ImageButton) findViewById(R.id.btSleep));
        m_btSleep.setOnClickListener(OnSleepClick);
        m_btWake = ((ImageButton) findViewById(R.id.btWake));
        m_btWake.setOnClickListener(OnWakeClick);
        m_btSearch = ((Button) findViewById(R.id.btSearch));
        m_btSearch.setOnClickListener(onSearchClick);
        
        m_genreBaseButton = (Button) findViewById(R.id.btGenre);
        m_genreSelectButton = new MultipleSelectionButton(m_genreBaseButton, "Genre", this, OnGenreResults, true);
        
        m_artistBaseButton = (Button) findViewById(R.id.btArtist);
        m_artistSelectButton = new MultipleSelectionButton(m_artistBaseButton, "Artist", this, OnArtistResults, true);
        
        m_moodBaseButton = (Button) findViewById(R.id.btMood);
        m_moodSelectButton = new MultipleSelectionButton(m_moodBaseButton, "Mood", this, onMoodResults, false);
        
        m_playlistBaseButton = (Button) findViewById(R.id.btPlayList);
        m_playlistSelectButton = new MultipleSelectionButton(m_playlistBaseButton, "Playlist", this, OnPlaylistResults, false);
        
        //m_drawer = (SlidingDrawer) this.findViewById(R.id.musicDrawer);
        //m_drawer.setOnDrawerOpenListener(onDrawerOpenListener);
        
        m_progStatusBar = (ProgressBar) this.findViewById(R.id.progressStatus);
        
        ((ImageButton) findViewById(R.id.btPlay)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btPause)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btStop)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btNext)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btPrev)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btRepeat)).setOnClickListener(OnClickCommand);
        ((ImageButton) findViewById(R.id.btMute)).setOnClickListener(OnClickCommand);
        
        mTabHost = (TabHost)findViewById(android.R.id.tabhost); 
        mTabHost.setup();
        
        mTabHost.addTab(mTabHost.newTabSpec(TAB_TAG_CONTROLS).setIndicator("Controls").setContent(R.id.tab1));
        mTabHost.addTab(mTabHost.newTabSpec(TAB_TAG_SELECTION).setIndicator("Selection").setContent(R.id.tab2));
        mTabHost.addTab(mTabHost.newTabSpec("3").setIndicator("Other").setContent(R.id.tab3));
        
        mTabHost.setCurrentTab(0);
        mTabHost.setOnTabChangedListener(onTabChangeListener);
        
        
    }

    /**
     * Called when the activity is about to start interacting with the user.
     */
    @Override
    protected void onResume() {
        super.onResume();
        Utilities.showPopupMessage("On Music Resume");
        UpdateStatus();
    }

    @Override
    protected void onSaveInstanceState(Bundle outState)
    {
    	Utilities.showPopupMessage("On Music save");
    	super.onSaveInstanceState(outState);
    }
    
    @Override
    protected void onRestoreInstanceState(Bundle state)
    {
    	super.onRestoreInstanceState(state);
    	Utilities.showPopupMessage("On Music restore");
    	
    }
    
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener mBtBackListener = new OnClickListener() {
        public void onClick(View v) {
        	m_instance = null;
        	Intent intent = new Intent();
            setResult(RESULT_OK, intent);
            finish();
        }
    };
    
    public int GetZoneId()
    {
    	return m_zoneId.intValue();
    }
    
    public void UpdateStatus()
    {
    	ServerStatus srv = API.getLastServerStatus();
    	ZoneDetails zone;
    	if (m_instance != null && srv.IsServerOn && srv.ZoneDetails != null)
    	{
    		zone = API.GetZone(m_instance.GetZoneId(), srv); 
			
			if (zone != null)
			{
				m_instance.m_txSongName.setText(zone.Title);
				m_instance.m_seekVolume.setProgress(zone.VolumeLevel - VolumeLevels.VolumeSilence);
				m_instance.m_ratingBar.setProgress(zone.Rating);
				m_instance.m_txAuthor.setText(zone.Author);
				m_instance.m_txGenre.setText(zone.Genre);
				m_instance.m_txAlbum.setText(zone.Album);
				m_instance.m_txYear.setText(zone.Year);
				m_instance.m_txPlaycount.setText(String.valueOf(zone.Playcount));
				if (zone.SleepHourMin.equals(""))
					m_instance.m_txSleepMinutes.setText("Sleep Off");
				else
					m_instance.m_txSleepMinutes.setText(zone.SleepHourMin);
				
				if (zone.WakeTime.equals(""))
					m_instance.m_txWake.setText("Wake Off");
				else
					m_instance.m_txWake.setText(zone.WakeTime);
			}
    	}
    }
    
    private void getMusicData()
    {
    	GetGenreList();
        GetArtistList();
        GetPlaylist();
        getMoodList();
        m_genreLoaded = true;
    }
        public void GetGenreList() {
        	try
        	{
        		ValueList param = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(m_zoneId));
        		param.Add(Metadata.GlobalParams.command, GlobalCommands.genrelist.toString());
        		CommandResult res = API.sendCommand(m_txStatus, m_progStatusBar, param);
        		
        		if (res.ValueList != null)
				{
					List<String> vals = res.ValueList.GetIndexValueList();
            		
            		if (!vals.equals(m_genreSelectButton.GetOptions()))
            		{
        	    		m_genreSelectButton = new MultipleSelectionButton(m_genreBaseButton, "Genre", this, OnGenreResults, true);
        	    		m_genreSelectButton.SetOption(res.ValueList.GetIndexValueList());
		            }
				}
        		
        		
        	}
        	catch (Exception e)
        	{
        		Log.i("Genre.OnClickCommand", e.getMessage());
        	}
        
    };
    
    public void GetArtistList() {
    	try
    	{
    		ValueList param = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(m_zoneId));
    		param.Add(Metadata.GlobalParams.command, GlobalCommands.artistlist.toString());
    		CommandResult res = API.sendCommand(m_txStatus, m_progStatusBar,  param);
    		if (res.ValueList != null)
			{
				List<String> vals = res.ValueList.GetIndexValueList();
        		
        		if (!vals.equals(m_artistSelectButton.GetOptions()))
        		{
    	    		m_artistSelectButton = new MultipleSelectionButton(m_artistBaseButton, "Artist", this, OnArtistResults, true);
    	    		m_artistSelectButton.SetOption(res.ValueList.GetIndexValueList());
        		}
        	}
    	}
    	catch (Exception e)
    	{
    		Log.i("Artist.OnClickCommand", e.getMessage());
    	}
    
    };
    
    public void GetPlaylist() {
    	try
    	{
    		ValueList param = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(m_zoneId));
    		param.Add(Metadata.GlobalParams.command, GlobalCommands.medialist.toString());
    		CommandResult res = API.sendCommand(m_txStatus, m_progStatusBar, param);
    		if (res.ValueList != null)
			{
				List<String> vals = res.ValueList.GetIndexValueList();
        		
        		if (!vals.equals(m_playlistSelectButton.GetOptions()))
        		{
    	    		m_playlistSelectButton = new MultipleSelectionButton(m_playlistBaseButton, "Playlist", this, OnPlaylistResults, false);
    	    		m_playlistSelectButton.SetOption(res.ValueList.GetIndexValueList(), res.ValueList.GetIndexList());
        		}
        	}
    	}
    	catch (Exception e)
    	{
    		Log.i("Artist.OnClickCommand", e.getMessage());
    	}
    
    };
    
    public void getMoodList(){
    	try
    	{
    		ValueList param = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(m_zoneId));
    		param.Add(Metadata.GlobalParams.command, GlobalCommands.getmoodmusiclist.toString());
    		CommandResult res = API.sendCommand(m_txStatus, m_progStatusBar, param);
    		if (res.ValueList != null)
			{
				List<String> vals = res.ValueList.GetIndexValueList();
        		
        		if (!vals.equals(m_moodSelectButton.GetOptions()))
        		{
    	    		//m_playlistSelectButton = new MultipleSelectionButton(m_playlistBaseButton, "Playlist", this, OnPlaylistResults, false);
    	    		m_moodSelectButton.SetOption(res.ValueList.GetIndexValueList(), res.ValueList.GetIndexList());
        		}
        	}
    	}
    	catch (Exception e)
    	{
    		Log.i("moodlist", e.getMessage());
    	}
    }
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnGenreResults = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		
        		
        		ValueList res = m_genreSelectButton.GetSelections();
        		res.Add(Metadata.GlobalParams.zoneid, m_zoneId.toString());
        		res.Add(Metadata.GlobalParams.command, GlobalCommands.setgenrelist.toString());
        		API.sendCommand(m_txStatus, m_progStatusBar, res);
        		
        	}
        	catch (Exception e)
        	{
        		Log.i("Genre.OnGenreResults", e.getMessage());
        	}
        	GetPlaylist();
        	UpdateStatus();
        }
    };
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnArtistResults = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		
        		
        		ValueList res = m_artistSelectButton.GetSelections();
        		res.Add(Metadata.GlobalParams.zoneid, m_zoneId.toString());
        		res.Add(Metadata.GlobalParams.command, GlobalCommands.setartistlist.toString());
        		API.sendCommand(m_txStatus, m_progStatusBar, res);
        		
        	}
        	catch (Exception e)
        	{
        		Log.i("Artist.OnResults", e.getMessage());
        	}
        	GetPlaylist();
        	UpdateStatus();
        }
    };
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnPlaylistResults = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		ValueList res = m_playlistSelectButton.GetSelections();
        		res.Add(Metadata.GlobalParams.zoneid, m_zoneId.toString());
        		res.Add(Metadata.GlobalParams.command, GlobalCommands.setmediaitem.toString());
        		API.sendCommand(m_txStatus, m_progStatusBar,  res);
        		
        	}
        	catch (Exception e)
        	{
        		Log.i("Playlist.OnResults", e.getMessage());
        	}
        	UpdateStatus();
        }
    };
    
    
    OnClickListener onMoodResults = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		ValueList res = m_moodSelectButton.GetSelections();
        		res.Add(Metadata.GlobalParams.zoneid, m_zoneId.toString());
        		res.Add(Metadata.GlobalParams.command, GlobalCommands.setmoodmusic.toString());
        		API.sendCommand(m_txStatus, m_progStatusBar,  res);
        		
        	}
        	catch (Exception e)
        	{
        		Log.i("Mood.OnResults", e.getMessage());
        	}
        	UpdateStatus();
        }
    };
    
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnSleepClick = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		m_dlgInput = new DialogWithInputBox(m_instance, OnSleepValueSelection, InputType.TYPE_CLASS_NUMBER, "minutes until sleep");
        	}
        	catch (Exception e)
        	{
        		Log.i("OnSleepClick", e.getMessage());
        	}
        }
    };
    
    
    OnClickListener OnWakeClick = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		m_dlgInput = new DialogWithInputBox(m_instance, OnWakeValueSelection, InputType.TYPE_DATETIME_VARIATION_TIME, "hh:mm for wakeup");
        	}
        	catch (Exception e)
        	{
        		Log.i("OnWakeClick", e.getMessage());
        	}
        }
    };
    

    OnClickListener onSearchClick = new OnClickListener() {
        public void onClick(View v) {
        	try
        	{
        		m_dlgInput = new DialogWithInputBox(m_instance, onSearchValueSelection, InputType.TYPE_CLASS_TEXT, "search text");
        	}
        	catch (Exception e)
        	{
        		Log.i("OnsearchClick", e.getMessage());
        	}
        	
        }
    };
    
    DialogInterface.OnClickListener onSearchValueSelection = new DialogInterface.OnClickListener()     {
		public void onClick(DialogInterface dialog, int whichButton) 
		{
			String value = m_dlgInput.input.getText().toString().trim();
			ValueList res  = new ValueList(Metadata.GlobalParams.zoneid,m_zoneId.toString());
			switch (whichButton) {
			case DialogInterface.BUTTON_POSITIVE:
				
	    		res.Add(Metadata.GlobalParams.searchvalue, value);
	    		res.Add(Metadata.GlobalParams.command, GlobalCommands.searchmediaitem.toString());
	    		API.sendCommand(m_txStatus, m_progStatusBar, res);
	    		
				break;
			case DialogInterface.BUTTON_NEUTRAL:
				//clear search
				res.Add(Metadata.GlobalParams.command, GlobalCommands.searchmediaitem.toString());
				res.Add(Metadata.GlobalParams.searchvalue,"");
				API.sendCommand(m_txStatus, m_progStatusBar, res);
			default:
				break;
			}
			GetPlaylist();
        	UpdateStatus();
		}
	};
    
    DialogInterface.OnClickListener OnSleepValueSelection = new DialogInterface.OnClickListener()     {
		public void onClick(DialogInterface dialog, int whichButton) 
		{
			String value = m_dlgInput.input.getText().toString().trim();
			ValueList res  = new ValueList(Metadata.GlobalParams.zoneid,m_zoneId.toString());
			switch (whichButton) {
			case DialogInterface.BUTTON_POSITIVE:
				//enable timer
	    		res.Add(Metadata.GlobalParams.datetime, value);
	    		res.Add(Metadata.GlobalParams.command, GlobalCommands.sleeptimer.toString());
	    		API.sendCommand(m_txStatus, m_progStatusBar, res);
	    		
				break;
			case DialogInterface.BUTTON_NEUTRAL:
				//remove timer
				res.Add(Metadata.GlobalParams.command, GlobalCommands.sleeptimer.toString());
				res.Add(Metadata.GlobalParams.datetime,"-1");
				API.sendCommand(m_txStatus, m_progStatusBar, res);
			default:
				break;
			}
			UpdateStatus();
		}
	};
	
    
    
	DialogInterface.OnClickListener OnWakeValueSelection = new DialogInterface.OnClickListener() {
		public void onClick(DialogInterface dialog, int whichButton) {
			String value = m_dlgInput.input.getText().toString().trim();
			ValueList res  = new ValueList(Metadata.GlobalParams.zoneid,m_zoneId.toString());
			switch (whichButton) {
			case DialogInterface.BUTTON_POSITIVE:
				//enable timer
	    		res.Add(Metadata.GlobalParams.datetime, value);
	    		res.Add(Metadata.GlobalParams.command, GlobalCommands.setwaketimer.toString());
	    		API.sendCommand(m_txStatus, m_progStatusBar, res);
	    		
				break;
			case DialogInterface.BUTTON_NEUTRAL:
				//remove timer
				res.Add(Metadata.GlobalParams.command, GlobalCommands.setwaketimer.toString());
				res.Add(Metadata.GlobalParams.datetime, "");
				API.sendCommand(m_txStatus, m_progStatusBar, res);
			default:
				break;
			}
			UpdateStatus();
		}
	};
    /**
     * A call-back for when the user presses the back button.
     */
    OnClickListener OnClickCommand = new OnClickListener() {
        public void onClick(View v) {
        	String tag = v.getTag().toString();
        	try
        	{
        		Metadata.GlobalCommands cmd = Metadata.GlobalCommands.valueOf(tag);
        		ValueList param = new ValueList(Metadata.GlobalParams.zoneid, Integer.toString(m_zoneId));
        		param.Add(Metadata.GlobalParams.command, cmd.toString());
        		API.sendCommand(m_txStatus, m_progStatusBar, param);
        	}
        	catch (Exception e)
        	{
        		Log.i("Music.OnClickCommand"+tag, e.getMessage());
        	}
        	UpdateStatus();
        }
    };
    
    OnSeekBarChangeListener OnSeekVolume = new OnSeekBarChangeListener() {
		
		public void onStopTrackingTouch(SeekBar seekBar) {
			// TODO Auto-generated method stub
			int volume = VolumeLevels.VolumeSilence + seekBar.getProgress();
			ValueList params = new ValueList(Metadata.GlobalParams.zoneid,m_zoneId.toString());
			params.Add(Metadata.GlobalParams.volumelevel, Integer.toString(volume));
			params.Add(Metadata.GlobalParams.command, GlobalCommands.volumeset.toString());
			API.sendCommand(m_txStatus, m_progStatusBar, params);
			UpdateStatus();
		}
		
		public void onStartTrackingTouch(SeekBar seekBar) {
			// TODO Auto-generated method stub
		}
		
		public void onProgressChanged(SeekBar seekBar, int progress,
				boolean fromUser) {
			// TODO Auto-generated method stub
			if (fromUser)
			{
			}
		}
	};
	
	OnRatingBarChangeListener onRatingBarChange = new OnRatingBarChangeListener() {
		
		public void onRatingChanged(RatingBar ratingBar, float rating,
				boolean fromUser) {
			// TODO Auto-generated method stub
			if (fromUser)
			{
				int intRating = Math.round(rating);
				ValueList params = new ValueList(Metadata.GlobalParams.zoneid,m_zoneId.toString());
				params.Add(Metadata.GlobalParams.ratingvalue, Integer.toString(intRating));
				params.Add(Metadata.GlobalParams.command, GlobalCommands.ratingset.toString());
				API.sendCommand(m_txStatus, m_progStatusBar, params);
				UpdateStatus();
			}
		}
	};
    
	OnTabChangeListener onTabChangeListener = new OnTabChangeListener() {
		
		public void onTabChanged(String tabId) {
			// TODO Auto-generated method stub
			if (tabId.equals(TAB_TAG_SELECTION))
			{
				if (!m_genreLoaded)
					getMusicData();
			}
		}
	};

	
}
