using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class UserAccessLevel
{
    public int UserId { get; set; }

    public int AccessLevel { get; set; }

    public virtual AccessLevel AccessLevelNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
