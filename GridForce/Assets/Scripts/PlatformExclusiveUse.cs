using UnityEngine;
using System.Collections;

public class PlatformExclusiveUse : MonoBehaviour
{
    public Component exclusiveComponent = null;
    public TargetPlatform targetPlatform = TargetPlatform.Standalone;

	// Use this for initialization
	void Start ()
    {
        switch (this.targetPlatform)
        {
            case TargetPlatform.Standalone:
                #if !UNITY_EDITOR && !UNITY_STANDALONE
                
                UnityEngine.Object.Destroy(this.exclusiveComponent);

                #endif
                break;

            case TargetPlatform.iOS:
                #if !UNITY_IPHONE

                UnityEngine.Object.Destroy(this.exclusiveComponent);

                #endif
                break;
        }
        UnityEngine.Object.Destroy(this);
	}
}

public enum TargetPlatform
{
    Standalone,
    iOS
}