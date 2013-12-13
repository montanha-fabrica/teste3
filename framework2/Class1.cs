using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;
using Microsoft.Speech;
using System.Windows.Media;
//using System.Windows.Media.Imaging;


namespace framework2
{
    public class Class1
    {
        public void teste()
        {
            Console.WriteLine("burro");
        }
    }

    public class AudioCreate 
    {
        private static AudioCreate instance;  //atributo singleton
        
        private KinectSensor Kinect;

        private Stream audioStream;

        private Thread readingThread;

        private bool reading;

        private int AudioPollingInterval;

        private int SamplesPerMillisecond;

        private int BytesPerSample;

        private byte[] audioBuffer;

        private int energyIndex;

        private int SamplesPerColumn;

        private int EnergyBitmapWidth;

        private int EnergyBitmapHeight;

        private double[] energy;

        private double[] energycopy;

        private int newEnergyAvailable;

        private double beenangle;

        private double sourceangle;

        private double sourceangconfidence;

        private bool inlock;

       

     //   private readonly WriteableBitmap energyBitmap;



        public readonly object energyLock = new object();

        public AudioCreate()
        {
        }

        private AudioCreate(KinectSensor theKinect) //construtor da classe 
        {
            Kinect = theKinect;
            instance = this;
        }             

        public static AudioCreate Instance(KinectSensor thekinect )    // metodo de implementação do singleton
        {
            
                if (instance == null)
                {
                    instance = new AudioCreate( thekinect );
                    
                }

                return instance;
            
        }


        public void Stoplistem()
        {
            reading = false;
          //  this.readingThread.Abort();
           
        }

        public void ReciveAudio(int NBetween, int Npermilisec, int NBytes, int SPerCol, int EnergBmapHe, int EnergBmapWi)  //metodo de recebimento de audio - retorna objeto de audio
        {
            
            BytesPerSample = NBytes;
            SamplesPerMillisecond = Npermilisec;
            AudioPollingInterval = NBetween;

            SamplesPerColumn = SPerCol;
            EnergyBitmapWidth = EnergBmapWi;
            EnergyBitmapHeight = EnergBmapHe;

           

            audioStream = Kinect.AudioSource.Start();
            reading = true;

                
                this.readingThread = new Thread(AudioReadingThread);
                this.readingThread.Start();

        }

        private void AudioReadingThread()
        {
            // Bottom portion of computed energy signal that will be discarded as noise.
            // Only portion of signal above noise floor will be displayed.
            const double EnergyNoiseFloor = 0.2;
           // int aux;
            double instantenergy;
            energycopy = new double[(uint)(EnergyBitmapWidth * 1.25)];
            inlock = true;
            
                while (this.reading)
                {


                    audioBuffer = new byte[AudioPollingInterval * SamplesPerMillisecond * BytesPerSample];

                    energy = new double[(uint)(EnergyBitmapWidth * 1.25)];


                    int readCount = audioStream.Read(audioBuffer, 0, audioBuffer.Length);

                    int accumulatedSampleCount = 0;

                    double accumulatedSquareSum = 0;

                    // Calculate energy corresponding to captured audio.
                    // In a computationally intensive application, do all the processing like
                    // computing energy, filtering, etc. in a separate thread.
                    inlock = true;
                    lock (this.energyLock)
                    {
                        for (int i = 0; i < readCount; i += 2)
                        {
                            // compute the sum of squares of audio samples that will get accumulated
                            // into a single energy value.
                            short audioSample = BitConverter.ToInt16(audioBuffer, i);
                            accumulatedSquareSum += audioSample * audioSample;
                            ++accumulatedSampleCount;

                            if (accumulatedSampleCount < SamplesPerColumn)
                            {
                                continue;
                            }

                            // Each energy value will represent the logarithm of the mean of the
                            // sum of squares of a group of audio samples.
                            double meanSquare = accumulatedSquareSum / SamplesPerColumn;
                            double amplitude = Math.Log(meanSquare) / Math.Log(int.MaxValue);

                            // Renormalize signal above noise floor to [0,1] range.
                            instantenergy = Math.Max(0, amplitude - EnergyNoiseFloor) / (1 - EnergyNoiseFloor);

                            this.energy[this.energyIndex] = instantenergy;
                            energycopy[this.energyIndex] = instantenergy;


                            Console.WriteLine("energy value " + getactualenergy() + " in index " + this.energyIndex);

                            this.energyIndex = (this.energyIndex + 1) % this.energy.Length;

                            accumulatedSquareSum = 0;
                            accumulatedSampleCount = 0;
                            ++this.newEnergyAvailable;
                        }
                        
                        // Console.WriteLine("centro...maldito valor q deveria ter? " + energycopy[50]);
                    }
                    
                  //  Console.WriteLine(newenergy);
                    inlock = false;
                }
            }
        
        public double GetEnergyinstant(int index)
        {
            double aux;
            if (index < this.energy.Length)
            {
                try
                {
                    while (inlock)
                    {
                    }
                        aux = energycopy[index];
                    
                }
                catch
                {
                    return -1;
                }
                
                return aux;
                
            }
            else return -1;

        }

        public double[] getallenergy()
        {
           
                double[] aux;
                aux = new double[this.energy.Length];
                while (inlock)
                {

                }
                
                
                  //  Console.WriteLine(newenergy);
                    this.energycopy.CopyTo(aux, 0);
                    
                
                
                return aux;
            
        }

        public double getactualenergy()
        {
            double aux;
            
                try
                {
                    
                        aux = this.energy[this.energyIndex - 1];
                    
                    
                }
                catch
                {
                   // Console.WriteLine("erro1");
                    return -1;
                }
                return aux;
            
           
        }

        public double getbeenangle()
        {
            
            beenangle = this.Kinect.AudioSource.BeamAngle;
            return beenangle;
        }
        public double getsourceangle()
        {
            sourceangle = this.Kinect.AudioSource.SoundSourceAngle;
            return sourceangle;
        }
        public double getangleconfidence()
        {
            sourceangconfidence = this.Kinect.AudioSource.SoundSourceAngleConfidence;
            return sourceangconfidence;
        }

        public int getenergyindex()
        {
            return this.energyIndex;
        }

        public Object MakeAudio(String Create) // metodo que cria audio a partir de 1 string
        {

            return 0;
        }

        public void ChangeAngle(int Angle) // metodo que move a camera segundo o valor int do angulo informado
        {
            try
            {
                Kinect.ElevationAngle = Angle;
            }
            catch
            {
                Console.WriteLine("Bad Tick Angle");
            }
        }

    }


    public class AudioAplication
    {
        private static AudioCreate concrete;
        

        public void CreateAudioInstance(KinectSensor thekinect)
        {
            concrete = AudioCreate.Instance(thekinect);
            
        }

        public void NewAudioCreate(string lyric)
        {
            concrete.MakeAudio(lyric);
        }

        public void AudioListner(int NBetween, int Npermilisec, int NBytes, int SPerCol, int EnergBmapHe, int EnergBmapWi)
        {
            
            
                
                    concrete.ReciveAudio(NBetween, Npermilisec, NBytes, SPerCol, EnergBmapHe, EnergBmapWi);
                
           
        }

        public int getindex()
        {
            
                return concrete.getenergyindex();
            
        }

        public object getlock(){
            return concrete.energyLock;
        }

        public void StopLister()
        {
            concrete.Stoplistem();
        }

        public double[] Whatwaslisten()
        {
            
                return concrete.getallenergy();
            
        }

        public double ListnenInstant(int x) 
        {
            return concrete.GetEnergyinstant(x);
        }

        public double GetActualBeenAngle()
        {
            return concrete.getbeenangle();
        }
        
        public double GetActualSourceAngle()
        {
            return concrete.getsourceangle();
        }

        public double GetActualAngleConfidance()
        {
            return concrete.getangleconfidence();
        }
        public void setnewangle(int angle)
        {
            concrete.ChangeAngle(angle);
        }
    }

    //-------------------------------fim classes criacionais ---------------------------------------


    public class Gliph_flyweight
    {
        private double[] Audiodata;

        public void setdata(double[] thedata)
        {
            Audiodata = thedata;
        }

        public double[] getaudio()
        {
            return Audiodata;
        }
    }


    public class flyweight
    {
        private LinkedList<Gliph_flyweight> gliphlist;

        private Thread readingThread;

        private bool block;

        private AudioAplication recivdatas;

        public readonly object flylock = new object();

        public flyweight(AudioAplication datas)
        {
            gliphlist = new LinkedList<Gliph_flyweight>();
            block=true;
            lock(datas){
            recivdatas = datas;
            
            this.readingThread = new Thread(threadlock);
            this.readingThread.Start();
            }
        }

        public void demonstrate()
        {
            if (gliphlist.Count != 0)
            {
                int count;
                double[] aux;
                int tam;

                while (block)
                {
                    count = 0;
                    tam = gliphlist.Count;
                    aux = gliphlist.Last.Value.getaudio();
                    while (count < aux.Length)
                    {
                        Console.WriteLine("Gliph " + tam + " na posição " + count + " temos " + aux[count]);
                        count++;
                    }
                }
            }
            else
            {
                Console.WriteLine("Gliph " + gliphlist.Count);
                this.demonstrate();
            }
        }

        public void stoprecept()
        {
            block = false;
        }
        public void restart()
        {
            block = true;
            this.readingThread = new Thread(threadlock);
            this.readingThread.Start();
        }

        private void threadlock()
        {
            Gliph_flyweight aux;
            // double[] reviv;


            lock(flylock){
                while (this.block)
                {
                    if (recivdatas.getindex() > recivdatas.Whatwaslisten().Length - 2)
                    {
                        aux = new Gliph_flyweight();

                        aux.setdata(recivdatas.Whatwaslisten());
                        if (gliphlist.Count != 0)
                        {
                            if (!aux.getaudio().Equals(gliphlist.Last.Value.getaudio()))
                            {

                                this.gliphlist.AddLast(aux);
                             //   Console.WriteLine("Gliph " + this.gliphlist.Count);

                            }
                            else
                            {
                                Console.WriteLine("Gliph " + this.gliphlist.Count + " na posição 50 temos " + this.gliphlist.Last.Value.getaudio()[50]);
                            }
                        }
                        else
                        {
                            this.gliphlist.AddLast(aux);
                        }
                    }
                }
            }
        }

        public double[] getdatafrom(int index)
        {
            lock (recivdatas.getlock())
            {
                Gliph_flyweight aux;
                aux = this.gliphlist.ElementAtOrDefault(index);
                return aux.getaudio();
            }
        }

        

       
        /*
        public void Highpassfilter(double valueabouveof)
        {
            int x;
            if (valueabouveof >= 0 && valueabouveof <= 1)
            {
                for (x = 0; x < _flavour.Length; x++)
                {
                    if (_flavour[x] < valueabouveof) _flavour[x] = 0;
                }
            }
        }
        public void Lowpassfilter(double valuelowerthen)
        {
            int x;
            if (valuelowerthen >= 0 && valuelowerthen <= 1)
            {
                for (x = 0; x < _flavour.Length; x++)
                {
                    if (_flavour[x] > valuelowerthen) _flavour[x] = 0;
                    
                }
                Console.WriteLine("work done!");
            }
        }*/


    }

    

}
