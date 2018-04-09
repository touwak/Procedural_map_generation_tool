﻿
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
      float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;


		UNITY_INSTANCING_CBUFFER_START(Props)
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
      
      float2 uv1 = IN.worldPos.xz;
      uv1.y += _Time.y;
      float4 noise1 = tex2D(_MainTex, uv1 * 0.025);

      float2 uv2 = IN.worldPos.xz;
      uv2.x += _Time.y;
      float4 noise2 = tex2D(_MainTex, uv2 * 0.025);

      //blend
      float blendWave = sin((IN.worldPos.x + IN.worldPos.z) * 0.1 + 
        (noise1.y + noise2.z) + _Time.y);
      blendWave *= blendWave;

      //waves
      float waves = 
        lerp(noise1.z, noise1.w, blendWave) +
        lerp(noise2.x, noise2.y, blendWave);
      waves = smoothstep(0.75, 2, waves);

      fixed4 c = saturate(_Color + waves);
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		