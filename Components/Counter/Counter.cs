using Godot;
using System;

public partial class Counter : Control
{
    private const int TotalDigits = 4;
    
    public override void _Ready()
    {
        base._Ready();
        RegisterChildren();
        SetupForDebug();
    }
    
    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessInputForDebug();
    }
    
    private Label _underlay;
    private Label _overlay;
    
    private void RegisterChildren()
    {
        _underlay = GetNode<Label>("%Underlay");
        _overlay = GetNode<Label>("%Value");
    }

    private Color _color;
    private Color Color
    {
        get => _color;
        set
        {
            _color = value;
            _underlay.Modulate = value with { A = _underlay.Modulate.A };
            _overlay.Modulate = Color with { A = 1f };
        }
    }

    private int _value;
    public int Value
    {
        get => _value;
        set
        {
            var oldValue = _value;
            var newValue = int.Max(0, value);
            _value = newValue;
            
            var diff = newValue - oldValue;
            var isSame = newValue == oldValue;
            var isPositive = diff > 0;

            if (isSame)
            {
                UpdateLabels(newValue);
                return;
            }
            
            var tween = CreateTween();
            tween.TweenMethod(Callable.From<int>(UpdateLabels), oldValue, newValue, float.Clamp(float.Abs(diff) * 0.05f, 0f, .3f));
            Color = isPositive ? Colors.Green : Colors.IndianRed;
            tween.Finished += () => Color = Colors.White;
        }
    }

    private void UpdateLabels(int value)
    {
        _underlay.Text = "".PadLeft(TotalDigits - value.ToString().Length, '0');
        _overlay.Text = value.ToString().PadLeft(TotalDigits, ' ');
    }
    
    // VVV DEBUG VVV  //

    private bool IsDebug => GetTree().GetCurrentScene() == this;

    private void SetupForDebug()
    {
        if (!IsDebug) return;

        Value = 0;
        Scale = Vector2.One * 5f;
        GD.Print(Size);
        Position = DisplayServer.WindowGetSize() / 2 - Size * Scale / 2;
    }

    private void ProcessInputForDebug()
    {
        if (!IsDebug) return;

        var change = 0;
        if (Input.IsActionJustPressed("ui_up")) change += 1;
        if (Input.IsActionJustPressed("ui_down")) change -= 1;
        if (Input.IsActionJustPressed("ui_left")) change -= 15;
        if (Input.IsActionJustPressed("ui_right")) change += 15;
        if (change != 0) Value += change;
    }
}
