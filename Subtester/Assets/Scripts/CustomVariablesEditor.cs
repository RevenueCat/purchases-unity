using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RevenueCatUI;

/// <summary>
/// A simple editor for custom paywall variables.
/// Allows adding, viewing, and removing custom variables that will be passed to paywalls.
/// </summary>
public class CustomVariablesEditor : MonoBehaviour
{
    [Header("UI References")]
    public GameObject editorPanel;
    public InputField keyInputField;
    public InputField valueInputField;
    public Text variablesListText;
    public Button addButton;
    public Button clearAllButton;
    public Button closeButton;
    public Text statusText;

    /// <summary>
    /// The custom variables dictionary that will be used when presenting paywalls.
    /// </summary>
    public Dictionary<string, string> CustomVariables { get; private set; } = new Dictionary<string, string>();

    /// <summary>
    /// Event fired when custom variables are updated.
    /// </summary>
    public event Action<Dictionary<string, string>> OnVariablesChanged;

    private void Start()
    {
        if (addButton != null)
            addButton.onClick.AddListener(AddVariable);

        if (clearAllButton != null)
            clearAllButton.onClick.AddListener(ClearAllVariables);

        if (closeButton != null)
            closeButton.onClick.AddListener(HideEditor);

        // Add some sample variables for testing
        AddSampleVariables();

        UpdateVariablesList();

        // Start hidden
        if (editorPanel != null)
            editorPanel.SetActive(false);
    }

    private void AddSampleVariables()
    {
        // Add some sample variables that are commonly used
        CustomVariables["player_name"] = "Unity Tester";
        CustomVariables["level"] = "42";
        CustomVariables["coins"] = "1000";
    }

    public void ShowEditor()
    {
        if (editorPanel != null)
        {
            editorPanel.SetActive(true);
            UpdateVariablesList();
        }
    }

    public void HideEditor()
    {
        if (editorPanel != null)
            editorPanel.SetActive(false);
    }

    public void ToggleEditor()
    {
        if (editorPanel != null)
        {
            editorPanel.SetActive(!editorPanel.activeSelf);
            if (editorPanel.activeSelf)
                UpdateVariablesList();
        }
    }

    public bool IsEditorVisible()
    {
        return editorPanel != null && editorPanel.activeSelf;
    }

    private void AddVariable()
    {
        if (keyInputField == null || valueInputField == null)
        {
            SetStatus("Error: Input fields not configured");
            return;
        }

        string key = keyInputField.text?.Trim();
        string value = valueInputField.text?.Trim();

        if (string.IsNullOrEmpty(key))
        {
            SetStatus("Error: Key cannot be empty");
            return;
        }

        // Validate key format (should start with letter, contain only letters, numbers, underscores)
        if (!IsValidVariableKey(key))
        {
            SetStatus("Error: Key must start with a letter and contain only letters, numbers, and underscores");
            return;
        }

        CustomVariables[key] = value ?? "";

        // Clear input fields
        keyInputField.text = "";
        valueInputField.text = "";

        UpdateVariablesList();
        OnVariablesChanged?.Invoke(CustomVariables);

        SetStatus($"Added: {key} = {value}");
        Debug.Log($"[CustomVariablesEditor] Added variable: {key} = {value}");
    }

    private bool IsValidVariableKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return false;

        // Must start with a letter
        if (!char.IsLetter(key[0]))
            return false;

        // Can only contain letters, numbers, and underscores
        foreach (char c in key)
        {
            if (!char.IsLetterOrDigit(c) && c != '_')
                return false;
        }

        return true;
    }

    public void RemoveVariable(string key)
    {
        if (CustomVariables.ContainsKey(key))
        {
            CustomVariables.Remove(key);
            UpdateVariablesList();
            OnVariablesChanged?.Invoke(CustomVariables);
            SetStatus($"Removed: {key}");
            Debug.Log($"[CustomVariablesEditor] Removed variable: {key}");
        }
    }

    private void ClearAllVariables()
    {
        CustomVariables.Clear();
        UpdateVariablesList();
        OnVariablesChanged?.Invoke(CustomVariables);
        SetStatus("All variables cleared");
        Debug.Log("[CustomVariablesEditor] All variables cleared");
    }

    private void UpdateVariablesList()
    {
        if (variablesListText == null)
            return;

        if (CustomVariables.Count == 0)
        {
            variablesListText.text = "(No custom variables defined)\n\nVariables use {{ custom.key }} syntax in paywalls.";
            return;
        }

        var lines = new List<string>();
        lines.Add($"Custom Variables ({CustomVariables.Count}):");
        lines.Add("----------------------------");

        foreach (var kvp in CustomVariables)
        {
            // Show variable and how it would be used in a paywall
            lines.Add($"  {kvp.Key} = \"{kvp.Value}\"");
            lines.Add($"    Usage: {{{{ custom.{kvp.Key} }}}}");
        }

        variablesListText.text = string.Join("\n", lines);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log($"[CustomVariablesEditor] {message}");
    }

    /// <summary>
    /// Gets the custom variables as a dictionary suitable for PaywallOptions.
    /// Returns null if no variables are defined.
    /// </summary>
    public Dictionary<string, CustomVariableValue> GetCustomVariablesForPaywall()
    {
        if (CustomVariables.Count == 0)
            return null;

        var result = new Dictionary<string, CustomVariableValue>();
        foreach (var kvp in CustomVariables)
        {
            result[kvp.Key] = CustomVariableValue.String(kvp.Value);
        }
        return result;
    }
}
