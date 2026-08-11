# Linux Filesystem, Permissions, and Identities

## Mục tiêu / Learning Objectives

Sau chương này, người học có thể:

- giải thích đường dẫn Linux được resolve qua directory entry và inode như thế nào;
- phân biệt read, write, execute trên file với read, write, search trên directory;
- dự đoán quyền hiệu lực từ user, group, mode bits, ACL, umask và capabilities;
- điều tra lỗi Permission denied mà không dùng chmod 777;
- nhận diện disk full, inode exhaustion, read-only mount, deleted-open file và symlink risk;
- thiết kế ownership và secret permissions phù hợp cho một service .NET.

## Tại sao cần học? / Why It Matters

Một service có thể chạy đúng binary nhưng vẫn thất bại vì không traverse được parent directory, file thuộc nhầm group, ACL mask chặn quyền, filesystem bị remount read-only hoặc container identity không khớp host identity. Lỗi thường xuất hiện dưới cùng một thông báo Permission denied, trong khi nguyên nhân và cách sửa rất khác nhau.

Quyền filesystem cũng là security boundary. Mở rộng quyền để chữa cháy có thể biến lỗi availability thành lộ secret, sửa binary hoặc privilege escalation.

## Tổng quan / Overview

Để một process mở được một path, cần trả lời theo thứ tự:

~~~text
Path string
  ↓ resolve từng component; cần search trên mỗi directory
Directory entry → inode
  ↓ chọn owner / named user / group / other và áp ACL mask
Requested operation được mode/ACL cho phép?
  ↓
Mount flags, read-only state, capabilities, LSM và namespace còn cho phép?
  ↓
Open file descriptor hoặc errno
~~~

Permissions không phải encryption. Một principal có quyền đọc hoặc một process bị compromise vẫn thấy plaintext.

## Mental Model

### Tên, inode và nội dung là ba khái niệm khác nhau

- Directory lưu mapping từ tên đến inode.
- Inode lưu type, owner, group, mode, timestamps và metadata trỏ tới dữ liệu.
- File descriptor đã mở tham chiếu kernel object; xóa tên không nhất thiết giải phóng dữ liệu ngay.
- Hard link là tên khác cho cùng inode. Symbolic link là file chứa một target path và được resolve tiếp.

### Directory permissions

| Bit trên directory | Ý nghĩa thực tế |
| --- | --- |
| r | Liệt kê tên entry, nếu các quyền khác cho phép truy cập directory |
| w | Tạo, đổi tên hoặc xóa entry; thường cần kết hợp x |
| x | Search/traverse: đi qua directory hoặc lookup một tên đã biết |

Vì vậy, file có mode 0644 vẫn có thể không đọc được nếu một parent directory thiếu x. Ngược lại, có x mà không có r có thể lookup tên đã biết nhưng không liệt kê toàn bộ directory.

### Quyền hiệu lực

Kernel không cộng tùy ý owner, group và other. Nó chọn class phù hợp, sau đó ACL mask có thể giới hạn named user/group entries. Root cũng không nên được xem như bỏ qua mọi boundary: capabilities chia nhỏ đặc quyền, mount/namespace/LSM có thể tiếp tục áp chính sách.

## Thuật ngữ / Terminology

| Thuật ngữ | Ý nghĩa |
| --- | --- |
| UID / GID | Numeric user/group identity kernel dùng để kiểm tra quyền |
| Effective identity | Identity thường được dùng khi kiểm tra permission cho operation |
| Inode | Metadata object của filesystem; tên file nằm ở directory entry |
| Mode bits | owner/group/other với read, write, execute cùng special bits |
| umask | Mặt nạ loại bỏ quyền khi tạo object mới; không tự sửa object cũ |
| ACL | Danh sách quyền mở rộng cho named user/group và default ACL |
| Capability | Một phần đặc quyền truyền thống của root, ví dụ CAP_DAC_OVERRIDE |
| Sticky bit | Trên shared directory, hạn chế ai được xóa/rename entry |
| setgid directory | File/subdirectory mới thường kế thừa group của directory |
| Mount namespace | View riêng của process đối với mount tree |
| LSM | Security framework như SELinux hoặc AppArmor |

## Prerequisites

- Linux shell cơ bản và [module overview](README.md).
- Hiểu process chạy dưới một user cụ thể.
- Có stat, id, findmnt và df; getfacl, namei, lsof là optional.

## How It Works

### 1. Resolve path

Với /srv/myapp/config/appsettings.json, kernel phải search /, srv, myapp và config trước khi kiểm tra file cuối. Symlink có thể chuyển resolution sang cây khác. Một mount có thể che khuất nội dung vốn tồn tại bên dưới mount point.

### 2. Xác định identity

Ghi lại real/effective UID, primary group và supplementary groups của process. Tên hiển thị chỉ là mapping userspace; kernel chủ yếu so numeric IDs. Trong container hoặc user namespace, UID 0 bên trong không mặc nhiên tương đương host root.

### 3. Áp mode và ACL

- Nếu effective UID khớp owner, owner bits được xét.
- Nếu ACL có named-user entry phù hợp, entry đó và ACL mask quyết định quyền.
- Nếu group hoặc supplementary group khớp, group entries và mask được xét.
- Cuối cùng mới dùng other.

Default ACL và umask ảnh hưởng object mới. Muốn biết kết quả thật, kiểm tra object sau khi tạo thay vì chỉ suy luận từ umask.

### 4. Áp boundary còn lại

Mode bits đúng vẫn chưa đủ nếu:

- filesystem hoặc bind mount là read-only;
- mount có noexec và operation là execute;
- SELinux/AppArmor từ chối;
- process thiếu capability cần thiết;
- quota, block hoặc inode đã hết;
- path nằm ngoài mount namespace của service.

## Minimal Example

Tạo một sandbox có thời hạn do chính user sở hữu:

~~~bash
lab_dir=$(mktemp -d)
printf 'lab=%s\n' "$lab_dir"
mkdir "$lab_dir/private"
printf 'secret-placeholder\n' > "$lab_dir/private/config.txt"
chmod 700 "$lab_dir/private"
chmod 600 "$lab_dir/private/config.txt"

id
stat -c '%A %a %U:%G %n' "$lab_dir/private" "$lab_dir/private/config.txt"
~~~

Quan sát tác động của umask trong cùng sandbox:

~~~bash
old_umask=$(umask)
umask 027
: > "$lab_dir/created-with-umask.txt"
mkdir "$lab_dir/created-directory"
stat -c '%A %a %n' "$lab_dir/created-with-umask.txt" "$lab_dir/created-directory"
umask "$old_umask"
~~~

Kỳ vọng thông thường là file 0640 và directory 0750, nhưng ACL/default policy có thể làm kết quả khác. Evidence cuối cùng là stat/getfacl, không phải giả định.

## Production Example

Symptom: service myapp đọc /etc/myapp/appsettings.Production.json khi chạy thủ công, nhưng systemd báo Permission denied.

Điều tra theo boundary:

~~~bash
systemctl show myapp.service -p User -p Group -p SupplementaryGroups -p UMask -p RootDirectory -p ProtectSystem
systemctl status myapp.service --no-pager
journalctl -u myapp.service --since '15 minutes ago' --no-pager

namei -l /etc/myapp/appsettings.Production.json
stat -c '%A %a %U:%G %u:%g %n' /etc/myapp /etc/myapp/appsettings.Production.json
getfacl -p /etc/myapp /etc/myapp/appsettings.Production.json
findmnt -T /etc/myapp/appsettings.Production.json -o TARGET,SOURCE,FSTYPE,OPTIONS
~~~

Nếu namei hoặc getfacl không được cài, dùng stat cho từng parent directory. So sánh service identity với ownership bằng một shell chạy đúng identity chỉ khi tổ chức cho phép:

~~~bash
sudo -u myapp -- test -r /etc/myapp/appsettings.Production.json
printf 'exit=%s\n' "$?"
~~~

Không đổi permission trước khi biết boundary hỏng. Fix nhỏ nhất thường là owner/group đúng, directory search permission tối thiểu, service SupplementaryGroups đúng hoặc deployment tạo file với mode phù hợp. chmod 777 không phải chẩn đoán và thường vi phạm least privilege.

## .NET Integration

Ứng dụng .NET không bypass Linux permissions. FileStream, configuration providers, certificate loading và data-protection keys đều mở file dưới identity của process.

Ví dụ kiểm tra một path bắt buộc khi startup:

~~~csharp
using System.Security;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: PermissionProbe <absolute-path>");
    return 64;
}

try
{
    await using var stream = new FileStream(
        args[0],
        FileMode.Open,
        FileAccess.Read,
        FileShare.Read);

    Console.WriteLine($"Readable: {Path.GetFullPath(args[0])}; bytes={stream.Length}");
    return 0;
}
catch (UnauthorizedAccessException exception)
{
    Console.Error.WriteLine($"Access denied: {exception.Message}");
    return 77;
}
catch (SecurityException exception)
{
    Console.Error.WriteLine($"Security policy denied access: {exception.Message}");
    return 77;
}
catch (IOException exception)
{
    Console.Error.WriteLine($"I/O failure: {exception.Message}");
    return 74;
}
~~~

Trong production:

- chạy service bằng dedicated non-login user;
- cấp quyền cho secret, certificate private key, log/data directory và Unix socket một cách tường minh;
- tạo directory trong deployment/package step, không tự chmod rộng khi startup;
- log path chuẩn hóa và exception category nhưng không log nội dung secret;
- kiểm tra quyền data-protection key ring nếu nhiều replica cần dùng chung.

## Internals

### Open file và deleted file

Sau khi open thành công, process dùng file descriptor; permission thay đổi sau đó không tự thu hồi descriptor đã mở. Nếu một log bị unlink nhưng process vẫn giữ descriptor, df vẫn thấy block được dùng trong khi du có thể không cộng được tên đã xóa. lsof +L1 giúp tìm deleted-open files.

### Rename và durability

Atomic rename trong cùng filesystem thường được dùng để publish file hoàn chỉnh, nhưng atomic visibility không đồng nghĩa dữ liệu đã durable sau power loss. Durability cần flush file và, tùy filesystem/workflow, directory metadata. Rename xuyên filesystem không còn cùng primitive.

### Numeric identity

NFS, bind mounts và containers thường phơi bày vấn đề numeric UID/GID mismatch. Cùng username ở hai nơi không đảm bảo cùng UID. Luôn ghi cả tên và số khi điều tra.

## Common Mistakes

- Dùng chmod -R 777 để loại bỏ symptom.
- Chỉ kiểm tra file cuối, bỏ qua x trên parent directory.
- Nhầm directory r với khả năng traverse.
- Chỉ nhìn ls -l, bỏ qua ACL, mount options, namespace và LSM.
- Dùng access check rồi giả định open chắc chắn thành công; state có thể đổi giữa hai operation.
- Chown cả volume mà không kiểm tra symlink, mount boundary và số lượng target.
- Tin username thay vì numeric UID/GID trong container/NFS.
- Xóa log lớn nhưng không restart/reopen process đang giữ descriptor.
- Đặt secret trong image layer hoặc world-readable config.

## Performance Considerations

- Quá nhiều small files làm metadata lookup và inode pressure đáng kể.
- Recursive stat/chown/chmod trên cây lớn có thể tạo I/O storm và lock contention.
- Network filesystem thêm latency, cache consistency và server-side identity semantics.
- fsync tăng durability nhưng có latency; cần batching và đo tail latency.
- Logging quá mức có thể lấp disk, tăng write amplification và làm service khác thất bại.

## Security Considerations

- Áp least privilege cho user, group, ACL và Linux capabilities.
- Không dùng predictable file name trong shared temporary directory; dùng API tạo file atomically.
- Kiểm soát symlink/hard-link attacks khi service đặc quyền xử lý path do user cung cấp.
- Tránh time-of-check/time-of-use: mở resource an toàn rồi thao tác qua descriptor thay vì kiểm tra tên nhiều lần.
- Bảo vệ parent directory của executable/config; file chỉ read-only không đủ nếu attacker thay directory entry.
- Audit setuid/setgid binaries và file capabilities; không gán capability rộng để chữa lỗi quyền.
- Mount secrets read-only khi có thể và xoay secret nếu đã từng world-readable.

## Reliability / Failure Modes

| Failure mode | Evidence phân biệt | Hướng xử lý |
| --- | --- | --- |
| Parent thiếu search permission | namei -l hoặc stat từng component | Sửa đúng directory/group |
| ACL mask chặn quyền | getfacl hiển thị effective permissions | Sửa ACL/mask có chủ đích |
| Read-only filesystem | findmnt options; kernel log | Xử lý mount/storage cause |
| Hết block | df -h và write trả ENOSPC | Dọn/rotate/mở rộng có kiểm soát |
| Hết inode | df -i | Giảm small-file churn hoặc mở rộng |
| Deleted-open file | lsof +L1, df khác du | Reopen/rotate hoặc restart có kế hoạch |
| UID/GID mismatch | stat số và process identity | Đồng bộ numeric identity/mapping |
| Symlink target đổi | readlink/stat và deployment timeline | Dùng atomic deploy, pin boundary |
| LSM deny | audit/kernel logs | Sửa policy hoặc path, không disable global |

## Observability

Thu thập tối thiểu:

- path tuyệt đối đã chuẩn hóa và operation read/write/execute;
- errno/exception type;
- service UID/GID và supplementary groups;
- mode, owner/group số, ACL và parent traversal;
- mount source/type/options, free blocks và free inodes;
- deployment/config change timeline;
- LSM/audit event nếu hệ thống bật policy.

Không đưa file content, secret value hoặc private key vào log/ticket.

## Operational Considerations

- Package/deployment phải định nghĩa owner, group, mode và directory creation rõ ràng.
- Log rotation cần signal/reopen contract với application.
- Backup phải bảo toàn metadata cần thiết: mode, owner, ACL, xattrs và symlink.
- Health check ghi file có thể tạo false positive hoặc làm bẩn storage; kiểm tra đúng dependency và cleanup policy.
- Với WSL, file dưới Windows filesystem có semantics phụ thuộc mount metadata; lab production nên lặp lại trên filesystem Linux thực tế hoặc môi trường tương đương deployment.

## Architect Perspective

Filesystem boundary là một phần của architecture:

- config immutable/read-only hay runtime mutable;
- replica chia sẻ storage hay mỗi replica có state riêng;
- identity được cấp từ host, orchestrator hay external identity mapping;
- secret qua file, environment hay secret provider;
- durability, consistency, backup và disaster recovery contract;
- single-writer, locking và atomic publish strategy.

Decision record nên nêu owner của dữ liệu, lifetime, recovery objective và quyền tối thiểu của từng actor.

## Trade-offs

| Lựa chọn | Ưu điểm | Chi phí/rủi ro |
| --- | --- | --- |
| Mode bits + group | Đơn giản, dễ audit | Ít biểu đạt khi nhiều principal |
| POSIX ACL | Cấp quyền chi tiết | Dễ bị bỏ sót khi backup/deploy; mask gây nhầm |
| Shared writable volume | Chia sẻ state nhanh | Coupling, locking, identity và blast radius |
| Immutable image + read-only root | Giảm drift/tamper | Cần tách rõ writable paths |
| Root process | Ít lỗi permission ban đầu | Blast radius và security risk rất lớn |

## When NOT to Use It

- Không dùng filesystem permissions như cơ chế mã hóa secret.
- Không dùng shared file như distributed lock nếu chưa có rõ consistency/failure contract.
- Không đặt database-like mutable state lên network share chỉ vì dễ mount.
- Không chạy service bằng root chỉ để tránh thiết kế ownership.

## Alternatives

- Secret manager/KMS cho secret lifecycle và audit.
- Object storage cho immutable blob và versioning.
- Database cho concurrent structured state và transaction.
- Unix domain socket với group ownership cho local IPC.
- Capability cụ thể thay vì full root, nếu threat model và deployment hỗ trợ.

## Review Questions

1. Vì sao file 0644 vẫn có thể trả Permission denied?
2. r và x trên directory khác nhau thế nào?
3. umask tác động object mới ra sao và tại sao cần stat để xác nhận?
4. Vì sao df báo đầy nhưng du không tìm thấy dữ liệu tương ứng?
5. Khi nào ACL mask làm permission hiển thị dễ gây hiểu nhầm?
6. Tại sao UID 0 trong container không nhất thiết là host root?
7. chmod 777 tạo thêm những attack path nào?
8. Bạn cần evidence gì trước khi thay owner/group của production path?

## Hands-on Lab

### Mục tiêu

Chứng minh parent traversal, mode creation và deleted-open behavior trong sandbox do chính user tạo.

### Bước 1: tạo và ghi lại sandbox

Dùng các lệnh trong Minimal Example. Giữ nguyên path mà mktemp in ra; mọi thao tác sau chỉ được nhắm vào path này.

### Bước 2: chứng minh directory search

~~~bash
chmod 600 "$lab_dir/private"
head -n 1 "$lab_dir/private/config.txt"
printf 'expected failure exit=%s\n' "$?"

chmod 700 "$lab_dir/private"
head -n 1 "$lab_dir/private/config.txt"
~~~

Ghi lại output và giải thích vì sao quyền của config.txt không đổi nhưng kết quả đổi.

### Bước 3: quan sát open descriptor sau unlink

~~~bash
printf 'temporary-data\n' > "$lab_dir/open-file.txt"
exec 9< "$lab_dir/open-file.txt"
rm -- "$lab_dir/open-file.txt"
read -r first_line <&9
printf 'read-from-open-fd=%s\n' "$first_line"
exec 9<&-
~~~

rm ở đây chỉ xóa đúng một file trong sandbox vừa tạo; không dùng glob hoặc recursive deletion.

### Bước 4: báo cáo

Nộp:

- id output;
- stat trước/sau;
- exit code khi thiếu directory search;
- giải thích inode, directory entry và open file descriptor;
- đề xuất mode/owner/group cho /etc/myapp và một writable /var/lib/myapp.

## Exit Criteria

Hoàn thành khi người học có thể:

- dự đoán kết quả của một path access từ identity, parent directories, mode và ACL;
- chẩn đoán Permission denied mà không nới quyền thử nghiệm;
- phân biệt hết block, hết inode và deleted-open file;
- đề xuất production ownership cho config, secret, binary, log và data;
- mô tả ít nhất ba boundary ngoài mode bits.

## Related Topics

- [Production Troubleshooting Foundations](production-troubleshooting-foundations.md)
- [Process, Signals, and Resource Pressure](process-signals-and-resource-pressure.md)
- Containers, namespaces and cgroups
- Secure deployment and secret management
- Backup, restore and disaster recovery

## Official English Sources

- [Linux path resolution](https://www.man7.org/linux/man-pages/man7/path_resolution.7.html)
- [inode(7)](https://www.man7.org/linux/man-pages/man7/inode.7.html)
- [credentials(7)](https://www.man7.org/linux/man-pages/man7/credentials.7.html)
- [chmod(1)](https://www.man7.org/linux/man-pages/man1/chmod.1.html)
- [umask(2)](https://www.man7.org/linux/man-pages/man2/umask.2.html)
- [acl(5)](https://www.man7.org/linux/man-pages/man5/acl.5.html)
- [capabilities(7)](https://www.man7.org/linux/man-pages/man7/capabilities.7.html)
- [symlink(7)](https://www.man7.org/linux/man-pages/man7/symlink.7.html)
- [Filesystem Hierarchy Standard 3.0](https://refspecs.linuxfoundation.org/FHS_3.0/fhs/index.html)

## Vietnamese Resources

- [Quyền file giữa Windows và WSL](https://learn.microsoft.com/vi-vn/windows/wsl/file-permissions)

Tài liệu tiếng Việt giúp nhập môn, nhưng man-pages và kernel documentation tiếng Anh là nguồn chuẩn khi cần xác minh semantics/errno.

## Verification Metadata

- Last verified: 2026-08-11.
- Versions/scope: Linux man-pages 6.18 where pages expose a release marker; FHS 3.0; generic modern Linux semantics.
- Verification method: official man-pages, Linux Foundation standard and Microsoft WSL documentation.
- Runtime note: examples are POSIX/Linux shell commands. They were reviewed for bounded targets; this Windows workspace did not have an active general-purpose Linux runtime for execution verification.
