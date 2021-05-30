﻿using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

namespace NodeEditorFramework 
{
	public static partial class NodeEditorGUI 
	{
		internal static string curEditorUser;
		internal static bool isEditorWindow;

		// static GUI settings, textures and styles
		public static int knobSize = 16;

		public static Color NE_LightColor = new Color (0.4f, 0.4f, 0.4f);
		public static Color NE_TextColor = new Color (0.7f, 0.7f, 0.7f);

		public static Texture2D Background;
		public static Texture2D AALineTex;
		public static Texture2D GUIBox;
		public static Texture2D GUIButton;
		public static Texture2D GUIBoxSelection;

		public static GUISkin nodeSkin;
		public static GUISkin defaultSkin;

		public static GUIStyle nodeLabel;
		public static GUIStyle nodeLabelBold;
		public static GUIStyle nodeLabelSelected;
		public static GUIStyle nodeLabelCentered;
		public static GUIStyle nodeLabelBoldCentered;

		public static GUIStyle nodeBox;
		public static GUIStyle nodeBoxBold;
		
		public static bool Init ()
		{
			// Textures
			Background = ResourceManager.LoadTexture ("Textures/background.png");
			AALineTex = ResourceManager.LoadTexture ("Textures/AALine.png");
			GUIBox = ResourceManager.LoadTexture ("Textures/NE_Box.png");
			GUIButton = ResourceManager.LoadTexture ("Textures/NE_Button.png");
			GUIBoxSelection = ResourceManager.LoadTexture ("Textures/BoxSelection.png");
			
			if (!Background || !AALineTex || !GUIBox || !GUIButton)
				return false;

			// Skin & Styles
			nodeSkin = Object.Instantiate<GUISkin> (GUI.skin);

			// Label
			nodeSkin.label.normal.textColor = NE_TextColor;
			nodeLabel = nodeSkin.label;
			// Box
			nodeSkin.box.normal.textColor = NE_TextColor;
			nodeSkin.box.normal.background = GUIBox;
			nodeBox = nodeSkin.box;
			// Button
			nodeSkin.button.normal.textColor = NE_TextColor;
			nodeSkin.button.normal.background = GUIButton;
			// TextArea
			nodeSkin.textArea.normal.background = GUIBox;
			nodeSkin.textArea.active.background = GUIBox;
			// Bold Label
			nodeLabelBold = new GUIStyle (nodeLabel);
			nodeLabelBold.fontStyle = FontStyle.Bold;
			// Selected Label
			nodeLabelSelected = new GUIStyle (nodeLabel);
			nodeLabelSelected.normal.background = RTEditorGUI.ColorToTex (1, NE_LightColor);
			// Centered Label
			nodeLabelCentered = new GUIStyle (nodeLabel);
			nodeLabelCentered.alignment = TextAnchor.MiddleCenter;
			// Centered Bold Label
			nodeLabelBoldCentered = new GUIStyle (nodeLabelBold);
			nodeLabelBoldCentered.alignment = TextAnchor.MiddleCenter;
			// Bold Box
			nodeBoxBold = new GUIStyle (nodeBox);
			nodeBoxBold.fontStyle = FontStyle.Bold;

			return true;
		}

		public static void StartNodeGUI (string editorUser, bool IsEditorWindow) 
		{
			NodeEditor.checkInit(true);

			curEditorUser = editorUser;
			isEditorWindow = IsEditorWindow;

			defaultSkin = GUI.skin;
			if (nodeSkin != null)
				GUI.skin = nodeSkin;
			OverlayGUI.StartOverlayGUI (curEditorUser);
		}

		public static void EndNodeGUI () 
		{
			OverlayGUI.EndOverlayGUI ();
			GUI.skin = defaultSkin;
		}

		#region Connection Drawing

		// Curve parameters
		public static float curveBaseDirection = 1.5f, curveBaseStart = 2f, curveDirectionScale = 0.004f;

		/// <summary>
		/// Draws a node connection from start to end, horizontally
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 endPos, Color col) 
		{
			Vector2 startVector = startPos.x <= endPos.x? Vector2.right : Vector2.left;
			DrawConnection (startPos, startVector, endPos, -startVector, col);
		}
		/// <summary>
		/// Draws a node connection from start to end with specified vectors
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, Color col) 
		{
			#if NODE_EDITOR_LINE_CONNECTION
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.StraightLine, col);
			#else
			DrawConnection (startPos, startDir, endPos, endDir, ConnectionDrawMethod.Bezier, col);
			#endif
		}
		/// <summary>
		/// Draws a node connection from start to end with specified vectors
		/// </summary>
		public static void DrawConnection (Vector2 startPos, Vector2 startDir, Vector2 endPos, Vector2 endDir, ConnectionDrawMethod drawMethod, Color col) 
		{
			if (drawMethod == ConnectionDrawMethod.Bezier) 
			{
				float dirFactor = 80;//Mathf.Pow ((startPos-endPos).magnitude, 0.3f) * 20;
				//Debug.Log ("DirFactor is " + dirFactor + "with a bezier lenght of " + (startPos-endPos).magnitude);
				RTEditorGUI.DrawBezier (startPos, endPos, startPos + startDir * dirFactor, endPos + endDir * dirFactor, col * Color.gray, null, 3);
			}
			else if (drawMethod == ConnectionDrawMethod.StraightLine)
				RTEditorGUI.DrawLine (startPos, endPos, col * Color.gray, null, 3);
		}

		/// <summary>
		/// Optimises the bezier directions scale so that the bezier looks good in the specified position relation.
		/// Only the magnitude of the directions are changed, not their direction!
		/// </summary>
		public static void OptimiseBezierDirections (Vector2 startPos, ref Vector2 startDir, Vector2 endPos, ref Vector2 endDir) 
		{
			Vector2 offset = (endPos - startPos) * curveDirectionScale;
			float baseDir = Mathf.Min (offset.magnitude/curveBaseStart, 1) * curveBaseDirection;
			Vector2 scale = new Vector2 (Mathf.Abs (offset.x) + baseDir, Mathf.Abs (offset.y) + baseDir);
			// offset.x and offset.y linearly increase at scale of curveDirectionScale
			// For 0 < offset.magnitude < curveBaseStart, baseDir linearly increases from 0 to curveBaseDirection. For offset.magnitude > curveBaseStart, baseDir = curveBaseDirection
			startDir = Vector2.Scale(startDir.normalized, scale);
			endDir = Vector2.Scale(endDir.normalized, scale);
		}

		/// <summary>
		/// Gets the second connection vector that matches best, accounting for positions
		/// </summary>
		internal static Vector2 GetSecondConnectionVector (Vector2 startPos, Vector2 endPos, Vector2 firstVector) 
		{
			if (firstVector.x != 0 && firstVector.y == 0)
				return startPos.x <= endPos.x? -firstVector : firstVector;
			else if (firstVector.y != 0 && firstVector.x == 0)
				return startPos.y <= endPos.y? -firstVector : firstVector;
			else
				return -firstVector;
		}

		#endregion
	}
}