using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.RequestrrBot;

namespace Requestrr.WebApi.Controllers.Configuration
{
    [ApiController]
    [Route("/api/configuration/repair")]
    public class RepairSettingsController : ControllerBase
    {
        private readonly RepairSettingsProvider _repairSettingsProvider;

        public RepairSettingsController(RepairSettingsProvider repairSettingsProvider)
        {
            _repairSettingsProvider = repairSettingsProvider;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var settings = _repairSettingsProvider.Provide();

            return Ok(new
            {
                Enabled = settings.Enabled,
                DeleteFileBeforeReSearch = settings.DeleteFileBeforeReSearch,
                MonitoredRoles = settings.MonitoredRoles.Select(r => r.ToString()).ToArray(),
            });
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> SaveAsync([FromBody] RepairSettingsModel model)
        {
            var roles = new List<ulong>();

            if (model.MonitoredRoles != null)
            {
                foreach (var r in model.MonitoredRoles)
                {
                    if (!string.IsNullOrWhiteSpace(r) && ulong.TryParse(r.Trim(), out ulong parsed))
                    {
                        roles.Add(parsed);
                    }
                }
            }

            SettingsFile.Write(settings =>
            {
                if (settings.Repair == null)
                {
                    settings.Repair = new JObject();
                }

                settings.Repair.Enabled = model.Enabled;
                settings.Repair.DeleteFileBeforeReSearch = model.DeleteFileBeforeReSearch;
                settings.Repair.MonitoredRoles = new JArray(roles.Select(r => r.ToString()));
            });

            return Ok(new { ok = true });
        }

        public class RepairSettingsModel
        {
            public bool Enabled { get; set; }
            public bool DeleteFileBeforeReSearch { get; set; } = true;
            public string[] MonitoredRoles { get; set; } = new string[0];
        }
    }
}
