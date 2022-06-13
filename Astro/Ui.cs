using System.Numerics;
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
    
    private readonly Configuration configuration;
    private static readonly Vector4 Red = new(1, 0, 0, 1);

    public Ui(Configuration configuration)
    {
        this.configuration = configuration;
    }

    void IUi.Draw()
    {
        if (!visible || !ImGui.Begin("Astro", ref visible, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        Checkbox("Enable auto play", ref configuration.EnableAutoPlay);
        Checkbox("Enable auto redraw", ref configuration.EnableAutoRedraw);
        Checkbox("Deal three cards at burst.", ref configuration.EnableBurstCard);
        if (ImGui.IsItemHovered())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Red);
            ImGui.SetTooltip("Auto play will not work except when the card charge count reaches 2 or while executing Divination!");
            ImGui.PopStyleColor();
        }

        ImGui.End();
    }

    private void Checkbox(string label, ref bool value)
    {
        if (ImGui.Checkbox(label, ref value))
            configuration.Save();
    }

    private static void Tooltip(string tooltip)
    {
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }
}