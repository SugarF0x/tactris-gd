using System;
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

    private Block.Block[] Blocks = [];

    private Color _color = Colors.White;
    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateAllBlockColors();
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
            UpdateAllBlockPositions();
        }
    }

    private float _blockSize = 1f;
    public float BlockSize
    {
        get => _blockSize;
        set
        {
            _blockSize = value;
            UpdateAllBlockSizes();
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
        Blocks = new Block.Block[(Points ?? []).Length];
        for (var i = 0; i < Blocks.Length; i++)
        {
            var block = _blockScene.Instantiate<Block.Block>();
            AddChild(block);
            Blocks[i] = block;
            UpdateBlockSize(block);
            UpdateBlockColor(block);
        }
    }

    private void UpdateBlockColor(Block.Block block) => block.Color = Color;
    private void UpdateAllBlockColors() => Array.ForEach(Blocks, UpdateBlockColor);

    private void UpdateBlockSize(Block.Block block)
    {
        block.Size = BlockSize;
        UpdateBlockPosition(block);
    }
    private void UpdateAllBlockSizes() => Array.ForEach(Blocks, UpdateBlockSize);

    private void UpdateBlockPosition(Block.Block block)
    {
        var point = Points[Array.IndexOf(Blocks, block)];
        block.Position = point * (BlockSize + Gap);
    }
    private void UpdateAllBlockPositions() => Array.ForEach(Blocks, UpdateBlockPosition);
    
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
        if (direction != Vector2.Zero)
        {
            var size = ShapeSize;
            foreach (var block in Blocks) block.Move(direction * size);
        }

        if (Input.IsActionJustPressed("ui_accept")) foreach (var block in Blocks) block.Expand();
        if (Input.IsActionJustPressed("ui_cancel")) foreach (var block in Blocks) block.Collapse();
    }
}