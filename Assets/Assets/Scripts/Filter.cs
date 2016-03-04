using UnityEngine;
using System.Collections;

namespace Spatial
{

	public class Filter : MonoBehaviour 
	{
		/**
		 * Generate Gaussian Blur Center with m_sigma weighting
		 * 
		 * @param[out]	o_vertices		Smoothed mesh
		 * @param[in] 	i_kernel		Spatial kernel to apply
		 * 
		 */
		protected void ApplyFilter(ref Vector3[] o_vertices, float[,] i_kernel)
		{
			if (i_kernel.Length % 2 == 0) 
			{
				return;
			}

			int row_length = (int)Mathf.Sqrt(o_vertices.Length);
			int num_vertices = row_length * row_length;

			Vector3[] accumulator = new Vector3[num_vertices];

			// Apply filter within grid limits - 1
			int kernel_size = (int)Mathf.Sqrt (i_kernel.Length);
			int offset = (kernel_size - 1) / 2;
			
			for (int v = offset; v <= (row_length - offset - kernel_size/2); ++v) 
			{
				for (int u = offset; u <= (row_length - offset - kernel_size/2); ++u) 
				{
					float value = 0;
					
					for(int y = 0; y < kernel_size; ++y)
					{
						for(int x = 0; x < kernel_size; ++x)
						{
							int idx = (u + x - offset) + ( (v + y - offset) * row_length );
							value += i_kernel[x,y] * o_vertices[idx].y;
						}
					}
					
					int grid_idx = u + (v * row_length);
					
					o_vertices[grid_idx].y = value;
					accumulator[grid_idx] = o_vertices[grid_idx];
					//colours[grid_idx] = m_colour_gradient.Evaluate (value);
				}
			}
			
			o_vertices = accumulator;
		}

	}

}
