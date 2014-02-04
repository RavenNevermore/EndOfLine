Shader "--Custom--/Additive Texture"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		ZWrite Off
		Alphatest Greater 0
		Tags {Queue=Transparent}
		Blend One One 
		ColorMask RGB

		Pass
		{
			ColorMaterial AmbientAndDiffuse

			SetTexture [_MainTex]
			{
				Combine texture * primary, texture * primary
			}

			SetTexture [_MainTex]
			{
				constantColor [_TintColor]
				Combine previous * constant DOUBLE, previous * constant
			} 
		}
	}

	Fallback "Particles/Additive"
}