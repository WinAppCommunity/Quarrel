﻿// Quarrel © 2022

namespace Quarrel.Client.Models.Base.Interfaces
{
    /// <summary>
    /// An interface for discord items containing a snowflake as an id.
    /// </summary>
    public interface ISnowflakeItem
    {
        /// <summary>
        /// Gets or sets the id of the current item.
        /// </summary>
        ulong Id { get; }
    }
}
