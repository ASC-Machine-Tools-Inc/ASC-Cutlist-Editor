using System.ComponentModel;

namespace AscCutlistEditor.Frameworks
{
    // Base class for ViewModels that implements the INotifyPropertyChanged interface.
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}