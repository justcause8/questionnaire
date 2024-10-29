using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Anonymou
{
    public int Id { get; set; }

    public string SessionId { get; set; } = null!;

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
