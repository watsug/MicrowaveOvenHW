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
        public void US1_When_I_open_door_Light_is_on(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate door open
                ctx.Ui.OpenDoor();

                Assert.IsTrue(ctx.Hw.LightOn, "ensure the light is on after door closing");
            }
        }

        [Test(Description = "US_2: When I close door Light turns off.")]
        [TestCaseSource("GetImplementations")]
        public void US2_When_I_close_door_Light_turns_off(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate door open
                ctx.Ui.OpenDoor();
                Assert.IsTrue(ctx.Hw.LightOn, "ensure the light is on after door closing");

                // simulate door close
                ctx.Ui.CloseDoor();
                Assert.IsFalse(ctx.Hw.LightOn, "ensure the light is off after door closing");
            }
        }

        [Test(Description = "US_3: When I open door heater stops if running.")]
        [TestCaseSource("GetImplementations")]
        public void US3_When_I_open_door_heater_stops_if_running(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate press start button - start heating
                ctx.Ui.PressStartButton();

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

                // simulate door open
                ctx.Ui.OpenDoor();

                Assert.IsFalse(ctx.Hw.HeaterOn, "ensure the heater has been stopped after opening the door");
                Assert.IsTrue(ctx.Hw.LightOn, "ensure if light is on after opening the door");
            }
        }

        [Test(Description = "US_3: When I open door heater stops if running.")]
        [TestCaseSource("GetImplementations")]
        public void US3_When_I_open_door_heater_stops_if_running_ensure_total_time(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate press start button - start heating
                ctx.Ui.PressStartButton();

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

                // simulate door open
                ctx.Ui.OpenDoor();

                // simulate time elapsed - should not change enything
                SimulateTimeElapse(ctx.Ctrl, 60);

                // simulate door close
                ctx.Ui.CloseDoor();

                SimulateTimeElapse(ctx.Ctrl, 59);

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater is still running");
                Assert.IsFalse(ctx.Hw.LightOn, "ensure if light is of after closing the door");
            }
        }

        [Test(Description = "US_4: When I press start button when door is open nothing happens.")]
        [TestCaseSource("GetImplementations")]
        public void US4_When_I_press_start_button_when_door_is_open_nothing_happens(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate door open
                ctx.Ui.OpenDoor();

                Assert.IsTrue(ctx.Hw.DoorOpen, "ensure if door state is correct");
                Assert.IsTrue(ctx.Hw.LightOn, "ensure the light is on");
                Assert.IsFalse(ctx.Hw.HeaterOn, "ensure the heater is off");

                // simulate press start button
                ctx.Ui.PressStartButton();

                Assert.IsFalse(ctx.Hw.HeaterOn, "ensure if heating has not been started after 'PressStartButton'");
            }
        }

        [Test(Description = "US_5: When I press start button when door is closed, heater runs for 1 minute.")]
        [TestCaseSource("GetImplementations")]
        public void US5_When_button_pressed_and_door_closed_heater_runs_for_1_minute(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate press start button
                ctx.Ui.PressStartButton();

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater has been started after 'PressStartButton'");

                SimulateTimeElapse(ctx.Ctrl, 59);

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater is on 1 second before the end of heating");

                SimulateTimeElapse(ctx.Ctrl, 1);

                Assert.IsFalse(ctx.Hw.HeaterOn, "ensure the heater is off when 60 second elapsed");
            }
        }

        [Test(Description = "US_6: When I press start button when door is closed and already heating, increase remaining time with 1 minute.")]
        [TestCaseSource("GetImplementations")]
        public void US6_When_button_pressed_and_door_closed_and_heating_increase_time_1_minute(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate press start button
                ctx.Ui.PressStartButton();

                Assert.IsTrue(ctx.Hw.HeaterOn,
                    "ensure the heater has been started after 'PressStartButton'");

                SimulateTimeElapse(ctx.Ctrl, 59);

                // simulate second press of the start button
                ctx.Ui.PressStartButton();

                Assert.IsTrue(ctx.Hw.HeaterOn, "ensure the heater is still on");

                SimulateTimeElapse(ctx.Ctrl, 60);

                Assert.IsTrue(ctx.Hw.HeaterOn,
                    "ensure the heater is on 1 second before the end of heating (60 + 59 seconds)");

                SimulateTimeElapse(ctx.Ctrl, 1);

                Assert.IsFalse(ctx.Hw.HeaterOn,
                    "ensure the heater is off when 60 second elapsed");
            }
        }

        [Test(Description = "US_7: When I open the door twice...")]
        [TestCaseSource("GetImplementations")]
        public void US7_When_I_open_the_door_twice(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate door open twice - does it make any sense?
                ctx.Ui.OpenDoor();
                ctx.Ui.OpenDoor();
            }
        }

        [Test(Description = "US_8: When I close the door twice...")]
        [TestCaseSource("GetImplementations")]
        public void US8_When_I_close_the_door_twice(IMicrowaveOvenControler ctrl, IMicrowaveOvenHWEx hw)
        {
            using (var ctx = new MicrowaveTestSetup(ctrl, hw))
            {
                // simulate door open
                ctx.Ui.OpenDoor();

                // simulate door close twice - does it make any sense?
                ctx.Ui.CloseDoor();
                ctx.Ui.CloseDoor();
            }
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