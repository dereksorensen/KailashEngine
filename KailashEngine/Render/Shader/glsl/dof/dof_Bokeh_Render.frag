
out vec3 color; 

in vec4 g_bokehColor;
in float g_bokehDepth;
in vec2 g_TexCoord;

uniform sampler2D sampler0;		// Bokeh Shape
uniform sampler2D sampler1;		// Depth
uniform sampler2D sampler2;		// COC

uniform vec2 texture_size;

void main()
{
	vec2 texCoord = gl_FragCoord.xy * texture_size;
	float depth = texture(sampler1, texCoord).w;
	float coc = texture(sampler2, texCoord).r;
	vec4 shape = texture(sampler0, g_TexCoord);
	//shape *= shape.a;

	float depthCullThreshold = 0.0;

	float weight = clamp(depth - g_bokehDepth + depthCullThreshold, 0.0, 1.0);
	weight = clamp(weight + coc, 0.0, 1.0);

	
	float attenuation = 4.5;
	float att = clamp(length(g_TexCoord * 2.0 - 1.0),0.0,1.0);
	att = 1.0 - pow(att, attenuation);
	

	color = shape.xyz * g_bokehColor.xyz * weight * att;
	//color = vec3(0.0);
}
