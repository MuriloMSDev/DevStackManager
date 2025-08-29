using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public interface ComponentInterface
    {
        Task Install(string? version = null);
        void Uninstall(string? version = null);
        List<string> ListInstalled();
        List<string> ListAvailable();
        string Name { get; }
        string ToolDir { get; }
        string GetLatestVersion();
        bool IsService { get; }
        bool IsExecutable { get; }
        bool IsCommandLine { get; }
        string? ExecutableFolder { get; }
        string? ExecutablePattern { get; }
    }
}
