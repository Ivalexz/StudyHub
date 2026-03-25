namespace StudyHub.ViewModels;

public class HomeIndexViewModel
{
    public List<NoteViewModel> Notes { get; set; } = new();
    public List<TopAuthorViewModel> TopAuthors { get; set; } = new();
    public List<SubjectStatViewModel> SubjectStats { get; set; } = new();
    public List<SubjectViewModel> Subjects { get; set; } = new();
        
    public int TotalNotes { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
        
    // Filter parameters
    public int? SelectedSubjectId { get; set; }
    public int? SelectedCourse { get; set; }
    public string SortBy { get; set; } = "newest";
    public string SearchTerm { get; set; } = "";
        
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalPages { get; set; }
}