
## **Tóm tắt kiến trúc Clean**

**1.** Ứng dụng chia thành các layer, mỗi layer chứa các thành phần con tuỳ từng dự án:

<p align="center">
<img src="./DOCUMENT/clean_architecture_layers.png" width="400" align="center"/>
</p>

* **Core Layer:** chứa entity, model, định nghĩa interface, abstraction, xử lý nghiệp vụ cốt lõi,... hoàn toàn độc lập với mọi lớp khác, không phụ thuộc lib, framework nào

* **Infrastructure Layer:** triển khai các interface định nghĩa trong Core tới các tác nhân thứ cấp, kết nối, tương tác với thư viện và hệ thống bên thứ 3 như: FileSystem, FileLogger, Web service, API, SDK, DB,...

* **Presentation Layer:** hiển thị User Interface hoặc JSON cho các hệ thống khác, biến đổi và validation cơ bản các dữ liệu đầu vào, cấu hình theme, DI… sử dụng use case/business service,...  

> **Trong Core định nghĩa các IRepository triển khai ở Infrastructure Layer để tương tác với Web service, API hệ thống bên thứ 3. Nếu không có xử lý nghiệp vụ gì sâu, đặc biệt thì Presentation Layer có thể gọi trực tiếp các IRepository để đơn giản code mà không cần qua use case/business service**

**2.** Quy tắc phụ thuộc source code luôn hướng vào trong (Core), các module, class trong và giữa các layer nên giao tiếp với nhau qua interface, abstraction thay vì implementation giúp **dễ dàng thay thế, sửa đổi mà không bị ảnh hưởng gì**

**3.** Luôn đi kèm theo các quy tắc Dependency Inversion và Injection

**4.** Kết hợp với các chiến lược Cache đem lại UX tốt hơn

---
