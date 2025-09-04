using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//password: embind anecdota praiser obelize
namespace CrimsofallTechnologies.ServerSimulator
{
    public class CommandProcessor : MonoBehaviour
    {
        public Text text;
        public Text loadingHeavyText;
        public InputField field;
        public GameObject fieldGO;
        public GameObject foreInputSpace, centerInputSpace;

        public Chassis defaultChassis;
        public TimeZoneManager timeZoneManager;
        public TutorSetup tutor;
        public LogWriter logWriter;

        [Space]
        public float normalYOffset;
        public float spacedYOffset;

        [Space]
        public Text loginText;
        public ScrollRect scrollRect;
        public Scrollbar verticalScrollBar;
        public Vector2 normalizedPosition;
        public Text inputFieldSpace;
        [Range(0.1f, 3f)] public float timeMultiplier = 1f;

        [Space]
        public Color pink;
        public Color yellow;
        public Color red;
        public Color green;
        public Color blue;
        public int caretWidth = 10;

        [Space]
        public Text[] fontSizingTexts;
        public int defaultFontSize = 26;
        public int maxFontSize = 40;
        public int minFontSize = 10;

        public bool LoggedIn { get; set; }
        private bool enterEmail;
        private bool waitingForLoginSelect;
        public bool Loading { get; set; }
        private bool LoadingHeavy, waitingForResponse, waitingForTimezoneConfirm;
        private bool Rebooting = false;
        
        public Chassis chassis { get; private set; }
        public string LoggedInAs { get; set; }
        public string LoginText { get; set; }
        private string lastCommandInput = "";
        private RectTransform inputFieldRect, textRect, loadingTextRect;

        private bool CannotQuitHeavyLoadingTask = false;
        public bool Mounted = false;
        private List<string> CommandHistory = new();
        private int currentCommandHistoryIndex = 0;

        private bool WaitingForEnterToLogin = false;
        private bool ResetScroll = false;
        private int currentLine = -1, currentLine2 = -1;
        private int currentOSIndex = 0;
        private float lastTabTime = 0f;

        private string[] RandomWords = new string[]{ "unity", "hassle", "conveyorx", "develop", "fast", "slow", "blanks", "eggselent", "thronex", "gamer",
        "red", "blue", "god", "name", "wind", "far", "witcher", "ghost", "note", "give", "edge"};

        private void Start()
        {
            if(!Application.isEditor)
                timeMultiplier = 1f; //use realistic timing for builds!

            //init OS
            OS.blue = blue;
            OS.green = green;
            OS.red = red;
            OS.pink = pink;
            OS.yellow = yellow;

            field.caretWidth = caretWidth;
            OS.commandProcessor = this;
            inputFieldRect = field.GetComponent<RectTransform>();
            textRect = text.GetComponent<RectTransform>();
            loadingTextRect = loadingHeavyText.GetComponent<RectTransform>();

            loadingHeavyText.text = "";
            text.text = "";
            loginText.text = "";

            for (int i = 0; i < fontSizingTexts.Length; i++)
            {
                fontSizingTexts[i].fontSize = defaultFontSize;
            }

            chassis = defaultChassis;
            ActivateField();
        }

        private void Update()
        {
            if (ResetScroll) 
            {
                scrollRect.normalizedPosition = normalizedPosition;

                //replace input field object!
                Vector3 pos = inputFieldRect.anchoredPosition;
                if (lastCommandInput == "") //means the input field is on lower line than the current log line
                {
                    pos.y = 0f;
                    inputFieldSpace.text = "";
                }
                else
                {
                    inputFieldSpace.text = ("").PadRight(lastCommandInput.Trim().Length + 1);
                    pos.y = 25.3f;
                }
                inputFieldRect.anchoredPosition = pos;
            }

            if (!GlobalVar.commandProOpen)
                return;

            if (chassis == null)
                return;

            //Reboot purity partition selection
            if (Rebooting) 
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    Replace(currentLine, $"| *Purity {chassis.GetSecondPurityPartVersion()} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (2024031915> |");
                    Replace(currentLine2, $"|  Purity {chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (2024112620> |");
                    currentOSIndex = 0;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    Replace(currentLine, $"|  Purity {chassis.GetSecondPurityPartVersion()} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (2024031915> |");
                    Replace(currentLine2, $"| *Purity {chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (2024112620> |");
                    currentOSIndex = 1;
                }

                if (Input.GetKeyDown(KeyCode.Return)) 
                {
                    StopAllCoroutines();
                    RebootComplete();
                }

                return;
            }

            if (WaitingForEnterToLogin && Input.GetKeyDown(KeyCode.Return))
            {
                LogOut();
                WaitingForEnterToLogin = false;
                return;
            }

#if UNITY_EDITOR
            //ONLY FOR TESTING - WILL BE REMOVED LATER
            if (Application.isEditor)
            {
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    LoggedIn = true;
                    Mounted = true;
                    chassis.AddRandomUsbPort();
                    chassis.ResetOSInstalls();
                    chassis.AddRandomLaptopPort();
                    chassis.CT0FirmWareInstalled = true;

                    //ChangeTimeZone();
                    StartOSSetup(true);
                    //RebootChassis();
                    //ChangeUserPassword();
                }

                if (Input.GetKeyDown(KeyCode.F3)) 
                {
                    ManualChangeTimezone();
                }

                if(Input.GetKeyDown(KeyCode.F5))
                {
                    //quick login
                    PreLogin();
                }
            }
#endif

            //Auto complete file names in current directory
            if (Input.GetKeyDown(KeyCode.Tab)) 
            {
                if (Time.time - lastTabTime <= 0.3f) 
                {
                    AutocompleteFileNames();
                }
                lastTabTime = Time.time;
            }

            //Quit some loading tasks
            if (LoadingHeavy && Input.GetKeyDown(KeyCode.Q) && !CannotQuitHeavyLoadingTask) //quit current looping command! 
            {
                SetLoadingHeavy(false);
            }

            //Exiting purity setup
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C) && !CannotQuitHeavyLoadingTask)
            {
                if (waitingForIpSetup)
                {
                    ExitSetup();
                }

                //revert logins
                if (waitingForLoginSelect) 
                {
                    LogOut();
                    waitingForLoginSelect = false;
                }
            }

            //Command history
            if (Input.GetKeyDown(KeyCode.UpArrow) && CommandHistory.Count > 0) 
            {
                //Input last used command!
                field.text = CommandHistory[currentCommandHistoryIndex];
                field.caretPosition = CommandHistory[currentCommandHistoryIndex].Length;
                field.selectionFocusPosition = CommandHistory[currentCommandHistoryIndex].Length;
                
                currentCommandHistoryIndex--;
                if(currentCommandHistoryIndex < 0) 
                    currentCommandHistoryIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && CommandHistory.Count > 0)
            {
                //Input last used command!
                currentCommandHistoryIndex++;
                if (currentCommandHistoryIndex >= CommandHistory.Count)
                    currentCommandHistoryIndex = CommandHistory.Count - 1;

                field.text = CommandHistory[currentCommandHistoryIndex];
                field.caretPosition = CommandHistory[currentCommandHistoryIndex].Length;
                field.selectionFocusPosition = CommandHistory[currentCommandHistoryIndex].Length;
            }

            if (enterToRebootToSetupOS && Input.GetKeyDown(KeyCode.Return))
            {
                RebootChassis();
            }

            if (Loading || LoadingHeavy)
            {
                //do not let player enter anything!
                if (field.IsActive())
                    field.DeactivateInputField();

                return;
            }

            //Increase/decrease font size
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    int size = fontSizingTexts[0].fontSize;
                    size++;
                    if (size > maxFontSize) size = maxFontSize;
                    for (int i = 0; i < fontSizingTexts.Length; i++)
                    {
                        fontSizingTexts[i].fontSize = size;
                    }
                }

                if (Input.GetAxis("Mouse ScrollWheel") < 0)
                {
                    int size = fontSizingTexts[0].fontSize;
                    size--;
                    if (size < minFontSize) size = minFontSize;
                    for (int i = 0; i < fontSizingTexts.Length; i++)
                    {
                        fontSizingTexts[i].fontSize = size;
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl)) 
            {
                ResetScroll = false;
            }
        }

        private void AutocompleteFileNames() 
        {
            string[] splits = field.text.Split(' ');
            string fullName = chassis.GetClosestFileStartingWith(splits[1]);
            string txt = splits[0] + " " + fullName;
            field.text = txt;

            field.caretPosition = txt.Length;
            field.selectionFocusPosition = txt.Length;
        }

        #region INPUT_OUTPUT

        public void ReadInput(string value)
        {
            if (chassis == null)
                return;

            //if no controller is connected to make sure to avoid taking any commands!
            if (chassis.insertedLaptopPort == null) 
            {
                field.text = "";
                ActivateField();
                return;
            }

            if ((value == "\n" || value == "") && (setupPausedOnRapidDataLock || setupPausedOnDataErase))
            {              
                OS.ProcessCommand("\n");
            }

            if (ProcessCommand(value))
            {
                field.text = "";
                ActivateField();
                return;
            }

            if (string.IsNullOrEmpty(value))
            {
                if (isSettingUpOS && waitingForIpSetup)
                {
                    CommandHistory.Add(value);
                    currentCommandHistoryIndex = CommandHistory.Count - 1;

                    field.text = "";
                    SetIpConfigs(value);
                    ActivateField();
                    return;
                }

                Log(loginText.text);
                ActivateField();
                return;
            }
            else if (isSettingUpOS && waitingForIpSetup)
            {
                CommandHistory.Add(value);
                currentCommandHistoryIndex = CommandHistory.Count - 1;

                SetIpConfigs(value);
                ActivateField();
                return;
            }

            field.text = "";
            ActivateField();

            OS.ProcessCommand(value);

            if (value != "" || value != "\n") 
                CommandHistory.Add(value);
            currentCommandHistoryIndex = CommandHistory.Count - 1;

            ActivateField();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus) ActivateField();
        }

        public void ActivateField()
        {
            field.ActivateInputField();

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void DeactivateField()
        {
            field.DeactivateInputField();
        }

        /// <summary>
        /// Logs an message to command line.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <param name="add">does it add to the lines or erases all previous log and starts a new.</param>
        public void Log(string message, bool add = true, bool storeInput = false)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (!LoggedIn) 
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            if (storeInput) 
            {
                lastCommandInput = message;
            }

            if (!LoadingHeavy)
            {
                if (add)
                {
                    logWriter.AddToLog("\n"+message);
                    text.text += "\n" + message;
                }
                else
                {
                    logWriter.AddToLog(message);
                    text.text = message;
                }
            }
            else
            {
                if (add)
                {
                    logWriter.AddToLog("\n" + message);
                    loadingHeavyText.text += "\n" + message;
                }
                else
                {
                    logWriter.AddToLog(message);
                    loadingHeavyText.text = message;
                }
            }

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public int Log_Line(string message, bool padInputField) 
        {
            if (string.IsNullOrEmpty(message)) return -1;

            if (!LoggedIn)
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            //this will make the input field caret *move forward* by number of characters in the command (useful for y/n commands on same line)
            if (padInputField)
            {
                lastCommandInput = message;
            }

            if (!LoadingHeavy)
            {
                text.text += "\n" + message;
            }
            else
            {
                loadingHeavyText.text += "\n" + message;
            }
            logWriter.AddToLog("\n"+message);

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);

            if (!LoadingHeavy)
                return text.text.Split('\n').Length - 1;
            else
                return loadingHeavyText.text.Split('\n').Length - 1;
        }

        public void Log(string message, Color col)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (!LoggedIn)
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            string colorHex = "#" + ColorUtility.ToHtmlStringRGB(col);
            if (!LoadingHeavy)
            {
                text.text += $"\n<color={colorHex}>{message}</color>";
            }
            else
            {
                loadingHeavyText.text += $"\n<color={colorHex}>{message}</color>";
            }

            logWriter.AddToLog("\n" + message);
            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public int Log_Line(string message, Color col)
        {
            if (string.IsNullOrEmpty(message)) return -1;

            if (!LoggedIn)
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            string colorHex = "#" + ColorUtility.ToHtmlStringRGB(col);
            if(!LoadingHeavy)
                text.text += $"\n<color={colorHex}>{message}</color>";
            else
                loadingHeavyText.text += $"\n<color={colorHex}>{message}</color>";
            logWriter.AddToLog("\n" + message);

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
            return text.text.Split('\n').Length - 1;
        }

        public void LogWarning(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (!LoggedIn)
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            lastCommandInput = message;
            string colorHex = "#" + ColorUtility.ToHtmlStringRGB(yellow);
            if(!LoadingHeavy)
                text.text += $"\n<color={colorHex}>WARNING! {message}</color>";
            else
                loadingHeavyText.text += $"\n<color={colorHex}>WARNING! {message}</color>";

            logWriter.AddToLog($"\nWARNING! {message}");
            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void LogError(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            if (!LoggedIn)
            {
                string[] spl = message.Split('\n');
                if (spl.Length > 1) lastCommandInput = spl[spl.Length - 1];
                else lastCommandInput = message;
            }

            string colorHex = "#" + ColorUtility.ToHtmlStringRGB(red);
            if(!LoadingHeavy) text.text += $"\n  <color={colorHex}>{message}</color>";
            else loadingHeavyText.text += $"\n  <color={colorHex}>{message}</color>";
            logWriter.AddToLog($"\nERROR: {message}");

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void Replace(int line, string value) 
        {
            if (!LoadingHeavy)
            {
                string[] lines = text.text.Split('\n');
                if (line <= 0 || line >= lines.Length)
                    return;

                lines[line] = value;
                text.text = lines[0];
                for (int i = 1; i < lines.Length; i++)
                    text.text += "\n" + lines[i];
            }
            else 
            {
                string[] lines = loadingHeavyText.text.Split('\n');
                if (line <= 0 || line >= lines.Length)
                    return;

                lines[line] = value;
                loadingHeavyText.text = lines[0];
                for (int i = 1; i < lines.Length; i++)
                    loadingHeavyText.text += "\n" + lines[i];
            }

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void Replace(string textToReplace, string value)
        {
            if (string.IsNullOrEmpty(textToReplace)) return;

            if (!LoadingHeavy)
            {
                string[] lines = text.text.Split('\n');

                text.text = lines[0];
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i] == textToReplace)
                    {
                        lines[i] = value;
                    }
                    text.text += "\n" + lines[i];
                }
            }
            else 
            {
                string[] lines = loadingHeavyText.text.Split('\n');

                loadingHeavyText.text = lines[0];
                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i] == textToReplace)
                    {
                        lines[i] = value;
                    }
                    loadingHeavyText.text += "\n" + lines[i];
                }
            }

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void LogDualColumns(string[] textArray) 
        {
            int colWidth = 0;
            for (int i = 0; i < textArray.Length; i++)
            {
                if (textArray[i].Length > colWidth) 
                    colWidth = textArray[i].Length;
            }

            //also add the size of array (since this is written as numbers too)
            colWidth += 3 + textArray.Length.ToString().Length;

            string message= $"   1. " + textArray[0].PadRight(colWidth) + $"2. " + textArray[1];
            for (int i = 2; i < textArray.Length; i+=2)
            {
                message += $"\n   {i}. "+textArray[i].PadRight(colWidth) + ((i + 1 < textArray.Length) ? $"{i + 1}. " + textArray[i + 1] : "");
            }

            if (!LoadingHeavy)
            {
                text.text += $"\n{message}";
            }
            else
            {
                loadingHeavyText.text += $"\n{message}";
            }

            logWriter.AddToLog("\n" + message);
            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        #endregion

        private void CancelScrollReset()
        {
            ResetScroll = false;
        }

        private bool ProcessCommand(string value) 
        {
            if (!LoggedIn) 
            {
                // if (value == "pureeng" && chassis.IsOk()) //only works after array is working ok

                //user should only be able to login as 'pureeng' after OS is setup and installed.
                if (value == "pureeng" && LoggedInAs == "" && chassis.OSInstalled((chassis.selectedController == "CT0" ? 0 : 1))) //only check if OS is installed on current selected chassis.
                {
                    Log("1) Authenticate via CloudAssist\n2) Authenticate via command line\n3) Restore challenge\n" +
                        "Type the number corresponding to the authentication method you want to use.\n");
                    currentLine = Log_Line("Selection:", true);
                    SetLoginText("");
                    waitingForLoginSelect = true;
                    LoggedInAs = "pureeng";
                }

                //after os is installed cannot login as puresetup, but make sure once the OS is installed do not login as puresetup again.
                if (value == "puresetup") 
                {
					if(chassis.OSInstalled(chassis.selectedController == "CT0" ? 0 : 1) == false && LoggedInAs == "") {					
						currentLine = Log_Line("Password:", true);
						SetLoginText("");
						enterEmail = true;
						LoggedInAs = "puresetup";
					}
					else //if the server is already configured fail on trying to login as puresetup! 
					{
						
					}
                }

                if (waitingForLoginSelect) 
                {
                    int.TryParse(value, out int selection);
                    if (selection == 1) 
                    {
                        if (chassis.OSInstalled(chassis.selectedController == "CT0" ? 0 : 1) && LoggedInAs == "pureeng")
                        {
                            enterEmail = true;
                            Log("An email for identity verification will be sent once email is entered.\nPlease follow the authentication instructions provided there and return to this screen when finished of if email has not been received.\n\n");
                            currentLine = Log_Line("Enter an email address:", true);
                            waitingForLoginSelect = false;
                        }
                        else 
                        {
                            SetLoadingHeavy(true);
                            loadingHeavyText.text = text.text;
                            text.text = "";
                            CannotQuitHeavyLoadingTask = true;
                            Invoke(nameof(RevertToLoginOptions), 30f * timeMultiplier);
                        }
                    }
                    if (selection == 2)
                    {
                        throw new System.NotImplementedException();
                    }
                    if (selection == 3)
                    {
                        Log($"kid:1 {RandomWords[Random.Range(0, RandomWords.Length)]} {RandomWords[Random.Range(0, RandomWords.Length)]} {RandomWords[Random.Range(0, RandomWords.Length)]} {RandomWords[Random.Range(0, RandomWords.Length)]} {RandomWords[Random.Range(0, RandomWords.Length)]}");
                        waitingForLoginSelect = false;
                        waitingForResponse = true;
                    }
                    return true;
                }

                if (waitingForResponse && value.Length >= 150) //allow some characters to be skipped. 
                {
                    PreLogin();
                    waitingForResponse = false;
                    return true;
                }

                if (enterEmail)
                {
                    if (LoggedInAs == "puresetup") //logged in as puresetup? require password and no cloud logins! 
                    {
                        if (value == "embind anecdota praiser obelize")
                        {
                            Replace(currentLine, "Password: " + value);
                            PreLogin();
                        }
                        return true;
                    }

                    //make sure email is correct - a simple test!
                    if (!value.Contains("@") || !value.EndsWith("purestorage.com"))
                    {
                        string colorHex = "#" + ColorUtility.ToHtmlStringRGB(red);
                        Replace(currentLine, $"\n<color={colorHex}>Please enter a valid email address</color>");
                        currentLine = Log_Line("Enter an email address:", false);
                    }
                    else
                    {
                        Replace(currentLine, $"Enter an email address: {value}");
                        UIManager.Instance.ShowPopup("Email Conformation", "Check your mail and follow the link to Authenticate the session.");
                        PreLogin();
                    }
                }

                return true;
            }

            if (waitingForTimezoneConfirm)
            {
                if (value == "")
                {
                    //default selection (N)
                    Replace(currentLine, $"Confirm time zone change from {chassis.GetCurrentController().TimeZone} to {chassis.GetCurrentController().tempTimeZone} (y/N): n");
                }
                else
                {
                    Replace(currentLine, $"Confirm time zone change from {chassis.GetCurrentController().TimeZone} to {chassis.GetCurrentController().tempTimeZone} (y/N): y");
                    chassis.GetCurrentController().TimeZone = chassis.GetCurrentController().tempTimeZone;
                }

                SetLoginText(lastLoginText);
                lastLoginText = "";
                lastCommandInput = "";
                waitingForTimezoneConfirm = false;
                chassis.GetCurrentController().tempTimeZone = "";
                return true;
            }

            if (waitingForTimeSelection)
            {
                int.TryParse(value, out int i);

                if(i >= 0)
                    SelectTimeZoneNumber(i);
                return true;
            }

            if (inputOldPass || inputNewPass || reenterNewPass)
            {
                InputPassword(value);
                return true;
            }

            return false;
        }

        private void RevertToLoginOptions() 
        {
            SetLoadingHeavy(false);
            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";
            CannotQuitHeavyLoadingTask = false;
        }

        //connecting to another computer!
        public void ChangePuttyChassis(Chassis selectedChassis) 
        {
            //make sure the connecting chassis is on and ready!
            if (!selectedChassis.IsOn()) 
            {
                return;
            }

            Debug.Log("Connecting PuTTY to: " + selectedChassis.GetComputerName() + ":" + selectedChassis.selectedController);

            //disconnect connection cables from previous chassis
            if (chassis!=null && selectedChassis != chassis) 
            {
                chassis.rack.DisableWire("LAPTOP", 0, chassis.chassisIndex);
                chassis.rack.DisableWire("LAPTOP", 1, chassis.chassisIndex);
            }

            //clear the log and make computer login again!
            chassis = selectedChassis;
            loadingHeavyText.text = "";
            text.text = "";
            field.text = "";
            lastCommandInput = "";
            SetLoadingHeavy(false);
            WaitingForEnterToLogin = true;
            foreInputSpace.SetActive(false);
            centerInputSpace.SetActive(false);
            ActivateField();

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        public void SwitchChassis() 
        {
            //if (chassis.selectedController == "CT0" && chassis.OSInstalled(1) && chassis.CT1FirmWareInstalled)
            if(chassis.selectedController == "CT0")
            {
                chassis.selectedController = "CT1";
                
                Debug.Log("Connecting PuTTY to: " + chassis.GetComputerName() + ":" + chassis.selectedController);
                SetLoginText($"{LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:~$");
                return;
            }

            //if (chassis.selectedController == "CT1" && chassis.OSInstalled(0) && chassis.CT0FirmWareInstalled)
            if (chassis.selectedController == "CT1")
            {
                chassis.selectedController = "CT0";

                Debug.Log("Connecting PuTTY to: " + chassis.GetComputerName() + ":" + chassis.selectedController);
                SetLoginText($"{LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:~$");
            }
        }

        public void SetLoadingHeavy(bool value) 
        {
            if (value) 
            {
                loadingHeavyText.gameObject.SetActive(true);
                text.gameObject.SetActive(false);
                loadingHeavyText.fontSize = text.fontSize;

                LoadingHeavy = true;
                field.ActivateInputField();
            }
            else 
            {
                StopAllCoroutines();
                loadingHeavyText.gameObject.SetActive(false);
                text.gameObject.SetActive(true);
                text.fontSize = loadingHeavyText.fontSize;

                LoadingHeavy = false;
            }

            field.text = "";
            fieldGO.SetActive(!value);
        }

        public void SetLoginText(string text)
        {
            LoginText = text;
            loginText.text = text;

            centerInputSpace.SetActive(text != "");
            foreInputSpace.SetActive(text.StartsWith("root"));
        }

        public void Logout_Invoke() 
        {
            LogOut();
        }

        #region LOGIN_LOGOUT

        //take back to login to current computer page!
        public void LogOut(bool waitForEnter = false)
        {
            LoggedIn = false;
            SetLoginText("");
            LoggedInAs = "";
            field.text = "";
            loginText.text = "";
            field.ActivateInputField();
            foreInputSpace.SetActive(false);
            centerInputSpace.SetActive(false);

            if (!waitForEnter)
            {
                text.text = $"{chassis.GetComputerName()}-{chassis.selectedController} login:";
                lastCommandInput = text.text;
                logWriter.AddToLog("\n" + text.text);
            }
            else 
            {
                text.text = "";
                lastCommandInput = "";
                WaitingForEnterToLogin = true;
            }

            //cancel any tasks running when logged out!
            CannotQuitHeavyLoadingTask = false;
            SetLoadingHeavy(false);

            CancelInvoke(nameof(CancelScrollReset));
            ResetScroll = true;
            Invoke(nameof(CancelScrollReset), 0.5f);

            setupPausedOnDataErase = false;
            isSettingUpOS = false;
        }

        public void PreLogin() 
        {
            if(chassis == null) return;

            SetLoginText("");
            Loading = true;
            Invoke(nameof(Login), 2f);
        }

        private void Login()
        {
            enterEmail = false;
            if (LoggedInAs == "puresetup" && !chassis.FirmwareInstalled())
            {
                ShowFirmwareInstallUpgradeNotice();
            }

            if (chassis.selectedController == "CT0" && chassis.ShowUpdateCompCT0) 
            {
                ShowUpdateCompleteNotice();
            }

            if (chassis.selectedController == "CT1" && chassis.ShowUpdateCompCT1)
            {
                ShowUpdateCompleteNotice();
            }

            Log($"Successfully authenticated\nLast login: {PlayerPrefs.GetString("LastLoginTimestamp" + LoggedInAs, "no previous logins")}");
            lastCommandInput = "";
            PlayerPrefs.SetString("LastLoginTimestamp" + LoggedInAs, timeZoneManager.GetTimeNow());

            Invoke(nameof(CompletelyLoggedIn), 1.5f);
        }

        private void CompletelyLoggedIn() 
        {
            if (!chassis.FirmwareInstalled()) 
                ShowFirmwareInstallUpgradeNotice();

            lastCommandInput = "";
            
            LoggedIn = true;
            Log("\n"+ timeZoneManager.GetTimeNow());
            Log($"Welcome to {LoggedInAs}. This is Purity Version {chassis.GetCurrentPurityVersion()} on FlashArray {chassis.GetComputerName()}\nhttps://www/purestorage.com/");
            SetLoginText($"{LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:~$");
            Loading = false;
            field.ActivateInputField();

            ResetScroll = true;
            CancelInvoke(nameof(CancelScrollReset));
            Invoke(nameof(CancelScrollReset), 0.5f);
        }

        #endregion

        #region CONTINUOUS_WATCH_TASKS

        public void WorkToFindServerCableErrors()
        {
            Invoke(nameof(Delayed_WorkToFindServerCableErrors), 3f);
        }

        private void Delayed_WorkToFindServerCableErrors() 
        {
            Log("Slot listing:\nCT        Slot#        Type        Vendor        Model        FW Rev        Tracer        Assembly" +
                "\nct0        15        SAS        LSI Logic        SAS_MGA-241        14.00.03.00        PPPCFJ221910        PSO400051003" +
                "\nct0        201        SAS        LSI Logic        SAS_MGA-241        14.00.03.00        SP92917117        03-25704-02005" +
                "\nct0        eth6        ETH        Mellanox        MT27710        10.20.1002        (PST0020110036)        " +
                "\nct0        eth7        ETH        Mellanox        SAS_MGA-241        14.00.03.00        (PST0020110036)        " +
                "\nct0        eth8        ETH        Mellanox        SAS_MGA-241        14.00.03.00        (PST0020110036)        " +
                "\nct0        eth9        ETH        Mellanox        SAS_MGA-241        14.00.03.00        (PST0020110036)        ");
            Log("\nSlot summary information:\n" +
                  "--------------------------");
            Log("\nct0.eth7, (eth7 )] *\n\nct0.eth9, (eth9 )] *\n\nct0.SAS6, ( 201 0)] *\n\nct0.SAS4, ( 201 2)] *" +
                "\n\nct0.SAS5, ( 201 3)] *\n\nct0.SAS7, ( 201 1)] *\n\nct0.eth6, (eth6 )] *\n\nct0.eth8, (eth8 )] *");
            Log("\nConfiguration status\n" +
                  $"---------------------\nConfiguration check started for FA-X70R3\nSUCCESS: No errors detected.\nEnd time: {timeZoneManager.GetTimeNow()}");
        }

        public void WatchArray(string cmd)
        {
            //Log_Heavy($"Every 2.0s: {cmd}        lynpure01-ct0: " + System.DateTime.Now);
            
            SetLoadingHeavy(true);
            StartCoroutine(Delayed_WatchArray(cmd));
            Loading = true;
        }

        private IEnumerator Delayed_WatchArray(string cmd) 
        {
            while (true) 
            {
                Log($"Every 2.0s: {cmd}        lynpure01-ct0: {timeZoneManager.GetTimeNow()}\n\n\n" +
                $"Name      Capacity   Parity   Provisioned Size   Virtual  Thin Provisioning  Data Reduction Total Reduction  Unique  Snapshots  Shared  System  Replication  Total" +
                $"\nlynpure01  {chassis.GetTotalSize()}  100%  169037G   260.29T    84%          3.8 to 1     24.2 to 1        55.227      1.0T     0.00        0.00    -", false);
                yield return new WaitForSeconds(2f);
            }
        }

        public void WatchPuredriveList(string cmd)
        {
            SetLoadingHeavy(true);
            StartCoroutine(Delayed_WatchPuredriveList(cmd));
            //Log_Heavy($"Every 2.0s: {cmd}\n{System.DateTime.Now.ToString()}\n                            Capacity    Details");
            //InvokeRepeating(nameof(Delayed_WatchPuredriveList), 2f, 2f);
        }

        private IEnumerator Delayed_WatchPuredriveList(string cmd)
        {
            while (true) 
            {
                //watch as hard-drives are inserted, loaded, firmware installed and made ready!
                Log($"Every 2.0s: {cmd}\n{timeZoneManager.GetTimeNow()}\n                            Capacity    Details" +
                    $"\n{chassis.GetHardDrivesInstallStatus()}", false);
                yield return new WaitForSeconds(2f);
            }
        }

        #endregion

        #region INSTALLING_SERVER_OS

        //private bool FirmwareInstallationCompleteOnReboot = false;

        private void ShowFirmwareInstallUpgradeNotice() 
        {
            Log("===================================================================");
            Log("=                            NOTICE                               =");
            Log("===================================================================");
            Log("=                                                                 =");
            Log("=    Firmware is currently being checked and/or updated           =");
            Log("=    on this controller.                                          =");
            Log("=                                                                 =");
            Log("=    If updates are necessary they will be applied automatically, =");
            Log("=    and one or more automatic reboots may occur as a result.     =");
            Log("=    A completion notice will be provided when done.              =");
            Log("=                                                                 =");
            Log("=    DO NOT RESTART OR REMOVE POWER WHILE UPDATE IS IN PROGRESS   =");
            Log("=                                                                 =");
            Log("===================================================================");
        }

        private void ShowUpdateCompleteNotice() 
        {
            //Log($"\nBroadcast message from root@PCTFJ2448016D (somewhere) ({timeZoneManager.GetTimeNow()})");
            Log($"\nBroadcast message from root@{chassis.GetComputerName()} (somewhere) ({timeZoneManager.GetTimeNow()})");
            Log("\n===================================================================" +
                "\n=                   UPDATE AND/OR CHECK COMPLETE                  =" +
                "\n=                                                                 =" +
                "\n=    The firmware checks/updates on this system are now complete. = " +
                "\n=    No further action is required, and the controller will       =" +
                "\n=    continue running as usual                                    =" +
                "\n=                                                                 =" +
                "\n===================================================================");
        }

        public void CopyMountFiles(float copyTime, string[] FilesCopied) 
        {
            CannotQuitHeavyLoadingTask = true;
            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            text.text = "";
            chassis.CopyFilesToArray(FilesCopied);

            Invoke(nameof(CopiedFiles), copyTime);
        }

        private void CopiedFiles() 
        {
            SetLoadingHeavy(false);
            CannotQuitHeavyLoadingTask = false;
            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";

            field.ActivateInputField();
        }

        public void StartInstallation() 
        {
            CannotQuitHeavyLoadingTask = true;
            SetLoadingHeavy(true);
            StartCoroutine(AddDots());
        }

        private IEnumerator AddDots() 
        {
            string dots = ".";
            while (dots.Length < 100) 
            {
                yield return new WaitForSeconds(0.15f * timeMultiplier);
                loadingHeavyText.text = text.text + $"\nInstalling Purity on alternate partition labeled second." +
                $"\nErasing Purity software image from alternate partition second to prepare for installation." +
                $"\nWARNING: Do not interrupt this process!!\nUnpacking new Purity software.\n{dots}";
                dots += ".";
            }

            yield return new WaitForSeconds(1.5f * timeMultiplier);
            VerifyPackage();
        }

        private void VerifyPackage() 
        {
            //ShowFirmwareInstallUpgradeNotice();
            if (chassis.selectedController == "CT0" && chassis.ShowUpdateCompCT0)
                ShowUpdateCompleteNotice();

            if (chassis.selectedController == "CT1" && chassis.ShowUpdateCompCT1)
                ShowUpdateCompleteNotice();

            Log("Verifying Package...");
            StartCoroutine(FinalizeInstallation());
        }

        private IEnumerator FinalizeInstallation() 
        {
            yield return new WaitForSeconds(8f * timeMultiplier);
            Log("Finalizing installation. This may take several minutes.");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Executing /altroot/opt/purextras/bin/finish-install.sh");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Executing /altroot/opt/purextras/bin/finish-install.azure-propagate.sh");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Executing /altroot/opt/purextras/bin/finish-install.cbs.sh");
            yield return new WaitForSeconds(1.5f * timeMultiplier);
            Log("Executing /altroot/opt/purextras/bin/finish-install.cdu-pureinstall-lib.sh");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Executing /altroot/opt/purextras/bin/finish-install.fix-dpkg.sh");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Purity installed.\nInstallation complete. The new Purity version will load at next reboot.");

            //new version of purity is changed here
            if(chassis.selectedController == "CT0")
                chassis.purityOSVersion = chassis.InsertedUsbPort.purityVersionOnDrive;
            if (chassis.selectedController == "CT1")
                chassis.purityOSVersion1 = chassis.InsertedUsbPort.purityVersionOnDrive;

            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("\n\nImportant!\nThe first boot of a new Purity version may take longer if the new version includes controller firmware updates.\nDO NOT REBOOT THE CONTROLLER DURING THE FIRMWARE UPDATE." +
                "\n\nRefer to http://community.purestorage.com for more information about the Purity upgrade process and firmware updates.\n");

            //get the exact log commands to console text to make it real!
            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";
            CannotQuitHeavyLoadingTask = false;
            //FirmwareInstallationCompleteOnReboot = true; //firmware will get completed after rebooting!
            SetLoadingHeavy(false);
        }

        public void RebootChassis()
        {
            Debug.Log("Rebooting CHASSIS" + chassis.chassisIndex + " at " + chassis.selectedController);
            SetLoadingHeavy(false);
            CannotQuitHeavyLoadingTask = false;
            text.text = "";
            loadingHeavyText.text = "";

            //lights
            for (int i = 0; i < chassis.flashArrays.Length; i++)
            {
                chassis.flashArrays[i].Status = "offline";
                chassis.flashArrays[i].SetLights(false, true);
            }

            StartCoroutine(IReboot());
        }

        private IEnumerator IReboot()
        {
            CannotQuitHeavyLoadingTask = true;
            SetLoadingHeavy(true);
            loadingHeavyText.text = $"\n                            VERSION {Application.version}                            \n"+
                                    @"/------------------------------------------------------------------------------\";
            currentLine =  Log_Line($"|  Purity {chassis.GetSecondPurityPartVersion()} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (2024031915> |",false);
            currentLine2 = Log_Line($"| *Purity {chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (2024112620> |",false);
                                    

            Log("|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |" +
              "\n|                                                                              |\n" +
              @"\------------------------------------------------------------------------------/");
            text.text = "";
            yield return new WaitForSeconds(1f);
            Log("      Use the ^ and v keys to select which entry is highlighted.\n      Press enter to boot the selected OS, `e' to edit the commands before booting or `c' for a command-line.");
            Rebooting = true;
            yield return new WaitForSeconds(4f);
            //currentLine = Log_Line($"Purity {chassis.PurityVersionInSecPartition} (202404130351+34e2b1e66ad3) with kernel 5.15.123+ (2024031915>");
            //currentLine2 = Log_Line($"*Purity {chassis.GetCurrentPurityVersion()} (202412120507+7a7df3f70616) with kernel 5.15.123+ (2024112620>");
            currentOSIndex = 1;

            yield return new WaitForSeconds(1f);
            Log("The highlighted entry will be executed automatically in 3s.");
            yield return new WaitForSeconds(1f);
            Log("The highlighted entry will be executed automatically in 2s.");
            yield return new WaitForSeconds(1f);
            Log("The highlighted entry will be executed automatically in 1s.");
            yield return new WaitForSeconds(1f);
            Log("The highlighted entry will be executed automatically in 0s.");
            yield return new WaitForSeconds(1f);
            RebootComplete();
        }

        private void RebootComplete() 
        {
            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";

            SetLoadingHeavy(false);
            CannotQuitHeavyLoadingTask = false;

            if (enterToRebootToSetupOS)
            {
                chassis.OSInstallationComplete(chassis.InsertedUsbPort.purityVersionOnDrive);
                enterToRebootToSetupOS = false;
                isSettingUpOS = false; //OS is setup fully now!
                Mounted = false;
            }

            //lights
            if (chassis.IsOk()) 
            {
                for (int i = 0; i < chassis.flashArrays.Length; i++)
                {
                    chassis.flashArrays[i].Status = "ready";
                    chassis.flashArrays[i].SetLights(true, true);
                }
            }

            if (currentOSIndex == 0) //select OS version to start with!
            {
                string os = chassis.selectedController == "CT0" ? chassis.purityOSVersion : chassis.purityOSVersion1;

                chassis.purityOSVersion = chassis.GetSecondPurityPartVersion();
                //chassis.PurityVersionInSecPartition = os;
                chassis.PurityVersionInPartition0 = os;
            }

            /*if (FirmwareInstallationCompleteOnReboot)
            {
                if (!chassis.OSInstalled())
                {
                    Debug.Log("Installing Firmware in the Background...");
                    Invoke(nameof(InstalledFirmware), 15f);
                }
            }*/

            //Log out of current session as the computer has restarted!
            LogOut();
            Rebooting = false;
        }

        public void InstalledFirmware() 
        {
            //show a notice that the purity OS has been installed after reboot!
            if (LoggedIn)
            {
                ShowUpdateCompleteNotice();
            }
            else
            {
                chassis.ShowUpdateCompCT0 = true;
                chassis.ShowUpdateCompCT1 = true;
            }

            //FirmwareInstallationCompleteOnReboot = false;
            //if (chassis.selectedController == "CT0")
                chassis.CT0FirmWareInstalled = true;

            //if (chassis.selectedController == "CT1")
                chassis.CT1FirmWareInstalled = true;

            Debug.Log("Firmware installed on " + chassis.selectedController);
        }

        #endregion

        #region OS_SETUP

        public bool setupPausedOnDataErase { get; set; }
        public bool isSettingUpOS { get; set; }
        public bool setupPausedOnRapidDataLock { get; set; }
        public bool waitingForTimezone { get; set; }
        public bool waitingForIpSetup { get; set; }
        public bool enterToRebootToSetupOS { get; set; }
        public bool applyConfigToArray { get; set; }

        private bool timezoneManualChange;
        private string lastLoginText;

        private string arrayName, physicalIp, virtualIp, Netmask, Gateway, DNSServer, DNSDomain, NTPServers, SMTPRelayHost, SMTPSenderDomain, AlertSenderRecipients;

        [HideInInspector]
        public bool _arrayName, _physicalIp, _virtualIp, _Netmask, _Gateway, _DNSServer, _DNSDomain, _NTPServers, _SMTPRelayHost, _SMTPSenderDomain, _AlertSenderRecipients;

        public bool waitingForTimeSelection, geoRegionSelect, wantsToInputPassword, inputOldPass, inputNewPass, reenterNewPass;
        private string geoArea;

        public void StartOSSetup(bool installShippedOS = false) 
        {
            //make sure the firmware is installed before setup!
            if (chassis.selectedController == "CT0" && !chassis.CT0FirmWareInstalled)
            {
                LogError("OS Setup failure!");
                return;
            }
            if (chassis.selectedController == "CT1" && !chassis.CT1FirmWareInstalled)
            {
                LogError("OS Setup failure!");
                return;
            }

            if (!Mounted && !installShippedOS)
            {
                LogError("OS Setup failure!");
                return;
            }

            SetLoginText("");
            foreInputSpace.SetActive(false);
            centerInputSpace.SetActive(false);

            isSettingUpOS = true;
            SetLoadingHeavy(true);
            CannotQuitHeavyLoadingTask = false;
            loadingHeavyText.text = text.text + "\n\n##########################################\n" +
                "#   Welcome to the Purity Setup Wizard   #\n##########################################\nCONFIG_TASKS:CONFIG_TASK_START_PURITY,CONFIG_TASK_CONFIG_FLASHARRAY,CONFIG_TASK_TEST_CONNECTIVITY,CONFIG_TASK_ALL_COMPLETE,CONFIG_TASK_PURESETUP_EXITED\n" +
                "Controller hardware model is compatible with the controller purity version\n" +
                "0 shelves connected to this controller.\n" +
                "Verifying controller firmware is up to date. Controller may reboot if firmware is updated.";
            StartCoroutine(SetupOS());
        }

        private IEnumerator SetupOS() 
        {
            yield return new WaitForSeconds(2f * timeMultiplier);
            //ShowUpdateCompleteNotice();
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("\nStarting Boot-Time Components Update.");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Skipping BOOT_DRIVE_M5400 update since current image: (D4MU002) matches latest image: (D4MU002)");
            Log("Skipping BIOS update since current image: (L11P1N15) matches latest image: (L11P1N15)");
            Log("Skipping BIOS_CFG update since current image: (363d69) matches latest image: (363d69)");
            Log("Skipping BIOS_STBY-0 update since current image: (L11P1N14) to latest image: (L11P1N15) is not supported.");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Skipping BMC update since current image: (v0.0e-0aad-r0010) matches latest image: (v0.0e-0aad-r0010)");
            Log("Skipping BMC_STBY-0 update since current image: (v0.09-0aad-r0010) to latest image: (v0.0e-0aad-r0010) is not supported.");
            Log("Skipping CPLD update since current image: (R126) matches latest image: (R126)");
            Log("Skipping ADD_IN_ETH_MLX_CX6_25G_AS-1 update since current image: (26.35.2000) matches latest image: (26.35.2000)");
            Log("Skipping PFX_68A0_IMG update since current image: (0x0390b762) matches latest image: (0x0390b762)");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Skipping PFX_68A0_CFG update since current image: (0x3249620c) matches latest image: (0x3249620c)");
            Log("Skipping PFX_68B0_IMG update since current image: (0x0390b762) matches latest image: (0x0390b762)");
            Log("Skipping PFX_68B0_CFG update since current image: (0x3249620c) matches latest image: (0x3249620c)");
            Log("Skipping LOM_ETH_MLX_CX6_100G update since current image: (22.37.1014) matches latest image: (22.37.1014)");
            Log("Skipping LOM_ETH_MLX_CX6_25G update since current image: (26.37.1014) matches latest image: (26.37.1014)");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Skipping ADD_IN_FC_ELX_G7PLUS-3 update since current image: (14.0.663.7) matches latest image: (14.0.663.7)");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("All Boot-Time Components are up-to-date.");

            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Controller firmware is up to date.");
            Log("Verifying power supply firmware is up to date. If power supply firmware updates are needed, the update process can take a while.");

            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Update PSU Devices\n Policy: Update to latest but only if current version is below the specified minimum version allowed for the field, and don't downgrade." +
                "\nSkipping PSU_DELTA_TI_1600-0 update since current image: (00.36.37) to latest image: (00.36.30) is a downgrade" +
                "\nSkipping PSU_DELTA_TI_1600-1 update since current image: (00.36.37) to latest image: (00.36.30) is a downgrade\n" +
                "\n Update PSU Complete");

            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Power supply firmware is up to date.");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Verifying WSSD firmware is up to date.");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Checking for Purity activity on 14 wssd devices before attempting updates\nBeginning parallel fw version check/update for 14 WSSD Devices" +
                "\n Policy: Update to latest but only if current version is different than latest, and don't downgrade.");

            //fail here if there is less than 10 drives inserted:
            if(chassis.numInsertedDrives < 10)
            {
                SetLoadingHeavy(false);
                text.text = loadingHeavyText.text;
                CannotQuitHeavyLoadingTask = false;
                loadingHeavyText.text = "";
                ActivateField();
                SetLoginText($"{LoggedInAs}@{chassis.GetComputerName()}-{chassis.selectedController}:~$");

                Log("Purity Setup Failure! reason: CANNOT_CREATE_PARTITION");
                Log("Rolling back changes...");

                yield return null;
            }

            //Firmware updating!
            int i = 0;
            while (i < chassis.HardDrives.Length) 
            {
                yield return new WaitForSeconds(0.5f * timeMultiplier);
                if (chassis.HardDrives[i].status != HardDriveStatus.not_inserted)
                {
                    Log($" Successful parallel update of WSSD /dev/nvme{chassis.HardDrives[i].myIndex}n1 From:4.1.20 To:4.1.29");
                    chassis.HardDrives[i].InstalledFirmware();
                }
                i++;
            }

            /*yield return new WaitForSeconds(2f * timeMultiplier);
            Log(" Successful parallel update of WSSD /dev/nvme10n1 From:4.1.20 To:4.1.29\n" +
                " Successful parallel update of WSSD /dev/nvme10n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme11n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme12n1 From:4.1.20 To:4.1.29\n" +
                " Successful parallel update of WSSD /dev/nvme15n1 From:4.1.20 To:4.1.29");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log(" Successful parallel update of WSSD /dev/nvme14n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme3n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme1n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme5n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme2n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme4n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme6n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme7n1 From:4.1.20 To:4.1.29" +
                "\n Successful parallel update of WSSD /dev/nvme8n1 From:4.1.20 To:4.1.29");*/
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log($"Updated {chassis.GetInsertedDrivesCount()} WSSD Devices\n Performing additional version check/update for {chassis.HardDrives.Length} WSSD Devices");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Done! Completed additional check/update, which updated 0 WSSD devices\nWSSD firmware is up to date.\n" +
                "Controller type supports multiple storage types, detecting type of storage installed...");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Detected storage type X\nPre-existing storage type X was found on the device\nSuccessfully configured storage type X for array controller" +
                "\nPurity Apartment not found.\nInitial system setup is required which will remove everything from this array." +
                "\nIf you believe the array may contain data, exit the puresetup procedure and contact Pure Storage Support to determine the appropriate action.");

            yield return new WaitForSeconds(1f);
            currentLine = Log_Line("(continue/EXIT):", true);
            SetLoginText("");
            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";
            SetLoadingHeavy(false);
            CannotQuitHeavyLoadingTask = false;
            setupPausedOnDataErase = true;
            Debug.Log("Waiting for user input to continue...");
            ActivateField();
        }

        public void ContinueSetup() 
        {
            setupPausedOnDataErase = false;
            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            CannotQuitHeavyLoadingTask = true;
            Replace(currentLine, "(continue/EXIT): continue");
            
            Log("wait-for-state pureapp stop/waiting\n" +
                "wait-for-state mariadb stop/waiting\n" +
                "\nlio-drv disabling\nwait-for-state foed stop/waiting\nwait-for-state lio-drv stop/waiting\nwait-for-state purity_started stop/waiting\nPure Storage is offline.");

            //skip rapid data locking on secondary array controller!
            if (chassis.selectedController == "CT0")
            {
                SetLoginText("");
                currentLine = Log_Line("Configure Rapid Data Locking (y/N):", true);
                ActivateField();
                text.text = loadingHeavyText.text;
                loadingHeavyText.text = "";
                SetLoadingHeavy(false);
                CannotQuitHeavyLoadingTask = false;
                setupPausedOnRapidDataLock = true;
            }
            else 
            {
                //skip.
                StartCoroutine(ContinueSetup_2());
            }
        }

        public void ContinueSetup2(string response) 
        {
            SetLoadingHeavy(true);
            setupPausedOnRapidDataLock = false;
            CannotQuitHeavyLoadingTask = true;
            loadingHeavyText.text = text.text;
            Replace(currentLine, $"Configure Rapid Data Locking (y/N): {response}");
            StartCoroutine(ContinueSetup_2());
        }

        private IEnumerator ContinueSetup_2() 
        {
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Verifying interposer firmware is up to date.");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Beginning recycle of 0 bays...\nRecycle complete.\nInterposer firmware is up to date.");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Check io card pci id's........ OK\npurity.service stablized: done\npurity start/running");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("platform: ..done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("foed: .done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("gui: ....done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("rest: done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("platform_env: 0.done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("quorum: .....done");

            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("foed_env: 673.673.673.406.0.done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("foed_ready: done");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("remote_patch: done\ndriver: done\nsan: ............done\nhealth: done\nlio-drv: done");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("middleware-db: done\nmiddleware: ....");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log($"Broadcast message from root@PCTFJ2448016D (somewhere) ({timeZoneManager.GetTimeNow()})");

            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Purity//FA Information System Status\n====================================\nPurity//FA has successfully started for the first time after an install or upgrade.\n" +
                $"Purity//FA {chassis.InsertedUsbPort.purityVersionOnDrive} (202412120507+7a7df3f70616-65x) is now set to be the default.");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("done\nvasa: done\nPure Storage is online.");

            StartCoroutine(SetupEmailTasks());
        }

        private IEnumerator SetupEmailTasks() 
        {
            //Log("CONFIG_TASK_ARRAY_ID: 210659-179933279-2246381107586208163\nCONFIG_TASK_COMPLETED:CONFIG_TASK_START_PURITY\n" +
            Log($"CONFIG_TASK_ARRAY_ID: {Random.Range(0, 999999).ToString("000000")}-{Random.Range(0, 999999999).ToString("000000000")}-{Random.Range(0, 999999999).ToString("000000000")}{Random.Range(0, 9999999999).ToString("0000000000")}\nCONFIG_TASK_COMPLETED:CONFIG_TASK_START_PURITY\n" +
               $"Name       Created                  Expires\npuresetup  {timeZoneManager.GetTimeNow()} UTC  {System.DateTime.UtcNow.AddHours(3.0).ToString()} UTC");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Start initial configuration for management and support connectivity.");
            Log("No changes are made until entire configuration is confirmed.");
            Log("Enter CTRL-C at any time to quit the puresetup procedure.");
            Log($"Array Name:{chassis.GetComputerName()}");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Note: All IP addresses are entered as IPv4 address or IPv6 address, and netmask takes the format xxx.xxx.xxx.xxx for IPv4 or [0-128] for IPv6.");
            Log($"The following management port configuration will be applied to {chassis.selectedController.ToLower()}.eth0");

            if (chassis.selectedController == "CT1" && chassis.ipConfigsDoneOnCT0)
            {
                currentLine = Log_Line($"Physical IP Address:", true);
                _physicalIp = true;
            }
            else 
            {
                _arrayName = true;
                currentLine = Log_Line("Array Name:", true);
            }

            waitingForIpSetup = true;

            SetLoadingHeavy(false);
            text.text = loadingHeavyText.text;
            CannotQuitHeavyLoadingTask = false;
            loadingHeavyText.text = "";
            ActivateField();
            SetLoginText("");
        }

        //setting up individual stuffs
        public void SetArrayName(string value) 
        {
            arrayName = value;
            _arrayName = false;
            _physicalIp = true;
            Replace(currentLine, $"Array Name: {value}");
            currentLine = Log_Line($"Physical IP Address:", true);
        }
        public void SetPhysicalIP(string value) {
            if (value.Split('.').Length < 4)
                return;
            physicalIp = value;
            _physicalIp = false;
            Replace(currentLine, $"Physical IP Address: {value}");

            if (chassis.selectedController == "CT0" && !chassis.ipConfigsDoneOnCT0)
            {
                _virtualIp = true;
                currentLine = Log_Line($"Virtual IP Address:", true);
            }
            else //set netmask if already set some things on CT0
            {
                _Netmask = true;
                currentLine = Log_Line($"Netmask:", true);
            }
        }
        public void SetVirtualIP(string value) 
        {
            if (value.Split('.').Length < 4)
                return;

            //make sure the first 3 octets match the IP
            string[] octets = physicalIp.Split(".");
            string[] octetsV = value.Split(".");
            if (octetsV[0] != octets[0] || octetsV[1] != octets[1] || octetsV[2] != octets[2]) 
            {
                return;
            }

            virtualIp = value;
            _virtualIp = false;
            _Netmask = true;
            Replace(currentLine, $"Virtual IP Address: {value}");
            currentLine = Log_Line($"Netmask:", true);
        }
        public void SetNetMask(string value)
        {
            if (value.Split('.').Length < 4)
                return;
            Netmask = value;
            _Netmask = false;
            _Gateway = true;
            Replace(currentLine, $"Netmask: {value}");
            currentLine = Log_Line($"Gateway:", true);
        }
        public void SetGateway(string value)
        {
            if (value.Split('.').Length < 4)
                return;
            Gateway = value;
            _Gateway = false;
            Replace(currentLine, $"Gateway: {value}");

            if (chassis.selectedController == "CT0" && !chassis.ipConfigsDoneOnCT0)
            {
                _DNSServer = true;
                currentLine = Log_Line($"DNS Servers (1-3, comma-separated):", true);
            }
            else //if already set some things on CT0 then skip to timezones right away! 
            {
                SetLoadingHeavy(true);
                CannotQuitHeavyLoadingTask = true;
                loadingHeavyText.text = text.text;
                text.text = "";
                chassis.ipConfigsDoneOnCT0 = true;
                waitingForIpSetup = false;

                //continue server setup...
                Debug.Log("Continuing server setup...");
                StartCoroutine(ContinueSetup_3());
            }
        }
        public void SetDNSServer(string value)
        {
            if (value.Split('.').Length < 4)
                return;
            DNSServer = value;
            _DNSServer = false;
            _DNSDomain = true;
            Replace(currentLine, $"DNS Servers (1-3, comma-separated): {value}");
            currentLine = Log_Line($"DNS Domain Suffix (Optional):", true);
        }
        public void SetDNSDomain(string value, bool isSkipped = false)
        {
            if (isSkipped)
            {
                DNSDomain = "not set";
                _DNSDomain = false;
                _NTPServers = true;
                Replace(currentLine, $"DNS Domain Suffix (Optional): not set");
                currentLine = Log_Line($"NTP Servers:", true);
                //field.text = "ntp1.purestorage.com,ntp2.purestorage.com,ntp3.purestorage.com";

                field.SetTextWithoutNotify("ntp1.purestorage.com,ntp2.purestorage.com,ntp3.purestorage.com");
                int l = field.text.Length;
                field.caretPosition = l;
                field.selectionAnchorPosition = l;
                field.selectionAnchorPosition = l;
            }
            else
            {
                if (value.Split('.').Length < 2)
                    return;

                DNSDomain = value;
                _DNSDomain = false;
                _NTPServers = true;
                Replace(currentLine, $"DNS Domain Suffix (Optional): {value}");
                currentLine = Log_Line($"NTP Servers:", true);

                field.SetTextWithoutNotify("ntp1.purestorage.com,ntp2.purestorage.com,ntp3.purestorage.com");
                int l = field.text.Length;
                field.caretPosition = l;
                field.selectionAnchorPosition = l;
                field.selectionAnchorPosition = l;
                field.DeactivateInputField();
            }
        }
        public void SetNTPServer(string value) 
        {
            if (value.Split('.').Length < 2)
                return;
            NTPServers = value;
            _NTPServers = false;
            _SMTPRelayHost = true;
            Replace(currentLine, $"NTP Servers: {value}");
            currentLine = Log_Line($"Email Relay Server (Optional):", true);
        }
        public void SetEmailRelayServer(string value, bool isSkipped = false)
        {
            if (isSkipped)
            {
                SMTPRelayHost = "not set";
                _SMTPRelayHost = false;
                _SMTPSenderDomain = true;
                Replace(currentLine, $"Email Relay Server (Optional): not set");
                currentLine = Log_Line($"Email Sender Domain:", true);
            }
            else 
            {
                if (value.Split('.').Length < 2)
                    return;

                SMTPRelayHost = value;
                _SMTPRelayHost = false;
                _SMTPSenderDomain = true;
                Replace(currentLine, $"Email Relay Server (Optional): {value}");
                currentLine = Log_Line($"Email Sender Domain:", true);
            }
        }
        public void SetEmailSenderDomain(string value)
        {
            if (value.Split('.').Length < 2)
                return;
            SMTPSenderDomain = value;
            _SMTPSenderDomain = false;
            _AlertSenderRecipients = true;
            Replace(currentLine, $"Email Sender Domain: {value}");
            currentLine = Log_Line($"Alert Email Recipients (Optional: 0-19, comma-separated):", true);
        }
        public void SetAlertEmailRec(string value, bool isSkipped = false)
        {
            if (isSkipped)
            {
                AlertSenderRecipients = "not set";
                _AlertSenderRecipients = false;
                waitingForIpSetup = false;
                Replace(currentLine, $"Alert Email Recipients (Optional: 0-19, comma-separated): not set");
            }
            else 
            {
                if (value.Split('.').Length < 2)
                    return;

                AlertSenderRecipients = value;
                _AlertSenderRecipients = false;
                waitingForIpSetup = false;
                Replace(currentLine, $"Alert Email Recipients (Optional: 0-19, comma-separated): {value}");
            }

            SetLoadingHeavy(true);
            CannotQuitHeavyLoadingTask = true;
            loadingHeavyText.text = text.text;
            text.text = "";
            chassis.ipConfigsDoneOnCT0 = true;

            //continue server setup...
            Debug.Log("Continuing server setup...");
            StartCoroutine(ContinueSetup_3());
        }

        private void SetIpConfigs(string value)
        {
            string[] splits = value.Split(' ');
            field.ActivateInputField();
            field.text = "";

            if (_arrayName) 
            {
                SetArrayName(splits[0]); return;
            }
            if (_physicalIp) {
                
                SetPhysicalIP(splits[0]); return; 
            }
            if (_virtualIp)
            {
                SetVirtualIP(splits[0]); return;
            }
            if (_NTPServers) 
            {
                SetNTPServer(splits[0]); return;
            }
            if (_Netmask)
            {
                SetNetMask(splits[0]); return;
            }
            if (_Gateway)
            {
                SetGateway(splits[0]); return;
            }
            if (_DNSServer)
            {
                SetDNSServer(splits[0]); return;
            }
            if (_DNSDomain)
            {
                if (value == "" || value == "\n") 
                {
                    SetDNSDomain("", true); //skip domain suffix as it's optional!
                    return;
                }
                else
                    SetDNSDomain(splits[0]); return;
            }
            if (_SMTPRelayHost)
            {
                if (value == "" || value == "\n")
                {
                    SetEmailRelayServer("", true); //skip domain suffix as it's optional!
                    return;
                }
                else
                    SetEmailRelayServer(splits[0]); return;
            }
            if (_SMTPSenderDomain)
            {
                SetEmailSenderDomain(splits[0]); return;
            }
            if (_AlertSenderRecipients)
            {
                if (value == "" || value == "\n")
                {
                    SetAlertEmailRec("", true); //skip domain suffix as it's optional!
                    return;
                }
                else
                    SetAlertEmailRec(splits[0]); return;
            }
        }

        private IEnumerator ContinueSetup_3() 
        {
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("\nHTTP proxy for phonehome and remoteassist (optional):n");
            yield return new WaitForSeconds(3f * timeMultiplier);
            Log("Confirm configuration.");
            Log($"  Physical IP:             {physicalIp}\n  Virtual IP:              {virtualIp}\n  Netmask:                 {Netmask}\n  Gateway:                 {Gateway}");
            Log($"  DNS Servers:             {DNSServer}\n  DNS Domain:              {DNSDomain}\n  NTP Servers:             {NTPServers}\n  SMTP Relay Host:         {SMTPRelayHost}");
            Log($"  SMTP Sender Domain:      {SMTPSenderDomain}\n  Alert Email Recipients:  {AlertSenderRecipients}\nHTTP proxy:");
            yield return new WaitForSeconds(2f * timeMultiplier);
            currentLine = Log_Line("Type 'y' to apply configurations or 'n' to re-enter parameters:", true);

            SetLoadingHeavy(false);
            text.text = loadingHeavyText.text;
            CannotQuitHeavyLoadingTask = false;
            applyConfigToArray = true;
            loadingHeavyText.text = "";
            SetLoginText("");
        }

        public void ApplyConfigToArray() 
        {
            Replace(currentLine, "Type 'y' to apply configurations or 'n' to re-enter parameters: y");
            //chassis.SetComputerName(arrayName, chassis.selectedController == "CT0" ? 0 : 1);
            chassis.ipConfigsDoneOnCT0 = true;
            
            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            applyConfigToArray = false;
            logWriter.AddToLog("\nApplying configuration to the array.");
            text.text = "Applying configuration to the array.";

            //really apply config to array!
            FlashArray array = chassis.selectedController == "CT0" ? chassis.flashArrays[0] : chassis.flashArrays[1];
            array.arrayName = arrayName;
            array.physicalIp = physicalIp;
            array.virtualIp = virtualIp;
            array.NTPServers = NTPServers;
            array.DNSServer = DNSServer;
            array.DNSDomain = DNSDomain;
            array.Netmask = Netmask;
            array.Gateway = Gateway;
            array.SMTPSenderDomain = SMTPSenderDomain;
            array.SMTPRelayHost = SMTPRelayHost;
            array.AlertSenderRecipients = AlertSenderRecipients;

            StartCoroutine(ContinueSetup_4());
        }

        public void ReenterArrayConfigs() 
        {
            applyConfigToArray = false;
            Replace(currentLine, "Type 'y' to apply configurations or 'n' to re-enter parameters: n");

            Log("Re-enter configurations (all previous inputs are erased):");
            currentLine = Log_Line("Array Name:", true);
            _arrayName = true;
            _physicalIp = false;
            _NTPServers = false;
            _virtualIp = false;
            _DNSDomain = false;
            _DNSServer=false;
            _Gateway = false;
            _Netmask = false;
            _SMTPRelayHost = false;
            _SMTPSenderDomain = false;
            waitingForIpSetup = true;
            SetLoadingHeavy(false);
            CannotQuitHeavyLoadingTask = false;
            SetLoginText("");
        }

        private IEnumerator ContinueSetup_4() 
        {
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("Configuring array name.\nConfiguring management port.\nConfiguring network interfaces\nConfiguring DNS." +
                "\nConfiguring NTP server.");
            yield return new WaitForSeconds(1.5f * timeMultiplier);
            Log("Configuring HTTP proxy.\nConfiguring SMTP sender domain and relay host.\nConfiguring alert email." +
                "New configuration applied.");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("CONFIG_TASK_COMPLETED:CONFIG_TASK_CONFIG_FLASHARRAY");
            Log("Skipping connectivity tests.");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("CONFIG_TASK_COMPLETED:CONFIG_TASK_TEST_CONNECTIVITY");
            yield return new WaitForSeconds(2f * timeMultiplier);

            //for secondary controller show pureuser before timezone!
            if (chassis.selectedController == "CT0")
            {
                //timezone
                Log("passwd: password expiry information changed.\nCurrent time zone: Etc/UTC");
                currentLine = Log_Line("Change time zone [requires reboot] (y/n):", true);
                timezoneManualChange = false;

                SetLoadingHeavy(false);
                text.text = loadingHeavyText.text;
                CannotQuitHeavyLoadingTask = false;
                waitingForTimezone = true;
                loadingHeavyText.text = "";
                SetLoginText("");
            }
            else 
            {
                //pureuser
                ChangeUserPassword();
            }
        }

        public void ManualChangeTimezone() 
        {
            timezoneManualChange = true;
            lastLoginText = LoginText;
            SetLoginText("");
            ChangeTimeZone(false);
        }

        public void ChangeTimeZone(bool replaceLine = true) 
        {
            SetLoadingHeavy(true);
            SetLoginText("");
            loadingHeavyText.text = text.text;
            waitingForTimezone = false;
            CannotQuitHeavyLoadingTask = true;

            if(replaceLine) 
                Replace(currentLine, "Change time zone [requires reboot] (y/n): y");

            Log("Configuring tzdata\n------------------");
            StartCoroutine(SetupTimeZone(false));
        }

        public void SkipTimeZone() 
        {
            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            waitingForTimezone = false;
            CannotQuitHeavyLoadingTask = true;
            Replace(currentLine, "Change time zone [requires reboot] (y/n): n");

            StartCoroutine(SetupTimeZone(true));
        }

        private IEnumerator SetupTimeZone(bool skip)
        {
            yield return new WaitForSeconds(1f * timeMultiplier);
            loadingHeavyText.text += "\n";
            if (!skip)
            {
                Log("Please select the geographic area in which you live. Subsequent configuration questions will narrow this down by presenting a list of cities, representing the time zones in which they are located.\n");
                timeZoneManager.ShowGeographicAreas();
                currentLine = Log_Line("Geographic area:", true);
                SetLoadingHeavy(false);
                ActivateField();
                text.text = loadingHeavyText.text;
                CannotQuitHeavyLoadingTask = false;
                loadingHeavyText.text = "";
                waitingForTimeSelection = true;
                geoRegionSelect = true;
                yield return null;
            }
            else
            {
                Log("Skipping time zone setup, continuing with default time zone.");
                StartCoroutine(ContinueSetup_5());
            }
        }

        public void SelectTimeZoneNumber(int value) 
        {
            if (geoRegionSelect)
            {
                if (value - 1 >= timeZoneManager.GeographicRegions.Count || value - 1 < 0)
                {
                    return;
                }

                Replace(currentLine, "Geographic area: " + value);

                if (value - 1 == timeZoneManager.GeographicRegions.Count - 1) //none of the above? 
                {
                    Log("Skipping time zone setup, continuing with default time zone.");
                    StartCoroutine(ContinueSetup_5());
                    return;
                }
            }
            else 
            {
                if (!timeZoneManager.HasTimezonesUpto(value - 1)) 
                {
                    Replace(currentLine, "Index is out of Range! No Time zone exists at input index!");
                    currentLine = Log_Line("Time Zone:", true);
                    return;
                }

                Replace(currentLine, "Time zone: " + value);
                waitingForTimeSelection = false;

                //we do not use real time zones (yet) will default and use UTC for now!
                chassis.GetCurrentController().tempTimeZone = $"'{geoArea}/{timeZoneManager.GetAreaNameIndexed(value - 1)}'";
                Log("\n\nCurrent default time zone: " + $"'{geoArea}/{timeZoneManager.GetAreaNameIndexed(value-1)}'");

                if (!timezoneManualChange) //auto during setup?
                {
                    SetLoadingHeavy(true);
                    CannotQuitHeavyLoadingTask = false;
                    loadingHeavyText.text = text.text;

                    StartCoroutine(ContinueSetup_5());
                }
                else
                {
                    /*SetLoginText(lastLoginText);
                    lastLoginText = "";
                    lastCommandInput = "";*/

                    timezoneManualChange = false;
                    
                    //confirm
                    currentLine = Log_Line($"Confirm time zone change from {chassis.GetCurrentController().TimeZone} to {chassis.GetCurrentController().tempTimeZone} (y/N):", true);
                    waitingForTimezoneConfirm = true;
                }
                return;
            }

            geoRegionSelect = false;

            /*if(timeZoneManager.GeographicRegions[value] == "US")
                geoArea = timeZoneManager.GeographicRegions[2]; //select America
            else
                geoArea = timeZoneManager.GeographicRegions[value];*/
            geoArea = timeZoneManager.GeographicRegions[value];

            Log("\n\nPlease select the city or region corresponding to your time zone.\n");
            timeZoneManager.ShowTimeZones(geoArea);
            ActivateField();
            currentLine = Log_Line("Time zone:", true);
        }

        private IEnumerator ContinueSetup_5()
        {
            yield return new WaitForSeconds(1f * timeMultiplier);
            //apply time zone!
            chassis.GetCurrentController().TimeZone = chassis.GetCurrentController().tempTimeZone;

            Log($"Local time is now:      {timeZoneManager.GetTimeNow()}.\nUniversal Time is now:  {System.DateTime.UtcNow.ToString()}.");
            Log("monitor stop/waiting\nwait-for-state pureapp stop/waiting\nwait-for-state mariadb stop/waiting");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("lio-drv disabling\nplatform stop/waiting\nwait-for-state foed stop/waiting\niostat stop/waiting");
            yield return new WaitForSeconds(1f * timeMultiplier);
            Log("statistics stop/waiting\nrtm-file-collector stop/waiting\nwait-for-state lio-drv stop/waiting\npurity stop/waiting\nwait-for-state purity_started stop/waiting");
            yield return new WaitForSeconds(2f * timeMultiplier);
            Log("Pure Storage is offline.\nTunable parameter set: PURITY_START_ON_BOOT=1");

            //here it wants the trainee to setup pure-user password!
            //make sure to let user do this on CT1 installation not after CT0
            if (chassis.selectedController == "CT0" && !chassis.OSInstalled(0)) 
            {
                wantsToInputPassword = false;
                SkipPassword();
                yield return null;
            }

            //enter to rebbot CT1
            if (chassis.selectedController == "CT1" && !chassis.OSInstalled(1)) 
            {
                Log("Press ENTER to reboot.", true, true);
                Invoke(nameof(EnterToBoot), 0.5f);
                yield return null;
            }

            //ChangeUserPassword();

            /*Log("puresetup:Enabling pureuser\npasswd: password expiry information changed.\nSetting the new pureuser password.");
            currentLine = Log_Line("Type 'continue' to enter the new pureuser password, Type 'skip' to skip password setup.", true);
            wantsToInputPassword = true;

            SetLoadingHeavy(false);
            text.text = loadingHeavyText.text;
            CannotQuitHeavyLoadingTask = false;
            loadingHeavyText.text = "";*/
        }

        private void ChangeUserPassword() 
        {
            Log("puresetup:Enabling pureuser\npasswd: password expiry information changed.\nSetting the new pureuser password.");
            currentLine = Log_Line("Type 'continue' to enter the new pureuser password, Type 'skip' to skip password setup.", true);
            wantsToInputPassword = true;

            SetLoadingHeavy(false);
            text.text = loadingHeavyText.text;
            CannotQuitHeavyLoadingTask = false;
            loadingHeavyText.text = "";
        }

        public void ContinuePassword() 
        {
            wantsToInputPassword = false;
            Replace(currentLine, "Type 'continue' to enter the new pureuser password, Selected value: 'continue'");
            Log("puresetup:Prompt: Setting the new pureuser password.\npuresetup:Requesting new pureuser password.");
            currentLine = Log_Line("Enter old password:", true);
            inputOldPass = true;
        }

        private string lastPasswd = "";
        public void InputPassword(string passwd) 
        {
            if (inputOldPass) { 
                inputOldPass = false; 
                inputNewPass = true;
                Replace(currentLine, "Enter old password: "+ConvertToPassword(passwd));
                currentLine=Log_Line("Enter new password:", true);
                return;
            }

            if (inputNewPass) { 
                inputNewPass = false; 
                lastPasswd = passwd;
                reenterNewPass = true;
                Replace(currentLine, "Enter new password: "+ConvertToPassword(passwd));
                currentLine = Log_Line("Retype new password:", true);
                return;
            }

            if (reenterNewPass) {
                if (passwd == lastPasswd)
                {
                    reenterNewPass = false;
                    Replace(currentLine, "Retype new password: " + ConvertToPassword(passwd));
                    /*Log("puresetup:Pureuser password successfully set.\nPress ENTER to reboot.");
                    Invoke(nameof(EnterToBoot), 2f);*/

                    //allow user to setup timezone after pureuser now!
                    Log("passwd: password expiry information changed.\nCurrent time zone: Etc/UTC");
                    currentLine = Log_Line("Change time zone [requires reboot] (y/n):", true);
                    timezoneManualChange = false;

                    SetLoadingHeavy(false);
                    CannotQuitHeavyLoadingTask = false;
                    text.text = loadingHeavyText.text;
                    SetLoginText("");
                    ChangeTimeZone();
                    return;
                }
                else {
                    lastCommandInput = "";
                    Log("   Passwords do not match!", true);
                }
            }
        }

        private void EnterToBoot() {
            enterToRebootToSetupOS = true;
        }

        public void SkipPassword() 
        {
            wantsToInputPassword = false;
            reenterNewPass = false;
            inputNewPass = false;
            inputOldPass = false;

            if (chassis.selectedController == "CT0")
            {
                Log("Press ENTER to reboot.", true, true);
                enterToRebootToSetupOS = true;

                SetLoadingHeavy(false);
                text.text = loadingHeavyText.text;
                CannotQuitHeavyLoadingTask = false;
                loadingHeavyText.text = "";
            }

            if(chassis.selectedController == "CT1" && !chassis.OSInstalled(1))
            {
                //now change time-zone
                Log("passwd: password expiry information changed.\nCurrent time zone: Etc/UTC");
                currentLine = Log_Line("Change time zone [requires reboot] (y/n):", true);

                SetLoadingHeavy(false);
                //text.text = loadingHeavyText.text;
                CannotQuitHeavyLoadingTask = false;
                waitingForTimezone = true;
                loadingHeavyText.text = "";
                SetLoginText("");
                
                timezoneManualChange = false;
            }
        }

        private string ConvertToPassword(string pass) {
            string p = "";
            for (int i = 0; i < pass.Length; i++)
            {
                p += "*";
            }
            return p;
        }

        public void ExitSetup() 
        {
            //make sure to log the last line!
            if (setupPausedOnDataErase)
                Replace(currentLine, "(continue/EXIT): exit");

            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            CannotQuitHeavyLoadingTask = true;

            Mounted = false;
            isSettingUpOS = false;
            setupPausedOnDataErase = false;
            waitingForTimezone = false;
            setupPausedOnRapidDataLock = false;
            applyConfigToArray = false;
            enterToRebootToSetupOS = false;
            waitingForIpSetup = false;
            currentLine = -1;

            Log("Purity Setup cancelled!\nRolling back changes... This may take several minutes");
            Invoke(nameof(Logout_Invoke), 15f);
        }

        public void ShowArrayInfo() 
        {
            Log("##########################################\n" +
                "#   Welcome to the Purity Setup Wizard   #\n##########################################\n");
            Log($"  Physical IP:             {chassis.flashArrays[0].physicalIp}\n  Virtual IP:              {chassis.flashArrays[0].virtualIp}\n  Netmask:                 {chassis.flashArrays[0].Netmask}\n  Gateway:                 {chassis.flashArrays[0].Gateway}");
            Log($"  DNS Servers:             {chassis.flashArrays[0].DNSServer}\n  DNS Domain:              {chassis.flashArrays[0].DNSDomain}\n  NTP Servers:             {chassis.flashArrays[0].NTPServers}\n  SMTP Relay Host:         {chassis.flashArrays[0].SMTPRelayHost}");
            Log($"  SMTP Sender Domain:      {chassis.flashArrays[0].SMTPSenderDomain}\n  Alert Email Recipients:  {chassis.flashArrays[0].AlertSenderRecipients}\n  HTTP proxy:");

            Log($"  Physical IP:             {chassis.flashArrays[1].physicalIp}\n  Netmask:                 {chassis.flashArrays[1].Netmask}\n  Gateway:                 {chassis.flashArrays[1].Gateway}\nCONFIG_TASK_COMPLETED:CONFIG_TASK_PURESETUP_EXITED");
        }

        #endregion
    
        #region MISC_COMMANDS

        //change state of controller to primary or secondary (wait 30 sec with amber lights too)
        public void ChangeControllerState(FlashArray array, string state)
        {
            SetLoadingHeavy(true);
            loadingHeavyText.text = text.text;
            text.text = "";
            CannotQuitHeavyLoadingTask = true;

            chassis.flashArrays[0].SetLights(false, true);
            chassis.flashArrays[1].SetLights(false, true);

            StartCoroutine(DelayedChangeControllerState(array.index, state));
        }

        private IEnumerator DelayedChangeControllerState(int arrayIndex, string state)
        {
            yield return new WaitForSeconds(30f * timeMultiplier); //30 seconds
            CannotQuitHeavyLoadingTask =false;
            SetLoadingHeavy(false);

            //make one primary and other secondary AS A MUST!
            if(arrayIndex == 0) {
                chassis.flashArrays[0].State = state;
                chassis.flashArrays[1].State = state == "primary" ? "secondary" : "primary";
            }
            else
            {
                chassis.flashArrays[1].State = state;
                chassis.flashArrays[0].State = state == "primary" ? "secondary" : "primary";
            }

            //reset lights on both controllers!
            chassis.SetChassisLights();
            //chassis.flashArrays[0].SetLights(true, true);
            //chassis.flashArrays[1].SetLights(true, true);

            text.text = loadingHeavyText.text;
            loadingHeavyText.text = "";
        }

        #endregion
    }
}