using UnityEditor;

namespace BuildVariants.View.Explorer {
    public class BuildTargetIcons : IBuildTargetIcons {
        
        public string GetIconForBuildTarget(BuildTarget buildTarget) {
            switch ((int)buildTarget) {
                case 19:
                case 17:
                case 2:
                case 4:
                case 5:
                case 24:
                case 25:
                case 27:
                    return "BuildSettings.Standalone.Small";
                case 20:
                    return "BuildSettings.WebGL.Small";
                case 9:
                    return "BuildSettings.iPhone.Small";
                case 37:
                    return "BuildSettings.tvOS.Small";
                case 13:
                    return "BuildSettings.Android.Small";
                case 21:
                    return "BuildSettings.Metro.Small";
                case 29:
                    return "BuildSettings.Tizen.Small";
                case 30:
                    return "BuildSettings.PSP2.Small";
                case 31:
                    return "BuildSettings.PS4.Small";
                case 32:
                    return "BuildSettings.PSM.Small";
                case 33:
                    return "BuildSettings.XboxOne.Small";
                case 34:
                    return "BuildSettings.SamsungTV.Small";
                case 35:
                    return "BuildSettings.N3DS.Small";
                case 36:
                    return "BuildSettings.WiiU.Small";
                case 38:
                    return "BuildSettings.Switch.Small";
                case -2:
                    return null;
                default:
                    return null;
            }
        }
    }
}