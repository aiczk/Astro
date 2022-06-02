using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Astro.Helper;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Astro;

public interface IUi
{
    bool Visible { get; set; }
    void Draw();
}

public class Ui : IUi
{
    private bool visible;
    bool IUi.Visible { get => visible; set => visible = value; }
    
    private readonly Configuration configuration;
    private readonly IEnumerable<Action> actions;
    
    public Ui(Configuration configuration)
    {
        this.configuration = configuration;
        actions = DalamudApi.DataManager.Excel
            .GetSheet<Action>()!
            .Where(x => x.IsPlayerAction && !x.IsPvP && !x.IsRoleAction)
            .Where(x => x.ClassJobCategory.Value!.AST && x.ActionCategory.Value?.RowId == 4)
            .Where(x => x.Recast100ms > 10)
            .Skip(1);
    }

    void IUi.Draw()
    {
        if (!visible || !ImGui.Begin("Astro", ref visible))
            return;

        if (ImGui.CollapsingHeader("Abilities"))
        {
            ImGui.Text("Turn on the toggle below to stop auto-play/redraw until that ability is executed.");
            foreach (var action in actions)
            {
                var toggle = configuration.Abilities.Contains(action.RowId);
                if (!ImGui.RadioButton(action.Name, toggle))
                    continue;

                if (toggle)
                {
                    configuration.Abilities.Remove(action.RowId);
                    configuration.Save();
                    continue;
                }

                configuration.Abilities.Add(action.RowId);
                configuration.Save();
            }
        }

        ImGui.End();
    }
}