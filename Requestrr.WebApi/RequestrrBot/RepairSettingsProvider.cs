using System.Collections.Generic;
using System.Linq;

namespace Requestrr.WebApi.RequestrrBot
{
    /// <summary>
    /// Reads <see cref="RepairSettings"/> out of the on-disk settings.json.
    /// Mirrors the pattern used by the other settings providers in this
    /// project (e.g. <c>TvShowsSettingsProvider</c>).
    /// </summary>
    public class RepairSettingsProvider
    {
        public RepairSettings Provide()
        {
            dynamic settings = SettingsFile.Read();

            var result = new RepairSettings();

            try
            {
                if (settings.Repair != null)
                {
                    if (settings.Repair.Enabled != null)
                    {
                        result.Enabled = (bool)settings.Repair.Enabled;
                    }

                    if (settings.Repair.DeleteFileBeforeReSearch != null)
                    {
                        result.DeleteFileBeforeReSearch = (bool)settings.Repair.DeleteFileBeforeReSearch;
                    }

                    if (settings.Repair.MonitoredRoles != null)
                    {
                        var roles = new List<ulong>();
                        foreach (var r in settings.Repair.MonitoredRoles)
                        {
                            if (ulong.TryParse((string)r.ToString(), out ulong parsed))
                            {
                                roles.Add(parsed);
                            }
                        }
                        result.MonitoredRoles = roles;
                    }
                }
            }
            catch
            {
                // Settings file does not contain a Repair section yet — fall back to defaults.
            }

            return result;
        }
    }
}
