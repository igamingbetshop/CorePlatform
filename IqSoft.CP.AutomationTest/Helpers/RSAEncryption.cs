using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IqSoft.CP.AutomationTest.Helpers
{
    public static class RSAEncryption
    {
        private static string XMLPrivateKey =
          "<RSAKeyValue><Modulus>xDV6Tkh5noY4NWUd5G3qmdLkhBCRwwG06buiCc34Xyshtc8eRMaLZarHeQpEkPk43f7Xo7UhOekqQ1+/JlZOrCKRUb8VPdLv/gA9lxzDAJDuSTN8RY0BBJlwNjrP3nrDLIfxAe5OZXxgdwvSi5RxnghWaiM7qtrcaAKcnZnoNrHXvouw9CbLEJa92RJjKIUE94GbFozDSM62CokXrKC9RgcwZfLQijzlgKM90pjLjAlw1Iz/IN3g5k+TqrbHX7zo8I85MIn/1yRyUIGMT3BUMiPKRkCqVZ1NGPLLsuz4vCe/wA9dmhZxrEAECLEFp6/9PoT9abcIxIYJpV1PhxeG1w==</Modulus><Exponent>AQAB</Exponent><P>5LLJ45YKO6chOa/GPvmnGy5oIzY3al3StqK4b4O+Iw37ERnCzuksp6t0P/v21Rxce9Dn0wd5iiR6H+ZLbo7nvdQTV7ryVDWDO0SkNvH1XD6J4gptDFWz7M0i3vG10KDwhzaC7N9j5NPy4B+PjOsDoB8iFgFeTVO4PvfFSe9kzds=</P><Q>26HIir0MLHdeX2WusUTHSe2ukowl8UnFFaQoxD/ttXTsdBQMZC6xgWgxb9KtU/26v3oQv8uFqtLldP5eRefoV7Mivq6CQ0TQVWd3xMkd8R07ybXt9lME5/DgWPz6SbEdXFlHjum2y4usJ6kYUgXCOwUctpgz4rqbmimNScYWYbU=</Q><DP>WJj+54Ef075Ke9uhtJHo7/nJdCKz0ywnzoM5alIiXdgztItDUf85QneEoKkPFb5YAcuLk9BogGDjQupnvJv2IS9AkxMkgAT/Iv3TlEmmISdFKWGan1WwT4OlB7OiGQHQTMGMdRGR1HtbswHnDdOZ4vVMsjOzgcd2MEaykpMAfVM=</DP><DQ>q/r1Q95gx/j4xw6iSmEnBHa/ejWQCG7Riu6ulW3Rv4M9HHAOe+wsRr7F52A7JUfLkeANeYHuuyLFVmVQgMDlqLa3AEU5717VG+sXV9p8Pa+8f2icW4QKlWyC4GvHuSidaxDl/bx4zM4kEjJQvvmPbBPGthxclK+25HKhFiGsqPk=</DQ><InverseQ>iePVWEunHFD6l6A6Y74ppPtNP0zjetRNSjdppS+eU4dQsYU1K56pGdvg+RqQlT4hXLpdnjHUlyWBrOSm7MuicSVLasCJ3285bRpCvWtJcLlglLCE9fkTOPNLjnQ1foQk1sWB7NLUclaG9xppnvmgLZvx0YQoVhP2Vr1wDm1bpWA=</InverseQ><D>VQLCoiZeo2ON+Px9rho9mjY4kkvHi9EyfE6yj0LxiPJcIbTCbZQEk6Eh2fyr5pBEplKjRafV5Ix0pkpWvJqKbaRwiBWdc3LwToH2LYHlr1ocFBU9k7jbJw4AA08J/1/7LlEcB/UjfG8eMJYrvBQuAgWkw0nOsWEwO9Rd3R7w8Ljs/BpmVviNlkyWvjZ6spfo3R18jTQY102+jOoyIh3w++W9vo4XypTnWOWm4Wnd9Da4IYrwPfDrV5QeM9s4RTlPBAb9vXo2BHjwOtVShiWkL78JxIPTWWKIDwZcZJrm1turfjJqEKzzfGsHwAr2/iuFaRLeVWCRkKdARAWOiOwPGQ==</D></RSAKeyValue>";
        private static string PEMPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxDV6Tkh5noY4NWUd5G3qmdLkhBCRwwG06buiCc34Xyshtc8eRMaLZarHeQpEkPk43f7Xo7UhOekqQ1+/JlZOrCKRUb8VPdLv/gA9lxzDAJDuSTN8RY0BBJlwNjrP3nrDLIfxAe5OZXxgdwvSi5RxnghWaiM7qtrcaAKcnZnoNrHXvouw9CbLEJa92RJjKIUE94GbFozDSM62CokXrKC9RgcwZfLQijzlgKM90pjLjAlw1Iz/IN3g5k+TqrbHX7zo8I85MIn/1yRyUIGMT3BUMiPKRkCqVZ1NGPLLsuz4vCe/wA9dmhZxrEAECLEFp6/9PoT9abcIxIYJpV1PhxeG1wIDAQAB";

        private static RSACryptoServiceProvider CreateRsaProviderFromPublicKey(string publicKeyString)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] x509key = Convert.FromBase64String(publicKeyString); 

            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (var mem = new MemoryStream(x509key))
            {
                using (var binr = new BinaryReader(mem)) //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    {
                        binr.ReadByte(); //advance 1 byte
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16(); //advance 2 bytes
                    }
                    else
                    {
                        return null;
                    }

                    var seq = binr.ReadBytes(15); //read the Sequence OID
                    if (!CompareBytearrays(seq, SeqOID)) //make sure Sequence for OID is correct
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                    {
                        binr.ReadByte(); //advance 1 byte
                    }
                    else if (twobytes == 0x8203)
                    {
                        binr.ReadInt16(); //advance 2 bytes
                    }
                    else
                    {
                        return null;
                    }

                    bt = binr.ReadByte();
                    if (bt != 0x00) //expect null byte next
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                    {
                        binr.ReadByte(); //advance 1 byte
                    }
                    else if (twobytes == 0x8230)
                    {
                        binr.ReadInt16(); //advance 2 bytes
                    }
                    else
                    {
                        return null;
                    }

                    twobytes = binr.ReadUInt16();
                    byte lowbyte = 0x00;
                    byte highbyte = 0x00;

                    if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                    {
                        lowbyte = binr.ReadByte(); // read next bytes which is bytes in modulus
                    }
                    else if (twobytes == 0x8202)
                    {
                        highbyte = binr.ReadByte(); //advance 2 bytes
                        lowbyte = binr.ReadByte();
                    }
                    else
                    {
                        return null;
                    }

                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 }; //reverse byte order since asn.1 key uses big endian order
                    int modsize = BitConverter.ToInt32(modint, 0);

                    int firstbyte = binr.PeekChar();
                    if (firstbyte == 0x00)
                    {
                        //if first byte (highest order) of modulus is zero, don't include it
                        binr.ReadByte(); //skip this null byte
                        modsize -= 1; //reduce modulus buffer size by 1
                    }

                    byte[] modulus = binr.ReadBytes(modsize); //read the modulus bytes

                    if (binr.ReadByte() != 0x02) //expect an Integer for the exponent data
                    {
                        return null;
                    }

                    int expbytes = binr.ReadByte(); // should only need one byte for actual exponent data (for all useful values)
                    byte[] exponent = binr.ReadBytes(expbytes);

                    // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                    RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                    RSAParameters RSAKeyInfo = new RSAParameters();
                    RSAKeyInfo.Modulus = modulus;
                    RSAKeyInfo.Exponent = exponent;
                    RSA.ImportParameters(RSAKeyInfo);

                    return RSA;
                }
            }
        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                {
                    return false;
                }

                i++;
            }

            return true;
        }

        public static string RSAEncryptByPEM(string data)
        {
            using (var provider = CreateRsaProviderFromPublicKey(PEMPublicKey))
            {
                return Convert.ToBase64String(provider.Encrypt(Encoding.UTF8.GetBytes(data), false));
            }
        }

        public static string RSADecryptByXMLKey(string data)
        {
            using (var provider = new RSACryptoServiceProvider(2048))
            {
                provider.FromXmlString(XMLPrivateKey);
                return Encoding.UTF8.GetString(provider.Decrypt(Convert.FromBase64String(data), false));
            }
        }
    }
}
