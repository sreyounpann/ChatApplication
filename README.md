# ChatApplication

ChatApplication is a simple networked chat application built using C#, WPF, and networking programming. This project consists of two applications: a server application and a client application. The server manages client connections, and clients can exchange text messages in a chat-like environment.

## Table of Contents

- [Server Application](#server-application)
  - [Features](#features)
  - [Usage](#usage)
  - [Implementation Details](#implementation-details)

- [Client Application](#client-application)
  - [Features](#features-1)
  - [Usage](#usage-1)
  - [Implementation Details](#implementation-details-1)

- [Getting Started](#getting-started)
- [Video Demo](#video-demo)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Server Application

### Features

- Acts as a server to manage client connections.
- Listens for incoming connections from multiple clients.
- Displays a list of connected users.
- Allows clients to define their usernames.
- Notifies clients when a user joins or leaves the chat.
- Broadcasts messages from one user to all other connected users.

### Usage

1. Run the server application.
2. The server will start listening for incoming connections.
3. Clients can connect to the server and provide a username.
4. The server displays the list of connected users.
5. Users can exchange text messages in the chat.

### Implementation Details

The server is implemented using C# and includes features such as handling client connections, managing usernames, broadcasting messages, and updating the list of connected clients.

## Client Application

### Features

- Connects to the server to join the chat.
- Allows users to define their usernames.
- Displays incoming messages from other users.
- Provides a user-friendly interface for chatting.

### Usage

1. Run the client application.
2. Enter a username when prompted.
3. Connect to the server.
4. Chat with other connected users.

### Implementation Details

The client application is implemented using C#, WPF, and networking programming. It connects to the server, sends and receives messages, and updates the user interface accordingly.

## Getting Started

To get started with the ChatApplication, follow these steps:

1. Clone the repository to your local machine.
2. Open the solution in Visual Studio or your preferred C# development environment.
3. Build and run the server application.
4. Build and run the client application on one or more machines.

## Video Demo
[![ChatApplication Demo](https://github.com/sreyounpann/ChatApplication/raw/main/assets/thumbnail.png)](https://github.com/sreyounpann/ChatApplication/raw/main/assets/demo.mp4)

Demo with 4 clients can connected to server and can chat with each other. 

## Contributing

If you'd like to contribute to ChatApplication, please follow these guidelines:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Make your changes and submit a pull request.

## License

This project is licensed under the [MIT License](LICENSE.md).

## Acknowledgments

- [C#](https://docs.microsoft.com/en-us/dotnet/csharp/)
- [WPF](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Json.NET (Newtonsoft.Json)](https://www.newtonsoft.com/json)

Feel free to customize this template based on additional details specific to your project or any additional features you may want to highlight.
