using System;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronizationContexts
{
    class Program
    {
        static void SingleNewThreadSynchronizationContextDemo()
        {
            var synchronizationContext = new SingleNewThreadSynchronizationContext();

            // Creates some tasks to test that the whole calls in the tasks (before and after awaiting) will be called in the same thread.
            for (int i = 0; i < 20; i++)
                Task.Run(async () =>
                {
                    SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                    // Before yielding, the task will be started in some thread-pool thread.
                    var threadIdBeforeYield = Thread.CurrentThread.ManagedThreadId;
                    // We yield to post the rest of the task after await to the SynchronizationContext.
                    // Other possiblity here is maybe to start the whole Task using a different TaskScheduler.
                    await Task.Yield();

                    var threadIdBeforeAwait1 = Thread.CurrentThread.ManagedThreadId;
                    await Task.Delay(100);
                    var threadIdBeforeAwait2 = Thread.CurrentThread.ManagedThreadId;
                    await Task.Delay(100);

                    Console.WriteLine($"SynchronizationContext: thread Id '{synchronizationContext.ManagedThreadId}' | type '{SynchronizationContext.Current?.GetType()}.'");
                    Console.WriteLine($"Thread Ids: Before yield '{threadIdBeforeYield}' | Before await1 '{threadIdBeforeAwait1}' | Before await2 '{threadIdBeforeAwait2}' | After last await '{Thread.CurrentThread.ManagedThreadId}'.{Environment.NewLine}");
                });
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Entry thread {Thread.CurrentThread.ManagedThreadId}");
            SingleNewThreadSynchronizationContextDemo();
            Console.WriteLine($"Exit thread {Thread.CurrentThread.ManagedThreadId}");

            Console.ReadLine();
        }
    }
}
