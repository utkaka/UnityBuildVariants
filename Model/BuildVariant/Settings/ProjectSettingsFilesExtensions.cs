using System.Collections.Generic;
using System.Linq;

namespace BuildVariants.Model.BuildVariant.Settings {
    public static class ProjectSettingsFilesExtensions {        
        public static IEnumerable<IProjectSettingsFile> Merge(
            this IEnumerable<IProjectSettingsFile> settings) {
            var groupedFiles = settings.Where(p => p.RootNode != null).GroupBy(
                p => p.FileName, 
                p => p,
                (filename, files) => new { FileName = filename, ProjectSettingsFiles = files.ToList() });

            foreach (var group in groupedFiles) {
                IProjectSettingsFile projectSettingsFile = null;
                foreach (var file in group.ProjectSettingsFiles) {
                    projectSettingsFile = projectSettingsFile == null ? file : projectSettingsFile.Merge(file);
                }
                yield return projectSettingsFile;
            }
        }

        public static IEnumerable<IProjectSettingsFile> Append(
            this IEnumerable<IProjectSettingsFile> settings,
            IProjectSettingsFile projectSettingsFile) {
            foreach (var setting in settings) {
                yield return setting;
            }
            yield return projectSettingsFile;
        }

        public static IEnumerable<IProjectSettingsFile> Diff(this IEnumerable<IProjectSettingsFile> settings,
            IEnumerable<IProjectSettingsFile> otherSettings) {
            var groupedFiles = settings.Concat(otherSettings).GroupBy(
                p => p.FileName, 
                p => p,
                (filename, files) => new { FileName = filename, ProjectSettingsFiles = files.ToList() });
            foreach (var group in groupedFiles) {
                IProjectSettingsFile projectSettingsFile = null;
                foreach (var file in group.ProjectSettingsFiles) {
                    projectSettingsFile = projectSettingsFile == null ? file : projectSettingsFile.Diff(file);
                }
                if (projectSettingsFile != null) yield return projectSettingsFile;
            }
        }
    }
}