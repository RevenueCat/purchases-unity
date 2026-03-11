using System;
using System.Collections.Generic;
using UnityEngine;
using RevenueCatUI;

public class CustomVariablesEditor : MonoBehaviour
{
    [Serializable]
    public struct CustomVariable
    {
        public string key;
        public string value;
    }

    [Header("Custom Variables (edit in Inspector)")]
    public List<CustomVariable> variables = new List<CustomVariable>();

    public Dictionary<string, CustomVariableValue> GetCustomVariablesForPaywall()
    {
        if (variables.Count == 0)
            return null;

        var result = new Dictionary<string, CustomVariableValue>();
        foreach (var v in variables)
        {
            if (!string.IsNullOrEmpty(v.key))
                result[v.key] = CustomVariableValue.String(v.value ?? "");
        }
        return result.Count > 0 ? result : null;
    }
}
