namespace TheUnraveller.Core.Entities;

/// <summary>
/// Nhiệm vụ con trong một kịch bản (Mission).
/// VD: "Hỏi chỗ ngồi", "Hỏi mật khẩu WiFi" trong kịch bản Coffee Shop.
/// </summary>
public class MissionSubTask
{
    public int Id { get; set; }
    public int MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    /// <summary>Thứ tự hiển thị trong danh sách (1-indexed)</summary>
    public int OrderIndex { get; set; }

    /// <summary>Nhãn tiếng Việt. VD: "Hỏi chỗ ngồi"</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Nhãn tiếng Anh. VD: "Ask for a seat"</summary>
    public string LabelEn { get; set; } = string.Empty;

    /// <summary>Câu gợi ý tiếng Anh cho người chơi. VD: "Is there a seat available?"</summary>
    public string HintPhrase { get; set; } = string.Empty;

    /// <summary>
    /// Keywords để phát hiện subtask đã được thực hiện trong chat.
    /// AI sẽ so sánh message của player với list này.
    /// </summary>
    public List<string> TriggerKeywords { get; set; } = new();

    /// <summary>Nếu true, subtask này không bắt buộc để hoàn thành mission</summary>
    public bool IsOptional { get; set; } = false;

    /// <summary>XP thưởng thêm khi hoàn thành subtask này</summary>
    public int XpBonus { get; set; } = 10;
}

/// <summary>
/// Theo dõi trạng thái hoàn thành subtask của từng user trong từng mission.
/// </summary>
public class UserSubTaskProgress
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MissionId { get; set; }
    public Mission Mission { get; set; } = null!;

    public int SubTaskId { get; set; }
    public MissionSubTask SubTask { get; set; } = null!;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
