using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Data;

/// <summary>
/// Database context cho Cinema Booking System.
/// Sử dụng Oracle Database với Entity Framework Core Code-First.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Movie> Movies { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<SeatTemplate> SeatTemplates { get; set; } = null!;
    public DbSet<Showtime> Showtimes { get; set; } = null!;
    public DbSet<ShowtimeSeat> ShowtimeSeats { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure bool columns to NUMBER(1) for Oracle compatibility
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.ClrType == typeof(bool))
                {
                    property.SetColumnType("NUMBER(1)");
                }
            }
        }

        ConfigureMovieEntity(modelBuilder);
        ConfigureRoomEntity(modelBuilder);
        ConfigureSeatTemplateEntity(modelBuilder);
        ConfigureShowtimeEntity(modelBuilder);
        ConfigureShowtimeSeatEntity(modelBuilder);
        ConfigureBookingEntity(modelBuilder);
        ConfigureTicketEntity(modelBuilder);
        ConfigureUserEntity(modelBuilder);
        SeedData(modelBuilder);
    }

    /// <summary>
    /// Cấu hình Movie entity với Fluent API.
    /// </summary>
    private static void ConfigureMovieEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            // Table configuration
            entity.ToTable("Movies");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Title)
                .HasColumnName("Title")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnName("Description")
                .HasMaxLength(1000);

            entity.Property(e => e.Director)
                .HasColumnName("Director")
                .HasMaxLength(100);

            entity.Property(e => e.Cast)
                .HasColumnName("Cast")
                .HasMaxLength(500);

            entity.Property(e => e.Genre)
                .HasColumnName("Genre")
                .HasMaxLength(100);

            entity.Property(e => e.DurationMinutes)
                .HasColumnName("DurationMinutes");

            entity.Property(e => e.Language)
                .HasColumnName("Language")
                .HasMaxLength(50);

            entity.Property(e => e.RatingCode)
                .HasColumnName("RatingCode")
                .HasMaxLength(10);

            entity.Property(e => e.PosterUrl)
                .HasColumnName("PosterUrl");

            entity.Property(e => e.TrailerUrl)
                .HasColumnName("TrailerUrl");

            entity.Property(e => e.BannerUrl)
                .HasColumnName("BannerUrl");

            entity.Property(e => e.IsFeatured)
                .HasColumnName("IsFeatured")
                .HasDefaultValue(false);

            entity.Property(e => e.ReleaseDate)
                .HasColumnName("ReleaseDate")
                .HasColumnType("DATE");

            entity.Property(e => e.EndDate)
                .HasColumnName("EndDate")
                .HasColumnType("DATE");

            entity.Property(e => e.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasConversion(
                    v => (int)v,
                    v => (MovieStatus)v)
                .HasDefaultValue(MovieStatus.ComingSoon);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            // Relationships
            entity.HasMany(e => e.Showtimes)
                .WithOne(s => s.Movie)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Cấu hình Room entity với Fluent API.
    /// </summary>
    private static void ConfigureRoomEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            // Table configuration
            entity.ToTable("Rooms");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .HasColumnName("Name")
                .HasColumnType("VARCHAR2(50)")
                .IsRequired();

            entity.Property(e => e.Capacity)
                .HasColumnName("Capacity")
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("Type")
                .HasColumnType("VARCHAR2(50)")
                .IsRequired();

            // Relationships
            entity.HasMany(e => e.SeatTemplates)
                .WithOne(s => s.Room)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Showtimes)
                .WithOne(s => s.Room)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Cấu hình SeatTemplate entity với Fluent API.
    /// </summary>
    private static void ConfigureSeatTemplateEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SeatTemplate>(entity =>
        {
            entity.ToTable("SeatTemplates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RoomId)
                .HasColumnName("RoomId")
                .IsRequired();

            entity.Property(e => e.Row)
                .HasColumnName("Row")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(e => e.Number)
                .HasColumnName("Number")
                .IsRequired();

            entity.Property(e => e.Type)
                .HasColumnName("Type")
                .HasConversion(
                    v => (int)v,
                    v => (SeatType)v)
                .IsRequired();

            entity.Property(e => e.DisplayOrder)
                .HasColumnName("DisplayOrder")
                .IsRequired();

            // Relationships
            entity.HasOne(e => e.Room)
                .WithMany(r => r.SeatTemplates)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.RoomId)
                .HasDatabaseName("IX_SeatTemplate_RoomId");

            entity.HasIndex(e => new { e.RoomId, e.Row, e.Number })
                .IsUnique()
                .HasDatabaseName("UX_SeatTemplate_RoomId_Row_Number");
        });
    }

    /// <summary>
    /// Cấu hình Showtime entity với Fluent API.
    /// </summary>
    private static void ConfigureShowtimeEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Showtime>(entity =>
        {
            // Table configuration
            entity.ToTable("Showtimes");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.MovieId)
                .HasColumnName("MovieId")
                .IsRequired();

            entity.Property(e => e.RoomId)
                .HasColumnName("RoomId")
                .IsRequired();

            entity.Property(e => e.StartTime)
                .HasColumnName("StartTime")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.EndTime)
                .HasColumnName("EndTime")
                .HasColumnType("DATE")
                .IsRequired();

            entity.Property(e => e.BasePrice)
                .HasColumnName("BasePrice")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.TotalSeats)
                .HasColumnName("TotalSeats")
                .IsRequired();

            entity.Property(e => e.BookedSeatsCount)
                .HasColumnName("BookedSeatsCount")
                .HasDefaultValue(0);

            entity.Property(e => e.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            // Foreign key constraint
            entity.HasOne(e => e.Movie)
                .WithMany(m => m.Showtimes)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Showtimes)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationships
            entity.HasMany(e => e.Seats)
                .WithOne(s => s.Showtime)
                .HasForeignKey(s => s.ShowtimeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.Showtime)
                .HasForeignKey(b => b.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.MovieId)
                .HasDatabaseName("IX_Showtime_MovieId");

            entity.HasIndex(e => new { e.StartTime, e.IsActive })
                .HasDatabaseName("IX_Showtime_StartTime_IsActive");
        });
    }

    /// <summary>
    /// Cấu hình ShowtimeSeat entity với Fluent API.
    /// KẾ TRỌNG: Hỗ trợ pessimistic locking (SELECT...FOR UPDATE NOWAIT).
    /// </summary>
    private static void ConfigureShowtimeSeatEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShowtimeSeat>(entity =>
        {
            // Table configuration
            entity.ToTable("ShowtimeSeats");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.ShowtimeId)
                .HasColumnName("ShowtimeId")
                .IsRequired();

            entity.Property(e => e.SeatNumber)
                .HasColumnName("SeatNumber")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.RowLetter)
                .HasColumnName("RowLetter")
                .HasMaxLength(5)
                .IsRequired();

            entity.Property(e => e.ColumnNumber)
                .HasColumnName("ColumnNumber")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasConversion(
                    v => (int)v,
                    v => (SeatStatus)v)
                .HasDefaultValue(SeatStatus.Available);

            entity.Property(e => e.Type)
                .HasColumnName("Type")
                .HasConversion(
                    v => (int)v,
                    v => (SeatType)v)
                .IsRequired();

            // Pessimistic locking fields (CRITICAL)
            entity.Property(e => e.LockedAt)
                .HasColumnName("LockedAt")
                .HasColumnType("DATE")
                .IsRequired(false);

            entity.Property(e => e.LockedBySessionId)
                .HasColumnName("LockedBySessionId")
                .HasMaxLength(100);

            entity.Property(e => e.BookedByUserId)
                .HasColumnName("BookedByUserId");

            entity.Property(e => e.TicketId)
                .HasColumnName("TicketId");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            // Foreign key constraints
            entity.HasOne(e => e.Showtime)
                .WithMany(s => s.Seats)
                .HasForeignKey(e => e.ShowtimeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Ticket)
                .WithOne(t => t.ShowtimeSeat)
                .HasForeignKey<ShowtimeSeat>(e => e.TicketId)
                .OnDelete(DeleteBehavior.SetNull);

            // INDEXES (Critical for performance & locking cleanup)
            // 1. Find seats by showtime + status (frequent query)
            entity.HasIndex(e => new { e.ShowtimeId, e.Status })
                .HasDatabaseName("IX_ShowtimeSeat_ShowtimeId_Status");

            // 2. Find expired locks (cleanup job runs every 5 minutes)
            entity.HasIndex(e => e.LockedAt)
                .HasDatabaseName("IX_ShowtimeSeat_LockedAt");

            // 3. Unique: one seat per showtime
            entity.HasIndex(e => new { e.ShowtimeId, e.SeatNumber })
                .IsUnique()
                .HasDatabaseName("UX_ShowtimeSeat_ShowtimeId_SeatNumber");

            // 4. Find seats by session ID (for per-user booking)
            entity.HasIndex(e => e.LockedBySessionId)
                .HasDatabaseName("IX_ShowtimeSeat_LockedBySessionId");
        });
    }

    /// <summary>
    /// Cấu hình Booking entity với Fluent API.
    /// </summary>
    private static void ConfigureBookingEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            // Table configuration
            entity.ToTable("Bookings");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.BookingCode)
                .HasColumnName("BookingCode")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.ShowtimeId)
                .HasColumnName("ShowtimeId")
                .IsRequired();

            entity.Property(e => e.CustomerId)
                .HasColumnName("CustomerId");

            entity.Property(e => e.Status)
                .HasColumnName("Status")
                .HasConversion(
                    v => (int)v,
                    v => (BookingStatus)v)
                .HasDefaultValue(BookingStatus.PendingPayment);

            entity.Property(e => e.TotalTickets)
                .HasColumnName("TotalTickets")
                .IsRequired();

            entity.Property(e => e.SubTotal)
                .HasColumnName("SubTotal")
                .HasPrecision(15, 2)
                .IsRequired();

            entity.Property(e => e.DiscountAmount)
                .HasColumnName("DiscountAmount")
                .HasPrecision(15, 2)
                .HasDefaultValue(0);

            entity.Property(e => e.TotalAmount)
                .HasColumnName("TotalAmount")
                .HasPrecision(15, 2)
                .IsRequired();

            entity.Property(e => e.PromotionCode)
                .HasColumnName("PromotionCode")
                .HasMaxLength(50);

            entity.Property(e => e.SessionId)
                .HasColumnName("SessionId")
                .HasMaxLength(100);

            entity.Property(e => e.BookedAt)
                .HasColumnName("BookedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.ExpiresAt)
                .HasColumnName("ExpiresAt")
                .HasColumnType("DATE");

            entity.Property(e => e.PaidAt)
                .HasColumnName("PaidAt")
                .HasColumnType("DATE");

            entity.Property(e => e.PaymentMethod)
                .HasColumnName("PaymentMethod")
                .HasMaxLength(20);

            entity.Property(e => e.TransactionId)
                .HasColumnName("TransactionId")
                .HasMaxLength(100);

            entity.Property(e => e.Notes)
                .HasColumnName("Notes")
                .HasMaxLength(500);

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            // Unique constraint on BookingCode
            entity.HasIndex(e => e.BookingCode)
                .IsUnique()
                .HasDatabaseName("UX_Booking_BookingCode");

            // Foreign key constraints
            entity.HasOne(e => e.Showtime)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ShowtimeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Customer)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Relationships
            entity.HasMany(e => e.Tickets)
                .WithOne(t => t.Booking)
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.ShowtimeId)
                .HasDatabaseName("IX_Booking_ShowtimeId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Booking_Status");

            entity.HasIndex(e => new { e.ShowtimeId, e.Status })
                .HasDatabaseName("IX_Booking_ShowtimeId_Status");

            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_Booking_ExpiresAt");
        });
    }

    /// <summary>
    /// Cấu hình Ticket entity với Fluent API.
    /// </summary>
    private static void ConfigureTicketEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            // Table configuration
            entity.ToTable("Tickets");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.TicketCode)
                .HasColumnName("TicketCode")
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(e => e.BookingId)
                .HasColumnName("BookingId")
                .IsRequired();

            entity.Property(e => e.ShowtimeSeatId)
                .HasColumnName("ShowtimeSeatId")
                .IsRequired();

            entity.Property(e => e.Price)
                .HasColumnName("Price")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(e => e.SeatType)
                .HasColumnName("SeatType")
                .HasMaxLength(20)
                .HasDefaultValue("STANDARD");

            entity.Property(e => e.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.IssuedAt)
                .HasColumnName("IssuedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UsedAt)
                .HasColumnName("UsedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.PrintCount)
                .HasColumnName("PrintCount")
                .HasDefaultValue(0);

            entity.Property(e => e.LastPrintedAt)
                .HasColumnName("LastPrintedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.Notes)
                .HasColumnName("Notes")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            // Unique constraint on TicketCode
            entity.HasIndex(e => e.TicketCode)
                .IsUnique()
                .HasDatabaseName("UX_Ticket_TicketCode");

            // Foreign key constraints
            entity.HasOne(e => e.Booking)
                .WithMany(b => b.Tickets)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ShowtimeSeat)
                .WithOne(s => s.Ticket)
                .HasForeignKey<Ticket>(e => e.ShowtimeSeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.BookingId)
                .HasDatabaseName("IX_Ticket_BookingId");

            entity.HasIndex(e => e.ShowtimeSeatId)
                .HasDatabaseName("IX_Ticket_ShowtimeSeatId");

            entity.HasIndex(e => e.UsedAt)
                .HasDatabaseName("IX_Ticket_UsedAt");
        });
    }

    /// <summary>
    /// Cấu hình User entity với Fluent API.
    /// </summary>
    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Table configuration
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            // Column configuration
            entity.Property(e => e.Id)
                .HasColumnName("Id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Username)
                .HasColumnName("Username")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("Email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.FullName)
                .HasColumnName("FullName")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("PasswordHash")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PhoneNumber)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20);

            entity.Property(e => e.Role)
                .HasColumnName("Role")
                .HasConversion(
                    v => (int)v,
                    v => (UserRole)v)
                .HasDefaultValue(UserRole.Customer);

            entity.Property(e => e.IsEmailConfirmed)
                .HasColumnName("IsEmailConfirmed")
                .HasDefaultValue(false);

            entity.Property(e => e.IsActive)
                .HasColumnName("IsActive")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("CreatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("UpdatedAt")
                .HasColumnType("DATE");

            entity.Property(e => e.LastLogin)
                .HasColumnName("LastLogin")
                .HasColumnType("DATE");

            // Unique constraints
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("UX_User_Username");

            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("UX_User_Email");

            // Relationships
            entity.HasMany(e => e.Bookings)
                .WithOne(b => b.Customer)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_User_IsActive");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_User_CreatedAt");
        });
    }

    /// <summary>
    /// Seed data for Room and SeatTemplate entities.
    /// Creates 2 rooms with 20 seat templates each (rows A-D, 5 seats per row).
    /// </summary>
    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Rooms
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, Name = "Phòng 01", Capacity = 20, Type = "2D" },
            new Room { Id = 2, Name = "Phòng 02", Capacity = 20, Type = "3D" }
        );

        // Seed SeatTemplates for Room 1 (Phòng 01)
        var room1Seats = new List<SeatTemplate>();
        var rows = new[] { "A", "B", "C", "D" };
        var seatTypePerRow = new Dictionary<string, SeatType>
        {
            { "A", SeatType.Regular },
            { "B", SeatType.Regular },
            { "C", SeatType.VIP },
            { "D", SeatType.VIP }
        };

        int seatId = 1;
        for (int r = 0; r < rows.Length; r++)
        {
            for (int n = 1; n <= 5; n++)
            {
                room1Seats.Add(new SeatTemplate
                {
                    Id = seatId,
                    RoomId = 1,
                    Row = rows[r],
                    Number = n,
                    Type = seatTypePerRow[rows[r]],
                    DisplayOrder = seatId
                });
                seatId++;
            }
        }

        // Seed SeatTemplates for Room 2 (Phòng 02)
        for (int r = 0; r < rows.Length; r++)
        {
            for (int n = 1; n <= 5; n++)
            {
                room1Seats.Add(new SeatTemplate
                {
                    Id = seatId,
                    RoomId = 2,
                    Row = rows[r],
                    Number = n,
                    Type = seatTypePerRow[rows[r]],
                    DisplayOrder = seatId - 20
                });
                seatId++;
            }
        }

        modelBuilder.Entity<SeatTemplate>().HasData(room1Seats);
    }
}
