using System.ComponentModel.DataAnnotations.Schema;
using Melodee.Common.Enums;
using Melodee.Common.Utility;

namespace Melodee.Common.Data.Models;

public class ClientApplication : DataModelBase
{
    public int ClientApplicationStatus { get; set; }

    [NotMapped] public ClientApplicationStatus ClientApplicationStatusValue => SafeParser.ToEnum<ClientApplicationStatus>(ClientApplicationStatus);
}
