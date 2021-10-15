using System;
using System.Collections.Generic;
using System.Text;

namespace AscCutlistEditor.Frameworks
{
    public interface ISettings
    {
        string DataSource { get; set; }

        string DatabaseName { get; set; }

        string Username { get; set; }

        string Password { get; set; }
    }
}