import os
import docx
from docx.shared import Inches, Pt, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH

materials_dir = r"C:\Users\makar\Desktop\ПР3 Парван\PR3Parvan\EduCabinet\wwwroot\uploads\materials"
os.makedirs(materials_dir, exist_ok=True)

def create_doc(filename, title, subtitle, sections):
    doc = docx.Document()
    
    # Page margins
    sections_doc = doc.sections
    for section in sections_doc:
        section.top_margin = Inches(0.8)
        section.bottom_margin = Inches(0.8)
        section.left_margin = Inches(1.0)
        section.right_margin = Inches(0.8)
        
    # Title
    p_title = doc.add_paragraph()
    p_title.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run_title = p_title.add_run(title)
    run_title.font.name = 'Times New Roman'
    run_title.font.size = Pt(18)
    run_title.font.bold = True
    run_title.font.color.rgb = RGBColor(26, 82, 118)
    
    # Subtitle
    p_sub = doc.add_paragraph()
    p_sub.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run_sub = p_sub.add_run(subtitle)
    run_sub.font.name = 'Times New Roman'
    run_sub.font.size = Pt(13)
    run_sub.font.italic = True
    
    doc.add_paragraph() # spacer
    
    for h, texts in sections:
        h_p = doc.add_paragraph()
        h_run = h_p.add_run(h)
        h_run.font.name = 'Times New Roman'
        h_run.font.size = Pt(14)
        h_run.font.bold = True
        h_run.font.color.rgb = RGBColor(40, 55, 71)
        
        for t in texts:
            p = doc.add_paragraph()
            p.paragraph_format.first_line_indent = Inches(0.5)
            p.paragraph_format.line_spacing = 1.15
            run = p.add_run(t)
            run.font.name = 'Times New Roman'
            run.font.size = Pt(12)
            
    doc.save(os.path.join(materials_dir, filename))
    print(f"Created {filename}")

# Lecture 1
create_doc(
    "lecture1_auth_security.docx",
    "Электронный учебный кабинет: Безопасность веб-приложений",
    "Тема 1. Лекция 1: Основы аутентификации, хеширование паролей и защита сессий",
    [
        ("1. Введение в безопасность аутентификации", [
            "Аутентификация — ключевой рубеж защиты любого информационного ресурса. Компрометация учетных записей открывает атакующему доступ к закрытой информации и бизнес-логике приложения.",
            "При разработке системы особое внимание уделяется хранению паролей пользователей. Категорически запрещено хранение паролей в исходном текстовом виде."
        ]),
        ("2. Использование криптографической соли и PBKDF2", [
            "Для противодействия атакам с использованием радужных таблиц (Rainbow Tables) к каждому паролю перед хешированием добавляется уникальная случайная последовательность — соль (Salt).",
            "В современных платформах, включая ASP.NET Core Identity, применяется стандарт PBKDF2 с алгоритмом HMAC-SHA256 и 100 000 итерациями замедления."
        ]),
        ("3. Безопасность cookie-сессий", [
            "Сессионные маркеры должны быть защищены флагами HttpOnly (блокирует кражу через XSS) и Secure (передача только по HTTPS).",
            "Для защиты от CSRF-атак применяется атрибут SameSite со значениями Lax или Strict."
        ])
    ]
)

# Lecture 2
create_doc(
    "lecture2_sqli_xss.docx",
    "Электронный учебный кабинет: Безопасность веб-приложений",
    "Тема 1. Лекция 2: Уязвимости веб-приложений: SQL-инъекции, XSS и защита на уровне ORM",
    [
        ("1. Механизм SQL-инъекций", [
            "SQL Injection (SQLi) — одна из наиболее опасных уязвимостей согласно классификации OWASP Top 10. Она возникает при некорректной конкатенации строк при формировании запросов к СУБД.",
            "Внедрение произвольного SQL-кода позволяет обходить аутентификацию, читать приватные данные других пользователей и разрушать целостность базы данных."
        ]),
        ("2. Предотвращение инъекций через Entity Framework Core", [
            "Современные объектно-реляционные мапперы (ORM), такие как Entity Framework Core, автоматически используют параметризованные запросы для всех операций LINQ.",
            "Параметры передаются отдельно от текста команды, что делает внедрение вредоносного синтаксиса технически невозможным."
        ]),
        ("3. Защита от межсайтового скриптинга (XSS)", [
            "Атака XSS основана на внедрении скриптов в генерируемый HTML. Движок Razor в ASP.NET Core автоматически экранирует все выводимые значения (@Model.Data), нейтрализуя спецсимволы <, >, &, \"."
        ])
    ]
)

# Lecture 3
create_doc(
    "lecture3_backup_maintenance.docx",
    "Электронный учебный кабинет: Безопасность веб-приложений",
    "Тема 2. Лекция 3: Регламент развёртывания, резервного копирования и мониторинга",
    [
        ("1. Процедура внедрения на компьютер преподавателя", [
            "Приложение публикуется в конфигурации Release, запускается на встроенном веб-сервере Kestrel.",
            "Сетевой экран настраивается на разрешение соединений внутри аудиторной локальной сети. База данных PostgreSQL изолируется на локальном порту 127.0.0.1:5432."
        ]),
        ("2. Регламент резервного копирования базы данных", [
            "Резервное копирование выполняется с помощью штатной утилиты pg_dump. Периодичность — ежедневно в конце учебного дня.",
            "Созданные дампы сохраняются в двух раздельных физических хранилищах (локальный диск и внешний накопитель)."
        ]),
        ("3. Мониторинг и журнал аудита", [
            "Все ключевые операции (авторизация, ручное открытие лекций преподавателем, сброс попыток) регистрируются в таблице AuditLogs для контроля за учебным процессом."
        ])
    ]
)
