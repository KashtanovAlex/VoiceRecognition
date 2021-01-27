using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Speech;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;

namespace VoiceControl
{
    public partial class Form1 : Form
    {
        private CultureInfo _culture;
        private SpeechRecognitionEngine _sre;
        private SpeechRecognitionEngine sr;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _culture = new CultureInfo("ru-RU");
                _sre = new SpeechRecognitionEngine(_culture);
                // Setup event handlers
                _sre.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(sr_SpeechDetected);
                _sre.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(sr_RecognizeCompleted);
                _sre.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sr_SpeechHypothesized);
                _sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sr_SpeechRecognitionRejected);
                _sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sr_SpeechRecognized);

                // select input source
                _sre.SetInputToDefaultAudioDevice();

                // load grammar
                _sre.LoadGrammar(CreateSampleGrammar1());
                _sre.LoadGrammar(CreateSampleGrammar2());

                // start recognition
                _sre.RecognizeAsync(RecognizeMode.Multiple);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private Choices CreateSampleChoices()
        {
            var val1 = new SemanticResultValue("калькулятор", "calc");
            var val2 = new SemanticResultValue("проводник", "explorer");
            var val3 = new SemanticResultValue("блокнот", "notepad");
            var val4 = new SemanticResultValue("пэйнт", "mspaint");
            var val5 = new SemanticResultValue("дэмопример", "ConsoleApplication1.exe");

            return new Choices(val1, val2, val3, val4,val5);
        }

        private Grammar CreateSampleGrammar1()
        {
            var programs = CreateSampleChoices();

            var grammarBuilder = new GrammarBuilder("запустить", SubsetMatchingMode.SubsequenceContentRequired);
            grammarBuilder.Culture = _culture;
            grammarBuilder.Append(new SemanticResultKey("start", programs));

            return new Grammar(grammarBuilder);
        }

        private Grammar CreateSampleGrammar2()
        {
            var programs = CreateSampleChoices();

            var grammarBuilder = new GrammarBuilder("закрыть", SubsetMatchingMode.SubsequenceContentRequired);
            grammarBuilder.Culture = _culture;
            grammarBuilder.Append(new SemanticResultKey("close", programs));

            return new Grammar(grammarBuilder);
        }

        private void sr_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            AppendLine("Speech Recognition Rejected: " + e.Result.Text);
        }

        private void sr_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            AppendLine("Speech Hypothesized: " + e.Result.Text + " (" + e.Result.Confidence + ")");
        }

        private void sr_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            AppendLine("Recognize Completed: " + e.Result.Text);
        }

        private void sr_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            AppendLine("Speech Detected: audio pos " + e.AudioPosition);
        }

        private void sr_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            AppendLine("\t" + "Speech Recognized:");

            AppendLine(e.Result.Text + " (" + e.Result.Confidence + ")");

            if (e.Result.Confidence < 0.1f)
                return;

            for (var i = 0; i < e.Result.Alternates.Count; ++i)
            {
                AppendLine("\t" + "Alternate: " + e.Result.Alternates[i].Text + " (" + e.Result.Alternates[i].Confidence + ")");
            }

            for (var i = 0; i < e.Result.Words.Count; ++i)
            {
                AppendLine("\t" + "Word: " + e.Result.Words[i].Text + " (" + e.Result.Words[i].Confidence + ")");

                if (e.Result.Words[i].Confidence < 0.1f)
                    return;
            }

            foreach (var s in e.Result.Semantics)
            {
                var program = (string)s.Value.Value;

                switch (s.Key)
                {
                    case "start":
                        Process.Start(program);
                        break;
                    case "close":
                        var p = Process.GetProcesses();
                        var proc = p.Where(n => n.ProcessName.Contains(program)).FirstOrDefault();
                        if (proc!=null)
                            proc.Kill();
                        break;
                }
            }
        }

        private void AppendLine(string text)
        {
            richTextBox1.AppendText(text + Environment.NewLine);
            richTextBox1.ScrollToCaret();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
