using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using doob.PgSql.Logging;

namespace doob.PgSql.Helper
{
    public class SecureDataManager
    {

        private static readonly ILog Logger = LogProvider.For<SecureDataManager>();


        private readonly X509Certificate2 cert;

        public SecureDataManager(string certificatePath, string password)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(password))
                {
                    cert = new X509Certificate2(certificatePath);
                }
                else
                {
                    cert = new X509Certificate2(certificatePath, password);
                }
                
                Logger.Debug($"Certificate Loaded: {cert.FriendlyName}");
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Faield to load Certififcate from {certificatePath} with ErrorMessage: {e.Message}");
            }
            
        }


        public string Encrypt(string value)
        {
            try
            {
                return EncryptString(cert, value);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to Encrypt Data with ErrorMessage: {e.Message}");
                throw;
            }
            
        }

        public string Decrypt(string value)
        {
            try
            {
                return DecryptString(cert, value);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to Decrypt Data with ErrorMessage: {e.Message}");
                throw;
            }
            
        }


        //private IEnumerable<X509Certificate2> GetCertificatesFromStore(string certificateName, bool validOnly = false)
        //{
        //    var storeName = (StoreName)Enum.Parse(typeof(StoreName), _storeName, true);
        //    var storeLocation = (StoreLocation)Enum.Parse(typeof(StoreLocation), _storeLocation, true);
        //    X509Store store = new X509Store(storeName, storeLocation);
        //    store.Open(OpenFlags.ReadOnly);
        //    X509Certificate2Collection certificatesInStore = store.Certificates;
        //    var findResult = certificatesInStore.Find(X509FindType.FindBySubjectName, certificateName, validOnly);

        //    List<X509Certificate2> certificates = new List<X509Certificate2>();
        //    foreach (X509Certificate2 x509Certificate2 in findResult)
        //    {
        //        certificates.Add(x509Certificate2);
        //    }
        //    store.Dispose();
        //    return certificates;
        //}

        private string EncryptString(X509Certificate2 x509, string stringToEncrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringToEncrypt))
                throw new Exception("A x509 certificate and string for encryption must be provided");

            //var rsa1 = System.Security.Cryptography.X509Certificates.RSACertificateExtensions.GetRSAPublicKey(x509);
            
            var rsa = x509.GetRSAPublicKey();
            byte[] bytestoEncrypt = ASCIIEncoding.ASCII.GetBytes(stringToEncrypt);
            byte[] encryptedBytes = rsa.Encrypt(bytestoEncrypt, RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptString(X509Certificate2 x509, string stringTodecrypt)
        {
            if (x509 == null || string.IsNullOrEmpty(stringTodecrypt))
                throw new Exception("A x509 certificate and string for decryption must be provided");

            if (!x509.HasPrivateKey)
                throw new Exception("x509 certicate does not contain a private key for decryption");

            var rsa = x509.GetRSAPrivateKey();
            byte[] bytestodecrypt = Convert.FromBase64String(stringTodecrypt);
            byte[] plainbytes = rsa.Decrypt(bytestodecrypt, RSAEncryptionPadding.OaepSHA1);
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            return enc.GetString(plainbytes);
        }
    }
}
