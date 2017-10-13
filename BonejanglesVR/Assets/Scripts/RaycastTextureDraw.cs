using VRTK;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent(typeof(MeshCollider), typeof(Renderer))]
    public class RaycastTextureDraw : MonoBehaviour
    {
        private MeshCollider meshCollider;
        private Texture2D objectTexture;
        private Texture2D originalTexture;
        private Renderer rend;

        public Color drawingColor;
        public int blockSize;
        private Color[] drawColorBlock;
        private Color[] eraseColorBlock;

        private void Awake()
        {
            meshCollider = GetComponent<MeshCollider>();
            rend = GetComponent<Renderer>();
            if (rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null) {
                Debug.LogError("The renderer must have a main texture with proper format applied. " +
                    "Try changing texture format to RGBA32 in the override settings for texture");
                enabled = false;
            }
        }

        private void Start()
        {
            originalTexture = rend.sharedMaterial.mainTexture as Texture2D;
            Texture2D clonedTexture = CloneTexture(originalTexture);
            if (clonedTexture) {
                rend.material.mainTexture = clonedTexture;
                objectTexture = clonedTexture;
                InitColorBlocks();
            }
            else
                enabled = false;
        }

        private void OnDisable() {
            // trigger cleanup
            objectTexture = null;
        }

        public Texture2D CloneTexture(Texture2D tex, TextureFormat format = TextureFormat.RGBA32, bool mipMap = false)
        {
            if (tex == null)
                return null;

            Texture2D newTex = new Texture2D(tex.width, tex.height, format, mipMap);
            newTex.SetPixels(tex.GetPixels());
            return newTex;
        }

        private void InitColorBlocks()
        {
            drawColorBlock = new Color[blockSize*blockSize];
            eraseColorBlock = new Color[blockSize * blockSize];
            Color eColor = Color.white;
            for(int i = 0; i < drawColorBlock.Length; i++) {
                drawColorBlock[i] = drawingColor;
                eraseColorBlock[i] = eColor;
            }
        }

        public void Draw(object sender, DestinationMarkerEventArgs args)
        {
            if(args.target == transform) 
            {
                Vector2 pixelUV = args.raycastHit.textureCoord;
                pixelUV.x *= objectTexture.width;
                pixelUV.y *= objectTexture.height;
                if(objectTexture.width - pixelUV.x > blockSize && objectTexture.height - pixelUV.y > blockSize)
                    objectTexture.SetPixels((int)pixelUV.x, (int)pixelUV.y, blockSize, blockSize, drawColorBlock);
                objectTexture.Apply();
            }
        }
    }
}