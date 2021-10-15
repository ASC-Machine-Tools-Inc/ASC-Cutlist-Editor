using System;
using System.Collections.Generic;
using System.Text;

namespace AscCutlistEditorTests.Common
{
    internal class Strings
    {
        /// <summary>
        /// Connection string for local testing database.
        /// </summary>
        public static string ConnectionString =
            "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\" +
            "Users\\PTON\\source\\repos\\ASC Cutlist Editor\\" +
            "AscCutlistEditorTests\\Assets\\cutlistEditorTestDb.mdf\";" +
            "Integrated Security=True;Connect Timeout=10";
    }
}