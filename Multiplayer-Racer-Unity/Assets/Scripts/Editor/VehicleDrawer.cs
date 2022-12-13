using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(VehicleAttribute))]
public class VehicleDrawer : PropertyDrawer {
    bool showContent = false;
    //bool showLongDescription = false;
    static float margin = 4f;
    static float height = 16f;

    private GUIStyle style;
    private GUIStyle styleBold;
    private GUIStyle styleFoldout;

    public VehicleDrawer() {
        style = new GUIStyle();
        style.normal.textColor = new Color(0.67f, 0.67f, 0.67f);
        style.wordWrap = true;
        styleBold = new GUIStyle(style);
        styleBold.fontStyle = FontStyle.Bold;
        styleFoldout = EditorStyles.foldout;
        styleFoldout.fontStyle = FontStyle.Bold;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return (height * 2) + margin;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        VehicleSO vehicleSO = (VehicleSO)property.objectReferenceValue;
        Rect nextYPos = new Rect(position.x, position.y, position.width, height);
        EditorGUI.ObjectField(nextYPos, property);
        if (vehicleSO == null) return;
        var fieldValues = vehicleSO.GetType()
                     .GetFields()
                     .Select(field => field.GetValue(vehicleSO))
                     .ToList();
        var fieldNames = typeof(VehicleSO).GetFields()
                            .Select(field => field.Name)
                            .ToList();
        nextYPos = new Rect(nextYPos.x, nextYPos.y += height + margin, nextYPos.width, height);
        //EditorGUI.LabelField(nextYPos, "Name", pokemon.name, style);

        #region Stats

        showContent = EditorGUILayout.Foldout(showContent, vehicleSO.name, true, styleFoldout);
        if (showContent) {
            for (int i = 0; i < fieldValues.Count; i++) {
                EditorGUILayout.LabelField(fieldNames[i], fieldValues[i].ToString(), style);
            }
        }

        #endregion
    }
}
