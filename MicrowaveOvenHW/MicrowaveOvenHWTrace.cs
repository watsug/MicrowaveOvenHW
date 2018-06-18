using System.Composition;
using System.Diagnostics;
using System.ComponentModel;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenHWEx))]
    [Description("HW Trace")]
    public class MicrowaveOvenHWTrace : MicrowaveOvenHW
    {
        public override void TurnOnHeater()
        {
            Debug.WriteLine("The heater has been turned on");
            base.TurnOnHeater();
        }

        public override void TurnOffHeater()
        {
            Debug.WriteLine("The heater has been turned of");
            base.TurnOffHeater();
        }

        public override void TurnOnLight()
        {
            Debug.WriteLine("The light has been turned on");
            base.TurnOnLight();
        }

        public override void TurnOffLight()
        {
            Debug.WriteLine("The light has been turned off");
            base.TurnOffLight();
        }

        public override void OpenDoor()
        {
            Debug.WriteLine("The door has been opened");
            base.OpenDoor();
        }

        public override void CloseDoor()
        {
            Debug.WriteLine("The door has been closed");
            base.CloseDoor();
        }

        public override void PressStartButton()
        {
            Debug.WriteLine("The start button has been pressed");
            base.PressStartButton();
        }

        public override string ToString()
        {
            return "HW Trace";
        }
    }
}