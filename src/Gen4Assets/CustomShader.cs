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

        public static Shader GuiShader = new Shader(@"
define AlphaBlend;
define DisableDepthTest;
define DisableFaceCulling;

uniform instanced mat4 Model;
uniform vec3 Color = vec3(1.0, 1.0, 1.0);
uniform sampler2D Albedo;
uniform float Alignment = 0.0;
uniform float Ratio;

vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
{
    vec4 transformed = Model * vec4(position - vec3(Alignment, 0.0, 0.0), 1.0);
    return vec4(transformed.x / Ratio, transformed.y, transformed.z, 1.0) + vec4(Alignment, 0.0, 0.0, 0.0);
}

vec4 Fragment()
{
    return texture(Albedo, uv);
}
");

        public static Shader BattleGroundShader = new Shader(@"
uniform instanced mat4 Model;
uniform mat4 View;
uniform mat4 Projection;
uniform sampler2D Albedo;
uniform float Ratio;
uniform float Delta = 0.01;
uniform vec2 Center1;
uniform vec2 Center2;
uniform vec2 Center3;
uniform vec2 Center4;
uniform float Radius1;
uniform float Radius2;
uniform float Radius3;
uniform float Radius4;

float DistanceCircle(vec2 position, vec2 center, float radius)
{
    float centerDistanceAdjust = pow(2.0, -max(radius, 0.001));
    return (pow(2.0, length(position - center) - radius) - centerDistanceAdjust) / (1.0 - centerDistanceAdjust);
}

float smin( float a, float b, float k )
{
    float h = a-b;
    return 0.5*( (a+b) - sqrt(h*h+k) );
}

float Distance(vec2 position)
{
    position = round(position * 32.0) / 32.0;
    return min(smin(
        DistanceCircle(position, Center1, Radius1),
        DistanceCircle(position, Center2, Radius2),
        0.02),smin(
        DistanceCircle(position, Center3, Radius3),
        DistanceCircle(position, Center4, Radius4),
        0.02));
}

vec4 SampleTexture(float distance)
{
    vec2 texCoord = vec2(0.5, 0.5 - distance * 0.5);
    if(texCoord.y < 0.001) return vec4(0.0);
    return texture(Albedo, texCoord);
}

vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
{
    vec4 worldPos = Model * vec4(position, 1.0);
    return Projection * View * worldPos;
}

vec4 Fragment()
{
    float distance = Distance(worldPos.xz);
    vec4 texColor = SampleTexture(distance);
    if(texColor.a < 0.5)
        discard;
    return texColor;
}
");



        //        public static Shader BattleGroundShader = new Shader(@"
        //uniform instanced mat4 Model;
        //uniform mat4 View;
        //uniform mat4 Projection;
        //uniform sampler2D Albedo;
        //uniform float Ratio;
        //uniform float Delta = 0.01;
        //uniform vec2 Center1;
        //uniform vec2 Center2;
        //uniform float Radius1;
        //uniform float Radius2;

        //float Distance(vec2 position, vec2 center, float radius)
        //{
        //    return length(position - center) - radius;
        //}

        //vec2 ScreenPos(vec3 position)
        //{
        //    vec4 projected = Projection * View * vec4(position, 1.0);
        //    return projected.xy * (1.0 / projected.w);
        //}

        //vec2 WeightedScreenDelta(vec3 position, vec2 screenCenter, float centerDistance, vec2 center, float radius)
        //{
        //    vec2 screenPos = ScreenPos(position);
        //    float distance = Distance(position.xz, center, radius);
        //    return (screenPos - screenCenter) * (distance - centerDistance);
        //}

        //vec2 ScreenSpaceNormal(vec3 position, float centerDistance, vec2 center, float radius)
        //{
        //    vec2 screenCenter = ScreenPos(position);
        //    vec2 normal = vec2(0.0);
        //    normal = normal + WeightedScreenDelta(position + vec3(Delta, 0.0, 0.0), screenCenter, centerDistance, center, radius);
        //    normal = normal + WeightedScreenDelta(position + vec3(-Delta, 0.0, 0.0), screenCenter, centerDistance, center, radius);
        //    normal = normal + WeightedScreenDelta(position + vec3(0.0, 0.0, Delta), screenCenter, centerDistance, center, radius);
        //    normal = normal + WeightedScreenDelta(position + vec3(0.0, 0.0, -Delta), screenCenter, centerDistance, center, radius);
        //    normal = normalize(normal * vec2(Ratio, 1.0));
        //    return normalize(normal * vec2(0.6, 1.0));
        //}

        //vec4 SampleTexture(vec3 worldPosition, float distance, vec2 center, float radius)
        //{
        //    float centerDistanceAdjust = pow(8.0, -max(radius, 0.001));
        //    vec2 texCoord = (ScreenSpaceNormal(worldPosition, distance, center, radius) * (pow(8.0, distance) - centerDistanceAdjust) / (1.0 - centerDistanceAdjust)) * 0.5 + vec2(0.5, 0.5);
        //    if(texCoord.x > 0.999) return vec4(0.0);
        //    if(texCoord.x < 0.001) return vec4(0.0);
        //    if(texCoord.y > 0.999) return vec4(0.0);
        //    if(texCoord.y < 0.001) return vec4(0.0);
        //    return texture(Albedo, texCoord);
        //}

        //vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
        //{
        //    vec4 worldPos = Model * vec4(position, 1.0);
        //    return Projection * View * worldPos;
        //}

        //vec4 Fragment()
        //{
        //    float distance1 = Distance(worldPos.xz, Center1, Radius1);
        //    float distance2 = Distance(worldPos.xz, Center2, Radius2);
        //    vec4 texColor1 = SampleTexture(worldPos.xyz, distance1, Center1, Radius1);
        //    vec4 texColor2 = SampleTexture(worldPos.xyz, distance2, Center2, Radius2);
        //    vec4 texColor = mix(texColor1, texColor2, clamp((distance1-distance2) * 10.0 + 0.5, 0.0, 1.0));
        //    if(texColor.a < 0.5)
        //        discard;
        //    return texColor;
        //}
        //");

        public static Shader BattleSkyboxShader = new Shader(@"
define DisableDepthTest;
define FrontFaceCulling;

uniform instanced mat4 Model;
uniform mat4 View;
uniform mat4 Projection;
uniform sampler2D Albedo;
uniform float Pi = 3.14159265359;

vec4 Vertex(vec3 position, vec2 uv, vec3 normal, vec3 tangent)
{
    vec4 worldPos = Model * vec4(position, 1.0);
    return Projection * View * worldPos;
}

vec4 Fragment()
{
    float x = (atan(worldPos.x, worldPos.z) / Pi + 1.0) * 0.75;
    float weight = smoothstep(0.0, 1.0, clamp(2.0 - abs(-x * 4.0 + 2.0), 0.0, 1.0));
    float y = atan(length(worldPos.xz), worldPos.y) / Pi;
    y = 1.0 - abs(y * 1.5 - 0.5);
    return mix(
        texture(Albedo, vec2(mod(x + 0.75, 1.5), y)),
        texture(Albedo, vec2(x, y)),
        weight);
}
");
    }
}
