namespace DerotMyBrain.Core.DTOs;

public class QuizDto
{
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public int? CorrectOptionIndex { get; set; }
    public string? CorrectAnswer { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public string Type { get; set; } = "MCQ";
}
