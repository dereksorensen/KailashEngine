
out vec4 color;
in vec2 v_TexCoord;


uniform sampler2D sampler0;		// Velocity
uniform sampler2D sampler1;		// MB
uniform sampler2D sampler2;		// Scene


uniform float fps_scaler;






void main()
{

	vec2 velocity = texture(sampler0, v_TexCoord).xy * 60.0;
	vec4 mb = texture(sampler1, v_TexCoord);
	vec4 scene = texture(sampler2, v_TexCoord);



	
	vec4 final = scene * mb.a + mb * (1.0 - mb.a);


	color = final;

}
