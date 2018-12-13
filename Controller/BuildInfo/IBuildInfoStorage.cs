namespace BuildVariants.Controller.BuildInfo {
    public interface IBuildInfoStorage {
        Model.BuildInfo.BuildInfo LoadBuildInfo();
        void SaveBuildInfo(Model.BuildInfo.BuildInfo buildInfo);
    }
}