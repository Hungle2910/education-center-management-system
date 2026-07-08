# Education Center CRM — Final AI Agent Implementation Spec

> **Ngôn ngữ giao diện:** Tiếng Việt  
> **Định hướng:** CRM vận hành trung tâm giáo dục, tối ưu cho admin, staff, giáo viên, phụ huynh và học sinh.  
> **Mục tiêu:** Dễ dùng, logic, ít thao tác, dữ liệu nhất quán, có thể vận hành thật.

---

## 1. Tóm tắt dự án

**Education Center CRM** là hệ thống quản lý trung tâm giáo dục gồm tuyển sinh, học thử, học sinh, phụ huynh, giáo viên, lớp học, lịch học, điểm danh, học bù, học phí, VietQR, thanh toán, thông báo realtime, báo cáo, bảng lương giáo viên và lịch sử thao tác.

Hệ thống không phải CRUD cơ bản. Toàn bộ nghiệp vụ đi theo một luồng vận hành rõ ràng:

```text
Tư vấn → Học thử → Ghi danh → Xếp lớp → Tạo lịch → Điểm danh
→ Nghỉ / học bù → Tính học phí → Tạo VietQR → Thanh toán
→ Xác nhận → Báo cáo → Tính lương giáo viên
```

---

## 2. Nguyên tắc sản phẩm

1. **Admin toàn quyền.** Admin xem và thao tác toàn bộ dữ liệu, bao gồm dashboard vận hành và dashboard giảng dạy.
2. **Mỗi role chỉ thấy đúng việc cần làm.** Staff, Teacher, Parent, Student có giao diện riêng, không bị quá tải.
3. **Lịch học là dữ liệu gốc.** Học phí, điểm danh, học bù và lương giáo viên phải dựa trên lịch học thực tế.
4. **Tiền bạc phải có lịch sử.** Mọi chỉnh sửa học phí, giảm giá, VietQR và thanh toán phải có audit log.
5. **Không xóa cứng dữ liệu quan trọng.** Dùng trạng thái: Đã hủy, Tạm dừng, Lưu trữ, Ngừng hoạt động.
6. **Tác vụ dài phải dùng wizard.** Tạo lớp, tạo học phí tháng, tạo lịch học bù cần chia bước rõ ràng.
7. **Thông báo quan trọng không được bị miss.** Dùng SignalR, Notification Center, toast, popup và badge chưa đọc.
8. **Giao diện tiếng Việt ngắn gọn.** Không dùng thuật ngữ kỹ thuật dài dòng.

---

## 3. Tech stack đề xuất

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- SignalR
- JWT Authentication
- Role-based Authorization
- FluentValidation
- Serilog
- Swagger/OpenAPI
- Hangfire hoặc Quartz.NET cho background jobs

### Frontend

- Next.js hoặc React
- TypeScript
- Tailwind CSS
- React Query
- Axios
- SignalR Client
- FullCalendar hoặc custom calendar UI
- Toast notification library

### Database & Tooling

- Database: SQL Server
- DB Tool: SQL Server Management Studio hoặc Azure Data Studio
- Migration: EF Core Migrations

### Integration

- VietQR: tạo mã thanh toán có sẵn số tiền và nội dung chuyển khoản
- Google Calendar API: đồng bộ lịch học cho giáo viên, phụ huynh, học sinh
- Zalo link: mở nhanh chat phụ huynh theo số điện thoại
- Optional: Google Cloud Storage cho ảnh biên lai và file import/export

---

## 4. Chuẩn wording toàn hệ thống

### 4.1 Menu chính

```text
Tổng quan
Vận hành
Giảng dạy
Tuyển sinh
Học sinh
Phụ huynh
Giáo viên
Lớp học
Lịch học
Điểm danh
Học phí
Thanh toán
Báo cáo
Cài đặt
```

### 4.2 Dashboard Admin

Admin có 3 tab chính:

```text
Tổng quan | Vận hành | Giảng dạy
```

Ý nghĩa:

- **Tổng quan:** toàn cảnh trung tâm.
- **Vận hành:** toàn bộ dữ liệu staff đang xử lý.
- **Giảng dạy:** toàn bộ dữ liệu giáo viên và lớp học.

### 4.3 Tên role

| Role | Wording hiển thị |
|---|---|
| Admin | Quản trị viên |
| Staff | Nhân viên |
| Teacher | Giáo viên |
| Parent | Phụ huynh |
| Student | Học sinh |

### 4.4 Mô tả quyền

| Role | Mô tả ngắn |
|---|---|
| Admin | Toàn quyền hệ thống |
| Staff | Quản lý vận hành |
| Teacher | Quản lý lớp đang dạy |
| Parent | Theo dõi lịch học và học phí của con |
| Student | Theo dõi lịch học cá nhân |

### 4.5 Trạng thái học phí

```text
Bản nháp
Chưa thanh toán
Chờ xác nhận
Đã thanh toán
Thanh toán thiếu
Thanh toán dư
Quá hạn
Đã hủy
Đã hoàn tiền
```

### 4.6 Trạng thái lớp học

```text
Sắp khai giảng
Đang học
Cần tuyển thêm
Tạm dừng
Đã kết thúc
Đã hủy
```

### 4.7 Trạng thái buổi học

```text
Đã lên lịch
Đã học
Đã nghỉ
Chờ học bù
Học bù
Đã hủy
```

### 4.8 Trạng thái điểm danh

```text
Có mặt
Vắng có phép
Vắng không phép
Đi trễ
Đã học bù
```

### 4.9 Nút thao tác chuẩn

```text
Tạo lớp
Thêm học sinh
Thêm phụ huynh
Tạo học phí tháng
Tạo VietQR
Tạo lại VietQR
Áp mã giảm
Xác nhận thanh toán
Báo đã thanh toán
Xuất PDF
In phiếu
Gửi phụ huynh
Tạo lịch học bù
Hủy buổi học
Xem chi tiết
Đánh dấu đã đọc
Đánh dấu đã xử lý
```

### 4.10 Toast chuẩn

```text
Tạo lớp thành công.
Cập nhật lịch học thành công.
Tạo phiếu học phí thành công.
Tạo VietQR thành công.
Đã gửi phiếu cho phụ huynh.
Đã xác nhận thanh toán.
Đã tạo lịch học bù.
Không thể lưu dữ liệu.
Không thể tạo VietQR.
Vui lòng kiểm tra lại thông tin.
Bạn không có quyền thực hiện thao tác này.
```

### 4.11 Popup quan trọng

#### Trùng lịch

```text
Không thể lưu lịch

Lớp này đang trùng phòng hoặc trùng giáo viên.
Vui lòng chọn thời gian khác.

[Chọn lại] [Đóng]
```

#### VietQR cần tạo lại

```text
Cần tạo lại VietQR

Số tiền hoặc nội dung chuyển khoản đã thay đổi.
Vui lòng tạo lại VietQR trước khi gửi phụ huynh.

[Tạo lại VietQR] [Để sau]
```

#### Hủy buổi học

```text
Hủy buổi học?

Vui lòng chọn cách xử lý học phí cho buổi nghỉ này.

( ) Tạo lịch học bù
( ) Trừ học phí buổi này

Lý do nghỉ:
[Nhập lý do]

[Không hủy] [Xác nhận]
```

---

## 5. Phân quyền

### 5.1 Admin

Admin có toàn quyền:

- Xem tất cả dashboard.
- Xem toàn bộ dữ liệu staff, teacher, parent, student.
- Thêm, sửa, hủy, lưu trữ dữ liệu.
- Quản lý tài khoản và phân quyền.
- Quản lý tuyển sinh, học thử, học sinh, phụ huynh, giáo viên.
- Quản lý lớp học, lịch học, điểm danh, học bù.
- Tạo và chỉnh học phí.
- Tạo mã giảm.
- Tạo và tạo lại VietQR.
- Xác nhận thanh toán.
- Xem công nợ, doanh thu, bảng lương giáo viên.
- Import/export Excel.
- Xem lịch sử thao tác.
- Cấu hình hệ thống.

### 5.2 Staff

Staff quản lý vận hành theo quyền được cấp:

- Tuyển sinh và học thử.
- Học sinh, phụ huynh.
- Lớp học và lịch học.
- Điểm danh, học bù.
- Học phí và thanh toán nếu được cấp quyền.
- Gửi thông báo phụ huynh.

### 5.3 Teacher

Teacher chỉ thấy dữ liệu lớp mình dạy:

- Lịch dạy.
- Danh sách lớp.
- Danh sách học sinh.
- Điểm danh.
- Nhận xét học sinh.
- Lịch học bù.
- Số buổi đã dạy.

Không mặc định xem dữ liệu học phí chi tiết.

### 5.4 Parent

Parent chỉ thấy dữ liệu của con mình:

- Lịch học.
- Thông báo lớp.
- Phiếu học phí.
- VietQR.
- Lịch sử thanh toán.
- Điểm danh và nhận xét nếu được bật.

### 5.5 Student

Student chỉ thấy dữ liệu cá nhân:

- Lịch học.
- Thông báo lớp.
- Điểm danh cá nhân.
- Nhận xét học tập nếu được bật.

---

## 6. Dashboard

### 6.1 Dashboard Admin — Tổng quan

Hiển thị:

- Doanh thu
- Chưa thanh toán
- Chờ xác nhận
- Lớp cần tuyển thêm
- Trùng lịch
- Lớp cần học bù
- Lương giáo viên
- Học thử hôm nay
- Cần chăm sóc
- Thông báo quan trọng

Bộ lọc:

```text
Thời gian
Lớp học
Giáo viên
Nhân viên
Trạng thái
```

### 6.2 Dashboard Admin — Vận hành

Admin thấy toàn bộ dữ liệu của Staff:

- Lịch học hôm nay
- Học thử hôm nay
- Phụ huynh cần chăm sóc
- Thanh toán chờ xác nhận
- Phiếu học phí cần xử lý
- Lịch học bù cần tạo
- Lớp sắp khai giảng
- Thông báo vận hành

Bộ lọc:

```text
Tất cả nhân viên
Chọn nhân viên
Thời gian
Trạng thái xử lý
```

### 6.3 Dashboard Admin — Giảng dạy

Admin thấy toàn bộ dữ liệu của Teacher:

- Lịch dạy hôm nay
- Lịch dạy trong tuần
- Danh sách lớp đang dạy
- Điểm danh cần hoàn tất
- Buổi học đã nghỉ
- Lịch học bù
- Số buổi đã dạy
- Lương dự kiến

Bộ lọc:

```text
Tất cả giáo viên
Chọn giáo viên
Thời gian
Lớp học
```

### 6.4 Dashboard Staff

- Lịch học hôm nay
- Học thử hôm nay
- Cần chăm sóc
- Chờ xác nhận thanh toán
- Lớp cần học bù
- Lớp sắp khai giảng

### 6.5 Dashboard Teacher

- Lịch dạy hôm nay
- Lịch dạy tuần này
- Lớp đang dạy
- Điểm danh cần làm
- Lịch học bù
- Nhận xét cần cập nhật

### 6.6 Dashboard Parent

- Lịch học của con
- Phiếu học phí
- Mã VietQR
- Thông báo mới
- Lịch sử thanh toán

---

## 7. Module nghiệp vụ

## 7.1 Tài khoản & quyền

Chức năng:

- Đăng nhập / đăng xuất.
- Quản lý tài khoản.
- Gán role.
- Bật/tắt tài khoản.
- Phân quyền chi tiết.
- Cấu hình quyền xem học phí, thanh toán, báo cáo.

Business rules:

1. Admin có toàn quyền.
2. Staff chỉ có quyền theo cấu hình.
3. Teacher chỉ truy cập lớp mình dạy.
4. Parent chỉ truy cập dữ liệu của con mình.
5. Student chỉ truy cập dữ liệu cá nhân.

---

## 7.2 Tuyển sinh

Chức năng:

- Lưu học sinh tiềm năng.
- Lưu nguồn tuyển sinh.
- Ghi chú tư vấn.
- Tạo lịch học thử.
- Theo dõi trạng thái chăm sóc.
- Chuyển học thử thành học viên chính thức.

Trạng thái:

```text
Mới quan tâm
Đã tư vấn
Đã hẹn học thử
Đã học thử
Đã đăng ký
Không đăng ký
Cần chăm sóc lại
```

---

## 7.3 Học thử

Workflow:

```text
Tạo học thử → Xếp vào lớp → Giáo viên nhận xét → Staff chăm sóc → Ghi danh hoặc không đăng ký
```

Thông tin cần lưu:

- Học sinh
- Phụ huynh
- Lớp học thử
- Ngày học thử
- Giáo viên
- Nhận xét
- Kết quả
- Ghi chú chăm sóc

---

## 7.4 Học sinh & phụ huynh

Chức năng học sinh:

- Hồ sơ học sinh.
- Lớp đang học.
- Lịch học.
- Điểm danh.
- Học phí.
- Lịch sử học tập.
- Trạng thái học viên.

Trạng thái học viên:

```text
Học thử
Đang học
Tạm nghỉ
Bảo lưu
Chuyển lớp
Đã nghỉ
Hoàn thành
```

Chức năng phụ huynh:

- Hồ sơ phụ huynh.
- SĐT.
- Email.
- Zalo quick chat.
- Liên kết với học sinh.
- Nhật ký chăm sóc.

Zalo quick chat:

```text
https://zalo.me/{normalizedPhone}
```

Quy tắc chuẩn hóa số điện thoại:

- Bỏ khoảng trắng, dấu chấm, dấu gạch ngang, ngoặc.
- Chuyển `0909123456` thành `84909123456`.
- Nếu số không hợp lệ, hiển thị: `Số điện thoại không hợp lệ.`

---

## 7.5 Giáo viên

Chức năng:

- Hồ sơ giáo viên.
- Môn dạy.
- Lịch dạy.
- Lớp đang dạy.
- Số buổi đã dạy.
- Số buổi nghỉ.
- Số buổi dạy bù.
- Lương dự kiến.
- Lịch sử giảng dạy.

---

## 7.6 Lớp học

Chức năng:

- Tạo lớp.
- Gán giáo viên.
- Gán phòng.
- Gán học sinh.
- Cấu hình học phí.
- Cấu hình tiền giáo viên.
- Theo dõi sĩ số.
- Theo dõi trạng thái lớp.

Trạng thái lớp:

```text
Sắp khai giảng
Đang học
Cần tuyển thêm
Tạm dừng
Đã kết thúc
Đã hủy
```

Business rules:

1. Lớp có dưới số học sinh tối thiểu được đánh dấu `Cần tuyển thêm`.
2. Lớp không được vượt sức chứa phòng.
3. Lớp cùng khối không nên trùng lịch nếu trung tâm cấu hình chặn.
4. Trung tâm không chạy quá 4 lớp cùng khung giờ nếu chỉ có 4 phòng.

---

## 7.7 Lịch học

Giao diện lịch học giống Google Calendar, nhưng dùng wording tiếng Việt.

View:

```text
Ngày | Tuần | Tháng
```

Hiển thị trên event:

- Tên lớp
- Giáo viên
- Phòng
- Thời gian
- Trạng thái

Popup chi tiết buổi học:

```text
Chi tiết buổi học

Lớp:
Giáo viên:
Phòng:
Thời gian:
Sĩ số:
Trạng thái:

[Điểm danh] [Sửa lịch] [Hủy buổi]
```

Business rules:

1. Phòng không được có 2 lớp cùng thời gian.
2. Giáo viên không được dạy 2 lớp cùng thời gian.
3. Thời gian bắt đầu phải trước thời gian kết thúc.
4. Khi hủy buổi học, bắt buộc chọn: tạo học bù hoặc trừ học phí.
5. Nếu đổi lịch, gửi thông báo realtime cho role liên quan.

Google Calendar sync:

- Admin tạo lịch học → tạo event Google Calendar.
- Sửa lịch → update event.
- Hủy lịch → update/cancel event.
- Giáo viên, phụ huynh, học sinh chỉ nhận event của lớp liên quan.
- Lưu `GoogleCalendarEventId` để cập nhật sau.

---

## 7.8 Điểm danh & học bù

Điểm danh:

```text
Có mặt
Vắng có phép
Vắng không phép
Đi trễ
Đã học bù
```

Học bù gồm 2 loại:

```text
Học bù cả lớp
Học bù cá nhân
```

Logic học phí:

1. Lớp nghỉ + có học bù cả lớp → không trừ học phí.
2. Lớp nghỉ + không học bù → trừ học phí.
3. Học sinh tự nghỉ → không tự động trừ học phí.
4. Học sinh học bù cá nhân → lưu lịch sử, không ảnh hưởng học phí lớp.

---

## 7.9 Tính học phí lớp

Admin nhập:

- Tiền giáo viên nhận/buổi.
- Số buổi/tuần.
- Hệ số quy đổi.
- Học phí/HS.
- Sĩ số mục tiêu.

Cấu hình mặc định:

```text
Tỷ lệ giáo viên: 70%
Tỷ lệ trung tâm: 30%
Số tuần/tháng: 4
Buổi 3 tiếng = 2 buổi 90 phút
Sĩ số mục tiêu: 10
```

Công thức:

```text
Buổi/tháng = Số buổi/tuần × Hệ số quy đổi × Số tuần/tháng
```

```text
Chi phí GV/tháng = Tiền GV/buổi × Buổi/tháng
```

```text
HS tối thiểu hòa vốn = CEILING(Chi phí GV/tháng / Học phí mỗi HS)
```

```text
HS tối thiểu đạt 70/30 = CEILING(Chi phí GV/tháng / (70% × Học phí mỗi HS))
```

```text
Tổng thu mục tiêu = Học phí mỗi HS × Sĩ số mục tiêu
```

```text
Lương GV mục tiêu = MAX(Chi phí GV/tháng, Tổng thu mục tiêu × 70%)
```

```text
Trung tâm giữ = Tổng thu mục tiêu - Lương GV mục tiêu
```

```text
Tỷ lệ trung tâm giữ = Trung tâm giữ / Tổng thu mục tiêu
```

Trạng thái:

```text
Nếu thiếu tiền GV/buổi → Cần bổ sung
Nếu trung tâm giữ < 0 → Lỗ
Nếu tỷ lệ trung tâm giữ < 30% → Chưa đạt
Nếu tỷ lệ trung tâm giữ >= 30% → Đạt
```

---

## 7.10 Phiếu học phí

Workflow tạo học phí tháng:

```text
Chọn tháng → Chọn lớp → Kiểm tra buổi nghỉ / học bù → Áp mã giảm
→ Xem trước → Tạo phiếu → Tạo VietQR → Gửi phụ huynh
```

Công thức:

```text
Tiền 1 buổi = Học phí tháng / Tổng số buổi dự kiến
```

```text
Số tiền trừ = Tiền 1 buổi × Số buổi nghỉ được trừ
```

```text
Số tiền cần thanh toán = Học phí gốc - Số tiền trừ - Mã giảm + Phụ phí + Điều chỉnh
```

Rules:

1. Số tiền cần thanh toán không được âm.
2. Admin được chỉnh số tiền, nhưng phải nhập lý do.
3. Chỉnh số tiền phải lưu lịch sử thao tác.
4. Nếu chỉnh số tiền sau khi tạo VietQR, QR chuyển sang trạng thái `Cần tạo lại`.
5. Học sinh vào giữa tháng được tính theo số buổi còn lại.
6. Học sinh nghỉ cá nhân không tự động trừ học phí.

---

## 7.11 Mã giảm

Loại mã giảm:

```text
Giảm theo %
Giảm số tiền cố định
Giảm theo lớp
Giảm theo học sinh
Giảm học viên cũ
Giảm đăng ký sớm
Giảm đóng nhiều tháng
```

Rules:

1. Mã giảm có ngày bắt đầu và ngày kết thúc.
2. Có giới hạn số lần dùng.
3. Có trạng thái bật/tắt.
4. Phải lưu lịch sử áp mã.
5. Mã không hợp lệ hiển thị: `Mã giảm không hợp lệ hoặc đã hết hạn.`

---

## 7.12 VietQR & thanh toán

Admin có thể sửa trước khi tạo VietQR:

- Số tiền.
- Nội dung chuyển khoản.
- Ghi chú phụ huynh.

Mặc định nội dung chuyển khoản:

```text
KH-{YYYYMM}-{sequence} {StudentNameWithoutAccent}
```

Ví dụ:

```text
KH-202607-0001 LE PHU NGUYEN
```

Rules:

1. VietQR phải có sẵn số tiền và nội dung chuyển khoản.
2. Nội dung chuyển khoản nên bỏ dấu tiếng Việt.
3. Nếu số tiền hoặc nội dung thay đổi, QR phải được tạo lại.
4. Parent bấm `Báo đã thanh toán` và có thể upload biên lai.
5. Admin xác nhận thanh toán.
6. Thanh toán phải hỗ trợ: thiếu, dư, hoàn tiền, cấn trừ.

Trạng thái thanh toán:

```text
Chưa thanh toán
Chờ xác nhận
Đã thanh toán
Thanh toán thiếu
Thanh toán dư
Quá hạn
Đã hủy
Đã hoàn tiền
```

---

## 7.13 Bảng lương giáo viên

Cuối tháng hệ thống tổng hợp:

- Số buổi đã dạy.
- Số buổi nghỉ.
- Số buổi dạy bù.
- Tiền/buổi.
- Lương cố định nếu có.
- Lương theo phần trăm doanh thu nếu có.
- Lương dự kiến.
- Lương đã thanh toán.

Workflow:

```text
Tổng hợp buổi dạy → Tính lương → Admin kiểm tra → Xác nhận thanh toán
```

---

## 7.14 Nhật ký chăm sóc phụ huynh

Lưu lịch sử:

- Đã gọi.
- Đã nhắn Zalo.
- Đã gửi học phí.
- Phụ huynh hẹn đóng tiền.
- Phụ huynh phản hồi lịch học.
- Cần chăm sóc lại.

---

## 7.15 Thông báo realtime

Dùng SignalR.

Hub:

```text
NotificationHub
CalendarHub
PaymentHub
```

Groups:

```text
admin
staff
teacher:{teacherId}
parent:{parentId}
student:{studentId}
class:{classId}
invoice:{invoiceId}
```

Notification levels:

```text
Thông tin
Thành công
Cảnh báo
Khẩn cấp
```

Behavior:

1. Thông tin/thành công: toast tự tắt.
2. Cảnh báo: toast + lưu Notification Center.
3. Khẩn cấp: popup không tự tắt.
4. Mỗi thông báo có trạng thái đã đọc/chưa đọc.
5. Thông báo quan trọng có trạng thái đã xử lý/chưa xử lý.

Events:

```text
ClassScheduleUpdated
ClassSessionCancelled
MakeupSessionCreated
ScheduleConflictDetected
InvoiceCreated
InvoiceUpdated
VietQrGenerated
VietQrOutdated
PaymentProofSubmitted
PaymentConfirmed
TuitionOverdue
ClassAtRisk
ManualTuitionAdjusted
SystemError
```

---

## 7.16 Báo cáo

### Báo cáo học phí

- Tổng học phí đã thu.
- Chưa thanh toán.
- Quá hạn.
- Thanh toán thiếu.
- Thanh toán dư.
- Doanh thu theo lớp.
- Doanh thu theo tháng.

### Báo cáo lớp học

- Sĩ số từng lớp.
- Lớp cần tuyển thêm.
- Lớp có nguy cơ lỗ.
- Lớp đạt/chưa đạt 70/30.
- Tỷ lệ sử dụng phòng.

### Báo cáo giáo viên

- Số buổi đã dạy.
- Số buổi nghỉ.
- Số buổi dạy bù.
- Lương dự kiến.
- Lương đã thanh toán.

### Báo cáo tuyển sinh

- Lead mới.
- Học thử.
- Tỷ lệ đăng ký sau học thử.
- Nguồn tuyển sinh hiệu quả.

---

## 7.17 Import / Export Excel

Import:

- Học sinh.
- Phụ huynh.
- Giáo viên.
- Lớp học.
- Học phí.

Export:

- Danh sách học sinh.
- Danh sách phụ huynh.
- Học phí tháng.
- Công nợ.
- Bảng lương giáo viên.
- Báo cáo doanh thu.

Rules:

1. Import phải có bước xem trước.
2. Dòng lỗi phải được đánh dấu rõ.
3. Không ghi dữ liệu lỗi vào database.
4. Cho phép tải file lỗi.

---

## 7.18 Lịch sử thao tác

Mọi hành động quan trọng phải lưu:

- Người thao tác.
- Thời gian.
- Hành động.
- Dữ liệu cũ.
- Dữ liệu mới.
- Lý do nếu có.

Bắt buộc lưu với:

- Sửa học phí.
- Áp mã giảm.
- Tạo lại VietQR.
- Xác nhận thanh toán.
- Hủy buổi học.
- Tạo học bù.
- Sửa lịch học.
- Sửa quyền người dùng.

---

## 7.19 Cài đặt hệ thống

Onboarding lần đầu:

- Tên trung tâm.
- Logo.
- Địa chỉ.
- SĐT liên hệ.
- Email.
- Tài khoản nhận học phí.
- Ngân hàng.
- Tỷ lệ giáo viên/trung tâm.
- Số phòng học.
- Khung giờ học mặc định.
- Mẫu nội dung chuyển khoản.

---

## 8. Database đề xuất

### Identity & Role

```text
Users
Roles
UserRoles
Permissions
RolePermissions
```

### Admissions

```text
Leads
LeadSources
TrialSessions
ConsultationLogs
```

### People

```text
Students
Parents
StudentParents
Teachers
StaffProfiles
```

### Class & Schedule

```text
Subjects
Rooms
Classes
ClassStudents
ClassSchedules
ScheduleOccurrences
AttendanceRecords
MakeupSessions
```

### Tuition & Payment

```text
BusinessSettings
ClassPricingPlans
DiscountCodes
TuitionInvoices
TuitionInvoiceItems
InvoiceDiscounts
Payments
PaymentProofs
CreditBalances
Refunds
```

### Notification

```text
Notifications
NotificationRecipients
NotificationPreferences
```

### Reports & Audit

```text
TeacherPayrolls
TeacherPayrollItems
ParentCommunicationLogs
AuditLogs
ImportJobs
ExportJobs
SystemSettings
```

---

## 9. API endpoints chính

### Auth

```text
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me
```

### Dashboard

```text
GET /api/dashboard/admin/overview
GET /api/dashboard/admin/operations
GET /api/dashboard/admin/teaching
GET /api/dashboard/staff
GET /api/dashboard/teacher
GET /api/dashboard/parent
```

### Tuyển sinh

```text
GET    /api/leads
POST   /api/leads
PUT    /api/leads/{id}
POST   /api/leads/{id}/trial
POST   /api/leads/{id}/convert-to-student
```

### Học sinh & phụ huynh

```text
GET    /api/students
POST   /api/students
GET    /api/students/{id}
PUT    /api/students/{id}
POST   /api/students/{id}/parents
GET    /api/parents
POST   /api/parents
GET    /api/parents/{id}/zalo-link
POST   /api/parents/{id}/communication-log
```

### Lớp học

```text
GET    /api/classes
POST   /api/classes
GET    /api/classes/{id}
PUT    /api/classes/{id}
POST   /api/classes/{id}/students
DELETE /api/classes/{id}/students/{studentId}
POST   /api/classes/{id}/pricing/calculate
```

### Lịch học

```text
GET  /api/schedules/calendar
POST /api/schedules
PUT  /api/schedules/{id}
POST /api/schedules/{id}/cancel
POST /api/schedules/{id}/makeup
GET  /api/schedules/conflicts/check
POST /api/schedules/{id}/sync-google-calendar
```

### Điểm danh

```text
GET  /api/attendance/session/{occurrenceId}
POST /api/attendance/session/{occurrenceId}
PUT  /api/attendance/{id}
```

### Học phí

```text
POST /api/tuition/invoices/generate-monthly
GET  /api/tuition/invoices
GET  /api/tuition/invoices/{id}
PUT  /api/tuition/invoices/{id}/items
PUT  /api/tuition/invoices/{id}/manual-adjustment
PUT  /api/tuition/invoices/{id}/payment-content
POST /api/tuition/invoices/{id}/apply-discount
POST /api/tuition/invoices/{id}/generate-vietqr
POST /api/tuition/invoices/{id}/regenerate-vietqr
POST /api/tuition/invoices/{id}/submit-payment-proof
POST /api/tuition/invoices/{id}/confirm-payment
GET  /api/tuition/invoices/{id}/print-view
```

### Mã giảm

```text
GET    /api/discount-codes
POST   /api/discount-codes
PUT    /api/discount-codes/{id}
DELETE /api/discount-codes/{id}
```

### Thông báo

```text
GET  /api/notifications
GET  /api/notifications/unread-count
POST /api/notifications/{id}/read
POST /api/notifications/read-all
POST /api/notifications/{id}/resolve
GET  /api/notification-preferences
PUT  /api/notification-preferences
```

### Báo cáo

```text
GET /api/reports/tuition
GET /api/reports/classes
GET /api/reports/teachers
GET /api/reports/admissions
GET /api/reports/payroll
```

---

## 10. Background jobs

Dùng Hangfire hoặc Quartz.NET cho:

- Tạo hàng loạt phiếu học phí.
- Nhắc học phí sắp đến hạn.
- Đánh dấu học phí quá hạn.
- Gửi thông báo hàng loạt.
- Đồng bộ Google Calendar.
- Tạo báo cáo cuối tháng.
- Tính lương giáo viên cuối tháng.
- Dọn file import/export cũ.

---

## 11. UX tối ưu

### 11.1 Wizard

Tạo lớp:

```text
Thông tin lớp → Lịch học → Giáo viên & phòng → Học phí → Kiểm tra → Hoàn tất
```

Tạo học phí tháng:

```text
Chọn tháng → Chọn lớp → Kiểm tra nghỉ/học bù → Áp mã giảm → Xem trước → Tạo phiếu
```

Hủy buổi học:

```text
Chọn buổi → Nhập lý do → Chọn học bù hoặc trừ học phí → Xác nhận → Gửi thông báo
```

### 11.2 Quick Actions

Dashboard cần có:

```text
+ Thêm học sinh
+ Tạo lớp
+ Tạo học phí tháng
+ Tạo lịch học bù
+ Gửi thông báo
+ Xác nhận thanh toán
```

### 11.3 Global Search

Placeholder:

```text
Tìm học sinh, phụ huynh, lớp học, SĐT, mã phiếu...
```

Search theo:

- Tên học sinh.
- Tên phụ huynh.
- SĐT.
- Mã phiếu.
- Lớp học.
- Giáo viên.

---

## 12. Testing bắt buộc

Unit tests:

1. Chuẩn hóa số điện thoại Việt Nam.
2. Tạo link Zalo.
3. Tính buổi/tháng.
4. Tính chi phí GV/tháng.
5. Tính số HS hòa vốn.
6. Tính số HS đạt 70/30.
7. Tính học phí khi lớp nghỉ không học bù.
8. Không trừ học phí khi có học bù.
9. Tính học phí học sinh vào giữa tháng.
10. Áp mã giảm theo %.
11. Áp mã giảm số tiền cố định.
12. Mã giảm hết hạn.
13. Chỉnh học phí bắt buộc nhập lý do.
14. QR hết hiệu lực khi đổi số tiền.
15. QR hết hiệu lực khi đổi nội dung chuyển khoản.
16. Parent không xem được phiếu của học sinh khác.
17. Teacher không xem được lớp không được phân công.
18. Check trùng phòng.
19. Check trùng giáo viên.
20. SignalR gửi đúng group.

---

## 13. Roadmap triển khai

### Phase 1 — Core CRM

- Auth & role.
- Học sinh, phụ huynh, giáo viên.
- Lớp học.
- Lịch học.
- Check trùng lịch.
- Điểm danh.
- Học bù.

### Phase 2 — Học phí & thanh toán

- Tính học phí lớp.
- Tạo học phí tháng.
- Mã giảm.
- VietQR.
- Phụ huynh báo thanh toán.
- Admin xác nhận.
- Công nợ / thiếu / dư.

### Phase 3 — Vận hành nâng cao

- Tuyển sinh.
- Học thử.
- Nhật ký chăm sóc phụ huynh.
- Bảng lương giáo viên.
- Báo cáo.
- Import/export Excel.
- Audit log.

### Phase 4 — Tích hợp

- SignalR Notification Center.
- Google Calendar sync.
- Zalo quick chat.
- Background jobs.
- File storage.

---

## 14. Prompt tổng cho AI Agent

```text
You are a senior full-stack .NET architect and product engineer.

Build a production-ready Education Center CRM using ASP.NET Core C#, SQL Server, Entity Framework Core, SignalR, and React/Next.js.

The entire UI must be in Vietnamese. Use concise, professional, easy-to-understand wording. Avoid technical English labels.

Project goal:
Create a real CRM for an education center. The system must manage the full workflow:
Tư vấn → Học thử → Ghi danh → Xếp lớp → Tạo lịch → Điểm danh → Nghỉ / học bù → Tính học phí → Tạo VietQR → Thanh toán → Xác nhận → Báo cáo → Tính lương giáo viên.

Tech stack:
- Backend: ASP.NET Core Web API with C#
- Database: SQL Server
- ORM: Entity Framework Core
- Realtime: SignalR
- Auth: JWT with role-based authorization
- Validation: FluentValidation
- Logging: Serilog
- API docs: Swagger/OpenAPI
- Frontend: React or Next.js with TypeScript
- UI: Tailwind CSS
- Calendar: FullCalendar or custom Google Calendar-like UI
- Background jobs: Hangfire or Quartz.NET

Roles:
- Admin: full system access
- Staff: operations management
- Teacher: assigned classes only
- Parent: children only
- Student: personal data only

Admin requirements:
1. Admin has full access to all functions and data.
2. Admin can view Admin Overview, Operations, and Teaching dashboards.
3. Admin can see everything Staff and Teacher can see.
4. Admin can filter operations by staff.
5. Admin can filter teaching data by teacher.

Main modules:
1. Tài khoản & quyền
2. Tuyển sinh
3. Học thử
4. Học sinh
5. Phụ huynh
6. Giáo viên
7. Lớp học
8. Lịch học
9. Điểm danh & học bù
10. Học phí
11. Mã giảm
12. VietQR & thanh toán
13. Thông báo realtime
14. Báo cáo
15. Bảng lương giáo viên
16. Lịch sử thao tác
17. Cài đặt

Core business rules:
- A room cannot have two classes at the same time.
- A teacher cannot teach two classes at the same time.
- The center cannot run more classes than available rooms in the same time slot.
- Schedule is the source of truth for attendance, tuition deduction, makeup sessions, and teacher payroll.
- Class cancellation must choose either makeup session or tuition deduction.
- Student personal absence does not reduce tuition by default.
- Tuition must support discounts, manual adjustment, extra fees, debt, overpayment, refund, and credit balance.
- Manual money changes require reason and audit log.
- VietQR must include final amount and payment content.
- VietQR becomes outdated if amount or payment content changes.
- Parent can only see children’s invoices and schedules.
- Teacher can only see assigned classes.
- Admin can see all dashboards and all data.

Vietnamese wording:
Use these main navigation labels:
Tổng quan, Vận hành, Giảng dạy, Tuyển sinh, Học sinh, Phụ huynh, Giáo viên, Lớp học, Lịch học, Điểm danh, Học phí, Thanh toán, Báo cáo, Cài đặt.

Before coding, generate:
1. Product brief
2. User flows
3. Database schema
4. API contract
5. Folder structure
6. UI page list
7. Development roadmap

Then implement step by step with clean architecture, tests, seed data, and README.
```

---

## 15. Prompt backend cho Codex

```text
Implement the backend for the Education Center CRM.

Use:
- ASP.NET Core Web API
- C#
- SQL Server
- EF Core
- Clean Architecture
- JWT Authentication
- Role-based Authorization
- SignalR
- FluentValidation
- Serilog
- Swagger
- Hangfire or Quartz.NET

Architecture:
- Domain
- Application
- Infrastructure
- API
- Tests

Implement modules:
1. Auth and permissions
2. Admissions and trial sessions
3. Students and parents
4. Teachers
5. Classes and schedules
6. Attendance and makeup sessions
7. Class pricing calculation
8. Tuition invoices
9. Discount codes
10. VietQR generation
11. Payments
12. Notifications with SignalR
13. Teacher payroll
14. Reports
15. Audit logs
16. System settings

Admin must have full access to every endpoint and dashboard.
Staff, Teacher, Parent, and Student must be restricted by role.

Add unit tests for all important business rules.
Seed realistic demo data.
```

---

## 16. Prompt frontend cho Codex

```text
Build the Vietnamese frontend for the Education Center CRM.

Use:
- React or Next.js
- TypeScript
- Tailwind CSS
- React Query
- Axios
- SignalR client
- FullCalendar or custom calendar
- Toast notifications

The entire UI must be in Vietnamese. Use short, professional, clear wording.

Main navigation:
- Tổng quan
- Vận hành
- Giảng dạy
- Tuyển sinh
- Học sinh
- Phụ huynh
- Giáo viên
- Lớp học
- Lịch học
- Điểm danh
- Học phí
- Thanh toán
- Báo cáo
- Cài đặt

Build role-based dashboards:
- Admin: Tổng quan, Vận hành, Giảng dạy
- Staff: Vận hành
- Teacher: Giảng dạy
- Parent: Lịch học và học phí của con
- Student: Lịch học cá nhân

Admin must be able to view all staff and teacher dashboards.

Design style:
- Clean
- Warm
- Modern
- Easy for non-technical users
- Rounded cards
- Soft shadows
- Clear money formatting
- Clear status badges
- Mobile responsive

Important UI components:
- Dashboard cards
- Global search
- Quick actions
- Calendar view
- Notification bell
- Notification center
- Critical popup
- Tuition invoice detail
- VietQR payment panel
- Wizard for class creation
- Wizard for monthly tuition generation
```

---

## 17. Definition of Done

Dự án được xem là hoàn chỉnh khi:

1. Admin thao tác được toàn bộ hệ thống.
2. Staff vận hành được lớp, học sinh, phụ huynh, học phí theo quyền.
3. Teacher điểm danh và xem lịch dạy dễ dàng.
4. Parent xem lịch học, học phí và quét VietQR được.
5. Lịch học check trùng phòng và trùng giáo viên chính xác.
6. Học phí tự tính theo lịch, nghỉ, học bù, mã giảm và điều chỉnh.
7. VietQR có đúng số tiền và nội dung chuyển khoản.
8. Thanh toán có trạng thái rõ ràng.
9. Thông báo realtime không bị miss.
10. Báo cáo doanh thu, công nợ, lớp học và lương giáo viên hoạt động đúng.
11. Mọi chỉnh sửa tiền bạc có lịch sử thao tác.
12. UI tiếng Việt ngắn gọn, dễ hiểu và nhất quán.
```
