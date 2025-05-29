using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using NaughtyAttributes;

[System.Serializable]
public class BoolAnimationParameter
{
    [Tooltip("Nombre del parámetro en el Animator")]
    public string parameterName;

    [ReadOnly]
    [Tooltip("Valor actual del parámetro")]
    public bool currentValue;

    [Space]
    [Tooltip("Se activa cuando el parámetro se establece en verdadero")]
    public UnityEvent OnSetTrue;

    [Tooltip("Se activa cuando el parámetro se establece en falso")]
    public UnityEvent OnSetFalse;
}

[System.Serializable]
public class FloatAnimationParameter
{
    [Tooltip("Nombre del parámetro en el Animator")]
    public string parameterName;

    [ReadOnly]
    [Tooltip("Valor actual del parámetro")]
    public float currentValue;

    [Space]
    [Tooltip("Se activa cuando el valor del parámetro cambia")]
    public UnityEvent OnValueChanged;
}

public class AnimatorManager : MonoBehaviour
{
    [Required]
    [SerializeField, Tooltip("Referencia al componente Animator")]
    private Animator targetAnimator;

    [BoxGroup("Boolean Parameters")]
    [ReorderableList]
    [SerializeField] private List<BoolAnimationParameter> boolParameters = new List<BoolAnimationParameter>();

    [BoxGroup("Float Parameters")]
    [ReorderableList]
    [SerializeField] private List<FloatAnimationParameter> floatParameters = new List<FloatAnimationParameter>();

    // Diccionarios para acceso rápido
    private Dictionary<string, BoolAnimationParameter> boolParameterLookup;
    private Dictionary<string, FloatAnimationParameter> floatParameterLookup;

    private void Awake()
    {
        InitializeLookupDictionaries();
        SyncInitialValues();
    }

    private void InitializeLookupDictionaries()
    {
        boolParameterLookup = new Dictionary<string, BoolAnimationParameter>();
        floatParameterLookup = new Dictionary<string, FloatAnimationParameter>();

        foreach (var param in boolParameters)
        {
            boolParameterLookup[param.parameterName] = param;
        }

        foreach (var param in floatParameters)
        {
            floatParameterLookup[param.parameterName] = param;
        }
    }

    private void SyncInitialValues()
    {
        if (targetAnimator == null) return;

        // Sincronizar valores iniciales de parámetros booleanos
        foreach (var param in boolParameters)
        {
            param.currentValue = targetAnimator.GetBool(param.parameterName);
        }

        // Sincronizar valores iniciales de parámetros flotantes
        foreach (var param in floatParameters)
        {
            param.currentValue = targetAnimator.GetFloat(param.parameterName);
        }
    }

    private void Update()
    {
        if (targetAnimator == null) return;

        UpdateBoolParameters();
        UpdateFloatParameters();
    }

    private void UpdateBoolParameters()
    {
        foreach (var param in boolParameters)
        {
            bool animatorValue = targetAnimator.GetBool(param.parameterName);

            // Solo actualizar si hay un cambio
            if (animatorValue != param.currentValue)
            {
                param.currentValue = animatorValue;

                // Disparar evento apropiado
                if (param.currentValue)
                {
                    param.OnSetTrue?.Invoke();
                }
                else
                {
                    param.OnSetFalse?.Invoke();
                }
            }
        }
    }

    private void UpdateFloatParameters()
    {
        foreach (var param in floatParameters)
        {
            float animatorValue = targetAnimator.GetFloat(param.parameterName);

            // Solo actualizar si hay un cambio significativo
            if (!Mathf.Approximately(animatorValue, param.currentValue))
            {
                param.currentValue = animatorValue;
                param.OnValueChanged?.Invoke();
            }
        }
    }

    #region Public Methods

    // ====================
    // MÉTODOS BOOLEANOS
    // ====================

    public void SetBoolTrue(string parameterName)
    {
        if (targetAnimator == null) return;

        if (boolParameterLookup.TryGetValue(parameterName, out BoolAnimationParameter param))
        {
            targetAnimator.SetBool(parameterName, true);
            param.currentValue = true;
            param.OnSetTrue?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Parámetro booleano '{parameterName}' no encontrado");
        }
    }

    public void SetBoolFalse(string parameterName)
    {
        if (targetAnimator == null) return;

        if (boolParameterLookup.TryGetValue(parameterName, out BoolAnimationParameter param))
        {
            targetAnimator.SetBool(parameterName, false);
            param.currentValue = false;
            param.OnSetFalse?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Parámetro booleano '{parameterName}' no encontrado");
        }
    }

    public void ToggleBool(string parameterName)
    {
        if (targetAnimator == null) return;

        if (boolParameterLookup.TryGetValue(parameterName, out BoolAnimationParameter param))
        {
            bool newValue = !param.currentValue;
            targetAnimator.SetBool(parameterName, newValue);
            param.currentValue = newValue;

            if (newValue) param.OnSetTrue?.Invoke();
            else param.OnSetFalse?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Parámetro booleano '{parameterName}' no encontrado");
        }
    }

    public bool GetBoolValue(string parameterName)
    {
        if (boolParameterLookup.TryGetValue(parameterName, out BoolAnimationParameter param))
        {
            return param.currentValue;
        }

        Debug.LogWarning($"Parámetro booleano '{parameterName}' no encontrado");
        return false;
    }

    // ===================
    // MÉTODOS FLOTANTES
    // ===================

    public void SetFloatValue(string parameterName, float newValue)
    {
        if (targetAnimator == null) return;

        if (floatParameterLookup.TryGetValue(parameterName, out FloatAnimationParameter param))
        {
            targetAnimator.SetFloat(parameterName, newValue);
            param.currentValue = newValue;
            param.OnValueChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Parámetro flotante '{parameterName}' no encontrado");
        }
    }

    public float GetFloatValue(string parameterName)
    {
        if (floatParameterLookup.TryGetValue(parameterName, out FloatAnimationParameter param))
        {
            return param.currentValue;
        }

        Debug.LogWarning($"Parámetro flotante '{parameterName}' no encontrado");
        return 0f;
    }
    #endregion

    #region Editor Helpers
    // Métodos para facilitar la configuración en el editor
    [Button("Cargar Parámetros del Animator")]
    private void LoadAnimatorParameters()
    {
        if (targetAnimator == null) return;

        // Limpiar listas existentes
        boolParameters.Clear();
        floatParameters.Clear();

        // Cargar parámetros del Animator
        foreach (var param in targetAnimator.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                    boolParameters.Add(new BoolAnimationParameter
                    {
                        parameterName = param.name,
                        currentValue = targetAnimator.GetBool(param.name)
                    });
                    break;

                case AnimatorControllerParameterType.Float:
                    floatParameters.Add(new FloatAnimationParameter
                    {
                        parameterName = param.name,
                        currentValue = targetAnimator.GetFloat(param.name)
                    });
                    break;
            }
        }

        InitializeLookupDictionaries();
        Debug.Log($"Se cargaron {boolParameters.Count} parámetros booleanos y {floatParameters.Count} parámetros flotantes");
    }
    #endregion
}