using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Helpers/Note")]
public class Note : MonoBehaviour 
{
	private Texture2D _texture;
    private Texture2D texture
    {
        get
        { 
            if (_texture == null)
            {
                _texture = new Texture2D(1,1);
                _texture.SetPixel(0, 0, new Color(0,0,0,0.75f));
                _texture.Apply();
            }
            return _texture;
        }
    }

	public string myNoteTitle = "NewNote";
	public string myNote = "";
	public int colorIndex;
	public Color myColor = Color.yellow;
	public float width = 100f;
	public bool drawBox = true;
	public bool boldBox = false;
    public Note[] links = new Note[0];

	#if UNITY_EDITOR

	void OnDrawGizmos() 
	{
		if (NotePrefs.hideNotes)
		{
			return;	
		}

		if (SceneView.currentDrawingSceneView != null)
		{
			// if notes is behind camera, don't draw it.
			Camera cam = SceneView.currentDrawingSceneView.camera;
			Vector3 camDir = cam.transform.forward;
			Vector3 camPos = cam.transform.position;
			Vector3 heading = (this.transform.position - camPos).normalized;
			float dot = Vector3.Dot(heading, camDir);
			if (dot < 0.5f) return;

			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, cam.transform.rotation, Vector3.one);
			Gizmos.matrix = rotationMatrix; 
		}

		float handleSize = HandleUtility.GetHandleSize(transform.position);
		if (handleSize > NotePrefs.appearSizeNotes)
		{
			Gizmos.color = myColor;
			Gizmos.DrawCube (Vector3.zero, Vector3.one * 0.5f);
		}
		else
		{
			string textToShow = myNoteTitle;
			if (NotePrefs.encryptNotes)
			{
				textToShow = NoteCrypt.GetText(textToShow);
			}

			GUIStyle style = new GUIStyle();
			style.normal.textColor = myColor;
			style.alignment = TextAnchor.MiddleCenter;
			style.wordWrap = true;
			style.padding = new RectOffset(10,10,10,10);
			style.normal.background = texture;
			float height = style.CalcHeight( new GUIContent(textToShow as string),width);
			
			Handles.BeginGUI();
			Vector3 pos = transform.position;
			Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
			pos2D.y -= 2; // approximation... to fit with GizmosWireCube
			GUI.Label(new Rect(pos2D.x-(width*0.5f), pos2D.y-(height*0.5f), width, height),textToShow, style);
			Handles.EndGUI();

			Gizmos.color = Color.clear;
			Vector3 cubeSize = new Vector3(handleSize*1.25f * (width/100), handleSize*(height/80f),0);
			Gizmos.DrawCube(Vector3.zero, cubeSize);

			if (myNote!="")
			{
				Gizmos.color = myColor;
				Gizmos.DrawWireSphere(new Vector3(+cubeSize.x*0.5f, +cubeSize.y*0.5f, 0), handleSize * 0.05f);
			}

			if (drawBox)
			{
				if (boldBox)
				{
					Gizmos.color = myColor;
					Vector3 cubeSizeBold = new Vector3(handleSize*1.25f * ((width+7)/100), handleSize*((height+7)/80f),0);
					Gizmos.DrawWireCube(Vector3.zero, cubeSizeBold);
				}

				Gizmos.color = new Color(myColor.r, myColor.g, myColor.b, 0.4f);
				Gizmos.DrawWireCube(Vector3.zero, cubeSize);

				Gizmos.DrawWireCube(new Vector3(+cubeSize.x*0.5f, +cubeSize.y*0.5f, 0), Vector3.one * handleSize * 0.07f);
				Gizmos.DrawWireCube(new Vector3(-cubeSize.x*0.5f, -cubeSize.y*0.5f, 0), Vector3.one * handleSize * 0.07f);
				Gizmos.DrawWireCube(new Vector3(-cubeSize.x*0.5f, +cubeSize.y*0.5f, 0), Vector3.one * handleSize * 0.07f);
				Gizmos.DrawWireCube(new Vector3(+cubeSize.x*0.5f, -cubeSize.y*0.5f, 0), Vector3.one * handleSize * 0.07f);
			}

			for (int i=0; i<links.Length; i++)
			{
				if (links[i] != null)
				{
					Vector3 startPos = transform.position;
					Vector3 endPos = links[i].transform.position;
					startPos = this.GetLinkPoint(links[i].transform);
					endPos = links[i].GetLinkPoint(this.transform);
					Vector3 midPos = (startPos + endPos) * 0.5f;

					Handles.color = new Color(myColor.r, myColor.g, myColor.b, 0.4f);
					Handles.DrawLine(startPos, midPos);
					Handles.color = new Color(links[i].myColor.r, links[i].myColor.g, links[i].myColor.b, 0.4f);
					Handles.DrawLine(midPos, endPos);
					Handles.color = new Color(myColor.r, myColor.g, myColor.b, 0.4f);

					Vector3 handleForward = Vector3.forward;
					if (SceneView.currentDrawingSceneView != null)
					{
						handleForward = SceneView.currentDrawingSceneView.camera.transform.forward;
					}

					Vector2 lineDir = Quaternion.AngleAxis(-45f*0.5f, handleForward) * (startPos-endPos).normalized;
					Handles.DrawSolidArc(midPos, handleForward, lineDir, 45f, handleSize * 0.1f);

					startPos = this.GetLinkPoint(links[i].transform, true); // this One is good
					Gizmos.color = new Color(myColor.r, myColor.g, myColor.b, 0.4f);
					Gizmos.DrawWireCube(startPos, Vector3.one * handleSize * 0.07f);
				}
			}
		}
	}

	public Vector3 GetLinkPoint(Transform otherNote, bool relative = false)
	{
		Vector3 pos = Vector3.zero;

		// Get this note size
		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.MiddleCenter;
		style.wordWrap = true;
		style.padding = new RectOffset(10,10,10,10);
		float height = style.CalcHeight( new GUIContent(this.myNoteTitle as string), this.width);
		float handleSize = HandleUtility.GetHandleSize(this.transform.position);
		Vector3 cubeSize = new Vector3(handleSize*1.25f * (this.width/100), handleSize*(height/80f),0);

		// Get the best point (this cube size vs otherNote.position) = shortest distance
		float dist = Mathf.Infinity;
		int bestIndex = -1;
		Vector3[] points = new Vector3[4];
		points[0] = transform.position + new Vector3(0, +cubeSize.y*0.5f, 0);
		points[1] = transform.position + new Vector3(0, -cubeSize.y*0.5f, 0);
		points[2] = transform.position + new Vector3(-cubeSize.x*0.5f, 0, 0);
		points[3] = transform.position + new Vector3(+cubeSize.x*0.5f, 0, 0);

		for (int i=0; i<points.Length; i++)
		{
			float curDistance = Vector3.Distance(otherNote.position, points[i]);
			if (curDistance < dist)
			{
				dist = curDistance;
				bestIndex = i;
			}
		}
		pos = points[bestIndex];
		if (relative) 
		{
			Vector3[] absPoints = new Vector3[4];
			absPoints[0] = new Vector3(0, +cubeSize.y*0.5f, 0);
			absPoints[1] = new Vector3(0, -cubeSize.y*0.5f, 0);
			absPoints[2] = new Vector3(-cubeSize.x*0.5f, 0, 0);
			absPoints[3] = new Vector3(+cubeSize.x*0.5f, 0, 0);
			pos = absPoints[bestIndex];
		}
		return pos;
	}

	[MenuItem("GameObject/Create Note", false, 0)] 				
	public static void CreateNoteInScene() 
	{ 
		GameObject go = new GameObject("Note");
		go.AddComponent<Note>();
		CenterOnScreen(go);
	}

	private static void CenterOnScreen(GameObject obj) 
	{
		SceneView sceneView = SceneView.lastActiveSceneView;
		if (sceneView == null) return;
		Camera sceneCam = sceneView.camera;
		Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f,0.5f,15f));
		Ray ray = sceneCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		Plane groundPlane = new Plane(Vector3.up, Vector3.up * (sceneCam.transform.position.y - 5));
		float rayDistance;
		if (groundPlane.Raycast(ray, out rayDistance)) spawnPos = ray.GetPoint(rayDistance);
		obj.transform.position = new Vector3(Mathf.Round(spawnPos.x), Mathf.Round(spawnPos.y), Mathf.Round(spawnPos.z));
		Selection.activeGameObject = obj;
	}

	#endif
}
