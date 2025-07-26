# ClearSight - AI-Powered Eye Disease Detection System

ClearSight is a comprehensive healthcare platform that leverages artificial intelligence and machine learning to detect eye diseases through fundus camera analysis. The system provides a complete solution for patients, doctors, and administrators to manage eye health diagnostics efficiently.

## 🎯 Overview

ClearSight revolutionizes eye disease diagnosis by combining modern web technologies with AI-powered image analysis. Patients can upload fundus camera images to receive instant disease predictions, while doctors can verify results and manage patient access to their medical history.

## ✨ Key Features

### 🔐 **Multi-Role Authentication System**
- **Patients**: Register, login, and manage personal eye health data
- **Doctors**: Professional verification system with document upload
- **Administrators**: Complete system oversight and user management
- JWT-based authentication with refresh token support
- Email verification and password reset functionality
- Google OAuth integration support

### 🤖 **AI-Powered Disease Detection**
- Machine learning model integration via Flask API
- Real-time fundus image analysis
- Confidence scoring for predictions
- Support for multiple eye disease classifications
- Arabic and English disease descriptions

### 👥 **Patient Management**
- Complete patient profile management
- Medical history tracking with date stamps
- Doctor access control system
- Scan result storage with cloud integration
- Paginated history viewing

### 🩺 **Doctor Portal**
- Professional verification workflow
- Patient history access (with patient permission)
- Availability scheduling system
- Document upload for credentials
- Patient scan review capabilities

### 👨‍💼 **Administrative Dashboard**
- Doctor verification and approval system
- User management and oversight
- System statistics and monitoring
- Feedback management system

### 🔒 **Security & Compliance**
- Role-based authorization policies
- Rate limiting protection
- Account lockout after failed attempts
- Secure file upload with Cloudinary integration
- HTTPS enforcement and CORS configuration

## 🏗️ Technical Architecture

### **Clean Architecture Implementation**
```
ClearSight.Api/
├── ClearSight.Api/          # Presentation Layer
│   ├── Controllers/         # API Controllers
│   ├── CustomMiddleware/    # Custom middleware
│   └── Program.cs          # Application configuration
├── ClearSight.Core/         # Domain Layer
│   ├── Models/             # Domain entities
│   ├── Dtos/               # Data transfer objects
│   ├── Interfaces/         # Contracts
│   ├── Enums/              # System enumerations
│   └── Helpers/            # Utility classes
└── ClearSight.Infrastructure/ # Infrastructure Layer
    ├── Context/            # Database context
    ├── Implementations/    # Service implementations
    ├── Migrations/         # Database migrations
    └── DbSeeding/          # Data seeding
```

### **Technology Stack**
- **.NET 9.0** - Latest framework for high performance
- **ASP.NET Core Web API** - RESTful API development  
- **Entity Framework Core** - Database ORM
- **SQL Server** - Primary database
- **JWT Authentication** - Secure token-based auth
- **Serilog** - Structured logging
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation
- **Cloudinary** - Cloud image storage

## 📋 API Endpoints

### Authentication (`/api/Auth`)
```http
POST /api/Auth/register          # User registration
POST /api/Auth/login             # User login  
POST /api/Auth/ChangePassword    # Change password
GET  /api/Auth/ConfirmEmail      # Email confirmation
GET  /api/Auth/GetCode           # Password reset code
POST /api/Auth/reset-password    # Reset password
GET  /api/Auth/refreshToken      # Refresh JWT token
DELETE /api/Auth/revokeToken     # Revoke refresh token
POST /api/Auth/generateUserName  # Generate unique username
```

### Patients (`/api/Patients`)
```http
GET  /api/Patients/Profile              # Get patient profile
POST /api/Patients/EditProfile          # Update patient profile
GET  /api/Patients/DoctorsList          # List available doctors
GET  /api/Patients/SearchUsingDoctorName # Search doctors by name
GET  /api/Patients/access-list          # Doctors with access
POST /api/Patients/grant-access         # Grant doctor access
POST /api/Patients/revoke-access        # Revoke doctor access
POST /api/Patients/Scan                 # Upload fundus image for analysis
GET  /api/Patients/GetPatientHistory    # Get scan history
```

### Doctors (`/api/Doctors`)
```http
GET  /api/Doctors/Profile                    # Get doctor profile
POST /api/Doctors/EditProfile               # Update doctor profile
POST /api/Doctors/UploadCredentialsDocument # Upload verification documents
GET  /api/Doctors/GetPatientsWithAccess     # List accessible patients
GET  /api/Doctors/GetPatientHistory         # View patient scan history
```

### Admins (`/api/Admins`)
```http
GET  /api/Admins/GetAllDoctors        # List all doctors
POST /api/Admins/VerifyDoctor         # Approve/reject doctor
GET  /api/Admins/GetAllPatients       # List all patients
GET  /api/Admins/GetAllUsers          # List all users
```

### Feedback (`/api/Feedback`)
```http
POST /api/Feedback/CreateFeedback     # Submit system feedback
GET  /api/Feedback/GetAllFeedback     # View all feedback (Admin)
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code
- Cloudinary account (for image storage)
- Flask ML API server (for predictions)

### Installation Steps

1. **Clone the repository**
```bash
git clone https://github.com/MinaFarag-0/ClearSight.git
cd ClearSight
```

2. **Configure database connection**
```json
{
  "ConnectionStrings": {
    "defaultconnection": "Server=(localdb)\\mssqllocaldb;Database=ClearSightDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

3. **Set up configuration secrets**
```json
{
  "JWT": {
    "Key": "your-super-secret-jwt-key-here",
    "Issuer": "ClearSight",
    "Audience": "ClearSight-Users",
    "DurationInMinutes": 60
  },
  "MailSettings": {
    "Mail": "your-email@gmail.com",
    "DisplayName": "ClearSight",
    "Password": "your-app-password",
    "Host": "smtp.gmail.com",
    "Port": 587
  },
  "Cloudinary": {
    "CloudName": "your-cloudinary-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "ModelURL": "http://localhost:5000/predict"
}
```

4. **Apply database migrations**
```bash
cd ClearSight.Api/ClearSight.Api
dotnet ef database update
```

5. **Run the application**
```bash
dotnet run
```

6. **Access Swagger documentation**
```
https://localhost:7001/swagger
```

## 🔧 Configuration Details

### Email Templates
The system includes pre-built HTML email templates for:
- Account verification
- Password reset codes  
- Welcome messages

### Rate Limiting
- Global limit: 20 requests per minute per host
- Fixed window: 10 requests per minute for specific endpoints
- Automatic 429 responses for exceeded limits

### Security Features
- Account lockout: 3 failed attempts = 10-minute lockout
- JWT token expiration with automatic refresh
- Security stamp validation for session management
- Role-based authorization policies

## 🤖 Machine Learning Integration

The system integrates with a Flask-based ML API for fundus image analysis:

```python
# Expected Flask API endpoint
POST /predict
Content-Type: multipart/form-data

Response:
{
  "prediction": "Normal/Diabetic Retinopathy/Glaucoma/etc",
  "confidence": 0.95
}
```

## 📊 Database Schema

### Core Entities
- **User**: Base user information with ASP.NET Identity
- **Patient**: Patient-specific data and relationships
- **Doctor**: Medical professional profiles and verification
- **Admin**: Administrative user accounts
- **PatientHistory**: Scan results and medical history
- **PatientDoctorAccess**: Permission management
- **Feedback**: User feedback and suggestions

## 🔐 Authorization Policies

### Custom Policies
- **DoctorApproved**: Requires verified doctor status
- **WriteFeedback**: Allows patients and approved doctors

### Role Hierarchy
- **Admin**: Full system access
- **Doctor**: Patient data access (with permission)
- **Patient**: Personal data management

## 📝 Logging & Monitoring

Comprehensive logging with Serilog:
- Request/response logging
- Error tracking and exception handling
- Performance monitoring
- Structured logging to console, file, and Seq

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue in this repository
- Contact the development team
- Check the API documentation at `/swagger`

## 🏆 Acknowledgments

- Machine Learning model integration
- ASP.NET Core community
- Clean Architecture principles
- Medical professionals who provided domain expertise

---

**ClearSight** - Making eye health accessible through technology 👁️✨