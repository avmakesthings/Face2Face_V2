using UnityEngine;
using System.Collections;

public class RenderGLWires : MonoBehaviour {
    void OnPreRender() {
        GL.wireframe = true;
    }
    void OnPostRender() {
        GL.wireframe = false;
    }
}