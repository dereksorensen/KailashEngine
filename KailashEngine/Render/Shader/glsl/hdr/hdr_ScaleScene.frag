
out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;		// Scene


layout (std430, binding=0) buffer ex
{
	vec2 exposure[];
};



void main()
{


	vec4 scene = texture(sampler0, v_TexCoord);

	//float lum_avg = -log(exposure[0].y * exposure[0].y);
	float lum_avg = 0.05 / exposure[0].y;

	vec3 final = scene.rgb;
	final *= lum_avg;


	color = vec4(final,1.0);

}
