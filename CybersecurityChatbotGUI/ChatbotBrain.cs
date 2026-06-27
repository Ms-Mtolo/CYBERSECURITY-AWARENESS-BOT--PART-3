using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbotGUI
{
    
    // A delegate is like a contract - it defines what a method must look like
    // This one is for detecting user sentiment (emotions) and changing the UI color
    public delegate void SentimentDetectedHandler(string sentiment, string colorHex);

    public class ChatbotBrain
    {
        

        // Stores the user's name, favorite topic, and what topics they've discussed
        public string UserName { get; set; } = "";
        public string FavouriteTopic { get; private set; } = "";
        public string LastTopic { get; private set; } = "";
        public string CurrentTopic { get; private set; } = "";

        // Track if we've shown the definition for the current topic
        // This prevents showing the same definition twice in a row
        private bool _definitionShown = false;

        // Delegate for sentiment detection - this lets the UI know when the user shows emotion
        public SentimentDetectedHandler? OnSentimentDetected;

        // Random object for picking random responses from lists
        private readonly Random _random = new Random();

        
        // DATABASE STORAGE
        
        // This connects to MySQL to save and load user tasks
        private TaskStorage _taskStorage = new TaskStorage();

        
        // ACTIVITY LOG
        
        // Keeps track of everything the user does with the bot
        private List<ActivityLogEntry> _activityLog = new List<ActivityLogEntry>();
        private const int MAX_LOG_DISPLAY = 10; // Only show the 10 most recent activities

        
        // QUIZ SYSTEM - FIXED: Initialize with empty list first
        
        private List<QuizQuestion> _quizQuestions = new List<QuizQuestion>(); // All the quiz questions
        private int _currentQuestionIndex = 0; // Which question we're currently on
        private int _quizScore = 0; // How many questions the user got right
        private bool _quizActive = false; // Is the quiz currently running?


        // TOPIC DEFINITIONS - NEW!
        // These are simple explanations for each cybersecurity topic
        // When users ask "What is phishing?" they get a clear definition
        private readonly Dictionary<string, string> _topicDefinitions = new Dictionary<string, string>
        {
            ["password"] = " PASSWORD DEFINITION:\nA password is a secret combination of characters (letters, numbers, and symbols) that you use to prove your identity and gain access to your accounts. Think of it as the key to your digital house - if someone gets your key, they can get inside!",

            ["phishing"] = " PHISHING DEFINITION:\nPhishing is a cyberattack where criminals send fake emails, messages, or websites that look like they're from legitimate companies (like your bank or PayPal). Their goal is to trick you into giving away your personal information, passwords, or credit card details. It's called 'phishing' because they're 'fishing' for your information!",

            ["scam"] = " SCAM DEFINITION:\nA scam is a dishonest scheme designed to trick people out of their money or personal information. Scammers pretend to be someone they're not - like a bank official, a lottery winner, or even a loved one in trouble - to gain your trust and exploit it for financial gain.",

            ["privacy"] = " PRIVACY DEFINITION:\nOnline privacy means having control over who can see and use your personal information on the internet. This includes your name, address, photos, browsing habits, and anything else you share online. Your personal data is valuable - protect it like you would protect your wallet!",

            ["2fa"] = " TWO-FACTOR AUTHENTICATION (2FA) DEFINITION:\nTwo-Factor Authentication is a security method that requires TWO different ways to prove who you are before you can log in. Usually it's: 1) something you KNOW (your password) and 2) something you HAVE (your phone). Even if a hacker steals your password, they still can't get in without your phone!",

            ["malware"] = " MALWARE DEFINITION:\nMalware is short for 'malicious software' - any program designed to damage, disrupt, or steal from your computer or device. This includes viruses, ransomware (which locks your files and demands payment), and spyware (which secretly watches what you do).",

            ["wifi"] = " WIFI SAFETY DEFINITION:\nWiFi is the wireless network that connects your devices to the internet. Public WiFi (like in cafes, airports, or hotels) is dangerous because hackers can create fake networks or intercept the data you send. This means they could see your passwords, emails, or banking details if you're not careful!",

            ["update"] = " SOFTWARE UPDATE DEFINITION:\nA software update is a free download that fixes problems in your programs, adds new features, and most importantly - closes security holes that hackers could use to break in. Not updating your software is like leaving your front door unlocked!"
        };


        // EXPANDED KEYWORDS - More ways for users to ask!
        // These are all the different phrases users might use to ask about each topic
        // This makes the bot smarter - users don't have to use exact keywords
        private readonly string[] _passwordKeywords = {
            "password", "passwords", "pwd", "login", "log in", "credentials",
            "password safety", "password tips", "secure password"
        };

        private readonly string[] _phishingKeywords = {
            "phishing", "phish", "scam email", "fake email", "email scam",
            "phishing attack", "phishing emails"
        };

        private readonly string[] _scamKeywords = {
            "scam", "scams", "scammer", "scammers", "con artist",
            "internet scam", "online scam", "fraud"
        };

        private readonly string[] _privacyKeywords = {
            "privacy", "private", "data privacy", "personal info",
            "information privacy", "online privacy"
        };

        private readonly string[] _2faKeywords = {
            "2fa", "two factor", "two-factor", "multi factor", "mfa",
            "two step", "2-step", "two step verification"
        };

        private readonly string[] _malwareKeywords = {
            "malware", "virus", "viruses", "ransomware", "spyware",
            "trojan", "worm", "malicious software"
        };

        private readonly string[] _wifiKeywords = {
            "wifi", "wi-fi", "wireless", "hotspot", "public wifi",
            "public wi-fi", "wireless network"
        };

        private readonly string[] _updateKeywords = {
            "update", "updates", "update software", "software update",
            "patch", "patches", "outdated"
        };

        // Combined keyword mapping for easy lookup
        // This dictionary groups all keywords by topic for quick searching
        private readonly Dictionary<string, string[]> _allKeywords = new Dictionary<string, string[]>();


        // TIP RESPONSES 
        // Each topic has multiple tips - the bot picks one randomly
        // This makes conversations feel more natural and varied
        private readonly Dictionary<string, List<string>> _keywordResponses = new Dictionary<string, List<string>>
        {
            ["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Aim for 12+ characters with symbols, numbers, and mixed case.",
                "Avoid using personal details like birthdays or pet names in passwords. Hackers try these first!",
                "Consider using a password manager like Bitwarden or LastPass - they generate and remember complex passwords for you.",
                "Never reuse the same password on multiple sites. If one site gets hacked, all your accounts become vulnerable.",
                "Change your passwords every 3-6 months, especially for sensitive accounts like banking and email.",
                "Use a passphrase instead of a password - like 'PurpleCoffeeMountain42!' - it's longer and easier to remember!"
            },
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations like banks or PayPal.",
                "Check the sender's email address carefully. 'paypa1.com' (with a 1) is NOT 'paypal.com'.",
                "Legitimate companies will NEVER ask for your password via email. If they do, it's a scam.",
                "Hover over links before clicking. If the URL looks strange or doesn't match the company, don't click it.",
                "Look for poor spelling and grammar - professional companies don't make these mistakes!",
                "When in doubt, go directly to the company's website by typing the URL yourself, not clicking the link."
            },
            ["scam"] = new List<string>
            {
                "If something sounds too good to be true online, it probably is a scam. Trust your instincts.",
                "Never send money to someone you've only met online, especially through gift cards or wire transfers.",
                "Scammers create urgency - 'Act NOW or your account will be closed!' - to stop you thinking clearly. Slow down.",
                "Report scams to the South African Police Service (SAPS) and the South African Banking Risk Information Centre (SABRIC).",
                "Be wary of anyone asking for your OTP (One-Time Password) - banks never ask for this!",
                "If a 'family member' calls asking for money urgently, call them back on their usual number to verify."
            },
            ["privacy"] = new List<string>
            {
                "Review your social media privacy settings regularly. Make sure only friends can see your personal info.",
                "Never post your home address, ID number, or banking details online.",
                "Use a VPN (Virtual Private Network) to hide your browsing activity, especially on public Wi-Fi.",
                "Be careful what apps you give permissions to. Does that flashlight app really need your contacts?",
                "Use strong privacy settings on WhatsApp - set 'Last Seen' to 'My Contacts' only.",
                "Think before you post - once something is online, it's hard to remove completely."
            },
            ["2fa"] = new List<string>
            {
                "Two-Factor Authentication (2FA) means even if someone steals your password, they still can't log in without your phone.",
                "Use an authenticator app like Google Authenticator instead of SMS codes - it's more secure.",
                "Enable 2FA on your most important accounts first: email, banking, and social media.",
                "2FA stops 99.9% of automated hacking attempts. It's one of the best things you can do right now.",
                "Some services offer hardware keys like YubiKey - the most secure form of 2FA!",
                "Always keep backup codes safe - if you lose your phone, you'll need these to get back into your accounts."
            },
            ["malware"] = new List<string>
            {
                "Malware is malicious software designed to damage or steal from your device. Keep antivirus software updated.",
                "Never download files from untrusted websites. Even a PDF can contain malware.",
                "Ransomware encrypts your files and demands payment. The best defence is regular backups (3-2-1 rule).",
                "If your device suddenly slows down or shows pop-ups, run an antivirus scan immediately.",
                "Be careful with USB drives from others - they can carry malware too!",
                "Enable your device's built-in protection like Windows Defender or Mac's XProtect."
            },
            ["wifi"] = new List<string>
            {
                "Public Wi-Fi is dangerous! Hackers can create fake hotspots to steal your data.",
                "Never do banking or shopping on public Wi-Fi. Use your mobile data instead.",
                "A VPN encrypts your connection, making public Wi-Fi much safer to use.",
                "Always check with staff for the exact Wi-Fi name - fake networks often have similar names.",
                "Turn off Wi-Fi auto-connect on your phone to avoid connecting to fake networks automatically.",
                "Use HTTPS websites (look for the padlock ) even when using public Wi-Fi."
            },
            ["update"] = new List<string>
            {
                "Software updates close security holes. The WannaCry ransomware infected 200,000 computers because people didn't update.",
                "Enable automatic updates so you don't have to remember. This alone prevents most common attacks.",
                "Outdated software is one of the top reasons people get hacked. Don't delay updates!",
                "Updates apply to everything: your phone, laptop, browser, and apps.",
                "Mobile apps also need updates - they often fix security issues too!",
                "Set a weekly reminder to check for updates on all your devices."
            }
        };


        // TOPIC SWITCHING KEYWORDS - added method
        // These are phrases users can say to switch to a different topic
        private readonly string[] _topicSwitchKeywords = {
            "change topic", "switch topic", "talk about", "tell me about",
            "what about", "let's discuss", "let's talk", "how about",
            "switch to", "change to", "i want to know about"
        };

        // These are phrases users say when they want more information
        private readonly string[] _wantMoreKeywords = {
            "tell me more", "more tips", "more info", "another tip",
            "give me more", "i want more", "more", "elaborate", "go on"
        };

        // These are phrases users say when they want a definition
        private readonly string[] _definitionKeywords = {
            "what is", "what are", "define", "explain", "meaning of",
            "definition", "describe", "what does", "what's"
        };


        // SENTIMENT KEYWORDS (ORIGINAL - KEEP ALL OF THIS)
        // These detect how the user is feeling and change the bot's response
        private readonly Dictionary<string, (string message, string colorHex)> _sentimentMap =
            new Dictionary<string, (string, string)>
            {
                ["worried"] = ("I can sense you're worried. That's completely understandable!", "#FF8C00"),
                ["scared"] = ("It's okay to feel scared about cyber threats.", "#FF8C00"),
                ["anxious"] = ("Feeling anxious about online safety is very common.", "#FF8C00"),
                ["frustrated"] = ("I hear you - cybersecurity can feel overwhelming.", "#FF4500"),
                ["confused"] = ("No worries at all - these topics can be confusing at first.", "#9370DB"),
                ["curious"] = ("I love your curiosity! That's the best mindset for learning.", "#00CED1"),
                ["excited"] = ("That excitement will take you far in cybersecurity!", "#00CED1"),
                ["happy"] = ("Great! A positive mindset makes learning much easier.", "#32CD32"),
                ["good"] = ("Wonderful! Let's keep that energy going.", "#32CD32"),
                ["great"] = ("Fantastic! You're in a great headspace to learn today.", "#32CD32"),
                ["terrible"] = ("I'm sorry to hear that. I'll try to keep things simple and helpful.", "#FF8C00"),
                ["bad"] = ("I'm sorry you're not feeling great. Let's focus on what we can control online.", "#FF8C00"),
                ["overwhelmed"] = ("Let's take it one step at a time - cybersecurity doesn't have to be complicated.", "#FF8C00"),
            };


        // PHISHING TIPS (ORIGINAL - KEEP ALL OF THIS)
        // Extra tips specifically for phishing topics
        private readonly List<string> _phishingTips = new List<string>
        {
            "Be cautious of emails with urgent subject lines like 'Your account will be suspended!'",
            "Always check: does the email address match the real company domain?",
            "Poor spelling and grammar in professional emails is a big red flag.",
            "Hover over links BEFORE clicking - check if the URL matches what's shown.",
            "If you're unsure, go directly to the company's official website instead of clicking the link.",
            "Legitimate banks never ask for your PIN or full password via email.",
            "Be suspicious of unexpected attachments, even from people you know - their account may be hacked."
        };



        // TASK KEYWORDS - added for part 3
        // These are phrases users say to manage their tasks
        private readonly string[] _addTaskKeywords = {
            "add task", "create task", "new task", "add a task", "make a task",
            "create a task", "add new task", "task add"
        };

        private readonly string[] _reminderKeywords = {
            "remind me", "set reminder", "reminder", "remind in", "remind on",
            "set a reminder", "add reminder"
        };

        private readonly string[] _viewTasksKeywords = {
            "show tasks", "list tasks", "view tasks", "my tasks", "what tasks",
            "show my tasks", "display tasks", "task list"
        };

        private readonly string[] _completeTaskKeywords = {
            "complete task", "mark task", "finish task", "done task",
            "complete", "mark complete", "task done"
        };

        private readonly string[] _deleteTaskKeywords = {
            "delete task", "remove task", "erase task", "delete",
            "remove", "task delete"
        };

        private readonly string[] _quizKeywords = {
            "start quiz", "play quiz", "take quiz", "begin quiz",
            "cybersecurity quiz", "quiz time", "let's quiz"
        };

        private readonly string[] _activityLogKeywords = {
            "activity log", "what have you done", "show log",
            "recent actions", "summary", "log", "action log"
        };

        // Pending task for reminder flow
        // These store the task while the user decides about a reminder
        private string? _pendingTaskTitle = null;
        private string? _pendingTaskDesc = null;


        // CONSTRUCTOR
        // This runs when the bot is created - sets up everything
        public ChatbotBrain()
        {
            InitializeKeywordMapping();
            InitializeQuizQuestions(); // Now this will work because _quizQuestions is initialized
            LogActivity("Chatbot initialized", "System started successfully");
        }


        // This connects all the keywords to their topics for easy searching
        private void InitializeKeywordMapping()
        {
            _allKeywords["password"] = _passwordKeywords;
            _allKeywords["phishing"] = _phishingKeywords;
            _allKeywords["scam"] = _scamKeywords;
            _allKeywords["privacy"] = _privacyKeywords;
            _allKeywords["2fa"] = _2faKeywords;
            _allKeywords["malware"] = _malwareKeywords;
            _allKeywords["wifi"] = _wifiKeywords;
            _allKeywords["update"] = _updateKeywords;
        }


        
        // Returns a list of all topics the bot knows about
        public List<string> GetAvailableTopics()
        {
            return new List<string> { "password", "phishing", "scam", "privacy", "2fa", "malware", "wifi", "update" };
        }

        // Converts a topic code to a user-friendly display name with emoji
        public string GetTopicDisplayName(string topic)
        {
            return topic switch
            {
                "password" => " Passwords",
                "phishing" => " Phishing",
                "scam" => " Scams",
                "privacy" => " Privacy",
                "2fa" => " 2FA",
                "malware" => " Malware",
                "wifi" => " WiFi Safety",
                "update" => " Updates",
                _ => topic
            };
        }


        // QUIZ INITIALIZATION
        // Creates all the quiz questions with answers and explanations
        private void InitializeQuizQuestions()
        {
            _quizQuestions = new List<QuizQuestion> // Now we assign the list (not add to it)
            {
                new QuizQuestion {
                    Text = "What is the strongest type of password?",
                    Options = new List<string> { "Your birthdate", "password123", "A 12+ character mix of letters, numbers, and symbols", "Your pet's name" },
                    CorrectIndex = 2,
                    Explanation = "Strong passwords are long, complex, and unique. Avoid personal information!"
                },
                new QuizQuestion {
                    Text = "What should you do if you receive a suspicious email from 'your bank' asking for your password?",
                    Options = new List<string> { "Reply with your password", "Click the link to verify", "Delete the email and report it as phishing", "Forward it to friends" },
                    CorrectIndex = 2,
                    Explanation = "Legitimate banks never ask for passwords via email. Report phishing emails!"
                },
                new QuizQuestion {
                    Text = "True or False: Using the same password for multiple accounts is safe if the password is strong.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "If one account gets hacked, all accounts with the same password become vulnerable."
                },
                new QuizQuestion {
                    Text = "What does 2FA stand for?",
                    Options = new List<string> { "Two Factor Authentication", "Second File Access", "Double Password System", "Two Form Agreement" },
                    CorrectIndex = 0,
                    Explanation = "2FA adds a second verification step, like a code sent to your phone."
                },
                new QuizQuestion {
                    Text = "Which of these is a sign of a phishing email?",
                    Options = new List<string> { "Poor spelling and grammar", "Urgent threatening language", "Suspicious sender email address", "All of the above" },
                    CorrectIndex = 3,
                    Explanation = "Phishing emails often have multiple red flags. Always be cautious!"
                },
                new QuizQuestion {
                    Text = "True or False: Public Wi-Fi is completely safe for online banking.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation = "Public Wi-Fi can be intercepted by hackers. Use mobile data or a VPN for sensitive activities."
                },
                new QuizQuestion {
                    Text = "What is malware?",
                    Options = new List<string> { "A type of antivirus", "Malicious software designed to harm devices", "A secure password manager", "A Wi-Fi protocol" },
                    CorrectIndex = 1,
                    Explanation = "Malware includes viruses, ransomware, and spyware that can steal your data."
                },
                new QuizQuestion {
                    Text = "How often should you update your software?",
                    Options = new List<string> { "Never, updates are useless", "Only when you remember", "As soon as updates are available", "Once a year" },
                    CorrectIndex = 2,
                    Explanation = "Updates patch security vulnerabilities. Delaying updates leaves you exposed!"
                },
                new QuizQuestion {
                    Text = "True or False: A VPN makes your internet connection more secure on public Wi-Fi.",
                    Options = new List<string> { "True", "False" },
                    CorrectIndex = 0,
                    Explanation = "VPNs encrypt your traffic, protecting you from hackers on public networks."
                },
                new QuizQuestion {
                    Text = "What's the best way to store your passwords?",
                    Options = new List<string> { "On a sticky note on your monitor", "In a text file on your desktop", "In a password manager", "Same password for everything" },
                    CorrectIndex = 2,
                    Explanation = "Password managers generate and store strong, unique passwords securely."
                },
                new QuizQuestion {
                    Text = "What should you do if someone asks for your personal info online?",
                    Options = new List<string> { "Give it to them if they seem nice", "Never share personal info with strangers", "Only share your birthday", "Share but ask them to keep it secret" },
                    CorrectIndex = 1,
                    Explanation = "Never share personal information with strangers online, no matter how trustworthy they seem."
                },
                new QuizQuestion {
                    Text = "Which of these is a strong password?",
                    Options = new List<string> { "password123", "P@ssw0rd!", "G7#kL9$mQ2@xR5", "12345678" },
                    CorrectIndex = 2,
                    Explanation = "Strong passwords are long, random, and include uppercase, lowercase, numbers, and symbols."
                }
            };
        }


        // ACTIVITY LOG METHODS added for part 3
        // These track what the user does for the activity log
        private void LogActivity(string action, string details)
        {
            // Add the new activity at the beginning of the list
            _activityLog.Insert(0, new ActivityLogEntry
            {
                Timestamp = DateTime.Now,
                Action = action,
                Details = details
            });

            // Keep only last 50 entries to save memory
            if (_activityLog.Count > 50)
                _activityLog.RemoveAt(_activityLog.Count - 1);
        }

        // Returns a formatted string showing recent user activities
        public string GetActivityLog()
        {
            if (_activityLog.Count == 0)
                return " No activities have been recorded yet.\n\nStart chatting, add tasks, or play the quiz to create activity logs!";

            string logText = " ACTIVITY LOG - Recent Actions:\n";
            logText += "═══════════════════════════════════════════════\n\n";

            int count = 0;
            foreach (var entry in _activityLog.Take(MAX_LOG_DISPLAY))
            {
                count++;
                logText += $"{count}. [{entry.Timestamp:HH:mm}] {entry.Action}\n";
                logText += $"   → {entry.Details}\n\n";
            }

            if (_activityLog.Count > MAX_LOG_DISPLAY)
                logText += $"... and {_activityLog.Count - MAX_LOG_DISPLAY} more activities.\n";

            logText += "\n Type 'Show activity log' anytime to see this again.";
            return logText;
        }


        // TASK MANAGEMENT METHODS added for part 3
        // These handle all the task operations using the database

        // Gets all tasks from the database
        public List<TaskItem> GetTasks()
        {
            return _taskStorage.GetTasks();
        }

        // Adds a task from text input
        public string AddTaskViaText(string input)
        {
            string? taskCommandResult = ProcessTaskCommand(input);
            return taskCommandResult ?? " Sorry, I couldn't process that task request.";
        }

        // Marks a task as completed by its number in the list
        public string CompleteTaskByNumber(int taskNumber)
        {
            var tasks = _taskStorage.GetTasks();
            if (taskNumber > 0 && taskNumber <= tasks.Count)
            {
                var task = tasks[taskNumber - 1];
                if (_taskStorage.MarkTaskCompleted(task.Id))
                {
                    LogActivity("Task completed", $"'{task.Title}' marked as completed");
                    return $" Task '{task.Title}' marked as completed! 🎉";
                }
            }
            return "Could not complete the task. Please check the task number.";
        }

        // Deletes a task by its number in the list
        public string DeleteTaskByNumber(int taskNumber)
        {
            var tasks = _taskStorage.GetTasks();
            if (taskNumber > 0 && taskNumber <= tasks.Count)
            {
                var task = tasks[taskNumber - 1];
                if (_taskStorage.DeleteTask(task.Id))
                {
                    LogActivity("Task deleted", $"'{task.Title}' removed from tasks");
                    return $" Task '{task.Title}' has been deleted.";
                }
            }
            return " Could not delete the task. Please check the task number.";
        }

        // Processes task-related commands from user input
        private string? ProcessTaskCommand(string input)
        {
            string lowerInput = input.ToLower();

            // to view Tasks
            if (_viewTasksKeywords.Any(k => lowerInput.Contains(k)))
            {
                return HandleViewTasks();
            }

            // to complete Task
            if (_completeTaskKeywords.Any(k => lowerInput.Contains(k)))
            {
                return HandleCompleteTask(lowerInput);
            }

            // to delete Task
            if (_deleteTaskKeywords.Any(k => lowerInput.Contains(k)))
            {
                return HandleDeleteTask(lowerInput);
            }

            //to add Task
            if (_addTaskKeywords.Any(k => lowerInput.Contains(k)))
            {
                return HandleAddTask(lowerInput);
            }

            // Reminder (can be part of add task flow)
            if (_reminderKeywords.Any(k => lowerInput.Contains(k)))
            {
                if (_pendingTaskTitle != null)
                {
                    return HandleSetReminder(lowerInput);
                }
            }

            return null;
        }

        // Shows all tasks to the user
        private string HandleViewTasks()
        {
            var tasks = _taskStorage.GetTasks();
            if (tasks.Count == 0)
            {
                return " You have no tasks yet.\n\nTry: 'Add task to enable two-factor authentication'";
            }

            string taskListText = " YOUR TASKS:\n";
            taskListText += "═══════════════════════════════\n\n";

            int completedCount = 0;
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                string status = task.IsCompleted ? " " : " ";
                string reminder = task.ReminderDate.HasValue
                    ? $" (Reminder: {task.ReminderDate.Value:yyyy-MM-dd})"
                    : "";

                taskListText += $"{i + 1}. {status} {task.Title}{reminder}\n";
                if (!string.IsNullOrEmpty(task.Description))
                    taskListText += $"    {task.Description}\n";

                if (task.IsCompleted) completedCount++;
            }

            taskListText += $"\n {completedCount}/{tasks.Count} tasks completed";
            taskListText += $"\n\n To manage tasks:\n";
            taskListText += $"   • Complete task 1\n";
            taskListText += $"   • Delete task 1\n";
            taskListText += $"   • Add task to [your task]";

            LogActivity("Tasks viewed", $"{tasks.Count} tasks displayed ({completedCount} completed)");
            return taskListText;
        }

        // Handles adding a new task
        private string HandleAddTask(string input)
        {
            // Extract task title from input
            string? taskTitle = ExtractTaskText(input);
            if (string.IsNullOrEmpty(taskTitle))
            {
                return " What task would you like to add?\n\nExample: 'Add task to enable two-factor authentication'";
            }

            _pendingTaskTitle = taskTitle;
            _pendingTaskDesc = $"Cybersecurity task: {taskTitle}";

            LogActivity("Task pending", $"Task '{taskTitle}' waiting for reminder option");

            return $" Task: '{taskTitle}' is ready.\n\n" +
                   " Would you like to set a reminder?\n" +
                   "   Type 'remind me in 7 days' or 'remind me tomorrow'\n" +
                   "   Or type 'no reminder' to skip.";
        }

        // Handles setting a reminder for a pending task
        private string HandleSetReminder(string input)
        {
            if (_pendingTaskTitle == null)
                return " I don't have a pending task. Say 'Add task' first!";

            DateTime? reminderDate = ParseReminderDate(input);

            if (reminderDate == null && !input.Contains("no reminder"))
            {
                return " I didn't understand that. Try:\n" +
                       "• 'remind me in 3 days'\n" +
                       "• 'remind me tomorrow'\n" +
                       "• 'remind me in 1 week'\n" +
                       "• 'no reminder' to skip";
            }

            int addResult = _taskStorage.AddTask(_pendingTaskTitle, _pendingTaskDesc ?? "", reminderDate);

            if (addResult > 0)
            {
                string reminderMsg = reminderDate.HasValue
                    ? $" Reminder set for {reminderDate.Value:yyyy-MM-dd}"
                    : " No reminder set";

                LogActivity("Task added", $"'{_pendingTaskTitle}' added (Reminder: {reminderMsg})");

                string responseText = $" Task '{_pendingTaskTitle}' added successfully!\n";
                responseText += $"   {reminderMsg}\n\n";
                responseText += " Type 'Show my tasks' to see all your tasks.";

                _pendingTaskTitle = null;
                _pendingTaskDesc = null;
                return responseText;
            }

            return " Sorry, I couldn't save your task. Please try again.";
        }

        // Handles completing a task
        private string HandleCompleteTask(string input)
        {
            int taskNumber = ExtractTaskNumber(input);
            if (taskNumber > 0)
            {
                return CompleteTaskByNumber(taskNumber);
            }
            return " Please specify which task to complete.\n\nExample: 'Complete task 1'";
        }

        // Handles deleting a task
        private string HandleDeleteTask(string input)
        {
            int taskNumber = ExtractTaskNumber(input);
            if (taskNumber > 0)
            {
                return DeleteTaskByNumber(taskNumber);
            }
            return " Please specify which task to delete.\n\nExample: 'Delete task 1'";
        }

        // Extracts the task title from user input
        private string? ExtractTaskText(string input)
        {
            // Remove command keywords to extract the actual task
            string cleaned = input;
            foreach (var keyword in _addTaskKeywords)
            {
                if (cleaned.ToLower().Contains(keyword))
                {
                    cleaned = cleaned.Substring(cleaned.ToLower().IndexOf(keyword) + keyword.Length).Trim();
                    break;
                }
            }

            // Remove common filler words
            string[] fillers = { "to", "a", "my", "the", "for", "of" };
            foreach (var filler in fillers)
            {
                if (cleaned.ToLower().StartsWith(filler + " "))
                    cleaned = cleaned.Substring(filler.Length + 1).Trim();
            }

            if (string.IsNullOrEmpty(cleaned))
                return null;

            // Capitalize first letter
            return char.ToUpper(cleaned[0]) + cleaned.Substring(1);
        }

        // Parses a reminder date from natural language
        private DateTime? ParseReminderDate(string input)
        {
            string lower = input.ToLower();

            // Parse "in X days"
            if (lower.Contains("in") && lower.Contains("day"))
            {
                var words = input.Split(' ');
                for (int i = 0; i < words.Length; i++)
                {
                    if (int.TryParse(words[i], out int days) && days > 0)
                    {
                        return DateTime.Now.AddDays(days);
                    }
                }
                // Default to 7 days if "in" and "day" but no number
                if (lower.Contains("week"))
                    return DateTime.Now.AddDays(7);
                if (lower.Contains("month"))
                    return DateTime.Now.AddMonths(1);
                return DateTime.Now.AddDays(7);
            }

            if (lower.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            if (lower.Contains("today"))
                return DateTime.Now;

            if (lower.Contains("week"))
                return DateTime.Now.AddDays(7);

            if (lower.Contains("month"))
                return DateTime.Now.AddMonths(1);

            if (lower.Contains("year"))
                return DateTime.Now.AddYears(1);

            // Try parsing a specific date (e.g., "2024-12-25")
            try
            {
                var dateMatch = System.Text.RegularExpressions.Regex.Match(input, @"\d{4}-\d{2}-\d{2}");
                if (dateMatch.Success)
                {
                    return DateTime.Parse(dateMatch.Value);
                }
            }
            catch { }

            return null;
        }

        // Extracts a task number from user input
        private int ExtractTaskNumber(string input)
        {
            var words = input.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int number))
                    return number;
            }
            return 0;
        }


        // QUIZ METHODS added for part 3
        // These handle the cybersecurity quiz game

        // Starts a new quiz
        public void StartQuiz()
        {
            _quizActive = true;
            _currentQuestionIndex = 0;
            _quizScore = 0;
            LogActivity("Quiz started", "User began cybersecurity quiz");
        }

        // Gets the next quiz question
        public string GetNextQuizQuestion()
        {
            if (_currentQuestionIndex >= _quizQuestions.Count)
            {
                _quizActive = false;
                int totalQuestions = _quizQuestions.Count;
                int percentage = (_quizScore * 100) / totalQuestions;

                string feedback;
                if (percentage >= 80)
                    feedback = " Excellent! You're a cybersecurity pro! 🎉";
                else if (percentage >= 60)
                    feedback = " Good job! Keep learning to stay safe online! 📚";
                else
                    feedback = " Keep studying cybersecurity topics to improve! You'll get there!";

                LogActivity("Quiz completed", $"Score: {_quizScore}/{totalQuestions} ({percentage}%)");

                return $" QUIZ COMPLETE! \n" +
                       $"═══════════════════════════\n\n" +
                       $"Your score: {_quizScore} out of {totalQuestions}\n" +
                       $"Percentage: {percentage}%\n\n" +
                       $"{feedback}\n\n" +
                       $"Type 'Start quiz' to play again!";
            }

            var q = _quizQuestions[_currentQuestionIndex];
            string optionsText = "";
            for (int i = 0; i < q.Options.Count; i++)
            {
                optionsText += $"\n{(char)('A' + i)}. {q.Options[i]}";
            }

            return $" QUESTION {_currentQuestionIndex + 1}/{_quizQuestions.Count}\n" +
                   $"═══════════════════════════\n\n" +
                   $"{q.Text}\n\n" +
                   $"Select your answer (A, B, C, or D):{optionsText}";
        }

        // Processes the user's quiz answer
        private string ProcessQuizAnswer(string input)
        {
            if (!_quizActive)
                return " No quiz is currently active. Type 'Start quiz' to begin!";

            string lowerInput = input.ToLower().Trim();
            int selectedIndex = -1;

            // Check for A, B, C, D
            if (lowerInput.Length == 1 && lowerInput[0] >= 'a' && lowerInput[0] <= 'd')
                selectedIndex = lowerInput[0] - 'a';
            // Check for 1, 2, 3, 4
            else if (lowerInput.Length == 1 && lowerInput[0] >= '1' && lowerInput[0] <= '4')
                selectedIndex = lowerInput[0] - '1';
            else if (int.TryParse(lowerInput, out int num) && num >= 1 && num <= 4)
                selectedIndex = num - 1;
            // Check for full words
            else if (lowerInput.Contains("a") || lowerInput.Contains("one"))
                selectedIndex = 0;
            else if (lowerInput.Contains("b") || lowerInput.Contains("two"))
                selectedIndex = 1;
            else if (lowerInput.Contains("c") || lowerInput.Contains("three"))
                selectedIndex = 2;
            else if (lowerInput.Contains("d") || lowerInput.Contains("four"))
                selectedIndex = 3;

            if (selectedIndex == -1)
            {
                return " Please select A, B, C, or D (or 1, 2, 3, 4)";
            }

            var q = _quizQuestions[_currentQuestionIndex];

            if (selectedIndex == q.CorrectIndex)
            {
                _quizScore++;
                string quizResponse = $" CORRECT! \n\n{q.Explanation}\n\n";
                _currentQuestionIndex++;
                LogActivity("Quiz answer", $"Question {_currentQuestionIndex}: Correct!");
                return quizResponse + "━━━━━━━━━━━━━━━━━━━━━\n\n" + GetNextQuizQuestion();
            }
            else
            {
                string correctAnswer = q.Options[q.CorrectIndex];
                 string quizResponse = $" INCORRECT.\n\n";
                  quizResponse += $"The correct answer was: {correctAnswer}\n\n";
                   quizResponse += $"{q.Explanation}\n\n";
                _currentQuestionIndex++;
                LogActivity("Quiz answer", $"Question {_currentQuestionIndex}: Incorrect");
                return quizResponse + "━━━━━━━━━━━━━━━━━━━━━\n\n" + GetNextQuizQuestion();
            }
        }


        // HANDLE TOPIC DEFINITION 
        // Shows a definition for the requested topic
        private string HandleTopicDefinition(string topic)
        {
            // Reset the definition shown flag when we start a new topic
            _definitionShown = false;
            CurrentTopic = topic;

            // Check if we have a definition for this topic
            if (_topicDefinitions.ContainsKey(topic))
            {
                _definitionShown = true;
                RememberFavouriteTopic(topic, topic);

                string definition = _topicDefinitions[topic];
                string topicResult = $"{definition}\n\n";
                topicResult += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                 topicResult += $" Would you like me to share some tips about {topic}?\n";
                  topicResult += $"   Type 'tell me more' for tips, or ask about another topic!\n\n";
                   topicResult += $" Available topics: passwords, phishing, scams, privacy, 2FA, malware, WiFi, updates";

                LogActivity("Topic definition shown", $"Definition for '{topic}' displayed");
                return topicResult;
            }

            return $"I don't have a definition for '{topic}'. Try asking about passwords, phishing, scams, privacy, 2FA, malware, WiFi, or updates!";
        }


        // HANDLE TELL ME MORE 
        // Shows additional tips when the user asks for more information
        private string HandleTellMeMore()
        {
            if (string.IsNullOrEmpty(CurrentTopic))
            {
                return "What topic would you like more information on?\n\n" +
                       "Try asking: 'What is phishing?' or 'Tell me about passwords'";
            }

            // If we haven't shown the definition yet, show it first
            if (!_definitionShown && _topicDefinitions.ContainsKey(CurrentTopic))
            {
                string definition = _topicDefinitions[CurrentTopic];
                _definitionShown = true;
                return $"{definition}\n\n Type 'tell me more' again for tips on {CurrentTopic}!";
            }

            // Now show tips
            if (_keywordResponses.ContainsKey(CurrentTopic))
            {
                int randomIndex = _random.Next(_keywordResponses[CurrentTopic].Count);
                string tip = _keywordResponses[CurrentTopic][randomIndex];

                // Check if we have more tips to offer
                string morePrompt = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                morePrompt += $" Want another tip about {CurrentTopic}? Type 'tell me more' again!\n";
                morePrompt += $"   Or type 'change topic' to switch to something else.";

                LogActivity("Tips shown", $"Tips for '{CurrentTopic}' displayed");
                return $" TIPS ABOUT {CurrentTopic.ToUpper()}:\n\n{tip}\n\n{morePrompt}";
            }

            return $"I don't have tips saved for '{CurrentTopic}', but I can help you learn about passwords, phishing, scams, privacy, 2FA, malware, WiFi, or updates!";
        }


        // DETECT TOPIC FROM INPUT 
        // Figures out which topic the user is asking about
        private string? DetectTopic(string input)
        {
            string lowerInput = input.ToLower();

            foreach (var kvp in _allKeywords)
            {
                foreach (var keyword in kvp.Value)
                {
                    if (lowerInput.Contains(keyword))
                    {
                        return kvp.Key;
                    }
                }
            }
            return null;
        }


        // CHECK IF ASKING FOR DEFINITION - NEW!
        // Detects if the user wants a definition
        private bool IsAskingForDefinition(string input)
        {
            string lowerInput = input.ToLower();
            return _definitionKeywords.Any(k => lowerInput.Contains(k));
        }


        // CHECK IF ASKING TO CHANGE TOPIC - NEW!
        // Detects if the user wants to switch topics
        private bool IsAskingToChangeTopic(string input)
        {
            string lowerInput = input.ToLower();
            return _topicSwitchKeywords.Any(k => lowerInput.Contains(k));
        }


        // CHECK IF ASKING FOR MORE - NEW!
        // Detects if the user wants more information
        private bool IsAskingForMore(string input)
        {
            string lowerInput = input.ToLower();
            return _wantMoreKeywords.Any(k => lowerInput.Contains(k));
        }


        // MAIN PROCESS INPUT METHOD (MODIFIED FOR PART 3)
        // This is the most important method - it processes everything the user says
        public string ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch that. Could you try rephrasing?";

            string input = userInput.Trim().ToLower();


            // STEP 1: Check for Activity Log request
            // If the user asks for the activity log, show it
            if (_activityLogKeywords.Any(k => input.Contains(k)))
            {
                return GetActivityLog();
            }


            // STEP 2: Check for Quiz commands
            // If the user wants to start a quiz, begin it
            if (_quizKeywords.Any(k => input.Contains(k)))
            {
                StartQuiz();
                LogActivity("Quiz started", "User started the cybersecurity quiz");
                return "🎮 QUIZ STARTED!\n\n" + GetNextQuizQuestion();
            }


            // STEP 3: Process Quiz answer if active
            // If the quiz is running, process the user's answer
            if (_quizActive)
            {
                return ProcessQuizAnswer(input);
            }


            // STEP 4: Check for Task commands
            // If the user is managing tasks, handle it
            string? taskResponse = ProcessTaskCommand(input);
            if (taskResponse != null)
            {
                return taskResponse;
            }


            // STEP 5: Check for "tell me more" - NEW!
            // If the user wants more info, provide it
            if (IsAskingForMore(input))
            {
                return HandleTellMeMore();
            }


            // STEP 6: Check for topic switching 
            // If the user wants to change topics, handle it
            if (IsAskingToChangeTopic(input))
            {
                // Extract the new topic from the input
                string? newTopic = DetectTopic(input);
                if (newTopic != null)
                {
                    // Reset the definition flag for the new topic
                    _definitionShown = false;
                    CurrentTopic = newTopic;
                    return HandleTopicDefinition(newTopic);
                }
                else
                {
                    return "What topic would you like to switch to?\n\n" +
                           " Available topics: passwords, phishing, scams, privacy, 2FA, malware, WiFi, updates\n\n" +
                           "Example: 'Tell me about phishing' or 'Switch to 2FA'";
                }
            }


            // STEP 7: Check if asking for a definition 
            // If the user wants a definition, provide it
            if (IsAskingForDefinition(input))
            {
                string? topic = DetectTopic(input);
                if (topic != null)
                {
                    return HandleTopicDefinition(topic);
                }
                else
                {
                    return "I can help you understand cybersecurity terms!\n\n" +
                           "Try asking:\n" +
                           "• 'What is phishing?'\n" +
                           "• 'Define 2FA'\n" +
                           "• 'What are scams?'\n" +
                           "• 'Explain malware'\n\n" +
                           "Or ask me about passwords, privacy, or WiFi safety!";
                }
            }


            // STEP 8: Check Sentiment 
            // Detect how the user is feeling
            string sentimentResponse = CheckSentiment(input);

            // STEP 9: Check for Keyword matches (ORIGINAL)
            // See if the user mentioned a cybersecurity topic
            // First try to detect a topic
            string? detectedTopic = DetectTopic(input);
            if (detectedTopic != null)
            {
                // If it's a different topic than current, show definition first
                if (detectedTopic != CurrentTopic)
                {
                    CurrentTopic = detectedTopic;
                    _definitionShown = false;
                    string definitionResult = HandleTopicDefinition(detectedTopic);
                    if (!string.IsNullOrEmpty(sentimentResponse))
                        return sentimentResponse + "\n\n" + definitionResult;
                    return definitionResult;
                }

                // Same topic - show a tip
                if (_keywordResponses.ContainsKey(detectedTopic))
                {
                    int randomIndex = _random.Next(_keywordResponses[detectedTopic].Count);
                    string tip = _keywordResponses[detectedTopic][randomIndex];

                    // If we haven't shown the definition for this topic yet, show it first
                    if (!_definitionShown && _topicDefinitions.ContainsKey(detectedTopic))
                    {
                        _definitionShown = true;
                        string definition = _topicDefinitions[detectedTopic];
                        string topicWithTipResult = $"{definition}\n\n";
                        topicWithTipResult += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                        topicWithTipResult += $" Here's a helpful tip:\n{tip}\n\n";
                        topicWithTipResult += $"Type 'tell me more' for additional tips on {detectedTopic}!";
                        if (!string.IsNullOrEmpty(sentimentResponse))
                            return sentimentResponse + "\n\n" + topicWithTipResult;
                        return topicWithTipResult;
                    }

                    RememberFavouriteTopic(detectedTopic, input);

                    string tipOnlyResult = $"TIP ABOUT {detectedTopic.ToUpper()}:\n\n{tip}\n\n";
                    tipOnlyResult += "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
                    tipOnlyResult += $" Want more tips? Type 'tell me more'\n";
                    tipOnlyResult += $"   Want to change topic? Type 'change topic'\n";
                    tipOnlyResult += $"   Or ask 'What is {detectedTopic}?' for a definition!";

                    if (!string.IsNullOrEmpty(sentimentResponse))
                        return sentimentResponse + "\n\n" + tipOnlyResult;
                    return tipOnlyResult;
                }
            }


            // STEP 10: Sentiment-only response 
            // If the user expressed emotion but no topic, respond to the emotion
            if (!string.IsNullOrEmpty(sentimentResponse))
                return sentimentResponse + "\n\n" +
                       "Would you like me to share some cybersecurity tips? Try asking:\n" +
                       "• 'What is phishing?' (for a definition)\n" +
                       "• 'Tell me about passwords' (for tips)\n" +
                       "• 'Change topic to 2FA' (to switch)\n\n" +
                       "Or try my new features:\n" +
                       "• 'Add task to enable 2FA'\n" +
                       "• 'Start quiz'\n" +
                       "• 'Show my tasks'\n" +
                       "• 'Activity log'";


            // STEP 11: Greetings 
            // Respond to greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey") ||
                input.Contains("good day") || input.Contains("good morning") || input.Contains("good evening"))
            {
                return $"Hello {UserName}! Great to see you. \n\n" +
                       "I've been enhanced with new features:\n" +
                       "•  Task Manager - 'Add task to enable 2FA'\n" +
                       "•  Cybersecurity Quiz - 'Start quiz'\n" +
                       "•  Activity Log - 'Activity log'\n" +
                       "•  Topic Definitions - 'What is phishing?'\n" +
                       "•  Topic Switching - 'Change topic to passwords'\n\n" +
                       "What would you like to do today?";
            }


            // STEP 12: Thanks (ORIGINAL)
            // Respond to thanks
            if (input.Contains("thank"))
                return $"You're very welcome, {UserName}! Staying informed is the first step to staying safe online. 😊\n\n" +
                       "Remember, you can ask me about any cybersecurity topic at any time!";


            // STEP 13: Default with helpful suggestions
            // If nothing else matched, show a helpful menu
            return $"I'm not sure I understand, {UserName}.\n\n" +
                   "Here's what you can ask me:\n" +
                   "─────────────────────────\n" +
                   " DEFINITIONS: 'What is phishing?' 'Define 2FA'\n" +
                   " TIPS: 'Tell me about passwords'\n" +
                   " SWITCH TOPIC: 'Change topic to privacy'\n" +
                   " Tasks: 'Add task to enable 2FA'\n" +
                   " Quiz: 'Start quiz'\n" +
                   " Activity log: 'Show activity log'\n\n" +
                   "How can I help you today?";
        }


        // ORIGINAL METHODS (KEEP ALL OF THESE)
        // These are from the original chatbot code

        // Checks if the user expressed any sentiment/emotion
        private string CheckSentiment(string input)
        {
            foreach (var sentiment in _sentimentMap)
            {
                if (input.Contains(sentiment.Key))
                {
                    OnSentimentDetected?.Invoke(sentiment.Key, sentiment.Value.colorHex);
                    return sentiment.Value.message;
                }
            }
            return "";
        }

        // Remembers if the user said they like a particular topic
        private void RememberFavouriteTopic(string keyword, string fullInput)
        {
            if (fullInput.Contains("interested in") || fullInput.Contains("i like") ||
                fullInput.Contains("i love") || fullInput.Contains("i want to learn about") ||
                fullInput.Contains("favourite topic"))
            {
                FavouriteTopic = keyword;
                LastTopic = keyword;
                LogActivity("Favourite topic set", $"User interested in {keyword}");
            }
        }

        // Manually sets the user's favorite topic
        public string SetFavouriteTopic(string topic)
        {
            FavouriteTopic = topic;
            LogActivity("Favourite topic set", $"{topic}");
            return $"Great! I'll remember that you're interested in {topic}. It's a crucial part of staying safe online. 🔒\n\nI'll tailor my tips to your interest as we chat!";
        }

        // Gets the welcome message when the user first starts chatting
        public string GetWelcomeResponse(string name)
        {
            UserName = name;
            LogActivity("User session started", $"{name} started chatting");

            return $"Welcome, {UserName}! 🛡️\n" +
                   "═══════════════════════════════════════════\n\n" +
                   "I'm your Cybersecurity Awareness Assistant with NEW features:\n\n" +
                   " DEFINITIONS: Ask 'What is phishing?' or 'Define 2FA'\n" +
                   " TIPS: Ask 'Tell me about passwords' for helpful tips\n" +
                   " SWITCH TOPIC: Type 'Change topic to privacy'\n" +
                   " TASK MANAGER: 'Add task to enable 2FA'\n" +
                   " CYBERSECURITY QUIZ: 'Start quiz'\n" +
                   " ACTIVITY LOG: 'Show activity log'\n\n" +
                   "How are you doing today? ";
        }

        // Gets detailed information about a specific topic
        public string GetTopicInfo(string topic)
        {
            CurrentTopic = topic;
            _definitionShown = true; // Mark as shown since this is a direct request
            LogActivity("Topic viewed", $"{topic}");

            // Return the full definition and tips
            string definition = _topicDefinitions.ContainsKey(topic) ? _topicDefinitions[topic] : "";
            string tips = "";

            if (_keywordResponses.ContainsKey(topic))
            {
                tips = string.Join("\n• ", _keywordResponses[topic].Take(3));
                tips = "• " + tips;
            }

            string topicInfoResult = $"{definition}\n\n";
            topicInfoResult += $"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n\n";
            topicInfoResult += $" TOP TIPS:\n{tips}\n\n";
            topicInfoResult += $"Type 'tell me more' for additional tips, or 'change topic' to switch topics!";

            return topicInfoResult;
        }
    }


    // SUPPORTING CLASSES
    // These are helper classes used by the chatbot

    // Represents a single quiz question with options and answer
    public class QuizQuestion
    {
        public string Text { get; set; } = ""; // The question text
        public List<string> Options { get; set; } = new List<string>(); // The answer choices
        public int CorrectIndex { get; set; } // Which option is correct (0-based index)
        public string Explanation { get; set; } = ""; // Why the answer is correct
    }

    // Represents a single entry in the activity log
    public class ActivityLogEntry
    {
        public DateTime Timestamp { get; set; } // When the activity happened
        public string Action { get; set; } = ""; // What the user did
        public string Details { get; set; } = ""; // More information about the action
    }
}