#nullable enable
using System;
using Dalamud;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Command;
using FFXIVClientStructs.FFXIV.Client.Game;
using Action = Lumina.Excel.GeneratedSheets.Action;

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

        public static void RegisterUi(IUi ui, System.Action openConfigUi)
        {
            DalamudApi.PluginInterface.UiBuilder.Draw += ui.Draw;
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += openConfigUi;
        }

        public static void RegisterCommand(string cmdName, string helpMessage, Action<string, string> cmdHandler)
        {
            var cmdInfo = new CommandInfo((command, arguments) => cmdHandler(command, arguments))
            {
                HelpMessage = helpMessage
            };
            DalamudApi.CommandManager.AddHandler(cmdName, cmdInfo);
        }
        
    }
}