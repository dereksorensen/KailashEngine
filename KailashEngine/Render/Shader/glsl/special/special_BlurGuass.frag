


in vec2 v_TexCoord;


uniform sampler2D sampler0;				// Source Texture
writeonly uniform image2D sampler1;		// Destination Texture

uniform float direction;



void main()
{

	vec2 tex_coord = gl_FragCoord.xy;

	vec4 tex = texture(sampler0, v_TexCoord);



	imageStore(sampler1, ivec2(tex_coord), vec4(tex.xyz * direction, 1.0));

}
