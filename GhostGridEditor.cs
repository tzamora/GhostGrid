#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Editor controls for GhostGrid.
/// </summary>
using System.IO;


[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid;
    private string message;
	Transform[] children;

	// prefabs information
	FileInfo[] prefabsFileInfo;

    public void OnEnable()
    {
        grid = target as GhostGrid;
		children = grid.GetComponentsInChildren<Transform>();
        message = "";

		DirectoryInfo dir = new DirectoryInfo("Assets/Resources");
		prefabsFileInfo = dir.GetFiles("*.prefab");

    }
	
	Vector2 scrollPosition = Vector2.zero;
	public override void OnInspectorGUI()
    {
		//////
        GUILayout.Label("");
        DrawDefaultInspector();

        GUILayout.Label("");
        GUILayout.Label("SNAPPING");
        GUILayout.BeginHorizontal();

        // Snap once button
        if (GUILayout.Button("Snap Once", GUILayout.ExpandWidth(false)))
        {
            message = "Grid snapped!";

            if (grid.autoSnapEnabled)
            {
                grid.autoSnapEnabled = false;
            }
            else
            {
                grid.SnapAll();
            }
        }


        // Auto snap button
        if (GUILayout.Button(grid.autoSnapEnabled ? "Disable Auto Snap" : "Enable Auto Snap", GUILayout.ExpandWidth(false)))
        {
            message = "Changed!";

            grid.autoSnapEnabled = !grid.autoSnapEnabled;

            if (grid.autoSnapEnabled)
                grid.SnapAll();
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.Label("OPTIMIZATIONS");

        // Rename button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rename children", GUILayout.ExpandWidth(false)))
        {
            message = grid.RenameChildren() + " children renamed!";
        }
        GUILayout.EndHorizontal();


        // Exclude overlapped button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Exclude Overlapped Children", GUILayout.ExpandWidth(false)))
        {
            message = grid.ExcludeOverlappedChildren() + " overlapped children deleted!";
        }
        GUILayout.EndHorizontal();


        // Optimize colliders button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Turn Off Unneeded 2D Colliders", GUILayout.ExpandWidth(false)))
        {
            message = grid.TurnOffUnneededColliders2D() + " unneeded colliders were turned off!";
        }
        GUILayout.EndHorizontal();


        // All optimizations colliders button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ALL ^", GUILayout.ExpandWidth(false)))
        {
            message =
                grid.RenameChildren() + " children renamed!" + "\n" +
                grid.ExcludeOverlappedChildren() + " overlapped children deleted!" + "\n" +
                grid.TurnOffUnneededColliders2D() + " unneeded colliders were turned off!";
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("#experimental Polygon Builder", GUILayout.ExpandWidth(false)))
        {
            message = grid.RebuildPolygon() + "";
        }
        GUILayout.EndHorizontal();

		drawAssetPreview ();

        // Status label
        GUILayout.Label("");
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Auto Snap Disabled.");
        if (message.Length > 0)
            GUILayout.Label(message);

        // Credits
        GUILayout.Label("");
        GUILayout.Label("GhostGrid v0.1.3.5 by @matnesis");
    }

	void drawAssetPreview(){
		//		if(!isSetup)
		//			return;
		EditorGUILayout.BeginVertical(GUILayout.MinHeight(128.0f));

		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		
		// create a button for each image loaded in, 4 buttons in width
		// calls the handler when a new image is selected.
		int counter = 0;
		int n = 10; // prefabs
		//foreach(Texture2D img in images)

		prefabsFileInfo.Select(f => f.FullName).ToArray();
		foreach (FileInfo f in prefabsFileInfo) 
		{ 

			GameObject prefab = Resources.Load (f.Name.Split('.')[0]) as GameObject;

			Debug.Log("putting " + f.Name.Split('.')[0]);

			//Texture2D img = AssetPreview.GetAssetPreview(prefab);
			
//			if(counter % 3 == 0)
//			{
//				GUILayout.BeginHorizontal();
//			}
			
			++counter;
			
			if(GUILayout.Button(f.Name, GUILayout.Height(27)))
			{
				grid.mainPrefab = prefab;
			}
			
//			if(counter % 3 == 0 || counter == n){
//				GUILayout.EndHorizontal();
//			}
		}

//		for (int i = 0; i < n; i++) {
//				
//		}

		GUILayout.EndScrollView();

		EditorGUILayout.EndVertical();
	}

	void OnSceneGUI()
	{
		Event e = Event.current;

		// We use hotControl to lock focus onto the editor (to prevent deselection)
		int controlID = GUIUtility.GetControlID(FocusType.Passive);

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		drawGridCursor ();

		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:

				drawGridCursor ();

				drawGridRectangle();

				GUIUtility.hotControl = controlID;

				PlaceObject();
				
				Event.current.Use();
			break;
			
			case EventType.MouseUp:
				GUIUtility.hotControl = 0;
				Event.current.Use();
			break;
			
		}

		HandleUtility.Repaint ();
	}

	void drawGridRectangle(){

		float bottomY = children.Cast<Transform>().OrderBy(t=>t.position.y).First().position.y; // bottom
		
		float topY = children.Cast<Transform>().OrderBy(t=>t.position.y).Last().position.y; // top
		
		float leftX = children.Cast<Transform>().OrderBy(t=>t.position.x).First().position.x; // bottom
		
		float rightX = children.Cast<Transform>().OrderBy(t=>t.position.x).Last().position.x; // top
		
		Debug.DrawLine(new Vector3(leftX, topY, 0f), new Vector3(rightX, topY, 0f), Color.yellow);
		
		Debug.DrawLine(new Vector3(rightX, bottomY, 0f), new Vector3(rightX, topY, 0f), Color.blue);
		
		Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(leftX, topY, 0f), Color.black);
		
		Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(rightX, bottomY, 0f), Color.red);
	}

	void drawGridCursor(){

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		
		// draw a cursor of the size of the gridSize

		Vector3 mousePos = new Vector3(ray.origin.x, ray.origin.y, 0f);

		mousePos = GhostGrid.GetSnapVector (mousePos, grid.gridSize);

		// set the paddings to center mouse
		mousePos = mousePos - new Vector3 (grid.gridSize / 2, grid.gridSize / 2);

		//draw
		Handles.DrawLine(mousePos,mousePos  + new Vector3(grid.gridSize, 0f, 0f));
		Handles.DrawLine(mousePos,mousePos + new Vector3(0f, grid.gridSize, 0f));
		Handles.DrawLine(mousePos + new Vector3(0f, grid.gridSize, 0f),mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f));
		Handles.DrawLine(mousePos + new Vector3(grid.gridSize, 0f, 0f) ,mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f));

	}

	void PlaceObject()
	{
		if (grid.mainPrefab == null) {
			return;		
		}

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		Vector3 mousePos = new Vector3(ray.origin.x, ray.origin.y, 0f);
		
		mousePos = GhostGrid.GetSnapVector (mousePos, grid.gridSize);

		GameObject go = (GameObject)GameObject.Instantiate(grid.mainPrefab, mousePos, Quaternion.identity);

		go.transform.parent = grid.transform;
	}

}
#endif
