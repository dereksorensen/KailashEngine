
out vec4 color;

uniform float r;
uniform vec4 dhdH;
uniform int layer;

uniform sampler3D deltaSRSampler;
uniform sampler3D deltaSMSampler;


void main() 
{
    vec3 uvw = vec3(gl_FragCoord.xy, float(layer) + 0.5) / vec3(ivec3(RES_MU_S * RES_NU, RES_MU, RES_R));
    vec4 ray = texture(deltaSRSampler, uvw);
    vec4 mie = texture(deltaSMSampler, uvw);
    color = vec4(ray.rgb, mie.r); // store only red component of single Mie scattering (cf. "Angular precision")
}