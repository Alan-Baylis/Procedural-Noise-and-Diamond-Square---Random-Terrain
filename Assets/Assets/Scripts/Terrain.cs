using UnityEngine;
using System.Collections;


namespace Terrain
{
	/// Struct for configuring the terrain properties
	[System.Serializable]
	public class TerrainProperties
	{
		/// Displacement of fractals orthogonal to plane
		[Range(0.0f, 2.0f)]
		public float 	m_gain = 1.0f;
		
		/// Render vertex normals
		public bool 	m_show_normals = false;
		
		/// User configurable damping parameters
		public bool m_amplitude_damp = false;

		/// Gaussian kernel length
		public int 		m_kernel_length = 3;
		
		/// Gaussian kernel sigma
		public float 	m_sigma = 1;

		/// True to apply spatial filter
		public bool 	m_filter = false;
	}


	public abstract class Terrain : Texture.TextureFractal 
	{
		/// Game object mesh
		private Mesh m_mesh;

		/// Terrain user configuration
		public TerrainProperties m_config;


		/**
		 * Create Mesh filter
		 */
		protected void CreateTerrain()
		{
			try
			{
				if(m_mesh == null)
				{
					// Create mesh and attach mesh filter by reference. 
					m_mesh = new Mesh();
					m_mesh.name = "Terrain";
					GetComponent<MeshFilter>().mesh = m_mesh;
				}
			}
			catch
			{
				Debug.Log ("Error: Failed to create mesh component to mesh filter");
			}
		}


		/**
		 * Filter Mesh
		 */
		private void FilterMesh()
		{
			Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;

			Spatial.FilterGaussian filter = new Spatial.FilterGaussian();
			filter.Initialise(m_config.m_kernel_length, m_config.m_sigma);
			filter.ApplyFilter(ref vertices);

			GetComponent<MeshFilter>().mesh.vertices = vertices;
			GetComponent<MeshFilter>().mesh.RecalculateNormals ();
		}

		
		/**
		 * Called to render terrain on user callback or instantiation user interaction
		 */
		public void Refresh()
		{
			CreateMesh();

			if (m_config.m_show_normals) 
			{
				OnDrawGizmosSelected ();
			}

			if(m_config.m_filter)
			{
				FilterMesh();
			}
		}


		/**
		 * Called to create new mesh - Intentions to override
		 */
		protected abstract void CreateMesh();


		/**
		 * Render vertex normals for debug purposes
		 */
		void OnDrawGizmosSelected ()
		{
			float scale = 0.05f;

			Vector3[] vertices = m_mesh.vertices;
			Vector3[] normals = m_mesh.normals;

			if (m_config.m_show_normals) 
			{
				Gizmos.color = Color.yellow;

				for (int v = 0; v < vertices.Length; v++) 
				{
					Gizmos.DrawRay(vertices[v], normals[v] * scale);
				}
			}
		}

		/**
		 * Set Terrain mesh
		 * 
		 * @param[in] i_mesh New terrain mesh
		 */
		protected void SetTerrainMesh(ref Mesh i_mesh)
		{
			try
			{
				GetComponent<MeshFilter>().mesh = i_mesh;
			}
			catch
			{
				Debug.Log ("Error: Failed to assign mesh component to mesh filter");
			}
		}


		/**
		 * Get the terrain configuration
		 * 
		 * \return Terrain configuration parameters
		 */
		protected TerrainProperties GetTerrainProperties()
		{
			return m_config;
		}


		/**
		 * Set amplification factor. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetTerrainGain(float i_value)
		{
			// Only supports 1 or 2 dimensions
			i_value = Mathf.Min (2, i_value);
			i_value = Mathf.Max (0, i_value);
			
			m_config.m_gain = i_value;
			Refresh();
		}


		/**
		 * Set amplitude damping bool. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetAmplitudeDamping(bool i_value)
		{
			m_config.m_amplitude_damp = i_value;
			Refresh();
		}


		/**
		 * Enable/Disable spatial filter. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetTerrainSpatialFilter(bool i_value)
		{
			m_config.m_filter = i_value;
			Refresh();
		}
	}
}


