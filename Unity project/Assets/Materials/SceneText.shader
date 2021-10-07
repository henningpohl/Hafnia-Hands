Shader "GUI/SceneText" {

    Properties
    {
        _MainTex("Font Texture", 2D) = "white" {}
        _Color("Text Color", Color) = (1,1,1,1)
    }

    SubShader
    {
            Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
            Lighting Off Cull back ZWrite Off ZTest On Fog { Mode Off }
            Blend SrcAlpha OneMinusSrcAlpha
            Pass
            {
            Color[_Color]
            SetTexture[_MainTex]
            {
                combine primary, texture * primary
            }
            }
    }
    
}
