using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Terrain
{
    using Pair = System.Collections.Generic.KeyValuePair<int, int>;

    public class MeshEditorROI : MonoBehaviour
    {
        public GameObject m_object;
        public GameObject m_ui_object;

        public Slider m_roi_x;
        public Slider m_roi_y;
        public Slider m_roi_gain;
        public Slider m_roi_size;
        public Toggle m_edge_smoothing;

        Color32[] m_original_pixels;
        Vector3[] m_original_vertices;
        Vector3[] m_new_vertices;

        bool m_enabled;

        public void Start()
        {
            // 254 x 254 is the max and default texture size
            SetTextureSize(254.0f);
        }

        /**
         * change interactable state of ROI editing features based on checkbox state
         * 
         * @param[in] i_value   True to enable ROI editing features
         */
        public void Toggle(bool i_value)
        {
            Texture2D texture = m_object.GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;

            // store unedited texture state or apply old state
            if (i_value)
            {
                m_original_pixels = texture.GetPixels32();
                m_original_vertices = (Vector3[])m_object.GetComponent<MeshFilter>().mesh.vertices.Clone();
            }
            else
            {
                texture.SetPixels32(m_original_pixels);

                m_object.GetComponent<MeshFilter>().mesh.vertices = m_original_vertices;
                m_object.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                m_object.GetComponent<MeshRenderer>().material.mainTexture = texture;
                texture.Apply();
            }

            m_enabled = i_value;
            m_roi_x.interactable = i_value;
            m_roi_y.interactable = i_value;
            m_roi_gain.interactable = i_value;
            m_roi_size.interactable = i_value;
            m_edge_smoothing.interactable = i_value;
        }

        /**
         * Apply the ROI changes to the mesh renderer
         */
        public void Apply()
        {
            Texture2D texture = m_object.GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;

            int min_x = (int)Mathf.Max(m_roi_x.value - m_roi_size.value, 0.0f);
            int min_y = (int)Mathf.Max(m_roi_y.value - m_roi_size.value, 0.0f);

            int max_x = (int)Mathf.Min(m_roi_x.value + m_roi_size.value, (float)texture.width);
            int max_y = (int)Mathf.Min(m_roi_y.value + m_roi_size.value, (float)texture.height);

            if (m_edge_smoothing.isOn)
            {
                m_original_vertices = m_new_vertices;
                m_roi_gain.value = m_roi_gain.maxValue / 2;

                // Smooth boundary of processed ROI
                Spatial.FilterGaussian filter = new Spatial.FilterGaussian();
                filter.Initialise(5, 1.0f);

                int smooth_size = (int)m_roi_size.value / 4;

                filter.ApplyFilter(ref m_original_vertices, new Pair(min_x, max_x), new Pair(min_y - smooth_size, min_y + smooth_size));
                filter.ApplyFilter(ref m_original_vertices, new Pair(min_x, max_x), new Pair(max_y - smooth_size, max_y + smooth_size));

                filter.ApplyFilter(ref m_original_vertices, new Pair(min_x - smooth_size, min_x + smooth_size), new Pair(min_y, max_y));
                filter.ApplyFilter(ref m_original_vertices, new Pair(max_x - smooth_size, max_x + smooth_size), new Pair(min_y, max_y));

                // Update mesh and normal map
                m_ui_object.GetComponent<MeshFilter>().mesh.vertices = m_original_vertices;
                m_ui_object.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            }
        }

        /**
         * Update sliders with texture size
         * 
         * @param[in] i_value Texture size
         */
        public void SetTextureSize(float i_value)
        {
            m_roi_x.maxValue = i_value;
            m_roi_y.maxValue = i_value;
            m_roi_size.maxValue = i_value / 2;

            m_roi_x.value = m_roi_x.maxValue / 2;
            m_roi_y.value = m_roi_y.maxValue / 2;
            m_roi_size.value = m_roi_size.maxValue / 2;
        }


        /**
         * Move the ROI along the objects texture
         */
        void Update()
        {
            Texture2D texture = m_object.GetComponent<MeshRenderer>().material.GetTexture("_MainTex") as Texture2D;

            if (m_enabled)
            {
                // Return mesh to original state
                texture.SetPixels32(m_original_pixels);
                m_new_vertices = (Vector3[])m_original_vertices.Clone();

                // Edit ROI
                int min_x = (int)Mathf.Max(m_roi_x.value - m_roi_size.value, 0.0f);
                int min_y = (int)Mathf.Max(m_roi_y.value - m_roi_size.value, 0.0f);

                int max_x = (int)Mathf.Min(m_roi_x.value + m_roi_size.value, (float)texture.width);
                int max_y = (int)Mathf.Min(m_roi_y.value + m_roi_size.value, (float)texture.height);

                for (int i = min_x; i < max_x; ++i)
                {
                    for (int j = min_y; j < max_y; ++j)
                    {
                        int idx = i + ((texture.width + 1) * j);

                        Color c = texture.GetPixel(i, j);
                        Vector3 v = m_new_vertices[idx];

                        c.a = 0.1f;
                        v.y *= m_roi_gain.value;

                        texture.SetPixel(i, j, Color.white);
                        m_new_vertices[idx] = v;
                    }
                }

                // Render new attributes
                m_object.GetComponent<MeshFilter>().mesh.vertices = m_new_vertices;
                m_object.GetComponent<MeshFilter>().mesh.RecalculateNormals();

                m_object.GetComponent<MeshRenderer>().material.mainTexture = texture;
                texture.Apply();
            }
        }

    }
}