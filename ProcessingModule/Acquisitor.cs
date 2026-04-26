using Common;
using System;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for periodic polling.
    /// </summary>
    public class Acquisitor : IDisposable
	{
		private AutoResetEvent acquisitionTrigger;
        private IProcessingManager processingManager;
        private Thread acquisitionWorker;
		private IStateUpdater stateUpdater;
		private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Acquisitor"/> class.
        /// </summary>
        /// <param name="acquisitionTrigger">The acquisition trigger.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="stateUpdater">The state updater.</param>
        /// <param name="configuration">The configuration.</param>
		public Acquisitor(AutoResetEvent acquisitionTrigger, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.acquisitionTrigger = acquisitionTrigger;
			this.processingManager = processingManager;
			this.configuration = configuration;
			this.InitializeAcquisitionThread();
			this.StartAcquisitionThread();
		}

		#region Private Methods

        /// <summary>
        /// Initializes the acquisition thread.
        /// </summary>
		private void InitializeAcquisitionThread()
		{
			this.acquisitionWorker = new Thread(Acquisition_DoWork);
			this.acquisitionWorker.Name = "Acquisition thread";
		}

        /// <summary>
        /// Starts the acquisition thread.
        /// </summary>
		private void StartAcquisitionThread()
		{
			acquisitionWorker.Start();
		}

        /// <summary>
        /// Acquisitor thread logic.
        /// </summary>
		private void Acquisition_DoWork()
        {
            ushort counter = 0;

            while (true)
            {
                acquisitionTrigger.WaitOne();

                foreach (IConfigItem item in configuration.GetConfigurationItems())
                {
                    // analogne (200 i 2500) - svake 4 sekunde
                    if ((item.StartAddress == 200 || item.StartAddress == 2500) && counter % 4 == 0)
                    {
                        processingManager.ExecuteReadCommand(
                            item,
                            counter,
                            41,
                            item.StartAddress,
                            item.NumberOfRegisters
                        );
                    }
                    // digitalne (2300, 2302, 2305, 2306) - svake 3 sekunde
                    else if ((item.StartAddress == 2300 || item.StartAddress == 2302 || item.StartAddress == 2305 || item.StartAddress == 2306) && counter % 3 == 0)
                    {
                        processingManager.ExecuteReadCommand(
                            item,
                            counter,
                            41,
                            item.StartAddress,
                            item.NumberOfRegisters
                        );
                    }
                }

                counter++;
            }
        }

        #endregion Private Methods

        /// <inheritdoc />
        public void Dispose()
		{
			acquisitionWorker.Abort();
        }
	}
}