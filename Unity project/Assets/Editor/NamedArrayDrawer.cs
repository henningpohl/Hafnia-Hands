using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// https://forum.unity.com/threads/how-to-change-the-name-of-list-elements-in-the-inspector.448910/
[CustomPropertyDrawer(typeof(NamedArrayAttribute))]
public class NamedArrayDrawer : PropertyDrawer {
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label) {
        try {
            int pos = int.Parse(property.propertyPath.Split('[', ']')[1]);
            EditorGUI.ObjectField(rect, property, new GUIContent(((NamedArrayAttribute)attribute).names[pos]));
        } catch {
            EditorGUI.ObjectField(rect, property, label);
        }
    }
}
