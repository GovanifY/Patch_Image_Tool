using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BAR_Editor
{
    public class BAR
    {
        public List<BARFile> fileList;

        public BAR()
        {
            fileList = new List<BARFile>();
        }

        public BAR(Stream file)
        {
            if (!file.CanRead || !file.CanSeek)
            {
                throw new NotSupportedException("Cannot read or seek in stream");
            }
            using (var br = new BinaryReader(file))
            {
                if (file.Length < 16 || br.ReadUInt32() != 0x01524142)
                {
                    throw new InvalidDataException("Invalid signature");
                }
                int fileC = br.ReadInt32();
                fileList = new List<BARFile>(fileC);
                Debug.WriteLine("Loading BAR with " + fileC + " files");
                file.Position = 16;
                for (int i = 0; i < fileC; i++)
                {
                    var bf = new BARFile();
                    bf.type = br.ReadUInt32();
                    {
                        byte[] b = br.ReadBytes(4);
                        Buffer.BlockCopy(b, 0, bf._id, 0, 4);
                    }
                    long lpos = file.Position + 8;
                    uint pos = br.ReadUInt32();
                    int len = br.ReadInt32();
                    file.Position = pos;
                    bf.data = br.ReadBytes(len);
                    fileList.Add(bf);
                    file.Position = lpos;
                }
            }
            //BinaryReader should close file
        }

        public int fileCount
        {
            get { return fileList.Count; }
        }

        public void save(Stream file)
        {
            if (!file.CanWrite)
            {
                throw new NotSupportedException("Cannot write to stream");
            }
            using (var bw = new BinaryWriter(file))
            {
                long basePos = file.Position;
                bw.Write(0x01524142u);
                int fileC = fileList.Count;
                bw.Write(fileC);
                bw.Write(0u);
                bw.Write(0u);
                uint offset = 16 * ((uint)fileC + 1);
                for (int i = 0; i < fileC; i++)
                {
                    file.Position = basePos + 16 * (i + 1);
                    BARFile bf = fileList[i];
                    bw.Write(bf.type);
                    bw.Write(bf._id);
                    bw.Write(offset);
                    bw.Write(bf.data.Length);
                    uint szPad = (uint)Math.Ceiling((double)bf.data.Length / 16) * 16;
                    file.Position = basePos + offset;
                    bw.Write(bf.data);
                    offset += szPad;
                }
            }
            //BinaryWriter should close file
        }

        public class BARFile
        {
            public byte[] data = null;
            public uint type = 0;

            public BARFile()
            {
                _id = new byte[4];
            }

            public byte[] _id { get; private set; }

            public string id
            {
                get { return Encoding.ASCII.GetString(_id).TrimEnd(new[] { '\0' }); }
                set
                {
                    Array.Clear(_id, 0, 4);
                    Encoding.ASCII.GetBytes(value, 0, Math.Min(4, value.Length), _id, 0);
                }
            }
        }
    }
}