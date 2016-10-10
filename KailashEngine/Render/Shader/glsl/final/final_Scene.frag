

out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D sampler0;



vec3 Uncharted2Tonemap(vec3 x)
{
	x = max(x, 0.0);

	/* DEFAULT
	float A = 0.15;		// Sholder Strength
	float B = 0.50;		// Linear Strength
	float C = 0.10;		// Linear Angle
	float D = 0.30;		// Toe Strength (default: 0.2)
	float E = 0.02;		// Toe Numerator
	float F = 0.30;		// Toe Denominator
	*/
	
	float A = 0.22;		// Sholder Strength
	float B = 0.30;		// Linear Strength
	float C = 0.10;		// Linear Angle
	float D = 0.50;		// Toe Strength (default: 0.2)
	float E = 0.02;		// Toe Numerator
	float F = 0.30;		// Toe Denominator

	vec3 final = ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;

    return final;
}


void main()
{






	
	vec4 tex = texture(sampler0, v_TexCoord);


	vec3 final = Uncharted2Tonemap(2.0*tex.xyz);
	
	float W = 11.2;
	vec3 whiteScale = 1.0f/Uncharted2Tonemap(vec3(W));
	final *= whiteScale;


	float gamma = 1.8;
	vec3 gamma_correction = vec3(1.0 / gamma);
	final = pow(final.xyz, gamma_correction);

	color = vec4(final, 1.0);

}
