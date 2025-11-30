using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class Module
{
    public int ModuleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<QuestionBlock> QuestionBlocks { get; set; } = new List<QuestionBlock>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
