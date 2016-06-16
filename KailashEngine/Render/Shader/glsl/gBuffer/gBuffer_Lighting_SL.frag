

out vec4 color;


//------------------------------------------------------
// perspective and view matrices
//------------------------------------------------------
layout(std140, binding = 0) uniform cameraMatrices
{
	mat4 view;
	mat4 perspective;
	vec3 cam_position;
};

uniform sampler2D sampler0;		// Normal & Depth
uniform sampler2D sampler1;		// Specular

uniform vec3 light_position;
uniform vec3 light_direction;
uniform vec3 light_color;
uniform float light_intensity;
uniform float light_falloff;


void main()
{

	//Calculate Texture Coordinates
	vec2 texCoord = gl_FragCoord.xy / textureSize(sampler0, 0);

	vec4 normal_depth = texture(sampler0, texCoord);
	
	color = vec4(normal_depth);

}
