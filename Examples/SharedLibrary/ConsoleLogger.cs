using System;

namespace Examples.SharedLibrary
{
    public class ConsoleLogger
    {
        public void Debug(string message)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = orig;
        }

        public void Info(string message)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ForegroundColor = orig;
        }

        public void Info(string message, params object[] input)
        {
            Info(string.Format(message, input));
        }

        public void Warn(string message)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ForegroundColor = orig;
        }

        public void Warn(Exception exception)
        {
            Warn(exception.ToString());
        }

        public void Warn(string message, Exception exception)
        {
            Warn(string.Format("{0}: {1}", message, exception));
        }

        public void Warn(string message, Exception exception, Exception innerException)
        {
            Warn(string.Format("WARNING: {0}{3}Error: {1}{3}InnerException:{2}", message, exception, innerException,
                Environment.NewLine));
        }

        public void Error(string message)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = orig;
        }

        public void Error(Exception exception)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(exception);
            Console.ForegroundColor = orig;
        }

        public void Error(string message, Exception exception)
        {
            Error(string.Format("{0}: {1}", message, exception));
        }

        public void Error(string message, Exception exception, Exception innerException)
        {
            Error(string.Format("ERROR: {0}{3}Error: {1}{3}InnerException:{2}", message, exception, innerException,
                Environment.NewLine));
        }

        public void Fatal(string message)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(message);
            Console.ForegroundColor = orig;
        }

        public void Fatal(Exception exception)
        {
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(exception);
            Console.ForegroundColor = orig;
        }

        public void Fatal(string message, Exception exception)
        {
            Fatal(string.Format("{0}: {1}", message, exception));
        }

        public void Fatal(string message, Exception exception, Exception innerException)
        {
            Fatal(string.Format("FATAL: {0}{3}Error: {1}{3}InnerException:{2}", message, exception, innerException,
                Environment.NewLine));
        }
    }
}