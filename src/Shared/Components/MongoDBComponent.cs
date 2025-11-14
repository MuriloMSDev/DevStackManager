using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// MongoDB NoSQL database component manager for DevStack.
    /// Handles MongoDB installation and service management for document-oriented database operations.
    /// MongoDB runs as a single process with internal thread management.
    /// </summary>
    public class MongoDBComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "mongodb";
        
        /// <summary>
        /// Gets the display label for MongoDB.
        /// </summary>
        public override string Label => "MongoDB";
        
        /// <summary>
        /// Gets the MongoDB installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.mongoDir;
        
        /// <summary>
        /// Gets whether MongoDB runs as a Windows service.
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets the MongoDB service process pattern for monitoring.
        /// </summary>
        public override string? ServicePattern => "mongod.exe";
        
        /// <summary>
        /// Gets the maximum number of MongoDB processes to track.
        /// Value is 1 because MongoDB runs as a single process with internal threading.
        /// </summary>
        public override int? MaxWorkers => 1;
    }
}
