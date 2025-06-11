using System;
using Godot;

namespace Tactris.Components.Block;

public partial class Block : Node2D
{
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

    private Polygon2D _polygon;
    
    private void RegisterChildren()
    {
        _polygon = GetNode<Polygon2D>("%Polygon2D");
    }
    
    private float _size = 1f;
    public float Size
    {
        get => _size;
        set
        {
            _size = value;
            _polygon.Polygon =
            [
                new Vector2(-.5f * value, -.5f * value),
                new Vector2(.5f * value, -.5f * value),
                new Vector2(.5f * value, .5f * value),
                new Vector2(-.5f * value, .5f * value)
            ];
        }
    }

    private bool _isTweenRunning;
    public bool Move(Vector2 translation, Action callback = null)
    {
        if (_isTweenRunning) return false;
        _isTweenRunning = true;
        
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.SetEase(Tween.EaseType.InOut);
        tween.Finished += () =>
        {
            _isTweenRunning = false;
            callback?.Invoke();
        };
        
        const float duration = .5f;
        const float stretchAmount = .1f;
        
        tween.Parallel().TweenProperty(this, "position", Position + translation, duration);

        var initialScale = Scale;
        var direction = translation.Normalized().Abs();
        var perpendicularDirection = new Vector2(-direction.Y, direction.X).Abs();

        var scaleAlong = Mathf.Abs(initialScale.Dot(direction));
        var scalePerp = Mathf.Abs(initialScale.Dot(perpendicularDirection));

        var targetScaleAlong = scaleAlong * (1 + stretchAmount);
        var targetScalePerp = scalePerp * (1 - stretchAmount);

        var targetScale = Scale * (direction * targetScaleAlong + perpendicularDirection * targetScalePerp).Clamp(1 - stretchAmount, 1 + stretchAmount);
        
        tween.Parallel().TweenProperty(this, "scale", targetScale, duration / 2);
        tween.Parallel().TweenProperty(this, "scale", initialScale, duration / 2).SetDelay(duration / 2);
        
        return true;
    }

    private bool IsDebug => GetTree().GetCurrentScene() != this;

    private void SetupForDebug()
    {
        if (IsDebug) return;

        Size = 100;
        Position = DisplayServer.WindowGetSize() / 2;
    }

    private void ProcessInputForDebug()
    {
        if (IsDebug) return;
        
        var direction = Vector2.Zero;
        if (Input.IsActionJustPressed("ui_up")) direction += Vector2.Up;
        if (Input.IsActionJustPressed("ui_down")) direction += Vector2.Down;
        if (Input.IsActionJustPressed("ui_left")) direction += Vector2.Left;
        if (Input.IsActionJustPressed("ui_right")) direction += Vector2.Right;
        if (direction != Vector2.Zero) Move(direction * Size);
    }
}