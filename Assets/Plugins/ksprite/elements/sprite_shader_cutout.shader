Shader "kSprite/Sprite Cutout Shader" {
        Properties {
             	_Center ("Hole Center", Vector) = (.5, .5, 0 , 0)
    			_Radius ("Hole Radius", Float) = .25
    			_Border ("Border Thikness", Float) = .1
                _MainTex ("Font Texture", 2D) = "white" {}
                _Color ("Border Color", Color) = (1,1,1,1)
        }
 	
 	    SubShader {
 
                Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
                Lighting Off Cull back ZTest Always ZWrite Off Fog { Mode Off }
                Blend SrcAlpha OneMinusSrcAlpha
 
                Pass {
				         CGPROGRAM
				         #pragma vertex vert
				         #pragma fragment frag
				         
				         #include "UnityCG.cginc"
				         
				         struct appdata {
				             float4 position : POSITION;
				             half2 texCoord : TEXCOORD;
				             fixed4 color : COLOR;
				         }; 
				         
				         struct v2f {
				             half4 vertex : POSITION;
				             half4 worldPos : TEXCOORD1;
							 fixed2 texcoord : TEXCOORD0;
                             fixed4 color : COLOR;
                             
				         };
				         
				         uniform sampler2D _MainTex;
				         uniform float4 _MainTex_ST;
                         uniform fixed4 _Color;
				         uniform half2 _Center;
				         half _Radius, _Shape,_Border;
				         
				         v2f vert(appdata i) {
				             v2f o;
				             o.vertex = mul(UNITY_MATRIX_MVP, i.position);
				             o.texcoord = TRANSFORM_TEX(i.texCoord,_MainTex);
				             o.color = i.color;
				             o.worldPos = i.position;
				             return o;
				         }
				         
				         fixed4 frag(v2f i) : COLOR {        
				             fixed4 fragColor = tex2D(_MainTex, i.texcoord);
				             float dist = distance(i.worldPos, _Center);
				             if (dist > _Radius) {
				            	 fragColor.a = 0;
							 } else if (abs(dist - _Radius)<= _Border){
							 	
							 	fragColor =_Color * max(0, abs(dist - _Radius) / _Border);
							 	fragColor.a = abs(dist - _Radius) / _Border;
							 }
							 
				             return fragColor;
				         }
				         ENDCG
				 }
     }
}
