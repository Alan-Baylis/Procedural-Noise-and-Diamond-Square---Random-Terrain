using UnityEngine;
using System.Collections;

namespace Spatial
{
    using Pair = System.Collections.Generic.KeyValuePair<int, int>;

	public class Filter : MonoBehaviour 
	{
		/**
		 * Generate Gaussian Blur Center with m_sigma weighting
		 * 
		 * @param[out]	o_vertices		Smoothed mesh
		 * @param[in] 	i_kernel		Spatial kernel to apply
         * @param[in]   i_x_offset      X offset
         * @param[in]   i_y_offset      Y offset
		 * 
		 */
		protected void ApplyFilter(ref Vector3[] o_vertices, float[,] i_kernel, Pair i_x_limits, Pair i_y_limits)
		{
			if (i_kernel.Length % 2 == 0) 
			{
				return;
			}

			int row_length = (int)Mathf.Sqrt(o_vertices.Length);
			int num_vertices = row_length * row_length;

            Vector3[] accumulator = (Vector3 [])o_vertices.Clone();

			// Apply filter within grid limits - 1
			int kernel_size = (int)Mathf.Sqrt (i_kernel.Length);
			int offset = (kernel_size - 1) / 2;

            // Use manual ROI is specified
            int min_x = offset;
            int min_y = min_x;
            int max_x = row_length - offset - (kernel_size / 2);
            int max_y = max_x;

            if(i_x_limits.Key + i_x_limits.Value + i_y_limits.Key + i_y_limits.Value != 0)
            {
                min_x = System.Math.Max(min_x, i_x_limits.Key);
                min_y = System.Math.Max(min_y, i_y_limits.Key);

                max_x = System.Math.Min(max_x, i_x_limits.Value);
                max_y = System.Math.Min(max_y, i_y_limits.Value);
            }

            for (int v = min_x; v <= max_x; ++v) 
			{
                for (int u = min_y; u <= max_y; ++u) 
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
				}
			}
			
			o_vertices = accumulator;
		}

	}

}
