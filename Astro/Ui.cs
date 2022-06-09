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
    
    public Ui(Configuration configuration)
    {
        this.configuration = configuration;
    }

    void IUi.Draw()
    {
        if (!visible || !ImGui.Begin("Astro", ref visible))
            return;

        if (ImGui.Checkbox("Enable auto play", ref configuration.EnableAutoPlay))
            configuration.Save();

        if (ImGui.Checkbox("Enable auto redraw", ref configuration.EnableAutoRedraw))
            configuration.Save();
        
        if(ImGui.Checkbox("If Divination is executable, stop auto play/redraw", ref configuration.EnableDivination))
            configuration.Save();

        ImGui.End();
    }
}