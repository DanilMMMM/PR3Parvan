--
-- PostgreSQL database dump
--

\restrict tT0Uge6EWWMgz3NIoqrAY8IHZjq6TtCIeWEs8bfE67fRw7aj4kgBddflelO6Leg

-- Dumped from database version 18.0
-- Dumped by pg_dump version 18.0

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE IF EXISTS ONLY public."Topics" DROP CONSTRAINT IF EXISTS "FK_Topics_Disciplines_DisciplineId";
ALTER TABLE IF EXISTS ONLY public."Tests" DROP CONSTRAINT IF EXISTS "FK_Tests_Lectures_LectureId";
ALTER TABLE IF EXISTS ONLY public."TestQuestions" DROP CONSTRAINT IF EXISTS "FK_TestQuestions_Tests_TestId";
ALTER TABLE IF EXISTS ONLY public."TestAttempts" DROP CONSTRAINT IF EXISTS "FK_TestAttempts_Users_StudentId";
ALTER TABLE IF EXISTS ONLY public."TestAttempts" DROP CONSTRAINT IF EXISTS "FK_TestAttempts_Tests_TestId";
ALTER TABLE IF EXISTS ONLY public."StudentAnswers" DROP CONSTRAINT IF EXISTS "FK_StudentAnswers_TestQuestions_QuestionId";
ALTER TABLE IF EXISTS ONLY public."StudentAnswers" DROP CONSTRAINT IF EXISTS "FK_StudentAnswers_TestAttempts_AttemptId";
ALTER TABLE IF EXISTS ONLY public."Lectures" DROP CONSTRAINT IF EXISTS "FK_Lectures_Topics_TopicId";
ALTER TABLE IF EXISTS ONLY public."LectureProgress" DROP CONSTRAINT IF EXISTS "FK_LectureProgress_Users_StudentId";
ALTER TABLE IF EXISTS ONLY public."LectureProgress" DROP CONSTRAINT IF EXISTS "FK_LectureProgress_Lectures_LectureId";
ALTER TABLE IF EXISTS ONLY public."GroupMemberships" DROP CONSTRAINT IF EXISTS "FK_GroupMemberships_Users_StudentId";
ALTER TABLE IF EXISTS ONLY public."GroupMemberships" DROP CONSTRAINT IF EXISTS "FK_GroupMemberships_StudentGroups_GroupId";
ALTER TABLE IF EXISTS ONLY public."Disciplines" DROP CONSTRAINT IF EXISTS "FK_Disciplines_Users_CreatedByTeacherId";
ALTER TABLE IF EXISTS ONLY public."ControlQuestions" DROP CONSTRAINT IF EXISTS "FK_ControlQuestions_Lectures_LectureId";
ALTER TABLE IF EXISTS ONLY public."AuditLogs" DROP CONSTRAINT IF EXISTS "FK_AuditLogs_Users_TeacherId";
ALTER TABLE IF EXISTS ONLY public."AnswerOptions" DROP CONSTRAINT IF EXISTS "FK_AnswerOptions_TestQuestions_QuestionId";
DROP INDEX IF EXISTS public."IX_Users_Username";
DROP INDEX IF EXISTS public."IX_Topics_DisciplineId";
DROP INDEX IF EXISTS public."IX_Tests_LectureId";
DROP INDEX IF EXISTS public."IX_TestQuestions_TestId";
DROP INDEX IF EXISTS public."IX_TestAttempts_TestId";
DROP INDEX IF EXISTS public."IX_TestAttempts_StudentId";
DROP INDEX IF EXISTS public."IX_StudentAnswers_QuestionId";
DROP INDEX IF EXISTS public."IX_StudentAnswers_AttemptId";
DROP INDEX IF EXISTS public."IX_Lectures_TopicId";
DROP INDEX IF EXISTS public."IX_LectureProgress_StudentId_LectureId";
DROP INDEX IF EXISTS public."IX_LectureProgress_LectureId";
DROP INDEX IF EXISTS public."IX_GroupMemberships_StudentId_GroupId";
DROP INDEX IF EXISTS public."IX_GroupMemberships_GroupId";
DROP INDEX IF EXISTS public."IX_Disciplines_CreatedByTeacherId";
DROP INDEX IF EXISTS public."IX_ControlQuestions_LectureId";
DROP INDEX IF EXISTS public."IX_AuditLogs_TeacherId";
DROP INDEX IF EXISTS public."IX_AnswerOptions_QuestionId";
ALTER TABLE IF EXISTS ONLY public."Users" DROP CONSTRAINT IF EXISTS "PK_Users";
ALTER TABLE IF EXISTS ONLY public."Topics" DROP CONSTRAINT IF EXISTS "PK_Topics";
ALTER TABLE IF EXISTS ONLY public."Tests" DROP CONSTRAINT IF EXISTS "PK_Tests";
ALTER TABLE IF EXISTS ONLY public."TestQuestions" DROP CONSTRAINT IF EXISTS "PK_TestQuestions";
ALTER TABLE IF EXISTS ONLY public."TestAttempts" DROP CONSTRAINT IF EXISTS "PK_TestAttempts";
ALTER TABLE IF EXISTS ONLY public."StudentGroups" DROP CONSTRAINT IF EXISTS "PK_StudentGroups";
ALTER TABLE IF EXISTS ONLY public."StudentAnswers" DROP CONSTRAINT IF EXISTS "PK_StudentAnswers";
ALTER TABLE IF EXISTS ONLY public."Lectures" DROP CONSTRAINT IF EXISTS "PK_Lectures";
ALTER TABLE IF EXISTS ONLY public."LectureProgress" DROP CONSTRAINT IF EXISTS "PK_LectureProgress";
ALTER TABLE IF EXISTS ONLY public."GroupMemberships" DROP CONSTRAINT IF EXISTS "PK_GroupMemberships";
ALTER TABLE IF EXISTS ONLY public."Disciplines" DROP CONSTRAINT IF EXISTS "PK_Disciplines";
ALTER TABLE IF EXISTS ONLY public."ControlQuestions" DROP CONSTRAINT IF EXISTS "PK_ControlQuestions";
ALTER TABLE IF EXISTS ONLY public."AuditLogs" DROP CONSTRAINT IF EXISTS "PK_AuditLogs";
ALTER TABLE IF EXISTS ONLY public."AnswerOptions" DROP CONSTRAINT IF EXISTS "PK_AnswerOptions";
DROP TABLE IF EXISTS public."Users";
DROP TABLE IF EXISTS public."Topics";
DROP TABLE IF EXISTS public."Tests";
DROP TABLE IF EXISTS public."TestQuestions";
DROP TABLE IF EXISTS public."TestAttempts";
DROP TABLE IF EXISTS public."StudentGroups";
DROP TABLE IF EXISTS public."StudentAnswers";
DROP TABLE IF EXISTS public."Lectures";
DROP TABLE IF EXISTS public."LectureProgress";
DROP TABLE IF EXISTS public."GroupMemberships";
DROP TABLE IF EXISTS public."Disciplines";
DROP TABLE IF EXISTS public."ControlQuestions";
DROP TABLE IF EXISTS public."AuditLogs";
DROP TABLE IF EXISTS public."AnswerOptions";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AnswerOptions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AnswerOptions" (
    "Id" integer NOT NULL,
    "QuestionId" integer NOT NULL,
    "OptionText" text NOT NULL,
    "IsCorrect" boolean NOT NULL
);


ALTER TABLE public."AnswerOptions" OWNER TO postgres;

--
-- Name: AnswerOptions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AnswerOptions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AnswerOptions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AuditLogs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AuditLogs" (
    "Id" integer NOT NULL,
    "TeacherId" integer,
    "Action" character varying(100) NOT NULL,
    "Details" character varying(1000) NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL
);


ALTER TABLE public."AuditLogs" OWNER TO postgres;

--
-- Name: AuditLogs_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."AuditLogs" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AuditLogs_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: ControlQuestions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."ControlQuestions" (
    "Id" integer NOT NULL,
    "LectureId" integer NOT NULL,
    "QuestionText" text NOT NULL,
    "QuestionType" character varying(50) NOT NULL,
    "CorrectAnswer" text NOT NULL,
    "OrderIndex" integer NOT NULL
);


ALTER TABLE public."ControlQuestions" OWNER TO postgres;

--
-- Name: ControlQuestions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."ControlQuestions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."ControlQuestions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Disciplines; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Disciplines" (
    "Id" integer NOT NULL,
    "Title" character varying(200) NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "CreatedByTeacherId" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Disciplines" OWNER TO postgres;

--
-- Name: Disciplines_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Disciplines" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Disciplines_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: GroupMemberships; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."GroupMemberships" (
    "Id" integer NOT NULL,
    "StudentId" integer NOT NULL,
    "GroupId" integer NOT NULL
);


ALTER TABLE public."GroupMemberships" OWNER TO postgres;

--
-- Name: GroupMemberships_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."GroupMemberships" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."GroupMemberships_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: LectureProgress; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."LectureProgress" (
    "Id" integer NOT NULL,
    "StudentId" integer NOT NULL,
    "LectureId" integer NOT NULL,
    "IsCompleted" boolean NOT NULL,
    "IsUnlockedManually" boolean NOT NULL,
    "UnlockedAt" timestamp with time zone,
    "CompletedAt" timestamp with time zone
);


ALTER TABLE public."LectureProgress" OWNER TO postgres;

--
-- Name: LectureProgress_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."LectureProgress" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."LectureProgress_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Lectures; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Lectures" (
    "Id" integer NOT NULL,
    "TopicId" integer NOT NULL,
    "Title" character varying(250) NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "Content" text NOT NULL,
    "FilePath" character varying(500),
    "OriginalFileName" character varying(200),
    "OrderIndex" integer NOT NULL,
    "MinPassingScore" integer NOT NULL
);


ALTER TABLE public."Lectures" OWNER TO postgres;

--
-- Name: Lectures_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Lectures" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Lectures_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: StudentAnswers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."StudentAnswers" (
    "Id" integer NOT NULL,
    "AttemptId" integer NOT NULL,
    "QuestionId" integer NOT NULL,
    "SelectedOptionIdsJson" text,
    "TextAnswer" text,
    "IsCorrect" boolean NOT NULL
);


ALTER TABLE public."StudentAnswers" OWNER TO postgres;

--
-- Name: StudentAnswers_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."StudentAnswers" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."StudentAnswers_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: StudentGroups; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."StudentGroups" (
    "Id" integer NOT NULL,
    "Name" character varying(100) NOT NULL
);


ALTER TABLE public."StudentGroups" OWNER TO postgres;

--
-- Name: StudentGroups_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."StudentGroups" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."StudentGroups_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: TestAttempts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestAttempts" (
    "Id" integer NOT NULL,
    "StudentId" integer NOT NULL,
    "TestId" integer NOT NULL,
    "AttemptNumber" integer NOT NULL,
    "ScorePercent" integer NOT NULL,
    "TotalPoints" integer NOT NULL,
    "EarnedPoints" integer NOT NULL,
    "IsPassed" boolean NOT NULL,
    "CompletedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."TestAttempts" OWNER TO postgres;

--
-- Name: TestAttempts_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TestAttempts" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."TestAttempts_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: TestQuestions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."TestQuestions" (
    "Id" integer NOT NULL,
    "TestId" integer NOT NULL,
    "QuestionText" text NOT NULL,
    "QuestionType" character varying(50) NOT NULL,
    "Points" integer NOT NULL
);


ALTER TABLE public."TestQuestions" OWNER TO postgres;

--
-- Name: TestQuestions_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."TestQuestions" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."TestQuestions_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Tests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Tests" (
    "Id" integer NOT NULL,
    "LectureId" integer NOT NULL,
    "Title" character varying(200) NOT NULL,
    "PassingScorePercent" integer NOT NULL,
    "MaxAttempts" integer NOT NULL,
    "TimeLimitMinutes" integer NOT NULL
);


ALTER TABLE public."Tests" OWNER TO postgres;

--
-- Name: Tests_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Tests" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Tests_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Topics; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Topics" (
    "Id" integer NOT NULL,
    "DisciplineId" integer NOT NULL,
    "Title" character varying(200) NOT NULL,
    "OrderIndex" integer NOT NULL
);


ALTER TABLE public."Topics" OWNER TO postgres;

--
-- Name: Topics_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Topics" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Topics_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: Users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Users" (
    "Id" integer NOT NULL,
    "Username" character varying(100) NOT NULL,
    "PasswordHash" text NOT NULL,
    "FullName" character varying(150) NOT NULL,
    "Role" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL
);


ALTER TABLE public."Users" OWNER TO postgres;

--
-- Name: Users_Id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public."Users" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."Users_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Data for Name: AnswerOptions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AnswerOptions" ("Id", "QuestionId", "OptionText", "IsCorrect") FROM stdin;
1	1	MD5 (быстрый, небезопасный алгоритм)	t
2	1	PBKDF2	f
3	1	bcrypt	f
4	1	Argon2id	f
5	2	Для искусственного замедления вычисления хеша и усложнения брутфорса на GPU	t
6	2	Для сжатия длины выходного хеша	f
7	2	Для шифрования канала связи	f
8	3	HttpOnly (блокирует кражу через JavaScript)	t
9	3	Secure (передача строго по протоколу HTTPS)	t
10	3	SameSite=Strict или Lax (защита от CSRF)	t
11	3	AllowCrossDomainScripting=true	f
12	5	Параметризованные запросы (Prepared Statements)	t
13	5	Конкатенация строк с кавычками	f
14	5	Проверка длины логина	f
15	6	Автоматическое экранирование спецсимволов HTML (&lt;, &gt;, &amp;, &quot;)	t
16	6	Удаление всех гласных букв	f
17	6	Шифрование алгоритмом AES	f
18	7	Автоматическая трансляция LINQ-запросов в параметризованный SQL	t
19	7	Хранение данных исключительно в оперативной памяти	f
20	7	Отключение возможности выполнения запросов	f
21	8	pg_dump	t
22	8	pg_browser	f
23	8	pg_scan	f
24	9	Для соблюдения принципа наименьших привилегий и исключения прямого несанкционированного доступа к данным в обход бизнес-логики приложения	t
25	9	Потому что PostgreSQL не умеет работать по сети	f
26	4	Соль	t
27	10	8	t
28	10	88	f
29	10	4	f
30	10	2	f
31	11	Криптографическая соль:::Уникальная случайная строка для защиты от радужных таблиц	t
32	11	Флаг HttpOnly:::Защита сессионных cookie от кражи через XSS-скрипты	t
33	11	Флаг SameSite:::Предотвращение межсайтовой подделки запросов (CSRF)	t
34	11	PBKDF2 / Argon2:::Медленный адаптивный алгоритм с большим числом итераций	t
\.


--
-- Data for Name: AuditLogs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AuditLogs" ("Id", "TeacherId", "Action", "Details", "Timestamp") FROM stdin;
1	1	SystemInit	Первоначальная инициализация учебной системы и заведение дисциплины «Безопасность веб-приложений и информационных систем»	2026-10-18 17:51:34.697805+03
2	1	ResetAttempts	Преподаватель сбросил попытки прохождения теста «Контрольное тестирование: Аутентификация, хеширование и сессии» для студента Смирнова Екатерина Павловна (Id=3)	2026-10-18 18:24:47.952134+03
3	1	ManualUnlock	Преподаватель открыл доступ к лекции «Лекция 3. Регламент развёртывания, резервного копирования и мониторинга веб-ресурсов» (Id=3) студенту Смирнова Екатерина Павловна (Id=3). Причина: Ручное открытие преподавателем	2026-10-18 18:24:51.856613+03
4	1	ManualUnlock	Преподаватель открыл доступ к лекции «Лекция 3. Регламент развёртывания, резервного копирования и мониторинга веб-ресурсов» (Id=3) студенту Макаренко Данил Александрович (Id=4). Причина: Ручное открытие преподавателем	2026-10-18 18:24:58.943473+03
5	1	ManualUnlock	Преподаватель открыл доступ к лекции «Лекция 2. Уязвимости веб-приложений: SQL-инъекции, XSS и защита на уровне ORM» (Id=2) студенту Макаренко Данил Александрович (Id=4). Причина: Ручное открытие преподавателем	2026-10-18 18:25:00.080406+03
6	1	ManualUnlock	Преподаватель открыл доступ к лекции «Лекция 1. Основы аутентификации: криптографическое хеширование паролей и защита сессий» (Id=1) студенту Макаренко Данил Александрович (Id=4). Причина: Ручное открытие преподавателем	2026-10-18 18:25:01.177606+03
7	1	ResetAttempts	Преподаватель сбросил попытки прохождения теста «Контрольное тестирование: Аутентификация, хеширование и сессии» для студента Макаренко Данил Александрович (Id=4)	2026-10-18 18:25:02.945664+03
8	1	CreateDiscipline	Создана новая дисциплина «Физ культура» (Id=2)	2026-10-20 09:26:07.340743+03
9	1	CreateLecture	Создана лекция «ввввв» (Id=5)	2026-10-20 09:30:50.789062+03
10	1	EditLecture	Преподаватель отредактировал лекцию «ввввв» (Id=5)	2026-10-20 09:31:08.858211+03
11	1	CreateTest	Создан тест «Тестирование к лекции «67»» (Id=4)	2026-10-20 09:31:27.539132+03
12	1	EditLecture	Преподаватель отредактировал лекцию «ввввв» (Id=5)	2026-10-20 09:33:06.278883+03
13	1	AddTestQuestion	Добавлен вопрос в тест «Тестирование к лекции «67»» (вопрос: 4+4)	2026-10-20 10:52:09.607903+03
14	1	ResetAttempts	Преподаватель сбросил попытки прохождения теста «Контрольное тестирование: Аутентификация, хеширование и сессии» для студента Макаренко Данил Александрович (Id=6)	2026-10-20 10:56:21.035321+03
15	1	ResetAttempts	Преподаватель сбросил попытки прохождения теста «Контрольное тестирование: Защита от SQL-инъекций и XSS» для студента Danil Makarenko (Id=7)	2026-10-27 10:08:09.810734+02
16	1	CreateDiscipline	Создана новая дисциплина «аыавыа» (Id=4)	2026-10-27 11:08:04.721311+02
\.


--
-- Data for Name: ControlQuestions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."ControlQuestions" ("Id", "LectureId", "QuestionText", "QuestionType", "CorrectAnswer", "OrderIndex") FROM stdin;
1	1	Какой криптографический элемент добавляется к паролю перед вычислением хеша для защиты от атак по радужным таблицам?	TextAnswer	Соль	1
2	1	Какой флаг Cookie предотвращает чтение сессионного токена через скрипты JavaScript (защита от XSS)?	SingleChoice	HttpOnly	2
3	5	2+2	TextAnswer	4	1
\.


--
-- Data for Name: Disciplines; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Disciplines" ("Id", "Title", "Description", "CreatedByTeacherId", "CreatedAt") FROM stdin;
1	Безопасность веб-приложений и информационных систем	Изучение принципов защищённой веб-разработки, архитектуры безопасной аутентификации, противодействия уязвимостям OWASP Top 10, настройки серверов и регламентов резервного копирования.	1	2026-10-18 17:51:34.362196+03
2	Физ культура	Прыжки	1	2026-10-20 09:26:07.137008+03
4	аыавыа	аавава	1	2026-10-27 11:08:04.543256+02
\.


--
-- Data for Name: GroupMemberships; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."GroupMemberships" ("Id", "StudentId", "GroupId") FROM stdin;
1	2	1
2	3	1
3	4	1
4	6	1
5	7	1
6	8	1
\.


--
-- Data for Name: LectureProgress; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."LectureProgress" ("Id", "StudentId", "LectureId", "IsCompleted", "IsUnlockedManually", "UnlockedAt", "CompletedAt") FROM stdin;
2	3	1	t	f	2026-10-16 17:51:34.669067+03	2026-10-17 17:51:34.669098+03
3	3	2	f	f	2026-10-17 17:51:34.66978+03	\N
1	2	1	t	f	2026-10-18 17:51:34.642106+03	2026-10-18 18:23:24.861439+03
5	2	2	t	f	2026-10-18 18:23:35.504173+03	2026-10-18 18:23:49.8442+03
6	2	3	t	f	2026-10-18 18:23:51.036965+03	2026-10-18 18:24:01.12988+03
7	3	3	f	t	2026-10-18 18:24:51.849232+03	\N
8	4	3	f	t	2026-10-18 18:24:58.941488+03	\N
9	4	2	f	t	2026-10-18 18:25:00.078463+03	\N
4	4	1	f	t	2026-10-18 18:25:01.175753+03	\N
10	2	5	t	f	2026-10-20 09:31:46.854365+03	2026-10-20 10:52:44.840371+03
11	6	1	t	f	2026-10-20 10:53:57.139322+03	2026-10-20 10:55:37.469592+03
12	7	1	t	f	2026-10-27 10:03:27.088296+02	2026-10-27 10:05:35.657714+02
13	7	2	f	f	2026-10-27 10:05:53.255286+02	\N
14	8	1	t	f	2026-10-27 11:11:27.176623+02	2026-10-27 11:12:11.230134+02
15	8	2	f	f	2026-10-27 11:12:13.891988+02	\N
\.


--
-- Data for Name: Lectures; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Lectures" ("Id", "TopicId", "Title", "Description", "Content", "FilePath", "OriginalFileName", "OrderIndex", "MinPassingScore") FROM stdin;
1	1	Лекция 1. Основы аутентификации: криптографическое хеширование паролей и защита сессий	Изучение стандартов хранения паролей (PBKDF2, bcrypt, Argon2), добавления криптографической соли, безопасных cookie-сессий (HttpOnly, Secure, SameSite) и защиты от брутфорса.	<h4>1. Принципы безопасного хранения паролей</h4>\n<p>Хранение паролей в открытом виде (plaintext) либо с использованием быстрых хеш-функций (MD5, SHA-1, SHA-256) является критической уязвимостью. Злоумышленник, получив дамп базы данных, может мгновенно восстановить пароли с помощью предварительно вычисленных <i>радужных таблиц (Rainbow Tables)</i> либо высокоскоростного перебора на GPU.</p>\n<p><b>Обязательные требования к хешированию:</b></p>\n<ul>\n    <li><b>Криптографическая соль (Salt):</b> Случайная последовательность байт уникальная для каждого пользователя. Предотвращает групповой взлом и сводит на нет эффективность радужных таблиц.</li>\n    <li><b>Медленные адаптивные алгоритмы:</b> PBKDF2, bcrypt, scrypt, Argon2id. Они содержат параметр итераций (work factor), который увеличивает вычислительную сложность для атакующего.</li>\n    <li>В платформе <b>ASP.NET Core</b> используется встроенная реализация <code>PasswordHasher&lt;TUser&gt;</code> на основе <b>PBKDF2-HMAC-SHA256</b> с уникальной 128-битной солью и 100 000 итераций.</li>\n</ul>\n\n<h4>2. Защита пользовательских сессий</h4>\n<p>При передаче идентификатора сессии через Cookie необходимо активировать флаги защиты:</p>\n<ul>\n    <li><code>HttpOnly = true</code> — запрещает чтение cookie скриптами JavaScript, блокируя кражу сессии через XSS.</li>\n    <li><code>Secure = true</code> — гарантирует отправку cookie только по защищённому каналу HTTPS.</li>\n    <li><code>SameSite = SameSiteMode.Lax / Strict</code> — защищает от атак межсайтовой подделки запросов (CSRF).</li>\n</ul>	/uploads/materials/lecture1_auth_security.docx	lecture1_auth_security.docx	1	70
2	1	Лекция 2. Уязвимости веб-приложений: SQL-инъекции, XSS и защита на уровне ORM	Анализ векторов атак Injection и Cross-Site Scripting. Роль параметризованных запросов и Entity Framework Core в предотвращении компрометации базы данных.	<h4>1. Механизм SQL-инъекций и методы защиты</h4>\n<p>SQL Injection (SQLi) возникает при динамической конкатенации пользовательского ввода в тело SQL-запроса без валидации и экранирования. Атакующий может внедрить собственные SQL-конструкции (например, <code>' OR '1'='1</code>), обойти аутентификацию или извлечь конфиденциальные данные.</p>\n<p><b>Главный метод защиты:</b> использование <i>параметризованных запросов (Prepared Statements)</i> или современных ORM (Entity Framework Core). В EF Core вызовы LINQ типа <code>context.Users.Where(u =&gt; u.Username == input)</code> автоматически транслируются в параметризованный SQL, полностью исключая инъекцию.</p>\n\n<h4>2. Межсайтовый скриптинг (XSS)</h4>\n<p>Атака XSS заключается во внедрении вредоносного JavaScript-кода в веб-страницу, просматриваемую другими пользователями. В Razor Pages и MVC шаблонизатор Razor по умолчанию автоматически выполняет <b>HTML-экранирование (HTML Encoding)</b> всех выводимых переменных <code>@Model.Text</code>, нейтрализуя теги <code>&lt;script&gt;</code>.</p>	/uploads/materials/lecture2_sqli_xss.docx	lecture2_sqli_xss.docx	2	70
3	2	Лекция 3. Регламент развёртывания, резервного копирования и мониторинга веб-ресурсов	Внедрение приложения в рабочую среду: настройка веб-сервера Kestrel, изоляция СУБД PostgreSQL, организация автоматического создания дампов pg_dump и регламент восстановления после аварий.	<h4>1. Архитектура внедрения на компьютер преподавателя</h4>\n<p>Развертывание веб-приложения включает следующие этапы:</p>\n<ul>\n    <li>Сборка проекта в конфигурации <b>Release</b> без отладочных символов для максимального быстродействия.</li>\n    <li>Запуск автономного кроссплатформенного веб-сервера <b>Kestrel</b> на выделенном порту.</li>\n    <li>Настройка сетевого интерфейса и правила брандмауэра Windows (Firewall) для разрешения входящих TCP-соединений только по локальной сети учебной аудитории.</li>\n    <li>Изоляция СУБД: СУБД PostgreSQL слушает локальный интерфейс <code>127.0.0.1:5432</code> и недоступна для прямого подключения студентов из внешней сети.</li>\n</ul>\n\n<h4>2. Регламент резервного копирования и восстановления данных</h4>\n<p>Для предотвращения потери учебных результатов студентов организуется процедура регулярного резервного копирования:</p>\n<ul>\n    <li><b>Инструмент:</b> утилита <code>pg_dump</code> для создания целостного дампа схемы и данных базы <code>PR3</code>.</li>\n    <li><b>Периодичность:</b> ежедневное создание архива по окончании занятий.</li>\n    <li><b>Хранение:</b> принцип 3-2-1 (локальный каталог, резервный съёмный носитель преподавателя, облачное резервное хранилище).</li>\n</ul>	/uploads/materials/lecture3_backup_maintenance.docx	lecture3_backup_maintenance.docx	3	70
5	3	ввввв	вв	ввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввввв	/uploads/materials/fbdeb46a9c2a4a7f962bb2e5006cb86e.docx	Ответственность_за_преступления_против_несовершеннолетних.docx	1	70
\.


--
-- Data for Name: StudentAnswers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."StudentAnswers" ("Id", "AttemptId", "QuestionId", "SelectedOptionIdsJson", "TextAnswer", "IsCorrect") FROM stdin;
9	4	1	[1]	\N	t
10	4	2	[5]	\N	t
11	4	3	[8,9,10]	\N	t
12	4	4	\N	Соль	t
13	5	5	[12]	\N	t
14	5	6	[15]	\N	t
15	5	7	[18]	\N	t
16	6	8	[21]	\N	t
17	6	9	[24]	\N	t
18	8	10	[27]	\N	t
19	9	10	[28]	\N	f
24	11	1	[1]	\N	t
25	11	2	[5]	\N	t
26	11	3	[8,9,10]	\N	t
27	11	4	\N	соль	t
31	13	1	[1]	\N	t
32	13	2	[5]	\N	t
33	13	3	[8,9,10]	\N	t
34	13	4	\N	соль	t
35	14	5	[13]	\N	f
36	14	6	[16]	\N	f
37	14	7	[19]	\N	f
\.


--
-- Data for Name: StudentGroups; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."StudentGroups" ("Id", "Name") FROM stdin;
1	ИС-21
2	ПИ-31
\.


--
-- Data for Name: TestAttempts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TestAttempts" ("Id", "StudentId", "TestId", "AttemptNumber", "ScorePercent", "TotalPoints", "EarnedPoints", "IsPassed", "CompletedAt") FROM stdin;
4	2	1	1	100	5	5	t	2026-10-18 18:23:24.551066+03
5	2	2	1	100	3	3	t	2026-10-18 18:23:49.835637+03
6	2	3	1	100	2	2	t	2026-10-18 18:24:01.122385+03
7	2	4	1	0	0	0	f	2026-10-20 09:32:10.190372+03
8	2	4	2	100	1	1	t	2026-10-20 10:52:44.747517+03
9	2	4	3	0	1	0	f	2026-10-20 10:52:52.87955+03
11	7	1	1	100	5	5	t	2026-10-27 10:05:35.527154+02
13	8	1	1	100	5	5	t	2026-10-27 11:12:11.107805+02
14	8	2	1	0	3	0	f	2026-10-27 11:12:24.821254+02
\.


--
-- Data for Name: TestQuestions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."TestQuestions" ("Id", "TestId", "QuestionText", "QuestionType", "Points") FROM stdin;
1	1	Какой из перечисленных алгоритмов хеширования НЕ рекомендуется использовать для хранения паролей?	SingleChoice	1
2	1	Для чего в современных алгоритмах хеширования (PBKDF2, bcrypt, Argon2) используется параметр итераций (work factor)?	SingleChoice	1
3	1	Какие атрибуты Cookie обеспечивают максимальную безопасность сессионных данных? (выберите все верные)	MultipleChoice	2
4	1	Введите название криптографического компонента (на русском языке), уникального для каждого пользователя, который добавляется к паролю перед хешированием:	TextAnswer	1
5	2	Какой способ построения SQL-запросов является наиболее надёжной защитой от SQL-инъекций?	SingleChoice	1
6	2	Какое действие выполняет шаблонизатор Razor по умолчанию при выводе строк в HTML (@Model.Text)?	SingleChoice	1
7	2	Каким образом ORM Entity Framework Core защищает приложение от SQL-инъекций?	SingleChoice	1
8	3	Какая утилита PostgreSQL применяется для создания целостной резервной копии базы данных?	SingleChoice	1
9	3	Почему порт СУБД PostgreSQL (5432) не следует открывать для прямого сетевого подключения студентов?	SingleChoice	1
10	4	4+4	SingleChoice	1
11	1	Установите соответствие между механизмом веб-безопасности и его функциональным назначением:	Matching	2
\.


--
-- Data for Name: Tests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Tests" ("Id", "LectureId", "Title", "PassingScorePercent", "MaxAttempts", "TimeLimitMinutes") FROM stdin;
1	1	Контрольное тестирование: Аутентификация, хеширование и сессии	70	3	15
2	2	Контрольное тестирование: Защита от SQL-инъекций и XSS	70	3	15
3	3	Контрольное тестирование: Внедрение и регламент резервного копирования	70	3	15
4	5	Тестирование к лекции «67»	70	3	15
\.


--
-- Data for Name: Topics; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Topics" ("Id", "DisciplineId", "Title", "OrderIndex") FROM stdin;
1	1	Тема 1. Архитектура безопасности и аутентификация	1
2	1	Тема 2. Защита от уязвимостей и администрирование систем	2
3	2	Спринт	1
4	4	ТАмв 4	1
\.


--
-- Data for Name: Users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Users" ("Id", "Username", "PasswordHash", "FullName", "Role", "CreatedAt") FROM stdin;
2	student1	AQAAAAIAAYagAAAAEFbJe/bFGsXDrXYFoN/CSqtcNcb2kM9J0kWtYWtpndfEfXJvO2MG6CSt16IvdlIJyQ==	Иванов Алексей Сергеевич	Student	2026-10-18 17:51:33.872305+03
3	student2	AQAAAAIAAYagAAAAEHHwX7lsjpMuPAIwV7vXgINH8IgrX5ujLr1hM2c7kQdwduRaiW18fN8dI5ru5Tl0MQ==	Смирнова Екатерина Павловна	Student	2026-10-18 17:51:33.9734+03
4	Danil	AQAAAAIAAYagAAAAEGXaQofg06EZLblvMBRGvl70+as0faHkNUM0X0cuANEWbFgK99Ok11gNkcXfCkZIfQ==	Макаренко Данил Александрович	Student	2026-10-18 18:03:41.558001+03
5	Danil1	AQAAAAIAAYagAAAAEAEO276fatR33nd+5gX8JNvRIqmO7ul2mFXp/ncMqPCf20PfxqDf3YG6eXXwgGRLlw==	Danil Makarenko	Teacher	2026-10-18 18:06:34.075091+03
1	teacher	AQAAAAIAAYagAAAAENcLkmD6AaQaRH2aoR2z/3EDIPkupVGhZs8UVQngipplWtlyoL4pop2i4M40+Ktsxw==	Парван Е.Н.	Teacher	2026-10-18 17:51:33.765703+03
6	Danil111	AQAAAAIAAYagAAAAEE0pn3bJoNAD8v/c+k5M7uZQDzyZhM+68gGWWM32uldMIdVzuMEQZo0SQcGBZTDV/A==	Макаренко Данил Александрович	Student	2026-10-20 10:53:32.258138+03
7	Dan	AQAAAAIAAYagAAAAEBSt/dx64jID+MnN5i2hvpPiSOL3bV/TKJ8uIzoNTAic50DOXRac3o1PqGMbKG/3/Q==	Danil Makarenko	Student	2026-10-27 10:02:41.445839+02
8	Dan100	AQAAAAIAAYagAAAAEFlrVLTeSsMRN14CD1PjhEI+swL7EO15fT6TjI6MPuP1XQN0GGXxa+gL/JwbVub+Ig==	Данил макаренко	Student	2026-10-27 11:10:59.961473+02
\.


--
-- Name: AnswerOptions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AnswerOptions_Id_seq"', 34, true);


--
-- Name: AuditLogs_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."AuditLogs_Id_seq"', 16, true);


--
-- Name: ControlQuestions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."ControlQuestions_Id_seq"', 3, true);


--
-- Name: Disciplines_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Disciplines_Id_seq"', 4, true);


--
-- Name: GroupMemberships_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."GroupMemberships_Id_seq"', 6, true);


--
-- Name: LectureProgress_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."LectureProgress_Id_seq"', 15, true);


--
-- Name: Lectures_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Lectures_Id_seq"', 6, true);


--
-- Name: StudentAnswers_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."StudentAnswers_Id_seq"', 37, true);


--
-- Name: StudentGroups_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."StudentGroups_Id_seq"', 2, true);


--
-- Name: TestAttempts_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."TestAttempts_Id_seq"', 14, true);


--
-- Name: TestQuestions_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."TestQuestions_Id_seq"', 11, true);


--
-- Name: Tests_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Tests_Id_seq"', 4, true);


--
-- Name: Topics_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Topics_Id_seq"', 4, true);


--
-- Name: Users_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public."Users_Id_seq"', 8, true);


--
-- Name: AnswerOptions PK_AnswerOptions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswerOptions"
    ADD CONSTRAINT "PK_AnswerOptions" PRIMARY KEY ("Id");


--
-- Name: AuditLogs PK_AuditLogs; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id");


--
-- Name: ControlQuestions PK_ControlQuestions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ControlQuestions"
    ADD CONSTRAINT "PK_ControlQuestions" PRIMARY KEY ("Id");


--
-- Name: Disciplines PK_Disciplines; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Disciplines"
    ADD CONSTRAINT "PK_Disciplines" PRIMARY KEY ("Id");


--
-- Name: GroupMemberships PK_GroupMemberships; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GroupMemberships"
    ADD CONSTRAINT "PK_GroupMemberships" PRIMARY KEY ("Id");


--
-- Name: LectureProgress PK_LectureProgress; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LectureProgress"
    ADD CONSTRAINT "PK_LectureProgress" PRIMARY KEY ("Id");


--
-- Name: Lectures PK_Lectures; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Lectures"
    ADD CONSTRAINT "PK_Lectures" PRIMARY KEY ("Id");


--
-- Name: StudentAnswers PK_StudentAnswers; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."StudentAnswers"
    ADD CONSTRAINT "PK_StudentAnswers" PRIMARY KEY ("Id");


--
-- Name: StudentGroups PK_StudentGroups; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."StudentGroups"
    ADD CONSTRAINT "PK_StudentGroups" PRIMARY KEY ("Id");


--
-- Name: TestAttempts PK_TestAttempts; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAttempts"
    ADD CONSTRAINT "PK_TestAttempts" PRIMARY KEY ("Id");


--
-- Name: TestQuestions PK_TestQuestions; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestQuestions"
    ADD CONSTRAINT "PK_TestQuestions" PRIMARY KEY ("Id");


--
-- Name: Tests PK_Tests; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Tests"
    ADD CONSTRAINT "PK_Tests" PRIMARY KEY ("Id");


--
-- Name: Topics PK_Topics; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Topics"
    ADD CONSTRAINT "PK_Topics" PRIMARY KEY ("Id");


--
-- Name: Users PK_Users; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");


--
-- Name: IX_AnswerOptions_QuestionId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AnswerOptions_QuestionId" ON public."AnswerOptions" USING btree ("QuestionId");


--
-- Name: IX_AuditLogs_TeacherId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_AuditLogs_TeacherId" ON public."AuditLogs" USING btree ("TeacherId");


--
-- Name: IX_ControlQuestions_LectureId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_ControlQuestions_LectureId" ON public."ControlQuestions" USING btree ("LectureId");


--
-- Name: IX_Disciplines_CreatedByTeacherId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Disciplines_CreatedByTeacherId" ON public."Disciplines" USING btree ("CreatedByTeacherId");


--
-- Name: IX_GroupMemberships_GroupId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_GroupMemberships_GroupId" ON public."GroupMemberships" USING btree ("GroupId");


--
-- Name: IX_GroupMemberships_StudentId_GroupId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_GroupMemberships_StudentId_GroupId" ON public."GroupMemberships" USING btree ("StudentId", "GroupId");


--
-- Name: IX_LectureProgress_LectureId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_LectureProgress_LectureId" ON public."LectureProgress" USING btree ("LectureId");


--
-- Name: IX_LectureProgress_StudentId_LectureId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_LectureProgress_StudentId_LectureId" ON public."LectureProgress" USING btree ("StudentId", "LectureId");


--
-- Name: IX_Lectures_TopicId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Lectures_TopicId" ON public."Lectures" USING btree ("TopicId");


--
-- Name: IX_StudentAnswers_AttemptId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_StudentAnswers_AttemptId" ON public."StudentAnswers" USING btree ("AttemptId");


--
-- Name: IX_StudentAnswers_QuestionId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_StudentAnswers_QuestionId" ON public."StudentAnswers" USING btree ("QuestionId");


--
-- Name: IX_TestAttempts_StudentId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_TestAttempts_StudentId" ON public."TestAttempts" USING btree ("StudentId");


--
-- Name: IX_TestAttempts_TestId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_TestAttempts_TestId" ON public."TestAttempts" USING btree ("TestId");


--
-- Name: IX_TestQuestions_TestId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_TestQuestions_TestId" ON public."TestQuestions" USING btree ("TestId");


--
-- Name: IX_Tests_LectureId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Tests_LectureId" ON public."Tests" USING btree ("LectureId");


--
-- Name: IX_Topics_DisciplineId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Topics_DisciplineId" ON public."Topics" USING btree ("DisciplineId");


--
-- Name: IX_Users_Username; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX "IX_Users_Username" ON public."Users" USING btree ("Username");


--
-- Name: AnswerOptions FK_AnswerOptions_TestQuestions_QuestionId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AnswerOptions"
    ADD CONSTRAINT "FK_AnswerOptions_TestQuestions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES public."TestQuestions"("Id") ON DELETE CASCADE;


--
-- Name: AuditLogs FK_AuditLogs_Users_TeacherId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuditLogs"
    ADD CONSTRAINT "FK_AuditLogs_Users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES public."Users"("Id");


--
-- Name: ControlQuestions FK_ControlQuestions_Lectures_LectureId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."ControlQuestions"
    ADD CONSTRAINT "FK_ControlQuestions_Lectures_LectureId" FOREIGN KEY ("LectureId") REFERENCES public."Lectures"("Id") ON DELETE CASCADE;


--
-- Name: Disciplines FK_Disciplines_Users_CreatedByTeacherId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Disciplines"
    ADD CONSTRAINT "FK_Disciplines_Users_CreatedByTeacherId" FOREIGN KEY ("CreatedByTeacherId") REFERENCES public."Users"("Id") ON DELETE RESTRICT;


--
-- Name: GroupMemberships FK_GroupMemberships_StudentGroups_GroupId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GroupMemberships"
    ADD CONSTRAINT "FK_GroupMemberships_StudentGroups_GroupId" FOREIGN KEY ("GroupId") REFERENCES public."StudentGroups"("Id") ON DELETE CASCADE;


--
-- Name: GroupMemberships FK_GroupMemberships_Users_StudentId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."GroupMemberships"
    ADD CONSTRAINT "FK_GroupMemberships_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: LectureProgress FK_LectureProgress_Lectures_LectureId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LectureProgress"
    ADD CONSTRAINT "FK_LectureProgress_Lectures_LectureId" FOREIGN KEY ("LectureId") REFERENCES public."Lectures"("Id") ON DELETE CASCADE;


--
-- Name: LectureProgress FK_LectureProgress_Users_StudentId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."LectureProgress"
    ADD CONSTRAINT "FK_LectureProgress_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: Lectures FK_Lectures_Topics_TopicId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Lectures"
    ADD CONSTRAINT "FK_Lectures_Topics_TopicId" FOREIGN KEY ("TopicId") REFERENCES public."Topics"("Id") ON DELETE CASCADE;


--
-- Name: StudentAnswers FK_StudentAnswers_TestAttempts_AttemptId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."StudentAnswers"
    ADD CONSTRAINT "FK_StudentAnswers_TestAttempts_AttemptId" FOREIGN KEY ("AttemptId") REFERENCES public."TestAttempts"("Id") ON DELETE CASCADE;


--
-- Name: StudentAnswers FK_StudentAnswers_TestQuestions_QuestionId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."StudentAnswers"
    ADD CONSTRAINT "FK_StudentAnswers_TestQuestions_QuestionId" FOREIGN KEY ("QuestionId") REFERENCES public."TestQuestions"("Id") ON DELETE CASCADE;


--
-- Name: TestAttempts FK_TestAttempts_Tests_TestId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAttempts"
    ADD CONSTRAINT "FK_TestAttempts_Tests_TestId" FOREIGN KEY ("TestId") REFERENCES public."Tests"("Id") ON DELETE CASCADE;


--
-- Name: TestAttempts FK_TestAttempts_Users_StudentId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestAttempts"
    ADD CONSTRAINT "FK_TestAttempts_Users_StudentId" FOREIGN KEY ("StudentId") REFERENCES public."Users"("Id") ON DELETE CASCADE;


--
-- Name: TestQuestions FK_TestQuestions_Tests_TestId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."TestQuestions"
    ADD CONSTRAINT "FK_TestQuestions_Tests_TestId" FOREIGN KEY ("TestId") REFERENCES public."Tests"("Id") ON DELETE CASCADE;


--
-- Name: Tests FK_Tests_Lectures_LectureId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Tests"
    ADD CONSTRAINT "FK_Tests_Lectures_LectureId" FOREIGN KEY ("LectureId") REFERENCES public."Lectures"("Id") ON DELETE CASCADE;


--
-- Name: Topics FK_Topics_Disciplines_DisciplineId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Topics"
    ADD CONSTRAINT "FK_Topics_Disciplines_DisciplineId" FOREIGN KEY ("DisciplineId") REFERENCES public."Disciplines"("Id") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

\unrestrict tT0Uge6EWWMgz3NIoqrAY8IHZjq6TtCIeWEs8bfE67fRw7aj4kgBddflelO6Leg

