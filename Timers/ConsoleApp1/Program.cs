using System;
using System.Threading;
using System.Timers;

namespace ConsoleApp1
{
    class Program
    {
        // This is the synchronization point that prevents events
        // from running concurrently
        private static int syncPoint = 0;

        private static int numEvents = 0;
        private static int numExecuted = 0;
        private static int numSkipped = 0;

        private static System.Timers.Timer _myTimer;

        public static void Main(string[] args)
        {
            SetTimer();
            Console.Read();
        }

        public static void SetTimer()
        {
            // Set syncPoint to zero before starting the timer
            
            syncPoint = 0;
            _myTimer = new System.Timers.Timer(0.5 * 1000);
            _myTimer.Elapsed += HandleElapsed;
            _myTimer.AutoReset = true;
            _myTimer.Enabled = true;
        }

        private static void HandleElapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine($"Inside handle Events. NumEvents so Far: {numEvents}");

            if (numEvents >= 100)
            {
                _myTimer.Stop(); //For testing result purposes Just stopping the timer after 100 Elapsed Events
                Console.WriteLine("{0} events were raised.", numEvents);
                Console.WriteLine("{0} events executed.", numExecuted);
                Console.WriteLine("{0} events were skipped for concurrency.", numSkipped);
                Console.Read();
            }

            numEvents += 1;

            // This example assumes that overlapping events can be
            // discarded. That is, if an Elapsed event is raised before 
            // the previous event is finished processing, the second
            // event is ignored. 
            //
            // CompareExchange is used to take control of syncPoint, 
            // and to determine whether the attempt was successful. 
            // CompareExchange attempts to put 1 into syncPoint, but
            // only if the current value of syncPoint is zero 
            // (specified by the third parameter). If another thread
            // has set syncPoint to 1, or if the control thread has
            // set syncPoint to -1, the current event is skipped. 
            // (Normally it would not be necessary to use a local 
            // variable for the return value. A local variable is 
            // used here to determine the reason the event was 
            // skipped.)
            //
            int sync = Interlocked.CompareExchange(ref syncPoint, 1, 0);
            if (sync == 0)
            {
                // No other event was executing.
                // The event handler simulates an amount of work
                // lasting 2 seconds
                // some events will overlap.
                Console.WriteLine("Inside long running Task on the Timer Thread");
                Thread.Sleep(2 * 1000);
                numExecuted += 1;

                // Release control of syncPoint.
                syncPoint = 0;
            }
            else
            {
                if (sync == 1) { numSkipped += 1; } 
            }
        }
    }
}
