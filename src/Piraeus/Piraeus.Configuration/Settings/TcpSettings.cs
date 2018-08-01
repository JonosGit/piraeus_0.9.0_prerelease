using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;

namespace Piraeus.Configuration.Settings
{
    public class TcpSettings
    {
        
        public TcpSettings(bool useLengthPrefix, int blockSize, int maxBufferSize, bool authenticate = false, 
            X509Certificate2 certificate = null, Dictionary<string, byte[]> presharedKeys = null)
        {
            UseLengthPrefix = useLengthPrefix;
            Authenticate = authenticate;
            Certificate = certificate;
            //PresharedKeys = presharedKeys;
            psks =  presharedKeys;
            BlockSize = blockSize;
            MaxBufferSize = maxBufferSize;
        }

        private Dictionary<string, byte[]> psks;
       
        public bool UseLengthPrefix { get; set; }
        public bool Authenticate { get; set; }
        public X509Certificate2 Certificate { get; set; }

        public Dictionary<string, byte[]> PresharedKeys
        {
            get
            {
                return DeepClone<Dictionary<string, byte[]>>(psks);
            }
        }

        public int BlockSize { get; set; }

        public int MaxBufferSize { get; set; }

        private T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}
