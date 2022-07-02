using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Dalamud.Logging;

namespace Astro.Helper;

public static class AstroLog
{
    public static readonly Subject<string> Log = new();

    public static void Initialize() =>
        Log
            .DistinctUntilChanged()
            .Subscribe(x => PluginLog.Log(x));

    public static void Dispose() => Log.Dispose();
}