namespace Melodee.Common.Models;

public enum OperationResponseType
{
    NotSet = 0,

    Unauthorized = 401,

    AccessDenied = 403,

    Error = 500,

    NotFound = 404,

    Ok = 200,

    ValidationFailure = 400,
    
    NotImplementedOrDisabled = 501
    
}
