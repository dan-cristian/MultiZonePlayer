using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;

namespace MultiZonePlayer {
    public class DB {
        protected static SQLiteConnection m_dbcon = null;
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

        public const string TABLE_EVENT = "event";
        public static string COL_EVENT_DATETIME = "datetime";
        public static string COL_EVENT_ZONEID = "zoneid";
        public static string COL_EVENT_RELAYSTATE = "relaystate";
        public static string COL_EVENT_EVENTTYPE = "eventtype";
        public static string COL_EVENT_KEY = "key";

        public const string TABLE_ERROR = "error";
        public static string COL_ERROR_DATETIME = "datetime";
        public static string COL_ERROR_ZONEID = "zoneid";
        public static string COL_ERROR_ID = "id";
        public static string COL_ERROR_TEXT= "text";

        public static Object m_lock = new Object();

        public enum PARAMS {
            zoneid,
            startdatetime,
            enddatetime,
            sensorid,
            type
        }
        public const String PARAM_ID = "@";
        public const String PARAM_ZONEID = "zoneid";
        public const String PARAM_START_DATETIME = "startdatetime";
        public const String PARAM_END_DATETIME = "enddatetime";
        public const String PARAM_SENSORID = "sensorid";
        public const String PARAM_TYPE = "type";

        public class QueryPair {
            public string QueryName;
            public string QuerySQL;
            public QueryPair(String name, String sql){
                QueryName = name;
                QuerySQL = sql;
            }
        }

        public static String QUERYNAME_TEMPERATURE_RECORDS = "TEMPERATURE_RECORDS";
        private static String QUERYSQL_TEMPERATURE_ROWS_FILTER = " WHERE " + COL_TEMPERATURE_ZONEID + "=" + PARAM_ID + PARAM_ZONEID + " AND " + COL_TEMPERATURE_DATETIME
            + ">='" + PARAM_ID + PARAM_START_DATETIME+"'";
        private static String QUERYSQL_TEMPERATURE_RECORDS = "SELECT " + COL_TEMPERATURE_DATETIME + ", " + COL_TEMPERATURE_VALUE
            + " FROM " + DB.TABLE_TEMPERATURE + QUERYSQL_TEMPERATURE_ROWS_FILTER + " AND " + COL_TEMPERATURE_SENSORID + "='" + PARAM_ID + PARAM_SENSORID+"'"
            + " AND "+ DB.COL_TEMPERATURE_DATETIME + " <='"+ PARAM_ID + PARAM_END_DATETIME+"' ORDER BY " + COL_TEMPERATURE_DATETIME + " ASC";

        public static String QUERYNAME_TEMPERATURE_POSITIONS = "TEMPERATURE_POSITIONS";
        private static String QUERYSQL_TEMPERATURE_POSITIONS = "SELECT DISTINCT(zoneid) FROM temperature WHERE datetime>='@startdatetime' AND datetime<='@enddatetime'";

        public const String QUERYNAME_TEMPERATURE_SENSOR_POSITIONS = "TEMPERATURE_SENSOR_POSITIONS";
        private static String QUERYSQL_TEMPERATURE_SENSOR_POSITIONS = "SELECT DISTINCT(" + COL_TEMPERATURE_SENSORID + "), "+COL_TEMPERATURE_SENSORNAME
            +" FROM " + DB.TABLE_TEMPERATURE + QUERYSQL_TEMPERATURE_ROWS_FILTER;

        public static String QUERYNAME_HUMIDITY_RECORDS = "HUMIDITY_RECORDS";
        private static String QUERYSQL_HUMIDITY_ROWS_FILTER = " WHERE " + COL_HUMIDITY_ZONEID + "=" + PARAM_ID + PARAM_ZONEID + " AND " + COL_HUMIDITY_DATETIME
            + ">='" + PARAM_ID + PARAM_START_DATETIME + "'";
        private static String QUERYSQL_HUMIDITY_RECORDS = "SELECT " + COL_HUMIDITY_DATETIME + ", " + COL_HUMIDITY_VALUE
            + " FROM " + DB.TABLE_HUMIDITY + QUERYSQL_HUMIDITY_ROWS_FILTER /*+ " AND " + COL_HUMIDITY_SENSORID + "='" + PARAM_ID + PARAM_SENSORID + "'"*/
            + " AND " + DB.COL_HUMIDITY_DATETIME + " <='" + PARAM_ID + PARAM_END_DATETIME + "' ORDER BY " + COL_HUMIDITY_DATETIME + " ASC";

        public static String QUERYNAME_COUNTER_POSITIONS = "COUNTER_POSITIONS";
        private static String QUERYSQL_COUNTER_POSITIONS = "SELECT DISTINCT(zoneid) FROM counter WHERE datetime>='@startdatetime' AND datetime<='@enddatetime'"
            +" AND utilitytype='@type'";
        public static String QUERYNAME_COUNTER_RECORDS = "COUNTER_RECORDS";
        private static String QUERYSQL_COUNTER_RECORDS = "SELECT datetime, mainunit, secondaryunit, cost FROM counter WHERE zoneid=@zoneid AND datetime>='@startdatetime' AND datetime<='@enddatetime'"
            + " AND utilitytype='@type'";

        public static String QUERYNAME_ERROR_POSITIONS = "ERROR_POSITIONS";
        private static String QUERYSQL_ERROR_POSITIONS = "SELECT DISTINCT(zoneid) FROM error WHERE datetime>='@startdatetime' AND datetime<='@enddatetime'";
        public static String QUERYNAME_ERROR_RECORDS = "ERROR_RECORDS";
        private static String QUERYSQL_ERROR_RECORDS = "SELECT datetime FROM error WHERE zoneid=@zoneid AND datetime>='@startdatetime' AND datetime<='@enddatetime'";

        public static String QUERYNAME_EVENT_POSITIONS = "EVENT_POSITIONS";
        private static String QUERYSQL_EVENT_POSITIONS = "SELECT DISTINCT(zoneid) FROM event WHERE datetime>='@startdatetime' AND datetime<='@enddatetime'";
        public static String QUERYNAME_EVENT_RECORDS = "EVENT_RECORDS";
        private static String QUERYSQL_EVENT_RECORDS = "SELECT datetime, relaystate, eventtype FROM event WHERE zoneid=@zoneid AND datetime>='@startdatetime' AND datetime<='@enddatetime'";

        private static List<QueryPair> m_queryList = null;
        public static void InitQueries() {
            m_queryList = new List<QueryPair>();
            m_queryList.Add(new QueryPair(QUERYNAME_TEMPERATURE_RECORDS, QUERYSQL_TEMPERATURE_RECORDS));
            m_queryList.Add(new QueryPair(QUERYNAME_TEMPERATURE_POSITIONS, QUERYSQL_TEMPERATURE_POSITIONS));
            m_queryList.Add(new QueryPair(QUERYNAME_TEMPERATURE_SENSOR_POSITIONS, QUERYSQL_TEMPERATURE_SENSOR_POSITIONS));
            m_queryList.Add(new QueryPair(QUERYNAME_HUMIDITY_RECORDS, QUERYSQL_HUMIDITY_RECORDS));
            m_queryList.Add(new QueryPair(QUERYNAME_COUNTER_POSITIONS, QUERYSQL_COUNTER_POSITIONS));
            m_queryList.Add(new QueryPair(QUERYNAME_COUNTER_RECORDS, QUERYSQL_COUNTER_RECORDS));
            m_queryList.Add(new QueryPair(QUERYNAME_ERROR_POSITIONS, QUERYSQL_ERROR_POSITIONS));
            m_queryList.Add(new QueryPair(QUERYNAME_ERROR_RECORDS, QUERYSQL_ERROR_RECORDS));
            m_queryList.Add(new QueryPair(QUERYNAME_EVENT_POSITIONS, QUERYSQL_EVENT_POSITIONS));
            m_queryList.Add(new QueryPair(QUERYNAME_EVENT_RECORDS, QUERYSQL_EVENT_RECORDS));
        }
        protected static void GetDatabase() {
            if (m_dbcon==null) {
                //DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
                //m_db = fact.CreateConnection();
                //m_db.ConnectionString = "Data Source=c:\\sqlite\\Sensors.db3";
                m_dbcon = new SQLiteConnection("Data Source="+IniFile.PARAM_SQLITEDB_PATH[1]+";Version=3;New=False;Compress=True;");
            }
            //if (m_db.State != System.Data.ConnectionState.Open) {
            //    m_db.Open();
            //}
            if (m_queryList == null)
                InitQueries();
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
                cmd.Connection = m_dbcon;
                if (m_dbcon.State != System.Data.ConnectionState.Closed) {
                    MLog.Log("potential error as DB is not closed, state is " + m_dbcon.State + " trying to close");
                    m_dbcon.Close();
                }
                m_dbcon.Open();
                trans = m_dbcon.BeginTransaction();
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
                    m_dbcon.Close();
                }
            }
        }

       

        private static void ExecuteQuery(string txtQuery) {
            lock (m_lock) {
                SQLiteCommand sql_cmd;
                DB.GetDatabase();
                m_dbcon.Open();
                sql_cmd = m_dbcon.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                sql_cmd.ExecuteNonQuery();
                m_dbcon.Close();
            }
        }
        private static DataTable LoadData(string sqlcommand) {
            lock (m_lock) {
                SQLiteDataAdapter DBadapt;
                SQLiteCommand sql_cmd;
                DataSet DS = new DataSet();
                DataTable DT = new DataTable();
                DB.GetDatabase();
                m_dbcon.Open();
                sql_cmd = m_dbcon.CreateCommand();
                DBadapt = new SQLiteDataAdapter(sqlcommand, m_dbcon);
                DS.Reset();
                DBadapt.Fill(DS);
                DT = DS.Tables[0];
                m_dbcon.Close();
                return DT;
            }
        }

       
            
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryname"></param>
        /// <param name="parameters">comma separated param name,value</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string queryname, params string[] parameters) {
            lock (m_lock) {
                DB.GetDatabase();
                DataTable dt = new DataTable();
                QueryPair query = m_queryList.Find(x=>x.QueryName==queryname);
                if (query != null) {
                    string sql = query.QuerySQL.ReplaceParams(PARAM_ID, parameters);
                    MLog.LogSql(sql);
                    DataTable result = LoadData(sql);
                    MLog.LogSql("Return OK rowscount=" + result.Rows.Count);
                    return result;
                }
                else {
                    Alert.CreateAlert("No query found with name=" + queryname, true);
                    return new DataTable();
                }
                
            }
        }
        public class Reports {
           
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
