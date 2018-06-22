namespace MicrowaveOvenHW.Core
{
    public class TriggerObject<T>
    {
        public TriggerObject(T t)
        {
            Trigger = t;
        }

        public bool Processed { get; protected set; } = false;
        public T Trigger { get; protected set; }

        public void SetProcessed()
        {
            Processed = true;
        }
    }
}