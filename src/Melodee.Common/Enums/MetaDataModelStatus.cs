namespace Melodee.Common.Enums;

public enum MetaDataModelStatus
{
    NotSet = 0,
    
    /// <summary>
    /// Ready for metadata update job to update metadata and find images, if necessary and not locked.
    /// </summary>
    ReadyToProcess,
    
    /// <summary>
    /// Has been processed by metadata update job.
    /// </summary>
    Processed,
    
    /// <summary>
    /// Added or updates images and should be manually reviewed.
    /// </summary>
    UpdatedImages
}
