using System;
using System.Collections.Generic;

namespace PddTrainingApp.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public string? StudentCode { get; set; }

    public virtual ICollection<QuestionBlock> QuestionBlocks { get; set; } = new List<QuestionBlock>();

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual ICollection<StudentAssignment> StudentAssignments { get; set; } = new List<StudentAssignment>();

    public virtual ICollection<TeacherStudent> TeacherStudentStudents { get; set; } = new List<TeacherStudent>();

    public virtual ICollection<TeacherStudent> TeacherStudentTeachers { get; set; } = new List<TeacherStudent>();
}
