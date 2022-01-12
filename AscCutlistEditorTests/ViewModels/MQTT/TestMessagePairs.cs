using System;
using System.Collections.Generic;
using System.Text;
using AscCutlistEditor.Models.MQTT;
using AscCutlistEditorTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AscCutlistEditorTests.ViewModels.MQTT
{
    // List of corresponding pub and sub messages by index for testing.
    public class TestMessagePairs
    {
        public static readonly List<MachineMessage> PubMessages = new List<MachineMessage>
        {
            // Pub message 0.
            new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MachineStatistics = new MachineStatistics
                        {
                            UserPrime = 3292.49,
                            UserScrap = 16.53,
                            UserUsage = 3309.01
                        },
                        MqttPub = new MqttPub
                        {
                            CoilDatReq = "TRUE",
                            CoilStoreReq = "TRUE",
                            CoilUsageDat = "TEST-ORDERNO1,TEST-COILID,TEST-MAT,TEST-ITEMID,5|TEST-ORDERNO2,TEST-COILID,TEST-MAT,TEST-ITEMID,10",
                            CoilUsageSend = "TRUE",
                            EmergencyStopped = "LINE IS ACTIVE",
                            JobNumber = "JN28174",
                            LineRunning = "LINE STOPPED",
                            OrderDatReq = "TRUE",
                            OrderNo = "10152",
                            ScanCoilID = "H2215"
                        },
                        PlantData = new PlantData
                        {
                            COIL = new COIL
                            {
                                COIL_CALCS = new COILCALCS
                                {
                                    ActiveData = new ActiveData()
                                },
                                DESCRIPTION = "",
                                LOG_COMMENT = ""
                            },
                            KPI = new KPI(),
                            WORKORDER = new WORKORDER()
                        }
                    },
                    set3 = new Set3()
                },
                timestamp = DateTime.Now
            },
            // Pub message 1.
            new MachineMessage
            {
                connected = "true",
                tags = new Tags
                {
                    set1 = new Set1
                    {
                        MachineStatistics = new MachineStatistics
                        {
                            UserPrime = 3292.49,
                            UserScrap = 16.53,
                            UserUsage = 3309.01
                        },
                        MqttPub = new MqttPub
                        {
                            CoilDatReq = "TRUE",
                            CoilStoreReq = "FALSE",
                            CoilUsageDat = "",
                            CoilUsageSend = "FALSE",
                            EmergencyStopped = "LINE IS ACTIVE",
                            JobNumber = "1",
                            LineRunning = "LINE STOPPED",
                            OrderDatReq = "FALSE",
                            OrderDatFilters = "OrderLen:LS",
                            OrderNo = "",
                            OrderNoImported = "NON-EXISTENT ORDER NO",
                            ScanCoilID = ""
                        },
                        PlantData = new PlantData
                        {
                            COIL = new COIL
                            {
                                COIL_CALCS = new COILCALCS
                                {
                                    ActiveData = new ActiveData()
                                },
                                DESCRIPTION = "",
                                LOG_COMMENT = ""
                            },
                            KPI = new KPI(),
                            WORKORDER = new WORKORDER()
                        }
                    },
                    set3 = new Set3()
                },
                timestamp = DateTime.Now
            }
        };

        public static readonly List<MachineMessage> SubMessages = new List<MachineMessage>
        {
            // Sub message 0.
            new MachineMessage
            {
                tags = new Tags
                {
                    set2 = new Set2
                    {
                        MqttSub = new MqttSub
                        {
                            MqttDest = "JN28174",
                            MqttString = null,
                            OrderDatAck = "TRUE",
                            OrderDatRecv = "96415-66288,173.000,23,1|96415-01008,173.000,23,2|96416-01947,185.000,40,2|96417-02726,222.000,57,2",
                            BundleDatRecv = "COSH2443LS,PANPBR24LSTONE,,asdf,Coating Kings,PANEL, PBR, 24GA Light Stone,Coating Kings,625 E Dockside,,Arkadelphia,AR,71923",
                            OrderNo = "10152",
                            CoilDatAck = "TRUE",
                            CoilDatRecv = "Coil is NOT OKAY to run!|Coil and order material are different.|Not enough material remaining.",
                            CoilDatValidAck = "FALSE",
                            CoilUsageRecvAck = "TRUE",
                            CoilStoreAck = "TRUE",
                            CoilStoreRecv = "202|H2287,COIL-HR 0.054 (16ga) x 13 15/16 GRAY,,2650.40|H2288,COIL-HR 0.054 (16ga) x 13 15/16 GRAY,,2650.40|H2289,COIL-HR 0.054 (16ga) x 13 15/16 RED,,2650.40|H2290,COIL-HR 0.054 (16ga) x 13 15/16 RED,,2650.40|H2291,COIL-HR 0.054 (16ga) x 15 15/16 GALV,,2424.40|H2292,COIL-HR 0.054 (16ga) x 15 15/16 GALV,,2650.40|H2293,COIL-HR 0.054 (16ga) x 15 15/16 GRAY,,2610.40|H2294,COIL-HR 0.054 (16ga) x 15 15/16 GRAY,,2650.40|H2295,COIL-HR 0.054 (16ga) x 15 15/16 RED,,2650.40|H2296,COIL-HR 0.054 (16ga) x 15 15/16 RED,,2650.40|H2297,COIL-HR 0.054 (16ga) x 16 15/16 GALV,,2633.40|H2298,COIL-HR 0.054 (16ga) x 16 15/16 GALV,,2650.40|H2299,COIL-HR 0.054 (16ga) x 16 15/16 GRAY,,2650.40|H2300,COIL-HR 0.054 (16ga) x 16 15/16 GRAY,,2650.40|H2301,COIL-HR 0.054 (16ga) x 16 15/16 RED,,2650.40|H2302,COIL-HR 0.054 (16ga) x 16 15/16 RED,,2650.40|H2303,COIL-HR 0.054 (16ga) x 17 15/16 GALV,,2650.40|H2304,COIL-HR 0.054 (16ga) x 17 15/16 GALV,,2650.40|H2305,COIL-HR 0.054 (16ga) x 17 15/16 GRAY,,2650.40|H2306,COIL-HR 0.054 (16ga) x 17 15/16 GRAY,,2650.40|H2307,COIL-HR 0.054 (16ga) x 17 15/16 RED,,2650.40|H2308,COIL-HR 0.054 (16ga) x 17 15/16 RED,,2650.40|T1971001,COIL-SH 0.0236 (24ga) x 43 GR80 ASH GRAY,COSH2443AG,3000.00|T1971002,COIL-SH 0.0236 (24ga) x 43 GR80 ASH GRAY,COSH2443AG,3000.00|T1971003,COIL-SH 0.0236 (24ga) x 43 GR80 BURNISHED SLATE,COSH2443BS,2944.50|T1971004,COIL-SH 0.0236 (24ga) x 43 GR80 BURNISHED SLATE,COSH2443BS,3000.00|T1971005,COIL-SH 0.0236 (24ga) x 43 GR80 COUNTRY RED,COSH2443CR,3.80|T1971006,COIL-SH 0.0236 (24ga) x 43 GR80 COUNTRY RED,COSH2443CR,2686.00|T1971007,COIL-SH 0.0236 (24ga) x 43 GR80 EVERGREEN,COSH2443EG,2700.00|T1971008,COIL-SH 0.0236 (24ga) x 43 GR80 EVERGREEN,COSH2443EG,3000.00|T1971009,COIL-SH 0.0236 (24ga) x 43 GR80 GALVALUME,COSH2443GA,2797.00|T1971010,COIL-SH 0.0236 (24ga) x 43 GR80 GALVALUME,COSH2443GA,3000.00|T1971011,COIL-SH 0.0236 (24ga) x 43 GR80 LIGHT STONE,COSH2443LS,3000.00|T1971012,COIL-SH 0.0236 (24ga) x 43 GR80 LIGHT STONE,COSH2443LS,3000.00|T1971013,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,3000.00|T1971014,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,2820.00|T1971015,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,3000.00|T1971016,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,3000.00|T1971017,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,3000.00|T1971018,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,3000.00|T1971019,COIL-SH 0.0185 (26ga) x 43 GR80 ASH GRAY,COSH2643AG,3150.00|T1971020,COIL-SH 0.0185 (26ga) x 43 GR80 ASH GRAY,COSH2643AG,3150.00|T1971021,COIL-SH 0.0185 (26ga) x 43 GR80 BURNISHED SLATE,COSH2643BS,3150.00|T1971022,COIL-SH 0.0185 (26ga) x 43 GR80 BURNISHED SLATE,COSH2643BS,3150.00|T1971023,COIL-SH 0.0185 (26ga) x 43 GR80 COUNTRY RED,COSH2643CR,0.00|T1971024,COIL-SH 0.0185 (26ga) x 43 GR80 COUNTRY RED,COSH2643CR,0.00|T1971025,COIL-SH 0.0185 (26ga) x 43 GR80 EVERGREEN,COSH2643EG,3150.00|T1971026,COIL-SH 0.0185 (26ga) x 43 GR80 EVERGREEN,COSH2643EG,3150.00|T1971027,COIL-SH 0.0185 (26ga) x 43 GR80 GALVALUME,COSH2643GA,3150.00|T1971028,COIL-SH 0.0185 (26ga) x 43 GR80 GALVALUME,COSH2643GA,3150.00|T1971029,COIL-SH 0.0185 (26ga) x 43 GR80 LIGHT STONE,COSH2643LS,3150.00|T1971030,COIL-SH 0.0185 (26ga) x 43 GR80 LIGHT STONE,COSH2643LS,3150.00|T1971031,COIL-SH 0.0185 (26ga) x 43 GR80 POLAR WHITE,COSH2643PW,3150.00|T1971032,COIL-SH 0.0185 (26ga) x 43 GR80 POLAR WHITE,COSH2643PW,3150.00|T1971033,COIL-SH 0.0185 (26ga) x 43 GR80 REGAL BLUE,COSH2643RB,3150.00|T1971034,COIL-SH 0.0185 (26ga) x 43 GR80 REGAL BLUE,COSH2643RB,3150.00|T1971035,COIL-SH 0.0185 (26ga) x 43 GR80 SADDLE TAN,COSH2643ST,3150.00|T1971036,COIL-SH 0.0185 (26ga) x 43 GR80 SADDLE TAN,COSH2643ST,3150.00|T1972001,COIL-SH 0.0185 (26ga) x 43 GR80 SADDLE TAN,COSH2643ST,3150.00|T1972002,COIL-SH 0.0185 (26ga) x 43 GR80 SADDLE TAN,COSH2643ST,3150.00|1195,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1196,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1197,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1198,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1199,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1200,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,960.00|1201,COIL-SH 0.0185 (26ga) x 43 GR80 ASH GRAY,COSH2643AG,3644.00|1203,COIL-SH 0.0236 (24ga) x 43 GR80 BURNISHED SLATE,COSH2443BS,827.30|1204,COIL-SH 0.0236 (24ga) x 43 GR80 BURNISHED SLATE,COSH2443BS,913.00|1205,COIL-SH 0.0236 (24ga) x 43 GR80 BURNISHED SLATE,COSH2443BS,913.00|1206,COIL-HR 0.095 (12ga) x 13 15/16 GALV,,496.10|1207,COIL-HR 0.095 (12ga) x 13 15/16 GALV,,512.00|1211,COIL-HR 0.095 (12ga) x 9 15/16 GALV,,287.70|1212,COIL-HR 0.095 (12ga) x 13 15/16 GRAY,,512.80|1214,COIL-HR 0.071 (14ga) x 11 15/16 GALV,,1028.10|1215,COIL-SH 0.0185 (26ga) x 43 GR80 BURNISHED SLATE,COSH2643BS,911.00|1216,COIL-SH 0.0185 (26ga) x 43 GR80 BURNISHED SLATE,COSH2643BS,911.00|1217,COIL-SH 0.0185 (26ga) x 43 GR80 POLAR WHITE,COSH2643PW,1.00|1218,COIL-SH 0.0185 (26ga) x 43 GR80 POLAR WHITE,COSH2643PW,671.00|1219,COIL-HR 0.095 (12ga) x 9 15/16 GALV,COSH2443LS,1206.00|1220,COIL-HR 0.095 (12ga) x 9 15/16 GALV,COSH2443LS,1206.00|1221,COIL-HR 0.095 (12ga) x 9 15/16 GRAY,,900.00|1222,COIL-HR 0.095 (12ga) x 9 15/16 GRAY,,900.00|1223,COIL-HR 0.095 (12ga) x 9 15/16 GRAY,,900.00|1224,COIL-HR 0.095 (12ga) x 9 15/16 RED,,900.00|1225,COIL-HR 0.095 (12ga) x 9 15/16 RED,,900.00|1226,COIL-HR 0.095 (12ga) x 9 15/16 RED,,900.00|1246,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3336.00|1247,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1248,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1249,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1250,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1251,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1252,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1253,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1254,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1255,48\" 24ga Kynar Coil - Weathered Zinc,COIL4824-WZ,3333.00|1256,48\" 24ga Kynar Coil - Glacier White,COIL4824-GW,1167.00|1167,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443LS,5199.00|1168,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,5197.00|1169,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,5197.00|1170,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,5197.00|1171,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,4866.00|1172,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,4657.00|1173,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,5197.00|1174,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,5197.00|1175,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,4992.00|1182,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,3000.00|1183,COIL-SH 0.0236 (24ga) x 43 GR80 POLAR WHITE,COSH2443PW,1000.00|1184,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,9800.00|1158,COIL-SH 0.0185 (26ga) x 43 GR80 COUNTRY RED,COSH2643CR,0.00|1143,COIL-HR 0.095 (12ga) x 16 15/16 RED,,0.00|H2201,COIL-HR 0.095 (12ga) x 9 15/16 GALV,,2650.40|H2202,COIL-HR 0.095 (12ga) x 9 15/16 GALV,,2650.40|H2203,COIL-HR 0.095 (12ga) x 9 15/16 GRAY,,2650.40|H2204,COIL-HR 0.095 (12ga) x 9 15/16 GRAY,,2650.40|H2205,COIL-HR 0.095 (12ga) x 9 15/16 RED,,2650.40|H2206,COIL-HR 0.095 (12ga) x 9 15/16 RED,,2650.40|H2207,COIL-HR 0.095 (12ga) x 11 15/16 GALV,,2650.40|H2208,COIL-HR 0.095 (12ga) x 11 15/16 GALV,,2650.40|H2209,COIL-HR 0.095 (12ga) x 11 15/16 GRAY,,150.40|H2210,COIL-HR 0.095 (12ga) x 11 15/16 GRAY,,150.40|H2211,COIL-HR 0.095 (12ga) x 11 15/16 RED,,2650.40|H2212,COIL-HR 0.095 (12ga) x 11 15/16 RED,,2650.40|H2213,COIL-HR 0.095 (12ga) x 13 15/16 GALV,,2650.40|H2214,COIL-HR 0.095 (12ga) x 13 15/16 GALV,,2650.40|H2215,COIL-HR 0.095 (12ga) x 13 15/16 GRAY,,2650.40|H2216,COIL-HR 0.095 (12ga) x 13 15/16 GRAY,,2650.40|H2217,COIL-HR 0.095 (12ga) x 13 15/16 RED,,2650.40|H2218,COIL-HR 0.095 (12ga) x 13 15/16 RED,,2650.40|H2219,COIL-HR 0.095 (12ga) x 15 15/16 GALV,,2621.20|H2220,COIL-HR 0.095 (12ga) x 15 15/16 GALV,,2650.40|H2221,COIL-HR 0.095 (12ga) x 15 15/16 GRAY,,2650.40|H2222,COIL-HR 0.095 (12ga) x 15 15/16 GRAY,,2650.40|H2223,COIL-HR 0.095 (12ga) x 15 15/16 RED,,2650.40|H2224,COIL-HR 0.095 (12ga) x 15 15/16 RED,,2650.40|H2225,COIL-HR 0.095 (12ga) x 16 15/16 GALV,,2650.40|H2226,COIL-HR 0.095 (12ga) x 16 15/16 GALV,,2650.40|H2227,COIL-HR 0.095 (12ga) x 16 15/16 GRAY,,2650.40|H2228,COIL-HR 0.095 (12ga) x 16 15/16 GRAY,,2650.40|H2229,COIL-HR 0.095 (12ga) x 16 15/16 RED,,2650.40|H2230,COIL-HR 0.095 (12ga) x 16 15/16 RED,,2650.40|H2231,COIL-HR 0.095 (12ga) x 17 15/16 GALV,,2606.60|H2232,COIL-HR 0.095 (12ga) x 17 15/16 GALV,,2650.40|H2233,COIL-HR 0.095 (12ga) x 17 15/16 GRAY,,2650.40|H2234,COIL-HR 0.095 (12ga) x 17 15/16 GRAY,,2650.40|H2235,COIL-HR 0.095 (12ga) x 17 15/16 RED,,2650.40|H2236,COIL-HR 0.095 (12ga) x 17 15/16 RED,,2650.40|H2237,COIL-HR 0.071 (14ga) x 9 15/16 GALV,,2650.40|H2238,COIL-HR 0.071 (14ga) x 9 15/16 GALV,,2650.40|H2239,COIL-HR 0.071 (14ga) x 9 15/16 GRAY,,2650.40|H2240,COIL-HR 0.071 (14ga) x 9 15/16 GRAY,,2650.40|H2241,COIL-HR 0.071 (14ga) x 9 15/16 RED,,2650.40|H2242,COIL-HR 0.071 (14ga) x 9 15/16 RED,,2650.40|H2243,COIL-HR 0.071 (14ga) x 11 15/16 GALV,,2650.40|H2244,COIL-HR 0.071 (14ga) x 11 15/16 GALV,,2650.40|H2245,COIL-HR 0.071 (14ga) x 11 15/16 GRAY,,2650.40|H2246,COIL-HR 0.071 (14ga) x 11 15/16 GRAY,,2650.40|H2247,COIL-HR 0.071 (14ga) x 11 15/16 RED,,2650.40|H2248,COIL-HR 0.071 (14ga) x 11 15/16 RED,,2650.40|H2249,COIL-HR 0.071 (14ga) x 13 15/16 GALV,,2650.40|H2250,COIL-HR 0.071 (14ga) x 13 15/16 GALV,,2650.40|H2251,COIL-HR 0.071 (14ga) x 13 15/16 GRAY,,2650.40|H2252,COIL-HR 0.071 (14ga) x 13 15/16 GRAY,,2650.40|H2253,COIL-HR 0.071 (14ga) x 13 15/16 RED,,2650.40|H2254,COIL-HR 0.071 (14ga) x 13 15/16 RED,,2650.40|H2255,COIL-HR 0.071 (14ga) x 15 15/16 GALV,,2650.40|H2256,COIL-HR 0.071 (14ga) x 15 15/16 GALV,,2650.40|H2257,COIL-HR 0.071 (14ga) x 15 15/16 GRAY,,2526.20|H2258,COIL-HR 0.071 (14ga) x 15 15/16 GRAY,,2650.40|H2259,COIL-HR 0.071 (14ga) x 15 15/16 RED,,2650.40|H2260,COIL-HR 0.071 (14ga) x 15 15/16 RED,,2650.40|H2261,COIL-HR 0.071 (14ga) x 16 15/16 GALV,,2650.40|H2262,COIL-HR 0.071 (14ga) x 16 15/16 GALV,,2650.40|H2263,COIL-HR 0.071 (14ga) x 16 15/16 GRAY,,2486.20|H2264,COIL-HR 0.071 (14ga) x 16 15/16 GRAY,,2650.40|H2265,COIL-HR 0.071 (14ga) x 16 15/16 RED,,2650.40|H2266,COIL-HR 0.071 (14ga) x 16 15/16 RED,,2650.40|H2267,COIL-HR 0.071 (14ga) x 17 15/16 GALV,,2650.40|H2268,COIL-HR 0.071 (14ga) x 17 15/16 GALV,,2650.40|H2269,COIL-HR 0.071 (14ga) x 17 15/16 GRAY,,2650.40|H2270,COIL-HR 0.071 (14ga) x 17 15/16 GRAY,,2650.40|H2271,COIL-HR 0.071 (14ga) x 17 15/16 RED,,2650.40|H2272,COIL-HR 0.071 (14ga) x 17 15/16 RED,,2650.40|H2273,COIL-HR 0.054 (16ga) x 9 15/16 GALV,,2650.40|H2274,COIL-HR 0.054 (16ga) x 9 15/16 GALV,,2650.40|H2275,COIL-HR 0.054 (16ga) x 9 15/16 GRAY,,2650.40|H2276,COIL-HR 0.054 (16ga) x 9 15/16 GRAY,,2650.40|H2277,COIL-HR 0.054 (16ga) x 9 15/16 RED,,2650.40|H2278,COIL-HR 0.054 (16ga) x 9 15/16 RED,,2650.40|H2279,COIL-HR 0.054 (16ga) x 11 15/16 GALV,,2650.40|H2280,COIL-HR 0.054 (16ga) x 11 15/16 GALV,,2650.40|H2281,COIL-HR 0.054 (16ga) x 11 15/16 GRAY,,2650.40|H2282,COIL-HR 0.054 (16ga) x 11 15/16 GRAY,,2650.40|H2283,COIL-HR 0.054 (16ga) x 11 15/16 RED,,2650.40|H2284,COIL-HR 0.054 (16ga) x 11 15/16 RED,,2650.40|H2285,COIL-HR 0.054 (16ga) x 13 15/16 GALV,,2617.40|H2286,COIL-HR 0.054 (16ga) x 13 15/16 GALV,,2650.40|1176,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,5197.00|1177,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,5197.00|1178,COIL-SH 0.0236 (24ga) x 43 GR80 SADDLE TAN,COSH2443ST,5197.00|1181,COIL-SH 0.0236 (24ga) x 43 GR80 REGAL BLUE,COSH2443RB,3000.00"
                        }
                    }
                }
            },
            // Sub message 1.
            new MachineMessage
            {
                tags = new Tags
                {
                    set2 = new Set2
                    {
                        MqttSub = new MqttSub
                        {
                            MqttDest = "1",
                            MqttString = "10152,COSH2443LS,72623.00000,,6/10/2021 12:00:00 AM|10171,COIL4824-WZ,35526.00000,Not Notched-,9/2/2021 12:00:00 AM|10173,COIL4824-WZ,35526.00000,Not Notched-,9/2/2021 12:00:00 AM|10147,COSH2443LS,26922.00000,,6/8/2021 12:00:00 AM|10152,COSH2443LS,24033.00000,Notched-,6/10/2021 12:00:00 AM|10152,COSH2443LS,14268.00000,Notched-,6/10/2021 12:00:00 AM|10147,COSH2443LS,11379.00000,,6/8/2021 12:00:00 AM|10152,COSH2443LS,3979.00000,,6/10/2021 12:00:00 AM|10160,TestMat,2700.00000,,7/30/2021 12:00:00 AM|10144,COSH2443RB,1800.00000,,|10144,COSH2443RB,1800.00000,,6/8/2021 12:00:00 AM|10145,COSH2443RB,1800.00000,,6/8/2021 12:00:00 AM|10148,COSH2443RB,1800.00000,,6/10/2021 12:00:00 AM|10151,COSH2443CR,1440.00000,,6/10/2021 12:00:00 AM|10146,COSH2443RB,900.00000,,6/8/2021 12:00:00 AM|10146,COSH2443RB,900.00000,,6/8/2021 12:00:00 AM",
                            CoilDatAck = "TRUE",
                            CoilDatRecv = "Coil ID blank - Tag or inventory data invalid!|No order number selected! Select order number to verify coil data.",
                            CoilDatValidAck = "FALSE"
                        }
                    }
                }
            }
        };
    }
}