using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ASC_Cutlist_Editor.Frameworks
{
    // Base class for binding commands in the view to that implements the ICommand interface.
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action();
        }

        // Always lets buttons/menu items be clicked instead of grayed out, so
        // we can say why that action can't be carried out instead.
        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67 // Ignore "event never used" warning

        public event EventHandler CanExecuteChanged;

#pragma warning restore 67
    }
}