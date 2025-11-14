using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Elasticsearch search engine component manager for DevStack.
    /// Handles Elasticsearch installation and service management for full-text search and analytics.
    /// Elasticsearch runs as a single JVM process with internal thread pools.
    /// </summary>
    public class ElasticsearchComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "elasticsearch";
        
        /// <summary>
        /// Gets the display label for Elasticsearch.
        /// </summary>
        public override string Label => "Elasticsearch";
        
        /// <summary>
        /// Gets the Elasticsearch installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.elasticDir;
        
        /// <summary>
        /// Gets whether Elasticsearch runs as a Windows service.
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets the Elasticsearch service process pattern for monitoring.
        /// </summary>
        public override string? ServicePattern => "elasticsearch.exe";
        
        /// <summary>
        /// Gets the maximum number of Elasticsearch processes to track.
        /// Value is 1 because Elasticsearch runs as a single JVM process with internal thread management.
        /// </summary>
        public override int? MaxWorkers => 1;
    }
}
