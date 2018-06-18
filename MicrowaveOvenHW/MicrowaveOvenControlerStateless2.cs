using System;
using System.Composition;
using System.ComponentModel;
using Stateless;

using MicrowaveOvenHW.Enums;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenControler))]
    [Description("Stateless2")]
    public class MicrowaveOvenControlerStateless2 : MicrowaveOvenControlerBase
    {
        #region private members
        private StateMachine<MicrowaveOwenState, MicrowaveOvenTriggers> _sm;
        #endregion

        #region IMicrowaveOvenControler
        public override void OneSecondTick()
        {
            _sm.Fire(MicrowaveOvenTriggers.OneSecondTick);
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
        }

        #region private methods
        protected override void InternalInitialize()
        {
            _sm = new StateMachine<MicrowaveOwenState, MicrowaveOvenTriggers>(MicrowaveOwenState.Stopped);

            _sm.Configure(MicrowaveOwenState.Stopped)
                .Ignore(MicrowaveOvenTriggers.OneSecondTick)
                .Ignore(MicrowaveOvenTriggers.DoorClose)
                .Permit(MicrowaveOvenTriggers.DoorOpen, MicrowaveOwenState.DoorOpenedStopped)
                .Permit(MicrowaveOvenTriggers.ButtonPress, MicrowaveOwenState.StoppedButtonPressed)
                .OnEntry(() => _hw.TurnOffHeater());

            _sm.Configure(MicrowaveOwenState.Started)
                .Ignore(MicrowaveOvenTriggers.DoorClose)
                .Permit(MicrowaveOvenTriggers.ButtonPress, MicrowaveOwenState.StartedButtonPressed)
                .Permit(MicrowaveOvenTriggers.OneSecondTick, MicrowaveOwenState.StartedOneSecondTick)
                .Permit(MicrowaveOvenTriggers.Finished, MicrowaveOwenState.Stopped)
                .Permit(MicrowaveOvenTriggers.DoorOpen, MicrowaveOwenState.DoorOpenedStarted)
                .OnEntryFrom(MicrowaveOvenTriggers.OneSecondTickProcessed, () =>
                    {
                        if (_heatingTimeLeft <= 0) _sm.Fire(MicrowaveOvenTriggers.Finished);
                    })
                .OnEntry(() => _hw.TurnOnHeater())
                .OnExit(() => _hw.TurnOffHeater());

            _sm.Configure(MicrowaveOwenState.StartedButtonPressed)
                .Permit(MicrowaveOvenTriggers.ButtonPressProcessed, MicrowaveOwenState.Started)
                .OnEntryFrom(MicrowaveOvenTriggers.ButtonPress, () =>
                {
                    _heatingTimeLeft += DEFAULT_HEATING_TIME;
                    _sm.Fire(MicrowaveOvenTriggers.ButtonPressProcessed);
                });

            _sm.Configure(MicrowaveOwenState.StoppedButtonPressed)
                .Permit(MicrowaveOvenTriggers.ButtonPressProcessed, MicrowaveOwenState.Started)
                .OnEntryFrom(MicrowaveOvenTriggers.ButtonPress, () =>
                {
                    _heatingTimeLeft = DEFAULT_HEATING_TIME;
                    _sm.Fire(MicrowaveOvenTriggers.ButtonPressProcessed);
                });

            _sm.Configure(MicrowaveOwenState.StartedOneSecondTick)
                .Permit(MicrowaveOvenTriggers.OneSecondTickProcessed, MicrowaveOwenState.Started)
                .OnEntryFrom(MicrowaveOvenTriggers.OneSecondTick, () =>
                {
                    _heatingTimeLeft--;
                    _sm.Fire(MicrowaveOvenTriggers.OneSecondTickProcessed);
                });

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