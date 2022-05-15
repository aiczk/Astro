using System;
using System.Linq;
using Astro.Helper;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Astro
{
    public unsafe class Astro : IDalamudPlugin
    {
        public string Name => "Astro";

        private delegate void ReceiveAbilityDelegate(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityHook;
        
        public Astro([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(pluginInterface);
            Resolver.Initialize();
            
            receiveAbilityHook = new Hook<ReceiveAbilityDelegate>(DalamudApi.SigScanner.ScanText("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??"), ReceiveAbilityEffectDetour);
            receiveAbilityHook.Enable();
        }

        private void ReceiveAbilityEffectDetour(uint sourceId, IntPtr sourceCharacter, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
        {
            receiveAbilityHook.Original(sourceId, sourceCharacter, position, effectHeader, effectArray, effectTrail);

            if(DalamudApi.ClientState.LocalPlayer?.ClassJob.GameData?.Abbreviation.RawString != "AST")
                return;

            if (!DalamudApi.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            if (AstrologianHelper.CurrentCard is AstrologianCard.None)
                return;

            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x61C, out float totalGcd);
            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x618, out float elapsedGcd);
            if(totalGcd - elapsedGcd <= 1.4f)
                return;

            var hasRedraw = DalamudApi.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == AstrologianHelper.ExecutionOfRedraw);
            if (hasRedraw && AstrologianHelper.CheckDuplicateArcanum())
            {
                DalamudHelper.AddQueueAction(AstrologianHelper.Redraw, DalamudApi.TargetManager.Target?.ObjectId ?? 0);
                return;
            }

            var cardId = AstrologianHelper.GetActionId(AstrologianHelper.CurrentCard);
            var targetId = AstrologianHelper.GetOptimumTargetId();
            DalamudHelper.AddQueueAction(cardId, targetId);
        }

        public void Dispose()
        {
            if(receiveAbilityHook.IsEnabled) receiveAbilityHook.Disable();
        }
    }
}
