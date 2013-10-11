﻿using System;
using System.Collections.Generic;
using System.Linq;
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
		private List<Tuple<int, DateTime, int>> m_eventHistoryList;

		public SimpleGraph()
		{
			InitializeComponent();
			LoadHistory();
		}

		public void AddTemperatureReading(Tuple<int, DateTime, double> reading)
		{
			m_tempHistoryList.Add(reading);
		}

		public void AddHumidityReading(Tuple<int, DateTime, double> reading)
		{
			m_humHistoryList.Add(reading);
		}

		public void AddEventReading(Tuple<int, DateTime, int> reading)
		{
			m_eventHistoryList.Add(reading);
		}

		public void ShowTempHumGraph(int zoneId, int ageHours)
		{
			chart1.Series.Clear();
			var series1 = new System.Windows.Forms.DataVisualization.Charting.Series
			{
				Name = "Temperature",
				Color = System.Drawing.Color.Red,
				IsVisibleInLegend = true,
				IsXValueIndexed = false,
				ChartType = SeriesChartType.Line
			};
			var series2 = new System.Windows.Forms.DataVisualization.Charting.Series
			{
				Name = "Humidity",
				Color = System.Drawing.Color.Blue,
				IsVisibleInLegend = true,
				IsXValueIndexed = false,
				ChartType = SeriesChartType.Line
			};

			this.chart1.Legends[0].Docking = Docking.Bottom;
			this.chart1.Series.Add(series1);
			this.chart1.Series.Add(series2);
			this.chart1.ChartAreas[0].AxisX.LabelStyle.Format = "dd MMM HH:mm";

			List<Tuple<int, DateTime,double>> tempValues = m_tempHistoryList.FindAll(x=>x.Item1==zoneId && DateTime.Now.Subtract(x.Item2).TotalHours<=ageHours);
			foreach (var point in tempValues)
			{
				series1.Points.AddXY(point.Item2, point.Item3);
			}

			List<Tuple<int, DateTime, double>> humValues = m_humHistoryList.FindAll(x => x.Item1 == zoneId && DateTime.Now.Subtract(x.Item2).TotalHours <= ageHours);
			//chart1.ChartAreas[0].RecalculateAxesScale();

			foreach (var point in humValues)
			{
				series2.Points.AddXY(point.Item2, point.Item3);
			}
			//Tuple<int, DateTime, double> min = tempValues.Find(x => x.Item3 == 0);
			double minT = tempValues.Min(x => x.Item3);
			double minY = tempValues.Count>0 ? minT : double.MaxValue;
			minY = humValues.Count > 0 ? Math.Min(minY, humValues.Min(x => x.Item3)) : minY;

			chart1.ChartAreas[0].AxisY.Minimum = minY;
			//DateTime lastValue = tempValues[tempValues.Count-1].Item2
			//CustomLabel monthLabel = new CustomLabel(startOffset, endOffset, monthName, 1,     LabelMarkStyle.Box);
			//chart1.ChartAreas[0].AxisX.CustomLabels.Add(;
			chart1.ChartAreas[0].RecalculateAxesScale();
			
			chart1.Invalidate();
			chart1.SaveImage(IniFile.CurrentPath() + IniFile.WEBROOT_SUBFOLDER 
				+ "m\\tmp\\temp-hum-" + zoneId + "-"+ageHours+".gif", ChartImageFormat.Gif);
		}

		private void ShowEventGraph(int zoneId)
		{
			chart1.Series.Clear();
			
			var series3 = new System.Windows.Forms.DataVisualization.Charting.Series
			{
				Name = "Events",
				Color = System.Drawing.Color.Yellow,
				IsVisibleInLegend = true,
				IsXValueIndexed = false,
				ChartType = SeriesChartType.Point
			};

			
			this.chart1.Series.Add(series3);
			
			foreach (var point in m_eventHistoryList.FindAll(x => x.Item1 == zoneId))
			{
				series3.Points.AddXY(point.Item2, point.Item3);
			}
			chart1.Invalidate();
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
			this.AutoScaleDimensions = new System.Drawing.SizeF(4F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			//this.ClientSize = new System.Drawing.Size(640, 480);
			this.Controls.Add(this.chart1);
			this.Name = "Form1";
			this.Text = "FakeChart";
			//this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.ResumeLayout(false);
		}

		private void LoadHistory()
		{
			m_tempHistoryList = new List<Tuple<int, DateTime, double>>();
			m_humHistoryList = new List<Tuple<int, DateTime, double>>();
			m_eventHistoryList = new List<Tuple<int, DateTime, int>>();

			//GET TEMP and HUM from storage
			string[] allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath()+ IniFile.CSV_TEMPERATURE_HUMIDITY);
			var query = from line in allLines
						let data = line.Split(',')
						select new
						{
							ZoneName = data[0],
							Type = data[1],
							Date = Convert.ToDateTime(data[2]),
							Value = Convert.ToDouble(data[3]),
							ZoneId = Convert.ToInt16(data[4])
						};

			foreach (var line in query)
			{
				switch (line.Type)
				{
					case Constants.CAPABILITY_TEMP:
						m_tempHistoryList.Add(new Tuple<int, DateTime, double>(line.ZoneId, line.Date, line.Value));
						break;
					case Constants.CAPABILITY_HUM:
						m_humHistoryList.Add(new Tuple<int, DateTime, double>(line.ZoneId, line.Date, line.Value));
						break;
				}
			}

			//GET closures from storage
			allLines = System.IO.File.ReadAllLines(IniFile.CurrentPath() + IniFile.CSV_CLOSURES);
			var query2 = from line in allLines
						let data = line.Split(',')
						select new
						{
							ZoneName = data[0],
							Key = data[1],
							Date = Convert.ToDateTime(data[2]),
							State = data[3],
							ZoneId = Convert.ToInt16(data[4]),
							EventType = data[5] == Constants.EVENT_TYPE_CLOSURE ? 1 : (data[5] == Constants.EVENT_TYPE_CAMALERT ? 2 : (data[5]==Constants.EVENT_TYPE_SENSORALERT?3:0)),
						};

			foreach (var line in query2)
			{
				m_eventHistoryList.Add(new Tuple<int, DateTime, int>(line.ZoneId, line.Date, line.EventType));
			}
		}
	}
}
