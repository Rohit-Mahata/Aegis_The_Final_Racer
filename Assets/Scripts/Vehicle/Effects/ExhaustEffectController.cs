using UnityEngine;

public class ExhaustEffectController : MonoBehaviour
{
    [Header("Exhaust Effects")]
    [SerializeField] private ParticleSystem[] exhaustParticles;

    [Header("Emission")]
    [SerializeField] private float minEmission = 2f;
    [SerializeField] private float maxEmission = 20f;

    private ParticleSystem.EmissionModule[] emissions;

    private void Awake()
    {
        emissions = new ParticleSystem.EmissionModule[exhaustParticles.Length];

        for (int i = 0; i < exhaustParticles.Length; i++)
        {
            if (exhaustParticles[i] != null)
                emissions[i] = exhaustParticles[i].emission;
        }
    }

    public void SetExhaust(float throttle)
    {
        throttle = Mathf.Clamp01(throttle);

        float rate = Mathf.Lerp(minEmission, maxEmission, throttle);

        for (int i = 0; i < emissions.Length; i++)
        {
            emissions[i].rateOverTime = rate;
        }
    }

    public void SetExhaustEnabled(bool enabled)
    {
        for (int i = 0; i < exhaustParticles.Length; i++)
        {
            if (exhaustParticles[i] == null)
                continue;

            if (enabled)
                exhaustParticles[i].Play();
            else
                exhaustParticles[i].Stop();
        }
    }
}