using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class QuestionType
{
    public int Id { get; set; }

    public string NameQuestion { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
