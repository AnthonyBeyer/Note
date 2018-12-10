using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(Note))]
[CanEditMultipleObjects]
public class NoteEditor : Editor
{
    SerializedProperty m_Script;
    SerializedProperty m_myNoteTitle;
    SerializedProperty m_myNote;
    int oldColorIndex;
    SerializedProperty m_colorIndex;
    SerializedProperty m_myColor;
    SerializedProperty m_width;
    SerializedProperty m_drawBox;
    SerializedProperty m_boldBox;
    SerializedProperty m_links;

    string[] colorOptions = {"Yellow","Red","Magenta","Green","Cyan","Blue","Gray","White"};

    GUIStyle _textFieldStyle;
    GUIStyle textFieldStyle
    {
        get
        { 
            if (_textFieldStyle == null)
            {
                _textFieldStyle = new GUIStyle(EditorStyles.textField); // need a new GuiStyle 
                _textFieldStyle.wordWrap = true; // for wordwrapping (easier to work with this feature)
            }
            return _textFieldStyle;
        }
    }

	void OnEnable()
	{
        oldColorIndex = -1;
        m_Script = serializedObject.FindProperty("m_Script");
        m_myNoteTitle = serializedObject.FindProperty("myNoteTitle");
        m_myNote = serializedObject.FindProperty("myNote");
        m_colorIndex = serializedObject.FindProperty("colorIndex");
        m_myColor = serializedObject.FindProperty("myColor");
        m_width = serializedObject.FindProperty("width");
        m_drawBox = serializedObject.FindProperty("drawBox");
        m_boldBox = serializedObject.FindProperty("boldBox");
        m_links = serializedObject.FindProperty("links");
	}

	public override void OnInspectorGUI()
	{
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(m_Script);
        GUI.enabled = true;

        GUI.backgroundColor = m_myColor.colorValue;
        m_colorIndex.intValue = EditorGUILayout.Popup("Color", m_colorIndex.intValue, colorOptions);
        m_width.floatValue = EditorGUILayout.FloatField("Width", m_width.floatValue);

        EditorGUILayout.BeginHorizontal();
        m_drawBox.boolValue = EditorGUILayout.ToggleLeft("Outline", m_drawBox.boolValue);
        if (m_drawBox.boolValue) 
            m_boldBox.boolValue = EditorGUILayout.ToggleLeft("Bold", m_boldBox.boolValue);
        EditorGUILayout.EndHorizontal();

        if (oldColorIndex != m_colorIndex.intValue)
        {
            if (m_colorIndex.intValue == 0) m_myColor.colorValue = Color.yellow;
            if (m_colorIndex.intValue == 1) m_myColor.colorValue = Color.red;
            if (m_colorIndex.intValue == 2) m_myColor.colorValue = Color.magenta;
            if (m_colorIndex.intValue == 3) m_myColor.colorValue = Color.green;
            if (m_colorIndex.intValue == 4) m_myColor.colorValue = Color.cyan;
            if (m_colorIndex.intValue == 5) m_myColor.colorValue = new Color(0.2f,0.2f,1,1);
            if (m_colorIndex.intValue == 6) m_myColor.colorValue = Color.gray;
            if (m_colorIndex.intValue == 7) m_myColor.colorValue = Color.white;
            oldColorIndex = m_colorIndex.intValue;
        }

        EditorGUILayout.Space(); 
        EditorGUILayout.Space();

        if (m_myNoteTitle.stringValue == "") 
            m_myNoteTitle.stringValue = EditorGUILayout.TextArea("NewNote", textFieldStyle);
        else 
            m_myNoteTitle.stringValue = EditorGUILayout.TextArea(m_myNoteTitle.stringValue, textFieldStyle);

        GUI.backgroundColor = Color.white;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("More", GUILayout.Width(40));
        m_myNote.stringValue = EditorGUILayout.TextArea(m_myNote.stringValue, textFieldStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(); 
        EditorGUILayout.Space(); 

        EditorGUILayout.PropertyField(m_links, true);

        serializedObject.ApplyModifiedProperties();
        Repaint();

	}
}
