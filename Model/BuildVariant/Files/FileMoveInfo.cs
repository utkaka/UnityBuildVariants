namespace BuildVariants.Model.BuildVariant.Files {
    public class FileMoveInfo {
        public string From { get; set; }
        public string To { get; set; }
        public BuildStage PerformOnStage { get; set; }
        public bool Revert { get; set; }
    }
}