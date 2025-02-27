using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class AccessLevel
{
    public int Id { get; set; }

    public string LevelName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
