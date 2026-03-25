namespace StudyHub.ViewModels;

public class NoteViewModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string SubjectName { get; set; }
    public int Course { get; set; }
    public string AuthorId { get; set; }
    public string AuthorName { get; set; }
    public int LikesCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsLikedByUser { get; set; }
}