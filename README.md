# Avengers Chat - Dynamic Demo

This is a demonstration project for Stacy Cashmore's talk **"Making the Static Dynamic - Adding Web PubSub to Azure Static Web Apps"**.

🔗 [Talk Details on Sessionize](https://sessionize.com/s/stacy-cashmore/making-static-dynamic-adding-signar-to-azure-stati/132417)

## Overview

This project demonstrates how to build a real-time chat application using Azure Static Web Apps and Azure Web PubSub. The application features an Avengers-themed chat where different superheroes can send messages to all heroes, to their group, or to individual heroes.

## Project Structure

The solution consists of three main projects:

### Client
A **Blazor WebAssembly** application that provides the user interface for the chat application. It runs on port 7001 (HTTP) or 7000 (HTTPS) when running locally.

- Built with .NET 9.0
- Uses WebSocket client for real-time communication with Azure Web PubSub
- Provides UI for selecting superheroes and sending messages

### Api
An **Azure Functions** project (.NET 9.0) that serves as the backend API. It runs on port 7071 when running locally.

- Provides endpoints for sending messages (to all, to groups, or to specific users)
- Manages Web PubSub connections
- Returns the list of available superheroes
- Uses Azure Functions isolated worker model

### Shared
A shared library containing models and data used by both the Client and Api projects.

- Contains superhero definitions
- Contains message models
- Shared between Client and Api

## Azure Resources Required

To run this application, you'll need the following Azure resources:

1. **Azure Static Web App**
   - Hosts the Blazor WebAssembly client application
   - Integrates with the Azure Functions API
   - [Create a Static Web App](https://learn.microsoft.com/en-us/azure/static-web-apps/get-started-portal)

2. **Azure Web PubSub Service**
   - Provides real-time messaging capabilities via WebSocket
   - Enables broadcasting messages to all users, groups, or specific users
   - [Create a Web PubSub resource](https://learn.microsoft.com/en-us/azure/azure-web-pubsub/howto-develop-create-instance)
   - You'll need the connection string from this service

3. **Cosmos DB** (Optional - referenced in Api.csproj)
   - The project has Cosmos DB extensions installed but may not be actively used
   - Can be used for message persistence if needed

4. **Application Insights** (Optional but recommended)
   - Configured in the API for telemetry and monitoring
   - Connection string can be provided via configuration

## User Secrets Configuration

The API project uses .NET User Secrets for local development. You need to configure the following secrets:

### Required Secrets

Navigate to the `Api` directory and run:

```bash
cd Api
dotnet user-secrets set "WebPubSubConnectionString" "<your-web-pubsub-connection-string>"
```

**To get your Web PubSub connection string:**
1. Go to your Azure Web PubSub resource in the Azure Portal
2. Navigate to "Keys" under Settings
3. Copy the "Connection String"

### Optional Secrets

```bash
# Optional: Custom hub name (defaults to "notifications" if not set)
dotnet user-secrets set "WebPubSubHubName" "notifications"

# Optional: Application Insights connection string
dotnet user-secrets set "APPLICATIONINSIGHTS_CONNECTION_STRING" "<your-app-insights-connection-string>"
```

## Running the Application Locally

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools v4](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azure Static Web Apps CLI](https://azure.github.io/static-web-apps-cli/)
- An Azure Web PubSub resource (see Azure Resources section)

### Installing the SWA CLI

The Azure Static Web Apps CLI (SWA CLI) is required to run the application locally as it emulates the Azure Static Web Apps environment.

Install globally using npm:

```bash
npm install -g @azure/static-web-apps-cli
```

Or using yarn:

```bash
yarn global add @azure/static-web-apps-cli
```

Verify installation:

```bash
swa --version
```

**Learn more:** [SWA CLI Documentation](https://azure.github.io/static-web-apps-cli/)

### Running with SWA CLI

The application requires both the API and Client to be running simultaneously. The SWA CLI orchestrates this.

1. **Configure your secrets** (see User Secrets Configuration section above)

2. **Start the API** (in one terminal):
   ```bash
   cd Api
   func start
   ```
   
   The API will start on port 7071.

3. **Start the Client** (in another terminal):
   ```bash
   cd Client
   dotnet run
   ```
   
   The client will start on port 7001 (HTTP) or 7000 (HTTPS).

4. **Start SWA CLI** (in a third terminal):
   ```bash
   swa start http://localhost:7001 --api-location http://localhost:7071
   ```
   
   The SWA CLI will start a local emulator that combines both the client and API, typically on port 4280.

5. **Access the application:**
   Open your browser to `http://localhost:4280`

### Alternative: Manual Configuration

If you prefer not to use the SWA CLI for local development, you can run the API and Client separately:

1. Start the API on port 7071 (see step 2 above)
2. Start the Client on port 7001 (see step 3 above)
3. Access the client directly at `http://localhost:7001`

> **Note:** When running without SWA CLI, ensure your API endpoints are correctly configured in the client application to point to `http://localhost:7071`.

## Build and Test

### Build the solution:
```bash
dotnet build
```

### Run tests (if available):
```bash
dotnet test
```

## Deployment

This project is configured for automatic deployment to Azure Static Web Apps via GitHub Actions. See `.github/workflows/azure-static-web-apps-witty-island-0ab2cf603.yml` for the deployment configuration.

When you push to the `main` branch, the application will automatically build and deploy to your Azure Static Web App.

## Learn More

- [Azure Static Web Apps Documentation](https://learn.microsoft.com/en-us/azure/static-web-apps/)
- [Azure Web PubSub Documentation](https://learn.microsoft.com/en-us/azure/azure-web-pubsub/)
- [Azure Functions Documentation](https://learn.microsoft.com/en-us/azure/azure-functions/)
- [Blazor WebAssembly Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)