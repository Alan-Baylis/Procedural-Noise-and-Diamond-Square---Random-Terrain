using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Spatial
{
    using Pair = System.Collections.Generic.KeyValuePair<int, int>;

	public class FilterGaussian : Filter 
	{
		private float[,] m_kernel;

		/**
		 * Create kernel
		 */
		public void Initialise(int i_kernel_size, float i_sigma)
		{
			m_kernel = CreateKernel (i_kernel_size, i_sigma);
		}


        /**
         * Apply filter on a list of vertices
         * 
         * @param[in] o_vertices	Vertex attributes to filter
         * 
         */
        public void ApplyFilter(ref Vector3[] o_vertices)
        {
            ApplyFilter(ref o_vertices, m_kernel, new Pair(0, 0), new Pair(0, 0));
        }

		/**
		 * Apply filter on a list of vertices
		 * 
		 * @param[in] o_vertices	Vertex attributes to filter
         * @param[in] i_x_offset    X start/end offset
         * @param[in] i_y_offset    Y start/end offset
		 * 
		 */
        public void ApplyFilter(ref Vector3[] o_vertices, Pair i_x_limits, Pair i_y_limits)
		{
            ApplyFilter(ref o_vertices, m_kernel, i_x_limits, i_y_limits);
		}


		/**
		 * Generate Gaussian Blur Center with m_sigma weighting
		 * 
		 * @param[in] i_kernel_size	Kernel window width
		 * @param[in] i_sigma		Kernel sigma
		 * 
		 * \return Gaussian kernel as 2D array
		 */
		private float[,] CreateKernel(int i_kernel_size, float i_sigma)
		{
			float[,] kernel = new float[i_kernel_size,i_kernel_size];
			
			float a = 1.0f / (2.0f * Mathf.PI * (i_sigma * i_sigma));
			float b = 2.0f * (i_sigma * i_sigma);
			
			float sum = 0.0f;
			i_kernel_size -= 1;
			
			// Center (0,0) at kernel center
			for( int y = -1; y < i_kernel_size; ++y)
			{
				for(int x = -1; x < i_kernel_size; ++x)
				{
					kernel[x+1,y+1] = a * Mathf.Exp (  ((float)(-(x*x) - (y*y))) / b);
					sum += kernel[x+1,y+1];
				}
			}
			
			// Normalise kernel
			i_kernel_size += 1;
			
			for (int x = 0; x < i_kernel_size; ++x) 
			{
				for(int y = 0; y < i_kernel_size; ++y)
				{
					kernel[x,y] /= sum;
				}
			}
			
			return kernel;
		}
	}

}