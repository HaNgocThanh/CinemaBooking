-- =====================================================
-- SEED USERS + LINK SEED BOOKINGS TO A TEST USER
-- Gán CustomerId cho 10 bookings mẫu trong seed_data.sql
--
-- SCRIPT NÀY KHÔNG CẦN CHẠY NẾU BẠN ĐÃ CÓ USER TRONG DB
-- Chỉ cần gán CustomerId cho các booking mẫu:
--   UPDATE "Bookings" SET "CustomerId" = <YOUR_USER_ID> WHERE "CustomerId" IS NULL;
--
-- =====================================================
-- USER MẪU: testuser / Password123  (CustomerId = 2)
-- ADMIN MẪU: admin / Admin123        (CustomerId = 1)
-- =====================================================

-- Bước 1: Kiểm tra xem Users đã có chưa
SELECT "Id", "Username", "Email", "Role" FROM "Users" ORDER BY "Id";

-- Bước 2: Nếu chưa có Users, chạy phần dưới đây
-- (bỏ comment 2 dòng INSERT + COMMIT nếu cần tạo users)
--
-- INSERT INTO "Users" ("Id", "Username", "Email", "FullName", "PasswordHash", "PhoneNumber", "Role", "IsEmailConfirmed", "IsActive", "CreatedAt")
-- VALUES (1, N'admin', N'admin@cinema.com', N'Quản trị viên',
--          N'$2a$12$eTmNPV2OJpaZh66/sfiS/uGM8H/DELPDr.GT5N9qn/ILWRqt5AnU2',
--          N'0901234567', 1, 1, 1, SYSDATE);
--
-- INSERT INTO "Users" ("Id", "Username", "Email", "FullName", "PasswordHash", "PhoneNumber", "Role", "IsEmailConfirmed", "IsActive", "CreatedAt")
-- VALUES (2, N'testuser', N'testuser@cinema.com', N'Nguyễn Văn A',
--          N'$2a$12$eTmNPV2OJpaZh66/sfiS/uGM8H/DELPDr.GT5N9qn/ILWRqt5AnU2',
--          N'0909876543', 0, 1, 1, SYSDATE);
--
-- COMMIT;

-- Bước 3: Gán CustomerId=2 (testuser) cho TẤT CẢ bookings mẫu trong seed_data.sql
UPDATE "Bookings" SET "CustomerId" = 2 WHERE "CustomerId" IS NULL;
COMMIT;

-- Bước 4: Xác nhận kết quả
SELECT "Id", "BookingCode", "CustomerId", "Status", "TotalAmount", "BookedAt" FROM "Bookings" ORDER BY "Id";
