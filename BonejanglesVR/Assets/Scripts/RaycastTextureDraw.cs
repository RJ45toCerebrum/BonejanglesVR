using VRTK;
using UnityEngine;

namespace com.EvolveVR.BonejanglesVR
{
    [RequireComponent(typeof(MeshCollider), typeof(Renderer))]
    public class RaycastTextureDraw : MonoBehaviour
    {
        public enum FilterType {Name, Tag};

        private Texture2D objectTexture;
        private Texture2D originalTexture;
        private Renderer rend;

        [SerializeField]
        private Color drawingColor;
        public int blockSize;
        private Color[] drawColorBlock;
        private Color[] eraseColorBlock;

        [Tooltip("SenderInfo and allowedSenderFilter is used to filter who is allowed to draw into texture")]
        public string senderInfo;
        public FilterType allowedSenderFilter = FilterType.Name;

        public Color DrawingColor
        {
            get { return drawingColor; }
        }


        private void Awake()
        {
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
                SetColorBlock(drawingColor);
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

        public void SetColorBlock(Color color)
        {
            drawColorBlock = new Color[blockSize*blockSize];
            eraseColorBlock = new Color[blockSize * blockSize];
            Color eColor = Color.white;
            for(int i = 0; i < drawColorBlock.Length; i++) {
                drawColorBlock[i] = color;
                eraseColorBlock[i] = eColor;
            }
        }

        public void Draw(GameObject sender, RaycastHit hit)
        {
            if(allowedSenderFilter == FilterType.Name) {
                if (sender.name != senderInfo)
                    return;
            }
            else {
                if (sender.tag != senderInfo)
                    return;
            }

            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= objectTexture.width;
            pixelUV.y *= objectTexture.height;
            if(objectTexture.width - pixelUV.x > blockSize && objectTexture.height - pixelUV.y > blockSize)
                objectTexture.SetPixels((int)pixelUV.x, (int)pixelUV.y, blockSize, blockSize, drawColorBlock);
            objectTexture.Apply();
        }
    }
}