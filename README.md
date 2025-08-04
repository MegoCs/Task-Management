# Task Management System

A comprehensive task management system built with .NET 8, MongoDB, MassTransit + RabbitMQ, and React TypeScript. Features real-time updates, drag-and-drop Kanban board, reminder notifications, and JWT authentication.

## 🚀 Features

- **Kanban Board Interface**: Drag-and-drop task management with visual status columns
- **Real-time Updates**: Live synchronization using SignalR across all connected clients
- **Task Management**: Create, update, delete, and organize tasks with priorities and due dates
- **Reminder System**: Automated email notifications for upcoming and overdue tasks
- **User Authentication**: Secure JWT-based authentication with user registration and login
- **Comments System**: Add and view comments on tasks for better collaboration
- **Responsive Design**: Mobile-friendly interface that works on all devices
- **Containerized**: Full Docker Compose setup for easy deployment

## 🏗️ Architecture

The system follows Clean Architecture principles with these components:

### Backend Services
- **TaskManagement.API**: Main Web API with SignalR hub for real-time updates
- **TaskManagement.ReminderService**: Background service for email notifications
- **TaskManagement.Domain**: Entity models and business logic
- **TaskManagement.Infrastructure**: Data access and external service integrations

### Frontend
- **React TypeScript SPA**: Modern single-page application with real-time capabilities

### Infrastructure
- **MongoDB**: Document database for task and user storage
- **RabbitMQ**: Message broker for reliable reminder notifications
- **Docker**: Containerization for easy deployment

## 🛠️ Technology Stack

### Backend
- **.NET 8.0**: Web API and background services
- **MongoDB**: NoSQL database with strongly-typed models
- **MassTransit + RabbitMQ**: Reliable message queuing and processing
- **SignalR**: Real-time web communication
- **JWT Authentication**: Secure token-based auth
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **BCrypt**: Password hashing

### Frontend
- **React 19**: Modern React with hooks and latest features
- **TypeScript**: Type-safe JavaScript
- **SignalR Client**: Real-time updates
- **Axios**: HTTP client
- **React Beautiful DnD**: Drag-and-drop functionality
- **React Router DOM**: Client-side routing

### DevOps
- **Docker & Docker Compose**: Containerization
- **Node.js serve**: Simple static file server for frontend
- **MongoDB**: Database with initialization scripts
- **RabbitMQ**: Message broker with management interface

## 📋 Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for development)
- [Node.js 18+](https://nodejs.org/) (for development)
- SMTP email server access (Gmail, Outlook, etc.) for notifications

## 🚀 Quick Start with Docker Compose

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd "Task Management"
   ```

2. **Configure environment variables**
   
   Edit the `docker-compose.yml` file and update these environment variables:
   
   ```yaml
   # RabbitMQ Configuration (for message queuing)
   RabbitMq__Host: "rabbitmq"
   RabbitMq__Port: "5672"
   RabbitMq__Username: "taskuser"
   RabbitMq__Password: "taskpass"
   RabbitMq__VirtualHost: "/"
   
   # Email Configuration (for notifications)
   Email__SmtpServer: "smtp.gmail.com"
   Email__SmtpPort: "587"
   Email__Username: "your-email@gmail.com"
   Email__Password: "your-app-password"
   
   # JWT Secret (change this!)
   Jwt__SecretKey: "your-super-secret-jwt-key-that-is-at-least-32-characters-long"
   ```

3. **Run the application**
   ```bash
   docker-compose up --build
   ```

4. **Access the application**
   - Frontend: http://localhost:3000
   - API: http://localhost:5000
   - MongoDB: localhost:27017
   - RabbitMQ Management: http://localhost:15672 (user: taskuser, pass: taskpass)

## 🔧 Development Setup

### Backend Development

1. **Navigate to the src directory**
   ```bash
   cd src
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Set up user secrets** (recommended for development)
   ```bash
   cd TaskManagement.API
   dotnet user-secrets init
   dotnet user-secrets set "Database:ConnectionString" "mongodb://localhost:27017/TaskManagementDB"
   dotnet user-secrets set "Database:DatabaseName" "TaskManagementDB"
   dotnet user-secrets set "RabbitMq:Host" "localhost"
   dotnet user-secrets set "RabbitMq:Username" "taskuser"
   dotnet user-secrets set "RabbitMq:Password" "taskpass"
   dotnet user-secrets set "Email:Username" "your-email@gmail.com"
   dotnet user-secrets set "Email:Password" "your-app-password"
   ```

4. **Run the API**
   ```bash
   dotnet run --project TaskManagement.API
   ```

5. **Run the Reminder Service** (in another terminal)
   ```bash
   dotnet run --project TaskManagement.ReminderService
   ```

### Frontend Development

1. **Navigate to frontend directory**
   ```bash
   cd src/TaskManagement.Frontend
   ```

2. **Install dependencies**
   ```bash
   npm install --legacy-peer-deps
   ```

3. **Start development server**
   ```bash
   npm start
   ```

## 📱 Usage

### Getting Started
1. **Register**: Create a new account on the login page
2. **Login**: Sign in with your credentials
3. **Create Tasks**: Use the "Add Task" button to create new tasks
4. **Organize**: Drag tasks between Todo, In Progress, and Done columns
5. **Collaborate**: Add comments to tasks for team communication

### Task Management
- **Priority Levels**: Low (Green), Medium (Yellow), High (Orange), Critical (Red)
- **Due Dates**: Set deadlines and get visual indicators for overdue tasks
- **Comments**: Click on any task to add comments and track progress
- **Real-time Updates**: See changes instantly across all connected devices

### Reminders
- **Automatic Notifications**: Get email reminders 24 hours before due dates
- **Overdue Alerts**: Receive notifications for overdue tasks
- **Background Processing**: Reminder service runs independently using RabbitMQ

## 🔧 Configuration

### Environment Variables

#### API Service
| Variable | Description | Default |
|----------|-------------|---------|
| `Database__ConnectionString` | MongoDB connection string | `mongodb://localhost:27017/TaskManagementDB` |
| `Database__DatabaseName` | MongoDB database name | `TaskManagementDB` |
| `RabbitMq__Host` | RabbitMQ host address | `localhost` |
| `RabbitMq__Port` | RabbitMQ port | `5672` |
| `RabbitMq__Username` | RabbitMQ username | `taskuser` |
| `RabbitMq__Password` | RabbitMQ password | `taskpass` |
| `RabbitMq__VirtualHost` | RabbitMQ virtual host | `/` |
| `Jwt__SecretKey` | JWT signing key (32+ chars) | Required |
| `Jwt__Issuer` | JWT token issuer | `TaskManagementAPI` |
| `Jwt__Audience` | JWT token audience | `TaskManagementClient` |
| `Jwt__ExpiryHours` | Token expiry in hours | `24` |

#### Email Configuration
| Variable | Description | Example |
|----------|-------------|---------|
| `Email__SmtpServer` | SMTP server address | `smtp.gmail.com` |
| `Email__SmtpPort` | SMTP server port | `587` |
| `Email__EnableSsl` | Enable SSL/TLS | `true` |
| `Email__Username` | Email username | `your-email@gmail.com` |
| `Email__Password` | Email password/app password | `your-app-password` |
| `Email__FromName` | Sender display name | `Task Management System` |

### Setting up Email (Gmail Example)

1. **Enable 2-Factor Authentication** on your Gmail account
2. **Generate App Password**: 
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate password for "Mail"
3. **Use the app password** in the `Email__Password` setting

### RabbitMQ Setup

RabbitMQ is automatically configured via Docker Compose with:
- **Host**: localhost (or `rabbitmq` in Docker network)
- **Port**: 5672 (AMQP), 15672 (Management UI)
- **Username**: taskuser
- **Password**: taskpass
- **Management UI**: http://localhost:15672

## 🧪 Testing

### Running Tests
```bash
# Backend tests
cd src
dotnet test

# Frontend tests
cd src/TaskManagement.Frontend
npm test
```

### Manual Testing
1. **API Endpoints**: Use Swagger UI at http://localhost:5000/swagger
2. **SignalR Connection**: Monitor browser console for connection logs
3. **Email Notifications**: Check email for reminder notifications  
4. **Real-time Updates**: Open multiple browser tabs to test live sync
5. **RabbitMQ Management**: Use http://localhost:15672 to monitor message queues

## 📁 Project Structure

```
src/
├── TaskManagement.API/              # Main Web API
│   ├── Controllers/                 # API controllers
│   ├── Hubs/                       # SignalR hubs
│   ├── Services/                   # Business services
│   └── Program.cs                  # Application entry point
├── TaskManagement.Domain/          # Domain entities and DTOs
│   ├── Entities/                   # Database entities
│   ├── DTOs/                       # Data transfer objects
│   └── Enums/                      # Enumerations
├── TaskManagement.Infrastructure/   # Data access and external services
│   ├── Data/                       # Database context and repositories
│   ├── Messaging/                  # MassTransit implementation
│   └── Services/                   # External service integrations
├── TaskManagement.ReminderService/ # Background notification service
├── TaskManagement.Frontend/        # React TypeScript SPA
│   ├── src/
│   │   ├── components/             # React components
│   │   ├── contexts/               # React contexts
│   │   ├── services/               # API and SignalR services
│   │   └── types/                  # TypeScript type definitions
├── docker-compose.yml              # Docker Compose configuration
├── Dockerfile.api                  # API Docker image
├── Dockerfile.reminder             # Reminder service Docker image
├── Dockerfile.frontend             # Frontend Docker image
└── init-mongo.js                   # MongoDB initialization script
```

## 🔍 API Documentation

### Authentication Endpoints
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration

### Task Endpoints
- `GET /api/tasks` - Get user's tasks
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/comments` - Add comment to task

### SignalR Hub
- **Connection**: `/taskhub`
- **Events**: 
  - `TaskCreated` - New task added
  - `TaskUpdated` - Task modified
  - `TaskDeleted` - Task removed

## 🐛 Troubleshooting

### Common Issues

1. **Docker Build Fails**
   ```bash
   # Clean Docker cache
   docker system prune -f
   docker-compose build --no-cache
   ```

2. **MongoDB Connection Issues**
   ```bash
   # Check MongoDB container logs
   docker logs task-management-mongodb
   
   # Verify MongoDB is running
   docker ps | grep mongo
   ```

3. **MongoDB Connection Issues**
   ```bash
   # Check MongoDB container logs
   docker logs task-management-mongodb
   
   # Verify MongoDB is running
   docker ps | grep mongo
   ```

4. **Email Notifications Not Working**
   - Verify SMTP settings in configuration
   - Check RabbitMQ connection and message processing
   - Ensure RabbitMQ queues exist and have proper permissions
   - Review reminder service logs

5. **RabbitMQ Connection Issues**
   ```bash
   # Check RabbitMQ container logs
   docker logs task-management-rabbitmq
   
   # Access RabbitMQ Management UI
   # http://localhost:15672 (user: taskuser, pass: taskpass)
   ```

4. **SignalR Connection Fails**
   - Check CORS configuration in API
   - Verify JWT token is valid
   - Monitor browser console for connection errors

6. **Frontend Build Issues**
   ```bash
   # Clear npm cache and reinstall
   cd src/TaskManagement.Frontend
   rm -rf node_modules package-lock.json
   npm install --legacy-peer-deps
   ```

### Logs
```bash
# View all service logs
docker-compose logs

# View specific service logs
docker-compose logs task-management-api
docker-compose logs task-management-reminder
docker-compose logs task-management-frontend
docker-compose logs task-management-rabbitmq
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Check the troubleshooting section above
- Review the API documentation at http://localhost:5000/swagger

## 🚀 Future Enhancements

- [ ] File attachments for tasks
- [ ] Team collaboration features
- [ ] Advanced filtering and search
- [ ] Mobile application
- [ ] Integration with external calendars
- [ ] Custom task templates
- [ ] Time tracking
- [ ] Reporting and analytics
- [ ] Webhook integrations
