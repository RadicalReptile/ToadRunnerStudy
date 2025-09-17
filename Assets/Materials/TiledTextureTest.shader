Shader "Custom/Banded Diffuse - Worldspace" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    _Scale("Texture Scale", Float) = 1.0
    _Precision("Precision", Float) = 0.01
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

CGPROGRAM
#pragma surface surf SimpleLambert
 
// Modified Lambert lighting from the ShaderLab manual from bart_the_13th and richardkettlewell on the Unity forum.
// They're thread can be found here: https://forum.unity.com/threads/intentional-light-banding.445714/

float _Precision;

  half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
         half NdotL = dot (s.Normal, lightDir);
         half4 c;
 
         float4 outputColor;        // this is whatever the shader is going to output. (i.e. the non-banded high precision color)
       
         float precision = _Precision;   // set the step size, eg 0.1 allows 0.0, 0.1, 0.2, 0.3, 0.4 etc. Put this is a shader constant so you can edit it easily
         c.rgb = _LightColor0.rgb * floor(s.Albedo * (NdotL * atten * 2) / precision) * precision;
         c.a = s.Alpha;
         outputColor += precision*0.5;
         return c;
  }


// Worldspace tiled texturing shader from BloodyGoldfish on Stack Overflow.
// Their original code: https://stackoverflow.com/questions/59898841/is-there-a-way-to-repeat-texture-on-unity-3d-object-without-specifying-amount-of/59902096

sampler2D _MainTex;
fixed4 _Color;
float _Scale;

struct Input {
    float3 worldNormal;
    float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {
    float2 UV;
    fixed4 c;

    if (abs(IN.worldNormal.x) > 0.5) {
        UV = IN.worldPos.yz;                // side
        c = tex2D(_MainTex, UV* _Scale);    // use WALLSIDE texture
    }
    else if (abs(IN.worldNormal.z) > 0.5) {
        UV = IN.worldPos.xy;                // front
        c = tex2D(_MainTex, UV* _Scale);    // use WALL texture
    }
    else {
        UV = IN.worldPos.xz;                // top
        c = tex2D(_MainTex, UV* _Scale);    // use FLR texture
    }

    o.Albedo = c.rgb * _Color;
}
ENDCG
}

Fallback "Legacy Shaders/VertexLit"
}