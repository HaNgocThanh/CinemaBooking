-- =====================================================
-- SAMPLE DATA FOR CINEMA BOOKING SYSTEM
-- =====================================================
-- Dữ liệu mẫu cho hệ thống đặt vé rạp chiếu phim
-- Không bao gồm dữ liệu User
-- =====================================================

-- =====================================================
-- 0. CLEANUP - XÓA DỮ LIỆU CŨ (DÙNG TRUNCATE)
-- =====================================================
-- TRUNCATE sẽ xóa dữ liệu và RESTART lại ID từ 1
BEGIN
    -- Disable constraints trước
    EXECUTE IMMEDIATE 'ALTER TABLE "Tickets" DISABLE CONSTRAINT "FK_Tickets_Bookings_BookingId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Tickets" DISABLE CONSTRAINT "FK_Tickets_ShowtimeSeats_ShowtimeSeatId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Bookings" DISABLE CONSTRAINT "FK_Bookings_Showtimes_ShowtimeId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "ShowtimeSeats" DISABLE CONSTRAINT "FK_ShowtimeSeats_Showtimes_ShowtimeId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Showtimes" DISABLE CONSTRAINT "FK_Showtimes_Movies_MovieId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Bookings" DISABLE CONSTRAINT "FK_Bookings_Users_CustomerId"';
    COMMIT;
EXCEPTION WHEN OTHERS THEN
    NULL;
END;
/

-- TRUNCATE xóa data và reset identity
BEGIN
    EXECUTE IMMEDIATE 'TRUNCATE TABLE "Tickets"';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE "Bookings"';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE "ShowtimeSeats"';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE "Showtimes"';
    EXECUTE IMMEDIATE 'TRUNCATE TABLE "Movies"';
    COMMIT;
EXCEPTION WHEN OTHERS THEN
    NULL;
END;
/

-- RESTART identity columns
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE "Movies" MODIFY ("Id" RESTART)';
    EXECUTE IMMEDIATE 'ALTER TABLE "Showtimes" MODIFY ("Id" RESTART)';
    EXECUTE IMMEDIATE 'ALTER TABLE "ShowtimeSeats" MODIFY ("Id" RESTART)';
    EXECUTE IMMEDIATE 'ALTER TABLE "Bookings" MODIFY ("Id" RESTART)';
    EXECUTE IMMEDIATE 'ALTER TABLE "Tickets" MODIFY ("Id" RESTART)';
    COMMIT;
EXCEPTION WHEN OTHERS THEN
    NULL;
END;
/

-- Re-enable constraints
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE "Showtimes" ENABLE CONSTRAINT "FK_Showtimes_Movies_MovieId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "ShowtimeSeats" ENABLE CONSTRAINT "FK_ShowtimeSeats_Showtimes_ShowtimeId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Bookings" ENABLE CONSTRAINT "FK_Bookings_Showtimes_ShowtimeId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Bookings" ENABLE CONSTRAINT "FK_Bookings_Users_CustomerId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Tickets" ENABLE CONSTRAINT "FK_Tickets_ShowtimeSeats_ShowtimeSeatId"';
    EXECUTE IMMEDIATE 'ALTER TABLE "Tickets" ENABLE CONSTRAINT "FK_Tickets_Bookings_BookingId"';
    COMMIT;
EXCEPTION WHEN OTHERS THEN
    NULL;
END;
/

-- =====================================================
-- 1. SAMPLE MOVIES (Dữ liệu mẫu phim)
-- =====================================================
BEGIN
INSERT INTO "Movies" ("Id", "Title", "Description", "Director", "Cast", "Genre", "DurationMinutes", "Language", "RatingCode", "PosterUrl", "TrailerUrl", "ReleaseDate", "EndDate", "IsActive", "CreatedAt")
VALUES 
(
    1,
    N'Dune: Part Two', 
    N'Tiếp nối câu chuyện về Paul Atreides khi anh trở thành một con người mạnh mẽ hơn. Paul phải bảo vệ gia đình mình trên hành tinh Arrakis trong khi theo đuổi mục tiêu phục hán những người làm tổn thương gia đình mình.',
    N'Denis Villeneuve',
    N'Timothée Chalamet, Zendaya, Austin Butler, Florence Pugh',
    N'Sci-Fi, Action, Adventure',
    166,
    N'English',
    N'PG-13',
    N'https://example.com/dune2.jpg',
    N'https://example.com/dune2-trailer.mp4',
    TO_DATE('2024-02-01', 'YYYY-MM-DD'),
    TO_DATE('2024-05-15', 'YYYY-MM-DD'),
    1,
    SYSDATE
);

INSERT INTO "Movies" ("Id", "Title", "Description", "Director", "Cast", "Genre", "DurationMinutes", "Language", "RatingCode", "PosterUrl", "TrailerUrl", "ReleaseDate", "EndDate", "IsActive", "CreatedAt")
VALUES 
(
    2,
    N'Oppenheimer',
    N'Một cuộc khảo sát toàn diện về cuộc sống của J. Robert Oppenheimer, giáo sư vật lý người Mỹ được giao nhiệm vụ lãnh đạo Dự án Manhattan trong Thế chiến thứ hai.',
    N'Christopher Nolan',
    N'Cillian Murphy, Robert Downey Jr., Emily Blunt, Matt Damon',
    N'Biography, Drama, History',
    180,
    N'English',
    N'R',
    N'https://example.com/oppenheimer.jpg',
    N'https://example.com/oppenheimer-trailer.mp4',
    TO_DATE('2023-07-21', 'YYYY-MM-DD'),
    TO_DATE('2023-10-15', 'YYYY-MM-DD'),
    1,
    SYSDATE
);

INSERT INTO "Movies" ("Id", "Title", "Description", "Director", "Cast", "Genre", "DurationMinutes", "Language", "RatingCode", "PosterUrl", "TrailerUrl", "ReleaseDate", "EndDate", "IsActive", "CreatedAt")
VALUES 
(
    3,
    N'Godzilla x Kong: The New Empire',
    N'Cộng hòa trong ngôi nhà của Kong, Godzilla bất ngờ nổi dậy từ sâu trong trái đất, kích hoạt các cuộc tấn công tàn phá. Mối quan hệ giữa Kong và nhân loại được thử thách khi hai titan thần kỳ xung đột.',
    N'Adam Wingard',
    N'Rebecca Hall, Brian Tyree Henry, Dan Stevens, Kaylee Hottle',
    N'Action, Adventure, Sci-Fi',
    145,
    N'English',
    N'PG-13',
    N'https://example.com/godzilla-kong.jpg',
    N'https://example.com/godzilla-kong-trailer.mp4',
    TO_DATE('2024-03-29', 'YYYY-MM-DD'),
    TO_DATE('2024-06-30', 'YYYY-MM-DD'),
    1,
    SYSDATE
);

INSERT INTO "Movies" ("Id", "Title", "Description", "Director", "Cast", "Genre", "DurationMinutes", "Language", "RatingCode", "PosterUrl", "TrailerUrl", "ReleaseDate", "EndDate", "IsActive", "CreatedAt")
VALUES 
(
    4,
    N'The Shawshank Redemption',
    N'Hai người đàn ông gặp nhau trong một nhà tù liên bang, lập thân với nhau trong một thời gian dài, tìm thấy sự cứu chuộc và hy vọng thông qua hành động.',
    N'Frank Darabont',
    N'Tim Robbins, Morgan Freeman',
    N'Drama, Crime',
    142,
    N'English',
    N'R',
    N'https://example.com/shawshank.jpg',
    N'https://example.com/shawshank-trailer.mp4',
    TO_DATE('1994-09-23', 'YYYY-MM-DD'),
    TO_DATE('1994-12-20', 'YYYY-MM-DD'),
    1,
    SYSDATE
);

INSERT INTO "Movies" ("Id", "Title", "Description", "Director", "Cast", "Genre", "DurationMinutes", "Language", "RatingCode", "PosterUrl", "TrailerUrl", "ReleaseDate", "EndDate", "IsActive", "CreatedAt")
VALUES 
(
    5,
    N'Everything Everywhere All at Once',
    N'Một phụ nữ bố mẹ châu Á, bị quấy rối do các khoản nợ, được thôi thúc bởi một vũ trụ luân hồi để kết hợp với các phiên bản thay thế của chính mình để ngăn chặn một kẻ phá hoại đa vũ trụ từ hủy diệt',
    N'Daniel Kwan, Daniel Scheinert',
    N'Michelle Yeoh, Ke Huy Quan, Stephanie Hsu, Evan Rachel Wood',
    N'Action, Adventure, Comedy',
    139,
    N'English, Mandarin',
    N'R',
    N'https://example.com/everything-everywhere.jpg',
    N'https://example.com/everything-everywhere-trailer.mp4',
    TO_DATE('2022-11-16', 'YYYY-MM-DD'),
    TO_DATE('2023-02-28', 'YYYY-MM-DD'),
    1,
    SYSDATE
);

COMMIT;
END;
/

-- =====================================================
-- COMMIT MOVIES DATA
-- =====================================================

-- =====================================================
-- 2. SAMPLE SHOWTIMES (Dữ liệu mẫu suất chiếu)
-- =====================================================
-- Suất chiếu cho Dune: Part Two (MovieId = 1)
INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (1, 1, N'101', TO_DATE('2024-04-23 10:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 12:46', 'YYYY-MM-DD HH24:MI'), 120000, 100, 25, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (2, 1, N'101', TO_DATE('2024-04-23 14:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 16:46', 'YYYY-MM-DD HH24:MI'), 120000, 100, 45, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (3, 1, N'102', TO_DATE('2024-04-23 19:30', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 22:16', 'YYYY-MM-DD HH24:MI'), 150000, 80, 60, 1, SYSDATE);

-- Suất chiếu cho Oppenheimer (MovieId = 2)
INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (4, 2, N'A1', TO_DATE('2024-04-23 11:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 13:00', 'YYYY-MM-DD HH24:MI'), 130000, 90, 30, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (5, 2, N'A1', TO_DATE('2024-04-23 16:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 18:00', 'YYYY-MM-DD HH24:MI'), 130000, 90, 50, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (6, 2, N'A2', TO_DATE('2024-04-23 20:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 22:00', 'YYYY-MM-DD HH24:MI'), 150000, 85, 70, 1, SYSDATE);

-- Suất chiếu cho Godzilla x Kong (MovieId = 3)
INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (7, 3, N'103', TO_DATE('2024-04-23 13:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 14:25', 'YYYY-MM-DD HH24:MI'), 125000, 120, 35, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (8, 3, N'103', TO_DATE('2024-04-23 15:30', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 16:55', 'YYYY-MM-DD HH24:MI'), 125000, 120, 85, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (9, 3, N'104', TO_DATE('2024-04-23 21:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 22:25', 'YYYY-MM-DD HH24:MI'), 140000, 100, 95, 1, SYSDATE);

-- Suất chiếu cho The Shawshank Redemption (MovieId = 4)
INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (10, 4, N'105', TO_DATE('2024-04-23 09:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 10:22', 'YYYY-MM-DD HH24:MI'), 100000, 90, 20, 1, SYSDATE);

INSERT INTO "Showtimes" ("Id", "MovieId", "RoomNumber", "StartTime", "EndTime", "BasePrice", "TotalSeats", "BookedSeatsCount", "IsActive", "CreatedAt")
VALUES (11, 4, N'105', TO_DATE('2024-04-23 17:00', 'YYYY-MM-DD HH24:MI'), TO_DATE('2024-04-23 18:22', 'YYYY-MM-DD HH24:MI'), 100000, 90, 40, 1, SYSDATE);

-- =====================================================
-- COMMIT SHOWTIMES DATA
-- =====================================================
COMMIT;

-- =====================================================
-- 3. SAMPLE SHOWTIME SEATS (Dữ liệu mẫu ghế trong suất chiếu)
-- =====================================================
-- Hàm tạo ghế cho suất chiếu 1 (Dune, 10:00, Room 101, 100 ghế)
-- Ghế được sắp xếp thành 10 hàng (A-J), mỗi hàng 10 ghế
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 10;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..v_total_rows LOOP
        v_row_letter := CHR(64 + row_idx); -- A, B, C...
        FOR col_idx IN 1..v_total_cols LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            -- Ngẫu nhiên gán trạng thái: 60% Available, 20% Locked, 15% Booked, 5% Unavailable
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.60 THEN 0 -- Available
                WHEN DBMS_RANDOM.VALUE < 0.80 THEN 1 -- Locked
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2 -- Booked
                ELSE 3 -- Unavailable
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (1, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 2 (Dune, 14:00, Room 101)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 10;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..v_total_rows LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..v_total_cols LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.55 THEN 0
                WHEN DBMS_RANDOM.VALUE < 0.75 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (2, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 3 (Dune, 19:30, Room 102)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 8;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..v_total_rows LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..v_total_cols LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.25 THEN 0 -- Fewer available seats (popular showtime)
                WHEN DBMS_RANDOM.VALUE < 0.45 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (3, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 4 (Oppenheimer, 11:00, Room A1)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 9;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..v_total_rows LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..v_total_cols LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.65 THEN 0
                WHEN DBMS_RANDOM.VALUE < 0.80 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (4, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 5 (Oppenheimer, 16:00, Room A1)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 9;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..v_total_rows LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..v_total_cols LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.44 THEN 0 -- Less available
                WHEN DBMS_RANDOM.VALUE < 0.70 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (5, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 6 (Oppenheimer, 20:00, Room A2)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
    v_total_rows NUMBER := 8;
    v_total_cols NUMBER := 10;
BEGIN
    FOR row_idx IN 1..8 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..11 LOOP
            v_col := col_idx;
            v_seat_number := v_row_letter || v_col;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.18 THEN 0 -- Very few available (prime time)
                WHEN DBMS_RANDOM.VALUE < 0.50 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (6, v_seat_number, v_row_letter, v_col, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 7 (Godzilla x Kong, 13:00, Room 103)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
BEGIN
    FOR row_idx IN 1..12 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..10 LOOP
            v_seat_number := v_row_letter || col_idx;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.70 THEN 0
                WHEN DBMS_RANDOM.VALUE < 0.80 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (7, v_seat_number, v_row_letter, col_idx, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 8 (Godzilla x Kong, 15:30, Room 103)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
BEGIN
    FOR row_idx IN 1..12 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..10 LOOP
            v_seat_number := v_row_letter || col_idx;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.29 THEN 0 -- Even fewer available
                WHEN DBMS_RANDOM.VALUE < 0.60 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (8, v_seat_number, v_row_letter, col_idx, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 9 (Godzilla x Kong, 21:00, Room 104)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
BEGIN
    FOR row_idx IN 1..10 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..10 LOOP
            v_seat_number := v_row_letter || col_idx;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.05 THEN 0 -- Almost fully booked
                WHEN DBMS_RANDOM.VALUE < 0.30 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (9, v_seat_number, v_row_letter, col_idx, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 10 (Shawshank, 09:00, Room 105)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
BEGIN
    FOR row_idx IN 1..9 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..10 LOOP
            v_seat_number := v_row_letter || col_idx;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.78 THEN 0 -- Mostly available (morning show)
                WHEN DBMS_RANDOM.VALUE < 0.85 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (10, v_seat_number, v_row_letter, col_idx, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- Ghế cho suất chiếu 11 (Shawshank, 17:00, Room 105)
DECLARE
    v_row_letter VARCHAR2(1);
    v_col NUMBER;
    v_status NUMBER;
    v_seat_number VARCHAR2(10);
BEGIN
    FOR row_idx IN 1..9 LOOP
        v_row_letter := CHR(64 + row_idx);
        FOR col_idx IN 1..10 LOOP
            v_seat_number := v_row_letter || col_idx;
            
            v_status := CASE
                WHEN DBMS_RANDOM.VALUE < 0.55 THEN 0
                WHEN DBMS_RANDOM.VALUE < 0.75 THEN 1
                WHEN DBMS_RANDOM.VALUE < 0.95 THEN 2
                ELSE 3
            END;
            
            INSERT INTO "ShowtimeSeats" ("ShowtimeId", "SeatNumber", "RowLetter", "ColumnNumber", "Status", "CreatedAt")
            VALUES (11, v_seat_number, v_row_letter, col_idx, v_status, SYSDATE);
        END LOOP;
    END LOOP;
END;
/

-- =====================================================
-- COMMIT SHOWTIME SEATS DATA
-- =====================================================
COMMIT;

-- =====================================================
-- 4. SAMPLE BOOKINGS (Dữ liệu mẫu đơn đặt vé)
-- =====================================================

-- Booking 1: Dune (Showtime 1), 3 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId", "Notes")
VALUES (N'BK202404231001001', 1, NULL, 1, 3, 360000, 0, 360000, N'session_001', SYSDATE - 1, SYSDATE - 1 + 5/1440, SYSDATE - 1, N'STRIPE', N'txn_stripe_001', N'Khách hàng VIP');

-- Booking 2: Dune (Showtime 2), 2 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404232001002', 2, NULL, 1, 2, 240000, 24000, 216000, N'session_002', SYSDATE - 2, SYSDATE - 2 + 5/1440, SYSDATE - 2, N'MOMO', N'txn_momo_001');

-- Booking 3: Dune (Showtime 3), 4 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404233001003', 3, NULL, 1, 4, 600000, 0, 600000, N'session_003', SYSDATE - 1/24, SYSDATE - 1/24 + 5/1440, SYSDATE - 1/24, N'VNPAY', N'txn_vnpay_001');

-- Booking 4: Oppenheimer (Showtime 4), 2 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404234001004', 4, NULL, 1, 2, 260000, 0, 260000, N'session_004', SYSDATE - 3, SYSDATE - 3 + 5/1440, SYSDATE - 3, N'STRIPE', N'txn_stripe_002');

-- Booking 5: Oppenheimer (Showtime 5), 5 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404235001005', 5, NULL, 1, 5, 650000, 65000, 585000, N'session_005', SYSDATE - 2, SYSDATE - 2 + 5/1440, SYSDATE - 2, N'MOMO', N'txn_momo_002');

-- Booking 6: Godzilla x Kong (Showtime 7), 3 vé, Pending Payment
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt")
VALUES (N'BK202404236001006', 7, NULL, 0, 3, 375000, 0, 375000, N'session_006', SYSDATE, SYSDATE + 5/1440);

-- Booking 7: Godzilla x Kong (Showtime 8), 2 vé, Expired
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt")
VALUES (N'BK202404237001007', 8, NULL, 3, 2, 250000, 0, 250000, N'session_007', SYSDATE - 1, SYSDATE - 1 + 5/1440);

-- Booking 8: Godzilla x Kong (Showtime 9), 6 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404238001008', 9, NULL, 1, 6, 840000, 84000, 756000, N'session_008', SYSDATE - 1/48, SYSDATE - 1/48 + 5/1440, SYSDATE - 1/48, N'VNPAY', N'txn_vnpay_002');

-- Booking 9: Shawshank (Showtime 10), 2 vé, Confirmed
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404239001009', 10, NULL, 1, 2, 200000, 0, 200000, N'session_009', SYSDATE - 5, SYSDATE - 5 + 5/1440, SYSDATE - 5, N'STRIPE', N'txn_stripe_003');

-- Booking 10: Shawshank (Showtime 11), 4 vé, Cancelled
INSERT INTO "Bookings" ("BookingCode", "ShowtimeId", "CustomerId", "Status", "TotalTickets", "SubTotal", "DiscountAmount", "TotalAmount", "SessionId", "BookedAt", "ExpiresAt", "PaidAt", "PaymentMethod", "TransactionId")
VALUES (N'BK202404240001010', 11, NULL, 2, 4, 400000, 0, 400000, N'session_010', SYSDATE - 2, SYSDATE - 2 + 5/1440, SYSDATE - 2, N'MOMO', N'txn_momo_003');

-- =====================================================
-- 5. SAMPLE TICKETS (Dữ liệu mẫu vé)
-- =====================================================
-- NOTE: Ticket records are skipped because ShowtimeSeatIds are auto-generated
-- and unpredictable. To insert Tickets properly, use the actual ShowtimeSeatIds
-- from the ShowtimeSeats table after it's been populated.
-- Example query to get ShowtimeSeatIds:
-- SELECT Id, ShowtimeId, SeatNumber FROM "ShowtimeSeats" WHERE ShowtimeId = 1 AND ROWNUM <= 10;

-- =====================================================
-- COMMIT CHANGES
-- =====================================================
COMMIT;

-- =====================================================
-- VERIFY DATA (Kiểm tra dữ liệu)
-- =====================================================
SELECT 'Total Movies' AS TableName, COUNT(*) AS TotalRecords FROM "Movies"
UNION ALL
SELECT 'Total Showtimes', COUNT(*) FROM "Showtimes"
UNION ALL
SELECT 'Total ShowtimeSeats', COUNT(*) FROM "ShowtimeSeats"
UNION ALL
SELECT 'Total Bookings', COUNT(*) FROM "Bookings";
