using System;
using System.Collections.Generic;
using Astro.Helper;
using Dalamud.Configuration;

namespace Astro;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool EnableAutoPlay = true,
        EnableAutoRedraw = true,
        EnableBurstCard = false,
        EnableManualRedraw = false,
        EnableManualPlay = false,
        AvoidOverflowingCards = false,
        IsDivinationCloseToReady = false;
    
    public List<string> MeleePriority = new(), RangePriority = new(), MeleeBurstPriority = new (), RangeBurstPriority = new();
    public int DivinationRange = 5;

    private static readonly List<string> MeleeList = new () { "SAM", "DRK", "MNK", "NIN", "RPR", "DRG" },
                                         RangeList = new () { "BLM", "DNC", "SMN", "MCH", "BRD", "RDM" };

    public void Init()
    {
        MeleePriority = MeleePriority.Count == 0 ? MeleeList : MeleePriority;
        RangePriority = RangePriority.Count == 0 ? RangeList : RangePriority;
        MeleeBurstPriority = MeleeBurstPriority.Count == 0 ? MeleeList : MeleeBurstPriority;
        RangeBurstPriority = RangeBurstPriority.Count == 0 ? RangeList : RangeBurstPriority;
    }

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig(this);
}