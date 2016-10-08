using UnityEngine;
using System.Collections;
using System;

namespace Noise
{
	public delegate float NoiseMethod (Vector3 point, float frequency);

	public static class ProceduralNoise 
	{
		/// 512 Permutation array Ken Perlin used for his reference implmentations
		private static int[] m_hash = {
			151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
			140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
			247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
			57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
			74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
			60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
			65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
			200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
			52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
			207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
			119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
			129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
			218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
			81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
			184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
			222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

			151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
			140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
			247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
			57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
			74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
			60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
			65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
			200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
			52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
			207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
			119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
			129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
			218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
			81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
			184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
			222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
		};

		/// Permutation array mask equal to the array size
		const int m_hash_mask_size = 255;

		/// Noise gradients for interpolating perlin noise
		private static Vector2[] m_gradients = 	{
													new Vector2( 1f, 0f),
													new Vector2(-1f, 0f),
													new Vector2( 0f, 1f),
													new Vector2( 0f,-1f),
												};
		
		private const int m_gradientsMask1D = 1;
		private const int m_gradientsMask2D = 3;


		/**
		 * Generate fractal noise (1/f) using harmonics and noise properties
		 * with the input method provided.
		 * 
		 * 
		 * @param[in] i_uv				Vertex position
		 * @param[in] i_noise_frequency	Periodicity of the permutation array
		 * @param[in] i_octaves			Number of octaves for fractal mixing
		 * @param[in] i_lucunarity		Frequency ratio between harmonics
		 * param[in]  i_persistence		Amplitude degredation per decade
		 * 
		 * \return randomly generated value
		 */
		public static float FractalNoise(NoiseMethod 	i_function, 
		                                 Vector3 		i_uv, 
		                                 float 			i_frequency, 
		                                 int 			i_octaves, 
		                                 float 			i_lacunarity, 
		                                 float 			i_persistence)
		{
			float sum = i_function(i_uv, i_frequency);
			float amplitude = 1.0f;
			float range = 1.0f;

			for (int o = 1; o < i_octaves; o++) 
			{
				i_frequency *= i_lacunarity;
				amplitude *= i_persistence;
				range += amplitude;
				sum += i_function(i_uv, i_frequency) * amplitude;
			}

			return sum / range;
		}


		/**
		 * Compute perlin noise using permutation array and noise gradient co-linear to x/y plane
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Perlin1D (Vector3 i_uv, float i_freq) 
		{
			i_uv *= i_freq;

			float t_0 = i_uv.x - Mathf.FloorToInt(i_uv.x);

			int u = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;

			// Use hash index to select a random diretion for the periodic. Without downsampling
			// The adjacent pixel, i + 1, would have the next hash iteration of pixel, i.
			// gradient t_0 to t_1. Directions are +/- x.
			float g_0 = m_gradients[m_hash[u] & m_gradientsMask1D].x;
			float g_1 = m_gradients[(m_hash[u]+1) & m_gradientsMask1D].x;

			// Every lattice has the same gradient (starts at t_0s and ends at t_1s).
			// Interpolate gradients with smoothed magnitude along unit vector. 
			// Interpolating two normalised opposing gradients = 0.5 so scale by 2
			return Mathf.Lerp (t_0 * g_0, (t_0 - 1.0f) * g_1, FifthOrderSmoothing (t_0)) * 2.0f;
		}


		/**
		 * Compute perlin noise using permutation array and 2D noise gradients
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Perlin2D (Vector3 i_uv, float frequency) 
		{
			i_uv *= frequency;

			// Compute start and end time of iteration
			float tx_0 = i_uv.x - Mathf.FloorToInt(i_uv.x);
			float ty_0 = i_uv.y - Mathf.FloorToInt(i_uv.y);

			float tx_1 = tx_0 - 1.0f;
			float ty_1 = ty_0 - 1.0f;

			// Compute hash keys for lattice control points in x and y
			int u_0 = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;
			int v_0 = Mathf.FloorToInt(i_uv.y) & m_hash_mask_size;

			int v_1 = v_0 + 1;

			// Get hash value for lattice control points horizontal components, px1 and px2.
			// Using 2D hash indexing increases random behaviour of permutation
			// (u + v) may be periodic at N frequencies depending on the downsample factor.
			int hash_u0 = m_hash[u_0];
			int hash_u1 = m_hash[u_0 + 1];

			// Compute gradient in (x,y) 
			// m (minus) implies direction pointing towards origin
			Vector2 gradient_mu = m_gradients[ m_hash[hash_u0 + v_0] & m_gradientsMask2D ];
			Vector2 gradient_u 	= m_gradients[ m_hash[hash_u1 + v_0] & m_gradientsMask2D ];
			Vector2 gradient_v 	= m_gradients[ m_hash[hash_u0 + v_1] & m_gradientsMask2D ];
			Vector2 gradient_uv = m_gradients[ m_hash[hash_u1 + v_1] & m_gradientsMask2D ];

			// Extract the magnitude of gradient unit vectors at t(x,y). Can think of 
			// which time, t = 0 or t = 1 to use based on the starting point (x,y) of quad side.
			// Note (0,0) is bottom left corner, rendering to (1,0), then (0,1) etc...
			// Select a point along 4 unit vectors to create 2 new gradients
			float mu 	= Dot(gradient_mu, 	tx_0, ty_0);
			float u 	= Dot(gradient_u, 	tx_1, ty_0);
			float v 	= Dot(gradient_v, 	tx_0, ty_1);
			float uv 	= Dot(gradient_uv, 	tx_1, ty_1);
			
			float tx = FifthOrderSmoothing(tx_0);

			// Select a point along the new horizontal and vertical vector in x
			// then interpolate along them to translate along y
			// Interpolating two normalised opposing gradients = 0.5 so scale by 2
			return Mathf.Lerp( 	Mathf.Lerp(mu, u, FifthOrderSmoothing(tx_0)),
			                  	Mathf.Lerp(v, uv, FifthOrderSmoothing(tx_0)),
			                  	FifthOrderSmoothing(ty_0)) * 2.0f;
		}


		/*
		 * Apply 1D lattice value to input cell. Assigns random value 
		 * using downsampled value as hash index.
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Value1D(Vector3 i_uv, float frequency = 1.0f)
		{
			i_uv *= frequency;

			int i = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;
			return ((float)m_hash[i]) / (float)m_hash_mask_size ;
		}


		/*
		 * Apply 2D lattice noise to input cell. Assigns random value 
		 * using downsampled value as hash index.
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Value2D(Vector3 i_uv, float frequency = 1.0f)
		{
			// apply downsampling
			i_uv *= frequency;

			// map resolution to hash index (bit mask) then normalise max to 1
			int i = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;
			int j = Mathf.FloorToInt(i_uv.y) & m_hash_mask_size;

			return ((float)m_hash[m_hash[i] + j]) / (float)m_hash_mask_size;
		}


		/**
		 * Applies a 5th order polynomial to interpolate between 2 values 
		 * per lattice on plane co-linear to x-axis
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Lattice1D(Vector3 i_uv, float frequency = 1.0f)
		{
			i_uv *= frequency;

			// Assign two values per lattice
			float t = i_uv.x - Mathf.FloorToInt(i_uv.x);

			int i = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;
			int j = i + 1;

			i = m_hash[i];
			j = m_hash[j];

			t = FifthOrderSmoothing (t);
			return Mathf.Lerp (i,j,t) / (float)m_hash_mask_size;
		}


		/**
		 * Applies a 5th order polynomial to interpolate between 2 values 
		 * per lattice to planes co-linear to x and y axis'
		 * 
		 * @param[in] i_uv 		Sample position (x,y)
		 * @param[in] i_freq	Frequency of permutation accessing
		 * 
		 * \return randomly generated value
		 */
		public static float Lattice2D(Vector3 i_uv, float frequency = 1.0f)
		{
			// apply downsampling
			i_uv *= frequency;

			float tx = i_uv.x - Mathf.FloorToInt(i_uv.x);
			float ty = i_uv.y - Mathf.FloorToInt(i_uv.y);

			// map resolution to hash index (bit mask) then normalise max to 1
			int ix0 = Mathf.FloorToInt(i_uv.x) & m_hash_mask_size;
			int iy0 = Mathf.FloorToInt(i_uv.y) & m_hash_mask_size;
			int ix1 = ix0 + 1;
			int iy1 = iy0 + 1;

			int h0 = m_hash[ix0];
			int h1 = m_hash[ix1];
			int h00 = m_hash[h0 + iy0];
			int h10 = m_hash[h1 + iy0];
			int h01 = m_hash[h0 + iy1];
			int h11 = m_hash[h1 + iy1];

			tx = FifthOrderSmoothing(tx);
			ty = FifthOrderSmoothing(ty);

			return Mathf.Lerp(	Mathf.Lerp(h00, h10, tx),
								Mathf.Lerp(h01, h11, tx),
			                  	ty) / (float)m_hash_mask_size;
								
		}


		/**
		 * Dot product
		 * 
		 * \return dot product of input
		 */
		private static float Dot (Vector2 i_line, float i_x, float i_y) 
		{
			return i_line.x * i_x + i_line.y * i_y;
		}
		

		/* 
		 * Smooth sample with fifth order derivatives - See Ken Perlin's 
		 * paper for Noise Derivative function definition
		 * 
		 * \return fifth order derivative of input
		 */
		private static float FifthOrderSmoothing(float t)
		{
			return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
		}
		
		/*
		 * Downsample point
		 * 
		 * \return downsampled point
		 */
		private static float PointValue(float i_x, float frequency = 1.0f)
		{
			// Apply downsampling
			i_x *= frequency;
			return Mathf.FloorToInt(i_x) & 1;
		}
	}
}
