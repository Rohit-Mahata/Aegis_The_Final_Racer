using UnityEngine;

public class CarLightingSystem : MonoBehaviour
{
    [Header("Headlights")]
    [SerializeField] private Light[] headlightLights;
    [SerializeField] private Renderer headlightRenderer;
    [SerializeField] private int headlightMaterialIndex = 1;

    [Header("Rear Lights")]
    [SerializeField] private Renderer rearLightRenderer;
    [SerializeField] private int rearLightMaterialIndex = 3;

    [Header("Back Light / Indicators")]
    [SerializeField] private Renderer backLightRenderer;
    [SerializeField] private int backLightMaterialIndex = 4;

    [Header("Controls")]
    [SerializeField] private KeyCode headlightKey = KeyCode.L;

    private bool headlightsOn;

    private Material headlightMaterial;
    private Material rearLightMaterial;
    private Material backLightMaterial;

    private Color emissionColor;
    private bool hasEmission;


    private void Awake()
    {
        // =====================================================
        // HEADLIGHT MATERIAL
        // =====================================================

        if (headlightRenderer == null)
        {
            Debug.LogError(
                "CarLightingSystem: Headlight Renderer is not assigned!"
            );
        }
        else
        {
            Material[] materials = headlightRenderer.materials;

            if (headlightMaterialIndex >= 0 &&
                headlightMaterialIndex < materials.Length)
            {
                headlightMaterial =
                    materials[headlightMaterialIndex];

                hasEmission =
                    headlightMaterial.HasProperty("_EmissionColor");

                if (hasEmission)
                {
                    emissionColor =
                        headlightMaterial.GetColor("_EmissionColor");
                }
                else
                {
                    Debug.LogError(
                        "Headlight material does not have an _EmissionColor property."
                    );
                }

                Debug.Log(
                    $"Headlight material bound: {headlightMaterial.name}"
                );
            }
            else
            {
                Debug.LogError(
                    "CarLightingSystem: Invalid headlight material index!"
                );
            }
        }


        // =====================================================
        // REAR LIGHT MATERIAL
        // rearlight1 = Material Element 3
        // =====================================================

        if (rearLightRenderer != null)
        {
            Material[] materials = rearLightRenderer.materials;

            if (rearLightMaterialIndex >= 0 &&
                rearLightMaterialIndex < materials.Length)
            {
                rearLightMaterial =
                    materials[rearLightMaterialIndex];

                Debug.Log(
                    $"Rear light material bound: {rearLightMaterial.name}"
                );
            }
            else
            {
                Debug.LogError(
                    "CarLightingSystem: Invalid rear light material index!"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "CarLightingSystem: Rear Light Renderer is not assigned."
            );
        }


        // =====================================================
        // BACK LIGHT / INDICATOR MATERIAL
        // Back_Light = Material Element 4
        // =====================================================

        if (backLightRenderer != null)
        {
            Material[] materials = backLightRenderer.materials;

            if (backLightMaterialIndex >= 0 &&
                backLightMaterialIndex < materials.Length)
            {
                backLightMaterial =
                    materials[backLightMaterialIndex];

                Debug.Log(
                    $"Back light material bound: {backLightMaterial.name}"
                );
            }
            else
            {
                Debug.LogError(
                    "CarLightingSystem: Invalid back light material index!"
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "CarLightingSystem: Back Light Renderer is not assigned."
            );
        }
    }


    private void Start()
    {
        // Headlights start OFF
        SetHeadlights(false);

        // Rear lights start OFF
        SetRearLightEmission(false);

        // Indicators start OFF
        SetBackLightEmission(false);
    }


    private void Update()
    {
        // =====================================================
        // HEADLIGHTS
        // L = Toggle headlights
        // =====================================================

        if (Input.GetKeyDown(headlightKey))
        {
            SetHeadlights(!headlightsOn);
        }


        // =====================================================
        // BRAKE + REVERSE
        // SPACE = Brake
        // S     = Reverse
        //
        // Either one turns rearlight1 ON.
        // Releasing both turns it OFF.
        // =====================================================

        bool brakeOrReverse =
            Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.S);

        SetRearLightEmission(brakeOrReverse);


        // =====================================================
        // INDICATORS
        // A = Left indicator input
        // D = Right indicator input
        //
        // For now both use the Back_Light material.
        // =====================================================

        bool indicator =
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D);

        SetBackLightEmission(indicator);
    }


    // =========================================================
    // HEADLIGHT SYSTEM
    // =========================================================

    private void SetHeadlights(bool state)
    {
        headlightsOn = state;

        // Turn physical headlights ON/OFF
        if (headlightLights != null)
        {
            foreach (Light light in headlightLights)
            {
                if (light != null)
                {
                    light.enabled = state;
                }
            }
        }


        // Turn headlight material emission ON/OFF
        if (headlightMaterial != null && hasEmission)
        {
            if (state)
            {
                headlightMaterial.EnableKeyword("_EMISSION");

                headlightMaterial.SetColor(
                    "_EmissionColor",
                    emissionColor
                );

                headlightMaterial.globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                headlightMaterial.SetColor(
                    "_EmissionColor",
                    Color.black
                );

                headlightMaterial.DisableKeyword("_EMISSION");

                headlightMaterial.globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }
        }
    }


    // =========================================================
    // BRAKE + REVERSE LIGHT
    // =========================================================

    private void SetRearLightEmission(bool state)
    {
        if (rearLightMaterial == null)
            return;

        if (!rearLightMaterial.HasProperty("_EmissionColor"))
            return;

        if (state)
        {
            // Use the emission color already configured
            // in rearlight1.
            rearLightMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            rearLightMaterial.DisableKeyword("_EMISSION");
        }
    }


    // =========================================================
    // A + D BACK LIGHT / INDICATOR
    // =========================================================

    private void SetBackLightEmission(bool state)
    {
        if (backLightMaterial == null)
            return;

        if (!backLightMaterial.HasProperty("_EmissionColor"))
            return;

        if (state)
        {
            // Use the emission color already configured
            // in Back_Light.
            backLightMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            backLightMaterial.DisableKeyword("_EMISSION");
        }
    }
}