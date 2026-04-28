using System;
using System.Collections.Generic;

namespace Requestrr.WebApi.RequestrrBot
{
    /// <summary>
    /// Settings for the /repair Discord slash command.  The feature is opt-in
    /// (Enabled defaults to false) so admins must explicitly turn it on.
    /// </summary>
    public class RepairSettings
    {
        /// <summary>
        /// Whether the /repair slash command is enabled at all.
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// If set, only Discord users who have one of these roles can use
        /// /repair.  An empty list means anyone with access to the bot can
        /// use it (subject to existing channel restrictions).
        /// </summary>
        public List<ulong> MonitoredRoles { get; set; } = new List<ulong>();

        /// <summary>
        /// When true, the existing file(s) on disk are deleted before a new
        /// search is triggered.  When false, only a fresh search is triggered
        /// and Sonarr/Radarr decide whether to upgrade.
        /// </summary>
        public bool DeleteFileBeforeReSearch { get; set; } = true;
    }
}
