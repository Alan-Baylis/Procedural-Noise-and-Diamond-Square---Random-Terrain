using UnityEngine;
using System.Collections;

namespace Texture
{
	[System.Serializable]
	public class TextureProperties
	{
		/// User allocated texture name
		public string 	m_name = "Procedural Noise";
		
		/// Texture x and y diemnsions (independent) - Max unity mesh is 254 x 254
		[Range(2,256)]
		public int 		m_size = 254;
		
		/// Ansitropic filtering level
		public int 		m_anisotropic = 9;
		
		/// True to apply mipmap interpolation
		public bool		m_mipmap = true;
		
		/// Texture interpolation mode
		public FilterMode 		m_filter = FilterMode.Trilinear;
		
		/// Texture wrapping mode for opposing edges
		public TextureWrapMode 	m_wrap = TextureWrapMode.Clamp;
	}


	public class TextureBase : MonoBehaviour 
	{
		/// Unity texture object
		private Texture2D 			m_texture;

		/// Struct for user configurable general texture configuration
		public TextureProperties 	m_texture_config;


		/**
		 * Attach texture to game object and configure texture properties
		 */
		public bool CreateTexture()
		{
			bool ret = true;
			
			// Create texture if script attached to gameObject
			try
			{
				// Instantiate texture with RGB 8 using user settings.
				// Attach to current GameObject if MeshRenderer component Exists
				m_texture = new Texture2D (m_texture_config.m_size, 
				                           m_texture_config.m_size, 
				                           TextureFormat.RGB24, 
				                           m_texture_config.m_mipmap);
				
				m_texture.name = m_texture_config.m_name;
				m_texture.wrapMode = m_texture_config.m_wrap;
				m_texture.filterMode = m_texture_config.m_filter;
				m_texture.anisoLevel = m_texture_config.m_anisotropic;


				GetComponent<MeshRenderer>().material.mainTexture = m_texture;
			}
			catch(UnityException)
			{
				ret = false;
			}
			
			return ret;
		}


		/**
		 * Generate a procedurally generated fractal from the user configuration
		 * and apply it to the mesh renderer
		 * 
		 * @param[in] i_uv	2D array of texture colours and (u,v) co-ordinates
		 */
		public void ApplyTexture(Color[,] i_uv)
		{
			for (int x = 0; x < i_uv.GetLength(0); ++x) 
			{
				for (int y = 0; y < i_uv.GetLength(1); ++y)
				{
					m_texture.SetPixel(x, y, i_uv[y,x]);
				}
			}

			m_texture.Apply ();
		}


		/**
		 * Get texture size
		 * 
		 * \return texture size
		 */
		public int GetTextureSize()
		{
			return m_texture_config.m_size;
		}


		/**
		 * Set texture size compatible with UI callbacks
		 * 
		 * @param[in] i_texture size
		 */
		public void SetTextureSize(float i_texture_size)
		{
			m_texture_config.m_size = (int)i_texture_size;
		}


		/**
		 * Return True if texture object is instantiated
		 */
		public bool TextureExist()
		{
			return m_texture != null;
		}
	}
}
