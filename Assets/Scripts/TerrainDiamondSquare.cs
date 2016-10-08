using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Terrain
{
	public class TerrainDiamondSquare : Terrain
	{
		/**
		 * Create base texture before displacement
		 */
		private void OnEnable()
		{
			CreateFractal ();
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
			
			// Generate terrain with diamond square if requested
			Vector3[,] grid = DefaultGrid(grid_size + 1);

			int max_idx = grid_size - 1;
			
			List<Vector3> seeds = new List<Vector3>{
				new Vector3(0,0,0),
				new Vector3(0,0,max_idx),
				new Vector3(max_idx,0,0),
				new Vector3(max_idx,0,max_idx)
			};
			
			for (int i = 0; i < seeds.Count(); ++i) 
			{
				grid [(int)seeds [i].x, (int)seeds [i].z] = seeds [i];
			}
			
			DiamondSquareRecursive (ref grid, seeds, 0.25f);

			
			// Populate mesh and tesselate
			for (int y = 0; y < grid_size + 1; ++y) 
			{
				for (int x = 0; x < grid_size + 1; ++x) 
				{
					// Populate mesh renderer with texture uv co-ordingates for default shader
					uv [vertex_idx] = new Vector2 ( x * quantisation, y * quantisation); 
					vertex_color[vertex_idx] = GetColour (x, y);

					vertices [vertex_idx] = grid[x,y];
					Debug.Log (grid[x,y]);
					
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
			
			if(m_config.m_filter)
			{
				Spatial.FilterGaussian filter = new Spatial.FilterGaussian();
				filter.Initialise(m_config.m_kernel_length, m_config.m_sigma);
				filter.ApplyFilter(ref vertices);
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


		/**
		 * Recusive diamond square algorithm for terrain generation
		 * 
		 * Depreciated: Wrapping enabled results in the clamped opposite vertex being uninitialised for all
		 * decomposed cells resulting in discontinuities at child boundaries. Resolved with iterative method.
		 * 
		 * @param[in] i_grid		2D array of vertex attributes
		 * @param[in] i_seeds		Corner and middle vertex attribute seeds
		 * @param[in] i_scale		Amplitude decay
		 * 
		 */
		private void DiamondSquareRecursive(ref Vector3[,] i_grid, List<Vector3> i_seeds, float i_scale = 1.0f)
		{
			// Calculate square center
			Vector3 square_center = AverageVector3 (i_seeds);
			Debug.Log ("SquareCenter");
			Debug.Log (square_center);
			
			// base case
			if (square_center.x - (int)square_center.x != 0 ||
			    square_center.z - (int)square_center.z != 0) 
			{
				return;
			}
			
			i_grid[(int)square_center.x, (int)square_center.z] = square_center;
			Debug.Log (i_grid [(int)square_center.x, (int)square_center.z]);
			
			// Create diamond. Sort input seeds into expected order: L2 norm from bottom left/top right
			// Note wrapping used (N = 4), or not used (clip N = 3)
			var sortedVectors = i_seeds.OrderBy(v => Mathf.Abs (v.x) + Mathf.Abs (v.z) ).ToArray<Vector3>();
			
			List<List<Vector3>> diamonds = new List<List<Vector3>>();
			List<Vector3> new_pts = new List<Vector3>();
			
			diamonds.Add (new List<Vector3> {sortedVectors[0], sortedVectors[1], square_center});
			diamonds.Add (new List<Vector3> {sortedVectors[0], sortedVectors[2], square_center});
			diamonds.Add (new List<Vector3> {sortedVectors[3], sortedVectors[1], square_center});
			diamonds.Add (new List<Vector3> {sortedVectors[3], sortedVectors[2], square_center});
			
			// Wrap fourth point of diamond shape and compute diamond midpoint.
			foreach (List<Vector3> vectors in diamonds) 
			{
				Vector3 diamond_center = AverageVector3 ( new List<Vector3> { vectors[0], vectors[1] } );
				
				diamond_center.y += Random.Range(-i_scale, i_scale);
				
				i_grid[(int)diamond_center.x, (int)diamond_center.z] = diamond_center;
				
				new_pts.Add (diamond_center);
			}
			
			// Recursive call Bottom Left, Top Left, Top Right, Bottom Right
			DiamondSquareRecursive(	
			                       	ref i_grid, 
			                       	new List<Vector3>{sortedVectors[0], new_pts[0], new_pts[1], square_center}, 
									i_scale / 2.0f);
			DiamondSquareRecursive(	
			                       	ref i_grid, 
			                       	new List<Vector3>{sortedVectors[1], new_pts[0], new_pts[2], square_center}, 
									i_scale / 2.0f);
			DiamondSquareRecursive(	
			                       	ref i_grid, 
			                       	new List<Vector3>{sortedVectors[3], new_pts[2], new_pts[3], square_center},
									i_scale / 2.0f);
			DiamondSquareRecursive(	
			                       	ref i_grid, 
			                       	new List<Vector3>{sortedVectors[2], new_pts[1], new_pts[3], square_center}, 
									i_scale / 2.0f);
		}
		
		
		/**
		 * Initialise default grid.
		 * 
		 * @param[in] i_grid_size Grid size in x and y
		 * 
		 * \return initialised grid vertices to (0,0,0) attributes
		 */
		private Vector3[,] DefaultGrid(int i_grid_size)
		{
			Vector3[,] grid = new Vector3[i_grid_size,i_grid_size]; 
			
			for (int j = 0; j < i_grid_size; ++j) 
			{
				for(int i = 0; i < i_grid_size; ++i)
				{
					grid[i,j] = new Vector3(0,0,0);
				}
			}
			
			return grid;
		}


		/**
		 * Calculate the mean vertex of an arbitary number of input attributes
		 * 
		 * @param[in] i_vectors List of input vertices
		 * 
		 * \return mean vertex
		 */
		Vector3 AverageVector3(List<Vector3> i_vectors)
		{
			int length = i_vectors.Count ();
			
			float x = 0;
			float y = 0;
			float z = 0;
			
			foreach( Vector3 v in i_vectors)
			{
				x += v.x;
				y += v.y;
				z += v.z;
			}
			
			Vector3 ret = new Vector3(x / length, y / length, z / length);
			
			return ret;
		}
	}
	
}