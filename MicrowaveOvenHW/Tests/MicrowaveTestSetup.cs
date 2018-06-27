using MicrowaveOvenHW.Interfaces;
using NUnit.Framework;
using System;

namespace MicrowaveOvenHW.Tests
{
    public class MicrowaveTestSetup : IDisposable
    {
        public IMicrowaveOvenControler Ctrl { get; private set; }
        public IMicrowaveOvenHWEx Hw { get; private set; }
        public IMicrowaveOvenUI Ui => Hw as IMicrowaveOvenUI;

        public MicrowaveTestSetup(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            Ctrl = ctrl;
            Hw = hw;

            Hw.Initialize();
            Ctrl.Initialize(Hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(Hw.HeaterOn);
            Assert.IsFalse(Hw.DoorOpen);
            Assert.IsFalse(Hw.LightOn);
        }

        public void Dispose()
        {
            if (Ctrl is IDisposable ctrlDisp)
            {
                ctrlDisp.Dispose();
            }
            Ctrl = null;
            if (Hw is IDisposable hwDisp)
            {
                hwDisp.Dispose();
            }
            Hw = null;
        }
    }
}