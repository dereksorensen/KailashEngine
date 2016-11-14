
out vec4 color; 

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
	float sceneDepth = texture(sampler1, texCoord).w;
	float coc = texture(sampler2, texCoord).r;
	vec4 shape = texture(sampler0, g_TexCoord);
	shape *= shape.a;

	float depthCullThreshold = 0.0;

	float weight = clamp(sceneDepth - (g_bokehDepth + depthCullThreshold), 0.0, 1.0);
	weight = clamp(weight + coc, 0.0, 1.0);

	
	float Attenuation = 2.5;
	float att = clamp(length(2.0*(g_TexCoord-vec2(0.5))),0.0,1.0);
	att = 1.0 - pow(att,Attenuation);
	

	color = shape * g_bokehColor * weight;// * att;

}
