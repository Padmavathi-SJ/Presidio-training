from dotenv import load_dotenv
load_dotenv()

import os
import base64
import email
from email.mime.text import MIMEText
from google.auth.transport.requests import Request
from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from groq import Groq  # Change: Import Groq instead of anthropic

SCOPES = ['https://www.googleapis.com/auth/gmail.modify']

# ============================================================
# ANALYZE WITH GROQ (instead of Claude)
# ============================================================
def analyze_with_groq(requirements_text):
    """Send requirements to Groq for structured analysis."""
    
    api_key = os.environ.get("GROQ_API_KEY")
    if not api_key:
        raise ValueError("GROQ_API_KEY environment variable not set")
    
    # Initialize Groq client
    client = Groq(api_key=api_key)
    
    prompt = f"""Analyze the following client requirements and organize them into EXACTLY these 5 categories:

1. FUNCTIONAL REQUIREMENTS: What the system MUST DO (features, actions, behaviors)
2. NON-FUNCTIONAL REQUIREMENTS: Quality attributes (performance, security, scalability, usability)
3. RISKS: Potential problems, technical challenges, dependencies
4. ASSUMPTIONS: Things you're assuming to be true without explicit confirmation
5. QUESTIONS TO CLIENT: Missing information or clarifications needed

Client Requirements:
{requirements_text}

Format your response EXACTLY as:

FUNCTIONAL REQUIREMENTS:
- [requirement 1]
- [requirement 2]

NON-FUNCTIONAL REQUIREMENTS:
- [requirement 1]

RISKS:
- [risk 1]

ASSUMPTIONS:
- [assumption 1]

QUESTIONS TO CLIENT:
- [question 1]"""

    print("Sending to Groq for analysis...")
    
    # Groq API call (different syntax from Claude)
    completion = client.chat.completions.create(
        model="llama-3.3-70b-versatile",  # Groq's fastest model
        messages=[
            {"role": "system", "content": "You are a senior technical business analyst. Extract structured requirements."},
            {"role": "user", "content": prompt}
        ],
        temperature=0.3,
        max_tokens=2000
    )
    
    analysis_result = completion.choices[0].message.content
    print("Groq analysis complete!")
    return analysis_result

# ============================================================
# GMAIL AUTHENTICATION (Same as before)
# ============================================================
def authenticate_gmail():
    """Authenticate and return Gmail service object."""
    creds = None
    
    if os.path.exists('token.json'):
        creds = Credentials.from_authorized_user_file('token.json', SCOPES)
    
    if not creds or not creds.valid:
        if creds and creds.expired and creds.refresh_token:
            creds.refresh(Request())
        else:
            flow = InstalledAppFlow.from_client_secrets_file(
                'credentials.json', SCOPES)
            creds = flow.run_local_server(port=0)
        
        with open('token.json', 'w') as token:
            token.write(creds.to_json())
    
    return build('gmail', 'v1', credentials=creds)

# ============================================================
# GET LATEST EMAIL WITH ATTACHMENT
# ============================================================
def get_latest_requirement_email(service):
    """Fetch the most recent unread email with a .txt attachment."""
    try:
        results = service.users().messages().list(
            userId='me', 
            labelIds=['INBOX'],
            q='has:attachment filename:txt is:unread',
            maxResults=1
        ).execute()
        
        messages = results.get('messages', [])
        
        if not messages:
            print("No unread emails with .txt attachments found.")
            return None, None, None
        
        msg_id = messages[0]['id']
        message = service.users().messages().get(
            userId='me', 
            id=msg_id,
            format='full'
        ).execute()
        
        headers = message['payload']['headers']
        sender = next((h['value'] for h in headers if h['name'] == 'From'), None)
        
        attachment_content = None
        if 'parts' in message['payload']:
            for part in message['payload']['parts']:
                if part['filename'] and part['filename'].endswith('.txt'):
                    if 'body' in part and 'attachmentId' in part['body']:
                        att_id = part['body']['attachmentId']
                        attachment = service.users().messages().attachments().get(
                            userId='me', 
                            messageId=msg_id, 
                            id=att_id
                        ).execute()
                        file_data = base64.urlsafe_b64decode(attachment['data'].encode('UTF-8'))
                        attachment_content = file_data.decode('utf-8')
                        print(f" Found attachment: {part['filename']}")
                        break
        
        service.users().messages().modify(
            userId='me', 
            id=msg_id,
            body={'removeLabelIds': ['UNREAD']}
        ).execute()
        
        return sender, attachment_content, msg_id
        
    except Exception as error:
        print(f"An error occurred: {error}")
        return None, None, None

# ============================================================
# CREATE AND SEND REPLY
# ============================================================
def create_reply_email(original_sender, analysis_result):
    """Create a reply email with Groq's analysis."""
    
    reply_body = f"""Dear Client,

Thank you for sharing your requirements. I've analyzed them and organized the information into a structured format for better clarity.

{analysis_result}

---
This analysis was generated automatically. Please review and let me know if you need any clarifications on the questions raised above.

Best regards,
AI Requirements Analyzer (Powered by Groq)
"""
    
    message = MIMEText(reply_body)
    message['To'] = original_sender
    message['Subject'] = 'Requirement Analysis Report'
    message['From'] = 'me'
    
    encoded_message = base64.urlsafe_b64encode(message.as_bytes()).decode()
    return {'raw': encoded_message}

def send_reply_email(service, reply_message):
    """Send reply using Gmail API."""
    try:
        sent_message = service.users().messages().send(
            userId='me',
            body=reply_message
        ).execute()
        print(f" Reply sent!")
        return True
    except Exception as error:
        print(f" Failed to send reply: {error}")
        return False

# ============================================================
# MAIN
# ============================================================
def main():
    print("=" * 60)
    print("AI REQUIREMENT ANALYZER (Powered by Groq)")
    print("=" * 60)
    
    print("\n Authenticating with Gmail...")
    service = authenticate_gmail()
    print(" Authenticated!")
    
    print("\n Checking for new requirement emails...")
    sender, requirements_text, email_id = get_latest_requirement_email(service)
    
    if not sender:
        print("No new requirement emails found. Exiting.")
        return
    
    print(f"\n Found requirement email from: {sender}")
    print(f" Requirements length: {len(requirements_text)} characters")
    
    print("\nAnalyzing with Groq...")
    analysis = analyze_with_groq(requirements_text)
    
    print("\n" + "=" * 60)
    print("GROQ'S ANALYSIS:")
    print("=" * 60)
    print(analysis)
    
    print("\nCreating reply email...")
    reply_message = create_reply_email(sender, analysis)
    
    print(f"\n Sending analysis back to {sender}...")
    send_reply_email(service, reply_message)
    

if __name__ == "__main__":
    main()