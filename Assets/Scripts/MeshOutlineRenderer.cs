using UnityEngine;
using System.Collections;

public class MeshOutlineRenderer : MonoBehaviour 
{
    public GameObject m_game_object;

    Mesh m_mesh;

    void Start()
    {
        m_mesh = null;

        gameObject.AddComponent<LineRenderer>();
        gameObject.AddComponent<MeshFilter>();

        if (gameObject.GetComponent<MeshRenderer>() == null)
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }

    void Update()
    {
        if (m_mesh == m_game_object.GetComponent<MeshFilter>().mesh)
        {
            return;
        }
        else
        {
            RenderLines();
        }
    }

    public void RenderLines()
    {
        m_mesh = m_game_object.GetComponent<MeshFilter>().mesh;

        if (m_mesh != null)
        {
            // Render mesh and materials
            Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;

            mesh.vertices = m_mesh.vertices;
            mesh.triangles = m_mesh.triangles;
            mesh.uv = m_mesh.uv;
            mesh.colors = m_mesh.colors;
            mesh.RecalculateNormals();

            gameObject.GetComponent<MeshRenderer>().material.mainTexture = m_game_object.GetComponent<MeshRenderer>().material.mainTexture;

            // Render outline
            LineRenderer line_renderer = gameObject.GetComponent<LineRenderer>();

            Color line_colour = new Color(1.0f, 1.0f, 1.0f, 0.1f);
            line_renderer.SetColors(line_colour, line_colour);
            line_renderer.SetWidth(0.0002f, 0.0002f);
            line_renderer.useWorldSpace = false;

            Vector3[] vertices = m_mesh.vertices;
            int[] triangles = m_mesh.triangles;

            int i = 0;
            foreach (int idx in triangles)
            {
                line_renderer.SetVertexCount(i + 1);
                line_renderer.SetPosition(i, vertices[idx]);
                ++i;
            }

            return;
        }
    }
}
