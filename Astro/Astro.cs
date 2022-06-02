using System;
using System.Collections.Generic;
using System.Linq;
using Astro.Helper;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Astro
{
    public unsafe class Astro : IDalamudPlugin
    {
        public string Name => "Astro";
        private const string CommandName = "/astro";

        private delegate void ReceiveAbilityDelegate(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityHook;
        private readonly Configuration configuration;
        
        public Astro([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(pluginInterface);
            Resolver.Initialize();

            var address = DalamudApi.SigScanner.ScanText("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??");
            receiveAbilityHook = new Hook<ReceiveAbilityDelegate>(address, ReceiveAbilityEffectDetour);
            receiveAbilityHook.Enable();

            configuration = DalamudApi.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            IUi ui = new Ui(configuration);
            DalamudHelper.RegisterCommand(CommandName, "Open config window for Astro.", (_, _) => ui.Visible = true);
            DalamudHelper.RegisterUi(ui, () => ui.Visible = true);
        }

        private void ReceiveAbilityEffectDetour(uint sourceId, IntPtr sourceCharacter, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
        {
            receiveAbilityHook.Original(sourceId, sourceCharacter, position, effectHeader, effectArray, effectTrail);

            if(DalamudHelper.LocalPlayer?.ClassJob.GameData?.Abbreviation.RawString != "AST")
                return;

            if (!DalamudHelper.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            if (configuration.Abilities.Any(x => !ActionManager.Instance()->IsRecastTimerActive(ActionType.Spell, x)))
                return;

            if (AstrologianHelper.IsAstroSignFilled || AstrologianHelper.CurrentCard is AstrologianCard.None)
                return;

            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x61C, out float totalGcd);
            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x618, out float elapsedGcd);
            if(totalGcd - elapsedGcd <= 1.4f)
                return;

            var hasRedraw = DalamudHelper.LocalPlayer.StatusList.Any(x => x.StatusId == AstrologianHelper.ExecutionOfRedraw);
            if (hasRedraw && AstrologianHelper.IsAstroSignDuplicated)
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
            DalamudApi.CommandManager.RemoveHandler(CommandName);
            if(receiveAbilityHook.IsEnabled) receiveAbilityHook.Disable();
        }
    }
}
