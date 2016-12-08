
out vec4 color;

uniform float k; // k=0 for line 4, k=1 for line 10
uniform sampler2D deltaESampler;

void main() 
{
    vec2 uv = gl_FragCoord.xy / vec2(SKY_W, SKY_H);
    color = k * texture2D(deltaESampler, uv); // k=0 for line 4, k=1 for line 10
}