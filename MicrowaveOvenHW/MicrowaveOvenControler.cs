using System;
using System.Composition;
using System.ComponentModel;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenControler))]
    [Description("Controler")]
    public class MicrowaveOvenControler : MicrowaveOvenControlerBase
    {
        #region IMicrowaveOvenControler
        public override void OneSecondTick()
        {
            // decrease heating time if: not finished and the heater is on
            if (_hw.HeaterOn && _heatingTimeLeft > 0)
            {
                _heatingTimeLeft--;

                if (_heatingTimeLeft <= 0)
                {
                    // stop heating if the time is finished
                    _hw.TurnOffHeater();
                }
            }
        }
        #endregion

        protected override void InternalInitialize()
        {
            // intentionally left blank
        }

        protected override void HwOnStartButtonPressed(object sender, EventArgs eventArgs)
        {
            // US_4: When I press start button when door is open nothing happens.
            if (_hw.DoorOpen) return;

            if (_hw.HeaterOn)
            {
                // US_6: When I press start button when door is closed and already heating,
                //       increase remaining time with 1 minute.
                _heatingTimeLeft += DEFAULT_HEATING_TIME;
            }
            else
            {
                // US_5: When I press start button when door is closed,
                //       heater runs for 1 minute.
                _heatingTimeLeft = DEFAULT_HEATING_TIME;
                _hw.TurnOnHeater();
            }
        }

        protected override void HwOnDoorOpenChanged(bool b)
        {
            if (b == DOOR_OPEN)
            {
                // US_1: When I open door Light is on.
                // US_3: When I open door heater stops if running.
                _hw.TurnOffHeater();
                _hw.TurnOnLight();
            }
            else
            {// DOOR CLOSE
                if (_heatingTimeLeft > 0)
                {
                    // US_x: Undefined, assumed as auto-start if not finished before
                    _hw.TurnOnHeater();
                }
                // US_2: When I close door Light turns off.
                _hw.TurnOffLight();
            }
        }

        public override string ToString()
        {
            return "Controler";
        }
    }
}