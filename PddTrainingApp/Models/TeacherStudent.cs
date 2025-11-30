using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class TeacherStudent
{
    public int Id { get; set; }

    public int TeacherId { get; set; }

    public int StudentId { get; set; }

    public DateTime? LinkedDate { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
