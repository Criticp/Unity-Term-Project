// Stylized Water 3 by Staggart Creations (http://staggart.xyz)
// COPYRIGHT PROTECTED UNDER THE UNITY ASSET STORE EULA (https://unity.com/legal/as-terms)
//    • Copying or referencing source code for the production of new asset store, or public, content is strictly prohibited!
//    • Uploading this file to a public repository will subject it to an automated DMCA takedown request.

using UnityEditor;

namespace StylizedWater3.DynamicEffects
{
    public class DynamicEffects : Extension
    {
        private DynamicEffects()
        {
            this.name = "Dynamic Effects";
            this.description = "Enables advanced effects to be projected onto the water surface. Such as boat wakes, ripples and shoreline waves.";

            this.version = "3.0.5";
            this.minBaseVersion = "3.0.4";
        }

        public static DynamicEffects extension;

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Init()
        {
            extension = new DynamicEffects();
        }
        #endif        
    }
}