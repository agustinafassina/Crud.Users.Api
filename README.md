## Users.Api
It is an API designed to manage user accounts, generate tokens, and perform related operations. It is built to connect to multiple databases, including MongoDB and MySQL, to provide flexible storage options and seamless integration across different data sources. The API serves as a core component for user management in your application infrastructure.

<img src="api-diagram.png" alt="Logo del proyecto" width="400" height="450">

#### The repository has some implementations
- [x] Implemented
- [ ] Not implemented

#### Implementations
- [x] MongoDB: Ec2 in AWS
- [x] Sql server
- [x] Migrations
- [x] Encrypted password
- [x] Cors
- [x] Environment variables
- [x] Docker
- [x] Azure pipeline: Elastic Container Registry (AWS)
- [x] Azure pipeline: Azure Container Registries
- [x] Authorization
- [x] Repository version
- [x] Diagram with python
- [ ] Repository (databases)
- [ ] Unit Tests
- [ ] Github action: pipeline with github

## 📄 API Reference
### Databases
<img src="rds-ec2-users.png" alt="Logo del proyecto" width="400" height="450">

### 🔐 Authorization
It implements JWT authentication to secure endpoints, validating issuer, audience, and signature, allowing access only to authorized users.
```
// Controllers:
[Authorize(AuthenticationSchemes = "SecurityAuth")]

// Variables in the appsettings.json
  "SecurityAuth": {
    "Authority": "https://AgusFassina",
    "Audience": "AgusFassina",
    "SecretKey": "das2...............21"
  },
```

### 🚀 Dotnet build and run
```
dotnet build
dotnet run
```

### 🚀 Docker build and run

```
# Docker build
docker build -f Dockerfile -t api .
# Docker run in the port 8787
docker run -d -p 8787:80 -e "ASPNETCORE_ENVIRONMENT=Development" --name api api
# api tests http://localhost:8787/swagger/index.html
```

### Migration in Sql Server
```
dotnet ef migrations add InitialCreate
dotnet ef database update

// delete db
dotnet ef migrations remove
```

### Request with curl
```
// Users
curl --request GET --url http://localhost:5142/api/v1/Users --header 'Authorization: Bearer eyJhbG...........................EA'

// Get token
curl --request POST --url http://localhost:5142/api/v1/Users/generate-token --header 'Content-Type: application/json' --data '{"email": "replaceEmail", "password": replacePass" }'

// Create user
curl --request POST --url http://localhost:5142/api/v1/Users --header 'Content-Type: application/json' --data '{
  "name": "Agus-02",
  "lastName": "",
  "email": "",
  "password": ""
}'
```


