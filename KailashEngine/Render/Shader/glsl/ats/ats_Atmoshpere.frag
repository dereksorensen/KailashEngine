out vec4 color;

in vec3 ray;
in vec2 v_TexCoord;

//------------------------------------------------------
// Camera Spatials
//------------------------------------------------------
layout(std140, binding = 1) uniform cameraSpatials
{
	mat4 view;
	mat4 perspective;
	mat4 inv_view_perspective;
	mat4 previous_view_persepctive;
	mat4 inv_previous_view_persepctive;
	vec3 cam_position;
	vec3 cam_look;
};


uniform sampler2D irradianceSampler;	//precomputed skylight irradiance (E table)
uniform sampler3D inscatterSampler;		//precomputed inscattered light (S table)

uniform sampler2D sampler0;		// Normal
uniform sampler2D sampler1;		// Diffuse
uniform sampler2D sampler2;		// Specular
uniform sampler2D sampler3;		// Scene

uniform vec3 sun_position;


const float ISun = 100.0;

//------------------------------------------------------
// Inscatter
//------------------------------------------------------

//inscattered light along ray x+tv, when sun in direction s (=S[L]-T(x,x0)S[L]|x0)
vec3 inscatter(inout vec3 x, inout float t, vec3 v, vec3 s, float angleOfInc, out float r, out float mu, out vec3 attenuation, out vec3 occluder_a) 
{
	vec3 result;
    r = length(x);
    mu = dot(x, v) / r;
    float d = -r * mu - sqrt(r * r * (mu * mu - 1.0) + Rt * Rt);

	bool inSpace = false;

	
    if (d > 0.0) { // if x in space and ray intersects atmosphere	
		vec3 x0 = x + t * v;
		float r0 = length(x0);

		// if terrain is above atmosphere, 
		if(r0 > Rt && t < d && t != -1.0)
		{
			attenuation = vec3(1.0);
			return vec3(0.0);
		}

        // move x to nearest intersection of ray with top atmosphere boundary
        x += d * v;
        t -= d;
        mu = (r * mu + d) / Rt;
        r = Rt;

		inSpace = true;
    }


	// if ray intersects atmosphere
    if (r <= Rt) { 
        float nu = dot(v, s);
        float muS = dot(x, s) / r;
        float phaseR = phaseFunctionR(nu);
        float phaseM = phaseFunctionM(nu);
		vec4 inscatter = max(texture4D(inscatterSampler, r, mu, muS, nu), 0.0 );
		occluder_a = (inSpace) ? vec3(0.0) : inscatter.rgb;



        if (t > 0.0) {

			occluder_a = vec3(0.0);

			vec3 x0 = x + t * v;

			

			
			float r0 = length(x0);
			float rMu0 = dot(x0, v);
			float mu0 = rMu0 / r0;
			float muS0 = dot(x0, s) / r0;


			// avoids imprecision problems in transmittance computations based on textures
			attenuation = clamp(analyticTransmittance(r, mu, t), 0.0, 1.0);



			// Terrain in atmposhere
			if (r0 > Rg + 0.00001 && r0 < Rt) 
			{
				// computes S[L]-T(x,x0)S[L]|x0
				//inscatter = max(inscatter - attenuation.rgbr * texture4D(inscatterSampler, r0, mu0, muS0, nu), 0.0);


				// avoids imprecision problems near horizon by interpolating between two points above and below horizon
				const float EPS = 0.004;
				float muHoriz = -sqrt(1.0 - (Rg / r) * (Rg / r));
				if (abs(mu - muHoriz) < EPS) {
					float a = ((mu - muHoriz) + EPS) / (2.0 * EPS);

					mu = muHoriz - EPS;
					r0 = sqrt(r * r + t * t + 2.0 * r * t * mu);
					mu0 = (r * mu + t) / r0;
					vec4 inScatter0 = texture4D(inscatterSampler, r, mu, muS, nu);
					vec4 inScatter1 = texture4D(inscatterSampler, r0, mu0, muS0, nu);
					vec4 inScatterA = max(inScatter0 - attenuation.rgbr * inScatter1, 0.0);

					mu = muHoriz + EPS;
					r0 = sqrt(r * r + t * t + 2.0 * r * t * mu);
					mu0 = (r * mu + t) / r0;
					inScatter0 = texture4D(inscatterSampler, r, mu, muS, nu);
					inScatter1 = texture4D(inscatterSampler, r0, mu0, muS0, nu);
					vec4 inScatterB = max(inScatter0 - attenuation.rgbr * inScatter1, 0.0);

					inscatter = mix(inScatterA, inScatterB, a);
				}
				else 
				{
					vec4 inScatter0 = inscatter;
					vec4 inScatter1 = texture4D(inscatterSampler, r0, mu0, muS0, nu);
					inscatter = max(inScatter0 - inScatter1 * attenuation.rgbr, 0.0);
				}
			}


        }

        // avoids imprecision problems in Mie scattering when sun is below horizon
        inscatter.w *= smoothstep(0.00, 0.02, muS);

        result = max(inscatter.rgb * phaseR + getMie(inscatter) * phaseM, 0.0); //  * max(angleOfInc, 0.0)
    } else { // x in space and ray looking in space
        result = vec3(0.0);
		attenuation = vec3(1.0);
		occluder_a = vec3(0.0);
    }
    return result * ISun;

}

//------------------------------------------------------
// Ground Color
//------------------------------------------------------


//------------------------------------------------------
// Sun Color
//------------------------------------------------------

// direct sun light for ray x+tv, when sun in direction s (=L0)
vec3 sunColor(float t, vec3 view_ray, float r, float mu) {
    if (t > 0.0) {
        return vec3(0.0);
    } else {
        vec3 transmittance = r <= Rt ? transmittanceWithShadow(r, mu) : vec3(1.0); // T(x,xo)
        float isun = smoothstep(cos((M_PI+4.0) / 180.0), cos(M_PI / 180.0), dot(view_ray, sun_position)) * ISun;// * 10.0; // Lsun
        return transmittance * isun; // Eq (9)
    }
}


//------------------------------------------------------
// Main
//------------------------------------------------------

void main() 
{
	vec3 cam_position = -cam_position;
	vec3 view_ray = normalize(ray);

	vec4 normal_depth = texture(sampler0, v_TexCoord);
	float depth = normal_depth.a;
	vec3 normal = normal_depth.rgb;

	vec4 diffuse_id = texture(sampler1, v_TexCoord);
	float object_id = diffuse_id.a;
	vec3 diffuse = diffuse_id.rgb;

	vec3 scene = texture(sampler3, v_TexCoord).rgb;

	vec3 world_position = calcWorldPosition(depth, view_ray, cam_position);


	depth = depth != 1 ? depth : -1.0;

	float r = length(cam_position);
	float mu = dot(cam_position, view_ray) / r;
	float t = -r * mu - sqrt(r * r * (mu * mu - 1.0) + Rg * Rg);

	vec3 occluder_a;
	vec3 attenuation;
	vec3 color_inscatter = inscatter(cam_position, t, view_ray, sun_position, 0, r, mu, attenuation, occluder_a);	// S[L]-T(x,xs)S[l]|xs
	vec3 color_ground = vec3(0.0);
	vec3 color_sun = sunColor(t, view_ray, r, mu);		// L0

	vec3 final = color_inscatter + color_ground + color_sun;

	if(object_id == 2)
	{
		vec3 space = diffuse;
		final += max((space) * mix(vec3(0.0),vec3(1.0),0.2-length(final)), 0.0);
	}
	else
	{
		final += scene;
	}

    color = vec4(view_ray, 1.0);
	color = vec4(color_inscatter, 1.0);
	//color = vec4(t);


	color = vec4(final, 1.0);

}