using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EduCabinet.Models;

namespace EduCabinet.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, string webRootPath)
        {
            // Ensure database schema is created
            await context.Database.EnsureCreatedAsync();

            // Check if already seeded
            if (await context.Users.AnyAsync())
            {
                return; // DB already has data
            }

            var passwordHasher = new PasswordHasher<User>();

            // 1. Groups
            var group = new StudentGroup { Name = "ИС-21" };
            var group2 = new StudentGroup { Name = "ПИ-31" };
            context.StudentGroups.AddRange(group, group2);
            await context.SaveChangesAsync();

            // 2. Users
            var teacher = new User
            {
                Username = "teacher",
                FullName = "Парван Е.Н. (Преподаватель)",
                Role = UserRoles.Teacher,
                CreatedAt = DateTime.UtcNow
            };
            teacher.PasswordHash = passwordHasher.HashPassword(teacher, "Teacher123!");

            var student1 = new User
            {
                Username = "student1",
                FullName = "Иванов Алексей Сергеевич",
                Role = UserRoles.Student,
                CreatedAt = DateTime.UtcNow
            };
            student1.PasswordHash = passwordHasher.HashPassword(student1, "Student123!");

            var student2 = new User
            {
                Username = "student2",
                FullName = "Смирнова Екатерина Павловна",
                Role = UserRoles.Student,
                CreatedAt = DateTime.UtcNow
            };
            student2.PasswordHash = passwordHasher.HashPassword(student2, "Student123!");

            context.Users.AddRange(teacher, student1, student2);
            await context.SaveChangesAsync();

            // Group memberships
            context.GroupMemberships.AddRange(
                new GroupMembership { StudentId = student1.Id, GroupId = group.Id },
                new GroupMembership { StudentId = student2.Id, GroupId = group.Id }
            );
            await context.SaveChangesAsync();

            // 3. Discipline
            var discipline = new Discipline
            {
                Title = "Безопасность веб-приложений и информационных систем",
                Description = "Изучение принципов защищённой веб-разработки, архитектуры безопасной аутентификации, противодействия уязвимостям OWASP Top 10, настройки серверов и регламентов резервного копирования.",
                CreatedByTeacherId = teacher.Id,
                CreatedAt = DateTime.UtcNow
            };
            context.Disciplines.Add(discipline);
            await context.SaveChangesAsync();

            // 4. Topics
            var topic1 = new Topic
            {
                DisciplineId = discipline.Id,
                Title = "Тема 1. Архитектура безопасности и аутентификация",
                OrderIndex = 1
            };
            var topic2 = new Topic
            {
                DisciplineId = discipline.Id,
                Title = "Тема 2. Защита от уязвимостей и администрирование систем",
                OrderIndex = 2
            };
            context.Topics.AddRange(topic1, topic2);
            await context.SaveChangesAsync();

            // Ensure materials directory exists
            var materialsDir = Path.Combine(webRootPath, "uploads", "materials");
            if (!Directory.Exists(materialsDir))
            {
                Directory.CreateDirectory(materialsDir);
            }

            // 5. Lectures
            var lecture1 = new Lecture
            {
                TopicId = topic1.Id,
                Title = "Лекция 1. Основы аутентификации: криптографическое хеширование паролей и защита сессий",
                Description = "Изучение стандартов хранения паролей (PBKDF2, bcrypt, Argon2), добавления криптографической соли, безопасных cookie-сессий (HttpOnly, Secure, SameSite) и защиты от брутфорса.",
                Content = @"<h4>1. Принципы безопасного хранения паролей</h4>
<p>Хранение паролей в открытом виде (plaintext) либо с использованием быстрых хеш-функций (MD5, SHA-1, SHA-256) является критической уязвимостью. Злоумышленник, получив дамп базы данных, может мгновенно восстановить пароли с помощью предварительно вычисленных <i>радужных таблиц (Rainbow Tables)</i> либо высокоскоростного перебора на GPU.</p>
<p><b>Обязательные требования к хешированию:</b></p>
<ul>
    <li><b>Криптографическая соль (Salt):</b> Случайная последовательность байт уникальная для каждого пользователя. Предотвращает групповой взлом и сводит на нет эффективность радужных таблиц.</li>
    <li><b>Медленные адаптивные алгоритмы:</b> PBKDF2, bcrypt, scrypt, Argon2id. Они содержат параметр итераций (work factor), который увеличивает вычислительную сложность для атакующего.</li>
    <li>В платформе <b>ASP.NET Core</b> используется встроенная реализация <code>PasswordHasher&lt;TUser&gt;</code> на основе <b>PBKDF2-HMAC-SHA256</b> с уникальной 128-битной солью и 100 000 итераций.</li>
</ul>

<h4>2. Защита пользовательских сессий</h4>
<p>При передаче идентификатора сессии через Cookie необходимо активировать флаги защиты:</p>
<ul>
    <li><code>HttpOnly = true</code> — запрещает чтение cookie скриптами JavaScript, блокируя кражу сессии через XSS.</li>
    <li><code>Secure = true</code> — гарантирует отправку cookie только по защищённому каналу HTTPS.</li>
    <li><code>SameSite = SameSiteMode.Lax / Strict</code> — защищает от атак межсайтовой подделки запросов (CSRF).</li>
</ul>",
                OriginalFileName = "lecture1_auth_security.docx",
                FilePath = "/uploads/materials/lecture1_auth_security.docx",
                OrderIndex = 1,
                MinPassingScore = 70
            };

            var lecture2 = new Lecture
            {
                TopicId = topic1.Id,
                Title = "Лекция 2. Уязвимости веб-приложений: SQL-инъекции, XSS и защита на уровне ORM",
                Description = "Анализ векторов атак Injection и Cross-Site Scripting. Роль параметризованных запросов и Entity Framework Core в предотвращении компрометации базы данных.",
                Content = @"<h4>1. Механизм SQL-инъекций и методы защиты</h4>
<p>SQL Injection (SQLi) возникает при динамической конкатенации пользовательского ввода в тело SQL-запроса без валидации и экранирования. Атакующий может внедрить собственные SQL-конструкции (например, <code>' OR '1'='1</code>), обойти аутентификацию или извлечь конфиденциальные данные.</p>
<p><b>Главный метод защиты:</b> использование <i>параметризованных запросов (Prepared Statements)</i> или современных ORM (Entity Framework Core). В EF Core вызовы LINQ типа <code>context.Users.Where(u =&gt; u.Username == input)</code> автоматически транслируются в параметризованный SQL, полностью исключая инъекцию.</p>

<h4>2. Межсайтовый скриптинг (XSS)</h4>
<p>Атака XSS заключается во внедрении вредоносного JavaScript-кода в веб-страницу, просматриваемую другими пользователями. В Razor Pages и MVC шаблонизатор Razor по умолчанию автоматически выполняет <b>HTML-экранирование (HTML Encoding)</b> всех выводимых переменных <code>@Model.Text</code>, нейтрализуя теги <code>&lt;script&gt;</code>.</p>",
                OriginalFileName = "lecture2_sqli_xss.docx",
                FilePath = "/uploads/materials/lecture2_sqli_xss.docx",
                OrderIndex = 2,
                MinPassingScore = 70
            };

            var lecture3 = new Lecture
            {
                TopicId = topic2.Id,
                Title = "Лекция 3. Регламент развёртывания, резервного копирования и мониторинга веб-ресурсов",
                Description = "Внедрение приложения в рабочую среду: настройка веб-сервера Kestrel, изоляция СУБД PostgreSQL, организация автоматического создания дампов pg_dump и регламент восстановления после аварий.",
                Content = @"<h4>1. Архитектура внедрения на компьютер преподавателя</h4>
<p>Развертывание веб-приложения включает следующие этапы:</p>
<ul>
    <li>Сборка проекта в конфигурации <b>Release</b> без отладочных символов для максимального быстродействия.</li>
    <li>Запуск автономного кроссплатформенного веб-сервера <b>Kestrel</b> на выделенном порту.</li>
    <li>Настройка сетевого интерфейса и правила брандмауэра Windows (Firewall) для разрешения входящих TCP-соединений только по локальной сети учебной аудитории.</li>
    <li>Изоляция СУБД: СУБД PostgreSQL слушает локальный интерфейс <code>127.0.0.1:5432</code> и недоступна для прямого подключения студентов из внешней сети.</li>
</ul>

<h4>2. Регламент резервного копирования и восстановления данных</h4>
<p>Для предотвращения потери учебных результатов студентов организуется процедура регулярного резервного копирования:</p>
<ul>
    <li><b>Инструмент:</b> утилита <code>pg_dump</code> для создания целостного дампа схемы и данных базы <code>PR3</code>.</li>
    <li><b>Периодичность:</b> ежедневное создание архива по окончании занятий.</li>
    <li><b>Хранение:</b> принцип 3-2-1 (локальный каталог, резервный съёмный носитель преподавателя, облачное резервное хранилище).</li>
</ul>",
                OriginalFileName = "lecture3_backup_maintenance.docx",
                FilePath = "/uploads/materials/lecture3_backup_maintenance.docx",
                OrderIndex = 3,
                MinPassingScore = 70
            };

            context.Lectures.AddRange(lecture1, lecture2, lecture3);
            await context.SaveChangesAsync();

            // 6. Control Questions for Lecture 1
            var cq1 = new ControlQuestion
            {
                LectureId = lecture1.Id,
                QuestionText = "Какой криптографический элемент добавляется к паролю перед вычислением хеша для защиты от атак по радужным таблицам?",
                QuestionType = "TextAnswer",
                CorrectAnswer = "Соль",
                OrderIndex = 1
            };
            var cq2 = new ControlQuestion
            {
                LectureId = lecture1.Id,
                QuestionText = "Какой флаг Cookie предотвращает чтение сессионного токена через скрипты JavaScript (защита от XSS)?",
                QuestionType = "SingleChoice",
                CorrectAnswer = "HttpOnly",
                OrderIndex = 2
            };
            context.ControlQuestions.AddRange(cq1, cq2);

            // 7. Test for Lecture 1
            var test1 = new Test
            {
                LectureId = lecture1.Id,
                Title = "Контрольное тестирование: Аутентификация, хеширование и сессии",
                PassingScorePercent = 70,
                MaxAttempts = 3,
                TimeLimitMinutes = 15
            };
            context.Tests.Add(test1);
            await context.SaveChangesAsync();

            // Test 1 Questions
            var t1q1 = new TestQuestion
            {
                TestId = test1.Id,
                QuestionText = "Какой из перечисленных алгоритмов хеширования НЕ рекомендуется использовать для хранения паролей?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            var t1q2 = new TestQuestion
            {
                TestId = test1.Id,
                QuestionText = "Для чего в современных алгоритмах хеширования (PBKDF2, bcrypt, Argon2) используется параметр итераций (work factor)?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            var t1q3 = new TestQuestion
            {
                TestId = test1.Id,
                QuestionText = "Какие атрибуты Cookie обеспечивают максимальную безопасность сессионных данных? (выберите все верные)",
                QuestionType = "MultipleChoice",
                Points = 2
            };
            var t1q4 = new TestQuestion
            {
                TestId = test1.Id,
                QuestionText = "Введите название криптографического компонента (на русском языке), уникального для каждого пользователя, который добавляется к паролю перед хешированием:",
                QuestionType = "TextAnswer",
                Points = 1
            };
            var t1q5 = new TestQuestion
            {
                TestId = test1.Id,
                QuestionText = "Установите соответствие между механизмом веб-безопасности и его функциональным назначением:",
                QuestionType = "Matching",
                Points = 2
            };
            context.TestQuestions.AddRange(t1q1, t1q2, t1q3, t1q4, t1q5);
            await context.SaveChangesAsync();

            // Answer options for Test 1
            context.AnswerOptions.AddRange(
                new AnswerOption { QuestionId = t1q1.Id, OptionText = "MD5 (быстрый, небезопасный алгоритм)", IsCorrect = true },
                new AnswerOption { QuestionId = t1q1.Id, OptionText = "PBKDF2", IsCorrect = false },
                new AnswerOption { QuestionId = t1q1.Id, OptionText = "bcrypt", IsCorrect = false },
                new AnswerOption { QuestionId = t1q1.Id, OptionText = "Argon2id", IsCorrect = false },

                new AnswerOption { QuestionId = t1q2.Id, OptionText = "Для искусственного замедления вычисления хеша и усложнения брутфорса на GPU", IsCorrect = true },
                new AnswerOption { QuestionId = t1q2.Id, OptionText = "Для сжатия длины выходного хеша", IsCorrect = false },
                new AnswerOption { QuestionId = t1q2.Id, OptionText = "Для шифрования канала связи", IsCorrect = false },

                new AnswerOption { QuestionId = t1q3.Id, OptionText = "HttpOnly (блокирует кражу через JavaScript)", IsCorrect = true },
                new AnswerOption { QuestionId = t1q3.Id, OptionText = "Secure (передача строго по протоколу HTTPS)", IsCorrect = true },
                new AnswerOption { QuestionId = t1q3.Id, OptionText = "SameSite=Strict или Lax (защита от CSRF)", IsCorrect = true },
                new AnswerOption { QuestionId = t1q3.Id, OptionText = "AllowCrossDomainScripting=true", IsCorrect = false },

                new AnswerOption { QuestionId = t1q4.Id, OptionText = "Соль", IsCorrect = true },

                new AnswerOption { QuestionId = t1q5.Id, OptionText = "Криптографическая соль:::Уникальная случайная строка для защиты от радужных таблиц", IsCorrect = true },
                new AnswerOption { QuestionId = t1q5.Id, OptionText = "Флаг HttpOnly:::Защита сессионных cookie от кражи через XSS-скрипты", IsCorrect = true },
                new AnswerOption { QuestionId = t1q5.Id, OptionText = "Флаг SameSite:::Предотвращение межсайтовой подделки запросов (CSRF)", IsCorrect = true },
                new AnswerOption { QuestionId = t1q5.Id, OptionText = "PBKDF2 / Argon2:::Медленный адаптивный алгоритм с большим числом итераций", IsCorrect = true }
            );

            // 8. Test for Lecture 2
            var test2 = new Test
            {
                LectureId = lecture2.Id,
                Title = "Контрольное тестирование: Защита от SQL-инъекций и XSS",
                PassingScorePercent = 70,
                MaxAttempts = 3,
                TimeLimitMinutes = 15
            };
            context.Tests.Add(test2);
            await context.SaveChangesAsync();

            var t2q1 = new TestQuestion
            {
                TestId = test2.Id,
                QuestionText = "Какой способ построения SQL-запросов является наиболее надёжной защитой от SQL-инъекций?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            var t2q2 = new TestQuestion
            {
                TestId = test2.Id,
                QuestionText = "Какое действие выполняет шаблонизатор Razor по умолчанию при выводе строк в HTML (@Model.Text)?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            var t2q3 = new TestQuestion
            {
                TestId = test2.Id,
                QuestionText = "Каким образом ORM Entity Framework Core защищает приложение от SQL-инъекций?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            context.TestQuestions.AddRange(t2q1, t2q2, t2q3);
            await context.SaveChangesAsync();

            context.AnswerOptions.AddRange(
                new AnswerOption { QuestionId = t2q1.Id, OptionText = "Параметризованные запросы (Prepared Statements)", IsCorrect = true },
                new AnswerOption { QuestionId = t2q1.Id, OptionText = "Конкатенация строк с кавычками", IsCorrect = false },
                new AnswerOption { QuestionId = t2q1.Id, OptionText = "Проверка длины логина", IsCorrect = false },

                new AnswerOption { QuestionId = t2q2.Id, OptionText = "Автоматическое экранирование спецсимволов HTML (&lt;, &gt;, &amp;, &quot;)", IsCorrect = true },
                new AnswerOption { QuestionId = t2q2.Id, OptionText = "Удаление всех гласных букв", IsCorrect = false },
                new AnswerOption { QuestionId = t2q2.Id, OptionText = "Шифрование алгоритмом AES", IsCorrect = false },

                new AnswerOption { QuestionId = t2q3.Id, OptionText = "Автоматическая трансляция LINQ-запросов в параметризованный SQL", IsCorrect = true },
                new AnswerOption { QuestionId = t2q3.Id, OptionText = "Хранение данных исключительно в оперативной памяти", IsCorrect = false },
                new AnswerOption { QuestionId = t2q3.Id, OptionText = "Отключение возможности выполнения запросов", IsCorrect = false }
            );

            // 9. Test for Lecture 3
            var test3 = new Test
            {
                LectureId = lecture3.Id,
                Title = "Контрольное тестирование: Внедрение и регламент резервного копирования",
                PassingScorePercent = 70,
                MaxAttempts = 3,
                TimeLimitMinutes = 15
            };
            context.Tests.Add(test3);
            await context.SaveChangesAsync();

            var t3q1 = new TestQuestion
            {
                TestId = test3.Id,
                QuestionText = "Какая утилита PostgreSQL применяется для создания целостной резервной копии базы данных?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            var t3q2 = new TestQuestion
            {
                TestId = test3.Id,
                QuestionText = "Почему порт СУБД PostgreSQL (5432) не следует открывать для прямого сетевого подключения студентов?",
                QuestionType = "SingleChoice",
                Points = 1
            };
            context.TestQuestions.AddRange(t3q1, t3q2);
            await context.SaveChangesAsync();

            context.AnswerOptions.AddRange(
                new AnswerOption { QuestionId = t3q1.Id, OptionText = "pg_dump", IsCorrect = true },
                new AnswerOption { QuestionId = t3q1.Id, OptionText = "pg_browser", IsCorrect = false },
                new AnswerOption { QuestionId = t3q1.Id, OptionText = "pg_scan", IsCorrect = false },

                new AnswerOption { QuestionId = t3q2.Id, OptionText = "Для соблюдения принципа наименьших привилегий и исключения прямого несанкционированного доступа к данным в обход бизнес-логики приложения", IsCorrect = true },
                new AnswerOption { QuestionId = t3q2.Id, OptionText = "Потому что PostgreSQL не умеет работать по сети", IsCorrect = false }
            );

            // Seed initial student progress:
            // Student 1 has access to Lecture 1 (unlocked)
            context.LectureProgresses.Add(new LectureProgress
            {
                StudentId = student1.Id,
                LectureId = lecture1.Id,
                IsCompleted = false,
                IsUnlockedManually = false,
                UnlockedAt = DateTime.UtcNow
            });

            // Student 2 has completed Lecture 1 with test passed, so Lecture 2 is unlocked!
            context.LectureProgresses.Add(new LectureProgress
            {
                StudentId = student2.Id,
                LectureId = lecture1.Id,
                IsCompleted = true,
                UnlockedAt = DateTime.UtcNow.AddDays(-2),
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            });
            context.LectureProgresses.Add(new LectureProgress
            {
                StudentId = student2.Id,
                LectureId = lecture2.Id,
                IsCompleted = false,
                UnlockedAt = DateTime.UtcNow.AddDays(-1)
            });

            var attemptStudent2 = new TestAttempt
            {
                StudentId = student2.Id,
                TestId = test1.Id,
                AttemptNumber = 1,
                ScorePercent = 100,
                TotalPoints = 5,
                EarnedPoints = 5,
                IsPassed = true,
                CompletedAt = DateTime.UtcNow.AddDays(-1)
            };
            context.TestAttempts.Add(attemptStudent2);

            // Audit log
            context.AuditLogs.Add(new AuditLog
            {
                TeacherId = teacher.Id,
                Action = "SystemInit",
                Details = "Первоначальная инициализация учебной системы и заведение дисциплины «Безопасность веб-приложений и информационных систем»",
                Timestamp = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}
