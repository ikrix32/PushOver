// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "kSprite/Text Shader" {
        Properties {
                _MainTex ("Font Texture", 2D) = "white" {}
                _Color ("Text Color", Color) = (1,1,1,1)
                _Clip ("ClipRect", Vector) = (-2000,-2000,4000,4000)
        }
 
        SubShader {
 
                Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
                Lighting Off Cull back ZTest Always ZWrite Off Fog { Mode Off }
                Blend SrcAlpha OneMinusSrcAlpha
 
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
                                half4 worldPos : TEXCOORD1;
                        };
 
                        sampler2D _MainTex;
                        uniform float4 _MainTex_ST;
                        uniform fixed4 _Color;
                        uniform	float4 _Clip;
                       
                        v2f vert (appdata_t v)
                        {
                                v2f o;
                                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
                                o.color = v.color;
                                o.worldPos = mul(_Object2World, v.vertex);
                                
                                return o;
                        }
 
                        fixed4 frag (v2f i) : COLOR
                        {
                                fixed4 col = i.color *_Color;
                                col.a *= tex2D(_MainTex, i.texcoord).a;
                   				
                   				float x = i.worldPos.x - _Clip.x;
                   				float xx= abs(x) + abs(_Clip.z - x);
                   				float y = i.worldPos.y - _Clip.y;
                   				float yy= abs(y) + abs(_Clip.w - y);
                   				
                   				float ss = (1.0 + sign(_Clip.z - xx)) * (1.0 + sign(_Clip.w - yy));
                   				
                   				col.a = ss * col.a;
                   				
                                return col;
                        }
                        ENDCG
                }
        }
}
