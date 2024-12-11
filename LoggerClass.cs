namespace Synapse.OrdersExample
{
    public class LoggerClass : ILoggerClass
    {
        public void Write(string message)
        {
            // 'Append' mode set to 'true' so that the log doesn't overwrite each write
            using (StreamWriter file = new("..\\..\\..\\Log.txt", true))
            {
                file.WriteLine(DateTime.Now + "   " + message + "\n\n");
            }
        }
    }
}


