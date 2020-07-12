using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SynchronizationContexts
{
    /// <summary>
    /// This <see cref="SynchronizationContext"/> will call all posted callbacks in a single new thread.
    /// </summary>
    public class SingleNewThreadSynchronizationContext : SynchronizationContext
    {
        readonly Thread _workerThread;
        readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>> _actionStatePairs = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

        /// <summary>
        /// Returns the Id of the worker <see cref="Thread"/> created by this <see cref="SynchronizationContext"/>.
        /// </summary>
        public int ManagedThreadId => _workerThread.ManagedThreadId;

        public SingleNewThreadSynchronizationContext()
        {
            // Creates a new thread to run the posted calls.
            _workerThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        var actionStatePair = _actionStatePairs.Take();
                        SetSynchronizationContext(this);
                        actionStatePair.Key?.Invoke(actionStatePair.Value);
                    }
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine($"The thread {_workerThread.ManagedThreadId} of {nameof(SingleNewThreadSynchronizationContext)} was aborted.");
                }
            });

            _workerThread.IsBackground = true;
            _workerThread.Start();
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            // Queues the posted callbacks to be called in this SynchronizationContext.
            _actionStatePairs.Add(new KeyValuePair<SendOrPostCallback, object>(d, state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotSupportedException();
        }

        public override void OperationCompleted()
        {
            _actionStatePairs.Add(new KeyValuePair<SendOrPostCallback, object>(new SendOrPostCallback(_ => _workerThread.Abort()), null));
            _actionStatePairs.CompleteAdding();
        }
    }
}
