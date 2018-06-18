using System.Linq;
using System.Collections.Generic;
using System.Composition.Hosting;
using NUnit.Framework;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using MicrowaveOvenHW.Interfaces;

namespace MicrowaveOvenHW.Tests
{
    [TestFixture]
    public class MicrowaveOvenTests
    {
        public static IEnumerable<TestCaseData> GetImplementations()
        {
            ContainerConfiguration configuration = new ContainerConfiguration().WithAssembly(typeof(MicrowaveOvenTests).Assembly);
            var ctrls = configuration.CreateContainer().GetExports<IMicrowaveOvenControler>();

            List<TestCaseData> ret = new List<TestCaseData>();
            foreach (var ctrl in ctrls)
            {
                var hws = configuration.CreateContainer().GetExports<IMicrowaveOvenHWEx>();
                foreach (var hw in hws)
                {
                    ret.Add(new TestCaseData(ctrl, hw)
                        .SetDescription(GetDescription(ctrl) + ": " + GetDescription(hw)));
                }
            }
            return ret;
        }

        [Test(Description = "US_1: When I open door Light is on.")]
        [TestCaseSource("GetImplementations")]
        public void US1_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();

            Assert.IsTrue(hw.LightOn, "ensure the light is on after door closing");
        }

        [Test(Description = "US_2: When I close door Light turns off.")]
        [TestCaseSource("GetImplementations")]
        public void US2_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();
            Assert.IsTrue(hw.LightOn, "ensure the light is on after door closing");

            // simulate door close
            ((IMicrowaveOvenUI)hw).CloseDoor();
            Assert.IsFalse(hw.LightOn, "ensure the light is off after door closing");
        }

        [Test(Description = "US_3: When I open door heater stops if running.")]
        [TestCaseSource("GetImplementations")]
        public void US3_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(hw.HeaterOn);
            Assert.IsFalse(hw.DoorOpen);

            // simulate press start button - start heating
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsTrue(hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();

            Assert.IsFalse(hw.HeaterOn, "ensure the heater has been stopped after opening the door");
            Assert.IsTrue(hw.LightOn, "ensure if light is on after opening the door");
        }

        [Test(Description = "US_3: When I open door heater stops if running.")]
        [TestCaseSource("GetImplementations")]
        public void US3_Test2(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(hw.HeaterOn);
            Assert.IsFalse(hw.DoorOpen);

            // simulate press start button - start heating
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsTrue(hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();

            // simulate time elapsed - should not change enything
            SimulateTimeElapse(ctrl, 60);

            // simulate door close
            ((IMicrowaveOvenUI)hw).CloseDoor();

            SimulateTimeElapse(ctrl, 59);

            Assert.IsTrue(hw.HeaterOn, "ensure the heater is still running");
            Assert.IsFalse(hw.LightOn, "ensure if light is of after closing the door");
        }

        [Test(Description = "US_4: When I press start button when door is open nothing happens.")]
        [TestCaseSource("GetImplementations")]
        public void US4_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(hw.HeaterOn);
            Assert.IsFalse(hw.DoorOpen);

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();

            Assert.IsTrue(hw.DoorOpen, "ensure if door state is correct");
            Assert.IsTrue(hw.LightOn, "ensure the light is on");
            Assert.IsFalse(hw.HeaterOn, "ensure the heater is off");

            // simulate press start button
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsFalse(hw.HeaterOn, "ensure if heating has not been started after 'PressStartButton'");
        }

        [Test(Description = "US_5: When I press start button when door is closed, heater runs for 1 minute.")]
        [TestCaseSource("GetImplementations")]
        public void US5_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(hw.HeaterOn);
            Assert.IsFalse(hw.DoorOpen);
            Assert.IsFalse(hw.LightOn);

            // simulate press start button
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsTrue(hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

            SimulateTimeElapse(ctrl, 59);

            Assert.IsTrue(hw.HeaterOn, "ensure the heater is on 1 second before the end of heating");

            SimulateTimeElapse(ctrl, 1);

            Assert.IsFalse(hw.HeaterOn, "ensure the heater is off when 60 second elapsed");
        }

        [Test(Description = "US_6: When I press start button when door is closed and already heating, increase remaining time with 1 minute.")]
        [TestCaseSource("GetImplementations")]
        public void US6_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // ensure the heater is off and the door closed - default initial state
            Assert.IsFalse(hw.HeaterOn);
            Assert.IsFalse(hw.DoorOpen);
            Assert.IsFalse(hw.LightOn);

            // simulate press start button
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsTrue(hw.HeaterOn, 
                "ensure the heater has been started after 'PressStartButton'");

            SimulateTimeElapse(ctrl, 59);

            // simulate second press of the start button
            ((IMicrowaveOvenUI)hw).PressStartButton();

            Assert.IsTrue(hw.HeaterOn, "ensure the heater is still on");

            SimulateTimeElapse(ctrl, 60);

            Assert.IsTrue(hw.HeaterOn, 
                "ensure the heater is on 1 second before the end of heating (60 + 59 seconds)");

            SimulateTimeElapse(ctrl, 1);

            Assert.IsFalse(hw.HeaterOn, 
                "ensure the heater is off when 60 second elapsed");
        }

        [Test(Description = "US_7: When I open the door twice...")]
        [TestCaseSource("GetImplementations")]
        public void US7_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // simulate door open twice - does it make any sense?
            ((IMicrowaveOvenUI)hw).OpenDoor();
            ((IMicrowaveOvenUI)hw).OpenDoor();
        }

        [Test(Description = "US_8: When I close the door twice...")]
        [TestCaseSource("GetImplementations")]
        public void US8_Test1(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            hw.Initialize();
            ctrl.Initialize(hw);

            // simulate door open
            ((IMicrowaveOvenUI)hw).OpenDoor();

            // simulate door close twice - does it make any sense?
            ((IMicrowaveOvenUI)hw).CloseDoor();
            ((IMicrowaveOvenUI)hw).CloseDoor();
        }

        #region private helpers
        private void SimulateTimeElapse(IMicrowaveOvenControler ctrl, uint seconds)
        {
            while (seconds > 0)
            {
                ctrl.OneSecondTick();
                seconds--;
            }
        }
        private static string GetDescription(object obj)
        {
            object[] ca = obj.GetType().GetCustomAttributes(false);
            return ca
                .SingleOrDefault((v) => v is DescriptionAttribute) is DescriptionAttribute desc 
                ? desc.Description : obj.ToString();
        }
        #endregion
    }
}