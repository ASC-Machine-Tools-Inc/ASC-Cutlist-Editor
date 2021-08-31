using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AscCutlistEditor.Frameworks;
using AscCutlistEditor.Models;
using AscCutlistEditor.Views;

namespace AscCutlistEditor.ViewModels.Bundles
{
    internal class BundleViewModel : ObservableObject
    {
        private ObservableCollection<SingleBundleControl> _bundles;

        public BundleViewModel()
        {
            Bundles = new ObservableCollection<SingleBundleControl>();
        }

        public ObservableCollection<SingleBundleControl> Bundles
        {
            get => _bundles;
            set
            {
                _bundles = value;
                RaisePropertyChangedEvent("Bundles");
            }
        }

        public async void CreateBundles(ObservableCollection<Cutlist> cutlists)
        {
            await CreateBundlesAsync(cutlists);
        }

        public void ClearUi()
        {
            Bundles = new ObservableCollection<SingleBundleControl>();
        }

        internal async Task CreateBundlesAsync(ObservableCollection<Cutlist> cutlists)
        {
            // Run on UI thread.
            // Select the correct dispatcher: if Application.Current is null,
            // we're running unit tests and should use CurrentDispatcher instead.
            Dispatcher dispatcher = Application.Current != null
                ? Application.Current.Dispatcher
                : Dispatcher.CurrentDispatcher;

            for (int i = 0; i < cutlists.Count; i++)
            {
                dispatcher.Invoke(() => { Bundles.Add(new SingleBundleControl()); });
            }
        }
    }
}