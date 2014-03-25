using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MultiZonePlayer
{
	public class SimpleGraph : Form
	{
		private System.ComponentModel.IContainer components = null;
		System.Windows.Forms.DataVisualization.Charting.Chart chart1;
		private List<Tuple<int, DateTime, double>> m_tempHistoryList, m_humHistoryList;
		private List<Tuple<int, DateTime, double, int>> m_voltageHistoryList;
		private List<Tuple<int, DateTime, double, double>> m_utilitiesHistoryList;
		private List<Tuple<int, DateTime, int, String, String>> m_eventHistoryList;
		System.Drawing.Color[] m_colors = new System.Drawing.Color[10];


		public SimpleGraph(bool needTempHum, bool needClosure, bool needVoltage, bool needUtilities)
		{
			InitializeComponent();
			m_colors[0] = System.Drawing.Color.Orchid;
			m_colors[1] = System.Drawing.Color.DarkRed;
			m_colors[2] = System.Drawing.Color.Aquamarine;
			m_colors[3] = System.Drawing.Color.GreenYellow;
			m_colors[4] = System.Drawing.Color.Olive;
			m_colors[5] = System.Drawing.Color.HotPink;
			m_colors[6] = System.Drawing.Color.Khaki;
			m_colors[7] = System.Drawing.Color.PaleVioletRed;
			m_colors[8] = System.Drawing.Color.LightSalmon;
			m_colors[9] = System.Drawing.Color.MediumSlateBlue;
			LoadHistory(needTempHum, needClosure, needVoltage, needUtilities);
		}

		public void AddTemperatureReading(Tuple<int, DateTime, double> reading)
		{
			m_tempHistoryList.Add(reading);
		}

		public void AddHumidityReading(Tuple<int, DateTime, double> reading)
		{
			m_humHistoryList.Add(reading);
		}

		public void AddEventReading(Tuple<int, DateTime, int, String, String> reading)
		{
			m_eventHistoryList.Add(reading);
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

		public void ShowTempHumGraph(int zoneId, int ageHours)
		{
			PrepareGraph("Temperature & Humidity @ " + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
			List<Tuple<int, DateTime,double>> tempValues = m_tempHistoryList.FindAll(x=>x.Item1==zoneId && DateTime.Now.Subtract(x.Item2).TotalHours<=ageHours);
			if (tempValues.Count > 0)
			{
				var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
				{
					Name = "Temperature",
					Color = System.Drawing.Color.Red,
					IsVisibleInLegend = true,
					IsXValueIndexed = false,
					ChartType = SeriesChartType.Line,
					BorderWidth = 1
				};
				this.chart1.Series.Add(series1);
				double total = 0, min = double.MaxValue, max = double.MinValue;
				foreach (var point in tempValues)
				{
					series1.Points.AddXY(point.Item2, point.Item3);
					min = Math.Min(min, point.Item3);
					max = Math.Max(max, point.Item3);
					total += point.Item3;
				}
				series1.Name += " Avg=" + Math.Round(total / series1.Points.Count, 2) + " Min=" + min + " Max=" + max;
			}

			List<Tuple<int, DateTime, double>> humValues = m_humHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours);
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
			minT = tempValues.Count>0? tempValues.Min(x => x.Item3):0;
			double minY = tempValues.Count>0 ? minT : double.MaxValue;
			minY = humValues.Count > 0 ? Math.Min(minY, humValues.Min(x => x.Item3)) : minY;
			chart1.ChartAreas[0].AxisY.Minimum = minY;
			chart1.ChartAreas[0].RecalculateAxesScale();
			chart1.Invalidate();
			chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER 
				+ "temp-hum-" + zoneId + "-"+ageHours+".gif", ChartImageFormat.Gif);
		}
		private void PrepareGraph(String title, int period) {
			chart1.Series.Clear();
			chart1.Titles.Clear();
			this.chart1.Legends[0].Docking = Docking.Bottom;
			this.chart1.ChartAreas[0].AxisX.LabelStyle.Format = GetDateFormat(period);
			chart1.Titles.Add(title);
			chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
			chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
			//chart1.ChartAreas[0].AxisX2.Enabled = AxisEnabled.False;
			//chart1.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
			//chart1.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
		}

		public void ShowVoltageGraph(int zoneId, int ageHours) {
			try {
				
				PrepareGraph("Voltage @ " + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
				int maxIndex = Convert.ToInt16(m_voltageHistoryList.Max(x => x.Item4));
				System.Windows.Forms.DataVisualization.Charting.Series[] series = new System.Windows.Forms.DataVisualization.Charting.Series[maxIndex+1];
				double minY = double.MaxValue, maxY = double.MinValue;
				for (int index = 2; index <= maxIndex; index++) {
					List<Tuple<int, DateTime, double, int>> tempValues = m_voltageHistoryList.FindAll(
						x => x.Item1 == zoneId
						&& DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours && x.Item4 == index);
					if (tempValues.Count > 0) {
						series[index] = new System.Windows.Forms.DataVisualization.Charting.Series {
							Name = "Voltage" + index,
							Color = m_colors[index],
							IsVisibleInLegend = true,
							IsXValueIndexed = false,
							ChartType = SeriesChartType.StepLine,
							BorderWidth = 1,
							MarkerSize = 3
						};
						this.chart1.Series.Add(series[index]);
						foreach (var point in tempValues) {
							series[index].Points.AddXY(point.Item2, point.Item3);
							minY = Math.Min(minY, tempValues.Min(x => x.Item3));
							maxY = Math.Max(maxY, tempValues.Max(x => x.Item3));
						}
					}
				}
				chart1.ChartAreas[0].AxisY.Maximum= maxY;
				chart1.ChartAreas[0].AxisY.Minimum= minY;
				chart1.ChartAreas[0].RecalculateAxesScale();
				chart1.Invalidate();
				chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
					+ "voltage-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Err gen voltage graph");
			}
		}

		public void ShowElectricityGraph(int zoneId, int ageHours) {
			try {

				PrepareGraph(Constants.CAPABILITY_ELECTRICITY+ " @ " + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
				System.Windows.Forms.DataVisualization.Charting.Series series1, series2;
				double minY = double.MaxValue, maxY = double.MinValue;
				
				List<Tuple<int, DateTime, double, double>> tempValues = m_utilitiesHistoryList.FindAll(
					x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours);
				if (tempValues.Count > 0) {
					series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
						//Name = Constants.CAPABILITY_ELECTRICITY,
						Color = Color.OrangeRed,
						IsVisibleInLegend = true,
						IsXValueIndexed = false,
						ChartType = SeriesChartType.StepLine,
						BorderWidth = 1,
						MarkerSize = 3,
						XAxisType = AxisType.Primary,
						YAxisType = AxisType.Primary
					};
					this.chart1.Series.Add(series1);
					
					/*series2 = new System.Windows.Forms.DataVisualization.Charting.Series {
						Name = "cost",
						Color = Color.Green,
						IsVisibleInLegend = true,
						IsXValueIndexed = false,
						ChartType = SeriesChartType.Bubble,
						BorderWidth = 1,
						MarkerSize = 1,
						XAxisType = AxisType.Secondary,
						YAxisType = AxisType.Secondary
					};
					this.chart1.Series.Add(series2);
					*/
					double value = 0, cost = 0;
					foreach (var point in tempValues) {
						series1.Points.AddXY(point.Item2, point.Item3);
						value += point.Item3;
						minY = Math.Min(minY, tempValues.Min(x => x.Item3));
						maxY = Math.Max(maxY, tempValues.Max(x => x.Item3));
						cost += point.Item4;
						//series2.Points.AddXY(point.Item2, point.Item4);
					}
					series1.Name = Constants.CAPABILITY_ELECTRICITY + "\nunits=" + Math.Round(value, 2) + "   cost=" + Math.Round(cost, 2);
				}
				else {
					maxY = 1;
					minY = 0;
				}

				//chart1.ChartAreas[0].AxisY.Maximum = maxY;
				//chart1.ChartAreas[0].AxisY.Minimum = minY;
				chart1.ChartAreas[0].RecalculateAxesScale();
				chart1.Invalidate();
				chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
					+ Constants.CAPABILITY_ELECTRICITY + "-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
			}
			catch (Exception ex) {
				MLog.Log(ex, this, "Err gen electricity graph");
			}
		}

		public void ShowTempGraph(int ageHours, List<ZoneDetails> zones)
		{
			double lastMinY=double.MaxValue, minY=double.MaxValue;
			PrepareGraph("Temperature @ " + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
			String color;
			foreach (ZoneDetails zone in zones)
			{
				color = zone.Color != null ? zone.Color : "Black";
				List<Tuple<int, DateTime, double>> tempValues = m_tempHistoryList.FindAll(x => x.Item1 == zone.ZoneId 
					&& DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours);
				if (tempValues.Count > 0)
				{
					var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
					{
						Name = "Temp " + zone.Description,
						Color = System.Drawing.Color.FromName(color),
						IsVisibleInLegend = true,
						IsXValueIndexed = false,
						ChartType = SeriesChartType.Line,
						BorderWidth = 1
					};
					this.chart1.Series.Add(series1);
					double total = 0, min = double.MaxValue, max = double.MinValue;
					foreach (var point in tempValues)
					{
						series1.Points.AddXY(point.Item2, point.Item3);
						total += point.Item3;
						min = Math.Min(min,point.Item3);
						max = Math.Max(max, point.Item3);
						total += point.Item3;
					}

					series1.Name += " Avg="+Math.Round(total/series1.Points.Count,2)+" Min="+min+" Max="+max;
				}
				
				double minT;
				minT = tempValues.Count > 0 ? tempValues.Min(x => x.Item3) : 0;
				minY = tempValues.Count > 0 ? minT : double.MaxValue;
				//minY = humValues.Count > 0 ? Math.Min(minY, humValues.Min(x => x.Item3)) : minY;

				lastMinY = Math.Min(lastMinY, minY);
			}
			chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
			chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.Minimum = lastMinY;
			chart1.ChartAreas[0].RecalculateAxesScale();
			chart1.Invalidate();
			chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				+ "temp-hum-all-" + ageHours + ".gif", ChartImageFormat.Gif);
		}
		public void ShowEventGraph(int zoneId, int ageHours)
		{
			PrepareGraph("Events @ " + DateTime.Now.ToString(IniFile.DATETIME_FULL_FORMAT), ageHours);
			List<Tuple<int, DateTime, int, String, String>> closureValues, sensorValues, camValues, powerValues;

			List<String> distinctClosureIdentifiers = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_CLOSURE).Select(y => y.Item5).Distinct().ToList();
			sensorValues =  m_eventHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours 
				&& x.Item4==Constants.EVENT_TYPE_SENSORALERT && x.Item3!=0);
			camValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_CAMALERT && x.Item3 != 0);
			powerValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours
				&& x.Item4 == Constants.EVENT_TYPE_POWER);
			int i = 0;
			Random r = new Random();
			foreach (String closureIdentifier in distinctClosureIdentifiers) {
				closureValues = m_eventHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours
					&& x.Item4 == Constants.EVENT_TYPE_CLOSURE && x.Item5 == closureIdentifier);
				if (closureValues.Count > 0) {
					var series1 = new System.Windows.Forms.DataVisualization.Charting.Series {
						Name = "Closure " + closureIdentifier,
						Color = m_colors[r.Next(m_colors.Length-1)],//System.Drawing.Color.Red,
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
			}
			chart1.ChartAreas[0].AxisY.Minimum = 0;
			chart1.ChartAreas[0].AxisY.Maximum = 4;
			chart1.ChartAreas[0].AxisY.MajorGrid.LineWidth = 0;
			chart1.ChartAreas[0].AxisX.MajorGrid.LineWidth = 1;
			chart1.ChartAreas[0].RecalculateAxesScale();
			chart1.Invalidate();
			chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEB_TMP_IMG_SUBFOLDER
				+ "event-" + zoneId + "-" + ageHours + ".gif", ChartImageFormat.Gif);
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

		private void LoadHistory(bool needTempHum, bool needClosure, bool needVoltage, bool needUtilities){
			m_tempHistoryList = new List<Tuple<int, DateTime, double>>();
			m_humHistoryList = new List<Tuple<int, DateTime, double>>();
			m_eventHistoryList = new List<Tuple<int, DateTime, int, String, String>>();
			m_voltageHistoryList = new List<Tuple<int, DateTime, double, int>>();
			m_utilitiesHistoryList = new List<Tuple<int, DateTime, double, double>>();
			string[] allLines;

			if (needTempHum) {
				MLog.Log(this, "Start reading TEMP/HUM from csv files");
				//GET TEMP and HUM from storage
				allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_TEMPERATURE_HUMIDITY);
				var query = from line in allLines
							let data = line.Split(',')
							select new {
								ZoneName = data[0],
								Type = data[1],
								Date = Convert.ToDateTime(data[2]),
								Value = Convert.ToDouble(data[3]),
								ZoneId = Convert.ToInt16(data[4])
							};
				MLog.Log(this, "Processing TEMP/HUM");
				foreach (var line in query) {
					switch (line.Type) {
						case Constants.CAPABILITY_TEMP:
							m_tempHistoryList.Add(new Tuple<int, DateTime, double>(line.ZoneId, line.Date, line.Value));
							break;
						case Constants.CAPABILITY_HUM:
							m_humHistoryList.Add(new Tuple<int, DateTime, double>(line.ZoneId, line.Date, line.Value));
							break;
					}
				}
				MLog.Log(this, "End reading TEMP/HUM from csv files");
			}
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
								 VoltageIndex = Convert.ToInt16(data[5])
							 };
				MLog.Log(this, "Processing Voltage");
				foreach (var line in query) {
					switch (line.Type) {
						case Constants.CAPABILITY_VOLTAGE:
							m_voltageHistoryList.Add(new Tuple<int, DateTime, double, int>(line.ZoneId, line.Date, line.Value, line.VoltageIndex));
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
			if (needUtilities) {
				MLog.Log(this, "START reading Utilities");
				allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_UTILITIES);
				var query = from line in allLines
							 let data = line.Split(',')
							 select new {
								 ZoneName = data[0],
								 Date = Convert.ToDateTime(data[1]),
								 Value = Convert.ToDouble(data[2]),
								 ZoneId = Convert.ToInt16(data[3]),
								 Type = data[4].ToLower(),
								 //TotalUnits
								 Cost = Convert.ToDouble(data[6])
								 //UnitCost
							 };
				MLog.Log(this, "Processing Utilities");
				foreach (var line in query) {
					switch (line.Type) {
						case Constants.CAPABILITY_ELECTRICITY:
							m_utilitiesHistoryList.Add(new Tuple<int, DateTime, double, double>(line.ZoneId, line.Date, line.Value, line.Cost));
							break;
					}
				}
				MLog.Log(this, "End reading Utilities");
			}
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
