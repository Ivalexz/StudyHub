using System.ComponentModel.DataAnnotations;
using StudyHub.Models;

public class Like
{
    public int Id { get; set; }

    public int NoteId { get; set; }
    public Note Note { get; set; }

    [Required]
    public string UserId { get; set; } 
}