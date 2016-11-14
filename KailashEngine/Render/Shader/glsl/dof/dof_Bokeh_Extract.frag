
out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Scene
uniform sampler2D sampler1;		// Depth
uniform sampler2D sampler2;		// COC

layout(binding = 0, offset = 0) uniform atomic_uint bokeh_counter;
writeonly uniform image1D sampler3;		// Bokeh Positions
writeonly uniform image1D sampler4;		// Bokeh Colors


vec3 sampler5x5(vec3 scene)
{
	vec3 final = vec3(0.0);
	vec2 tc;
	
	vec2 texture_size = 1.0 / textureSize(sampler0, 0);
		
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2(-1.5f,-1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( -0.5f,-1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 0.5f,-1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 1.5f,-1.5f))*texture_size,0).xyz;

	final += textureLod(sampler0,(gl_FragCoord.xy+vec2(-1.5f, 0.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( -0.5f, 0.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 0.5f, 0.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 1.5f, 0.5f))*texture_size,0).xyz;

	final += textureLod(sampler0,(gl_FragCoord.xy+vec2(-1.5f, 1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( -0.5f, 1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 0.5f, 1.5f))*texture_size,0).xyz;
	final += textureLod(sampler0,(gl_FragCoord.xy+vec2( 1.5f, 1.5f))*texture_size,0).xyz;

	final /= 12.0;
	
	return final;
}


void main()
{

	vec3 scene = texture(sampler0, v_TexCoord).rgb;
	float depth = texture(sampler1, v_TexCoord).w;
	float coc = texture(sampler2, v_TexCoord).r;


	vec3 scene_average = sampler5x5(scene);
	
	float scene_lum = dot(vec3(1.0), scene);
	float average_lum = dot(vec3(1.0), scene_average);

	float diff = max((scene_lum - average_lum),0.0);
	
	float lum_threshold = 0.5;
	float coc_threshold = 0.1;


	// Copy over scene if not a bokeh point
	color = vec4(scene,1.0);
	if( diff > lum_threshold && coc > coc_threshold)
	{
		int current = int(atomicCounterIncrement(bokeh_counter));

		imageStore(sampler3, current, vec4(v_TexCoord.x, v_TexCoord.y, depth, coc));
		imageStore(sampler4, current, vec4(scene.xyz,1.0));

		color = vec4(scene/max(coc*5.0,1.0),1.0);
	}


	//color = vec4(1.0);
}
