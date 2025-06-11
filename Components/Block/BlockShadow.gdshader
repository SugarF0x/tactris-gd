shader_type canvas_item;

uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float edge_softness : hint_range(0.0, 1.0) = 0.4;

void fragment() {
    vec2 centered_uv = abs(UV - vec2(0.5));
    float fade = smoothstep(0.5, 0.5 - edge_softness, max(centered_uv.x, centered_uv.y));
    COLOR = glow_color * fade;
}
