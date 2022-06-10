using System;
using System.Linq;
using Astro.Helper;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace Astro
{
    public unsafe class Astro : IDalamudPlugin
    {
        private static class Functions
        {
            internal delegate void ReceiveAbility(uint sourceId, IntPtr sourceCharacter, IntPtr pos, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail);
            internal delegate bool TryAction(IntPtr actionManager, ActionType actionType, uint actionId, ulong targetId, uint param, uint origin, uint unknown, void* location);
        }

        string IDalamudPlugin.Name => "Astro";
        private const string CommandName = "/astro";
        private readonly Configuration configuration;

        public Astro([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudApi.Initialize(pluginInterface);
            Resolver.Initialize();
            
            HookHelper.Enable<Functions.ReceiveAbility>("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??", ReceiveAbilityDetour);
            HookHelper.Enable<Functions.TryAction>((IntPtr)ActionManager.fpUseAction, TryActionDetour);
            
            configuration = DalamudApi.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            
            IUi ui = new Ui(configuration);
            DalamudHelper.RegisterCommand(CommandName, "Open config window for Astro.", (_, _) => ui.Visible = true);
            DalamudApi.PluginInterface.UiBuilder.Draw += ui.Draw;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += () => ui.Visible = true;
        }

        private void ReceiveAbilityDetour(uint sourceId, IntPtr sourceCharacter, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
        {
            HookHelper.Get<Functions.ReceiveAbility>()(sourceId, sourceCharacter, position, effectHeader, effectArray, effectTrail);

            if(DalamudHelper.LocalPlayer?.ClassJob.GameData?.Abbreviation.RawString != "AST" || !DalamudHelper.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            if (configuration.EnableDivination)
            {
                if (AstrologianHelper.IsDivinationExecutable)
                    return;
                return;
            }

            if (AstrologianHelper.IsAstroSignFilled || AstrologianHelper.CurrentCard is AstrologianCard.None)
                return;

            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x61C, out float totalGcd);
            SafeMemory.Read((IntPtr)ActionManager.Instance() + 0x618, out float elapsedGcd);
            if(totalGcd - elapsedGcd <= 1.4f)
                return;

            if (configuration.EnableAutoRedraw && AstrologianHelper.HasRedraw && AstrologianHelper.IsAstroSignDuplicated)
            {
                DalamudHelper.AddQueueAction(AstrologianHelper.Redraw, DalamudApi.TargetManager.Target?.ObjectId ?? 0);
                return;
            }
            
            if(!configuration.EnableAutoPlay)
                return;

            var cardId = AstrologianHelper.GetActionId(AstrologianHelper.CurrentCard);
            var targetId = AstrologianHelper.GetOptimumTargetId();
            DalamudHelper.AddQueueAction(cardId, targetId);
        }
        
        private bool TryActionDetour(IntPtr actionManager, ActionType actionType, uint actionId, ulong targetId, uint param, uint origin, uint unknown, void* location)
        {
            var tryAction = HookHelper.Get<Functions.TryAction>();
            
            if(actionId != AstrologianHelper.Play)
                return tryAction(actionManager, actionType, actionId, targetId, param, origin, unknown, location);
            
            if (configuration.EnableAutoRedraw && AstrologianHelper.HasRedraw && AstrologianHelper.IsAstroSignDuplicated)
                return tryAction(actionManager, actionType, AstrologianHelper.Redraw, targetId, param, origin, unknown, location);

            var cardId = AstrologianHelper.GetActionId(AstrologianHelper.CurrentCard);
            var optimumTargetId = AstrologianHelper.GetOptimumTargetId();
            return tryAction(actionManager, actionType, cardId, optimumTargetId, param, origin, unknown, location);
        }

        void IDisposable.Dispose()
        {
            DalamudApi.CommandManager.RemoveHandler(CommandName);
            HookHelper.Disable<Functions.ReceiveAbility>();
            HookHelper.Disable<Functions.TryAction>();
            GC.SuppressFinalize(this);
        }
    }
}
