namespace MicrowaveOvenHW.Interfaces
{
    /// <summary>
    /// Extension to the Interface to the Microwave oven hardware
    /// </summary>
    public interface IMicrowaveOvenHWEx : IMicrowaveOvenHW
    {
        /// <summary>
        /// This is to enforce initial state
        /// </summary>
        void Initialize();

        /// <summary>
        /// Indicates if the light in the Microwave oven is on or off
        /// </summary>
        bool LightOn { get; }

        /// <summary>
        /// Indicates if the heater in the Microwave oven is on or off
        /// </summary>
        bool HeaterOn { get; }

        /// <summary>
        /// Turns on the Microwave light
        /// </summary>
        void TurnOnLight();

        /// <summary>
        /// Turns off the Microwave light
        /// </summary>
        void TurnOffLight();
    }
}