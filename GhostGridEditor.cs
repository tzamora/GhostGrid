#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Editor controls for GhostGrid.
/// </summary>
[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid;
    private string message;


    public void OnEnable()
    {
        grid = target as GhostGrid;
        message = "";
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


        // Status label
        GUILayout.Label("");
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Auto Snap Disabled.");
        if (message.Length > 0)
            GUILayout.Label(message);

		drawAssetPreview ();


        // Credits
        GUILayout.Label("");
        GUILayout.Label("GhostGrid v0.1.3.5 by @matnesis");
    }

	void drawAssetPreview(){
		//		if(!isSetup)
		//			return;
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition,  GUILayout.Width(10), GUILayout.Height(150));
		
		// create a button for each image loaded in, 4 buttons in width
		// calls the handler when a new image is selected.
		int counter = 0;
		//foreach(Texture2D img in images)
		for (int i = 0; i < 10; i++) {
			
			Texture2D img = AssetPreview.GetAssetPreview(grid.mainPrefab);
			
			if(counter % 3 == 0 || counter == 0)
				GUILayout.BeginHorizontal();
			++counter;
			
			if(GUILayout.Button(img, GUILayout.Height(60), GUILayout.Width(60)))
			{
				// tell handler about new image, close selection window
				//handler(img);
				//EditorWindow.focusedWindow.Close();
			}
			
			if(counter % 3 == 0)
				GUILayout.EndHorizontal();
		}
		
		
		GUILayout.EndScrollView();
	}

	void OnSceneGUI()
	{
		Event e = Event.current;

		drawGridCursor ();

		// We use hotControl to lock focus onto the editor (to prevent deselection)
		int controlID = GUIUtility.GetControlID(FocusType.Passive);

		//Debug.Log("->" + Event.current.mousePosition);

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);


		// Debug.DrawLine(grid.transform.position, ray.origin,Color.green); esto dibuja por donde va el mouse

		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:

			drawGridCursor ();

			//Debug.Log("->" + Event.current.mousePosition);
				GUIUtility.hotControl = controlID;

				Transform[] children = grid.GetComponentsInChildren<Transform>();

				// get the extremes

				float bottomY = children.Cast<Transform>().OrderBy(t=>t.position.y).First().position.y; // bottom
				
				float topY = children.Cast<Transform>().OrderBy(t=>t.position.y).Last().position.y; // top

				float leftX = children.Cast<Transform>().OrderBy(t=>t.position.x).First().position.x; // bottom
				
				float rightX = children.Cast<Transform>().OrderBy(t=>t.position.x).Last().position.x; // top


				Debug.DrawLine(new Vector3(leftX, topY, 0f), new Vector3(rightX, topY, 0f), Color.yellow);

				Debug.DrawLine(new Vector3(rightX, bottomY, 0f), new Vector3(rightX, topY, 0f), Color.blue);
			
				Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(leftX, topY, 0f), Color.black);

				Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(rightX, bottomY, 0f), Color.red);

//				
//
//				RaycastHit hit;
//				if ( Physics.Raycast( ray, out hit, Mathf.Infinity))
//				{
//					Vector3 myPos = hit.point;
			Debug.Log("creo que lo logre");
					PlaceObject();
//				}
				
				Event.current.Use();
			break;
			
			case EventType.MouseUp:
				GUIUtility.hotControl = 0;
				Event.current.Use();
			break;
			
		}

		//Debug.Log ("esto se esta llamando muchas veces");

		HandleUtility.Repaint ();
	}



	void drawGridCursor(){

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		
		//Debug.DrawLine(grid.transform.position, ray.origin,Color.red);



		//GhostGrid.GetSnapVector (ray.origin, grid.gridSize);

		// draw a cursor of the size of the gridSize

		Vector3 mousePos = new Vector3(ray.origin.x, ray.origin.y, 0f);

		mousePos = GhostGrid.GetSnapVector (mousePos, grid.gridSize);

		mousePos = mousePos - new Vector3 (grid.gridSize / 2, grid.gridSize / 2);

//		Vector3 horizontalPadding = new Vector3 (grid.gridSize / 2, 0f, 0f);
//
//		Vector3 verticalPadding = new Vector3 (0f, grid.gridSize / 2, 0f);

		Handles.DrawLine(mousePos,mousePos  + new Vector3(grid.gridSize, 0f, 0f));
		Handles.DrawLine(mousePos,mousePos + new Vector3(0f, grid.gridSize, 0f));

		Handles.DrawLine(mousePos + new Vector3(0f, grid.gridSize, 0f),mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f));
		Handles.DrawLine(mousePos + new Vector3(grid.gridSize, 0f, 0f) ,mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f));

//		Handles.DrawLine(mousePos,mousePos  + new Vector3(grid.gridSize, 0f, 0f),Color.green);
//		Handles.DrawLine(mousePos,mousePos + new Vector3(0f, grid.gridSize, 0f),Color.blue);
//		
//		Handles.DrawLine(mousePos + new Vector3(0f, grid.gridSize, 0f),mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f),Color.red);
//		Handles.DrawLine(mousePos + new Vector3(grid.gridSize, 0f, 0f) ,mousePos + new Vector3(grid.gridSize, grid.gridSize, 0f),Color.blue);

//		Debug.DrawLine(new Vector3(leftX, topY, 0f), new Vector3(rightX, topY, 0f), Color.yellow);
//		
//		Debug.DrawLine(new Vector3(rightX, bottomY, 0f), new Vector3(rightX, topY, 0f), Color.blue);
//		
//		Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(leftX, topY, 0f), Color.black);
//		
//		Debug.DrawLine(new Vector3(leftX, bottomY, 0f), new Vector3(rightX, bottomY, 0f), Color.red);

	}

	void PlaceObject()
	{
		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		Vector3 mousePos = new Vector3(ray.origin.x, ray.origin.y, 0f);
		
		mousePos = GhostGrid.GetSnapVector (mousePos, grid.gridSize);

		GameObject go = (GameObject)GameObject.Instantiate(grid.mainPrefab, mousePos, Quaternion.identity);

		//Instantiate (go, myPos, Quaternion.identity);
	}

}
#endif
