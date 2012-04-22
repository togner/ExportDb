using System;

namespace ExportDb
{
    class Program
    {
        static void Main(string[] args)
        {
            Arguments _arguments = new Arguments(args);
            try
            {
                if (_arguments["?"] != null)
                {
                    Program.PrintHelp();
                    return;
                }
            }
            catch (Exception _e)
            {
                Console.WriteLine("Exception caught in Main()");
                Console.WriteLine("---------------------------------------");
                while (_e != null)
                {
                    Console.WriteLine(_e.Message);
                    Console.WriteLine();
                    Console.WriteLine(_e.GetType().FullName);
                    Console.WriteLine();
                    Console.WriteLine(_e.StackTrace);
                    Console.WriteLine("---------------------------------------");
                    _e = _e.InnerException;
                }
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine(
@"ExportDb.exe usage:

");
        }
    }
}
