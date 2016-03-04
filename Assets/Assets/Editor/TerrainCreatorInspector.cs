using UnityEditor;
using UnityEngine;

namespace Terrain
{
	[CustomEditor(typeof(Terrain))]

	public class TerrainCreatorInspector : Editor {

		/// Instance of terrain class attached to the game object
		private Terrain creator;


		/**
		 * Add Redo callback on texture managers serialized user configurable structures
		 * when unity inspector is instantiated
		 */
		private void OnEnable () 
		{
			creator = target as Terrain;
			Undo.undoRedoPerformed += RefreshCreator;
		}

		/**
		 * Disable Redo callback when unity inspector is instantiated
		 */
		private void OnDisable () 
		{
			Undo.undoRedoPerformed -= RefreshCreator;
		}


		/**
		 * Invoke the texture manager to check whether a new texture 
		 * needs generating.
		 */
		private void RefreshCreator () 
		{
			if (Application.isPlaying) 
			{
				creator.Refresh();
			}
		}

		/**
		 * Invoke the texture manager to check whether a new texture 
		 * needs generating.
		 */
		public override void OnInspectorGUI () 
		{
			EditorGUI.BeginChangeCheck();
			DrawDefaultInspector();
			
			if (EditorGUI.EndChangeCheck() && Application.isPlaying) 
			{
				RefreshCreator ();
			}
		}
	}

}
