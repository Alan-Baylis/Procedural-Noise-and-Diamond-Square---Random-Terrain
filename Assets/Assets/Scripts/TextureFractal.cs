using UnityEngine;
using System.Collections;


namespace Texture
{
	public enum ProceduralFuncs {Value, Lattice, Perlin};

	[System.Serializable]
	public class FractalProperties
	{
		/// Number of frequency octaves to sum
		[Range(1, 8)]
		public int 		m_octaves = 3;

		/// Frequency ratio between harmonics
		[Range(1.0f, 4.0f)]
		public float	m_lucunarity = 3.68f; 

		/// Amplitude degredation per decade
		[Range(0.0f, 1.0f)]
		public float	m_persistence = 0.47f; 

		/// Noise dimensionality
		[Range(1,2)]
		public int 		m_noise_dimensions = 2;

		/// Fractal frequency
		public float 	m_noise_frequency = 8.1f;

		/// Procedural noise method
		public ProceduralFuncs	m_methods = ProceduralFuncs.Perlin;
	}


	public class TextureFractal : TextureBase 
	{
		/// Struct for user configurable fractal configuration
		public FractalProperties	m_fractal_config;

		/// Gradient object for colour queries
		public Gradient colouring;

		/// Procedural noise method pointer
		public static Noise.NoiseMethod[] m_procedural_funcs = 	
			{	
				Noise.ProceduralNoise.Value1D,
				Noise.ProceduralNoise.Lattice1D,
				Noise.ProceduralNoise.Perlin1D,
				Noise.ProceduralNoise.Value2D,
				Noise.ProceduralNoise.Lattice2D,
				Noise.ProceduralNoise.Perlin2D
			};

		/// Texture colour for (u,v) co-ordinates
		Color[,] m_uv;

		/// Noise value for (u,v) co-ordinates
		float[,] m_noise;

		/*
		 * Create fractal and attach it to game objects texture
		 * 
		 * \return True if fractal texture was generated successfully
		 */
		protected bool CreateFractal()
		{
			bool ret = TextureExist();

			if (!ret) 
			{
				ret = CreateTexture();
			}

			if (ret) 
			{
				UpdateFractal();
			}

			return ret;
		}


		/**
		 * Checks if the user configuration matches the texture information.
		 * If false, create new fractal and apply texture
		 */
		public void UpdateFractal()
		{
			m_uv = CreateFractal (
				GetTextureSize(), 
				m_fractal_config.m_noise_frequency,
				m_fractal_config.m_octaves,
				m_fractal_config.m_lucunarity,
				m_fractal_config.m_persistence);

			ApplyTexture(m_uv);
		}


		/**
		 * Populate a 2D array with colour values of a procedurally generated fractal.
		 * 
		 * @param[in] i_size			Texture dimensions (x mirrors y)
		 * @param[in] i_noise_frequency	Periodicity of the permutation array
		 * @param[in] i_octaves			Number of octaves for fractal mixing
		 * @param[in] i_lucunarity		Frequency ratio between harmonics
		 * param[in]  i_persistence		Amplitude degredation per decade
		 * 
		 * \return 2D array containing texture values
		 */
		protected Color[,] CreateFractal(
	   		int i_size, 
			float i_noise_frequency, 
			int i_octaves, 
			float i_lucunarity, 
			float i_persistence)
		{
			Color[,] uv = new Color[i_size + 1, i_size + 1];
			m_noise = new float[i_size + 1, i_size + 1];

			//NoiseMethod noise = ProceduralNoise.PerlinNoise[m_noise_dimensions - 1];
			
			float stride = 1.0f / i_size;

			// Fill texture with procedural noise
			for (int x = 0; x < i_size; ++x) 
			{
				for(int y = 0; y < i_size; ++y)
				{
					Vector3 point = new Vector3( (x + 0.5f) * stride,
						                         (y + 0.5f) * stride,
						                         0);

					int method = (int)m_fractal_config.m_methods + (3 * (m_fractal_config.m_noise_dimensions - 1));

					//Normalise Perlin gradients from -1 to 1 to 0 to 1
					float sample = Noise.ProceduralNoise.FractalNoise(	
					                    m_procedural_funcs[method],
	                                    point, 
	                                    i_noise_frequency, 
	                                    i_octaves, 
	                                    i_lucunarity, 
	                                    i_persistence);

					// Offset uv co-ordinates to fit lattice centers
					sample = sample * 0.5f + 0.5f;

					m_noise[y,x] = sample;
					uv[y,x] = colouring.Evaluate (sample);
				}
			}

			return uv;
		}


		/**
		 * When the game object is instantiated, generate a texture
		 */
		void OnEnable()
		{
			if (!TextureExist()) 
			{
				CreateFractal();
			}
		}


		/**
		 * Get the Color of a pixel
		 * 
		 * @param[in] i_x	X position
		 * @param[in] i_y	Y position
		 * 
		 * \return Colour allocated from Gradient object
		 */
		public Color GetColour(int i_x, int i_y)
		{
			return m_uv [i_y, i_x];
		}


		/**
		 * Get the sample of a pixel
		 * 
		 * @param[in] i_x	X position
		 * @param[in] i_y	Y position
		 * 
		 * \return Sample generated for vertex using procedural noise
		 */
		public float GetSample(int i_x, int i_y)
		{
			return m_noise[i_y, i_x];
		}


		/**
		 * Set octaves values. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetOctaves(float i_value)
		{
			i_value = Mathf.Max (1, i_value);

			m_fractal_config.m_octaves = (int)i_value;
			UpdateFractal ();
		}


		/**
		 * Set lucunarity values. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetLucunarity(float i_value)
		{
			m_fractal_config.m_lucunarity = i_value;
			UpdateFractal ();
		}


		/**
		 * Set persistence values. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetPersistence(float i_value)
		{
			m_fractal_config.m_persistence = i_value;
			UpdateFractal ();
		}


		/**
		 * Set frequency value. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetFrequency(float i_value)
		{
			i_value = Mathf.Max (0, i_value);

			m_fractal_config.m_noise_frequency = i_value;
			UpdateFractal ();
		}


		/**
		 * Set frequency value. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetNoiseDimensions(float i_value)
		{
			// Only supports 1 or 2 dimensions
			i_value = Mathf.Min (2, i_value);
			i_value = Mathf.Max (1, i_value);

			m_fractal_config.m_noise_dimensions = (int)i_value;
			UpdateFractal ();
		}


		/**
		 * Set noise method. Compatible with UI callback (dynamic float)
		 * 
		 * @pram[in] i_value Input value
		 * 
		 */
		public void SetNoiseMethod(float i_value)
		{
			// Only supports 3 methods
			i_value = Mathf.Min (2, i_value);
			i_value = Mathf.Max (0, i_value);

			switch ((int)i_value) 
			{
				case 0:
					m_fractal_config.m_methods = ProceduralFuncs.Value;
					break;
				case 1:
					m_fractal_config.m_methods = ProceduralFuncs.Lattice;
					break;
				case 2:
					m_fractal_config.m_methods = ProceduralFuncs.Perlin;
					break;
			}
			
			UpdateFractal ();
		}
	}

}
