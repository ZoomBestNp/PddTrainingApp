using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class StudentAssignment
{
    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public int BlockId { get; set; }

    public DateTime? AssignedDate { get; set; }

    public DateTime? DueDate { get; set; }

    public bool? IsCompleted { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? Score { get; set; }

    public virtual QuestionBlock Block { get; set; } = null!;

    public virtual User Student { get; set; } = null!;
}
