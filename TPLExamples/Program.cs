using System;
using System.Threading.Tasks;

namespace TPLExamples
{
    class Program
    {
        static void Main()
        {
            var retry = new Retry();
            var someValue = "";
            var cancellationToken = new System.Threading.CancellationToken();
            var task = retry.HandleException<OutOfMemoryException>()
                 .SuccessChecker(() =>
                     {
                         Console.WriteLine("Checking...");
                         return someValue != "Hello";
                     }
                 )
                 .AttemptsExhausted(() => Console.WriteLine("Failed"))
                 .CancellationToken(cancellationToken)
                 .Wait(1)
                 .RunAsync(() =>
                 {
                     someValue = Run(1, "");
                 }
             , 10);

            task.Wait();
            Console.ReadLine();
        }

        public static string Run(int a, string b)
        {
            //throw new Exception();
            return "Hello";
        }
    }
}
