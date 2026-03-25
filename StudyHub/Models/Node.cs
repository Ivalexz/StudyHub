using System.ComponentModel.DataAnnotations;

namespace StudyHub.Models
{
    public enum NoteStatus
    {
        Pending,   
        Approved,  
        Rejected   
    }

    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введіть назву конспекту")]
        [StringLength(100)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Текст конспекту не може бути порожнім")]
        public string Content { get; set; }

        [Required]
        [Range(1, 4, ErrorMessage = "Курс має бути від 1 до 4")]
        public int Course { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        [Required]
        public string AuthorId { get; set; }

        public string AuthorName { get; set; }

        public NoteStatus Status { get; set; } = NoteStatus.Pending;

        public int LikesCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      
    }
}
