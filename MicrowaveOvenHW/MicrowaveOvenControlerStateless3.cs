using System;
using System.Composition;
using System.ComponentModel;
using Stateless;

using MicrowaveOvenHW.Enums;
using MicrowaveOvenHW.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MicrowaveOvenHW.Core;

namespace MicrowaveOvenHW
{
    [Export(typeof(IMicrowaveOvenControler))]
    [Description("Stateless3")]
    public class MicrowaveOvenControlerStateless3 : MicrowaveOvenControlerStateless2
    {
        #region public consts
        public const int DEFAULT_TIMEOUT = 10000; // in miliseconds
        #endregion

        #region private members
        private ConcurrentQueue<TriggerObject<MicrowaveOvenTriggers>> _queue
            = new ConcurrentQueue<TriggerObject<MicrowaveOvenTriggers>>();
        private Task _backgroundTask;
        private CancellationTokenSource _src;
        private CancellationToken _token;
        #endregion

        #region IMicrowaveOvenControler
        public override void OneSecondTick()
        {
            var to = new TriggerObject<MicrowaveOvenTriggers>(MicrowaveOvenTriggers.OneSecondTick);
            _queue.Enqueue(to);

            // wait until trigger is processed
            while (!to.Processed) Thread.Sleep(10);
        }
        #endregion

        protected override void HwOnDoorOpenChanged(bool b)
        {
            var to = new TriggerObject<MicrowaveOvenTriggers>(b == DOOR_OPEN ?
                MicrowaveOvenTriggers.DoorOpen
                : MicrowaveOvenTriggers.DoorClose);
            _queue.Enqueue(to);

            // wait until trigger is processed
            while (!to.Processed) Thread.Sleep(10);
        }

        protected override void HwOnStartButtonPressed(object sender, EventArgs eventArgs)
        {
            var to = new TriggerObject<MicrowaveOvenTriggers>(MicrowaveOvenTriggers.ButtonPress);
            _queue.Enqueue(to);

            // wait until trigger is processed
            while (!to.Processed) Thread.Sleep(10);
        }

        #region private methods
        protected override void InternalInitialize()
        {
            base.InternalInitialize();
            _src = new CancellationTokenSource();
            _token = _src.Token;
            _backgroundTask = new Task(BackgroundTask, _token);
            _backgroundTask.Start();
        }
        #endregion

        #region background task
        protected void BackgroundTask()
        {
            while (!_token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var to))
                {
                    _sm.Fire(to.Trigger);
                    to.SetProcessed();
                }
                Thread.Sleep(10);
            }
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            try
            {
                if (_backgroundTask != null)
                {
                    _src.Cancel();
                    _backgroundTask.Wait(DEFAULT_TIMEOUT);
                    _backgroundTask.Dispose();
                }
                base.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Exception] " + ex.Message);
            }
            finally
            {
                _backgroundTask = null;
            }
        }
        #endregion
    }
}