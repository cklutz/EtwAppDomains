using System;

namespace EtwAppDomains
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    Console.Error.WriteLine("Usage: {0} PID [TIMEOUT]",
                        typeof(Program).Assembly.GetName().Name);
                    return 1;
                }

                int processId = int.Parse(args[0]);
                var timeout = TimeSpan.Zero;
                if (args.Length > 1)
                {
                    timeout = TimeSpan.FromSeconds(int.Parse(args[1]));
                }

                foreach (var entry in AppDomainHelper.GetAppDomains(processId, timeout))
                {
                    Console.WriteLine("{0} (Id: {1}, Index: {2})", entry.Name, entry.Id, entry.Index);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return ex.HResult;
            }

            return 0;
        }
    }
}