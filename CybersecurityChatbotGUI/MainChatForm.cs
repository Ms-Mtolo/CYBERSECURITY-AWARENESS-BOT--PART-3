using System;
using System.Drawing;
using System.Windows.Forms;

namespace CybersecurityChatbotGUI
{
public class MainChatForm : Form
    {
       
        // The brain is the chatbot logic - it does all the thinking
        private ChatbotBrain _brain = null!;
        // Stores the color for sentiment detection (changes based on user mood)
        private Color _currentSentimentColor;

        // Existing controls - these are all the visual elements on the form
        private Panel headerPanel = null!;
         private Label titleLabel = null!;
          private Label userStatusLabel = null!;
           private RichTextBox chatDisplay = null!;
            private TextBox inputBox = null!;
             private Button sendButton = null!;
              private Panel topicButtonPanel = null!;
               private Panel memoryPanel = null!;
                private Label memoryLabel = null!;
                 private Label sentimentLabel = null!;
                  private Panel bottomPanel = null!;

        
        private Panel taskPanel = null!; // The panel that holds task-related controls
         private ListBox taskListBox = null!; // Shows the list of tasks
          private Button refreshTasksButton = null!; // Refreshes the task list
           private Button addTaskFromPanelButton = null!; // Adds a new task
            private Button completeTaskButton = null!; // Marks a task as complete
             private Button deleteTaskButton = null!; // Deletes a task
              private Button startQuizButton = null!; // Starts the cybersecurity quiz
               private Button activityLogButton = null!; // Shows the activity log
                private Button changeTopicButton = null!; // Opens the topic switching popup

        
        // This runs when the form is created - sets everything up
public MainChatForm(string userName)
        {
            // Create the brain and set the user's name
            _brain = new ChatbotBrain();
            _brain.UserName = userName;

            // Wire up the delegate - this lets the brain tell the UI about user emotions
            _brain.OnSentimentDetected = HandleSentimentDetected;

            // Build the window
            SetupWindow();
            BuildUI();

            // Show welcome message after controls are ready
            this.Load += MainChatForm_Load;
        }

        
        // Runs when the form first loads - shows welcome message
private void MainChatForm_Load(object? sender, EventArgs e)
        {
            string welcome = _brain.GetWelcomeResponse(_brain.UserName);
            AppendBotMessage(welcome);
            inputBox.Focus(); // Put cursor in the input box so user can start typing
            RefreshTasks(); // Load any saved tasks from the database
        }

        
        // Sets the window properties like size, title, and colors
private void SetupWindow()
        {
            this.Text = $"Cybersecurity Bot - Chatting with {_brain.UserName}";
             this.Size = new Size(1150, 780); // WIDER to fit expanded task panel
              this.StartPosition = FormStartPosition.CenterScreen; // Center on screen
              this.BackColor = Color.FromArgb(13, 17, 23); // Dark background
               this.Font = new Font("Consolas", 9f); // Monospace font for tech feel
                this.MinimumSize = new Size(950, 700); // Prevent window from getting too small
        }

        
        // Creates all the visual controls and arranges them on the form
private void BuildUI()
        {
            
            headerPanel = new Panel();
             headerPanel.BackColor = Color.FromArgb(0, 40, 20); // Dark green
              headerPanel.Dock = DockStyle.Top; // Sticks to the top
               headerPanel.Height = 60;

            titleLabel = new Label();
             titleLabel.Text = " CYBERSECURITY AWARENESS BOT";
              titleLabel.Font = new Font("Consolas", 14f, FontStyle.Bold);
               titleLabel.ForeColor = Color.LimeGreen;
                titleLabel.AutoSize = false;
                 titleLabel.Size = new Size(500, 60);
                  titleLabel.Location = new Point(15, 0);
                   titleLabel.TextAlign = ContentAlignment.MiddleLeft;

            userStatusLabel = new Label();
             userStatusLabel.Text = $" {_brain.UserName}";
              userStatusLabel.Font = new Font("Consolas", 10f);
               userStatusLabel.ForeColor = Color.Cyan;
                userStatusLabel.AutoSize = false;
                 userStatusLabel.Size = new Size(350, 60);
                  userStatusLabel.Location = new Point(530, 0);
                   userStatusLabel.TextAlign = ContentAlignment.MiddleRight;

            headerPanel.Controls.Add(titleLabel);
             headerPanel.Controls.Add(userStatusLabel);

            
            // Shows the user's detected mood/emotion
            
            sentimentLabel = new Label();
             sentimentLabel.Text = " Mood: Neutral";
              sentimentLabel.Font = new Font("Consolas", 9f);
               sentimentLabel.ForeColor = Color.Gray;
                sentimentLabel.BackColor = Color.FromArgb(20, 25, 35);
                 sentimentLabel.Dock = DockStyle.None;
                  sentimentLabel.AutoSize = false;
                   sentimentLabel.Size = new Size(1150, 25);
                    sentimentLabel.Location = new Point(0, 60);
                     sentimentLabel.TextAlign = ContentAlignment.MiddleLeft;
                      sentimentLabel.Padding = new Padding(10, 0, 0, 0);

           
            // These buttons let users quickly select a topic
           
            topicButtonPanel = new Panel();
             topicButtonPanel.BackColor = Color.FromArgb(20, 30, 20);
              topicButtonPanel.AutoSize = false;
               topicButtonPanel.Size = new Size(1150, 50);
                topicButtonPanel.Location = new Point(0, 85);

            var topicLabel = new Label();
             topicLabel.Text = "Quick Topics:";
              topicLabel.ForeColor = Color.Gray;
               topicLabel.Font = new Font("Consolas", 8f);
                topicLabel.AutoSize = true;
                 topicLabel.Location = new Point(8, 16);

            // Helper method to create each topic button
            Button MakeTopicButton(string text, string topic, int x)
            {
                var btn = new Button();
                 btn.Text = text;
                  btn.Tag = topic; // Store the topic name in the Tag property
                   btn.Font = new Font("Consolas", 8f);
                    btn.BackColor = Color.FromArgb(0, 60, 30);
                     btn.ForeColor = Color.LimeGreen;
                      btn.FlatStyle = FlatStyle.Flat;
                       btn.Size = new Size(115, 32);
                        btn.Location = new Point(x, 9);
                         btn.Cursor = Cursors.Hand; // Show hand cursor on hover
                          btn.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 60);
                           btn.Click += TopicButton_Click; // All buttons use the same click handler
                return btn;
            }

            topicButtonPanel.Controls.Add(topicLabel);
             topicButtonPanel.Controls.Add(MakeTopicButton(" Passwords", "password", 100));
              topicButtonPanel.Controls.Add(MakeTopicButton(" Phishing", "phishing", 220));
               topicButtonPanel.Controls.Add(MakeTopicButton(" Privacy", "privacy", 340));
                topicButtonPanel.Controls.Add(MakeTopicButton(" 2FA", "2fa", 460));
                 topicButtonPanel.Controls.Add(MakeTopicButton(" Scams", "scam", 580));
                  topicButtonPanel.Controls.Add(MakeTopicButton(" WiFi", "wifi", 700));
                   topicButtonPanel.Controls.Add(MakeTopicButton(" Updates", "update", 820));

            
            // Shows what the bot remembers about the user
           
            memoryPanel = new Panel();
             memoryPanel.BackColor = Color.FromArgb(20, 20, 40);
              memoryPanel.Size = new Size(1150, 28);
               memoryPanel.Location = new Point(0, 135);
                memoryPanel.Visible = false; // Hidden until the bot remembers something

            memoryLabel = new Label();
             memoryLabel.Text = " Memory: ...";
              memoryLabel.Font = new Font("Consolas", 8.5f, FontStyle.Italic);
               memoryLabel.ForeColor = Color.Plum;
                memoryLabel.AutoSize = false;
                 memoryLabel.Size = new Size(1130, 28);
                  memoryLabel.Location = new Point(10, 0);
                   memoryLabel.TextAlign = ContentAlignment.MiddleLeft;

            memoryPanel.Controls.Add(memoryLabel);

            
            // The main area where messages appear
           
            chatDisplay = new RichTextBox();
             chatDisplay.BackColor = Color.FromArgb(13, 17, 23);
              chatDisplay.ForeColor = Color.White;
               chatDisplay.Font = new Font("Consolas", 10f);
                chatDisplay.ReadOnly = true; // User can't type here
                 chatDisplay.BorderStyle = BorderStyle.None;
                  chatDisplay.ScrollBars = RichTextBoxScrollBars.Vertical;
                   chatDisplay.Size = new Size(830, 430);
                    chatDisplay.Location = new Point(0, 163);
                     chatDisplay.Padding = new Padding(10);
                      chatDisplay.WordWrap = true;

            
            // Shows tasks and provides task management buttons
            
            taskPanel = new Panel();
             taskPanel.BackColor = Color.FromArgb(20, 25, 35);
              taskPanel.Size = new Size(310, 430); // WIDER from 260 to 310
               taskPanel.Location = new Point(840, 163);
                taskPanel.BorderStyle = BorderStyle.FixedSingle;

            var taskPanelTitle = new Label();
             taskPanelTitle.Text = "📋 MY TASKS";
              taskPanelTitle.Font = new Font("Consolas", 10f, FontStyle.Bold);
               taskPanelTitle.ForeColor = Color.LimeGreen;
                taskPanelTitle.Size = new Size(290, 30);
                 taskPanelTitle.Location = new Point(10, 5);
                  taskPanelTitle.TextAlign = ContentAlignment.MiddleCenter;

            // This shows the list of tasks
            taskListBox = new ListBox();
             taskListBox.BackColor = Color.FromArgb(30, 35, 45);
              taskListBox.ForeColor = Color.White;
               taskListBox.Font = new Font("Consolas", 8f);
                taskListBox.Size = new Size(290, 140); // WIDER
                 taskListBox.Location = new Point(10, 40);

            
            // Refresh button - reloads tasks from database
            refreshTasksButton = new Button();
             refreshTasksButton.Text = "🔄 Refresh";
              refreshTasksButton.BackColor = Color.FromArgb(0, 80, 40);
               refreshTasksButton.ForeColor = Color.White;
                refreshTasksButton.FlatStyle = FlatStyle.Flat;
                 refreshTasksButton.Size = new Size(90, 32);
                  refreshTasksButton.Location = new Point(10, 190);
                   refreshTasksButton.Cursor = Cursors.Hand;
                    refreshTasksButton.FlatAppearance.BorderColor = Color.LimeGreen;
                     refreshTasksButton.FlatAppearance.BorderSize = 1;
                      refreshTasksButton.Click += RefreshTasksButton_Click;

            // Add Task button - opens input box for new task
            addTaskFromPanelButton = new Button();
             addTaskFromPanelButton.Text = " Add Task";
              addTaskFromPanelButton.BackColor = Color.FromArgb(0, 100, 50);
               addTaskFromPanelButton.ForeColor = Color.White;
                addTaskFromPanelButton.FlatStyle = FlatStyle.Flat;
                 addTaskFromPanelButton.Size = new Size(90, 32);
                  addTaskFromPanelButton.Location = new Point(108, 190);
                   addTaskFromPanelButton.Cursor = Cursors.Hand;
                    addTaskFromPanelButton.FlatAppearance.BorderColor = Color.LimeGreen;
                     addTaskFromPanelButton.FlatAppearance.BorderSize = 1;
                      addTaskFromPanelButton.Click += AddTaskFromPanel_Click;

            // Complete button - marks selected task as done
            completeTaskButton = new Button();
             completeTaskButton.Text = " Complete";
              completeTaskButton.BackColor = Color.FromArgb(0, 80, 40);
               completeTaskButton.ForeColor = Color.White;
                completeTaskButton.FlatStyle = FlatStyle.Flat;
                 completeTaskButton.Size = new Size(90, 32);
                  completeTaskButton.Location = new Point(206, 190);
                   completeTaskButton.Cursor = Cursors.Hand;
                    completeTaskButton.FlatAppearance.BorderColor = Color.LimeGreen;
                     completeTaskButton.FlatAppearance.BorderSize = 1;
                      completeTaskButton.Click += CompleteTaskButton_Click;

            // Delete button - removes selected task
            deleteTaskButton = new Button();
             deleteTaskButton.Text = " Delete";
              deleteTaskButton.BackColor = Color.FromArgb(80, 20, 20);
               deleteTaskButton.ForeColor = Color.White;
                deleteTaskButton.FlatStyle = FlatStyle.Flat;
                 deleteTaskButton.Size = new Size(90, 32);
                  deleteTaskButton.Location = new Point(10, 228);
                   deleteTaskButton.Cursor = Cursors.Hand;
                    deleteTaskButton.FlatAppearance.BorderColor = Color.Red;
                     deleteTaskButton.FlatAppearance.BorderSize = 1;
                      deleteTaskButton.Click += DeleteTaskButton_Click;

            // Start Quiz button - begins the cybersecurity quiz
            startQuizButton = new Button();
             startQuizButton.Text = " Start Quiz";
              startQuizButton.BackColor = Color.FromArgb(100, 50, 0);
               startQuizButton.ForeColor = Color.White;
                startQuizButton.FlatStyle = FlatStyle.Flat;
                 startQuizButton.Size = new Size(135, 35);
                  startQuizButton.Location = new Point(10, 270);
                   startQuizButton.Cursor = Cursors.Hand;
                    startQuizButton.FlatAppearance.BorderColor = Color.Orange;
                     startQuizButton.FlatAppearance.BorderSize = 1;
                      startQuizButton.Click += StartQuizButton_Click;

            // Activity Log button - shows what the user has done
            activityLogButton = new Button();
             activityLogButton.Text = " Activity Log";
              activityLogButton.BackColor = Color.FromArgb(0, 50, 80);
               activityLogButton.ForeColor = Color.White;
                activityLogButton.FlatStyle = FlatStyle.Flat;
                 activityLogButton.Size = new Size(135, 35);
                  activityLogButton.Location = new Point(155, 270);
                   activityLogButton.Cursor = Cursors.Hand;
                    activityLogButton.FlatAppearance.BorderColor = Color.Cyan;
                     activityLogButton.FlatAppearance.BorderSize = 1;
                      activityLogButton.Click += ActivityLogButton_Click;

            // Change Topic button - opens popup menu to switch topics
            changeTopicButton = new Button();
             changeTopicButton.Text = " Change Topic";
              changeTopicButton.BackColor = Color.FromArgb(60, 0, 80);
               changeTopicButton.ForeColor = Color.White;
                changeTopicButton.FlatStyle = FlatStyle.Flat;
                 changeTopicButton.Size = new Size(290, 35);
                  changeTopicButton.Location = new Point(10, 315);
                   changeTopicButton.Cursor = Cursors.Hand;
                    changeTopicButton.FlatAppearance.BorderColor = Color.Magenta;
                     changeTopicButton.FlatAppearance.BorderSize = 1;
                      changeTopicButton.Click += ChangeTopicButton_Click;

            // Status label - gives instructions to the user
            var taskStatusLabel = new Label();
             taskStatusLabel.Text = "Select a task and click Complete or Delete";
              taskStatusLabel.ForeColor = Color.DimGray;
               taskStatusLabel.Font = new Font("Consolas", 7f);
                taskStatusLabel.AutoSize = false;
                 taskStatusLabel.Size = new Size(290, 20);
                  taskStatusLabel.Location = new Point(10, 360);
                   taskStatusLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Add all task panel controls
            taskPanel.Controls.Add(taskPanelTitle);
             taskPanel.Controls.Add(taskListBox);
              taskPanel.Controls.Add(refreshTasksButton);
               taskPanel.Controls.Add(addTaskFromPanelButton);
                taskPanel.Controls.Add(completeTaskButton);
                 taskPanel.Controls.Add(deleteTaskButton);
                  taskPanel.Controls.Add(startQuizButton);
                   taskPanel.Controls.Add(activityLogButton);
                    taskPanel.Controls.Add(changeTopicButton);
                     taskPanel.Controls.Add(taskStatusLabel);

            // Where the user types their messages
            bottomPanel = new Panel();
            bottomPanel.BackColor = Color.FromArgb(20, 25, 35);
            bottomPanel.BackColor = Color.FromArgb(20, 25, 35);
             bottomPanel.Size = new Size(1150, 70);
              bottomPanel.Location = new Point(0, 710);
               bottomPanel.Dock = DockStyle.Bottom;

            // The text box where the user types
            inputBox = new TextBox();
             inputBox.Font = new Font("Consolas", 11f);
              inputBox.BackColor = Color.FromArgb(30, 40, 50);
               inputBox.ForeColor = Color.White;
                inputBox.BorderStyle = BorderStyle.FixedSingle;
                 inputBox.Size = new Size(700, 40);
                  inputBox.Location = new Point(10, 15);
                   inputBox.PlaceholderText = "Type your message here and press Enter or click Send...";
                    inputBox.KeyPress += InputBox_KeyPress; // Enter key sends message

            // The Send button
            sendButton = new Button();
             sendButton.Text = "SEND  ➤";
              sendButton.Font = new Font("Consolas", 10f, FontStyle.Bold);
               sendButton.BackColor = Color.FromArgb(0, 120, 60);
                sendButton.ForeColor = Color.White;
                 sendButton.FlatStyle = FlatStyle.Flat;
                  sendButton.Size = new Size(160, 40);
                   sendButton.Location = new Point(720, 15);
                    sendButton.Cursor = Cursors.Hand;
                     sendButton.FlatAppearance.BorderColor = Color.LimeGreen;
                      sendButton.FlatAppearance.BorderSize = 1;
                       sendButton.Click += SendButton_Click;

            bottomPanel.Controls.Add(inputBox);
             bottomPanel.Controls.Add(sendButton);

           
            // ADD EVERYTHING TO THE FORM
           
            this.Controls.Add(headerPanel);
            this.Controls.Add(sentimentLabel);
            this.Controls.Add(topicButtonPanel);
            this.Controls.Add(memoryPanel);
            this.Controls.Add(chatDisplay);
            this.Controls.Add(taskPanel);
            this.Controls.Add(bottomPanel);

            this.Resize += MainChatForm_Resize; // Handle window resizing
        }

        
        // Adjusts controls when the window is resized
        private void MainChatForm_Resize(object? sender, EventArgs e)
        {
            int topY = memoryPanel.Visible ? 163 : 163;
             int bottomHeight = bottomPanel.Height;
              int available = this.ClientSize.Height - topY - bottomHeight;
               int width = this.ClientSize.Width;

            // Chat display - takes most of the space
            chatDisplay.Width = width - 330; // Adjusted for wider task panel
             chatDisplay.Height = available > 100 ? available : 100;
              chatDisplay.Top = topY;

            // Task panel - stays on the right side
            taskPanel.Width = 310;
             taskPanel.Height = available > 100 ? available : 100;
              taskPanel.Top = topY;
               taskPanel.Left = width - 330;

            // Update other controls to match new width
            bottomPanel.Width = width;
             topicButtonPanel.Width = width;
              sentimentLabel.Width = width;
               memoryPanel.Width = width;
                headerPanel.Width = width;

            userStatusLabel.Left = width - userStatusLabel.Width - 10;
             inputBox.Width = width - sendButton.Width - 50;
              sendButton.Left = width - sendButton.Width - 20;
        }

       
        
        // Sends the user's message when they click Send
        private void SendButton_Click(object? sender, EventArgs e)
        {
            ProcessUserMessage(inputBox.Text);
        }

        
        
        // Sends the message when the user presses Enter
        private void InputBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true; // Stop the Enter key from making a new line
                ProcessUserMessage(inputBox.Text);
            }
        }

        // Handles clicking any topic button
        private void TopicButton_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string topic)
            {
                AppendUserMessage($"[Clicked: {btn.Text.Trim()}]");
                string response = _brain.GetTopicInfo(topic);
                AppendBotMessage(response);
                string memMsg = _brain.SetFavouriteTopic(topic);
                UpdateMemoryPanel(topic);
                inputBox.Focus();
                RefreshTasks();
            }
        }

        
        // Processes the user's message and gets a response from the brain
        private void ProcessUserMessage(string userText)
        {
            string trimmed = userText.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                inputBox.Clear();
                return;
            }

            AppendUserMessage(trimmed);
            inputBox.Clear();

            // Send to the brain and get a response
            string response = _brain.ProcessInput(trimmed);
            AppendBotMessage(response);

            // Update memory panel if the user has a favorite topic
            if (!string.IsNullOrEmpty(_brain.FavouriteTopic))
            {
                UpdateMemoryPanel(_brain.FavouriteTopic);
            }

            inputBox.Focus();
            RefreshTasks(); // Refresh tasks in case the user added/completed one
        }

        
        // Reloads tasks from the database and updates the list
        private void RefreshTasks()
        {
            try
            {
                var tasks = _brain.GetTasks();
                taskListBox.Items.Clear();
                foreach (var task in tasks)
                {
                    taskListBox.Items.Add(task.DisplayText);
                }
            }
            catch
            {
                // Silently handle errors (like if the database isn't available)
            }
        }

        
        // Adds the user's message to the chat display
        private void AppendUserMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm");
            chatDisplay.SelectionColor = Color.Cyan;
             chatDisplay.AppendText($"\n[{timestamp}] {_brain.UserName}: ");
              chatDisplay.SelectionColor = Color.White;
               chatDisplay.AppendText(message + "\n");
                chatDisplay.ScrollToCaret(); // Scroll to show the newest message
        }

        // Adds the bot's message to the chat display
        private void AppendBotMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm");
            chatDisplay.SelectionColor = Color.FromArgb(40, 60, 40);
             chatDisplay.AppendText("─────────────────────────────────────\n");
              chatDisplay.SelectionColor = _currentSentimentColor;
               chatDisplay.AppendText($"[{timestamp}]  Bot: ");
                chatDisplay.SelectionColor = Color.FromArgb(180, 255, 180);
                 chatDisplay.AppendText(message + "\n");
                  chatDisplay.ScrollToCaret(); // Scroll to show the newest message
        }

       
        // Called by the brain when it detects user sentiment/emotion
        private void HandleSentimentDetected(string sentiment, string colorHex)
        {
            Color sentimentColor = ColorTranslator.FromHtml(colorHex);
            _currentSentimentColor = sentimentColor;

            // Choose an emoji based on the sentiment
            string emoji = sentiment switch
            {
                "worried" or "scared" or "anxious" => " ",
                "frustrated" => " ",
                "confused" => " ",
                "curious" or "excited" => " ",
                "happy" or "good" or "great" => " ",
                "terrible" or "bad" or "overwhelmed" => " ",
                _ => "💬"
            };

            sentimentLabel.Text = $"{emoji} Mood detected: {sentiment.ToUpper()}  |  Adjusting response tone...";
             sentimentLabel.ForeColor = sentimentColor;
        }

        
        // Shows what the bot remembers about the user
        private void UpdateMemoryPanel(string topic)
        {
            memoryLabel.Text = $"Memory: I remember you're interested in {topic}. I'll personalise my tips for you!";
             memoryPanel.Visible = true;
              MainChatForm_Resize(null, EventArgs.Empty);
        }

       
        

        // Refreshes the task list
        private void RefreshTasksButton_Click(object? sender, EventArgs e)
        {
            RefreshTasks();
            AppendBotMessage(" Tasks refreshed!");
        }

        // Opens a popup to add a new task
        private void AddTaskFromPanel_Click(object? sender, EventArgs e)
        {
            string taskTitle = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter your cybersecurity task:",
                "Add New Task",
                "Enable two-factor authentication");

            if (!string.IsNullOrWhiteSpace(taskTitle))
            {
                string response = _brain.AddTaskViaText($"Add task {taskTitle}");
                AppendBotMessage(response);
                RefreshTasks();
            }
        }

        // Marks the selected task as complete
        private void CompleteTaskButton_Click(object? sender, EventArgs e)
        {
            if (taskListBox.SelectedIndex >= 0)
            {
                string response = _brain.CompleteTaskByNumber(taskListBox.SelectedIndex + 1);
                AppendBotMessage(response);
                RefreshTasks();
            }
            else
            {
                AppendBotMessage("Please select a task from the list first.");
            }
        }

        // Deletes the selected task
        private void DeleteTaskButton_Click(object? sender, EventArgs e)
        {
            if (taskListBox.SelectedIndex >= 0)
            {
                string response = _brain.DeleteTaskByNumber(taskListBox.SelectedIndex + 1);
                AppendBotMessage(response);
                RefreshTasks();
            }
            else
            {
                AppendBotMessage("Please select a task from the list first.");
            }
        }

        // Starts the cybersecurity quiz
        private void StartQuizButton_Click(object? sender, EventArgs e)
        {
            _brain.StartQuiz();
            string firstQuestion = _brain.GetNextQuizQuestion();
            AppendBotMessage(firstQuestion);
        }

        // Shows the activity log
        private void ActivityLogButton_Click(object? sender, EventArgs e)
        {
            string log = _brain.GetActivityLog();
            AppendBotMessage(log);
        }

        // Opens a popup menu to change the topic
        private void ChangeTopicButton_Click(object? sender, EventArgs e)
        {
            // Get available topics from the brain
            var topics = _brain.GetAvailableTopics();
            var displayNames = topics.Select(t => _brain.GetTopicDisplayName(t)).ToArray();

            // Show a popup list for the user to choose from
            using (var popup = new Form())
            {
                popup.Text = " Change Topic";
                 popup.Size = new Size(350, 380);
                  popup.StartPosition = FormStartPosition.CenterParent;
                   popup.BackColor = Color.FromArgb(13, 17, 23);
                    popup.FormBorderStyle = FormBorderStyle.FixedDialog;
                     popup.MaximizeBox = false;
                      popup.MinimizeBox = false;

                // Instruction label
                var instruction = new Label();
                 instruction.Text = "Select a topic to learn about:";
                  instruction.Font = new Font("Consolas", 10f);
                   instruction.ForeColor = Color.White;
                    instruction.AutoSize = false;
                     instruction.Size = new Size(330, 30);
                      instruction.Location = new Point(10, 10);
                       instruction.TextAlign = ContentAlignment.MiddleCenter;

                // List box for topics - shows all available topics
                var topicList = new ListBox();
                 topicList.BackColor = Color.FromArgb(30, 35, 45);
                  topicList.ForeColor = Color.White;
                   topicList.Font = new Font("Consolas", 10f);
                    topicList.Size = new Size(330, 220);
                     topicList.Location = new Point(10, 45);
                      topicList.Items.AddRange(displayNames);
                       topicList.SelectedIndex = 0;

                // Select button - confirms the choice
                var selectButton = new Button();
                 selectButton.Text = " Select Topic";
                  selectButton.BackColor = Color.FromArgb(0, 100, 50);
                   selectButton.ForeColor = Color.White;
                    selectButton.FlatStyle = FlatStyle.Flat;
                     selectButton.Size = new Size(150, 40);
                      selectButton.Location = new Point(50, 280);
                       selectButton.Cursor = Cursors.Hand;
                        selectButton.FlatAppearance.BorderColor = Color.LimeGreen;
                         selectButton.FlatAppearance.BorderSize = 1;

                // Cancel button - closes without changing
                var cancelButton = new Button();
                 cancelButton.Text = "❌ Cancel";
                  cancelButton.BackColor = Color.FromArgb(80, 20, 20);
                   cancelButton.ForeColor = Color.White;
                    cancelButton.FlatStyle = FlatStyle.Flat;
                     cancelButton.Size = new Size(150, 40);
                      cancelButton.Location = new Point(210, 280);
                       cancelButton.Cursor = Cursors.Hand;
                        cancelButton.FlatAppearance.BorderColor = Color.Red;
                         cancelButton.FlatAppearance.BorderSize = 1;

                // When user clicks Select, close and return the chosen topic
                selectButton.Click += (s, args) =>
                {
                    if (topicList.SelectedIndex >= 0)
                    {
                        string selectedTopic = topics[topicList.SelectedIndex];
                        popup.DialogResult = DialogResult.OK;
                         popup.Tag = selectedTopic;
                          popup.Close();
                    }
                };

                // When user clicks Cancel, just close
                cancelButton.Click += (s, args) =>
                {
                    popup.DialogResult = DialogResult.Cancel;
                     popup.Close();
                };

                // Allow double-click to select
                topicList.DoubleClick += (s, args) =>
                {
                    if (topicList.SelectedIndex >= 0)
                    {
                        string selectedTopic = topics[topicList.SelectedIndex];
                        popup.DialogResult = DialogResult.OK;
                         popup.Tag = selectedTopic;
                          popup.Close();
                    }
                };

                popup.Controls.Add(instruction);
                 popup.Controls.Add(topicList);
                  popup.Controls.Add(selectButton);
                   popup.Controls.Add(cancelButton);

                // Show the popup and handle the result
                var result = popup.ShowDialog(this);

                if (result == DialogResult.OK && popup.Tag is string selectedTopic)
                {
                    // Process the topic change
                    AppendUserMessage($"[Changed topic to: {_brain.GetTopicDisplayName(selectedTopic)}]");
                    string response = _brain.GetTopicInfo(selectedTopic);
                    AppendBotMessage(response);
                    _brain.SetFavouriteTopic(selectedTopic);
                    UpdateMemoryPanel(selectedTopic);
                    RefreshTasks();
                }
            }
        }
    }
}