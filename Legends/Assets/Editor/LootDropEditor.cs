using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(LootDrop))]
public class LootDropEditor : Editor
{
    SerializedProperty m_PlayerProp;
    SerializedProperty m_SpawnSoundProp;
    SerializedProperty m_PickUpSoundProp;
    SerializedProperty m_SpawnEventProp;
    SerializedProperty m_LootProp;

    bool[] m_FoldoutInfos;

    int toDelete = -1;

    void OnEnable()
    {
        //m_PlayerProp = serializedObject.FindProperty("Player");
        //m_SpawnSoundProp = serializedObject.FindProperty("SpawnedClip");
        //m_PickUpSoundProp = serializedObject.FindProperty("LootPickUpClip");
        m_SpawnEventProp = serializedObject.FindProperty("Events");
        //m_LootProp = serializedObject.FindProperty("Loot");

        m_FoldoutInfos = new bool[m_SpawnEventProp.arraySize];

        Undo.undoRedoPerformed += RecomputeFoldout;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= RecomputeFoldout;
    }

    void RecomputeFoldout()
    {
        serializedObject.Update();

        var newFoldout = new bool[m_SpawnEventProp.arraySize];
        Array.Copy(m_FoldoutInfos, newFoldout, Mathf.Min(m_FoldoutInfos.Length, newFoldout.Length));
        m_FoldoutInfos = newFoldout;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //EditorGUILayout.PropertyField(m_LootProp);
        //EditorGUILayout.PropertyField(m_PlayerProp);
        //EditorGUILayout.PropertyField(m_SpawnSoundProp);
        //EditorGUILayout.PropertyField(m_PickUpSoundProp);

        for (int i = 0; i < m_SpawnEventProp.arraySize; ++i)
        {
            var i1 = i;
            m_FoldoutInfos[i] = EditorGUILayout.BeginFoldoutHeaderGroup(m_FoldoutInfos[i], $"Slot {i}", null, (rect) => { ShowHeaderContextMenu(rect, i1); });

            if (m_FoldoutInfos[i])
            {
                var entriesArrayProp = m_SpawnEventProp.GetArrayElementAtIndex(i).FindPropertyRelative("Entries");

                int localToDelete = -1;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Item");
                GUILayout.Label("Weight");
                GUILayout.Space(16);
                EditorGUILayout.EndHorizontal();

                for (int j = 0; j < entriesArrayProp.arraySize; ++j)
                {
                    var entryProp = entriesArrayProp.GetArrayElementAtIndex(j);

                    var itemProp = entryProp.FindPropertyRelative("Item");
                    var weightProp = entryProp.FindPropertyRelative("Weight");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(itemProp, GUIContent.none);
                    EditorGUILayout.PropertyField(weightProp, GUIContent.none);
                    if (GUILayout.Button("-", GUILayout.Width(16)))
                    {
                        localToDelete = j;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (localToDelete != -1)
                {
                    entriesArrayProp.DeleteArrayElementAtIndex(localToDelete);
                }

                if (GUILayout.Button("Add New Entry", GUILayout.Width(100)))
                {
                    entriesArrayProp.InsertArrayElementAtIndex(entriesArrayProp.arraySize);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        if (toDelete != -1)
        {
            m_SpawnEventProp.DeleteArrayElementAtIndex(toDelete);
            ArrayUtility.RemoveAt(ref m_FoldoutInfos, toDelete);
            toDelete = -1;
        }

        if (GUILayout.Button("Add new Slot"))
        {
            m_SpawnEventProp.InsertArrayElementAtIndex(m_SpawnEventProp.arraySize);
            serializedObject.ApplyModifiedProperties();

            //insert will copy the last element, which can lead to having to empty a large spawn event to start new
            //so we manually "empty" the new event
            var newElem = m_SpawnEventProp.GetArrayElementAtIndex(m_SpawnEventProp.arraySize - 1);
            var entries = newElem.FindPropertyRelative("Entries");

            entries.ClearArray();

            ArrayUtility.Add(ref m_FoldoutInfos, false);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void ShowHeaderContextMenu(Rect position, int index)
    {
        var menu = new GenericMenu();
        menu.AddItem(new GUIContent("Remove"), false, () => { toDelete = index; });
        menu.DropDown(position);
    }
}