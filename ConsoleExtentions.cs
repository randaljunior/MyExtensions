namespace MyExtensions;

public static class ConsoleExtentions
{
    extension (Console)
    {
        public static void WriteLine(ReadOnlySpan<char> message)
        {
            Console.Out.WriteLine(message);
        }

        public static void Write(ReadOnlySpan<char> message)
        {
            Console.Out.Write(message);
        }

        public static void WriteColor(ReadOnlySpan<char> message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Out.Write(message);
            Console.ResetColor();
        }

        public static void WriteLineColor(ReadOnlySpan<char> message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Out.WriteLine(message);
            Console.ResetColor();
        }

        public static void WriteColor(string[] message, ConsoleColor?[] color)
        {
            for (int i = 0; i < message.Length; i++)
            {
                var _cor = (i >= color.Length) ? null : color[i];

                if (_cor is null)
                {
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = _cor ?? 0;
                }

                Console.Out.Write(message[i]);
            }

            Console.ResetColor();
        }

        public static void WriteAboveLine(ReadOnlySpan<char> message)
        {
            var (_left, _top) = Console.GetCursorPosition();
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(" ".Repeat(_left));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(message);
        }

        public static string ReadKey(out string key)
        {
            var keyInfo = Console.ReadKey(true);
            key = keyInfo.KeyChar.ToString();
            return keyInfo.KeyChar.ToString();
        }

    }

}
