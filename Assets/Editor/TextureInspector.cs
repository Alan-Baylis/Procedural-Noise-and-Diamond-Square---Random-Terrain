using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Texture
{
	[CustomEditor(typeof(TextureFractal))]
	public class TextureInspector : Editor 
	{
		/// Create texture manager for rendering fractal textures on game objects
		/// using procedural noise
		private TextureFractal m_creator;


		/**
		 * Add Redo callback on texture managers serialized user configurable structures
		 * when unity inspector is instantiated
		 */
		private void OnEnable () 
		{
			m_creator = target as TextureFractal;
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
				m_creator.UpdateFractal();
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