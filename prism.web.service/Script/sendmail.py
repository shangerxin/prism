import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart

import sys
import argparse
import doctest
from pathlib import Path
from typing import Optional


def parse_args(test_args: Optional[str] = None) -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description="Compare the csv files and generate the comparison report.")

    p.add_argument("--file",
                   "-f",
                   type=Path,
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The the html files to send by email. Support glob syntax *, ?, etc.")
    p.add_argument("--attach",
                   "-a",
                   type=Path,
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The the files to attach in the email. Support glob syntax *, ?, etc.")
    p.add_argument("--user",
                   "-u",
                   type=str,
                   required=True,
                   help="The email address of the recipient.")
    p.add_argument("--subject",
                   "-s",
                   type=str,
                   required=True,
                   help="The subject of the email.")
    p.add_argument("--password",
                   "-p",
                   type=str,
                   required=True,
                   help="The password for the email account.")
    p.add_argument("--to",
                   "-t",
                   type=str,
                   required=True,
                   help="The email address of the recipient. separated with comma.")
    p.add_argument("--proxy",
                   "-x",
                   type=str,
                   default=None,
                   help="The proxy server to use for sending email.")
    
    if test_args:
        return p.parse_args(test_args.split())
    else:
        return p.parse_args()
    

def send_email(subject, files: list[Path], attach: list[Path], to_email, password, from_email="erxin.shang@intel.com", proxy=None):
    smtp_server = "smtpauth.intel.com"
    smtp_port = 587 

    if proxy:
        import socks
        socks.setdefaultproxy(socks.HTTP, proxy.split(':')[0], int(proxy.split(':')[1]))
        socks.wrapmodule(smtplib)

    msg = MIMEMultipart()
    msg['From'] = from_email
    msg['To'] = to_email
    msg['Subject'] = subject
    
    for file in files:
        with open(file, 'r', encoding='utf-8') as f:
            if file.suffix.lower() == '.txt':
                attachment = MIMEText(f.read(), 'plain', 'utf-8')
            else:
                attachment = MIMEText(f.read(), 'html')
            msg.attach(attachment)

    for file in attach:
        with open(file, 'rb') as f:
            attachment = MIMEText(f.read(), 'base64', 'utf-8')
            attachment.add_header('Content-Disposition', 'attachment', filename=file.name)
            msg.attach(attachment)

    server = smtplib.SMTP(smtp_server, smtp_port)
    server.starttls()
    server.login(from_email, password)
    server.send_message(msg)
    server.quit()
    

def main(args: argparse.Namespace):
    files = []
    for pattern in args.file:
        files.extend(pattern.parent.glob(pattern.name))
    
    attach = []
    for pattern in args.attach:
        attach.extend(pattern.parent.glob(pattern.name))

    send_email(args.subject, files, attach, args.to, args.password, proxy=args.proxy)


if __name__ == "__main__":
    if len(sys.argv) == 1:
        print(f"*** Run doctest for {__file__}! ***")
        doctest.testmod(optionflags=doctest.ELLIPSIS |
                        doctest.IGNORE_EXCEPTION_DETAIL)
    else:
        main(parse_args())
