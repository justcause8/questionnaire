using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Token
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string RefreshToken { get; set; } = null!;

    public DateTime RefreshTokenDatetime { get; set; }

    public virtual User User { get; set; } = null!;
}
