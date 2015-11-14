using UnityEngine;
using UnityEditor;

//[ExecuteInEditMode]
public class EditorUpdaterTest : MonoBehaviour
{
    public int[] IntArray;
    public EditorObjectTest Grid;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}

[CustomEditor(typeof(EditorUpdaterTest), true)]
public class EditorUpdaterTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUI.changed)
        {
            ((EditorUpdaterTest)target).Grid.IntArray = ((EditorUpdaterTest) target).IntArray;
        }
    }
}
