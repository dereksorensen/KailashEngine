
out vec4 color;


in vec3 ray;

uniform samplerCube sampler0;		// SkyBox


void main()
{

	color = texture(sampler0, ray) * 100.0;

}
