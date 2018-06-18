using System;
using System.Composition;
using System.ComponentModel;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenHWEx))]
    [Description("HW")]
    public class MicrowaveOvenHW : IMicrowaveOvenHWEx, IMicrowaveOvenUI
    {
        #region private members
        private bool _doorOpened = false;
        private bool _heaterOn = false;
        private bool _lightOn = false;
        #endregion


        #region IMicrowaveOvenHW
        public bool DoorOpen => _doorOpened;

        public event Action<bool> DoorOpenChanged;
        public event EventHandler StartButtonPressed;

        public virtual void TurnOnHeater()
        {
            _heaterOn = true;
        }

        public virtual void TurnOffHeater()
        {
            _heaterOn = false;
        }
        #endregion

        #region IMicrowaveOvenHWEx

        public void Initialize()
        {
            _doorOpened = false;
            _heaterOn = false;
            _lightOn = false;
        }

        public bool LightOn => _lightOn;
        public bool HeaterOn => _heaterOn;

        public virtual void TurnOnLight()
        {
            _lightOn = true;
        }

        public virtual void TurnOffLight()
        {
            _lightOn = false;
        }
        #endregion

        #region IMicrowaveOvenUI
        public virtual void OpenDoor()
        {
            _doorOpened = true;
            DoorOpenChanged?.Invoke(true);
        }

        public virtual void CloseDoor()
        {
            _doorOpened = false;
            DoorOpenChanged?.Invoke(false);
        }

        public virtual void PressStartButton()
        {
            StartButtonPressed?.Invoke(this, new EventArgs());
        }
        #endregion

        public override string ToString()
        {
            return "HW";
        }
    }
}