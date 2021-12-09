using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Globalization;
using Syn.Speech.Api;
using System.IO;
using System.Speech.AudioFormat;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using Microsoft.CognitiveServices.Speech;

namespace SpeechDataset
{
    public partial class mainForm : Form
    {
        System.Speech.Recognition.SpeechRecognizer sr = null;
        static SpeechRecognitionEngine sre = null;

        static Configuration speechConfiguration;
        static StreamSpeechRecognizer speechRecognizer;

        private WaveIn _waveSource;
        private WaveFileWriter _waveFile;
        private readonly MMDevice _micDevice;
        private Stream _collectedData;

        public mainForm()
        {
            InitializeComponent();
            Initialize_Syn();
        }

        async Task Init()
        {
            await RecognizeSpeechAsync();

        }


        public async Task RecognizeSpeechAsync()
        {
            // Creates an instance of a speech config with specified subscription key and service region.
            // Replace with your own subscription key // and service region (e.g., "westus").
            var config = SpeechConfig.FromSubscription("acbc821e-19c7-45f6-9bfd-6dd84f1474e0", "East US");

            // Creates a speech recognizer.
            using (var recognizer = new Microsoft.CognitiveServices.Speech.SpeechRecognizer(config))
            {
                Console.WriteLine("Say something...");

                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed.  The task returns the recognition text as result. 
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query. 
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await recognizer.RecognizeOnceAsync();

                // Checks result.
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    richTextBox_content.Text += "Recognized:" + result.Text + Environment.NewLine;
                   Console.WriteLine($"We recognized: {result.Text}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    richTextBox_content.Text += "Not recognized:" + result.Text + Environment.NewLine;
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                }
            }
        }

        void Initialize_Syn()
        {

            var modelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Models");
            var audioDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Audio");
            var audioFile = Path.Combine(audioDirectory, "Long Audio.wav");

            if (!Directory.Exists(modelsDirectory) || !Directory.Exists(audioDirectory))
            {
                Console.WriteLine("No Models or Audio directory found!! Aborting...");
                Console.ReadLine();
                return;
            }

            speechConfiguration = new Configuration();
            speechConfiguration.AcousticModelPath = modelsDirectory;
            speechConfiguration.DictionaryPath = Path.Combine(modelsDirectory, "cmudict-en-us.dict");
            speechConfiguration.LanguageModelPath = Path.Combine(modelsDirectory, "en-us.lm.dmp");

            speechConfiguration.UseGrammar = true;
            speechConfiguration.GrammarPath = modelsDirectory;
            speechConfiguration.GrammarName = "hello";


            speechRecognizer = new StreamSpeechRecognizer(speechConfiguration);
            var stream = new FileStream(audioFile, FileMode.Open);


            speechRecognizer.StartRecognition(stream);


            Console.WriteLine("Transcribing...");
            var result = speechRecognizer.GetResult();

            if (result != null)
            {
                richTextBox_content.Text += result.GetHypothesis() + Environment.NewLine;

                Console.WriteLine("Result: " + result.GetHypothesis());
            }
            else
            {
                Console.WriteLine("Sorry! Coudn't Transcribe");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Init().Wait();

        }

        //public void StartListening()
        //{
        //    try
        //    {
        //        if (ListenerString == "Start Listening")
        //        {
        //            _waveSource = new WaveIn { WaveFormat = new WaveFormat(16000, 1) };
        //            _waveSource.DataAvailable += waveSource_DataAvailable;
        //            _waveSource.RecordingStopped += waveSource_RecordingStopped;
        //            _collectedData = new MemoryStream();
        //            _waveFile = new WaveFileWriter(_collectedData, _waveSource.WaveFormat);
        //            _waveSource.StartRecording();
        //            ListenerString = "Stop Listening";
        //        }
        //        else if (ListenerString == "Stop Listening")
        //        {
        //            _waveSource.StopRecording();
        //            ListenerString = "Start Listening";
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //    }

        //}

        //private void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        //{
        //    if (_waveFile != null)
        //    {
        //        _waveFile.Write(e.Buffer, 0, e.BytesRecorded);
        //    }
        //}

        //private void waveSource_RecordingStopped(object sender, StoppedEventArgs e)
        //{
        //    try
        //    {
        //        if (_waveSource != null)
        //        {
        //            _waveSource.Dispose();
        //            _waveSource = null;
        //        }

        //        _collectedData.Position = 0;

        //        var fileStream = new FileStream("speech.wav", FileMode.Create);

        //        _collectedData.CopyTo(fileStream);
        //        _collectedData.Position = 0;


        //        if (fileStream.Length > 1000)
        //        {
        //            CanListen = false;
        //            speechRecognizer.StartRecognition(_collectedData);
        //            var result = speechRecognizer.GetResult();
        //            speechRecognizer.StopRecognition();
        //            if (result != null)
        //            {
        //                MessageBox.Show(result.GetHypothesis());
        //            }
        //            _collectedData.Close();
        //        }

        //        fileStream.Close();

        //        CanListen = true;
        //    }
        //    catch (Exception exception)
        //    {
        //        //this.LogError(exception);
        //    }
        //}


        /*
        void Initialize()
        {
            sr = new SpeechRecognizer();
            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));

            // Create and load a grammar.  
            Grammar dictation = new DictationGrammar();
            dictation.Name = "Dictation Grammar";

            sre.LoadGrammar(dictation);

            //var num1To9 = new Choices(
            //    new SemanticResultValue("one", 1),
            //    new SemanticResultValue("two", 2),
            //    new SemanticResultValue("three", 3),
            //    new SemanticResultValue("four", 4),
            //    new SemanticResultValue("five", 5),
            //    new SemanticResultValue("six", 6),
            //    new SemanticResultValue("seven", 7),
            //    new SemanticResultValue("eight", 8),
            //    new SemanticResultValue("nine", 9));


            //GrammarBuilder gb = new GrammarBuilder("hello computer");
            //gb.Culture = CultureInfo.GetCultureInfo("en-US");
            //gb.Append("set timer for");
            //gb.Append(num1To9);
            //gb.Append("seconds");

            //Grammar gr = new Grammar(gb);
            //sre.LoadGrammar(gr);

            sre.SpeechRecognized += sre_SpeechRecognized; // if speech is recognized, call the specified method
            sre.SpeechRecognitionRejected += sre_SpeechRecognitionRejected; // if recognized speech is rejected, call the specified method
            sre.SetInputToDefaultAudioDevice(); // set the input to the default audio device
            sre.RecognizeAsync(RecognizeMode.Multiple); // recognize speech asynchronous

        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text =="clear")
            {
                richTextBox_content.Text = "";
            }
            else
            {
                richTextBox_content.Text +=  e.Result.Text+ Environment.NewLine;
            }
        }

        static void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.WriteLine("Speech rejected. Did you mean:");
            foreach (RecognizedPhrase r in e.Result.Alternates)
            {
                Console.WriteLine("    " + r.Text);
            }
        }
        */

    }
}
