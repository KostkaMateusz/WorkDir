# WorkDirAPI

## REST API dropbox clone

This project is data storage for users in the cloud.
The core functionality is complete **CRUD** for folders and files. Users can create folders and upload files. Also, they can share multiple files beetwen other users. The frontend project here [FrontEnd React App](https://github.com/TomaszJarkowski/DreamTeam).

---
### The RESTfull API is hosted [here](https://workdir.azurewebsites.net/swagger)
### Front End is hosted [here](https://dreamteam.azurewebsites.net/)
---
### Technologies/Tools:
- .NET8
- ASP.NET
- Entity Framework Core
- FluentValidation
- AspNetCore.Authentication
- Azure.Storage.Blobs
---
### It supports
- User Register
- User Login
- CRUD methods for files and folders
- File Upload Stream
- File download
---
### Objects Relation Model:

![Objects Relation Model](/DBschema.png "Objects Relation Model")