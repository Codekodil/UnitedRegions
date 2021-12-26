using UnhedderEngine;

namespace Gen4Assets
{
    public static class CustomShader
    {
        public static Shader CutoffShader = new Shader(@"
uniform instanced mat4 Model;
uniform mat4 View;
uniform mat4 Projection;
uniform vec3 Color = vec3(1.0, 1.0, 1.0);
uniform sampler2D Albedo;
uniform float InverseTextureScale = 1.0;

vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
{
    return Projection * View * Model * vec4(position, 1.0);
}

vec4 Fragment()
{
    vec4 texColor = texture(Albedo, uv * InverseTextureScale);
    if(texColor.a < 0.5)
        discard;
    return texColor * vec4(Color, 1.0);
}
");
    }
}
