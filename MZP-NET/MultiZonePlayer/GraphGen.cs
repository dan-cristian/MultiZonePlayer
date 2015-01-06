using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data;

namespace MultiZonePlayer
{
	public class SimpleGraph : Form
	{
		private System.ComponentModel.IContainer components = null;
		System.Windows.Forms.DataVisualization.Charting.Chart chart1;
		private List<Tuple<int, DateTime, double>> m_humHistoryList;
		private List<Tuple<int, DateTime, double, int, String>> m_tempHistoryList;
		private List<Tuple<int, DateTime, double, int, double>> m_voltageHistoryList;
		private List<Tuple<int, DateTime, double, double, double, String>> m_utilitiesHistoryList;
		private List<Tuple<int, DateTime, int, String, String>> m_eventHistoryList;
		private List<Tuple<int, DateTime, String, String, String, String, String>> m_errorHistoryList;
		System.Drawing.Color[] m_colors = new System.Drawing.Color[10];
		private static DateTime m_lastTempHumReference = DateTime.MinValue;
		private static DateTime m_lastVoltageReference = DateTime.MinValue;
		private static DateTime m_lastUtilityReference = DateTime.MinValue;
		private static DateTime m_lastErrorReference = DateTime.MinValue;
		private static DateTime m_lastEventReference = DateTime.MinValue;

		public SimpleGraph(bool needTempHum, bool needClosure, bool needVoltage, bool needUtilities, bool needErrors)
		{
			InitializeComponent();
			m_colors[0] = System.Drawing.Color.OrangeRed;
			m_colors[1] = System.Drawing.Color.DarkRed;
			m_colors[2] = System.Drawing.Color.BlueViolet;
			m_colors[3] = System.Drawing.Color.GreenYellow;
			m_colors[4] = System.Drawing.Color.Olive;
			m_colors[5] = System.Drawing.Color.HotPink;
			m_colors[6] = System.Drawing.Color.Khaki;
			m_colors[7] = System.Drawing.Color.PaleVioletRed;
			m_colors[8] = System.Drawing.Color.LightSalmon;
			m_colors[9] = System.Drawing.Color.MediumSlateBlue;
			//LoadHistory(needTempHum, needClosure, needVoltage, needUtilities, needErrors);
		}

		
		private String GetDateFormat(int ageHours)
		{
			String format;
			if (ageHours <= 24)
				format = "HH:mm";
			else
				if (ageHours <= 48)
					format = "ddd HH:mm";
				else
					format = "dd MMM HH:mm";
			return format;
			
		}

		private void PrepareGraph(ref Chart chart, String title, DateTime refDate, int period) {
            String dateformat = "dd MMM yy HH:mm";
            chart.Series.Clear();
			chart.Titles.Clear();
			chart.Legends[0].Docking = Docking.Bottom;
			chart.ChartAreas[0].AxisX.LabelStyle.Format = GetDateFormat(period);
            title = title + "\n" + refDate.AddHours(-period).ToString(dateformat) + " / " + refDate.ToString(dateformat);
			chart.Titles.Add(title);
			chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
			chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
			//chart1.Legends[0].MaximumAutoSize = 50;
			chart.Legends[0].TextWrapThreshold = 75;
			//chart1.ChartAreas[0].AxisX2.Enabled = AxisEnabled.False;
			//chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
			//chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
		}

        
        public void ShowDBTempHumGraphs(String uniqueId, List<int> zoneIdList, bool showAllZones, int ageHours, int hoursSupplement) {
            DateTime genStart = DateTime.Now;
            m_lastTempHumReference = GetDateReference(m_lastTempHumReference, hoursSupplement);
            PrepareGraph(ref chart1, "DB Temp & Hum zone "+uniqueId,m_lastTempHumReference, ageHours);
            int zoneId;
            List<ZoneDetails> zoneList = new List<ZoneDetails>();
            String color;
            ZoneDetails tempZone;
            if (showAllZones) {
                DataTable zonesAvail = DB.GetDataTable(DB.QUERYNAME_TEMPERATURE_POSITIONS,  
                    DB.PARAM_START_DATETIME, m_lastTempHumReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                    DB.PARAM_END_DATETIME, m_lastTempHumReference.ToString(Constants.DATETIME_DB_FORMAT));
                foreach (DataRow row in zonesAvail.Rows ) {
                    zoneId = row.Field<int>(DB.COL_TEMPERATURE_ZONEID);
                    tempZone = ZoneDetails.GetZoneById(zoneId);
                    if (tempZone != null)
                        zoneList.Add(tempZone);
                    else
                        MLog.Log(this, "Unexpected unknown zone in temp graph id=" + zoneId);
                }
            }
            else {
                zoneList = GetZoneList(zoneIdList);
            }
            List<int> positionList = new List<int>();
            foreach (ZoneDetails zone in zoneList) {
                zoneId = zone.ZoneId;
                DataTable positionsAvail = DB.GetDataTable(DB.QUERYNAME_TEMPERATURE_SENSOR_POSITIONS, DB.PARAM_ZONEID, zoneId.ToString(), DB.PARAM_START_DATETIME,
                    m_lastTempHumReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT));
                int poscount = positionsAvail.Rows.Count;
                DataTable records;
                string positionName;
                String positionValue;
                DateTime datetime; double value;
                color = zone.Color != null ? zone.Color : "Black";
                double total;
                double min, max;
                Color graphcolor;
                for (int positionIndex = 0; positionIndex < poscount; positionIndex++) {
                    total = 0; min = double.MaxValue; max = double.MinValue;
                    positionValue = positionsAvail.Rows[positionIndex].Field<String>(DB.COL_TEMPERATURE_SENSORID);
                    positionName = positionsAvail.Rows[positionIndex].Field<String>(DB.COL_TEMPERATURE_SENSORNAME);
                    records = DB.GetDataTable(DB.QUERYNAME_TEMPERATURE_RECORDS, DB.PARAM_ZONEID, zoneId.ToString(), DB.PARAM_SENSORID, positionValue.ToString(),
                        DB.PARAM_START_DATETIME, m_lastTempHumReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastTempHumReference.ToString(Constants.DATETIME_DB_FORMAT));
                    if (records.Rows.Count > 0) {
                        graphcolor = poscount>1 ? m_colors[positionIndex] : System.Drawing.Color.FromName(color);
                        var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                            Name = "Temp " + positionName + "("+positionValue+")",
                            Color = graphcolor,
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Line,
                            BorderWidth = 1
                        };
                        this.chart1.Series.Add(series1);
                        foreach (DataRow row in records.Rows) {
                            datetime = row.Field<DateTime>(DB.COL_TEMPERATURE_DATETIME);
                            value = row.Field<double>(DB.COL_TEMPERATURE_VALUE);
                            series1.Points.AddXY(datetime, value);
                            min = Math.Min(min, value);
                            max = Math.Max(max, value);
                            total += value;
                        }
                        series1.Name += " avg=" + Math.Round(total / records.Rows.Count, 2) + " min=" + min + " max=" + max;
                    }
                    records.Clear();
                }
                //humidity
                records = DB.GetDataTable(DB.QUERYNAME_HUMIDITY_RECORDS, DB.PARAM_ZONEID, zoneId.ToString(),
                    DB.PARAM_START_DATETIME, m_lastTempHumReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                    DB.PARAM_END_DATETIME, m_lastTempHumReference.ToString(Constants.DATETIME_DB_FORMAT));
                if (records.Rows.Count > 0) {
                    total = 0; min = double.MaxValue; max = double.MinValue;
                    var series2 = new System.Windows.Forms.DataVisualization.Charting.Series {
                        Name = "Hum ",
                        Color = System.Drawing.Color.FromName(color),
                        IsVisibleInLegend = true,
                        IsXValueIndexed = false,
                        ChartType = SeriesChartType.Line,
                        BorderDashStyle = ChartDashStyle.DashDot,
                        BorderWidth = 1
                    };
                    this.chart1.Series.Add(series2);
                    foreach (DataRow row in records.Rows) {
                        datetime = row.Field<DateTime>(DB.COL_HUMIDITY_DATETIME);
                        value = row.Field<double>(DB.COL_HUMIDITY_VALUE);
                        series2.Points.AddXY(datetime, value);
                        min = Math.Min(min, value);
                        max = Math.Max(max, value);
                        total += value;
                    }
                    series2.Name += " avg=" + Math.Round(total / records.Rows.Count, 2) + " min=" + min + " max=" + max;
                    records.Clear();
                }
            }
            chart1.ChartAreas[0].RecalculateAxesScale();
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
            chart1.Titles[0].Text += " ["+Math.Round(DateTime.Now.Subtract(genStart).TotalSeconds,0).ToString()+" s]";
            chart1.Invalidate();
            SaveGraph(chart1, uniqueId, ageHours, "temphum");
        }
        /*
		public void ShowTempHumGraph(int zoneId, int ageHours, int hoursSupplement)
		{
			m_lastTempHumReference = GetDateReference(m_lastTempHumReference, hoursSupplement);
			PrepareGraph(ref chart1, "Temperature & Humidity @ " + m_lastTempHumReference.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
			List<Tuple<int, DateTime, double, int, String>> tempValues = null;
			List<int> positionList = new List<int>();
			string positionName;
			int positionValue;
			positionList = m_tempHistoryList.FindAll(x => x.Item1 == zoneId).Select(x => x.Item4).Distinct().ToList();
			//one zone can have multiple temperature values from distinct sensors
			for (int positionIndex = 0; positionIndex < positionList.Count; positionIndex++) {
				positionValue = positionList[positionIndex];
				positionName = m_tempHistoryList.FindLast(x=>x.Item1==zoneId && x.Item4==positionValue).Item5;
				tempValues = m_tempHistoryList.FindAll(x => x.Item1 == zoneId && x.Item4==positionValue
					&& m_lastTempHumReference.Subtract(x.Item2).TotalHours <= ageHours && m_lastTempHumReference.Subtract(x.Item2).TotalHours>=0);
				var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Temperature " + positionName,
					Color = m_colors[positionIndex],
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.Line,
					BorderWidth = 1
				};
				this.chart1.Series.Add(series1);
				double total = 0, min = double.MaxValue, max = double.MinValue;
				foreach (var point in tempValues){
					series1.Points.AddXY(point.Item2, point.Item3);
					min = Math.Min(min, point.Item3);
					max = Math.Max(max, point.Item3);
					total += point.Item3;
				}
				series1.Name += " Avg=" + Math.Round(total / series1.Points.Count, 2) + " Min=" + min + " Max=" + max;
			}

			List<Tuple<int, DateTime, double>> humValues = m_humHistoryList.FindAll(x => x.Item1 == zoneId && m_lastTempHumReference.Subtract(x.Item2).TotalHours <= ageHours);
			//chart1.ChartAreas[0].RecalculateAxesScale();
			if (humValues.Count > 0)
			{
				var series2 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Humidity",
					Color = System.Drawing.Color.Blue,
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.Line,
					BorderWidth = 1
				};
				this.chart1.Series.Add(series2);
				double total = 0, min = double.MaxValue, max = double.MinValue;
				foreach (var point in humValues)
				{
					series2.Points.AddXY(point.Item2, point.Item3);
					min = Math.Min(min, point.Item3);
					max = Math.Max(max, point.Item3);
					total += point.Item3;
				}
				series2.Name += " Avg=" + Math.Round(total / series2.Points.Count, 2) + " Min=" + min + " Max=" + max;
			}
			double minT;
			minT = tempValues!=null && tempValues.Count>0? tempValues.Min(x => x.Item3):0;
			double minY = tempValues.Count>0 ? minT : double.MaxValue;
			minY = humValues.Count > 0 ? Math.Min(minY, humValues.Min(x => x.Item3)) : minY;
			chart1.ChartAreas[0].AxisY.Minimum = minY;
			chart1.ChartAreas[0].RecalculateAxesScale();
			chart1.Invalidate();
			SaveGraph(chart1, zoneId.ToString(), ageHours, "temphum");
		}
        */
		private void SaveGraph(Chart chart, String zoneId, int ageHours, String type) {

			chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				+ type+"-"+ zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
		}

		private void UpdateUnitsAndTotal(DateTime valueDate, double value, ref double totalUnits, ref DateTime lastValueDate, ref double lastValue) {
			double minutesElapsed;
			if (lastValueDate != DateTime.MinValue) {
				minutesElapsed = valueDate.Subtract(lastValueDate).TotalMinutes;
				totalUnits += minutesElapsed * lastValue;
				lastValueDate = valueDate;
				lastValue = value;
			}

			if (lastValueDate == DateTime.MinValue) {
				lastValueDate = valueDate;
				lastValue = value;
			}
		}

        public void ShowDBVoltageGraph(String uniqueId, List<int> zoneIdList, bool showAllZones, int ageHours, int hoursSupplement) {
            try {
                DateTime genStart = DateTime.Now;
                m_lastVoltageReference = GetDateReference(m_lastVoltageReference, hoursSupplement);
                PrepareGraph(ref chart1, "DB Voltage zone " + uniqueId, m_lastVoltageReference, ageHours);
                int zoneId;
                List<ZoneDetails> zoneList = new List<ZoneDetails>();
                String color;
                ZoneDetails tempZone;
                if (showAllZones) {
                    DataTable zonesAvail = DB.GetDataTable(DB.QUERYNAME_VOLTAGE_POSITIONS,
                        DB.PARAM_START_DATETIME, m_lastVoltageReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastVoltageReference.ToString(Constants.DATETIME_DB_FORMAT));
                    foreach (DataRow row in zonesAvail.Rows) {
                        zoneId = row.Field<int>(DB.COL_VOLTAGE_ZONEID);
                        tempZone = ZoneDetails.GetZoneById(zoneId);
                        if (tempZone != null)
                            zoneList.Add(tempZone);
                        else
                            MLog.Log(this, "Unexpected unknown zone in voltage graph id=" + zoneId);
                    }
                }
                else {
                    zoneList = GetZoneList(zoneIdList);
                }
                List<int> positionList = new List<int>();
                DataTable records;
                DateTime datetime; 
                double value, min = double.MaxValue, max=double.MinValue, total=0;
                Color graphcolor;
                foreach (ZoneDetails zone in zoneList) {
                    zoneId = zone.ZoneId;
                    color = zone.Color != null ? zone.Color : "Black";
                    records = DB.GetDataTable(DB.QUERYNAME_VOLTAGE_RECORDS,
                        DB.PARAM_ZONEID, zoneId.ToString(),
                        DB.PARAM_START_DATETIME, m_lastVoltageReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastVoltageReference.ToString(Constants.DATETIME_DB_FORMAT));
                    if (records.Rows.Count > 0) {
                        graphcolor = System.Drawing.Color.FromName(color);
                        var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                            Name = "Voltage " + zone.ZoneName,
                            Color = graphcolor,
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Line,
                            BorderWidth = 1
                        };
                        this.chart1.Series.Add(series1);
                        foreach (DataRow row in records.Rows) {
                            datetime = row.Field<DateTime>(DB.COL_VOLTAGE_DATETIME);
                            value = row.Field<Double>(DB.COL_VOLTAGE_VALUE);
                            series1.Points.AddXY(datetime, value);
                            min = Math.Min(min, value);
                            max = Math.Max(max, value);
                            total += value;
                        }
                        series1.Name += " avg=" + Math.Round(total / records.Rows.Count, 2) + " min=" + min + " max=" + max;
                    }
                    records.Clear();
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
                chart1.Titles[0].Text += " [" + Math.Round(DateTime.Now.Subtract(genStart).TotalSeconds, 0).ToString() + " s]";
                chart1.Invalidate();
                SaveGraph(chart1, uniqueId, ageHours, Constants.CAPABILITY_VOLTAGE);
            }
            catch (Exception ex) {
                MLog.Log(ex, this, "Err DBVoltageGraph");
            }
        }
		public void ShowVoltageGraph(String uniqueId, List<int> zoneIdList, int ageHours, bool showAll, int hoursSupplement) {
			try {
				m_lastVoltageReference = GetDateReference(m_lastVoltageReference, hoursSupplement);
				PrepareGraph(ref chart1, "Voltage", m_lastVoltageReference, ageHours);
				int maxIndex = Convert.ToInt16(m_voltageHistoryList.Max(x => x.Item4));
				System.Windows.Forms.DataVisualization.Charting.Series[] series = new System.Windows.Forms.DataVisualization.Charting.Series[maxIndex+1];
				double minY = double.MaxValue, maxY = double.MinValue;
				double voltageTotalValue=0,minutesElapsed;//,unitsTotalValue = 0, ;
				DateTime lastPointTaken = DateTime.MinValue; double lastVoltageValue=0;//,lastUnitsValue = 0, ;
				List<ZoneDetails> zoneList = new List<ZoneDetails>();
				ZoneDetails tempZone;
				int index = 0;
				if (showAll) {
					List<int> tempzoneIdList = m_voltageHistoryList.Select(x => x.Item1).Distinct().ToList();
					foreach (int id in tempzoneIdList) {
						tempZone = ZoneDetails.GetZoneById(id);
						if (tempZone != null)
							zoneList.Add(tempZone);
						else
							MLog.Log(this, "Unexpected unknown zone in voltage graph id=" + id);
					}
				}
				else {
					zoneList = GetZoneList(zoneIdList);
				}
                //only index 2 seems relevant on my sensor
				foreach (ZoneDetails zone in zoneList) {
					List<Tuple<int, DateTime, double, int, double>> voltValues = m_voltageHistoryList.FindAll(
						x => x.Item1 == zone.ZoneId
						&& m_lastVoltageReference.Subtract(x.Item2).TotalHours <= ageHours && m_lastVoltageReference.Subtract(x.Item2).TotalHours >=0
						&& x.Item4 == zone.VoltageSensorIndex);
					if (voltValues.Count > 0) {
						series[index] = new System.Windows.Forms.DataVisualization.Charting.Series {
							Name = "Voltage " + zone.ZoneName,
							Color = m_colors[index],
							IsVisibleInLegend = true,
							IsXValueIndexed = false,
							ChartType = SeriesChartType.StepLine,
							BorderWidth = 1,
							MarkerSize = 3
						};
						this.chart1.Series.Add(series[index]);
						foreach (var point in voltValues) {
							series[index].Points.AddXY(point.Item2, point.Item3);
							UpdateUnitsAndTotal(point.Item2, point.Item3, ref voltageTotalValue, ref lastPointTaken, ref lastVoltageValue);
							/*if (lastPointTaken != DateTime.MinValue){
								minutesElapsed = point.Item2.Subtract(lastPointTaken).TotalMinutes;
								unitsTotalValue += minutesElapsed * lastUnitsValue;
								voltageTotalValue += minutesElapsed * lastVoltageValue;
								lastPointTaken = DateTime.MinValue;
							}

							if (lastPointTaken == DateTime.MinValue){
								lastPointTaken = point.Item2;
								lastUnitsValue = point.Item5;
								lastVoltageValue = point.Item3;
							}*/
						}
						minY = Math.Min(minY, voltValues.Min(x => x.Item3));
						maxY = Math.Max(maxY, voltValues.Max(x => x.Item3));
						minutesElapsed = voltValues[voltValues.Count - 1].Item2.Subtract(voltValues[0].Item2).TotalMinutes;
						series[index].Name += " min=" + minY + " max=" + Math.Round(maxY,4) + " V/min="+ Math.Round(voltageTotalValue/minutesElapsed, 4)
							;// +" units/min=" + Math.Round(unitsTotalValue / minutesElapsed, 4);
					}
					index++;
				}
				chart1.ChartAreas[0].AxisY.Maximum= maxY;
				chart1.ChartAreas[0].AxisY.Minimum= minY;
				chart1.ChartAreas[0].AxisY.LabelStyle.Format = "N3";
				chart1.ChartAreas[0].RecalculateAxesScale();
				chart1.Invalidate();
				SaveGraph(chart1, uniqueId, ageHours, "voltage");
				//chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				//	+ "voltage-" + uniqueId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Err gen voltage graph");
			}
		}

        public void ShowDBCounterGraph(int zoneId, String zoneName, int ageHours, String utilityType, int hoursSupplement) {
            try {
                DateTime genStart = DateTime.Now;
                m_lastUtilityReference = GetDateReference(m_lastUtilityReference, hoursSupplement);
                PrepareGraph(ref chart1, zoneName + " " + utilityType, m_lastUtilityReference, ageHours);
                List<int> zoneList = new List<int>();
                if (zoneId != -1)
                    zoneList.Add(zoneId);
                else {
                    DataTable zonesAvailable = DB.GetDataTable(DB.QUERYNAME_COUNTER_POSITIONS, DB.PARAM_TYPE, utilityType, DB.PARAM_START_DATETIME,
                        m_lastUtilityReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastUtilityReference.ToString(Constants.DATETIME_DB_FORMAT));
                    foreach (DataRow row in zonesAvailable.Rows) {
                        zoneList.Add(row.Field<int>(DB.COL_COUNTER_ZONEID));
                    }
                }
                int zoneCount = zoneList.Count;
                System.Windows.Forms.DataVisualization.Charting.Series[] series = new System.Windows.Forms.DataVisualization.Charting.Series[zoneCount];
                System.Windows.Forms.DataVisualization.Charting.Series series1;
                DataTable records;
                ZoneDetails zone;
                for (int i = 0; i < zoneCount; i++) {
                    records = DB.GetDataTable(DB.QUERYNAME_COUNTER_RECORDS, DB.PARAM_ZONEID, zoneList[i].ToString(),
                        DB.PARAM_START_DATETIME, m_lastUtilityReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastUtilityReference.ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAMS.type.ToString(), utilityType);
                    zone = ZoneDetails.GetZoneById(zoneList[i]);
                    double minY = double.MaxValue, maxY = double.MinValue;
                    if (records.Rows.Count > 0) {
                        series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                            //Name = Constants.CAPABILITY_ELECTRICITY,
                            Color = m_colors[i],
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.StepLine,
                            BorderWidth = 1,
                            MarkerSize = 3,
                            XAxisType = AxisType.Primary,
                            YAxisType = AxisType.Primary
                        };
                        this.chart1.Series.Add(series1);
                        double value = 0, cost = 0, minwatts = double.MaxValue, maxwatts = double.MinValue, totalwatts = 0, avgwatts;
                        //double minValue = double.MaxValue, maxValue = double.MinValue;
                        double yvalue;
                        foreach (DataRow row in records.Rows) {
                            switch (utilityType) { 
                                case Constants.CAPABILITY_ELECTRICITY:
                                    yvalue = row.Field<double>(DB.COL_COUNTER_SECONDARYUNIT);
                                    series1.Points.AddXY(row.Field<DateTime>(DB.COL_COUNTER_DATETIME), yvalue);
                                    value += row.Field<double>(DB.COL_COUNTER_MAINUNIT);
                                    totalwatts += yvalue;
                                    if (yvalue != 0)
                                        minwatts = Math.Min(minwatts, yvalue);
                                    maxwatts = Math.Max(maxwatts, yvalue);
                                    break;
                                case Constants.CAPABILITY_WATER:
                                case Constants.CAPABILITY_GAS:
                                    yvalue = row.Field<double>(DB.COL_COUNTER_MAINUNIT);
                                    series1.Points.AddXY(row.Field<DateTime>(DB.COL_COUNTER_DATETIME), yvalue);
                                    value += yvalue;
                                    minY = Math.Min(minY, yvalue);
                                    maxY = Math.Max(maxY, yvalue);
                                    break;
                            }
                            cost += row.Field<double>(DB.COL_COUNTER_COST);
                        }
                        switch (utilityType) {
                            case Constants.CAPABILITY_ELECTRICITY:
                                avgwatts = totalwatts / records.Rows.Count;
                                series1.Name = "units=" + Math.Round(value, 0) + " "+zone.PulseMainUnitType + " cost=" + Math.Round(cost, 0) 
                                    + " watts min=" + Math.Round(minwatts, 0) + " max=" + Math.Round(maxwatts, 0) + " avg=" + Math.Round(avgwatts, 0)
                                    ;
                                break;
                            case Constants.CAPABILITY_WATER:
                            case Constants.CAPABILITY_GAS:
                                series1.Name = "units=" + Math.Round(value, 2) +" "+ zone.PulseMainUnitType+ " cost=" + Math.Round(cost, 2) + " min=" + minY + " max=" + maxY 
                                    ;
                                break;
                        }
                        if (zoneId == -1)
                            series1.Name = ZoneDetails.GetZoneById(zoneList[i]).ZoneName + " " + series1.Name;
                    }
                    else {
                        maxY = 1;
                        minY = 0;
                    }
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.ChartAreas[0].AxisY.IsStartedFromZero = true;
                chart1.Titles[0].Text += " [" + Math.Round(DateTime.Now.Subtract(genStart).TotalSeconds, 0).ToString() + " s]";
                chart1.Invalidate();
                SaveGraph(chart1, zoneId.ToString(), ageHours, utilityType);
                //chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
                //	+ utilityType + "-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
            }
            catch (Exception ex) {
                MLog.Log(ex, this, "Err gen counters graph");
            }
        }

        /*
		public void ShowUtilitiesGraph(int zoneId, String zoneName, int ageHours, String utilityType, int hoursSupplement) {
			try {
				m_lastUtilityReference = GetDateReference(m_lastUtilityReference, hoursSupplement);
				PrepareGraph(ref chart1, zoneName + " " + utilityType + " @ " + m_lastUtilityReference.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
				List<int> zoneList;
				if (zoneId != -1)
					zoneList = new List<int>() { zoneId};
				else
					zoneList = m_utilitiesHistoryList.FindAll(x => m_lastUtilityReference.Subtract(x.Item2).TotalHours <= ageHours && (x.Item6 == utilityType)).Select(x => x.Item1).Distinct().OrderBy(x => x).ToList();
				int zoneCount=zoneList.Count;
				System.Windows.Forms.DataVisualization.Charting.Series[] series = new System.Windows.Forms.DataVisualization.Charting.Series[zoneCount];
				System.Windows.Forms.DataVisualization.Charting.Series series1;
				for (int i = 0; i < zoneCount; i++) {
					List<Tuple<int, DateTime, double, double, double, String>> tempValues=m_utilitiesHistoryList.FindAll(x => (x.Item1 == zoneList[i])
						&& m_lastUtilityReference.Subtract(x.Item2).TotalHours <= ageHours && m_lastUtilityReference.Subtract(x.Item2).TotalHours>=0
						&& (x.Item6 == utilityType));
					double minY = double.MaxValue, maxY = double.MinValue;
					if (tempValues.Count > 0) {
						series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
							//Name = Constants.CAPABILITY_ELECTRICITY,
							Color = m_colors[i],
							IsVisibleInLegend = true,
							IsXValueIndexed = false,
							ChartType = SeriesChartType.StepLine,
							BorderWidth = 1,
							MarkerSize = 3,
							XAxisType = AxisType.Primary,
							YAxisType = AxisType.Primary
						};
						this.chart1.Series.Add(series1);
						double value = 0, cost = 0, minwatts = double.MaxValue, maxwatts = double.MinValue, totalwatts = 0, avgwatts;
						double minValue = double.MaxValue, maxValue = double.MinValue;
						foreach (var point in tempValues) {
							switch (utilityType) {
								case Constants.CAPABILITY_ELECTRICITY:
									series1.Points.AddXY(point.Item2, point.Item5);
									value += point.Item3;
									totalwatts += point.Item5;
									if (point.Item5 != 0)
										minwatts = Math.Min(minwatts, point.Item5);
									maxwatts = Math.Max(maxwatts, point.Item5);
									break;
								case Constants.CAPABILITY_WATER:
									series1.Points.AddXY(point.Item2, point.Item3);
									value += point.Item3;
									break;
								
							}
							cost += point.Item4;
						}
						switch (utilityType) {
							case Constants.CAPABILITY_ELECTRICITY:
								minY = Math.Min(minY, tempValues.Min(x => x.Item5));
								maxY = Math.Max(maxY, tempValues.Max(x => x.Item5));
								minValue = tempValues.Min(x => x.Item3);
								maxValue = tempValues.Max(x => x.Item3);
								avgwatts = totalwatts / series1.Points.Count;
								series1.Name = "units=" + Math.Round(value, 2) + " cost=" + Math.Round(cost, 2) + " min=" + Math.Round(minValue,2)+" max="+Math.Round(maxValue,2)
									+ "\n watts min=" + Math.Round(minwatts, 0) + " max=" + Math.Round(maxwatts, 0) + " avg=" + Math.Round(avgwatts, 0);
								break;
							case Constants.CAPABILITY_WATER:
								minY = Math.Min(minY, tempValues.Min(x => x.Item3));
								maxY = Math.Max(maxY, tempValues.Max(x => x.Item3));
								series1.Name = "units=" + Math.Round(value, 2) + " cost=" + Math.Round(cost, 2) + " min="+minY + " max="+maxY;
								break;
						}
						if (zoneId == -1)
							series1.Name = ZoneDetails.GetZoneById(zoneList[i]).ZoneName + " " + series1.Name;
					}
					else {
						maxY = 1;
						minY = 0;
					}
				}
				chart1.ChartAreas[0].RecalculateAxesScale();
				chart1.Invalidate();
				SaveGraph(chart1, zoneId.ToString(), ageHours, utilityType);
				//chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				//	+ utilityType + "-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Err gen utility graph");
			}
		}
        */
		private List<ZoneDetails> GetZoneList(List<int> zoneIdList) {
			List<ZoneDetails> zoneList = new List<ZoneDetails>();
			ZoneDetails tempZone;
			foreach (int id in zoneIdList) {
				tempZone = ZoneDetails.GetZoneById(id);
				if (tempZone != null)
					zoneList.Add(tempZone);
			}
			return zoneList;
		}

        public void ShowDBErrorGraphs(String uniqueId, List<int> zoneIdList, bool showAllZones, int ageHours, int hoursSupplement) {
            try {
                DateTime genStart = DateTime.Now;
                m_lastErrorReference = GetDateReference(m_lastErrorReference, hoursSupplement);
                PrepareGraph(ref chart1, "DB Error zone " + uniqueId, m_lastErrorReference, ageHours);
                int zoneId;
                List<ZoneDetails> zoneList = new List<ZoneDetails>();
                String color;
                ZoneDetails tempZone;
                if (showAllZones) {
                    DataTable zonesAvail = DB.GetDataTable(DB.QUERYNAME_ERROR_POSITIONS,
                        DB.PARAM_START_DATETIME, m_lastErrorReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastErrorReference.ToString(Constants.DATETIME_DB_FORMAT));
                    foreach (DataRow row in zonesAvail.Rows) {
                        zoneId = row.Field<int>(DB.COL_ERROR_ZONEID);
                        tempZone = ZoneDetails.GetZoneById(zoneId);
                        if (tempZone != null)
                            zoneList.Add(tempZone);
                        else
                            MLog.Log(this, "Unexpected unknown zone in error graph id=" + zoneId);
                    }
                }
                else {
                    zoneList = GetZoneList(zoneIdList);
                }
                List<int> positionList = new List<int>();
                foreach (ZoneDetails zone in zoneList) {
                    zoneId = zone.ZoneId;
                    DataTable records;
                    DateTime datetime; double value;
                    color = zone.Color != null ? zone.Color : "Black";
                    Color graphcolor;
                    records = DB.GetDataTable(DB.QUERYNAME_ERROR_RECORDS, 
                        DB.PARAM_ZONEID, zoneId.ToString(),
                        DB.PARAM_START_DATETIME, m_lastErrorReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastErrorReference.ToString(Constants.DATETIME_DB_FORMAT));
                    if (records.Rows.Count > 0) {
                        graphcolor = System.Drawing.Color.FromName(color);
                        var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                            Name = "Error " + zone.ZoneName,
                            Color = graphcolor,
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Point,
                            BorderWidth = 1
                        };
                        this.chart1.Series.Add(series1);
                        foreach (DataRow row in records.Rows) {
                            datetime = row.Field<DateTime>(DB.COL_ERROR_DATETIME);
                            value = zone.ZoneId;
                            series1.Points.AddXY(datetime, value);
                        }
                    }
                    records.Clear();
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
                chart1.Titles[0].Text += " [" + Math.Round(DateTime.Now.Subtract(genStart).TotalSeconds, 0).ToString() + " s]";
                chart1.Invalidate();
                SaveGraph(chart1, uniqueId, ageHours, Constants.CAPABILITY_ERROR);
            }
            catch (Exception ex) {
                MLog.Log(ex, this, "Err DBErrorGraph");
            }
        }
        /*
		public void ShowErrorGraph(int ageHours, int zoneId, bool showallzones, int hoursSupplement) {
			try {
				//double lastMinY = double.MaxValue, minY = double.MaxValue;
				if (m_lastErrorReference == DateTime.MinValue)
					m_lastErrorReference = DateTime.Now;
				else {
					if (hoursSupplement == 0)
						m_lastErrorReference = DateTime.Now;
					else
						m_lastErrorReference = m_lastErrorReference.AddHours(hoursSupplement);
				}
				PrepareGraph(ref chart1, "Errors", m_lastErrorReference, ageHours);
				String color;
				List<int> zoneIdList; 
				List<ZoneDetails> zoneList = new List<ZoneDetails>();
				if (showallzones) {
					ZoneDetails zonetemp;
					zoneIdList = m_errorHistoryList.Select(x => x.Item1).Distinct().ToList();
					foreach (int id in zoneIdList) {
						zonetemp = ZoneDetails.GetZoneById(id);
						if (zonetemp != null)
							zoneList.Add(zonetemp);
						else
							MLog.Log(this, "Unexpected unknown zone in err graph id="+id);
					}
				}
				else {
					zoneList.Add(ZoneDetails.GetZoneById(zoneId));
				}

				foreach (ZoneDetails zone in zoneList) {
					color = zone.Color != null ? zone.Color : "Black";
					List<Tuple<int, DateTime, String, String, String, String, String>> errValues = m_errorHistoryList.FindAll(x => x.Item1 == zone.ZoneId
						&& m_lastErrorReference.Subtract(x.Item2).TotalHours <= ageHours && m_lastErrorReference.Subtract(x.Item2).TotalHours>=0);
					if (errValues.Count > 0) {
						var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
							Name = "Errors in " + zone.ZoneName,
							Color = System.Drawing.Color.FromName(color),
							IsVisibleInLegend = true,
							IsXValueIndexed = false,
							ChartType = SeriesChartType.Point,
							BorderWidth = 1
						};
						this.chart1.Series.Add(series1);
						//double total = 0, min = double.MaxValue, max = double.MinValue;
						foreach (var point in errValues) {
							series1.Points.AddXY(point.Item2, zone.ZoneId);
						}
						series1.Name += " Count=" + errValues.Count;
					}

					//double minT;
					//minT = errValues.Count > 0 ? errValues.Min(x => x.Item2) : 0;
					//minY = errValues.Count > 0 ? minT : double.MaxValue;
					//lastMinY = Math.Min(lastMinY, minY);
				}
				chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
				chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;
				//chart1.ChartAreas[0].AxisY.Minimum = lastMinY;
				chart1.ChartAreas[0].RecalculateAxesScale();
				chart1.Invalidate();
				SaveGraph(chart1, zoneId.ToString(), ageHours, Constants.CAPABILITY_ERROR);
				//chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				//	+ Constants.CAPABILITY_ERROR + "-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Error generating err graph zoneid="+zoneId+"  age="+ageHours);
			}
		}
        */

        public void ShowDBEventGraphs(String uniqueId, List<int> zoneIdList, bool showAllZones, int ageHours, int hoursSupplement) {
            try {
                DateTime genStart = DateTime.Now;
                m_lastEventReference = GetDateReference(m_lastEventReference, hoursSupplement);
                PrepareGraph(ref chart1, "DB Event zone " + uniqueId, m_lastEventReference, ageHours);
                int zoneId;
                List<ZoneDetails> zoneList = new List<ZoneDetails>();
                String color;
                string eventtype;
                ZoneDetails tempZone;
                if (showAllZones) {
                    DataTable zonesAvail = DB.GetDataTable(DB.QUERYNAME_EVENT_POSITIONS,
                        DB.PARAM_START_DATETIME, m_lastEventReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastEventReference.ToString(Constants.DATETIME_DB_FORMAT));
                    foreach (DataRow row in zonesAvail.Rows) {
                        zoneId = row.Field<int>(DB.COL_EVENT_ZONEID);
                        tempZone = ZoneDetails.GetZoneById(zoneId);
                        if (tempZone != null)
                            zoneList.Add(tempZone);
                        else
                            MLog.Log(this, "Unexpected unknown zone in event graph id=" + zoneId);
                    }
                }
                else {
                    zoneList = GetZoneList(zoneIdList);
                }
                List<int> positionList = new List<int>();
                foreach (ZoneDetails zone in zoneList) {
                    zoneId = zone.ZoneId;
                    DataTable records;
                    DateTime datetime; double value;
                    color = zone.Color != null ? zone.Color : "Black";
                    Color graphcolor;
                    records = DB.GetDataTable(DB.QUERYNAME_EVENT_RECORDS,
                        DB.PARAM_ZONEID, zoneId.ToString(),
                        DB.PARAM_START_DATETIME, m_lastEventReference.AddHours(-ageHours).ToString(Constants.DATETIME_DB_FORMAT),
                        DB.PARAM_END_DATETIME, m_lastEventReference.ToString(Constants.DATETIME_DB_FORMAT));
                    if (records.Rows.Count > 0) {
                        graphcolor = System.Drawing.Color.FromName(color);
                        var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
                            Name = "Event " + zone.ZoneName,
                            Color = graphcolor,
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Point,
                            BorderWidth = 1
                        };
                        this.chart1.Series.Add(series1);
                        foreach (DataRow row in records.Rows) {
                            datetime = row.Field<DateTime>(DB.COL_EVENT_DATETIME);
                            eventtype = row.Field<String>(DB.COL_EVENT_EVENTTYPE);
                            switch (eventtype) {
                                case Constants.EVENT_TYPE_CAMALERT:
                                    value = 1;
                                    break;
                                case Constants.EVENT_TYPE_SENSORALERT:
                                    value = 2;
                                    break;
                                case Constants.EVENT_TYPE_CLOSURE:
                                    value = 3;
                                    break;
                                case Constants.EVENT_TYPE_PRESENCECONTACT:
                                    value = 4;
                                    break;
                                default:
                                    value = 10;
                                    break;

                            }
                            series1.Points.AddXY(datetime, value);
                        }
                    }
                    records.Clear();
                }
                chart1.ChartAreas[0].RecalculateAxesScale();
                chart1.ChartAreas[0].AxisY.IsStartedFromZero = false;
                chart1.Titles[0].Text += " [" + Math.Round(DateTime.Now.Subtract(genStart).TotalSeconds, 0).ToString() + " s]";
                chart1.Invalidate();
                SaveGraph(chart1, uniqueId, ageHours, Constants.EVENT_TYPE_CLOSURE);
            }
            catch (Exception ex) {
                MLog.Log(ex, this, "Err DBeventGraph");
            }
        }
		public void ShowEventGraph(int zoneId, int ageHours, int hoursSupplement)
		{
			m_lastEventReference = GetDateReference(m_lastEventReference, hoursSupplement);
			PrepareGraph(ref chart1, "Events", m_lastEventReference, ageHours);
			List<Tuple<int, DateTime, int, String, String>> closureValues, sensorValues, camValues, powerValues;

			List<String> distinctClosureIdentifiers = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && m_lastEventReference.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_CLOSURE).Select(y => y.Item5).Distinct().ToList();
			sensorValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && m_lastEventReference.Subtract(x.Item2).TotalHours <= ageHours 
				&& x.Item4==Constants.EVENT_TYPE_SENSORALERT && x.Item3!=0);
			camValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && m_lastEventReference.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_CAMALERT && x.Item3 != 0);
			powerValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && m_lastEventReference.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_POWER);
			int i = 0;
			Random r = new Random();
			foreach (String closureIdentifier in distinctClosureIdentifiers) {
				closureValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && m_lastEventReference.Subtract(x.Item2).TotalHours <= ageHours
					&& m_lastEventReference.Subtract(x.Item2).TotalHours>=0
					&& x.Item4 == Constants.EVENT_TYPE_CLOSURE && x.Item5 == closureIdentifier);
				if (closureValues.Count > 0) {
					var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
						Name = "Closure " + closureIdentifier,
						Color = m_colors[i],//System.Drawing.Color.Red,
						IsVisibleInLegend = true,
						IsXValueIndexed = false,
						ChartType = SeriesChartType.StepLine,
						MarkerSize = 6,
						BorderWidth = 1
					};
					this.chart1.Series.Add(series1);
					foreach (var point in closureValues) {
						series1.Points.AddXY(point.Item2, point.Item3 !=0 ? point.Item3 + i/2:0);//add i to avoid line overlapp
					}
					String details = closureValues[closureValues.Count - 1].Item2.ToString();
					series1.Name += " last @ " + details + ", " + closureValues.Count + " events";
				}
				i++;
			}
			if (sensorValues.Count > 0)
			{
				var series2 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Sensor",
					Color = System.Drawing.Color.Blue,
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.Point,
					MarkerSize = 6
				};
				this.chart1.Series.Add(series2);
				foreach (var point in sensorValues)
				{
					series2.Points.AddXY(point.Item2, point.Item3);
				}
				String details = sensorValues[sensorValues.Count - 1].Item2.ToString();
				series2.Name += " last @ " + details + ", " + sensorValues.Count + " events";
			}

			if (camValues.Count > 0)
			{
				var series3 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Camera",
					Color = System.Drawing.Color.Green,
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.Point,
					MarkerSize = 6
				};
				this.chart1.Series.Add(series3);
				foreach (var point in camValues)
				{
					series3.Points.AddXY(point.Item2, point.Item3);
				}
				String details = camValues[camValues.Count - 1].Item2.ToString();
				series3.Name += " last @ " + details + ", " + camValues.Count + " events";
			}
			if (powerValues.Count > 0)
			{
				var series4 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Power",
					Color = System.Drawing.Color.Orange,
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.StepLine,
					MarkerSize = 6,
					BorderWidth = 1
				};
				this.chart1.Series.Add(series4);
				foreach (var point in powerValues)
				{
					series4.Points.AddXY(point.Item2, point.Item3);
				}
				String details = powerValues[powerValues.Count - 1].Item2.ToString();
				series4.Name += " last @ " + details + ", " + powerValues.Count + " events";
			}
			chart1.ChartAreas[0].AxisY.Minimum = 0;
			chart1.ChartAreas[0].AxisY.Maximum = 4;
			chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
			chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;
			chart1.ChartAreas[0].RecalculateAxesScale();
			chart1.Invalidate();
			SaveGraph(chart1, zoneId.ToString(), ageHours, "event");
			//chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
			//	+ "event-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
		}

		private DateTime GetDateReference(DateTime lastDate, int hoursSupplement) {
			DateTime result;
			if (lastDate == DateTime.MinValue)
				lastDate = DateTime.Now;
			
			if (hoursSupplement == 0)
				result = DateTime.Now;
			else
				result = lastDate.AddHours(hoursSupplement);
			
			return result;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
			this.SuspendLayout();
			//
			// chart1
			//
			chartArea1.Name = "ChartArea1";
			this.chart1.ChartAreas.Add(chartArea1);
			this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
			legend1.Name = "Legend1";
			this.chart1.Legends.Add(legend1);
			this.chart1.Location = new System.Drawing.Point(0, 50);
			this.chart1.Name = "chart1";
			// this.chart1.Size = new System.Drawing.Size(284, 212);
			this.chart1.TabIndex = 0;
			this.chart1.Text = "chart1";
			//
			// Form1
			//
			//this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 18F);
			this.AutoScaleDimensions = new System.Drawing.SizeF(5F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			//this.ClientSize = new System.Drawing.Size(640, 480);
			this.Controls.Add(this.chart1);
			this.Name = "Form1";
			this.Text = "FakeChart";
			//this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.ResumeLayout(false);
		}

		private void LoadHistory(bool needTempHum, bool needClosure, bool needVoltage, bool needUtilities, bool needErrors){
			m_tempHistoryList = new List<Tuple<int, DateTime, double, int, String>>();
			m_humHistoryList = new List<Tuple<int, DateTime, double>>();
			m_eventHistoryList = new List<Tuple<int, DateTime, int, String, String>>();
			m_voltageHistoryList = new List<Tuple<int, DateTime, double, int, double>>();
			m_utilitiesHistoryList = new List<Tuple<int, DateTime, double, double, double, String>>();
			m_errorHistoryList = new List<Tuple<int, DateTime, string, string, string, string, string>>();

			string[] allLines;

			try {
				
				if (needVoltage) {
					MLog.Log(this, "START reading Voltage");
					allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_VOLTAGE);
					var query = from line in allLines
								let data = line.Split(',')
								select new {
									ZoneName = data[0],
									Type = data[1],
									Date = Convert.ToDateTime(data[2]),
									Value = Convert.ToDouble(data[3]),
									ZoneId = Convert.ToInt16(data[4]),
									VoltageIndex = Convert.ToInt16(data[5]),
									UnitsMeasured =  GetDouble(data[6])
								};
					MLog.Log(this, "Processing Voltage");
					foreach (var line in query) {
						switch (line.Type) {
							case Constants.CAPABILITY_VOLTAGE:
								m_voltageHistoryList.Add(new Tuple<int, DateTime, double, int, double>(line.ZoneId, line.Date, line.Value, line.VoltageIndex, line.UnitsMeasured));
								break;
						}
					}
					MLog.Log(this, "End reading Voltage");
				}
				if (needClosure) {
					MLog.Log(this, "START reading Closures");
					//GET closures from storage
					allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_CLOSURES);
					var query = from line in allLines
								let data = line.Split(',')
								select new {
									ZoneName = data[0],
									Key = data[1],
									Date = Convert.ToDateTime(data[2]),
									State = GetStateValue(data[3]),
									ZoneId = Convert.ToInt16(data[4]),
									EventType = data[5],
									Identifier = data.Length > 6 ? data[6] : "Main"
								};
					MLog.Log(this, "Processing Closures");
					foreach (var line in query) {
						m_eventHistoryList.Add(new Tuple<int, DateTime, int, String, String>(line.ZoneId, line.Date, line.State, line.EventType, line.Identifier));
					}
					MLog.Log(this, "End reading Closures");
				}
				
				if (needErrors) {
					MLog.Log(this, "START reading error list");
					allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_DEVICEERRORS);
					var query = from line in allLines
								let data = line.Split(',')
								select new {
									ZoneId = GetInt(data[0]),
									Date = GetDate(data[1]),
									DeviceAddress = data[2],
									DeviceName = data[3],
									ZoneName = data[4],
									DeviceType = data[5],
									ErrorMessage = data[6]
								};
					MLog.Log(this, "Processing error list");
					foreach (var line in query) {
						m_errorHistoryList.Add(new Tuple<int, DateTime, String, String, String, String, String>
							(line.ZoneId, line.Date, line.DeviceAddress, line.DeviceName,line.ZoneName, line.DeviceType, line.ErrorMessage));
					}
					MLog.Log(this, "End reading error list");
				}
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Error loading history for graph");
			}
		}
		private DateTime GetDate(String date) {
			DateTime datetime;
			//System.Globalization.CultureInfo culture;
			//System.Globalization.DateTimeStyles styles;
			//styles = System.Globalization.DateTimeStyles.None;
			//culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
			if (!DateTime.TryParse(date, /*culture, styles,*/out datetime)) {
				datetime = DateTime.Now;
				Alert.CreateAlertOnce("Error converting datetime in graph load @" + datetime, "GraphGetDate");
			}
			return datetime;
		}
		private double GetDouble(String doubl) {
			double val;
			if (!double.TryParse(doubl, out val)) {
				val = 0;
				Alert.CreateAlertOnce("Error converting double in graph load @" + doubl, "GraphGetDouble");
			}
			return val;
		}
		private int GetInt(String valueint) {
			int val;
			if (!int.TryParse(valueint, out val)) {
				val = 0;
				Alert.CreateAlertOnce("Error converting int in graph load @" + valueint, "GraphGetInt");
			}
			return val;
		}
		private int GetStateValue(string state)
		{
			switch (state)
			{
				case "ContactOpen":
					return 0;
				case "ContactClosed":
					return 2;
				case "Sensoropened":
					return 3;
				case "Sensorclosed":
					return 0;
				case "CamMove":
					return 4;
				case "PowerOn":
					return 1;
				case "PowerOff":
					return 0;
				default:
					return 0;
			}
		}
	}
}
