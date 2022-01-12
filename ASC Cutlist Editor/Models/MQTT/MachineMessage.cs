using System;
using AscCutlistEditor.Frameworks;

namespace AscCutlistEditor.Models.MQTT
{
    // A bunch of models for the JSON data coming through MQTT from the machines.
    public class MachineMessage : ObservableObject
    {
        public string connected { get; set; }
        public Tags tags { get; set; }
        public DateTime timestamp { get; set; }
    }

    public class Tags
    {
        public Set1 set1 { get; set; }

        public Set2 set2 { get; set; }

        public Set3 set3 { get; set; }
    }

    public class Set1
    {
        public MachineStatistics MachineStatistics { get; set; }
        public MqttPub MqttPub { get; set; }
        public PlantData PlantData { get; set; }
    }

    public class Set2
    {
        public MqttSub MqttSub { get; set; }
    }

    public class Set3
    {
        public MqttPub MqttPub { get; set; }
    }

    public class MachineStatistics
    {
        public double UserPrime { get; set; }
        public double UserScrap { get; set; }
        public double UserUsage { get; set; }
    }

    public class MqttPub
    {
        public string CoilDatReq { get; set; }
        public string CoilStoreReq { get; set; }
        public string CoilUsageDat { get; set; }
        public string CoilUsageSend { get; set; }
        public string EmergencyStopped { get; set; }
        public string JobNumber { get; set; }
        public string LineRunning { get; set; }
        public string OrderDatFilters { get; set; }
        public string OrderDatReq { get; set; }
        public string OrderNo { get; set; }
        public string OrderNoImported { get; set; }
        public string ScanCoilID { get; set; }
        public string ScrapUsageDat { get; set; }
        public string ScrapUsageSend { get; set; }
    }

    public class MqttSub
    {
        public string MqttDest { get; set; }
        public string MqttString { get; set; }
        public string OrderDatAck { get; set; }
        public string OrderDatRecv { get; set; }
        public string OrderDatDelSent { get; set; }
        public string BundleDatRecv { get; set; }
        public string OrderNo { get; set; }
        public string CoilDatAck { get; set; }
        public string CoilDatRecv { get; set; }
        public string CoilDatValidAck { get; set; }
        public string CoilUsageRecvAck { get; set; }
        public string CoilStoreAck { get; set; }
        public string CoilStoreRecv { get; set; }
        public string ScrapUsageAck { get; set; }
    }

    public class ActiveData
    {
        public double COIL_LENGTH { get; set; }
        public int COIL_THICK_GAUGE { get; set; }
        public double COIL_THICK_IN { get; set; }
        public double COIL_WEIGHT { get; set; }
        public double COIL_WIDTH { get; set; }
        public string FINISH { get; set; }
        public string GAUGE { get; set; }
        public string ID { get; set; }
        public string MATERIAL { get; set; }
    }

    public class COILCALCS
    {
        public ActiveData ActiveData { get; set; }
    }

    public class COIL
    {
        public COILCALCS COIL_CALCS { get; set; }
        public string DESCRIPTION { get; set; }
        public string LOG_COMMENT { get; set; }
    }

    public class KPI
    {
        public double BreakHrs { get; set; }
        public double BreakPct { get; set; }
        public double BundleHrs { get; set; }
        public double BundlePct { get; set; }
        public double CoilChangeHrs { get; set; }
        public double CoilChangePct { get; set; }
        public double DowntimeHrs { get; set; }
        public double DowntimeHrsExt { get; set; }
        public double DowntimeHrsOld { get; set; }
        public double DowntimePct { get; set; }
        public double DowntimeResult { get; set; }
        public double EmergencyHrs { get; set; }
        public double EmergencyPct { get; set; }
        public double IdleHrs { get; set; }
        public double IdlePct { get; set; }
        public string LOG_COMMENT { get; set; }
        public double MaintenanceHrs { get; set; }
        public double MaintPct { get; set; }
        public double PrimeFootagePct { get; set; }
        public double ScrapFootagePct { get; set; }
        public double ShiftChangeHrs { get; set; }
        public double ShiftChangePct { get; set; }
        public double TotalHours { get; set; }
        public double UptimeHrs { get; set; }
        public double UptimePct { get; set; }
    }

    public class WORKORDER
    {
        public double IMPORTED_FOOTAGE { get; set; }
        public string NUMBER { get; set; }
        public string PART_LOG_STRING { get; set; }
    }

    public class PlantData
    {
        public COIL COIL { get; set; }
        public KPI KPI { get; set; }
        public WORKORDER WORKORDER { get; set; }
    }

    /// <summary>
    /// Utility class for sending string data to SetUsageData.
    /// </summary>
    public class CoilUsage
    {
        public string orderno { get; set; }
        public string CoilId { get; set; }
        public string CoilMatl { get; set; }
        public string ItemID { get; set; }
        public decimal Length { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
    }
}