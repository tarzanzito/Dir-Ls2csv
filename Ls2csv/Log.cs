using System;

namespace Candal
{
    internal static class Log
    {
        public static void Information(string message)
        {
            Console.WriteLine($"Info:{ message}");
        }

        public static void Error(string message)
        {
            Console.WriteLine($"Error:{message}");
        }

    }
}
