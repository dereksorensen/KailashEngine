
out vec4 color;

in vec3 ray;

uniform samplerCubeArray sampler0;		// Cube Map Array
uniform int layer;

void main()
{
	color = textureLod(sampler0, vec4(ray, layer), 0);
}
