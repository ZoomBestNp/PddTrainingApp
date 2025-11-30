using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class QuestionBlock
{
    public int BlockId { get; set; }

    public int TeacherId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ModuleId { get; set; }

    public int? DifficultyLevel { get; set; }

    public int QuestionsCount { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Module? Module { get; set; }

    public virtual ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();

    public virtual User Teacher { get; set; } = null!;
}
