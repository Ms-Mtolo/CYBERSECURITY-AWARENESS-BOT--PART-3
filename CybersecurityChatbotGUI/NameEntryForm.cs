using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace CybersecurityChatbotGUI
{
    public class NameEntryForm : Form
    {
        
        // These are all the visual controls on the form
        private Panel headerPanel = null!; // The top section with the logo
         private Label asciiArtLabel = null!; // Shows the ASCII art logo
          private Label subtitleLabel = null!; // Shows the "STAY SAFE ONLINE" text
           private Label instructionLabel = null!; // Tells the user to enter their name
            private TextBox nameTextBox = null!; // Where the user types their name
             private Button startButton = null!; // The button to start chatting
              private Label errorLabel = null!; // Shows error message if name is empty

        // Constructor - runs when the form is created
        public NameEntryForm()
        {
            PlayVoiceGreeting(); // Play a welcome sound 
            SetupWindow(); // set up window method
            BuildUI(); // Create all the visual controls
        }

        // Sets up the window properties
        private void SetupWindow()
        {
            this.Text = "Cybersecurity Awareness Bot";
             this.Size = new Size(680, 600);
              this.StartPosition = FormStartPosition.CenterScreen; // Center on screen
               this.BackColor = Color.FromArgb(13, 17, 23); // Dark background
               this.FormBorderStyle = FormBorderStyle.FixedSingle; // Can't resize
                this.MaximizeBox = false; // Can't maximize
                 this.Font = new Font("Consolas", 9f); // Monospace font for tech feel
        }

        // Builds all the visual controls on the form
        private void BuildUI()
        {
            
            // HEADER PANEL - The top section with the logo
            
            headerPanel = new Panel();
             headerPanel.BackColor = Color.FromArgb(0, 40, 20); // Dark green
              headerPanel.Size = new Size(680, 280);
               headerPanel.Location = new Point(0, 0);

            // ASCII Art Logo - creates a cool text-based logo
            asciiArtLabel = new Label();
            asciiArtLabel.Text =
                "  ╔═══════════════════════════════════════════════════╗\n" +
                "  ║        CYBERSECURITY AWARENESS BOT               ║\n" +
                "  ╠═══════════════════════════════════════════════════╣\n" +
                "  ║  ██████╗██╗   ██╗██████╗ ███████╗██████╗        ║\n" +
                "  ║ ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗       ║\n" +
                "  ║ ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝       ║\n" +
                "  ║ ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗       ║\n" +
                "  ║ ╚██████╗   ██║   ██████╔╝███████╗██║  ██║       ║\n" +
                "  ║  ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝       ║\n" +
                "  ╚═══════════════════════════════════════════════════╝";
            asciiArtLabel.Font = new Font("Consolas", 8.5f, FontStyle.Bold);
             asciiArtLabel.ForeColor = Color.LimeGreen;
              asciiArtLabel.BackColor = Color.Transparent;
               asciiArtLabel.AutoSize = false;
                asciiArtLabel.Size = new Size(660, 220);
                 asciiArtLabel.Location = new Point(10, 10);
                  asciiArtLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Subtitle - shows "STAY SAFE ONLINE" with lock emoji
            subtitleLabel = new Label();
             subtitleLabel.Text = "  STAY SAFE ONLINE  ";
              subtitleLabel.Font = new Font("Consolas", 12f, FontStyle.Bold);
               subtitleLabel.ForeColor = Color.Cyan;
                subtitleLabel.BackColor = Color.Transparent;
                 subtitleLabel.AutoSize = false;
                  subtitleLabel.Size = new Size(660, 30);
                   subtitleLabel.Location = new Point(0, 235);
                    subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;

            headerPanel.Controls.Add(asciiArtLabel);
             headerPanel.Controls.Add(subtitleLabel);

            
            // INSTRUCTION - Tells the user what to do
            
            instructionLabel = new Label();
             instructionLabel.Text = "Please enter your name to begin:";
              instructionLabel.Font = new Font("Consolas", 11f);
               instructionLabel.ForeColor = Color.White;
                instructionLabel.AutoSize = false;
                 instructionLabel.Size = new Size(500, 30);
                  instructionLabel.Location = new Point(90, 310);

           
            // NAME TEXTBOX - Where the user types their name
           
            nameTextBox = new TextBox();
             nameTextBox.Font = new Font("Consolas", 14f);
              nameTextBox.BackColor = Color.FromArgb(30, 40, 50);
               nameTextBox.ForeColor = Color.LimeGreen;
                nameTextBox.BorderStyle = BorderStyle.FixedSingle;
                 nameTextBox.Size = new Size(380, 40);
                  nameTextBox.Location = new Point(90, 350);
                   nameTextBox.PlaceholderText = "Type your name here..."; // Hint text
                    nameTextBox.KeyPress += NameTextBox_KeyPress; // Enter key starts chat

            
            // ERROR LABEL - Shows when user tries to start without a name
           
            errorLabel = new Label();
             errorLabel.Text = "Please enter your name before continuing.";
              errorLabel.ForeColor = Color.OrangeRed;
               errorLabel.Font = new Font("Consolas", 9f);
                errorLabel.AutoSize = false;
                 errorLabel.Size = new Size(460, 25);
                  errorLabel.Location = new Point(90, 395);
                   errorLabel.Visible = false; // Hidden until there's an error

            
            // START BUTTON - Opens the main chat window
           
            startButton = new Button();
             startButton.Text = " START CHATTING";
              startButton.Font = new Font("Consolas", 12f, FontStyle.Bold);
               startButton.BackColor = Color.FromArgb(0, 100, 50);
                startButton.ForeColor = Color.White;
                 startButton.FlatStyle = FlatStyle.Flat;
                  startButton.Size = new Size(380, 50);
                   startButton.Location = new Point(90, 430);
                    startButton.Cursor = Cursors.Hand; // Hand cursor on hover
                     startButton.FlatAppearance.BorderColor = Color.LimeGreen;
                      startButton.Click += StartButton_Click;

            
            // VERSION LABEL - Shows the version at the bottom
            
            var versionLabel = new Label();
             versionLabel.Text = "Cybersecurity Awareness Bot  •  Part 3 Complete";
              versionLabel.ForeColor = Color.DimGray;
               versionLabel.Font = new Font("Consolas", 8f);
                versionLabel.AutoSize = false;
                 versionLabel.Size = new Size(660, 20);
                  versionLabel.Location = new Point(0, 555);
                   versionLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Add all controls to the form
            this.Controls.Add(headerPanel);
             this.Controls.Add(instructionLabel);
              this.Controls.Add(nameTextBox);
               this.Controls.Add(errorLabel);
                this.Controls.Add(startButton);
                 this.Controls.Add(versionLabel);
        }

        // Handles the Start button click - validates name and opens chat
private void StartButton_Click(object? sender, EventArgs e)
        {
            string name = nameTextBox.Text.Trim();

            // Check if the user entered a name
            if (string.IsNullOrWhiteSpace(name))
            {
                errorLabel.Visible = true; // Show the error message
                nameTextBox.Focus(); // Put cursor back in the textbox
                return;
            }

            // Capitalize the first letter, make the rest lowercase
            name = char.ToUpper(name[0]) + name.Substring(1).ToLower();

            // Create and show the main chat form
            MainChatForm chatForm = new MainChatForm(name);
            chatForm.Show();
            this.Hide(); // Hide the name entry form

            // When the chat form closes, close this form too
            chatForm.FormClosed += (s, args) => this.Close();
        }

        // Handles the Enter key press in the textbox - starts chat
private void NameTextBox_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                StartButton_Click(sender, EventArgs.Empty);
            }
        }

        // Plays a voice greeting sound if the file exists
private void PlayVoiceGreeting()
        {
            try
            {
                string audioFilePath = "Greetings.wav";
                if (File.Exists(audioFilePath))
                {
                    SoundPlayer player = new SoundPlayer(audioFilePath);
                    player.Play(); // Plays the sound asynchronously
                }
            }
            catch
            {
                // If the file is missing or corrupted, just continue silently
            }
        }
    }
}