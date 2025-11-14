using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Nginx web server component manager for DevStack.
    /// Handles Nginx installation and service management for serving web applications
    /// and acting as reverse proxy for PHP FastCGI and other backend services.
    /// </summary>
    public class NginxComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "nginx";
        
        /// <summary>
        /// Gets the display label for Nginx.
        /// </summary>
        public override string Label => "Nginx";
        
        /// <summary>
        /// Gets the Nginx installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.nginxDir;
        
        /// <summary>
        /// Gets whether Nginx runs as a service.
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets the Nginx service executable pattern.
        /// </summary>
        public override string? ServicePattern => "nginx.exe";
        
        /// <summary>
        /// Gets the maximum number of worker processes to track (1 for master process only).
        /// </summary>
        public override int? MaxWorkers => 1;
    }
}
