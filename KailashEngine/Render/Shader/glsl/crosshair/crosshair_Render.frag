
out vec4 color;


uniform sampler2D sampler0;		// Crosshair texture
uniform vec2 rotation;

void main()
{

	vec2 pCoord = gl_PointCoord * 2.0 - 1.0;
	vec2 texCoord = vec2(
		pCoord.x*rotation.y - pCoord.y*rotation.x,
		pCoord.x*rotation.x + pCoord.y*rotation.y
	);

	vec4 shape = texture(sampler0, texCoord * 0.5 + 0.5);

	color = shape;
}
