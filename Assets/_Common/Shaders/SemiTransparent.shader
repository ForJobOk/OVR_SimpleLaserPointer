Shader "Custom/SemiTransparent" {
    Properties { 
        _MainColor("_Color",Color) = (0,0,0,0)
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
    } 

	SubShader {
		Tags { "Queue" = "Transparent" }
		LOD 200

        Pass{
            ZWrite On
            ColorMask 0
        }

		CGPROGRAM
		#pragma surface surf Standard alpha:fade 
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
            float3 viewDir;
		};

        float _Alpha;
        fixed4 _MainColor;    
        float4 _RimColor;
        float _RimPower;   

		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _MainColor;
            o.Alpha = _MainColor.a;
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow (rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}