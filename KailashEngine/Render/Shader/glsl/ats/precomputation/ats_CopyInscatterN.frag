
out vec4 color;

uniform float r;
uniform vec4 dhdH;
uniform int layer;

uniform sampler3D deltaSSampler;


void main()
{
    float mu, muS, nu;
    getMuMuSNu(r, dhdH, mu, muS, nu);
    vec3 uvw = vec3(gl_FragCoord.xy, float(layer) + 0.5) / vec3(ivec3(RES_MU_S * RES_NU, RES_MU, RES_R));
    color = vec4(texture(deltaSSampler, uvw).rgb / phaseFunctionR(nu), 0.0);
}
