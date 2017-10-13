// Write black pixels onto the GameObject that is located
// by the script. The script is attached to the camera.
// Determine where the collider hits and modify the texture at that point.
//
// Note that the MeshCollider on the GameObject must have Convex turned off. This allows
// concave GameObjects to be included in collision in this example.
//
// Also to allow the texture to be updated by mouse button clicks it must have the Read/Write
// Enabled option set to true in its Advanced import settings.

using UnityEngine;
using System.Collections;

public class WriteIntoTex : MonoBehaviour
{
    public Color drawColor = new Color(0.5f, 0.1f, 0.5f, 1);
    public int blockSize;
    private Color[] draw;
    private Color[] erase;
    public Camera cam;

    public MeshCollider meshCollider;
    public Renderer rend;
    private Texture2D tex;

    void Start()
    {
        cam = GetComponent<Camera>();
        draw = new Color[blockSize * blockSize];
        erase = new Color[blockSize * blockSize];
        Color w = Color.white;
        for (int i = 0; i < draw.Length; i++) {
            draw[i] = drawColor;
            erase[i] = w;
        }

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;
        tex = rend.material.mainTexture as Texture2D;
        TextureClone();
    }

    private void TextureClone()
    {
        Texture2D newTex = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
        newTex.SetPixels32(tex.GetPixels32());
        rend.material.mainTexture = newTex;
        tex = newTex;
    }

    void Update()
    {
        bool left = Input.GetMouseButton(0);
        bool right = Input.GetMouseButton(1);
        if (right || left) {

            RaycastHit hit;
            if (!Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
                return;

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= tex.width;
            pixelUV.y *= tex.height;

            if(left)
                tex.SetPixels((int)pixelUV.x, (int)pixelUV.y, blockSize, blockSize, draw);
            else
                tex.SetPixels((int)pixelUV.x, (int)pixelUV.y, blockSize, blockSize, erase);
            tex.Apply();
        }
    }
}