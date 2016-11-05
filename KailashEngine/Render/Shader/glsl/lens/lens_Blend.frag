
out vec4 color;

in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Lens Dirt
uniform sampler2D sampler1;		// Lens Star

uniform sampler2D sampler2;		// Bloom
uniform sampler2D sampler3;		// Flare

uniform mat3 lens_star_mod;


void main()
{
	// Mods
	vec4 lens_dirt = texture(sampler0, v_TexCoord) * 2.0;	
	
	vec2 star_TexCoord = (lens_star_mod * vec3(v_TexCoord, 1.0)).xy;
	vec4 lens_star = texture(sampler1, star_TexCoord) * 30.0;

	vec4 lens_mod = lens_star + lens_dirt;

	// FXs
	vec4 bloom = texture(sampler2, v_TexCoord);
	bloom += bloom * lens_dirt;

	vec4 flare = texture(sampler3, v_TexCoord) * 1.0;
	flare += flare * lens_star;

	color = bloom + flare;
}