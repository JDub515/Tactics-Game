Shader "Custom/Stencil Mask" {
	SubShader{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1"}
		LOD 200

		ZWrite Off
		ColorMask 0

		Stencil{
			Ref 1
			Pass replace
		}

		Pass{}
	}
}
