using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // An isolated copy of the current character leaves the gameplay camera and actor untouched.
    public sealed class SettingsCharacterPreview : MonoBehaviour
    {
        GameObject stage;Camera previewCamera;RenderTexture texture;RawImage image;
        public void Initialize(WorldView world)
        {
            image=GetComponent<RawImage>();stage=new GameObject("Settings preview stage");stage.transform.position=new Vector3(0,-1000,0);
            var character=world.CreateCharacterPreview(stage.transform);character.transform.localRotation=Quaternion.Euler(0,165,0);
            foreach(var child in stage.GetComponentsInChildren<Transform>())child.gameObject.layer=31;
            var light=new GameObject("Preview light").AddComponent<Light>();light.transform.SetParent(stage.transform,false);light.type=LightType.Directional;light.intensity=2;light.color=new Color(1,.87f,.68f);light.cullingMask=1<<31;light.transform.rotation=Quaternion.Euler(35,-25,0);
            previewCamera=new GameObject("Settings preview camera").AddComponent<Camera>();previewCamera.transform.SetParent(stage.transform,false);previewCamera.transform.localPosition=new Vector3(0,2.7f,-7);previewCamera.transform.LookAt(stage.transform.position+Vector3.up*1.2f);
            previewCamera.orthographic=true;previewCamera.orthographicSize=2.3f;previewCamera.nearClipPlane=.1f;previewCamera.farClipPlane=15;previewCamera.cullingMask=1<<31;previewCamera.clearFlags=CameraClearFlags.SolidColor;previewCamera.backgroundColor=Color.clear;previewCamera.allowHDR=false;previewCamera.allowMSAA=false;
        }
        public void Resize(float width,float height)
        {
            if(previewCamera==null)return;int h=Mathf.Clamp(Mathf.RoundToInt(height),64,1024),w=Mathf.Clamp(Mathf.RoundToInt(h*width/Mathf.Max(1,height)),64,1024);
            if(texture!=null&&texture.width==w&&texture.height==h)return;ReleaseTexture();texture=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){name="Settings character portrait"};texture.Create();image.texture=texture;previewCamera.targetTexture=texture;previewCamera.aspect=width/Mathf.Max(1,height);previewCamera.orthographicSize=Mathf.Max(2.1f,1.65f/previewCamera.aspect);
        }
        void OnEnable(){if(stage!=null)stage.SetActive(true);}
        void OnDisable(){if(stage!=null)stage.SetActive(false);ReleaseTexture();}
        void ReleaseTexture(){if(previewCamera!=null)previewCamera.targetTexture=null;if(image!=null)image.texture=null;if(texture!=null){texture.Release();Destroy(texture);texture=null;}}
        void OnDestroy(){ReleaseTexture();if(stage!=null)Destroy(stage);}
    }
}
