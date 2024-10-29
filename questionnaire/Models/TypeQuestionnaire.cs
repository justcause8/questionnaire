using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class TypeQuestionnaire
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public virtual ICollection<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();
}
