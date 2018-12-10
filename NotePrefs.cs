#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

[InitializeOnLoad]
public class NotePrefs 
{

	private static bool prefsLoaded = false;
	public static bool hideNotes = false;
	public static bool encryptNotes = false;
	public static float appearSizeNotes = 3f;

	[PreferenceItem("Notes")]
	public static void PreferencesGUI()
	{
		if (!prefsLoaded)
		{
			hideNotes = EditorPrefs.GetBool("NotesHideKey", false);
			encryptNotes = EditorPrefs.GetBool("NotesEncryptKey", false);
			appearSizeNotes = EditorPrefs.GetFloat("NotesSizeKey", 3f);
			prefsLoaded = true;
		}

		hideNotes = EditorGUILayout.Toggle("Hide Notes", hideNotes);
		encryptNotes = EditorGUILayout.Toggle("Encrypt Notes", encryptNotes);
		appearSizeNotes = EditorGUILayout.FloatField("Notes appears over HandleSize", appearSizeNotes);

		if (GUI.changed)
		{
			EditorPrefs.SetBool("NotesHideKey", hideNotes);
			EditorPrefs.SetBool("NotesEncryptKey", encryptNotes);
			EditorPrefs.SetFloat("NotesSizeKey", appearSizeNotes);
		}
	}

}
#endif
