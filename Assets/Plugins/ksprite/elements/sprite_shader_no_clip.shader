Shader "kSprite/Sprite ShaderN" {
        Properties {
			_MainTex ("Sprite Texture", 2D) = "white" {}
			_Color ("Tint", Color) = (1,1,1,1)
			PixelSnap ("Pixel snap", Float) = 0
        }
 	
 	    SubShader {
 
                Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Mode Off }
				Blend One OneMinusSrcAlpha
 
 
                Pass { 
                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #pragma fragmentoption ARB_precision_hint_fastest
 
                        #include "UnityCG.cginc"
 
                        struct appdata_t {
                                half4 vertex : POSITION;
                                fixed2 texcoord : TEXCOORD0;
                                fixed4 color : COLOR;
                        };
 
                        struct v2f {
                                half4 vertex : POSITION;
                                fixed2 texcoord : TEXCOORD0;
                                fixed4 color : COLOR;
                                //half4 worldPos : TEXCOORD1;
                        };
 
                        sampler2D _MainTex;
                        uniform float4 _MainTex_ST;
                        uniform fixed4 _Color;
                        //uniform	float4 _Clip;
                       
                        v2f vert (appdata_t v)
                        {
                                v2f o;
                                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                                o.color = v.color * _Color;
                                //o.worldPos = mul(_Object2World, v.vertex);
                                
                                return o;
                        }
 
                        fixed4 frag (v2f i) : COLOR
                        {
                            fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
							c.rgb *= c.a;
							return c;
                        }
                        ENDCG
                        
                        //Color [_Color]
                        //SetTexture [_MainTex] {
                        //        combine texture * primary
                        //}
                }
        }      
}
