
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

	vec2 tCoord = -v_TexCoord + vec2(1.0);


	vec2 texelSize = 1.0 / vec2(textureSize(sampler0, 0));
	vec4 final_ghost_screen = vec4(0.0);
	vec4 final_ghost_orbs = vec4(0.0);
	vec4 final_halo = vec4(0.0);
	vec4 final = vec4(0.0);

	vec2 ghostVec = (vec2(0.5) - tCoord) * 0.98;

	//------------------------------------------------------
	// Ghost Screen
	//------------------------------------------------------

	float distortionVariable = 0.2;
	vec3 distortion = vec3(-texelSize.x * distortionVariable, 0.0, texelSize.x * distortionVariable);
	vec2 direction = normalize(ghostVec);

	for(int i = 0; i < 2; i++)
	{
		vec2 offset = fract(tCoord + ghostVec * float(i));
		
		float weight_ghost = length(vec2(0.5) - offset) / length(vec2(0.5));
		weight_ghost = pow(1.0 - weight_ghost, 3.0);
		final_ghost_screen += texture(sampler0, offset) * weight_ghost;	
	}



	ghostVec = (vec2(0.5) - tCoord) * 0.5;

	//------------------------------------------------------
	// Ghost Orbs
	//------------------------------------------------------

	distortionVariable = 1.0;
	distortion = vec3(-texelSize.x * distortionVariable, 0.0, texelSize.x * distortionVariable);

	for(int i = -1; i < 2; i++)
	{
		vec2 offset = fract(tCoord + ghostVec * float(i));

		float weight_ghost = length(vec2(0.5) - offset) / length(vec2(0.5));
		weight_ghost = pow(1.0 - weight_ghost, 3.0);
		final_ghost_orbs += vec4(textureDistorted(sampler0, offset, direction, distortion),1.0) * weight_ghost;
	}
	final_ghost_orbs *= texture(sampler1, 1.0 - length(vec2(0.5) - tCoord) / length(vec2(0.5)));


	//------------------------------------------------------
	// Halo
	//------------------------------------------------------

	distortionVariable = 1.5;
	distortion = vec3(-texelSize.x * distortionVariable, 0.0, texelSize.x * distortionVariable);

	vec2 haloVec = normalize(ghostVec) * 0.55;
	float weight_halo = length(vec2(0.5) - fract(tCoord + haloVec)) / length(vec2(0.5));
	weight_halo = pow(1.0 - weight_halo, 20.0);
	final_halo += vec4(textureDistorted(sampler0, tCoord + haloVec, direction, distortion),1.0) * weight_halo;


	//------------------------------------------------------
	// Blend them all
	//------------------------------------------------------


	color = final_ghost_screen + 
			final_ghost_orbs + 
			final_halo;

	color = final_ghost_orbs;

}
