using OxyPlot;
using OxyPlot.Series;

namespace AscCutlistEditor.Models.MQTT.MachineMessage
{
    internal class MachineMessagePlots
    {
        public PlotModel DetailedUptimePlot { get; set; }

        public PlotModel DetailedDowntimePlot { get; set; }

        private LineSeries _uptimeSeries;
        private BarSeries _downtimeStatsSeries;
    }
}