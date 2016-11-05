
// Inspired by http://john-chapman-graphics.blogspot.ca/2013/02/pseudo-lens-flare.html


out vec4 color;

in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Bright Spots
uniform sampler1D sampler1;		// Lens Color



vec3 textureDistorted(
      sampler2D tex,
      vec2 texcoord,
      vec2 direction, // direction of distortion
      vec3 distortion // per-channel distortion factor
   ) 
{
      return vec3(
         texture(tex, texcoord + direction * distortion.r).r ,
         texture(tex, texcoord + direction * distortion.g).g,
         texture(tex, texcoord + direction * distortion.b).b
      );
}


void main()
{

	vec2 flare_TexCoord = -v_TexCoord + vec2(1.0);


	vec2 texel_size = 1.0 / vec2(textureSize(sampler0, 0));
	vec4 final_ghost_orbs = vec4(0.0);
	vec4 final_halo = vec4(0.0);


	vec2 ghost_vector = (vec2(0.5) - flare_TexCoord) * 0.5;

	float distortionVariable = 2.0;
	vec3 distortion = vec3(-texel_size.x * distortionVariable, 0.0, texel_size.x * distortionVariable);
	vec2 direction = normalize(ghost_vector);

	//------------------------------------------------------
	// Ghost Orbs
	//------------------------------------------------------

	for(int i = -1; i < 2; i++)
	{
		vec2 offset = fract(flare_TexCoord + ghost_vector * float(i));

		float weight_ghost = length(vec2(0.5) - offset) / length(vec2(0.5));
		weight_ghost = pow(1.0 - weight_ghost, 3.0);
		final_ghost_orbs += vec4(textureDistorted(sampler0, offset, direction, distortion),1.0) * weight_ghost;
	}
	final_ghost_orbs *= texture(sampler1, 1.0 - length(vec2(0.5) - flare_TexCoord) / length(vec2(0.5)));
	final_ghost_orbs *= 0.6;


	//------------------------------------------------------
	// Halo
	//------------------------------------------------------

	vec2 haloVec = normalize(ghost_vector) * 0.58;
	float weight_halo = length(vec2(0.5) - fract(flare_TexCoord + haloVec)) / length(vec2(0.5));
	weight_halo = pow(1.0 - weight_halo, 20.0);
	final_halo += vec4(textureDistorted(sampler0, flare_TexCoord + haloVec, direction, distortion),1.0) * weight_halo;
	final_halo *= 0.1;


	//------------------------------------------------------
	// Blend them all
	//------------------------------------------------------

	color = (final_ghost_orbs + final_halo) * 0.7;

}
