using System;
using System.Collections.Generic;
using Astro.Helper;
using Dalamud.Configuration;

namespace Astro;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableAutoPlay = true, EnableAutoRedraw = true, EnableBurstCard = false;
    public List<string> MeleePriority = new(), RangePriority = new();

    public void Init()
    {
        MeleePriority = MeleePriority.Count == 0 ? new List<string> { "DRG", "SAM", "NIN", "MNK", "RPR" } : MeleePriority;
        RangePriority = RangePriority.Count == 0 ? new List<string> { "BLM", "SMN", "MCH", "BRD", "RDM", "DNC" } : RangePriority;
    }

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig(this);
}