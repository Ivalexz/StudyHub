using Microsoft.EntityFrameworkCore;
using StudyHub.Models;

namespace StudyHub.Data
{
    public class StudyHubContext : DbContext
    {
        public StudyHubContext(DbContextOptions<StudyHubContext> options) : base(options)
        {
        }

        public DbSet<Note> Notes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Subject)
                .WithMany()
                .HasForeignKey(n => n.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Like>()
                .HasOne(l => l.Note)
                .WithMany()
                .HasForeignKey(l => l.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.NoteId, l.UserId })
                .IsUnique();
        }
    }
}