
out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		//Scene


layout (std430, binding=0) buffer ex
{
	vec2 exposure[];
};


void main()
{

	vec4 scene = texture(sampler0, v_TexCoord);

	float lum_avg = 0.05 / exposure[0].y;

	color = vec4(scene.rgb * lum_avg,1.0);

}
