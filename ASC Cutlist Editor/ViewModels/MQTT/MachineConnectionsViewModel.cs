using System;
using System.Collections.Generic;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.MQTT;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AscCutlistEditor.ViewModels.MQTT
{
    /// <summary>
    /// Handles listening for new machines on alphapub and creating uptime
    /// status tabs for them.
    /// </summary>
    internal class MachineConnectionsViewModel : ObservableObject
    {
        private ObservableCollection<TabItem> _machineConnectionTabs = new ObservableCollection<TabItem>();

        private HashSet<string> _knownConnections = new HashSet<string>();
        // TODO: create a listener to call AddTab when a new number is detected (not in known connections)

        public ObservableCollection<TabItem> MachineConnectionTabs
        {
            get => _machineConnectionTabs;
            set
            {
                _machineConnectionTabs = value;
                RaisePropertyChangedEvent("MachineConnectionTabs");
            }
        }

        public void AddTab()
        {
            MachineDataViewModel model = new MachineDataViewModel();

            MachineConnectionTabs.Add(new TabItem
            {
                Header = "Test",
                Content = new UptimeControl(),
                DataContext = model
            });
        }
    }
}