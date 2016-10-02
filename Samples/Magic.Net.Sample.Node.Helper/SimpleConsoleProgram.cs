using System;
using JetBrains.Annotations;

namespace Magic.Net.Sample.Node
{
    public abstract class SimpleConsoleProgram
    {
        [PublicAPI]
        public static void Run(string[] args, SimpleConsoleProgram program)
        {
            program.Start(args);
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press Enter to exit");
            Console.ResetColor();

            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(false);

            } while (key.Key != ConsoleKey.Enter);

            program.Stop(args);
        }

        protected abstract void Start(string[] args);

        protected abstract void Stop(string[] args);
    }
}
