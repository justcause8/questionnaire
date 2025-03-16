using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace questionnaire.Models;

public partial class QuestionnaireContext : DbContext
{
    public QuestionnaireContext()
    {
    }

    public QuestionnaireContext(DbContextOptions<QuestionnaireContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessLevel> AccessLevels { get; set; }

    public virtual DbSet<Anonymou> Anonymous { get; set; }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }
    public virtual DbSet<QuestionOption> Options { get; set; }

    public virtual DbSet<QuestionType> QuestionTypes { get; set; }

    public virtual DbSet<Questionnaire> Questionnaires { get; set; }

    public virtual DbSet<QuestionnaireHistory> QuestionnaireHistories { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<TypeQuestionnaire> TypeQuestionnaires { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=questionnaire;Trusted_Connection=true;TrustServerCertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessLevel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AccessLe__3213E83F5CA6A6C9");

            entity.ToTable("AccessLevel");

            entity.HasIndex(e => e.Id, "UQ__AccessLe__3213E83E880716CE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LevelName)
                .HasMaxLength(255)
                .HasColumnName("level_name");
        });

        modelBuilder.Entity<Anonymou>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Anonymou__3213E83F1978D4C1");

            entity.HasIndex(e => e.Id, "UQ__Anonymou__3213E83E1D1C793D").IsUnique();
            entity.HasIndex(e => e.SessionId, "UQ__Anonymou__69B13FDD8723FC70").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd(); // Автоматическая генерация значения для int
            entity.Property(e => e.SessionId)
                .HasColumnName("session_id")
                .HasColumnType("uniqueidentifier") // Тип данных для GUID
                .HasDefaultValueSql("NEWID()"); // Автоматическая генерация GUID

            // Связь с ответами
            entity.HasMany(d => d.Answers)
                .WithOne(p => p.Anonymous)
                .HasForeignKey(d => d.AnonymousId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk1");
        });

        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Answer__3213E83F3574F1FF");
            entity.ToTable("Answer");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnonymousId).HasColumnName("anonymous_ID").IsRequired(false);
            entity.Property(e => e.UserId).HasColumnName("user_ID").IsRequired(false);
            entity.Property(e => e.QuestionId).HasColumnName("question_ID");
            entity.Property(e => e.SelectOption).HasColumnName("select_option");
            entity.Property(e => e.Text).HasMaxLength(255).HasColumnName("text");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").HasColumnName("created_at");

            // Связь с анонимным пользователем
            entity.HasOne(d => d.Anonymous)
                .WithMany(p => p.Answers)
                .HasForeignKey(d => d.AnonymousId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk1");

            // Связь с авторизованным пользователем
            entity.HasOne(d => d.User)
                .WithMany(p => p.Answers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk3");

            // Связь с вопросом
            entity.HasOne(d => d.Question)
                .WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_fk2");

            // Связь с вариантом ответа
            entity.HasOne(d => d.QuestionOption)
                .WithMany(p => p.Answers)
                .HasForeignKey(d => d.SelectOption)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Answer_QuestionOption_fk");
        });

        // Настройка связи Question -> Questionnaire
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F0FCDB0B0");
            entity.ToTable("Question");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionTypeId).HasColumnName("question_type_ID");
            entity.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_ID");
            entity.Property(e => e.Text).HasMaxLength(255).HasColumnName("text");

            // Связь с типом вопроса
            entity.HasOne(d => d.QuestionType)
                .WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Question_fk1");

            // Связь с анкетой
            entity.HasOne(d => d.Questionnaire)
                .WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuestionnaireId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Question_fk2");
        });


        // Настройка связи Question -> QuestionOption
        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_QuestionOption");
            entity.ToTable("QuestionOption");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionId).HasColumnName("question_ID");
            entity.Property(e => e.OptionText).HasMaxLength(255).HasColumnName("option_text");
            entity.Property(e => e.Order).HasColumnName("order");

            // Связь с вопросом
            entity.HasOne(d => d.Question)
                .WithMany(p => p.Options)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("QuestionOption_fk1");
        });

        modelBuilder.Entity<QuestionType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83FDD8A0518");

            entity.ToTable("QuestionType");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E0D186B02").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NameQuestion)
                .HasMaxLength(255)
                .HasColumnName("name_question");
        });

        modelBuilder.Entity<Questionnaire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83FB95D3D32");

            entity.ToTable("Questionnaire");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E8ABF1C9E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TypeQuestionnaireId).HasColumnName("type_questionnaire_ID");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.TypeQuestionnaire).WithMany(p => p.Questionnaires)
                .HasForeignKey(d => d.TypeQuestionnaireId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Questionnaire_fk1");

            entity.HasOne(d => d.User).WithMany(p => p.Questionnaires)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Questionnaire_fk2");
        });

        modelBuilder.Entity<QuestionnaireHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Question__3213E83F39C16374");

            entity.ToTable("QuestionnaireHistory");

            entity.HasIndex(e => e.Id, "UQ__Question__3213E83E9914531B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.QuestionnaireId).HasColumnName("questionnaire_ID");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
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

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tokens__3213E83FF333D097");

            entity.HasIndex(e => e.Id, "UQ__Tokens__3213E83E708D96DB").IsUnique();

            entity.HasIndex(e => e.RefreshToken, "UQ__Tokens__7FB69BAD6E2F1F3A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(255)
                .HasColumnName("refresh_token");
            entity.Property(e => e.RefreshTokenDatetime)
                .HasColumnType("datetime")
                .HasColumnName("refresh_token_datetime");
            entity.Property(e => e.UserId).HasColumnName("user_ID");

            entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Tokens_fk1");
        });

        modelBuilder.Entity<TypeQuestionnaire>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TypeQues__3213E83FBDD485ED");

            entity.ToTable("TypeQuestionnaire");

            entity.HasIndex(e => e.Id, "UQ__TypeQues__3213E83E16F8D4FB").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(255)
                .HasColumnName("type");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3213E83F3809148A");

            entity.ToTable("User");

            entity.HasIndex(e => e.Id, "UQ__User__3213E83EB8CF6A85").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessLevelId).HasColumnName("access_level_ID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasOne(d => d.AccessLevel).WithMany(p => p.Users)
                .HasForeignKey(d => d.AccessLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_fk1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
