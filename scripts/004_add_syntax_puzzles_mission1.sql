-- =============================================================================
-- MIGRATION 004: Add SyntaxPuzzles data for Mission 1 (Coffee Shop)
-- Database: PostgreSQL (Supabase)
-- Purpose: Fix "Luyện cú pháp" button that silently fails because SyntaxPuzzlesJson = '[]'
-- =============================================================================

UPDATE "Missions"
SET "SyntaxPuzzlesJson" = '[
  {
    "question": "Sắp xếp câu sau để gọi món cà phê lịch sự:",
    "scrambled": ["a", "like", "I", "latte", "please", "would", "hot"],
    "answer": "I would like a hot latte please",
    "hint": "Dùng ''would like'' thay vì ''want'' để nghe lịch sự hơn."
  },
  {
    "question": "Hỏi về menu một cách lịch sự:",
    "scrambled": ["the", "I", "please", "see", "menu", "Could"],
    "answer": "Could I see the menu please",
    "hint": "''Could I...'' là cách hỏi xin phép lịch sự hơn ''Can I...''."
  },
  {
    "question": "Hỏi gợi ý từ nhân viên:",
    "scrambled": ["recommend", "you", "today", "any", "Do", "blends", "house"],
    "answer": "Do you recommend any house blends today",
    "hint": "''Do you recommend...'' là cách hỏi ý kiến tự nhiên và lịch sự."
  },
  {
    "question": "Hỏi mật khẩu WiFi:",
    "scrambled": ["the", "Could", "please", "WiFi", "I", "get", "password"],
    "answer": "Could I get the WiFi password please",
    "hint": "Dùng ''Could I get...'' khi muốn xin thứ gì đó từ người khác."
  },
  {
    "question": "Yêu cầu tính tiền:",
    "scrambled": ["the", "get", "I", "bill", "Could", "please"],
    "answer": "Could I get the bill please",
    "hint": "''Could I get the bill?'' hoặc ''Check, please!'' đều được dùng phổ biến."
  }
]'
WHERE "Id" = 1;

-- Verify
SELECT "Id", "Title", LEFT("SyntaxPuzzlesJson", 80) AS "PuzzlePreview"
FROM "Missions"
WHERE "Id" = 1;
