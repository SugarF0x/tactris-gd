using System.Linq;
using Godot;

namespace Tactris.Screens.Stage;

public partial class Stage : Control
{
    private static readonly string[] DesktopPlatforms = ["Windows", "macOS"];
    
    public override void _Ready()
    {
        base._Ready();
        RegisterChildren();
        PrepareChildren();
        AdjustForSafeArea();
    }

    private MarginContainer _marginContainer;
    private VBoxContainer _currentScoreContainer;
    private VBoxContainer _maxScoreContainer;
    private Label _currentScoreLabel;
    private Label _maxScoreLabel;
    private Components.Counter.Counter _currentScoreCounter;
    private Components.Counter.Counter _maxScoreCounter;

    private void RegisterChildren()
    {
        _marginContainer = GetNode<MarginContainer>("MarginContainer");
        _currentScoreContainer = GetNode<VBoxContainer>("%CurrentScoreContainer");
        _maxScoreContainer = GetNode<VBoxContainer>("%MaxScoreContainer");
        _currentScoreLabel = GetNode<Label>("%CurrentScoreLabel");
        _maxScoreLabel = GetNode<Label>("%MaxScoreLabel");
        _currentScoreCounter = GetNode<Components.Counter.Counter>("%CurrentScoreCounter");
        _maxScoreCounter = GetNode<Components.Counter.Counter>("%MaxScoreCounter");
    }

    private void PrepareChildren()
    {
        _currentScoreCounter.Value = 0;
        _maxScoreCounter.Value = 0;
    }

    // TODO: test if this actually works on mobile since PC behaves funny with safeareas
    private void AdjustForSafeArea()
    {
        if (DesktopPlatforms.Contains(OS.GetName())) return;
        
        var safeArea = DisplayServer.GetDisplaySafeArea();
        var screenSize = DisplayServer.ScreenGetSize();
        _marginContainer.AddThemeConstantOverride("margin_top", _marginContainer.GetThemeConstant("margin_top") + safeArea.Position.Y);
        _marginContainer.AddThemeConstantOverride("margin_bottom", _marginContainer.GetThemeConstant("margin_bottom") + (screenSize.Y - safeArea.Position.Y - safeArea.Size.Y));
    }
}