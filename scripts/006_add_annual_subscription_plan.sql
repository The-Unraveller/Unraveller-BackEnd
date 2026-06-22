-- =============================================================================
-- MIGRATION 006: Update pricing for Free and Premium plans, remove annual plan
-- Database: PostgreSQL (Supabase)
-- =============================================================================

-- 1. Cập nhật gói Premium tháng: giữ nguyên 199k và đồng bộ tính năng
UPDATE "SubscriptionPlans"
SET
    "Price" = 199000,
    "Name" = 'Premium VIP',
    "Description" = 'Gói hội viên Premium với toàn bộ tính năng VIP. Hủy bất kỳ lúc nào.',
    "Features" = ARRAY[
        'Năng lượng vô hạn — không giới hạn lượt học',
        'Gấp đôi XP trên mỗi cuộc hội thoại (2x)',
        'Mở khóa tất cả 15+ kịch bản độc quyền',
        'AI Coach phân tích chuyên sâu ngữ pháp',
        'Tắt toàn bộ quảng cáo',
        'Giảm 20% khi mua vật phẩm Cửa Hàng',
        'Hồi năng lượng nhanh gấp đôi',
        'Badge VIP + Khung avatar đặc biệt'
    ]
WHERE "Tier" = 1;

-- 2. Cập nhật tên gói Free để thân thiện hơn
UPDATE "SubscriptionPlans"
SET
    "Name" = 'Explorer',
    "Description" = 'Miễn phí mãi mãi. Bắt đầu hành trình học tiếng Anh không cần thẻ tín dụng.',
    "Features" = ARRAY[
        '5 kịch bản giao tiếp đầy đủ tính năng',
        '100 năng lượng mỗi ngày (tự hồi phục)',
        'AI Feedback sau mỗi lượt chat',
        'Bài luyện cú pháp Terminal Hack',
        'Bảng xếp hạng cộng đồng',
        'Huy chương & thành tích cơ bản'
    ]
WHERE "Tier" = 0;

-- 3. Đảm bảo xóa gói Annual (Tier = 2) nếu đã lỡ chèn
DELETE FROM "SubscriptionPlans" WHERE "Tier" = 2;

-- Verify
SELECT "Id", "Name", "Tier", "Price", "DurationDays"
FROM "SubscriptionPlans"
ORDER BY "Tier";
