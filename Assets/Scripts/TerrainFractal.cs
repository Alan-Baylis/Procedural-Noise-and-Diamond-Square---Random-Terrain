using UnityEngine;
using System.Collections;

namespace Terrain
{

	public class TerrainFractal : Terrain
	{
		/**
		 * Create base texture before displacement
		 */
		private void OnEnable()
		{
			CreateFractal ();
			CreateTerrain ();
			Refresh();
		}

		/**
		 * Displace a fractal texture by the randomly generated sample,
		 * set the new colour and position attributes of vertices and triangulate them
		 * before recomputing normals.
		 */
		protected override void CreateMesh() 
		{
			Mesh mesh = new Mesh();

			int grid_size = GetTextureSize ();
			TerrainProperties config = GetTerrainProperties ();

			// Clear mesh data to resize vertex buffer
			mesh.Clear ();
			
			// Initialise Vertex Container. N^2 points and 6 triangulation points per grid cell
			int num_vertices = (grid_size + 1) * (grid_size + 1);
			
			Vector3[] vertices = new Vector3[num_vertices];
			Vector3[] normals = new Vector3[num_vertices];
			Vector2[] uv = new Vector2[num_vertices];
			Color[] vertex_color = new Color[num_vertices];
			int[] triangles = new int[grid_size * grid_size * 6];
			
			float quantisation = 1.0f / grid_size;
			
			// Generate NxN grid and displace height by the psuedo-random noise sample
			int vertex_idx = 0;
			int triangle_idx = 0;

			// Populate mesh and tesselate
			for (int y = 0; y < grid_size + 1; ++y) 
			{
				for (int x = 0; x < grid_size + 1; ++x) 
				{
					// Populate mesh renderer with texture uv co-ordingates for default shader
					uv [vertex_idx] = new Vector2 ( x * quantisation, y * quantisation); 
					vertex_color[vertex_idx] = GetColour (x, y);

					// Keep rate of change of adjacent harmonic amplitudes constant 
					float z = GetSample(x,y);
					z *= config.m_amplitude_damp ? m_config.m_gain / m_fractal_config.m_noise_frequency : m_config.m_gain;
					
					// Push new grid point into mesh vertex buffer
					vertices [vertex_idx] = new Vector3 ((x * quantisation) - 0.5f,
					                                     z,
					                                     (y * quantisation) - 0.5f);

					// Raster scan from (0,0) to (1,1). Set traingulation to 0 2 1 1 2 3 to
					// avoid face culling. + 1 to accommodate 0 indexing
					if (y < (grid_size - 1) && x < (grid_size - 1)) {
						triangles [triangle_idx++] = vertex_idx;
						triangles [triangle_idx++] = vertex_idx + grid_size + 1;
						triangles [triangle_idx++] = vertex_idx + 1;
						triangles [triangle_idx++] = vertex_idx + 1;
						triangles [triangle_idx++] = vertex_idx + grid_size + 1;
						triangles [triangle_idx++] = vertex_idx + grid_size + 2;
					}
					
					// Computer vertex normals as cross product of two triangulated edges. This
					// is essentially a partial derivative as X and Y are treated independently
					normals [vertex_idx] = Vector3.back;
					
					vertex_idx++;
				}
			}
			
			// Assign vertices to mesh
			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uv;
			mesh.colors = vertex_color;
			
			// Unity alternative to computing cross product of two traingulated edge per vertex.
			// This method computes partial derivatives (Z gradient, then X gradient, cross product)
			// instead of true derivatives (partial derivatives tend to true derivatives as res -> oo).
			// True derivatives do not depend on the mesh shape as they are directly computed from the 
			// procedural noise gradients using the 5th order derivative (Perlins method).
			mesh.RecalculateNormals ();

			SetTerrainMesh(ref mesh);
		}
	}

}