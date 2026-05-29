using System.IO;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProgPart2
{
    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public Brush BackgroundColor { get; set; } = Brushes.Transparent;
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;
        public bool IsUser { get; set; }
    }

    public partial class MainWindow : Window
    {
        private Dictionary<string, string> _userMemory = new Dictionary<string, string>();
        private string _currentUserName = string.Empty;
        private string _currentTopic = string.Empty;
        private int _topicDepth = 0;
        private Random _random = new Random();
        private SoundPlayer? _greetingPlayer;

        private List<string> _phishingResponses = new List<string>();
        private List<string> _passwordResponses = new List<string>();
        private List<string> _privacyResponses = new List<string>();
        private List<string> _greetings = new List<string>();
        private List<string> _farewells = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InitializeResponses();
            LoadUserNames();
        }

        // Window Loaded event - load logo and play sound AFTER window is ready
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLogo();
            PlayGreeting();
        }

        private void InitializeResponses()
        {
            _phishingResponses = new List<string>
            {
                "🔐 Heads up! Always check who sent an email before clicking links. South Africans have lost a lot of money to phishing scams.",
                "📧 Quick tip: Real banks like FNB, Capitec, and Standard Bank will NEVER ask for your password in an email.",
                "🌐 Before you type any personal info, make sure the website has 'https://' and a padlock icon.",
                "📱 Those texts about 'unclaimed parcels' or 'account frozen' are usually scams. Don't click the links!",
                "🎣 Watch out for messages that say 'URGENT' or have spelling mistakes - that's how you spot a scam.",
                "📞 Never give your OTP or PIN to anyone over the phone, no matter who they say they are.",
                "💼 If your 'boss' emails asking for urgent payment, call them first to check. It's a common trick.",
                "🛡️ Turn on 2-factor authentication for your important accounts - it's an extra layer of protection."
            };

            _passwordResponses = new List<string>
            {
                "🔑 Make your password at least 12 characters long with a mix of letters, numbers, and symbols.",
                "🔄 Don't use the same password on different sites. If one gets hacked, they all get hacked.",
                "🗝️ Try a password manager like Bitwarden or LastPass - they remember all your passwords for you.",
                "📅 Change your important passwords every few months, just to be safe.",
                "🚫 Don't use your name, birthday, or ID number in passwords. Hackers look for that stuff online.",
                "🔐 'SouthAfrica123' or 'Springboks' - these are too easy to guess. Pick something harder.",
                "💡 Use a passphrase like 'MyCatLovesEatingTuna2024!' - easier to remember, harder to crack.",
                "📊 Check 'haveibeenpwned.com' to see if your email has been in any data breaches."
            };

            _privacyResponses = new List<string>
            {
                "📱 Go check your Facebook and Instagram privacy settings right now. You'd be surprised what's visible.",
                "🔒 Think twice before sharing personal stuff online. Scammers use that info to steal your identity.",
                "👁️ Posting your vacation plans or fancy purchases makes you a target for criminals.",
                "📧 Never send your ID number or bank details through regular email - it's not secure.",
                "🕵️ Don't do banking on public Wi-Fi at coffee shops or airports. Hackers can see your traffic.",
                "🔐 South Africa's POPI Act gives you rights over your personal data. Know who has your info.",
                "📱 Why does a flashlight app need access to your contacts? It probably shouldn't.",
                "🔍 Search your own name on Google sometime. You might be surprised what comes up."
            };

            _greetings = new List<string>
            {
                "👋 Hey there! Ready to learn about staying safe online? What's on your mind?",
                "🇿🇦 Howzit! I can help with phishing, passwords, privacy, and more. Ask away!",
                "🛡️ Good to see you again! What cyber safety topic shall we talk about today?",
                "💬 I'm here to help you stay safe online. Got any questions for me?"
            };

            _farewells = new List<string>
            {
                "👋 Stay safe out there! Remember - think before you click!",
                "🛡️ Keep your digital life secure! Come back anytime if you need more tips.",
                "🔒 Stay sharp, South Africa! Those scammers are clever, but now you're clever too.",
                "📱 If it sounds too good to be true, it probably is. Take care!"
            };
        }

        /// <summary>
        /// Load logo image - FIXED to work properly on start page
        /// </summary>
        private void LoadLogo()
        {
            try
            {
                string foundPath = null;

                // Check multiple possible locations
                string[] searchLocations = new string[]
                {
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources"),
                    Directory.GetCurrentDirectory()
                };

                string[] possibleFiles = { "image.jpg", "image.png", "image.jpeg" };

                foreach (string location in searchLocations)
                {
                    foreach (string file in possibleFiles)
                    {
                        string fullPath = Path.Combine(location, file);
                        if (File.Exists(fullPath))
                        {
                            foundPath = fullPath;
                            break;
                        }
                    }
                    if (foundPath != null) break;
                }

                if (foundPath != null)
                {
                    LoadImageFromPath(foundPath);
                    System.Diagnostics.Debug.WriteLine($"Logo loaded from: {foundPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No logo file found - continuing without logo");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldn't load logo: {ex.Message}");
            }
        }

        private void LoadImageFromPath(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                // Set both logo images
                LogoImage.Source = bitmap;
                ChatLogoImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading image from {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Play greeting WAV - FIXED
        /// </summary>
        private void PlayGreeting()
        {
            try
            {
                string foundPath = null;

                string[] searchLocations = new string[]
                {
                    AppDomain.CurrentDomain.BaseDirectory,
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")),
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Directory.GetCurrentDirectory()
                };

                foreach (string location in searchLocations)
                {
                    string fullPath = Path.Combine(location, "greet.wav");
                    if (File.Exists(fullPath))
                    {
                        foundPath = fullPath;
                        break;
                    }
                }

                if (foundPath != null)
                {
                    _greetingPlayer = new SoundPlayer(foundPath);
                    _greetingPlayer.Play();
                    System.Diagnostics.Debug.WriteLine($"Sound played from: {foundPath}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No greeting.wav found - continuing without sound");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Couldn't play greeting: {ex.Message}");
            }
        }

        private void LoadUserNames()
        {
            string filename = "user_names.txt";
            if (File.Exists(filename))
            {
                var lines = File.ReadAllLines(filename);
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line) && line != "auto_create")
                    {
                        _userMemory[line.Trim()] = "returning_user";
                    }
                }
            }
        }

        private void SaveUserName(string name)
        {
            string filename = "user_names.txt";
            if (!File.Exists(filename))
            {
                File.AppendAllText(filename, "auto_create\n");
            }

            if (!_userMemory.ContainsKey(name))
            {
                File.AppendAllText(filename, name + "\n");
                _userMemory[name] = "returning_user";
            }
        }

        private bool CheckNameExists(string name)
        {
            return _userMemory.ContainsKey(name);
        }

        private void AddMessageToChat(string message, bool isUser)
        {
            Dispatcher.Invoke(() =>
            {
                chats.Items.Add(new ChatMessage
                {
                    Text = message,
                    Sender = isUser ? $"👤 {_currentUserName}" : "🛡️ Cyber Assistant",
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    BackgroundColor = isUser ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196f3"))
                                             : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4a4e69")),
                    Alignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                    IsUser = isUser
                });

                if (chats.Items.Count > 0)
                {
                    var scrollViewer = FindVisualChild<ScrollViewer>(chats);
                    scrollViewer?.ScrollToBottom();
                }
            });
        }

        private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                    return (T)child;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void send(object sender, RoutedEventArgs e)
        {
            ProcessUserQuestion();
        }

        private void Question_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !string.IsNullOrWhiteSpace(question.Text))
            {
                ProcessUserQuestion();
            }
        }

        private void UserName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && !string.IsNullOrWhiteSpace(user_name.Text))
            {
                submit_name(sender, e);
            }
        }

        private void ProcessUserQuestion()
        {
            string userInput = question.Text.Trim();

            if (string.IsNullOrEmpty(userInput))
            {
                error_method();
                return;
            }

            AddMessageToChat(userInput, true);
            question.Clear();

            CheckForPersonalInfo(userInput);

            string sentiment = DetectSentiment(userInput);
            if (sentiment == "worried")
            {
                AddMessageToChat("Look, I get it. Getting scammed is scary. But don't worry - I'll share some tips to help protect you.\n\n" + GetRandomPhishingTip(), false);
                return;
            }
            else if (sentiment == "frustrated")
            {
                AddMessageToChat("I know, it's a lot to keep track of. Let me break it down simply. Here's one thing you can do today:\n\n" + GetRandomPasswordTip(), false);
                return;
            }
            else if (sentiment == "curious")
            {
                AddMessageToChat("That's a good question! Learning about this stuff is the first step to staying safe.\n\n" + GetRandomPrivacyTip(), false);
                return;
            }

            if (IsFarewell(userInput))
            {
                AddMessageToChat(GetRandomFarewell(), false);
                return;
            }

            if (IsFollowUpRequest(userInput) && !string.IsNullOrEmpty(_currentTopic))
            {
                _topicDepth++;
                AddMessageToChat(ProvideMoreInfoOnTopic(_currentTopic), false);
                return;
            }

            if (!IsFollowUpRequest(userInput))
            {
                _topicDepth = 0;
            }

            string lowerInput = userInput.ToLower();

            if (ContainsKeyword(lowerInput, new[] { "phish", "scam", "email fraud", "fake email", "smishing", "vishing" }))
            {
                _currentTopic = "phishing";
                AddMessageToChat(GetRandomPhishingTip(), false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "password", "passcode", "pin", "login" }))
            {
                _currentTopic = "password";
                AddMessageToChat(GetRandomPasswordTip(), false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "privacy", "private", "data", "personal info", "popi" }))
            {
                _currentTopic = "privacy";
                AddMessageToChat(GetRandomPrivacyTip(), false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "2fa", "two factor", "mfa", "multifactor" }))
            {
                _currentTopic = "2fa";
                AddMessageToChat("🔐 Two-factor authentication gives you an extra layer of security. Even if someone steals your password, they can't get in without the second code from your phone. Turn it on for your email, banking, and social media accounts!", false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "ransomware", "virus", "malware", "trojan" }))
            {
                _currentTopic = "malware";
                AddMessageToChat("💀 Never download attachments from people you don't know. South African hospitals and businesses have been hit hard by ransomware. Keep your antivirus updated and back up your files to an external drive or the cloud.", false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "help", "topics", "what can you do" }))
            {
                AddMessageToChat(GetHelpMessage(), false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "thanks", "thank you", "appreciate" }))
            {
                AddMessageToChat("🙏 You're welcome! Stay safe out there. Want to learn about another topic?", false);
            }
            else if (ContainsKeyword(lowerInput, new[] { "how are you", "how do you do" }))
            {
                AddMessageToChat("I'm doing well, thanks for asking! Ready to talk about online safety. What would you like to know?", false);
            }
            else
            {
                AddMessageToChat("Hmm, I'm not sure what you mean. Try asking about:\n• Phishing scams\n• Strong passwords\n• Online privacy\n• Two-factor authentication\n\nType 'help' to see all the topics I know about!", false);
            }

            question.Focus();
        }

        private bool ContainsKeyword(string message, string[] keywords)
        {
            return keywords.Any(k => message.Contains(k));
        }

        private string DetectSentiment(string message)
        {
            string lower = message.ToLower();

            if (lower.Contains("worried") || lower.Contains("scared") || lower.Contains("nervous") ||
                lower.Contains("anxious") || lower.Contains("concerned") || lower.Contains("afraid"))
            {
                return "worried";
            }
            else if (lower.Contains("frustrated") || lower.Contains("angry") || lower.Contains("tired") ||
                     lower.Contains("annoyed") || lower.Contains("fed up") || lower.Contains("confused"))
            {
                return "frustrated";
            }
            else if (lower.Contains("curious") || lower.Contains("interesting") || lower.Contains("wondering") ||
                     lower.Contains("want to learn") || lower.Contains("tell me more"))
            {
                return "curious";
            }

            return "neutral";
        }

        private bool IsFarewell(string message)
        {
            string lower = message.ToLower();
            return lower.Contains("bye") || lower.Contains("goodbye") || lower.Contains("see you") ||
                   lower.Contains("farewell") || lower.Contains("exit") || lower.Contains("quit");
        }

        private bool IsFollowUpRequest(string message)
        {
            string lower = message.ToLower();
            return lower.Contains("more") || lower.Contains("another") || lower.Contains("tell me more") ||
                   lower.Contains("explain more") || lower.Contains("continue") || lower.Contains("additional") ||
                   lower.Contains("what else") || lower.Contains("elaborate");
        }

        private string ProvideMoreInfoOnTopic(string topic)
        {
            switch (topic)
            {
                case "phishing":
                    if (_topicDepth == 1)
                        return "🎣 True story: Someone got a text 'from FNB' saying their account was locked. They called the number, shared their OTP, and lost R50,000. Always call your bank's official number!";
                    else if (_topicDepth == 2)
                        return "📧 Check the sender's email address carefully. 'support@fnb-secure.co.za' is fake, 'fnb.co.za' is real. Hover over links before clicking to see where they really go.";
                    else
                        return "🛡️ If you get a suspicious email, forward it to your bank's fraud department and to SABRIC at info@sabric.co.za";

                case "password":
                    if (_topicDepth == 1)
                        return "💡 Try using a passphrase! 'MyCatLovesEatingTuna2024!' is way easier to remember than 'P@ssw0rd' and much harder to crack.";
                    else if (_topicDepth == 2)
                        return "🔑 Password managers store all your passwords in an encrypted vault. You only need to remember one strong master password - they handle the rest!";
                    else
                        return "📊 Go to 'haveibeenpwned.com' and enter your email. It'll show you if your info has been in any data breaches. If yes, change that password right away!";

                case "privacy":
                    if (_topicDepth == 1)
                        return "📱 Look at your phone's app permissions. Does that flashlight app really need to see your contacts and location? Probably not.";
                    else if (_topicDepth == 2)
                        return "🌐 Use private browsing mode for sensitive searches. Try privacy-focused browsers like Brave or Firefox with privacy add-ons.";
                    else
                        return "🔍 Search your own name on Google. You might be surprised what comes up! You can request to have personal info removed from some data broker sites.";

                default:
                    return "Here's another tip: " + GetRandomPhishingTip();
            }
        }

        private void CheckForPersonalInfo(string message)
        {
            Regex namePattern = new Regex(@"my name is (\w+)|i'm (\w+)|i am (\w+)", RegexOptions.IgnoreCase);
            Match nameMatch = namePattern.Match(message);
            if (nameMatch.Success)
            {
                string name = nameMatch.Groups[1].Success ? nameMatch.Groups[1].Value :
                             (nameMatch.Groups[2].Success ? nameMatch.Groups[2].Value : nameMatch.Groups[3].Value);
                if (!string.IsNullOrEmpty(name) && name.Length >= 2)
                {
                    _userMemory["name"] = name;
                    AddMessageToChat($"🎉 Nice to meet you, {name}! I'll remember that. What cyber safety topic shall we talk about?", false);
                }
            }

            if (ContainsKeyword(message.ToLower(), new[] { "interested in", "want to learn", "tell me about" }))
            {
                foreach (var topic in new[] { "privacy", "phishing", "password" })
                {
                    if (message.ToLower().Contains(topic))
                    {
                        _userMemory["interest"] = topic;
                        AddMessageToChat($"📚 Cool! Since you're interested in {topic}, here's something good to know:\n\n" + ProvideMoreInfoOnTopic(topic), false);
                        break;
                    }
                }
            }
        }

        private string GetRandomPhishingTip() => _phishingResponses[_random.Next(_phishingResponses.Count)];
        private string GetRandomPasswordTip() => _passwordResponses[_random.Next(_passwordResponses.Count)];
        private string GetRandomPrivacyTip() => _privacyResponses[_random.Next(_privacyResponses.Count)];
        private string GetRandomGreeting() => _greetings[_random.Next(_greetings.Count)];
        private string GetRandomFarewell() => _farewells[_random.Next(_farewells.Count)];

        private string GetHelpMessage()
        {
            return @"📚 **Here's what I can help with:**

🔐 **PHISHING SCAMS**
• Fake bank emails
• SMS scams (Smishing)
• Phone call scams (Vishing)
• How to spot fake websites

🔑 **PASSWORD SAFETY**
• Creating strong passwords
• Password managers
• Two-factor authentication

🛡️ **PRIVACY & DATA PROTECTION**
• Social media privacy
• POPIA (SA's privacy law)
• Public Wi-Fi dangers

Just ask me something like:
• ""What's phishing?""
• ""How do I make a strong password?""
• ""What is 2FA?""
• ""How to stay safe online?""";
        }

        private void error_method()
        {
            AddMessageToChat("Please type a question - I'm here to help you learn about staying safe online.", false);
        }

        private void submit_name(object sender, RoutedEventArgs e)
        {
            string name = user_name.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                AddMessageToChat("Please tell me your name so we can chat!", false);
                return;
            }

            _currentUserName = name;
            SaveUserName(name);
            bool isReturning = CheckNameExists(name);
            _userMemory["name"] = name;

            name_grid.Visibility = Visibility.Hidden;
            chats_grid.Visibility = Visibility.Visible;
            UserNameDisplay.Text = $"Hey there, {name}!";

            if (isReturning)
            {
                AddMessageToChat($"👋 Welcome back, {name}! Great to see you again.\n\n{GetRandomGreeting()}", false);
            }
            else
            {
                AddMessageToChat($"👋 Hey {name}! Welcome to the Cyber Safety Assistant.\n\n{GetRandomGreeting()}", false);
            }

            AddMessageToChat(@"
╔══════════════════════════════════════════════════════════════╗
║                     🛡️ CYBER SAFETY 🛡️                       ║
║                                                              ║
║      🇿🇦  PROTECTING SOUTH AFRICA FROM CYBER THREATS   🇿🇦     ║
║                                                              ║
║  • Phishing Scams     • Password Safety    • Online Privacy  ║
║  • Smishing/Vishing   • Social Engineering • Data Protection ║
║                                                              ║
║  Type 'help' to see topics, or just ask me a question!       ║
╚══════════════════════════════════════════════════════════════╝", false);
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            chats.Items.Clear();
            AddMessageToChat("Chat cleared! I still remember you though. How can I help?", false);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _greetingPlayer?.Dispose();
        }
    }
}