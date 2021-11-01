namespace AscCutlistEditorTests.Common
{
    internal class Strings
    {
        // NOTE: Don't have access to the local database? Copy of test tables
        // can be found at the Kitsuragi server.
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