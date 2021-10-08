﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using AscCutlistEditor.ViewModels.MQTT;
using MQTTnet.Client;

namespace AscCutlistEditor.Models.MQTT
{
    /// <summary>
    /// Model for a single machine connection.
    /// </summary>
    internal class MachineConnection
    {
        public IMqttClient Client;

        /// <summary>
        /// The topic to listen for messages from.
        /// </summary>
        public string SubTopic;

        /// <summary>
        /// The topic to publish response messages to.
        /// </summary>
        public string PubTopic;

        public SqlConnectionViewModel SqlConnection;

        public ObservableCollection<MachineMessage> MachineMessageCollection;

        public MachineConnection(
            IMqttClient client,
            string subTopic,
            string pubTopic,
            SqlConnectionViewModel sqlConnection)
        {
            Client = client;
            SubTopic = subTopic;
            PubTopic = pubTopic;
            SqlConnection = sqlConnection;
            MachineMessageCollection = new ObservableCollection<MachineMessage>();
        }
    }
}