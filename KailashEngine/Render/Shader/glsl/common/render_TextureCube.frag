
out vec4 color;

in vec3 ray;

uniform samplerCube sampler0;		// Cube Map

void main()
{
	color = texture(sampler0, ray);
}
