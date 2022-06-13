﻿#nullable enable
using System;
using Dalamud;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace Astro.Helper
{
    public static class DalamudHelper
    {
        public static PlayerCharacter? LocalPlayer => DalamudApi.ClientState.LocalPlayer;
        
        public static unsafe void AddQueueAction(uint actionId, uint targetId) => AddQueueAction((IntPtr)ActionManager.Instance(), ActionType.Spell, actionId, targetId, 0);

        private static void AddQueueAction(IntPtr actionManager, ActionType actionType, uint actionId, uint targetId, uint param) 
        {
            SafeMemory.Read<bool>(actionManager + 0x68, out var inQueue);
            if (!inQueue)
                return;

            SafeMemory.Write(actionManager + 0x68, true);
            SafeMemory.Write(actionManager + 0x6C, (byte)actionType);
            SafeMemory.Write(actionManager + 0x70, actionId);
            SafeMemory.Write(actionManager + 0x78, targetId);
            SafeMemory.Write(actionManager + 0x80, 0);
            SafeMemory.Write(actionManager + 0x84, param);
        }

        public static unsafe int GetActionChargeCount(uint actionId, int maxChargeCount, int chargeTime)
        {
            var recast = ActionManager.Instance()->GetRecastTimeElapsed(ActionType.Spell, actionId);
            return (int)Math.Floor(recast == 0 ? maxChargeCount : recast / chargeTime);
        }

        public static void RegisterCommand(string cmdName, string helpMessage, CommandInfo.HandlerDelegate cmdHandler)
        {
            var cmdInfo = new CommandInfo(cmdHandler)
            {
                HelpMessage = helpMessage
            };
            DalamudApi.CommandManager.AddHandler(cmdName, cmdInfo);
        }
        
    }
}