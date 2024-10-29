using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace questionnaire.Models;

public partial class questionnaireContext : DbContext
{
    public questionnaireContext()
    {
    }

    public questionnaireContext(DbContextOptions<questionnaireContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessLevel> AccessLevels { get; set; }

    public virtual DbSet<Anonymou> Anonymous { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Design> Designs { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Questionnaire> Questionnaires { get; set; }

    public virtual DbSet<QuestionnaireHistory> QuestionnaireHistories { get; set; }

    public virtual DbSet<TypeQuestionnaire> TypeQuestionnaires { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAccessLevel> UserAccessLevels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-RF1JFC10;Database=questionnaire;Trusted_Connection=true;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccessLe__3213E83FBB4E83A1");

            entity.ToTable("AccessLevel");

            entity.HasIndex(e => e.Id, "UQ__AccessLe__3213E83E282B51CA").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LevelName).HasColumnName("level_name");
        });

        modelBuilder.Entity<Anonymou>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Anonymou__3213E83F3287D0B9");

            entity.HasIndex(e => e.Id, "UQ__Anonymou__3213E83ECD5BFF0D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answer__3213E83F91F1849F");

            entity.ToTable("Answer");

            entity.HasIndex(e => e.Id, "UQ__Answer__3213E83E765749DC").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnonymousId).HasColumnName("anonymous_ID");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.QuestionId).HasColumnName("question_ID");
            entity.Property(e => e.SelectOption).HasColumnName("select_option");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Anonymous).WithMany(p => p.Answers)
                .HasForeignKey(d => d.AnonymousId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk1");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk2");
        });

        modelBuilder.Entity<Design>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Design__3213E83FCBDE32D8");

            entity.ToTable("Design");

            entity.HasIndex(e => e.Id, "UQ__Design__3213E83E3A3645AE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BackgroundColor).HasColumnName("background_color");
            entity.Property(e => e.Font).HasColumnName("font");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.SecondaryColor).HasColumnName("secondary_color");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83FA0DED0A6");

            entity.ToTable("Question");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E87D02590").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionTypeId).HasColumnName("question_type_ID");
            entity.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_ID");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.QuestionType).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Question_fk1");

            entity.HasOne(d => d.Questionnaire).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionnaireId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Question_fk2");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F6A78B633");

            entity.ToTable("QuestionType");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83EF3AFDEB6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NameQuestion).HasColumnName("name_question");
        });

        modelBuilder.Entity<Questionnaire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F48306DCE");

            entity.ToTable("Questionnaire");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E47071616").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DesignId).HasColumnName("design_ID");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.TypeQuestionnaireId).HasColumnName("type_questionnaire_ID");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.Design).WithMany(p => p.Questionnaires)
                .HasForeignKey(d => d.DesignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Questionnaire_fk2");

            entity.HasOne(d => d.TypeQuestionnaire).WithMany(p => p.Questionnaires)
                .HasForeignKey(d => d.TypeQuestionnaireId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Questionnaire_fk1");

            entity.HasOne(d => d.User).WithMany(p => p.Questionnaires)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Questionnaire_fk3");
        });

        modelBuilder.Entity<QuestionnaireHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F78A4218E");

            entity.ToTable("QuestionnaireHistory");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E19F57291").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_ID");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.Questionnaire).WithMany(p => p.QuestionnaireHistories)
                .HasForeignKey(d => d.QuestionnaireId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("QuestionnaireHistory_fk1");

            entity.HasOne(d => d.User).WithMany(p => p.QuestionnaireHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("QuestionnaireHistory_fk2");
        });

        modelBuilder.Entity<TypeQuestionnaire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TypeQues__3213E83FB78EC8E9");

            entity.ToTable("TypeQuestionnaire");

            entity.HasIndex(e => e.Id, "UQ__TypeQues__3213E83E6829479B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type).HasColumnName("type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83F8855185B");

            entity.ToTable("User");

            entity.HasIndex(e => e.Id, "UQ__User__3213E83E7D059DE3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<UserAccessLevel>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("UserAccessLevel");

            entity.Property(e => e.AccessLevel).HasColumnName("access_level");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AccessLevelNavigation).WithMany()
                .HasForeignKey(d => d.AccessLevel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserAccessLevel_fk1");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserAccessLevel_fk0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
