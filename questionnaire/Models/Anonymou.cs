using System;
using System.Collections.Generic;

namespace questionnaire.Models;

public partial class Anonymou
{
    public int Id { get; set; } // Первичный ключ для связей

    public Guid SessionId { get; set; } = Guid.NewGuid(); // GUID для отслеживания анонимных пользователей

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}
