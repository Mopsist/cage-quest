using System;

namespace CageQuest.Core
{
    public static class Message
    {
        public static void Text(string text)
        {
            Console.WriteLine(text);
        }

        public static void Info(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void Controls(string text)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
