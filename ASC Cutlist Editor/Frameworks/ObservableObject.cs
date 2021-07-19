using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ASC_Cutlist_Editor.Frameworks
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