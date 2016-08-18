using System;
using System.Collections.Generic;
using System.Text;

namespace MadeInTheUSB.MCU
{
    public class NusbioMCUProtocolSequence
    {
        public List<byte> SentSequence = new List<byte>();
        public List<byte> ReceivedSquence = new List<byte>();
        public int Index;

        public static string DisplayBuffer(byte[] buffer)
        {
            var sb = new StringBuilder();
            if (buffer == null)
            {
                return "null";
            }

            sb.AppendFormat("Len:{0} [", buffer.Length);

            foreach (var b in buffer)
                sb.AppendFormat("{0}, ", b);
            sb.Append("]").AppendLine();

            return sb.ToString();
        }

        public bool AssertReceived(List<byte> s)
        {
            var error = 0;
            for (var i = 0; i < this.ReceivedSquence.Count; i++)
            {
                if (this.ReceivedSquence[i] != s[i])
                {
                    Console.WriteLine("Error index:{0}, expected:{1}, actual:{2}", i, this.ReceivedSquence[i], s[i]);
                    error++;
                }
            }

            if (error > 0)
            {
                Console.Write("Seq Received:{0}", NusbioMCUProtocolSequence.DisplayBuffer(this.ReceivedSquence.ToArray()));
                Console.Write("Seq Expected:{0}", NusbioMCUProtocolSequence.DisplayBuffer(s.ToArray()));
            }

            return error == 0;
        }
    }

    public class NusbioMCUProtocolRecorder
    {
        public bool Record;

        public List<NusbioMCUProtocolSequence> Sequences = new List<NusbioMCUProtocolSequence>();
        public NusbioMCUProtocolRecorder()
        {
        }

        public void AddSent(List<byte> s)
        {
            if (Record)
            {
                var seq = new NusbioMCUProtocolSequence();
                seq.SentSequence.AddRange(s);
                this.Sequences.Add(seq);
            }
        }

        public void AddReceived(List<byte> s)
        {
            if (Record)
            {
                var seq = this.Sequences[this.Sequences.Count - 1];
                seq.ReceivedSquence.AddRange(s);
                this.Sequences.Add(seq);
            }
        }

        public void Save(string fileName)
        {
            var json = System.JSON.JSonObject.Serialize(this);
            System.IO.File.WriteAllText(fileName, json);
        }
        public static NusbioMCUProtocolRecorder Load(string fileName)
        {
            NusbioMCUProtocolRecorder r = System.JSON.JSonObject.Deserialize<NusbioMCUProtocolRecorder>(System.IO.File.ReadAllText(fileName));

            for(var i=0; i<r.Sequences.Count; i++)
                r.Sequences[i].Index = i;

            return r;
        }
    }
}