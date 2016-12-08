
out vec4 color;

void main() 
{
    float r, muS;
    getIrradianceRMuS(r, muS);
    color = vec4(transmittance(r, muS) * max(muS, 0.0), 0.0);
}