using System;
using System.Collections.Generic;

namespace SimpleExamSystem
{
    // ======= Basic People =======
    class Person
    {
        public string Id;
        public string Name;
        public string Faculty;

        public Person(string id, string name, string faculty)
        {
            Id = id;
            Name = name;
            Faculty = faculty;
        }
    }

    class Doctor : Person
    {
        public string Subject;
        public Doctor(string id, string name, string faculty, string subject)
            : base(id, name, faculty)
        {
            Subject = subject;
        }
    }

    class Student : Person
    {
        public Student(string id, string name, string faculty)
            : base(id, name, faculty) { }
    }

    // ======= Questions =======
    class Question
    {
        public int QId;
        public string Text;
        public string Type;
        public string Difficulty;
        public int Mark;
        public List<string> Options = new List<string>();
        public List<string> CorrectLabels = new List<string>();
        public string CorrectText;
    }

    // ======= Exam =======
    class Exam
    {
        public string Title;
        public string Subject;
        public List<Question> Questions = new List<Question>();

        public int TotalMarks()
        {
            int total = 0;
            for (int i = 0; i < Questions.Count; i++)
                total += Questions[i].Mark;
            return total;
        }
    }

    // ======= Program =======
    class Program
    {
        static List<string> Subjects = new List<string>();
        static Exam LastExam = null;
        static int NextQuestionId = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("=== Simple Examination System (OOP, Basic) ===");
            SeedSubjects();

            while (true)
            {
                Console.WriteLine("\n1) Doctor Mode");
                Console.WriteLine("2) Student Mode (take last created exam)");
                Console.WriteLine("0) Exit");
                Console.Write("Choose: ");
                string c = Console.ReadLine();

                if (c == "0") break;
                else if (c == "1") DoctorMode();
                else if (c == "2") StudentMode();
                else Console.WriteLine("Invalid choice.");
            }
        }

        static void DoctorMode()
        {
            Console.WriteLine("\n--- Doctor Mode ---");
            Console.Write("Doctor ID: ");
            string did = Console.ReadLine();
            Console.Write("Doctor Name: ");
            string dname = Console.ReadLine();
            Console.Write("Faculty: ");
            string dfac = Console.ReadLine();

            string subject = PickOrCreateSubject();
            Doctor dr = new Doctor(did, dname, dfac, subject);

            Console.Write("Exam Title: ");
            string title = Console.ReadLine();

            Exam exam = new Exam();
            exam.Title = title;
            exam.Subject = subject;

            int n = AskInt("How many questions? ", 1, 200);

            for (int i = 1; i <= n; i++)
            {
                Console.WriteLine($"\nQuestion #{i}");
                Console.WriteLine("Type: 1) True/False  2) Single Choice  3) Multiple Choice  4) Fill/Complete");
                int t = AskInt("Choose type (1-4): ", 1, 4);

                string qText = AskText("Question text");
                string diff = AskDifficulty();
                int mark = AskInt("Mark for this question: ", 1, 100);

                Question q = new Question();
                q.QId = NextQuestionId++;
                q.Text = qText;
                q.Difficulty = diff;
                q.Mark = mark;

                if (t == 1)
                {
                    q.Type = "TF";
                    bool tf = AskTF("Correct answer (T/F)");
                    q.CorrectText = tf ? "T" : "F";
                }
                else if (t == 2)
                {
                    q.Type = "SC";
                    BuildOptions(q);
                    if (q.CorrectLabels.Count != 1)
                    {
                        Console.WriteLine("Exactly one correct label required. Defaulting to A.");
                        q.CorrectLabels.Clear();
                        q.CorrectLabels.Add("A");
                    }
                }
                else if (t == 3)
                {
                    q.Type = "MC";
                    BuildOptions(q);
                    if (q.CorrectLabels.Count == 0)
                    {
                        Console.WriteLine("At least one correct label required. Defaulting to A.");
                        q.CorrectLabels.Add("A");
                    }
                }
                else
                {
                    q.Type = "Fill";
                    q.CorrectText = AskText("Correct text");
                }

                exam.Questions.Add(q);
            }

            LastExam = exam;
            Console.WriteLine($"\nExam CREATED: \"{exam.Title}\" | Subject: {exam.Subject} | Total Marks: {exam.TotalMarks()}");

            Console.WriteLine("\n--- Switch to Student Mode ---");
            StudentMode();
        }

        static void StudentMode()
        {
            if (LastExam == null)
            {
                Console.WriteLine("\nNo exam available yet. Ask a doctor to create one first.");
                return;
            }

            Console.WriteLine("\n--- Student Mode ---");
            Console.Write("Student ID: ");
            string sid = Console.ReadLine();
            Console.Write("Student Name: ");
            string sname = Console.ReadLine();

            Student st = new Student(sid, sname, LastExam.Subject);

            TakeExam(LastExam, st);
        }

        static void TakeExam(Exam exam, Student student)
        {
            Console.WriteLine($"\nStarting Exam: {exam.Title}  |  Subject: {exam.Subject}");
            Console.WriteLine($"Student: {student.Name} ({student.Id})\n");

            int total = exam.TotalMarks();
            int score = 0;

            for (int i = 0; i < exam.Questions.Count; i++)
            {
                Question q = exam.Questions[i];
                Console.WriteLine($"Q{i + 1} [{q.Difficulty}]  ({q.Mark} mark): {q.Text}");

                string answer = "";

                if (q.Type == "TF")
                {
                    bool a = AskTF("Your answer (T/F)");
                    answer = a ? "T" : "F";
                }
                else if (q.Type == "SC")
                {
                    PrintOptions(q.Options);
                    answer = AskLabel("Choose one label (A, B, C, ...)");
                }
                else if (q.Type == "MC")
                {
                    PrintOptions(q.Options);
                    answer = AskText("Choose labels without spaces (e.g., AC)");
                    answer = answer.Trim().ToUpper();
                }
                else
                {
                    answer = AskText("Your text answer");
                }

                bool correct = false;

                if (q.Type == "TF" || q.Type == "Fill")
                {
                    if (SameText(answer, q.CorrectText)) correct = true;
                }
                else if (q.Type == "SC")
                {
                    if (q.CorrectLabels.Count == 1 && SameText(answer, q.CorrectLabels[0])) correct = true;
                }
                else if (q.Type == "MC")
                {
                    List<string> chosen = ToLabelList(answer);
                    if (SameLabelSet(chosen, q.CorrectLabels)) correct = true;
                }

                if (correct) score += q.Mark;

                Console.WriteLine();
            }

            Console.WriteLine($"Submitted. Final Score: {score}/{total}\n");
        }

        static string PickOrCreateSubject()
        {
            if (Subjects.Count == 0)
            {
                string s = AskText("No subjects yet. Enter a subject name to create");
                Subjects.Add(s);
                return s;
            }

            Console.WriteLine("\nAvailable Subjects:");
            for (int i = 0; i < Subjects.Count; i++)
                Console.WriteLine($"{i + 1}) {Subjects[i]}");

            Console.Write("Pick subject number or type N to create new: ");
            string sIn = Console.ReadLine();
            if (sIn.Trim().ToUpper() == "N")
            {
                string s = AskText("New subject name");
                Subjects.Add(s);
                return s;
            }

            int idx;
            if (!int.TryParse(sIn, out idx)) idx = 1;
            if (idx < 1) idx = 1;
            if (idx > Subjects.Count) idx = Subjects.Count;
            return Subjects[idx - 1];
        }

        static void BuildOptions(Question q)
        {
            int count = AskInt("How many options? (2-6): ", 2, 6);
            q.Options.Clear();
            q.CorrectLabels.Clear();

            for (int i = 0; i < count; i++)
            {
                string label = ((char)('A' + i)).ToString();
                string text = AskText("Option " + label);
                q.Options.Add(text);

                string isC = AskText("Is " + label + " correct? (y/n)");
                if (isC.Trim().ToLower().StartsWith("y"))
                    q.CorrectLabels.Add(label);
            }
        }

        static void PrintOptions(List<string> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                string label = ((char)('A' + i)).ToString();
                Console.WriteLine("  " + label + ") " + options[i]);
            }
        }

        static string AskText(string prompt)
        {
            Console.Write(prompt + ": ");
            return Console.ReadLine();
        }

        static int AskInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = Console.ReadLine();
                int n;
                if (int.TryParse(s, out n) && n >= min && n <= max) return n;
                Console.WriteLine("Please enter a valid number.");
            }
        }

        static bool AskTF(string prompt)
        {
            while (true)
            {
                Console.Write(prompt + " ");
                string s = Console.ReadLine().Trim().ToUpper();
                if (s == "T" || s == "TRUE") return true;
                if (s == "F" || s == "FALSE") return false;
                Console.WriteLine("Enter T or F.");
            }
        }

        static string AskDifficulty()
        {
            Console.WriteLine("Difficulty: 1) Easy  2) Intermediate  3) Hard");
            int n = AskInt("Choose (1-3): ", 1, 3);
            if (n == 1) return "Easy";
            if (n == 2) return "Intermediate";
            return "Hard";
        }

        static bool SameText(string a, string b)
        {
            if (a == null) a = "";
            if (b == null) b = "";
            return a.Trim().ToLower() == b.Trim().ToLower();
        }

        static string AskLabel(string prompt)
        {
            Console.Write(prompt + ": ");
            string s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) return "A";
            s = s.Trim().ToUpper();
            if (s.Length > 1) s = s.Substring(0, 1);
            return s;
        }

        static List<string> ToLabelList(string s)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(s)) return list;
            s = s.Trim().ToUpper();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c >= 'A' && c <= 'Z')
                {
                    string label = c.ToString();
                    if (!list.Contains(label)) list.Add(label);
                }
            }
            return list;
        }

        static bool SameLabelSet(List<string> a, List<string> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
                if (!b.Contains(a[i])) return false;
            return true;
        }

        static void SeedSubjects()
        {
            Subjects.Add("Intro to CS");
            Subjects.Add("Discrete Math");
        }
    }
}
