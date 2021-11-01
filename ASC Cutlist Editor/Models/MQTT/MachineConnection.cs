using AscCutlistEditor.ViewModels.MQTT;
using MQTTnet.Client;
using System.Collections.ObjectModel;
using AscCutlistEditor.Frameworks;

namespace AscCutlistEditor.Models.MQTT
{
    /// <summary>
    /// Model for a single machine connection.
    /// </summary>
    public class MachineConnection : ObservableObject
    {
        public IMqttClient Client;

        /// <summary>
        /// The topic to listen for messages from.
        /// </summary>
        public string SubTopic { get; set; }

        /// <summary>
        /// The topic to publish response messages to.
        /// </summary>
        public string PubTopic { get; set; }

        /// <summary>
        /// The topic to display for the tab.
        /// </summary>
        public string DisplayTopic { get; set; }

        /// <summary>
        /// List of sent messages.
        /// </summary>
        public ObservableCollection<MachineMessage> MachineMessagePubCollection { get; set; }

        /// <summary>
        /// List of received messages.
        /// </summary>
        public ObservableCollection<MachineMessage> MachineMessageSubCollection { get; set; }

        public SqlConnectionViewModel SqlConnection;

        public MachineConnection(
            IMqttClient client,
            string subTopic,
            string pubTopic,
            string displayTopic,
            SqlConnectionViewModel sqlConnection)
        {
            Client = client;
            SubTopic = subTopic;
            PubTopic = pubTopic;
            DisplayTopic = displayTopic;
            SqlConnection = sqlConnection;
            MachineMessagePubCollection = new ObservableCollection<MachineMessage>();
            MachineMessageSubCollection = new ObservableCollection<MachineMessage>();
        }
    }
}