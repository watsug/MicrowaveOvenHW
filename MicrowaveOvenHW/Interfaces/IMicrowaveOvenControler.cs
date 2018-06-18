namespace MicrowaveOvenHW.Interfaces
{
    /// <summary>
    /// Internal interface to the Microwave oven hardware
    /// </summary>
    public interface IMicrowaveOvenControler
    {
        void Initialize(IMicrowaveOvenHWEx hw);
        void OneSecondTick();
    }
}