namespace Melodee.Common.Enums;

public enum MetaDataModelStatus
{
    NotSet = 0,
    
    /// <summary>
    /// Ready for metadata update job to update metadata and find images, if necessary and not locked.
    /// </summary>
    ReadyToProcess = 1,
    
    /// <summary>
    /// Has been processed by metadata update job.
    /// </summary>
    Processed = 2
}
