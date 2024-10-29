using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Design
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string BackgroundColor { get; set; } = null!;

    public string SecondaryColor { get; set; } = null!;

    public string Font { get; set; } = null!;

    public virtual ICollection<Questionnaire> Questionnaires { get; set; } = new List<Questionnaire>();
}
