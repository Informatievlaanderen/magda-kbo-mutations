# Certificates

For testing purposes, we use a mock sftp server.

## Create a Self-Signed Certificate

```bash
openssl req -x509 -nodes -days 3650 -newkey rsa:2048 -keyout vsftpd.pem -out vsftpd.pem -subj "/C=RU/O=vsftpd/CN=example.org"
```

## Converting PEM to CRT

```bash
openssl x509 -in vsftpd.pem -out vsftpd.crt
```

## Converting PEM to DER for the Private Key
Since we use crt and der files to communicate with MAGDA, we need to convert the pem file.

```bash
openssl rsa -in vsftpd.pem -outform DER -out vsftpd.der
```

## Copy all certificates to sftp/cert folder
```bash
cp vsftpd.* ./sftp/cert
```

## Use in Curl Command

```bash
curl --ssl-reqd --user files:FSBhuNOR --cert vsftpd.crt --key vsftpd.der --key-type DER ftp://localhost:21000/
```