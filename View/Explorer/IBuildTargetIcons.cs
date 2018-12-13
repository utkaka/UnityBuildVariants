using UnityEditor;

namespace BuildVariants.View.Explorer {
    public interface IBuildTargetIcons {
        string GetIconForBuildTarget(BuildTarget buildTarget);
    }
}