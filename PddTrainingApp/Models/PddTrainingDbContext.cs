using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PddTrainingApp.Models;

public partial class PddTrainingDbContext : DbContext
{
    public PddTrainingDbContext()
    {
    }

    public PddTrainingDbContext(DbContextOptions<PddTrainingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Module> Modules { get; set; }
    public virtual DbSet<Option> Options { get; set; }
    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<QuestionBlock> QuestionBlocks { get; set; }
    public virtual DbSet<Result> Results { get; set; }
    public virtual DbSet<StudentAssignment> StudentAssignments { get; set; }
    public virtual DbSet<TeacherStudent> TeacherStudents { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-LH07RGB\\SQLEXPRESS;Initial Catalog=PddTrainingDb;Integrated Security=true;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.ModuleId).HasName("PK__Modules__2B74778754D5B2D7");

            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        // НОВАЯ таблица Options
        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Options__0ADB74A8A1B2C3D4");

            entity.Property(e => e.OptionId).HasColumnName("OptionID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.OptionText)
                .IsRequired()
                .HasMaxLength(500);
            entity.Property(e => e.OptionOrder)
                .IsRequired()
                .HasDefaultValue(1);

            // Связь с Questions
            entity.HasOne(d => d.Question)
                .WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__Options__Questio__6EF57B66");

            // Уникальный индекс для предотвращения дублирования порядка
            entity.HasIndex(e => new { e.QuestionId, e.OptionOrder })
                .IsUnique()
                .HasDatabaseName("IX_Options_QuestionId_OptionOrder");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CE5882498");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(e => e.DifficultyLevel).HasDefaultValue(1);
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");

            // Связь с Module
            entity.HasOne(d => d.Module)
                .WithMany(p => p.Questions)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__Questions__Modul__5070F446");

            entity.HasOne(d => d.CorrectOption)
                .WithMany()
                .HasForeignKey(d => d.Answer)
                .OnDelete(DeleteBehavior.SetNull) 
                .IsRequired(false) 
                .HasConstraintName("FK__Questions__Answe__5165187F");

    
            entity.HasMany(q => q.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);


        });

        modelBuilder.Entity<QuestionBlock>(entity =>
        {
            entity.HasKey(e => e.BlockId).HasName("PK__Question__144215118F163542");

            entity.Property(e => e.BlockId).HasColumnName("BlockID");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.TeacherId).HasColumnName("TeacherID");

            entity.HasOne(d => d.Module)
                .WithMany(p => p.QuestionBlocks)
                .HasForeignKey(d => d.ModuleId)
                .HasConstraintName("FK__QuestionB__Modul__60A75C0F");

            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.QuestionBlocks)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QuestionB__Teach__5FB337D6");
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Results__97690228236C92DC");

            entity.Property(e => e.ResultId).HasColumnName("ResultID");
            entity.Property(e => e.Date).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Question)
                .WithMany(p => p.Results)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK__Results__Questio__5535A963");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Results)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Results__UserID__5441852A");
        });

        modelBuilder.Entity<StudentAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__StudentA__32499E570555534B");

            entity.Property(e => e.AssignmentId).HasColumnName("AssignmentID");
            entity.Property(e => e.AssignedDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.BlockId).HasColumnName("BlockID");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.StudentId).HasColumnName("StudentID");

            entity.HasOne(d => d.Block)
                .WithMany(p => p.StudentAssignments)
                .HasForeignKey(d => d.BlockId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAs__Block__66603565");

            entity.HasOne(d => d.Student)
                .WithMany(p => p.StudentAssignments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StudentAs__Stude__656C112C");
        });

        modelBuilder.Entity<TeacherStudent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TeacherS__3214EC07214E6D90");

            entity.HasIndex(e => new { e.TeacherId, e.StudentId }, "IX_TeacherStudents_Unique").IsUnique();

            entity.Property(e => e.LinkedDate).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Student)
                .WithMany(p => p.TeacherStudentStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeacherSt__Stude__6E01572D");

            entity.HasOne(d => d.Teacher)
                .WithMany(p => p.TeacherStudentTeachers)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TeacherSt__Teach__6D0D32F4");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACB9035131");

            entity.HasIndex(e => e.Login, "UQ__Users__5E55825BD102CCCA").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Login).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Student"); // По умолчанию Student, а не User
            entity.Property(e => e.StudentCode).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}