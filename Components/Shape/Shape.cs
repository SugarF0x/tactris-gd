using System.Linq;
using Godot;

namespace Tactris.Components.Shape;

public partial class Shape : Node2D
{
    private PackedScene _blockScene = GD.Load<PackedScene>("res://Components/Block/Block.tscn");
    
    public override void _Ready()
    {
        base._Ready();
        HydrateChildren();
        SetupForDebug();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ProcessInputForDebug();
    }

    private Color _color = Colors.White;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            foreach (var block in GetChildren().Cast<Block.Block>()) block.Color = value;
        }
    }
    
    private Vector2[] _points;
    public Vector2[] Points
    {
        get => _points;
        set
        {
            _points = value;
            HydrateChildren();
        }
    }

    private float _gap = 1f;
    public float Gap
    {
        get => _gap;
        set
        {
            _gap = value;
            var blocks = GetChildren().Cast<Block.Block>().ToArray();
            for (var i = 0; i < (Points ?? []).Length; i++)
            {
                var block = blocks[i];
                var point = Points[i];
                block.Position = point * BlockSize + value * i * Vector2.One;
            }
        }
    }

    private float _blockSize = 1f;
    public float BlockSize
    {
        get => _blockSize;
        set
        {
            _blockSize = value;
            var blocks = GetChildren().Cast<Block.Block>().ToArray();
            for (var i = 0; i < (Points ?? []).Length; i++)
            {
                var block = blocks[i];
                var point = Points[i];
                block.Size = value;
                block.Position = point * value;
            }
        }
    }

    public Vector2 ShapeSize =>
        (Points ?? []).Aggregate(new { Min = Vector2.Zero, Max = Vector2.Zero }, (acc, val) =>
            new {
                Min = new Vector2(float.Min(acc.Min.X, val.X), float.Min(acc.Min.Y, val.Y)),
                Max = new Vector2(float.Max(acc.Max.X, val.X), float.Max(acc.Max.Y, val.Y))
            }, resultSelector: acc => (acc.Max - acc.Min) * BlockSize);

    private void HydrateChildren()
    {
        foreach (var shape in GetChildren()) shape.QueueFree();
        for (var i = 0; i < (Points ?? []).Length; i++)
        {
            var block = _blockScene.Instantiate<Block.Block>();
            var point = Points[i];
            AddChild(block);
            block.Size = BlockSize;
            block.Position = point * (block.Size + Gap);
            block.Color = Color;
        }
    }
    
    // VVV DEBUG VVV  //

    private bool IsDebug => GetTree().GetCurrentScene() == this;

    private void SetupForDebug()
    {
        if (!IsDebug) return;

        BlockSize = 100f;
        Gap = 10f;
        Color = Colors.Cyan;
        Position = DisplayServer.WindowGetSize() / 2 - ShapeSize / 2;
        Points = [new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1)];
    }

    private void ProcessInputForDebug()
    {
        if (!IsDebug) return;
        
        var direction = Vector2.Zero;
        if (Input.IsActionJustPressed("ui_up")) direction += Vector2.Up;
        if (Input.IsActionJustPressed("ui_down")) direction += Vector2.Down;
        if (Input.IsActionJustPressed("ui_left")) direction += Vector2.Left;
        if (Input.IsActionJustPressed("ui_right")) direction += Vector2.Right;
        if (direction != Vector2.Zero) foreach (var block in GetChildren().Cast<Block.Block>()) block.Move(direction * ShapeSize);

        if (Input.IsActionJustPressed("ui_accept")) foreach (var block in GetChildren().Cast<Block.Block>()) block.Expand();
        if (Input.IsActionJustPressed("ui_cancel")) foreach (var block in GetChildren().Cast<Block.Block>()) block.Collapse();
    }
}