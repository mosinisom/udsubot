using Microsoft.EntityFrameworkCore;

public class DataContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public DataContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;Database=udsubot;User Id=postgres;Password=eder432;");
    }

    

    public DbSet<Institutes> Institutes { get; set; }
    public DbSet<Likes> Likes { get; set; }
    public DbSet<Messages> Messages { get; set; }
    public DbSet<Photos> Photos { get; set; }
    public DbSet<Students> Students { get; set; }
    public DbSet<Users> Users { get; set; }
    // onmodelcreating
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Institutes>(entity =>
        {
            entity.HasKey(e => e.Institute_ID)
                .HasName("institutes_pkey");

            entity.ToTable("institutes");

            entity.Property(e => e.Institute_ID)
                .HasColumnName("institute_id")
                .HasDefaultValueSql("nextval('institutes_institute_id_seq'::regclass)");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("institute_name");
        });

        modelBuilder.Entity<Likes>(entity =>
        {
            entity.HasKey(e => e.Like_ID)
                .HasName("likes_pkey");

            entity.ToTable("likes");

            entity.Property(e => e.Like_ID)
                .HasColumnName("like_id")
                .HasDefaultValueSql("nextval('likes_like_id_seq'::regclass)");

            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("like_date");

            entity.Property(e => e.FromStudent_ID).HasColumnName("from_student_id");
            entity.Property(e => e.ToStudent_ID).HasColumnName("to_student_id");

        });

        modelBuilder.Entity<Messages>(entity =>
        {
            entity.HasKey(e => e.Message_ID)
                .HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.Message_ID)
                .HasColumnName("message_id")
                .HasDefaultValueSql("nextval('messages_message_id_seq'::regclass)");

            entity.Property(e => e.Date)
                .HasColumnType("date")
                .HasColumnName("message_date");

            entity.Property(e => e.Text)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("message_text");

            entity.Property(e => e.FromStudent_ID).HasColumnName("from_student_id");
            entity.Property(e => e.ToStudent_ID).HasColumnName("to_student_id");

        });

        modelBuilder.Entity<Photos>(entity =>
        {
            entity.HasKey(e => e.Photo_ID)
                .HasName("photos_pkey");

            entity.ToTable("photos");

            entity.Property(e => e.Photo_ID)
                .HasColumnName("photo_id")
                .HasDefaultValueSql("nextval('photos_photo_id_seq'::regclass)");

            entity.Property(e => e.Path)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("photo_path");

            entity.Property(e => e.Student_ID).HasColumnName("student_id");

        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.HasKey(e => e.User_ID)
                .HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.User_ID)
                .HasColumnName("user_id")
                .HasDefaultValueSql("nextval('users_user_id_seq'::regclass)");

            entity.Property(e => e.Chat_ID).HasColumnName("chat_id");

            entity.Property(e => e.StudentCardNumber).HasColumnName("student_card_number");

            entity.Property(e => e.HasAccess).HasColumnName("has_access");

            entity.Property(e => e.RegistrationDate)
                .HasColumnType("date")
                .HasColumnName("registration_date");

        });

        modelBuilder.Entity<Students>(entity =>
        {
            entity.HasKey(e => e.Student_ID)
                .HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Student_ID)
                .HasColumnName("student_id")
                .HasDefaultValueSql("nextval('students_student_id_seq'::regclass)");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("student_name");

            entity.Property(e => e.Institute_ID).HasColumnName("institute_id");

            entity.Property(e => e.TelegramLink)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("student_tg_link");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("student_description");

            entity.Property(e => e.Year).HasColumnName("student_year");

            entity.Property(e => e.User_ID).HasColumnName("user_id");



        });
    }
}