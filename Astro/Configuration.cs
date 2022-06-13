using System;
using Astro.Helper;
using Dalamud.Configuration;

namespace Astro;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableAutoPlay = true, EnableAutoRedraw = true, EnableBurstCard = false;

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig(this);
}