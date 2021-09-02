using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Views.MQTT;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace AscCutlistEditor.ViewModels.MQTT
{
    internal class MachineConnectionsViewModel : ObservableObject
    {
        private ObservableCollection<TabItem> _machineConnectionTabs = new ObservableCollection<TabItem>();

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