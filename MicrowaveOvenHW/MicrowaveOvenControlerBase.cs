using System;
using System.ComponentModel;
using System.Linq;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW
{
    public abstract class MicrowaveOvenControlerBase : IMicrowaveOvenControler
    {
        #region public consts
        public const int DEFAULT_HEATING_TIME = 60;
        public const bool DOOR_OPEN = true;
        #endregion

        #region private members
        protected IMicrowaveOvenHWEx _hw;
        protected int _heatingTimeLeft = 0;
        #endregion

        #region IMicrowaveOvenControler
        public void Initialize(IMicrowaveOvenHWEx hw)
        {
            _hw = hw;

            // ensure initial state
            _hw.TurnOffHeater();
            _hw.TurnOffLight();

            InternalInitialize();

            // subscribe HW events
            _hw.DoorOpenChanged += HwOnDoorOpenChanged;
            _hw.StartButtonPressed += HwOnStartButtonPressed;
        }

        public abstract void OneSecondTick();
        #endregion

        protected abstract void InternalInitialize();
        protected abstract void HwOnStartButtonPressed(object sender, EventArgs eventArgs);
        protected abstract void HwOnDoorOpenChanged(bool b);

        public override string ToString()
        {
            object[] ca = GetType().GetCustomAttributes(false);
            return ca
                .SingleOrDefault((v) => v is DescriptionAttribute) is DescriptionAttribute desc
                ? desc.Description : base.ToString();
        }
    }
}