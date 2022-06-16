using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Astro.Helper;
using ImGuiNET;

namespace Astro;

public interface IUi
{
    bool Visible { set; }
    void Draw();
}

public class Ui : IUi
{
    private bool visible;
    bool IUi.Visible { set => visible = value; }
    
    private static readonly Vector4 Red = new(1, 0, 0, 1);

    void IUi.Draw()
    {
        if (!visible || !ImGui.Begin("Astro", ref visible))
            return;

        Checkbox("Enable auto play", ref DalamudApi.Configuration.EnableAutoPlay);
        Checkbox("Enable auto redraw", ref DalamudApi.Configuration.EnableAutoRedraw);
        Checkbox("Deal three cards at burst", ref DalamudApi.Configuration.EnableBurstCard);
        if (ImGui.IsItemHovered())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Red);
            ImGui.SetTooltip("Auto play will not work except when the card charge count reaches 2 or while executing Divination!");
            ImGui.PopStyleColor();
        }

        ImGui.Separator();
        ImGui.Text("Melee card priority (drag and drop to re-order)");
        ReordableList(DalamudApi.Configuration.MeleePriority);
        ImGui.Separator();
        ImGui.Text("Range card priority (drag and drop to re-order)");
        ReordableList(DalamudApi.Configuration.RangePriority);
        ImGui.TextWrapped("If you are missing a Class Job, please add the Abbreviation of the desired job to the \"(Melee|Range)Priority\" in %%appdata%%\\XIVLauncher\\pluginConfigs\\Astro.json.");

        ImGui.End();
    }
    
    private static unsafe void ReordableList(List<string> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            ImGui.Text($"{i + 1}.");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(ImGui.GetWindowWidth());
            ImGui.Selectable($"{list[i]}");

            if (ImGui.BeginDragDropSource())
            {
                ImGui.Text($"Selecting {list[i]}");
                ImGui.SetDragDropPayload("Index", (IntPtr)(&i), sizeof(int));
                ImGui.EndDragDropSource();
            }

            if (!ImGui.BeginDragDropTarget())
                continue;

            var payload = ImGui.AcceptDragDropPayload("Index");
            if (payload.NativePtr != null)
            {
                var dataPtr = (int*)payload.Data;
                if (dataPtr != null)
                {
                    var srcIndex = dataPtr[0];
                    (list[srcIndex], list[i]) = (list[i], list[srcIndex]);
                    DalamudApi.Configuration.Save();
                }
            }

            ImGui.EndDragDropTarget();
        }
    }

    private void Checkbox(string label, ref bool value)
    {
        if (ImGui.Checkbox(label, ref value))
            DalamudApi.Configuration.Save();
    }

    private static void Tooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }
}