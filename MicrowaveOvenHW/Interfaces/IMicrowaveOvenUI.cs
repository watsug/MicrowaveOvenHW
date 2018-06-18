namespace MicrowaveOvenHW.Interfaces
{
    /// <summary>
    /// User interface abstraction over MicrowaveOvenHW
    /// </summary>
    public interface IMicrowaveOvenUI
    {
        void OpenDoor();
        void CloseDoor();
        void PressStartButton();
    }
}