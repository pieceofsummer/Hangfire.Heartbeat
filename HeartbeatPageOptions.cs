using System;
using Hangfire.Annotations;

namespace Hangfire.Heartbeat
{
    /// <summary>
    /// Configuration options for Heartbeat dashboard page
    /// </summary>
    [PublicAPI]
    public class HeartbeatPageOptions
    {
        private string _title;
        private int _statsPollingInterval;

        public HeartbeatPageOptions()
        {
            _title = "Heartbeat";
            _statsPollingInterval = 1000;
        }

        /// <summary>
        /// Navigation menu title for the Heartbeat page.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentException("The Title property value should be non-empty string", nameof(value));
                
                _title = value;
            }
        }

        /// <summary>
        /// The interval the /heartbeat/stats endpoint should be polled with.
        /// </summary>
        public int StatsPollingInterval
        {
            get => _statsPollingInterval;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The StatsPollingInterval property value should be positive");

                _statsPollingInterval = value;
            }
        }
    }
}
