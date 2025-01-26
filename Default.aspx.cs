using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.Json;
using System.Web;
using System.Web.UI;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    public static string projectPath = HttpContext.Current.Server.MapPath("~/");
    public string previousPath = System.IO.Path.Combine(projectPath, "folderStates.json");

    public class PathInfo
    {
        public string Name { get; set; }
        public bool Accessed { get; set; }
        public List<PathFile> Files { get; set; }
    }

    public class PathFile
    {
        public string Name { get; set; }
        public FileStatus Status { get; set; }
        public DateTime LastModified { get; set; }
        public int Version { get; set; }
        public bool Accessed { get; set; }
    }

    public enum FileStatus
    {
        Same = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3,
    }

    protected void Start(object sender, EventArgs e)
    {
        try
        {
            var fullpath = string.Empty;
            if (Path.Text.Contains("~"))
            {
                fullpath = Server.MapPath(Path.Text);
            }
            else
            {
                fullpath = Path.Text;
            }
            
            if (Directory.Exists(fullpath))
            {
                var previousState = GetPreviousState(previousPath, fullpath);

                var currentState = GetFilesWithDetails(fullpath);

                var compareResult = Compare(previousState, currentState);

                Output.Text = compareResult.Item1.Replace(Environment.NewLine, "<br />");

                SaveCurrentState(previousPath, compareResult.Item2);
            }
            else
            {
                Output.Text = "Folder does not exist!";
            }
        }
        catch (Exception ex)
        {
            Output.Text = $"Error: {ex.Message}";
        }
    }

    static PathInfo GetFilesWithDetails(string folderPath)
    {
        var pathInfo = new PathInfo
        {
            Name = folderPath,
            Accessed = HasReadAccess(folderPath),
            Files = new List<PathFile>()
        };

        if (pathInfo.Accessed)
        {
            var files = Directory.GetFiles(folderPath).ToDictionary(file => file, File.GetLastWriteTime);
            foreach (var file in files)
            {
                pathInfo.Files.Add(new PathFile() { Name = file.Key, LastModified = file.Value, Accessed = true, Status = 0, Version = 0 });
            }

            var subFolders = Directory.GetDirectories(folderPath);

            foreach (var subFolder in subFolders)
            {
                pathInfo.Files.AddRange(GetFilesWithDetails(subFolder).Files);
            }
        }

        return pathInfo;
    }

    static bool HasReadAccess(string folderPath)
    {
        try
        {
            var directorySecurity = Directory.GetAccessControl(folderPath);

            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent();
            var accessRules = directorySecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount));

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if (rule.AccessControlType == AccessControlType.Allow &&
                    (rule.FileSystemRights & FileSystemRights.ReadData) == FileSystemRights.ReadData)
                {
                    return true;
                }
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    static (string, PathInfo) Compare(PathInfo previous, PathInfo current)
    {
        var text = string.Empty;
        var updated = current;

        if (previous == null)
        {
            text += $"New path, no changes." + Environment.NewLine;
        }
        else
        {
            var updatedPathInfo = GetAddModifyDelete(previous, current, text);
            updated = updatedPathInfo.Item2;
            text += updatedPathInfo.Item1;
        }

        if (text.Length == 0)
        {
            text += $"No changes." + Environment.NewLine;
        }

        return (text, updated);
    }

    private static (string, PathInfo) GetAddModifyDelete(PathInfo previous, PathInfo current, string text)
    {
        foreach (var file in current.Files)
        {
            if (file.Accessed)
            {
                var previousFile = previous?.Files.SingleOrDefault(x => x.Name == file.Name);

                if (previousFile != null)
                {
                    if (previousFile.LastModified != file.LastModified)
                    {
                        file.Status = FileStatus.Modified;
                        file.Version = ++previousFile.Version;
                        text += $"[M] {file.Name} version: {file.Version}" + Environment.NewLine;
                    }
                }
                else
                {
                    file.Status = FileStatus.Added;
                    text += $"[A] {file.Name}" + Environment.NewLine;
                }
            }
            else
            {
                text += $"[NA] {file.Name}" + Environment.NewLine;
            }
        }

        if (previous != null && previous.Files != null)
        {
            foreach (var previousFile in previous.Files)
            {
                if (current.Files.SingleOrDefault(x => x.Name == previousFile.Name) == null)
                {
                    previousFile.Status = FileStatus.Deleted;
                    current.Files.Add(previousFile);                    
                    text += $"[D] {previousFile.Name}" + Environment.NewLine;
                }
            }
        }

        return (text, current);
    }

    static PathInfo GetPreviousState(string filePath, string searchedPath)
    {
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            var paths = JsonSerializer.Deserialize<List<PathInfo>>(jsonData);
            return paths.SingleOrDefault(x => x.Name == searchedPath);
        }
        else
        {
            return null;
        }
    }

    static void SaveCurrentState(string filePath, PathInfo actualPathInfo)
    {
        if (File.Exists(filePath))
        {
            var jsonData = File.ReadAllText(filePath);
            var paths = JsonSerializer.Deserialize<List<PathInfo>>(jsonData);
            var previousPath = paths.SingleOrDefault(x => x.Name == actualPathInfo.Name);
            var versions = previousPath?.Files;

            if (previousPath == null)
            {
                paths.Add(actualPathInfo);
            }
            else
            {
                var index = paths.FindIndex(x => x.Name == actualPathInfo.Name);
                paths[index] = actualPathInfo;
            }

            string json = JsonSerializer.Serialize(paths, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
        else
        {
            string json = JsonSerializer.Serialize(new List<PathInfo> { actualPathInfo }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}