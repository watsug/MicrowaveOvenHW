using System;
using System.Threading;

namespace MicrowaveOvenHW.Core
{
    public class TriggerObject<T>
    {
        private const int TIME_OUT = 10000;
        private AutoResetEvent processedEvent = new AutoResetEvent(false);

        public TriggerObject(T t)
        {
            Trigger = t;
        }

        public void WaitUntilProcessed()
        {
            if (!processedEvent.WaitOne(TIME_OUT))
            {
                throw new TimeoutException("Trigger processing timeout!");
            }
        }

        public T Trigger { get; protected set; }

        public void SetProcessed()
        {
            processedEvent.Set();
        }
    }
}