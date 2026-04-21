using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

[CreateAssetMenu(fileName = "NewIngredient", menuName = "YesChef/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public int scoreValue;
    public GameObject rawPrefab;

    public bool preparationRequired;

    public PreparationType requiredPrepType;

    public float requiredPrepTime;
    public GameObject preparedPrefab;
}

#if UNITY_EDITOR
[CustomEditor(typeof(IngredientData))]
public class IngredientDataEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        root.Add(new PropertyField(serializedObject.FindProperty("ingredientName")));
        root.Add(new PropertyField(serializedObject.FindProperty("scoreValue")));
        root.Add(new PropertyField(serializedObject.FindProperty("rawPrefab")));

        SerializedProperty requiresPrepProp = serializedObject.FindProperty("preparationRequired");
        PropertyField requiresPrepField = new PropertyField(requiresPrepProp);
        root.Add(requiresPrepField);

        VisualElement prepContainer = new VisualElement();
        prepContainer.style.paddingLeft = 15;
        prepContainer.Add(new PropertyField(serializedObject.FindProperty("requiredPrepType")));
        prepContainer.Add(new PropertyField(serializedObject.FindProperty("requiredPrepTime")));
        prepContainer.Add(new PropertyField(serializedObject.FindProperty("preparedPrefab")));
        root.Add(prepContainer);

        void UpdateVisibility()
        {
            bool isPrepRequired = requiresPrepProp.boolValue;
            prepContainer.style.display = isPrepRequired ? DisplayStyle.Flex : DisplayStyle.None;

            if (!isPrepRequired)
            {
                serializedObject.FindProperty("requiredPrepType").objectReferenceValue = null;
                serializedObject.ApplyModifiedProperties();
            }
        }

        requiresPrepField.RegisterValueChangeCallback(evt => UpdateVisibility());

        UpdateVisibility();

        return root;
    }
}
#endif
