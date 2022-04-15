﻿// Quarrel © 2022

using Quarrel.Services.Analytics.Enums;

namespace Quarrel.Services.Analytics
{
    /// <summary>
    /// An interface for a service that logs analytics.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Logs an event with a specified title and optional properties.
        /// </summary>
        /// <param name="title">The title of the event to track.</param>
        /// <param name="data">The optional event properties.</param>
        void Log(string title, params (string, object)[] data);

        /// <summary>
        /// Logs an event with a specified title and optional properties.
        /// </summary>
        /// <param name="eventType">The type of event to track.</param>
        /// <param name="data">The optional event properties.</param>
        void Log(LoggedEvent eventType, params (string, object)[] data);
    }
}
