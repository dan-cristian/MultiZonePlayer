package com.dc.mzp;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.GZIPInputStream;

import org.apache.http.Header;
import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.params.HttpParams;
import org.json.JSONObject;

import com.google.gson.Gson;

import android.util.Log;
import android.widget.ProgressBar;
import android.widget.TextView;


public class HttpClient {
 private static final String TAG = "HttpClient";
 
 public static HttpResult SendHttpGet(TextView txStatus, ProgressBar progBar, String URL, ValueList... vals)
 {
	 HttpResult result = new HttpResult();
	 try {
		 	String cmdName = vals[0].GetValue(Metadata.GlobalParams.command);
		 
		   HttpParams httpParameters = new BasicHttpParams();
		   // Set the timeout in milliseconds until a connection is established.
		   int timeoutConnection = 5000;
		   HttpConnectionParams.setConnectionTimeout(httpParameters, timeoutConnection);
		   
		   // Set the default socket timeout (SO_TIMEOUT) 
		   // in milliseconds which is the timeout for waiting for data.
		   int timeoutSocket = 5000;
		   HttpConnectionParams.setSoTimeout(httpParameters, timeoutSocket);
		   
		   DefaultHttpClient httpclient = new DefaultHttpClient();
		   httpclient.setParams(httpParameters);
		   
		   HttpResponse response;
		   
		   /*if (false)
		   {
			   HttpGet httpGetRequest = new HttpGet(URL+"/?command="+cmdName+"&"+vals[0].GetValue(Metadata.GlobalParams.command));
			   response = (HttpResponse) httpclient.execute(httpGetRequest);
		   }
		   else
		   {*/
			   HttpPost httpPostRequest = new HttpPost(URL+"?command="+cmdName);
			   List<NameValuePair> nameValuePairs = new ArrayList<NameValuePair>(1);
			   String json =  new Gson().toJson(vals);
			   json = json.replaceAll(" ","%20");
			   nameValuePairs.add(new BasicNameValuePair("valuelist", json));
			   httpPostRequest.setEntity(new UrlEncodedFormEntity(nameValuePairs));
			   Utilities.showStatus(txStatus, progBar, 50, "Go");
			   response = (HttpResponse) httpclient.execute(httpPostRequest);
		   //}
		   
		   Utilities.showStatus(txStatus, progBar, 75, "K");
		   // Get hold of the response entity (-> the data):
		   HttpEntity entity = response.getEntity();
		   
		   if (entity != null) {
			    // Read the content stream
			    InputStream instream = entity.getContent();
			    Header contentEncoding = response.getFirstHeader("Content-Encoding");
			    if (contentEncoding != null && contentEncoding.getValue().equalsIgnoreCase("gzip")) {
			     instream = new GZIPInputStream(instream);
			    }

			    // convert content stream to a String
			    result.HttpResponse = convertStreamToString(instream);
			    instream.close();
			    //resultString = resultString.substring(,resultString.length()-1); // remove wrapping "[" and "]"
			    //Log.i(TAG, resultString);
			    //Toast.makeText(ctx, "OK", Toast.LENGTH_LONG).show();
		   }
		   
	 }
	 catch (Exception e)
	  {
		 Utilities.showStatus(txStatus, progBar, 75, "Er2 "+e.toString()+"|"+ e.getMessage());
	   // More about HTTP exception handling in another tutorial.
	   // For now we just print the stack trace.
		 //Toast.makeText(MainActivity.m_activity.getApplicationContext(), e.getMessage(), Toast.LENGTH_LONG).show();
		 Log.i("httpclient", e.getMessage());
		 result.HttpError = e.getMessage();
		 //e.printStackTrace();
	  }
	 return result;
 }
 
 
 public static JSONObject SendHttpPost(String URL, JSONObject jsonObjSend) {

  try {
   DefaultHttpClient httpclient = new DefaultHttpClient();
   HttpPost httpPostRequest = new HttpPost(URL);

   StringEntity se;
   se = new StringEntity(jsonObjSend.toString());

   // Set HTTP parameters
   httpPostRequest.setEntity(se);
   httpPostRequest.setHeader("Accept", "application/json");
   httpPostRequest.setHeader("Content-type", "application/json");
   //httpPostRequest.setHeader("Accept-Encoding", "gzip"); // only set this parameter if you would like to use gzip compression

   long t = System.currentTimeMillis();
   HttpResponse response = (HttpResponse) httpclient.execute(httpPostRequest);
   Log.i(TAG, "HTTPResponse received in [" + (System.currentTimeMillis()-t) + "ms]");

   // Get hold of the response entity (-> the data):
   HttpEntity entity = response.getEntity();

   if (entity != null) {
    // Read the content stream
    InputStream instream = entity.getContent();
    Header contentEncoding = response.getFirstHeader("Content-Encoding");
    if (contentEncoding != null && contentEncoding.getValue().equalsIgnoreCase("gzip")) {
     instream = new GZIPInputStream(instream);
    }

    // convert content stream to a String
    String resultString= convertStreamToString(instream);
    instream.close();
    resultString = resultString.substring(1,resultString.length()-1); // remove wrapping "[" and "]"

    Log.i(TAG, resultString);
    // Transform the String into a JSONObject
    //JSONObject jsonObjRecv = new JSONObject(resultString);
    // Raw DEBUG output of our received JSON object:
    //Log.i(TAG,"<jsonobject>\n"+jsonObjRecv.toString()+"\n</jsonobject>");

    return null;//jsonObjRecv;
   } 

  }
  catch (Exception e)
  {
   // More about HTTP exception handling in another tutorial.
   // For now we just print the stack trace.
   e.printStackTrace();
  }
  return null;
 }


 private static String convertStreamToString(InputStream is) {
  /*
   * To convert the InputStream to String we use the BufferedReader.readLine()
   * method. We iterate until the BufferedReader return null which means
   * there's no more data to read. Each line will appended to a StringBuilder
   * and returned as String.
   * 
   * (c) public domain: http://senior.ceng.metu.edu.tr/2009/praeda/2009/01/11/a-simple-restful-client-at-android/
   */
  BufferedReader reader = new BufferedReader(new InputStreamReader(is));
  StringBuilder sb = new StringBuilder();

  String line = null;
  try {
   while ((line = reader.readLine()) != null) {
    sb.append(line + "\n");
   }
  } catch (IOException e) {
   e.printStackTrace();
  } finally {
   try {
    is.close();
   } catch (IOException e) {
    e.printStackTrace();
   }
  }
  return sb.toString();
 }

}

class HttpResult
{
	 String HttpResponse=null;
	 String HttpError=null;
	 
	 public HttpResult()
	 {
		 
	 }
}