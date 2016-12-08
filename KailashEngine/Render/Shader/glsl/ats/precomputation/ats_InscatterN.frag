
out vec4 color;

uniform float r;
uniform vec4 dhdH;
uniform int layer;

uniform sampler3D deltaJSampler;


vec3 integrand(float r, float mu, float muS, float nu, float t)
{
    float ri = sqrt(r * r + t * t + 2.0 * r * mu * t);
    float mui = (r * mu + t) / ri;
    float muSi = (nu * t + muS * r) / ri;
    return texture4D(deltaJSampler, ri, mui, muSi, nu).rgb * transmittance(r, mu, t);
}

vec3 inscatter(float r, float mu, float muS, float nu)
{
    vec3 raymie = vec3(0.0);
    float dx = limit(r, mu) / float(INSCATTER_INTEGRAL_SAMPLES);
    float xi = 0.0;
    vec3 raymiei = integrand(r, mu, muS, nu, 0.0);
    for (int i = 1; i <= INSCATTER_INTEGRAL_SAMPLES; ++i) {
        float xj = float(i) * dx;
        vec3 raymiej = integrand(r, mu, muS, nu, xj);
        raymie += (raymiei + raymiej) / 2.0 * dx;
        xi = xj;
        raymiei = raymiej;
    }
    return raymie;
}

void main()
{
    float mu, muS, nu;
    getMuMuSNu(r, dhdH, mu, muS, nu);
    color.rgb = inscatter(r, mu, muS, nu);
}
