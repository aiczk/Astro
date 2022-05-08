﻿#nullable enable
using System;
using System.Linq;
using Dalamud;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using Monkey.Helper;

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
            
            receiveAbilityHook = new Hook<ReceiveAbilityDelegate>(DalamudApi.SigScanner.ScanText("4C 89 44 24 ?? 55 56 57 41 54 41 55 41 56 48 8D 6C 24 ??"), ReceiveAbilityEffectDetour);
            receiveAbilityHook.Enable();
        }

        private void ReceiveAbilityEffectDetour(uint sourceId, IntPtr sourceCharacter, IntPtr position, IntPtr effectHeader, IntPtr effectArray, IntPtr effectTrail)
        {
            receiveAbilityHook.Original(sourceId, sourceCharacter, position, effectHeader, effectArray, effectTrail);

            if (DalamudApi.ClientState.LocalPlayer == null || DalamudApi.TargetManager.Target == null)
                return;

            if (!DalamudApi.ClientState.LocalPlayer.StatusFlags.HasFlag(StatusFlags.InCombat))
                return;

            if (AstrologianHelper.CurrentCard is AstrologianCard.None)
                return;

            var hasRedraw = DalamudApi.ClientState.LocalPlayer.StatusList.Any(x => x.StatusId == 2713);
            if (hasRedraw && AstrologianHelper.CheckDuplicateArcanum())
            {
                AddQueueAction((IntPtr) ActionManager.Instance(), ActionType.Spell, 3593, 0, 0);
                return;
            }
            
            var actionId = AstrologianHelper.GetActionId(AstrologianHelper.CurrentCard);
            var targetId = AstrologianHelper.GetOptimumTargetId(AstrologianHelper.CurrentCard);
            AddQueueAction((IntPtr) ActionManager.Instance(), ActionType.Spell, actionId, targetId, 0);
        }
        
        private static void AddQueueAction(IntPtr actionManager, ActionType actionType, uint actionId, uint targetId, uint param) 
        {
            SafeMemory.Read(actionManager + 0x68, out int queue);
            if (queue > 0)
                return;

            SafeMemory.Write(actionManager + 0x68, 1);
            SafeMemory.Write(actionManager + 0x6C, (byte)actionType);
            SafeMemory.Write(actionManager + 0x70, actionId);
            SafeMemory.Write(actionManager + 0x78, targetId);
            SafeMemory.Write(actionManager + 0x80, 0);
            SafeMemory.Write(actionManager + 0x84, param);
        }

        public void Dispose()
        {
            if(receiveAbilityHook.IsEnabled) receiveAbilityHook.Disable();
        }
    }
}
