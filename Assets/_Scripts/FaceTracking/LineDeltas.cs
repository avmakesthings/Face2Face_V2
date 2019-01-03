using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDeltas : MonoBehaviour {

    public Color m_Color;
    public int m_Tail;

    private MeshFilter filter;
    private Vector3[] vertices;
    private List<List<Vector3>> lines = new List<List<Vector3>>();


    static Material lineMaterial;
    static void CreateLineMaterial()

    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
        
    public void OnRenderObject()
    {
        filter = GetComponent<MeshFilter>();
        if (filter.sharedMesh != null) {

            vertices = filter.sharedMesh.vertices;

            CreateLineMaterial ();
            lineMaterial.SetPass (0);
            GL.PushMatrix ();
            GL.MultMatrix (transform.localToWorldMatrix);

            for (int i = 0; i < vertices.Length; ++i) {
                if (lines.Count-1 < i) {
                    lines.Add (new List<Vector3> ());
                }

                if (lines [i].Count == m_Tail) {
                    lines [i].RemoveAt(0);
                }
                lines [i].Add (vertices [i]);

                GL.Begin (GL.LINES);
                for (int j = 0; j < lines[i].Count; ++j) {
                    float t = (float)j / (float)lines[i].Count;
                    //GL.Color (Color.Lerp(Color.clear, m_Color, t));
                    GL.Color(m_Color);

                    GL.Vertex3 (lines[i][j].x, lines[i][j].y, lines[i][j].z);
                }
                GL.End ();
            }


            GL.PopMatrix ();
        }
    }
        
}
