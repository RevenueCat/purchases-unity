using UnityEditor;
using UnityEngine;
using static RevenueCatUI.PaywallsBehaviour;

namespace RevenueCatUI.Editor
{
    [CustomPropertyDrawer(typeof(CustomVariableEntry))]
    public class CustomVariableEntryDrawer : PropertyDrawer
    {
        private const float TypeWidth = 70f;
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var keyProp = property.FindPropertyRelative("key");
            var typeProp = property.FindPropertyRelative("type");
            var valueProp = property.FindPropertyRelative("value");

            var availableWidth = position.width - TypeWidth - Spacing * 2;
            var fieldWidth = availableWidth / 2f;

            var keyRect = new Rect(position.x, position.y, fieldWidth, position.height);
            var typeRect = new Rect(keyRect.xMax + Spacing, position.y, TypeWidth, position.height);
            var valueRect = new Rect(typeRect.xMax + Spacing, position.y, fieldWidth, position.height);

            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
            EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

            var variableType = (CustomVariableType)typeProp.enumValueIndex;
            DrawValueField(valueRect, valueProp, variableType);

            EditorGUI.EndProperty();
        }

        private static void DrawValueField(Rect rect, SerializedProperty valueProp, CustomVariableType type)
        {
            switch (type)
            {
                case CustomVariableType.Boolean:
                    var boolValue = valueProp.stringValue == "true" || valueProp.stringValue == "True";
                    var newBool = EditorGUI.Toggle(rect, boolValue);
                    valueProp.stringValue = newBool ? "true" : "false";
                    break;
                case CustomVariableType.Number:
                    if (double.TryParse(valueProp.stringValue, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var numValue))
                    {
                        var newNum = EditorGUI.DoubleField(rect, numValue);
                        valueProp.stringValue = newNum.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        var newNum = EditorGUI.DoubleField(rect, 0);
                        valueProp.stringValue = newNum.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    }
                    break;
                default:
                    EditorGUI.PropertyField(rect, valueProp, GUIContent.none);
                    break;
            }
        }
    }
}
