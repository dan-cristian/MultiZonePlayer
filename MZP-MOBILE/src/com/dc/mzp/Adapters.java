package com.dc.mzp;

import android.app.Activity;
import android.content.Context;
import android.graphics.Color;
import android.graphics.PorterDuff.Mode;
import android.view.View;
import android.view.View.OnLongClickListener;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TableRow;
import android.widget.TextView;


	
	class ZoneAdapter extends BaseAdapter
	{
		/**
         * Remember our context so we can use it when constructing views.
         */
        private Context mContext;
		private ServerStatus m_srvStatus = new ServerStatus();
		private OnLongClickListener m_onButtonClick, m_onAlarmButtonClick;
		
		public ZoneAdapter(Context context, OnLongClickListener onButtonClick, OnLongClickListener onAlarmButtonClick)
		{
			mContext = context;
			m_onButtonClick = onButtonClick;
			m_onAlarmButtonClick = onAlarmButtonClick;
		}
		
		public void updateData(ServerStatus srvStatus)
		{
			m_srvStatus = srvStatus;
		}
		
		public int getCount() {
			// TODO Auto-generated method stub
			if (m_srvStatus.ZoneDetails != null)
				return m_srvStatus.ZoneDetails.length;
			else
				return 0;
		}

		public Object getItem(int position) {
			// TODO Auto-generated method stub
			return m_srvStatus.ZoneDetails[position];
		}

		public long getItemId(int position) {
			// TODO Auto-generated method stub
			return m_srvStatus.ZoneDetails[position].ZoneId;
		}

		public View getView(int position, View convertView, ViewGroup parent) {
			ZoneView sv;
            if (convertView == null) {
                sv = new ZoneView(mContext, m_srvStatus.ZoneDetails[position], m_onButtonClick, m_onAlarmButtonClick);
            } else {
                sv = (ZoneView) convertView;
                sv.setZoneDetails(m_srvStatus.ZoneDetails[position]);
                
            }

            return sv;
		}
		
	}
	
	/**
     * We will use a SpeechView to display each speech. It's just a LinearLayout
     * with two text fields.
     *
     */
     class ZoneView extends LinearLayout {
    	 private Button mBtZoneName;
    	 private Button m_btAlarmStatus;
    	 //private TextView mTxActivity;
         //private TextView mTxStatus;
         private TextView mTxAction;
         
        public ZoneView(Context context, ZoneDetails zone, OnLongClickListener onButtonLongClick, OnLongClickListener onAlarmButtonLongClick) {
            super(context);

            this.setOrientation(HORIZONTAL);

            // Here we build the child views in code. They could also have
            // been specified in an XML file.

            mBtZoneName = new Button(context);
            mBtZoneName.setOnLongClickListener(onButtonLongClick);
            m_btAlarmStatus = new Button(context);
            m_btAlarmStatus.setText("Alert ?");
            m_btAlarmStatus.setOnLongClickListener(onAlarmButtonLongClick);
            //mTxStatus = new TextView(context);
            //mTxActivity = new TextView(context);
            mTxAction = new TextView(context);
            setZoneDetails(zone);
            
            //list option
            /*
            addView(mBtZoneName, new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
            addView(m_btAlarmStatus, new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
            addView(mTxStatus, new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
            addView(mTxActivity, new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
            addView(mTxAction, new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
            */
            //table option
            
            addView(mBtZoneName, new TableRow.LayoutParams(90, LayoutParams.WRAP_CONTENT));
            addView(m_btAlarmStatus, new TableRow.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
            //addView(mTxStatus, new TableRow.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
            //addView(mTxActivity, new TableRow.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
            addView(mTxAction, new TableRow.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
            
        }
        
        /**
         * Convenience method to set the title of a SpeechView
         */
        public void setZoneDetails(ZoneDetails zone) {
        	mBtZoneName.setText(zone.ZoneName);
            mBtZoneName.setTag(zone.ZoneId);
            m_btAlarmStatus.setTag(zone.ZoneId);
            
            if (zone.CameraAlertActive != null)
            {
            	CharSequence text;
            	if (zone.CameraAlertActive)
            		text = "Alert On";
            	else
            		text = "Alert Off";
            	m_btAlarmStatus.setText(text);
            }
            switch (zone.ZoneState)
            {
            	case NotStarted:
            		mBtZoneName.getBackground().setColorFilter(Color.RED, Mode.LIGHTEN);
            		//mTxStatus.setText("notstart");
            		break;
            	case Running:
            		mBtZoneName.getBackground().setColorFilter(Color.GREEN, Mode.LIGHTEN);
            		//mTxStatus.setText("run");
            		break;
            	case Paused:
            		mBtZoneName.getBackground().setColorFilter(Color.YELLOW, Mode.LIGHTEN);
            		//mTxStatus.setText("pause");
            		break;
            	default:
            		mBtZoneName.getBackground().setColorFilter(null);
            		//mTxStatus.setText("");
            		break;
            }
            
            
            /*if (zone.ActivityType != Metadata.GlobalCommands.nul)
            	mTxActivity.setText(zone.ActivityType.toString() + " ");
            else
            	mTxActivity.setText("");
            */
            switch (zone.ActivityType) {
			case music:
				mTxAction.setText(zone.Title);
				break;

			default:
				mTxAction.setText("");
				break;
			}
        }
    }

     class MySpinnerAdapter
     {
     	public static void AddListAdapter(Spinner s, Activity act, CharSequence[] values)
     	{
     		ArrayAdapter adapter =new ArrayAdapter(act, android.R.layout.simple_spinner_item, values); 
     				//ArrayAdapter.createFromResource(act, R.array.planets, android.R.layout.simple_spinner_item);
     		
     	    //adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
     		adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
     	    s.setAdapter(adapter);
     	}
     }
