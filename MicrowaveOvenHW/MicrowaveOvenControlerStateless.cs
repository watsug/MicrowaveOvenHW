using System;
using System.Composition;
using System.ComponentModel;
using Stateless;

using MicrowaveOvenHW.Enums;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenControler))]
    [Description("Stateless")]
    public class MicrowaveOvenControler2 : MicrowaveOvenControlerBase
    {
        #region private members
        private StateMachine<MicrowaveOwenState, MicrowaveOvenTriggers> _sm;
        #endregion

        #region IMicrowaveOvenControler
        public override void OneSecondTick()
        {
            _sm.Fire(MicrowaveOvenTriggers.OneSecondTick);
            if (_sm.State == MicrowaveOwenState.Started)
            {
                _heatingTimeLeft--;
                if (_heatingTimeLeft <= 0)
                {
                    _sm.Fire(MicrowaveOvenTriggers.Finished);
                }
            }
        }
        #endregion

        protected override void HwOnDoorOpenChanged(bool b)
        {
            _sm.Fire(b == DOOR_OPEN ?
                MicrowaveOvenTriggers.DoorOpen
                : MicrowaveOvenTriggers.DoorClose);
        }

        protected override void HwOnStartButtonPressed(object sender, EventArgs eventArgs)
        {
            _sm.Fire(MicrowaveOvenTriggers.ButtonPress);
            if (_sm.State == MicrowaveOwenState.Started)
            {
                _heatingTimeLeft += DEFAULT_HEATING_TIME;
            }
        }

        #region private methods
        protected override void InternalInitialize()
        {
            _sm = new StateMachine<MicrowaveOwenState, MicrowaveOvenTriggers>(MicrowaveOwenState.Stopped);

            _sm.Configure(MicrowaveOwenState.Stopped)
                .Ignore(MicrowaveOvenTriggers.OneSecondTick)
                .Ignore(MicrowaveOvenTriggers.DoorClose)
                .Permit(MicrowaveOvenTriggers.DoorOpen, MicrowaveOwenState.DoorOpenedStopped)
                .Permit(MicrowaveOvenTriggers.ButtonPress, MicrowaveOwenState.Started)
                .OnEntry(() => _hw.TurnOffHeater());

            _sm.Configure(MicrowaveOwenState.Started)
                .Ignore(MicrowaveOvenTriggers.DoorClose)
                .PermitReentry(MicrowaveOvenTriggers.ButtonPress)
                .PermitReentry(MicrowaveOvenTriggers.OneSecondTick)
                .Permit(MicrowaveOvenTriggers.Finished, MicrowaveOwenState.Stopped)
                .Permit(MicrowaveOvenTriggers.DoorOpen, MicrowaveOwenState.DoorOpenedStarted)
                .OnEntry(() => _hw.TurnOnHeater())
                .OnExit(() => _hw.TurnOffHeater());

            _sm.Configure(MicrowaveOwenState.DoorOpenedStopped)
                .Ignore(MicrowaveOvenTriggers.OneSecondTick)
                .Ignore(MicrowaveOvenTriggers.ButtonPress)
                .Ignore(MicrowaveOvenTriggers.DoorOpen)
                .Permit(MicrowaveOvenTriggers.DoorClose, MicrowaveOwenState.Stopped)
                .OnEntry(() => _hw.TurnOnLight())
                .OnExit(() => _hw.TurnOffLight());

            _sm.Configure(MicrowaveOwenState.DoorOpenedStarted)
                .Ignore(MicrowaveOvenTriggers.OneSecondTick)
                .Ignore(MicrowaveOvenTriggers.ButtonPress)
                .Ignore(MicrowaveOvenTriggers.DoorOpen)
                .Permit(MicrowaveOvenTriggers.DoorClose, MicrowaveOwenState.Started)
                .OnEntry(() => _hw.TurnOnLight())
                .OnExit(() => _hw.TurnOffLight());
        }
        #endregion
    }
}