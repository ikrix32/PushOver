using UnityEngine;
using System.Collections;

public class TextureProcessor  
{

	public static Texture ApplyBlur(Texture texture,int downsample){
		Texture source = texture;

		//blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
		source.filterMode = FilterMode.Bilinear;

		int rtW = source.width >> downsample;
		int rtH = source.height >> downsample;

		Shader shader = Shader.Find("kShader/BlurHQ");
		Material blurMaterial = new Material(shader);

		//blurMaterial.GetTexture
		int noPasses = 7;//lurType == BlurType.StandardGauss ? 0 : 2;

		RenderTexture rt = null;
		for(int i = 0; i < noPasses; i++) {
			//blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));
			rt = RenderTexture.GetTemporary (rtW, rtH);
			rt.filterMode = FilterMode.Bilinear;

			Graphics.Blit (source, rt, blurMaterial, i);

			if(source is RenderTexture && source != texture)
				RenderTexture.ReleaseTemporary ((RenderTexture)source);

			source = rt;
		}

		//Graphics.Blit (rt, source);
		if(rt != null){
			RenderTexture currentRT = RenderTexture.active;
			RenderTexture.active = rt;
			Texture2D tex = new Texture2D(rtW,rtH);
			tex.ReadPixels(new Rect(0, 0, rtW, rtH), 0, 0);
			tex.Apply();

			RenderTexture.active = currentRT;

			RenderTexture.ReleaseTemporary (rt);
			tex.name = "Blurred Screenshot";

			return tex;
		}

		return texture;
	}

	public static Texture2D CopyTextureArea(Texture2D source,Rect rect)
	{
		RenderTexture rt = RenderTexture.GetTemporary (source.width, source.height);
		rt.filterMode = FilterMode.Bilinear;

		Graphics.Blit (source, rt);

		RenderTexture.active = rt;
		Texture2D tex = new Texture2D((int)rect.width,(int)rect.height);
		tex.ReadPixels(rect, 0, 0);
		tex.Apply();

		RenderTexture.ReleaseTemporary (rt);

		return tex;
	}
}
