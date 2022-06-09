using System;
using System.Collections.Generic;
using Astro.Helper;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace Astro;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableDivination = true, EnableAutoPlay = true, EnableAutoRedraw = true;

    public void Save() => DalamudApi.PluginInterface.SavePluginConfig(this);
}