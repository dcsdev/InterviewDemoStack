# Azure Services Demo Stack

This repository contains a demo stack showcasing a set of Azure services integrated to form an end-to-end solution for monitoring emails, processing attachments, storing data, and sending alerts.

## Overview

The demo stack includes the following key components:

1. **Email Monitor**  
   Monitors an email inbox and extracts attachments from incoming messages.

2. **Attachment Processor**  
   Converts extracted attachments to PDF format if they are text documents.

3. **Cosmos DB Storage**  
   - Stores details of incoming messages.
   - Stores submissions sent to the Event Grid.

4. **Event Grid Integration**  
   Sends events related to processed emails and attachments to Azure Event Grid.

5. **SendGrid Email Alerts**  
   Sends alert emails using SendGrid based on specific conditions or events.

## Architecture Diagram

(*You can add a diagram later to visually represent the flow.*)

## Prerequisites

- Azure account with access to the following services:
  - **Cosmos DB**
  - **Event Grid**
  - **SendGrid**
- Email account credentials for the monitor service.
- Azure Function or other compute resource for running the demo stack.

## Setup Instructions

1. **Clone this repository**  
   ```bash
   git clone <repository-url>
   cd <repository-folder>
