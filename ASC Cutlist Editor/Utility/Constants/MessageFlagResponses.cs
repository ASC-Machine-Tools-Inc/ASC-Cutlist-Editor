using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace AscCutlistEditor.Utility.Constants
{
    // Used in MessageFlagHandlers for returning responses to the operator.
    internal static class MessageFlagResponses
    {
        public static readonly string Requested = (string)Application.Current.Resources["Requested"];

        public static readonly string NotRequested = (string)Application.Current.Resources["NotRequested"];
    }
}