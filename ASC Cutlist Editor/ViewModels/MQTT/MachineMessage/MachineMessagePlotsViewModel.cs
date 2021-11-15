using System.Collections.Generic;
using System.Linq;
using AscCutlistEditor.Models.MQTT;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace AscCutlistEditor.ViewModels.MQTT.MachineMessage
{
    public class MachineMessagePlotsViewModel
    {
        public PlotModel TimeBarPlot { get; set; }

        public PlotModel DetailedUptimePlot { get; set; }

        public PlotModel DetailedDowntimePlot { get; set; }

        public MachineMessagePlotsViewModel()
        {
            CreateTimeBarPlot();
            CreateDetailedUptimePlot();
            CreateDetailedDowntimePlot();
        }

        /// <summary>
        /// Create the stacked bar chart for the machine's time percentages.
        /// </summary>
        public void CreateTimeBarPlot()
        {
            TimeBarPlot = new PlotModel
            {
                PlotAreaBorderColor = OxyColors.Transparent,
                PlotMargins = new OxyThickness(0),
                Padding = new OxyThickness(0)
            };

            // Invisible axes to hide default ones.
            TimeBarPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false
            });
            TimeBarPlot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });

            var timeSeriesRunning = new BarSeries
            {
                IsStacked = true,
                FillColor = OxyColors.Black
            };
            TimeBarPlot.Series.Add(timeSeriesRunning);

            var timeSeriesDown = new BarSeries
            {
                IsStacked = true,
                FillColor = OxyColors.Gray
            };
            TimeBarPlot.Series.Add(timeSeriesDown);
        }

        /// <summary>
        /// Create the detailed plot for showing uptime over time.
        /// </summary>
        public void CreateDetailedUptimePlot()
        {
            DetailedUptimePlot = new PlotModel
            {
                Title = "Status Over Time"
            };

            DetailedUptimePlot.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "h:mm:ss"
            });
            // Y axis with labels for current line status.
            DetailedUptimePlot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                // A little buffer room is added to make sure "Running" is
                // shown on the y-axis for the maximum.
                Maximum = 1.05,
                Minimum = 0,
                IsTickCentered = true,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                Labels = { "Stopped", "Running" }
            });

            var currentSeries = new LineSeries
            {
                Title = "Current Status",
                MarkerType = MarkerType.Square
            };
            DetailedUptimePlot.Series.Add(currentSeries);
        }

        /// <summary>
        /// Create the detailed plot for showing downtime reasons.
        /// </summary>
        public void CreateDetailedDowntimePlot()
        {
            DetailedDowntimePlot = new PlotModel
            {
                Title = "Downtime Statistics"
            };

            DetailedDowntimePlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Maximum = 105,
                Minimum = 0,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });
            DetailedDowntimePlot.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Left,
                IsTickCentered = true,
                IsZoomEnabled = false,
                IsPanEnabled = false,
                ItemsSource = new[]
                {
                    "Material Change",
                    "Bundle Unloading",
                    "Maintenance",
                    "Emergency",
                    "Idle",
                    "Shift Change",
                    "Break"
                }
            });

            var downtimeStatsSeries = new BarSeries
            {
                ItemsSource = new List<BarItem>(7),
                LabelFormatString = "{0:.00}%"
            };
            DetailedDowntimePlot.Series.Add(downtimeStatsSeries);
        }

        public void UpdatePlots(Models.MQTT.MachineMessage message)
        {
            UpdateTimeBarPlot(message);
            UpdateDetailedUptimePlot(message);
            UpdateDetailedDowntimePlot(message);
        }

        public void UpdateTimeBarPlot(Models.MQTT.MachineMessage message)
        {
            KPI kpi = message.tags.set1.PlantData.KPI;

            ((BarSeries)TimeBarPlot.Series[0]).ItemsSource = new List<BarItem>
            {
                new BarItem { Value = kpi.UptimePct }
            };
            ((BarSeries)TimeBarPlot.Series[1]).ItemsSource = new List<BarItem>
            {
                new BarItem { Value = kpi.DowntimePct }
            };
            TimeBarPlot.InvalidatePlot(true);
        }

        public void UpdateDetailedUptimePlot(Models.MQTT.MachineMessage message)
        {
            MqttPub pub = message.tags.set1.MqttPub;

            // Add new data point for line status.
            ((LineSeries)DetailedUptimePlot.Series.First()).Points.Add(
                new DataPoint(
                    DateTimeAxis.ToDouble(message.timestamp),
                    pub.LineRunning.Equals("LINE RUNNING") ? 1 : 0));
            DetailedUptimePlot.InvalidatePlot(true);
        }

        public void UpdateDetailedDowntimePlot(Models.MQTT.MachineMessage message)
        {
            KPI kpi = message.tags.set1.PlantData.KPI;

            // Recalculate percentage for downtime bars.
            ((BarSeries)DetailedDowntimePlot.Series.First()).ItemsSource = new List<BarItem>
            {
                new BarItem { Value = kpi.CoilChangePct },
                new BarItem { Value = kpi.BundlePct },
                new BarItem { Value = kpi.MaintPct },
                new BarItem { Value = kpi.EmergencyPct },
                new BarItem { Value = kpi.IdlePct },
                new BarItem { Value = kpi.ShiftChangePct },
                new BarItem { Value = kpi.BreakPct }
            };
            DetailedDowntimePlot.InvalidatePlot(true);
        }
    }
}