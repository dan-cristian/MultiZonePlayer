using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SQLite;

namespace MultiZonePlayer {
    public class DB {
        protected static SQLiteConnection m_db = null;
        public const string TABLE_TEMPERATURE="temperature";
        public static string COL_TEMPERATURE_DATETIME = "datetime";
        public static string COL_TEMPERATURE_ZONEID = "zoneid";
        public static string COL_TEMPERATURE_VALUE = "value";
        public static string COL_TEMPERATURE_SENSORPOSITION = "sensorposition";
        public static string COL_TEMPERATURE_SENSORID = "sensorid";
        public static string COL_TEMPERATURE_SENSORNAME = "sensorname";

        public const string TABLE_HUMIDITY = "humidity";
        public static string COL_HUMIDITY_DATETIME = "datetime";
        public static string COL_HUMIDITY_ZONEID = "zoneid";
        public static string COL_HUMIDITY_VALUE = "value";

        public const string TABLE_COUNTER = "counter";
        public static string COL_COUNTER_DATETIME = "datetime";
        public static string COL_COUNTER_ZONEID = "zoneid";
        public static string COL_COUNTER_MAINUNIT = "mainunit";
        public static string COL_COUNTER_SECONDARYUNIT = "secondaryunit";
        public static string COL_COUNTER_COST = "cost";
        public static string COL_COUNTER_TOTALMAINUNIT = "totalmainunit";
        public static string COL_COUNTER_TOTALCOUNTER = "totalcounter";
        public static string COL_COUNTER_UTILITYTYPE = "utilitytype";

        public static Object m_lock = new Object();

        protected static void GetDatabase() {
            if (m_db==null) {
                //DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
                //m_db = fact.CreateConnection();
                //m_db.ConnectionString = "Data Source=c:\\sqlite\\Sensors.db3";
                m_db = new SQLiteConnection("Data Source=c:\\sqlite\\Sensors.db3;Version=3;New=False;Compress=True;");
            }
            //if (m_db.State != System.Data.ConnectionState.Open) {
            //    m_db.Open();
            //}
        }
        public static void WriteRecord(string tablename, params Object[] fields) {
            if (fields == null) return;
            lock (m_lock) {
                DB.GetDatabase();
                SQLiteTransaction trans;
                string fieldnamelist = "", fieldparamlist = "";
                for (int i = 0; i < fields.Length; i = i + 2) {
                    fieldnamelist += fields[i] + ",";
                    fieldparamlist += "@" + fields[i] + ",";
                }
                //remove last comma
                fieldnamelist = fieldnamelist.Substring(0, fieldnamelist.Length - 1);
                fieldparamlist = fieldparamlist.Substring(0, fieldparamlist.Length - 1);
                string SQL = "INSERT INTO " + tablename + " (" + fieldnamelist + ") VALUES ";
                SQL += "(" + fieldparamlist + ")";
                SQLiteCommand cmd = new SQLiteCommand(SQL);
                for (int i = 0; i < fields.Length; i = i + 2) {
                    cmd.Parameters.AddWithValue("@" + fields[i], fields[i + 1]);
                }
                cmd.Connection = m_db;
                if (m_db.State != System.Data.ConnectionState.Closed) {
                    MLog.Log("potential error as DB is not closed, state is " + m_db.State + " trying to close");
                    m_db.Close();
                }
                m_db.Open();
                trans = m_db.BeginTransaction();
                int retval = 0;
                try {
                    retval = cmd.ExecuteNonQuery();
                    if (retval != 1)
                        MLog.Log("Row in DB table " + tablename + " NOT inserted!");
                }
                catch (Exception ex) {
                    MLog.Log("Error inserting row in table " + tablename + " err=" + ex.Message);
                    trans.Rollback();
                }
                finally {
                    trans.Commit();
                    cmd.Dispose();
                    m_db.Close();
                }
            }
        }
        /*
        public class Temperature {
            public static void WriteRecord(DateTime datetime, int zoneid, double value, int position, string sensorid) {
                DB.GetDatabase();
                SQLiteTransaction trans;
                string SQL = "INSERT INTO temperature(datetime, zoneid,value,sensorposition,sensorid) VALUES";
                SQL += "(@datetime, @zoneid, @value, @sensorposition, @sensorid)";
                SQLiteCommand cmd = new SQLiteCommand(SQL);
                cmd.Parameters.AddWithValue("@datetime", datetime);
                cmd.Parameters.AddWithValue("@zoneid", zoneid);
                cmd.Parameters.AddWithValue("@value", value);
                cmd.Parameters.AddWithValue("@sensorposition", position);
                cmd.Parameters.AddWithValue("@sensorid", sensorid);
                cmd.Connection = m_db;
                m_db.Open();
                trans = m_db.BeginTransaction();
                int retval = 0;
                try {
                    retval = cmd.ExecuteNonQuery();
                    if (retval != 1)
                        MLog.Log("Row in DB temp NOT inserted!");
                }
                catch (Exception ex) {
                    MLog.Log("Error inserting row in temp db=" + ex.Message);
                    trans.Rollback();
                }
                finally {
                    trans.Commit();
                    cmd.Dispose();
                    m_db.Close();
                }
            }
        }*/
    }
}
