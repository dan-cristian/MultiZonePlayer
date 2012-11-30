package com.dc.mzp;

import java.util.List;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

public class MultipleSelectionButton extends View{
	
	protected CharSequence[] _options= { "No options" };
	protected int[] _optionIDs = null;
	protected int _singleSelection=0;
	protected boolean[] _selections = new boolean[1];
	protected Button m_sourceButton;
	protected Activity m_parentActivity;
	protected CharSequence _title;
	protected OnClickListener _callback;
	protected boolean _multiChoice;
	protected AlertDialog m_dialog;
	/*
	public MultipleSelectionButton()
	{
		super(m_parentActivity); 
	}*/
	
	public MultipleSelectionButton(Button source, CharSequence title, Activity parentActivity, OnClickListener listenerCallback, boolean multiChoice)
	{
		super(parentActivity);
		
		m_sourceButton = source;
		m_parentActivity = parentActivity;
		_title = title;
		_callback = listenerCallback;
		_multiChoice = multiChoice;
		m_sourceButton.setOnClickListener( new ButtonClickHandler()  );
		
	}
	
	public class ButtonClickHandler implements View.OnClickListener {
		public void onClick( View view ) {
			showDialog(_multiChoice);
		}
	}
	
	public void SetOption(List<String> values)
	{
		PopulateOptions(values, null);
		_selections =  new boolean[ values.size() ];
	}
	
	public void SetOption(List<String> values, List<String> ids)
	{
		PopulateOptions(values, ids);
		_selections =  new boolean[ values.size() ];
	}
	
	private void PopulateOptions(List<String> values, List<String> ids)
	{
		_options = new CharSequence[values.size()];
		if (ids != null)
			_optionIDs = new int[ids.size()];
		else
			_optionIDs = null;
		for (int i=0; i<values.size();i++)
		{
			_options[i] = values.get(i);
			if (ids != null)
				_optionIDs[i] = Integer.parseInt(ids.get(i));
		}
		
		m_sourceButton.setText(_title + " ("+values.size()+")");
	}
	
	public CharSequence[] GetOptions()
	{
		return _options;
	}
    
	private void showDialog(boolean multiChoice)
	{
		
		
		DialogButtonClickHandler onClick = new DialogButtonClickHandler();
		
		if (multiChoice)
		{
			m_dialog = new AlertDialog.Builder( m_parentActivity )
    		.setTitle( _title )
    		.setMultiChoiceItems( _options, _selections, new DialogSelectionClickHandler() )
    		.setPositiveButton( "OK", onClick )
    		.setNeutralButton("All", onClick )
    		.setNeutralButton("Search", onClick)
    		.setNegativeButton("Close", onClick )
    		.create();
		}
		else
		{
			m_dialog = new AlertDialog.Builder( m_parentActivity )
    		.setTitle( _title )
    		.setSingleChoiceItems(_options, _singleSelection, new DialogOnClickHandler() )
    		.setPositiveButton( "OK", onClick )
    		//.setNeutralButton("All", new DialogButtonClickHandler() )
    		.setNeutralButton("Search", onClick)
    		.setNegativeButton("Close", onClick)    		
    		.create();
		}
		
		
		
		/*
		for( int i = 0; i < _options.length; i++ ){
			if ( _options[ i ] + " selected: " + _selections[i] );
		}
		*/
		///EditText txt = new EditText(getContext());
	    ///txt.setOnClickListener(new SearchClick());
	    //txt.setFocusable(true);
		
		m_dialog.getListView().setTextFilterEnabled(true);
		//m_dialog.getListView().addHeaderView(txt);
		m_dialog.getListView().setFastScrollEnabled(true);
		m_dialog.getListView().setFocusable(true);
		m_dialog.show();
	}
	
	public class SearchClick implements OnClickListener
	{

		public void onClick(View v) {
			// TODO Auto-generated method stub
			InputMethodManager keyboard = (InputMethodManager)m_parentActivity.getSystemService(Context.INPUT_METHOD_SERVICE);
		    boolean res = keyboard.showSoftInput(v, InputMethodManager.SHOW_FORCED);
		    
		    
		}
		
	}
	
	private void selectAll()
	{
		for( int i = 0; i < _options.length; i++ ){
			_selections[i]=true;
		}
	}
	
	public ValueList GetSelections()
	{
		ValueList res = new ValueList();
		if (_multiChoice)
		{
			CharSequence[] result = new CharSequence[_options.length];
			int[] ids = new int[_options.length];
			int r=0;
			for( int i = 0; i < _options.length; i++ ){
				if (_selections[i])
				{
					result[r] = _options[i];
					if (_optionIDs != null)
						ids[r] = _optionIDs[i];
					r++;
				}
			}
			//CharSequence buttonTitle = _title;
			
			
			for (int i=0;i<r;i++)
			{
				
					res.AddIndexPair(Integer.toString(ids[i]), result[i].toString());
				//if (i<5)
				//	buttonTitle = TextUtils.concat(buttonTitle, "-", result[i]);
			}
			
			//m_sourceButton.setText(_title + " ("+res.Values.size()+")");
		}
		else
		{
			res.Add(Metadata.GlobalParams.selectedindex, Integer.toString(_singleSelection));//, _options[_singleSelection].toString());
			//m_sourceButton.setText(_title+":"+_options[_singleSelection]);
		}
		return res;
	}
	
	public class DialogOnClickHandler implements DialogInterface.OnClickListener
	{
		public void onClick( DialogInterface dialog, int clicked )
		{
			_singleSelection = clicked;
		}
	}
	
	public class DialogSelectionClickHandler implements DialogInterface.OnMultiChoiceClickListener
	{
		public void onClick( DialogInterface dialog, int clicked, boolean selected )
		{
			_selections[clicked] = selected;
			
			
			
			// TODO Auto-generated method stub
			InputMethodManager imm = (InputMethodManager) m_parentActivity.getSystemService(Context.INPUT_METHOD_SERVICE);
			boolean res = imm.showSoftInput(m_dialog.getListView(), InputMethodManager.SHOW_FORCED);
			
			Toast.makeText(m_parentActivity.getApplicationContext(), "Keyboard Now:"+res, Toast.LENGTH_LONG).show();
		    
			//m_parentActivity.getWindow().setSoftInputMode(WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE);
		}
		
		
	}

	public class DialogButtonClickHandler implements DialogInterface.OnClickListener
	{
		public void onClick( DialogInterface dialog, int clicked )
		{
			switch( clicked )
			{
				case DialogInterface.BUTTON_POSITIVE:
					_callback.onClick(m_sourceButton);
					break;
				case DialogInterface.BUTTON_NEUTRAL:
					selectAll();
					_callback.onClick(m_sourceButton);
					//m_parentActivity.sho
					
					break;
				case DialogInterface.BUTTON_NEGATIVE:
					break;
			}
			
		}
	}
	
	/*
	protected void printSelectedPlanets(){
		for( int i = 0; i < _options.length; i++ ){
			Log.i( "ME", _options[ i ] + " selected: " + _selections[i] );
		}
	}*/
	
}

class DialogWithInputBox  {
	/** Called when the activity is first created. */
	public AlertDialog.Builder alert;
	public EditText input;
	
	public DialogWithInputBox(Context p_activity, DialogInterface.OnClickListener p_listener, int p_inputType, CharSequence p_hint) {

		alert = new AlertDialog.Builder(p_activity);
		input = new EditText(p_activity);
		input.setInputType(p_inputType);
		input.setHint(p_hint);
		alert.setView(input);
		alert.setPositiveButton("Ok", p_listener);
		alert.setNeutralButton("Remove", p_listener);
		alert.setNegativeButton("Close",p_listener);
		alert.show();
	}
}

class DialogYesNo{
	AlertDialog.Builder _builder;
	DialogInterface.OnClickListener _callbackOnClick;
	public DialogYesNo(Context p_activity, CharSequence message, DialogInterface.OnClickListener callbackOnClick)
	{
		_callbackOnClick = callbackOnClick;
		_builder = new AlertDialog.Builder(p_activity);
		_builder.setMessage(message).setPositiveButton("Yes", callbackOnClick)
			.setNeutralButton("All", callbackOnClick)
		    .setNegativeButton("No", callbackOnClick).show();
	}

	public void InvokeNegativeAction()
	{
		_callbackOnClick.onClick((DialogInterface) _builder, DialogInterface.BUTTON_NEGATIVE);
	}
	//sample
	/*
	DialogInterface.OnClickListener dialogClickListener = new DialogInterface.OnClickListener() {
	    //@Override
	    public void onClick(DialogInterface dialog, int which) {
	        switch (which){
	        case DialogInterface.BUTTON_POSITIVE:
	            //Yes button clicked
	        	_callbackOnClick.onClick(dialog, which);
	            break;
	        case DialogInterface.BUTTON_NEGATIVE:
	            //No button clicked
	        	_callbackOnClick.onClick(dialog, which);
	            break;
	        }
	    }
	};*/
}