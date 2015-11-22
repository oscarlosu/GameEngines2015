using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(EditorObjectTest))]
public class SceneTest : Editor
{

    public void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        EditorObjectTest t = (EditorObjectTest)target;

        Handles.BeginGUI();

        t.chosenIndex = EditorGUILayout.IntPopup("Tile", t.chosenIndex, t.IntArray.Select(x => x.ToString()).ToArray(), t.IntArray);

        Handles.EndGUI();

        switch (Event.current.type)
        {
            case EventType.MouseDown:
                if (Event.current.button == 0)
                {
                    Debug.Log("Mouse left down");
                    Debug.Log("Screen pos: " + Event.current.mousePosition);
                    var mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                    Debug.Log("World pos: " + mousePos);
                }
                else
                {
                    Debug.Log("Mouse other than left down");
                }
                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(t);
        }
    }


}
