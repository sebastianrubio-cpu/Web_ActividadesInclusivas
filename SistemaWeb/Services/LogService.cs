namespace SistemaWeb.Services
{
    public interface ILogService
    {
        void Log(string message);
    }

    public class LogService : ILogService
    {
        private readonly string _filePath;

        public LogService()
        {
            // Creates a log file in the app root
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "sistema_log.txt");
        }

        public void Log(string message)
        {
            var logEntry = $"{DateTime.Now}: {message}{Environment.NewLine}";
            // AppendAllText is a quick way to write to a file
            File.AppendAllText(_filePath, logEntry);
        }
    }
}