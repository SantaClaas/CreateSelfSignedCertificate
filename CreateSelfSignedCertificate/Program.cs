// All you need to create a self signed certificate in .NET to enable HTTPS
// tested with NGINX
// Certificate needs to be installed in the trusted root store for browsers to not show warnings (might require browser restart)
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


X500DistinguishedName subject = new("CN=lcoalhost");
// 2048 is minimal recommended size for a private key
using RSA key = RSA.Create(keySizeInBits: 2048);
CertificateRequest request = new(subject, key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Add Enhanced key usage
// Could not find the Oids for enhanced key usage so I just created them by looking at existing certificates
OidCollection enhancedUsages = new() { X509EnhancedKeyUsages.ClientAuthentication, X509EnhancedKeyUsages.ServerAuthentication };
X509EnhancedKeyUsageExtension enhancedUsagesExtension = new(enhancedUsages, false);
request.CertificateExtensions.Add(enhancedUsagesExtension);

// Subject alternative names
SubjectAlternativeNameBuilder builder = new();
// Add names and addresses under which the address of the machine can be resolved
builder.AddDnsName("localhost");
builder.AddIpAddress(new System.Net.IPAddress(new byte[] { 127, 0, 0, 1 }));
var alternativeNameExtension = builder.Build();
request.CertificateExtensions.Add(alternativeNameExtension);

// Create self signed certificate
var notBefore = DateTimeOffset.Now;
var notAfter = notBefore.AddYears(1);
var certificate = request.CreateSelfSigned(notBefore, notAfter);

// Export certificate and private key PEM encoded to be usable in NGINX

// Convert certificate to PEM
var pemCertificate = PemEncoding.Write("CERTIFICATE", certificate.RawData);
await File.WriteAllTextAsync("cert.pem", new string(pemCertificate));

// Convert key to PEM
var privateKeyData = certificate.PrivateKey.ExportPkcs8PrivateKey();
var pemPrivateKey = PemEncoding.Write("PRIVATE KEY", privateKeyData);
await File.WriteAllTextAsync("cert.key", new string(pemPrivateKey));